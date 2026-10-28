using Albatross.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Albatross.LogAnalysis.Models {
	/// <summary>
	/// Points an event at one log entry it was captured from. An event can reference several entries, such as a
	/// request's start and end lines.
	/// </summary>
	public class Reference {
		public required Guid EventId { get; init; }
		public Event Event { get; init; } = default!;

		public required Guid LogFileId { get; init; }
		public LogFile LogFile { get; init; } = default!;

		/// <summary>The 1-based line number of the first line of the entry</summary>
		public required int LineNumber { get; init; }
		public required int LineCount { get; init; }
	}

	public class ReferenceEntityMap : EntityMap<Reference> {
		public override void Map(EntityTypeBuilder<Reference> builder) {
			builder.HasKey(x => new { x.EventId, x.LogFileId, x.LineNumber });
			builder.HasOne(x => x.Event).WithMany(x => x.References).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
			builder.HasOne(x => x.LogFile).WithMany().HasForeignKey(x => x.LogFileId).OnDelete(DeleteBehavior.Restrict);
			builder.HasIndex(x => new { x.LogFileId, x.LineNumber });
		}
	}
}