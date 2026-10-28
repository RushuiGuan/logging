using Albatross.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Albatross.LogAnalysis.Models {
	/// <summary>
	/// A log file the scanner has found. The record outlives the file itself, because output derived from the file is
	/// kept after the file is purged.
	/// </summary>
	public class LogFile {
		public required Guid Id { get; init; }

		[MaxLength(850)]
		public required string FullName { get; init; }

		/// <summary>The file system creation time, which sets the order the files of one app are processed in</summary>
		public required DateTime CreatedUtc { get; init; }

		public DateTime LastWriteUtc { get; set; }

		/// <summary>The size in bytes when the scanner last looked at the file</summary>
		public long Size { get; set; }

		/// <summary>Set when the scanner finds the file gone. The file can no longer be processed, but its output is kept.</summary>
		public DateTime? RemovedUtc { get; set; }

		public List<Scan> Scans { get; init; } = [];
	}

	public class LogFileEntityMap : EntityMap<LogFile> {
		public override void Map(EntityTypeBuilder<LogFile> builder) {
			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id).ValueGeneratedNever();
			builder.Property(x => x.FullName).IsUnicode(true);
			builder.HasIndex(x => x.FullName).IsUnique();
		}
	}
}