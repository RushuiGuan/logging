using Albatross.LogAnalysis.Database.SqlServer;
using Extensions = Albatross.LogAnalysis.Models.Extensions;

namespace Albatross.LogAnalysis.Admin.Database {
	public class AnalysisSqlServerMigration : AnalysisSqlServerDbSession {
		public AnalysisSqlServerMigration() : this("any") { }
		public AnalysisSqlServerMigration(string connectionString)
			: base(Albatross.EFCore.SqlServer.Extensions.BuildSqlServerMigrationDbContext<AnalysisSqlServerMigration>(Extensions.Schema, connectionString)) {
		}
	}
}