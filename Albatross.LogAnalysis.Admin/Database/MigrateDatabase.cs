using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Outputs;
using Albatross.LogAnalysis.Admin.Dtos;
using Albatross.LogAnalysis.Admin.Services;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.LogAnalysis.Admin.Database {
	[Verb<MigrateDatabase>("database migrate", Description = "Apply pending EF Core migrations to the database")]
	public record class DatabaseMigrateParams : BaseParams { }

	public class MigrateDatabase : BaseHandler<DatabaseMigrateParams> {
		private readonly IDatabaseService databaseService;

		public MigrateDatabase(IDatabaseService databaseService, ParseResult result, DatabaseMigrateParams parameters) : base(result, parameters) {
			this.databaseService = databaseService;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			await this.databaseService.Migrate(cancellationToken);
			MigrationState data = await this.databaseService.GetMigrationState(cancellationToken);
			result.PrintSuccessWithData("Database migrated successfully", data, parameters.QueryExpression);
			return 0;
		}
	}
}