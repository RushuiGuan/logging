using Microsoft.Extensions.DependencyInjection;
#nullable enable
namespace Albatross.LogAnalysis.Client {
	public static partial class Extensions {
		public static IHttpClientBuilder AddClients(this IHttpClientBuilder builder) {
			return builder
				.AddTypedClient<IAppInfoClient, AppInfoClient>();
		}
	}
}