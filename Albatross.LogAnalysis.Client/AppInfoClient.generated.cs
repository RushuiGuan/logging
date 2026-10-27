using Albatross.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#nullable enable
namespace Albatross.LogAnalysis.Client {
	public partial interface IAppInfoClient {
		Task<string> Version(CancellationToken cancellationToken);
	}
	internal partial class AppInfoClient : IAppInfoClient {
		public AppInfoClient(HttpClient client) {
			this.client = client;
			this.jsonSerializerOptions = DefaultJsonSerializerOptions.Value;
		}
		public const string ControllerPath = "api/v1/app-info";
		private HttpClient client;
		private JsonSerializerOptions jsonSerializerOptions;
		public async Task<string> Version(CancellationToken cancellationToken) {
			var builder = new RequestBuilder()
				.WithMethod(HttpMethod.Get)
				.WithRelativeUrl($"{ControllerPath}/version");
			using var requestMsg = builder.Build();
			return await this.client.ExecuteOrThrow<string>(requestMsg, this.jsonSerializerOptions, cancellationToken);
		}
	}
}