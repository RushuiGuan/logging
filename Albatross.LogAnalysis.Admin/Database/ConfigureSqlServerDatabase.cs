using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Outputs;
using Albatross.LogAnalysis.Admin.Dtos;
using Albatross.LogAnalysis.Admin.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.LogAnalysis.Admin.Database {
	[Verb<ConfigureSqlServerDatabase>("database sqlserver", Description = "Configure the sql server connection")]
	public record class ConfigureSqlServerDatabaseParams : ConfigDatabaseParams {
		[Option(Description = "Database name", DefaultToInitializer = true)]
		public string Database { get; init; } = "LogAnalysis";

		// no backslash in this description: the generator emits it into a regular string literal
		[Option(["server"], Description = "Sql server name. Append the instance name for a named instance, or ,port for one moved off 1433")]
		public string? Host { get; init; }

		[Option(Description = "Login used by the admin program")]
		public string? User { get; init; }

		[Option(Description = "Password for the login")]
		public string? Password { get; init; }

		[Option(["trusted-conn"], Description = "Authenticate using the current user's Windows credential")]
		public bool IntegratedSecurity { get; init; }

		[Option(Description = "Require tls encryption on the connection", DefaultToInitializer = true)]
		public bool Encrypt { get; init; } = true;

		[Option(["trust-cert"], Description = "Accept the server's tls certificate without validating it", DefaultToInitializer = true)]
		public bool TrustServerCertificate { get; init; } = true;
	}

	public partial class ConfigureSqlServerDatabaseCommand {
		partial void Initialize() {
			this.Validators.Add(result => {
				string? connectionString = result.GetValue(this.Option_ConnectionString);
				if (!string.IsNullOrEmpty(connectionString)) {
					try {
						new SqlConnectionStringBuilder(connectionString);
					} catch (Exception err) {
						result.AddError($"--connection-string is not a valid sql server connection string: {err.Message}");
					}
					return;
				}
				if (string.IsNullOrEmpty(result.GetValue(this.Option_Host))) {
					result.AddError("Supply --server, or --connection-string");
				}
				bool integratedSecurity = result.GetValue(this.Option_IntegratedSecurity);
				bool hasLogin = !string.IsNullOrEmpty(result.GetValue(this.Option_User)) && !string.IsNullOrEmpty(result.GetValue(this.Option_Password));
				if (!integratedSecurity && !hasLogin) {
					result.AddError("Supply both --user and --password, or --trusted-conn to authenticate as a windows account");
				}
			});
		}
	}

	public class ConfigureSqlServerDatabase : BaseHandler<ConfigureSqlServerDatabaseParams> {
		private readonly ILogger<ConfigureSqlServerDatabase> logger;
		private readonly IDatabaseService databaseService;
		private readonly IAdminConfigService adminConfigService;

		public ConfigureSqlServerDatabase(ILogger<ConfigureSqlServerDatabase> logger,
			IDatabaseService databaseService,
			IAdminConfigService adminConfigService,
			ParseResult result, ConfigureSqlServerDatabaseParams parameters) : base(result, parameters) {
			this.logger = logger;
			this.databaseService = databaseService;
			this.adminConfigService = adminConfigService;
		}

		static string BuildConnectionString(ConfigureSqlServerDatabaseParams @params) {
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder {
				DataSource = @params.Host ?? string.Empty,
				InitialCatalog = @params.Database,
				Encrypt = @params.Encrypt ? SqlConnectionEncryptOption.Mandatory : SqlConnectionEncryptOption.Optional,
				TrustServerCertificate = @params.TrustServerCertificate,
				IntegratedSecurity = @params.IntegratedSecurity,
			};
			if (!@params.IntegratedSecurity) {
				builder.UserID = @params.User ?? string.Empty;
				builder.Password = @params.Password ?? string.Empty;
			}
			return builder.ConnectionString;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			string provider = Constants.SqlServerProvider;
			string connectionString = string.IsNullOrEmpty(parameters.ConnectionString) ? BuildConnectionString(parameters) : parameters.ConnectionString;
			bool reachable = await databaseService.TryConnect(provider, connectionString, cancellationToken);
			if (!reachable) {
				logger.LogError("Connection is refused by the sql server");
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