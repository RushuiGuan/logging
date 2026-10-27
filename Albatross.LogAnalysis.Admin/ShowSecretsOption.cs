using Albatross.CommandLine.Annotations;
using System.CommandLine;

namespace Albatross.LogAnalysis.Admin {
	[DefaultNameAliases("show-secrets")]
	public class ShowSecretsOption : Option<bool> {
		public ShowSecretsOption(string name, params string[] aliases) : base(name, aliases) {
			Description = "Set the flag to true to reveal secrets in output";
		}
	}
}