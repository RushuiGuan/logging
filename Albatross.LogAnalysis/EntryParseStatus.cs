namespace Albatross.LogAnalysis {
	public enum EntryParseStatus {
		/// <summary>Only the opening of the entry, the fields before the message, has been read</summary>
		Opening,
		Parsed,
		/// <summary>The full parse was attempted and failed; the opening fields and the raw text are still valid</summary>
		Failed,
	}
}