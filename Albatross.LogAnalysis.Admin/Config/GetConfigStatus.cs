using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Outputs;
using Albatross.LogAnalysis.Admin.Dtos;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.LogAnalysis.Admin.Config {
	[Verb<GetConfigStatus>("config status", Description = "Display configuration status")]
	public record class GetConfigStatusParams : BaseParams {
		[UseOption<ShowSecretsOption>]
		public bool ShowSecrets { get; init; }
	}

	public class GetConfigStatus : BaseHandler<GetConfigStatusParams> {
		private readonly AdminConfig config;

		public GetConfigStatus(AdminConfig config, ParseResult result, GetConfigStatusParams parameters) : base(result, parameters) {
			this.config = config;
		}

		public override Task<int> InvokeAsync(CancellationToken cancellationToken) {
			var status = new {
				config.DatabaseProvider,
				ConnectionString = parameters.ShowSecrets ? config.ConnectionString : DatabaseStatus.Masked,
			};
			status.Print(parameters.QueryExpression, parameters.Compact);
			return Task.FromResult(0);
		}
	}
}