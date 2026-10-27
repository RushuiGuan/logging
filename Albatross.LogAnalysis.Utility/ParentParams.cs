using Albatross.CommandLine.Annotations;

namespace Albatross.LogAnalysis.Utility {
	[Verb("config", Description = "Log analysis client configuration")]
	[Verb("app-info", Alias = ["info"], Description = "Inspect the log analysis daemon")]
	public record class ParentParams : BaseParams {
	}
}