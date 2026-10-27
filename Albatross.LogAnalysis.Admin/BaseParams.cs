using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Outputs;
using DevLab.JmesPath.Expressions;

namespace Albatross.LogAnalysis.Admin {
	public record class BaseParams {
		[UseOption<QueryOption>]
		public JmesPathExpression? QueryExpression { get; init; }

		[UseOption<CompactOption>]
		public bool Compact { get; init; }
	}
}