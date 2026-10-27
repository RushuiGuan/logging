using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Outputs;
using Albatross.LogAnalysis.Client;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.LogAnalysis.Utility {
	[Verb<GetVersion>("app-info version", Description = "Return the version of the log analysis daemon")]
	public record class GetVersionParams : BaseParams {
	}

	public class GetVersion : BaseHandler<GetVersionParams> {
		private readonly IAppInfoClient client;

		public GetVersion(IAppInfoClient client, ParseResult result, GetVersionParams parameters) : base(result, parameters) {
			this.client = client;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var version = await client.Version(cancellationToken);
			version.Print(parameters.QueryExpression, parameters.Compact);
			return 0;
		}
	}
}