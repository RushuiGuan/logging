using Albatross.EFCore;
using Albatross.LogAnalysis.Admin.Config;
using Albatross.LogAnalysis.Admin.Database;
using Albatross.LogAnalysis.Admin.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.LogAnalysis.Admin.Services {
	public interface IDatabaseService {
		Task<bool> TryConnect(string provider, string connectionString, CancellationToken cancellationToken);
		Task Migrate(CancellationToken cancellationToken);
		Task<MigrationState> GetMigrationState(CancellationToken cancellationToken);
	}

	public class DatabaseService : IDatabaseService {
		const string PostgresAdministrativeDatabase = "postgres";
		const string SqlServerAdministrativeDatabase = "master";

		private readonly ILogger<DatabaseService> logger;
		private readonly AdminConfig config;

		public DatabaseService(ILogger<DatabaseService> logger, AdminConfig config) {
			this.logger = logger;
			this.config = config;
		}

		public async Task<bool> TryConnect(string provider, string connectionString, CancellationToken cancellationToken) {
			// the connection string is not logged: it carries the password
			logger.LogDebug("TryConnect:{Provider}", provider);
			try {
				if (provider == Constants.PostgresProvider) {
					NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(connectionString) { Database = PostgresAdministrativeDatabase };
					await using NpgsqlConnection connection = new NpgsqlConnection(builder.ConnectionString);
					await connection.OpenAsync(cancellationToken);
				} else {
					SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = SqlServerAdministrativeDatabase };
					await using SqlConnection connection = new SqlConnection(builder.ConnectionString);
					await connection.OpenAsync(cancellationToken);
				}
				return true;
			} catch (OperationCanceledException) {
				throw;
			} catch (Exception err) {
				logger.LogDebug(err, "Database connection test failed");
				return false;
			}
		}

		IDbSession GetDbSession() {
			if (config.GetDatabaseProvider() == Constants.SqlServerProvider) {
				return new AnalysisSqlServerMigration(config.RequiredConnectionString);
			} else {
				return new AnalysisPostgresMigration(config.RequiredConnectionString);
			}
		}

		public async Task Migrate(CancellationToken cancellationToken) {
			using IDbSession session = GetDbSession();
			await session.DbContext.Database.MigrateAsync(cancellationToken);
		}

		public async Task<MigrationState> GetMigrationState(CancellationToken cancellationToken) {
			bool databaseExists = false;
			using IDbSession session = GetDbSession();
			try {
				databaseExists = await session.DbContext.Database.CanConnectAsync(cancellationToken);
			} catch (Exception err) {
				logger.LogDebug(err, "Database connection failed");
			}
			if (databaseExists) {
				string[] applied = (await session.DbContext.Database.GetAppliedMigrationsAsync(cancellationToken)).ToArray();
				string[] pending = (await session.DbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
				return new MigrationState {
					DatabaseProvider = config.GetDatabaseProvider(),
					ServerReachable = true,
					DatabaseExists = true,
					CurrentVersion = applied.Length > 0 ? applied[^1] : null,
					PendingMigrationCount = pending.Length,
				};
			}
			bool serverReachable = await this.TryConnect(config.GetDatabaseProvider(), config.RequiredConnectionString, cancellationToken);
			return new MigrationState {
				DatabaseProvider = config.GetDatabaseProvider(),
				ServerReachable = serverReachable,
				DatabaseExists = false,
			};
		}
	}
}