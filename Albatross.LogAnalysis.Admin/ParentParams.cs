using Albatross.CommandLine.Annotations;

namespace Albatross.LogAnalysis.Admin {
	[Verb("database", Alias = ["db"], Description = "Manage the database connection and migrations")]
	[Verb("config", Alias = ["cfg"], Description = "Configure the admin program")]
	public record class ParentParams {
	}
}