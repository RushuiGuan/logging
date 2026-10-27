using Albatross.CommandLine.Annotations;

namespace Albatross.LogAnalysis.Admin.Database {
	public record class ConfigDatabaseParams : BaseParams {
		[Option(["connection-string", "conn"], Description = "Connection string used by the admin program; requires schema change privileges and, if the database does not yet exist, database creation privileges")]
		public string? ConnectionString { get; init; }

		[UseOption<ShowSecretsOption>]
		public bool ShowSecrets { get; init; }

		[Option(Description = "Record the connection even when the server is unreachable and return the exit code of 0")]
		public bool AllowUnreachable { get; init; }
	}
}