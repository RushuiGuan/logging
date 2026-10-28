using Serilog.Events;
using System;
using System.Collections.Generic;

namespace Albatross.LogAnalysis.Models {
	/// <summary>
	/// One log entry: a line that starts an entry, plus any continuation lines that follow it, such as a stack trace.
	/// Produced by the scanner and handed to topics; it is never stored.
	/// </summary>
	public record class Entry {
		/// <summary>The 1-based line number of the first line of the entry within its file</summary>
		public required int LineNumber { get; init; }
		public required int LineCount { get; init; }
		public required DateTimeOffset Timestamp { get; init; }
		public required LogEventLevel Level { get; init; }

		/// <summary>The raw text of every line in the entry, joined with new lines</summary>
		public required string Text { get; init; }

		public EntryParseStatus ParseStatus { get; init; } = EntryParseStatus.Opening;

		/// <summary>
		/// The named fields read from the entry other than <see cref="Timestamp"/> and <see cref="Level"/>, keyed by
		/// the property name in the format definition. Holds only the opening fields until the entry is fully parsed.
		/// </summary>
		public IReadOnlyDictionary<string, string> Properties { get; init; } = new Dictionary<string, string>();
	}
}