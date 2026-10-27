using Albatross.EFCore;
using Albatross.LogAnalysis.Models;
using Microsoft.EntityFrameworkCore;

namespace Albatross.LogAnalysis.Database.SqlServer {
	public class AnalysisSqlServerDbSession : DbSession, IAnalysisDbSession {
		public AnalysisSqlServerDbSession(DbContextOptions<AnalysisSqlServerDbSession> option) : base(option) {
		}

		protected AnalysisSqlServerDbSession(DbContextOptions option) : base(option) {
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.HasDefaultSchema(Albatross.LogAnalysis.Models.Extensions.Schema);
		}
	}
}