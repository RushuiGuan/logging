using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Outputs;
using Albatross.LogAnalysis.Admin.Dtos;
using Albatross.LogAnalysis.Admin.Services;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.LogAnalysis.Admin.Database {
	[Verb<ConfigurePostgresDatabase>("database postgres", Description = "Configure the postgres connection")]
	public record class ConfigurePostgresDatabaseParams : ConfigDatabaseParams {
		[Option(Description = "Database name", DefaultToInitializer = true)]
		public string Database { get; init; } = "LogAnalysis";

		[Option(Description = "Postgres server name")]
		public string? Host { get; init; }

		[Option(DefaultToInitializer = true, Description = "Postgres server port")]
		public int Port { get; init; } = 5432;

		[Option(Description = "Role used by the admin program")]
		public string? User { get; init; }

		[Option(Description = "Password for the role")]
		public string? Password { get; init; }
	}

	public partial class ConfigurePostgresDatabaseCommand {
		partial void Initialize() {
			this.Validators.Add(result => {
				string? connectionString = result.GetValue(this.Option_ConnectionString);
				if (!string.IsNullOrEmpty(connectionString)) {
					try {
						new NpgsqlConnectionStringBuilder(connectionString);
					} catch (Exception err) {
						result.AddError($"--connection-string is not a valid postgres connection string: {err.Message}");
					}
				} else if (string.IsNullOrEmpty(result.GetValue(this.Option_Host))
					|| string.IsNullOrEmpty(result.GetValue(this.Option_User))
					|| string.IsNullOrEmpty(result.GetValue(this.Option_Password))) {
					result.AddError("Supply --host, --user and --password, or --connection-string");
				}
			});
		}
	}

	public class ConfigurePostgresDatabase : BaseHandler<ConfigurePostgresDatabaseParams> {
		private readonly ILogger<ConfigurePostgresDatabase> logger;
		private readonly IDatabaseService databaseService;
		private readonly IAdminConfigService adminConfigService;

		public ConfigurePostgresDatabase(ILogger<ConfigurePostgresDatabase> logger,
			IDatabaseService databaseService,
			IAdminConfigService adminConfigService,
			ParseResult result, ConfigurePostgresDatabaseParams parameters) : base(result, parameters) {
			this.logger = logger;
			this.databaseService = databaseService;
			this.adminConfigService = adminConfigService;
		}

		static string BuildConnectionString(ConfigurePostgresDatabaseParams @params) =>
			new NpgsqlConnectionStringBuilder {
				Host = @params.Host ?? string.Empty,
				Port = @params.Port,
				Database = @params.Database,
				Username = @params.User ?? string.Empty,
				Password = @params.Password ?? string.Empty,
				SslMode = SslMode.Prefer,
			}.ConnectionString;

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			string provider = Constants.PostgresProvider;
			string connectionString = string.IsNullOrEmpty(parameters.ConnectionString) ? BuildConnectionString(parameters) : parameters.ConnectionString;
			bool reachable = await databaseService.TryConnect(provider, connectionString, cancellationToken);
			if (!reachable) {
				logger.LogError("Connection is refused by the postgres server");
			}
			if (reachable || parameters.AllowUnreachable) {
				await adminConfigService.SetDatabase(provider, connectionString, cancellationToken);
			}
			DatabaseStatus status = new DatabaseStatus {
				DatabaseProvider = provider,
				ConnectionString = parameters.ShowSecrets ? connectionString : DatabaseStatus.Masked,
				Reachable = reachable,
			};
			bool failed = !reachable && !parameters.AllowUnreachable;
			status.Print(parameters.QueryExpression, parameters.Compact, failed);
			return failed ? 1 : 0;
		}
	}
}