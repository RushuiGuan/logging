using Albatross.EFCore.SqlServer;
using Albatross.LogAnalysis.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Albatross.LogAnalysis.Database.SqlServer {
	public static class Extensions {
		public static IServiceCollection AddAnalysisSqlServerDbSession(this IServiceCollection services) {
			services.AddScoped<IAnalysisDbSession>(provider => provider.GetRequiredService<AnalysisSqlServerDbSession>());
			services.AddSqlServerWithContextPool<AnalysisSqlServerDbSession>(
				provider => provider.GetRequiredService<IAnalysisDbConfig>().RequiredConnectionString,
				false);
			return services;
		}
	}
}