using Albatross.EFCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Albatross.LogAnalysis.Models {
	/// <summary>
	/// The definition of a kind of analysis, such as errors or usage. Its logic is a C# class; this record registers it
	/// so that events, summaries, and scans can refer to it. Events are the instances a topic captures, and scans record
	/// which files a topic has been run against.
	/// </summary>
	public class Topic {
		public required Guid Id { get; init; }

		[MaxLength(128)]
		public required string Name { get; init; }

		/// <summary>
		/// The current version of the topic's logic. Raised whenever the logic changes, so scans and events made with an
		/// older version can be found and rerun.
		/// </summary>
		public required int Version { get; set; }

		public string? Description { get; set; }

		public bool Enabled { get; set; }
		public required DateTime CreatedUtc { get; init; }

		public List<Scan> Scans { get; init; } = [];
	}

	public class TopicEntityMap : EntityMap<Topic> {
		public override void Map(EntityTypeBuilder<Topic> builder) {
			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id).ValueGeneratedNever();
			builder.Property(x => x.Description).IsUnicode(true);
			builder.HasIndex(x => x.Name).IsUnique();
		}
	}
}