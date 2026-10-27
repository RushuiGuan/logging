namespace Albatross.LogAnalysis.Admin.Dtos {
	public record class DatabaseStatus {
		public const string Masked = "xxxxxx";

		public string? DatabaseProvider { get; init; }
		public bool ConnectionStringConfigured => !string.IsNullOrEmpty(ConnectionString);
		public string? ConnectionString { get; init; }
		public bool Reachable { get; init; }
		public MigrationState? Migration { get; init; }
	}
}