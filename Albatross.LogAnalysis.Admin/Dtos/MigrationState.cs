namespace Albatross.LogAnalysis.Admin.Dtos {
	public record class MigrationState {
		public required string DatabaseProvider { get; init; }
		public bool ServerReachable { get; init; }
		public bool DatabaseExists { get; init; }
		public string? CurrentVersion { get; init; }
		public int? PendingMigrationCount { get; init; }
	}
}