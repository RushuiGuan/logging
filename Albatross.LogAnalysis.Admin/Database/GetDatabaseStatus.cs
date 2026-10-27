using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Outputs;
using Albatross.LogAnalysis.Admin.Config;
using Albatross.LogAnalysis.Admin.Dtos;
using Albatross.LogAnalysis.Admin.Services;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.LogAnalysis.Admin.Database {
	[Verb<GetDatabaseStatus>("database status", Description = "Get the status of the database connection and migrations")]
	public record class GetDatabaseStatusParams : BaseParams {
		[UseOption<ShowSecretsOption>]
		public bool ShowSecrets { get; init; }
	}

	public class GetDatabaseStatus : BaseHandler<GetDatabaseStatusParams> {
		private readonly IDatabaseService service;
		private readonly AdminConfig config;

		public GetDatabaseStatus(IDatabaseService service, AdminConfig config, ParseResult result, GetDatabaseStatusParams parameters) : base(result, parameters) {
			this.service = service;
			this.config = config;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			bool configured = !string.IsNullOrEmpty(config.DatabaseProvider) && !string.IsNullOrEmpty(config.ConnectionString);
			MigrationState? migration = configured ? await service.GetMigrationState(cancellationToken) : null;
			DatabaseStatus status = new DatabaseStatus {
				DatabaseProvider = config.DatabaseProvider,
				ConnectionString = parameters.ShowSecrets ? config.ConnectionString : DatabaseStatus.Masked,
				Reachable = migration?.ServerReachable == true,
				Migration = migration,
			};
			status.Print(parameters.QueryExpression, parameters.Compact);
			return 0;
		}
	}
}