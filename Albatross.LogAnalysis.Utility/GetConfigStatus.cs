using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Outputs;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.LogAnalysis.Utility {
	[Verb<GetConfigStatus>("config status", Description = "Return the log analysis client configuration status")]
	public record class GetConfigStatusParams : BaseParams {
	}

	public class GetConfigStatus : BaseHandler<GetConfigStatusParams> {
		private readonly UtilityConfig config;

		public GetConfigStatus(UtilityConfig config, ParseResult result, GetConfigStatusParams parameters) : base(result, parameters) {
			this.config = config;
		}

		public override Task<int> InvokeAsync(CancellationToken cancellationToken) {
			config.Print(parameters.QueryExpression, parameters.Compact);
			return Task.FromResult(0);
		}
	}
}