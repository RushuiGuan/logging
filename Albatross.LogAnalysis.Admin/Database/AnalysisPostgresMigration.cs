using Albatross.LogAnalysis.Database.Postgres;
using Extensions = Albatross.LogAnalysis.Models.Extensions;

namespace Albatross.LogAnalysis.Admin.Database {
	public class AnalysisPostgresMigration : AnalysisPostgresDbSession {
		public AnalysisPostgresMigration() : this("any") { }
		public AnalysisPostgresMigration(string connectionString)
			: base(Albatross.EFCore.PostgreSQL.Extensions.BuildPostgresMigrationDbContext<AnalysisPostgresMigration>(Extensions.Schema, connectionString)) {
		}
	}
}