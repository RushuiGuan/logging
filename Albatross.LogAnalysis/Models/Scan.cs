using Albatross.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Albatross.LogAnalysis.Models {
	public enum ScanStatus {
		Running,
		Completed,
		Failed,
	}

	/// <summary>
	/// The relationship between a topic and a log file: whether, how far, and with which version of its logic the topic
	/// has scanned the file. Its absence means the file has not been scanned for that topic. A scan with an older
	/// version than <see cref="Topic.Version"/> means the file needs to be scanned again.
	/// </summary>
	public class Scan {
		public required Guid LogFileId { get; init; }
		public LogFile LogFile { get; init; } = default!;

		public required Guid TopicId { get; init; }
		public Topic Topic { get; init; } = default!;

		public required ScanStatus Status { get; set; }

		/// <summary>
		/// The byte offset the scan has read up to. A topic that follows a growing file resumes from here, and a
		/// completed scan of a file that has since grown can tell how much is new.
		/// </summary>
		public long Position { get; set; }

		/// <summary>The 1-based line number at <see cref="Position"/>, so line numbers stay correct when a scan resumes</summary>
		public int LineNumber { get; set; }

		public required DateTime StartedUtc { get; set; }
		public DateTime? CompletedUtc { get; set; }

		public string? Error { get; set; }
	}

	public class ScanEntityMap : EntityMap<Scan> {
		public override void Map(EntityTypeBuilder<Scan> builder) {
			builder.HasKey(x => new { x.LogFileId, x.TopicId });
			builder.Property(x => x.Error).IsUnicode(true);
			builder.HasOne(x => x.LogFile).WithMany(x => x.Scans).HasForeignKey(x => x.LogFileId).OnDelete(DeleteBehavior.Restrict);
			builder.HasOne(x => x.Topic).WithMany(x => x.Scans).HasForeignKey(x => x.TopicId).OnDelete(DeleteBehavior.Restrict);
			builder.HasIndex(x => new { x.TopicId, x.Status });
		}
	}
}