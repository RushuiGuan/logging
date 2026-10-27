using Microsoft.Extensions.DependencyInjection;
using System;

namespace Albatross.LogAnalysis.Client {
	public static partial class Extensions {
		public static IServiceCollection AddLogAnalysisClient(this IServiceCollection services) {
			services.AddHttpClient("log-analysis", (sp, client) => {
				var config = sp.GetRequiredService<ILogAnalysisClientConfig>();
				client.BaseAddress = new Uri(config.RequiredDaemonUri);
			}).AddClients();
			return services;
		}
	}
}