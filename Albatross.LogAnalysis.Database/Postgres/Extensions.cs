using Albatross.EFCore.PostgreSQL;
using Albatross.LogAnalysis.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Albatross.LogAnalysis.Database.Postgres {
	public static class Extensions {
		public static IServiceCollection AddAnalysisPostgresDbSession(this IServiceCollection services) {
			services.AddScoped<IAnalysisDbSession>(provider => provider.GetRequiredService<AnalysisPostgresDbSession>());
			services.AddPostgresWithContextPool<AnalysisPostgresDbSession>(
				provider => provider.GetRequiredService<IAnalysisDbConfig>().RequiredConnectionString,
				false);
			return services;
		}
	}
}