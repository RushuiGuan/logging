using Albatross.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Albatross.LogAnalysis.Models {
	/// <summary>
	/// A semantic event a topic captured from log entries, such as an error, a request, or an app starting up.
	/// Events are tied to the file they came from, so reprocessing a file for a topic replaces that topic's events.
	/// </summary>
	public class Event {
		public required Guid Id { get; init; }

		public required Guid LogFileId { get; init; }
		public LogFile LogFile { get; init; } = default!;

		public required Guid TopicId { get; init; }
		public Topic Topic { get; init; } = default!;
		public required int TopicVersion { get; init; }

		/// <summary>The kind of event within the topic, such as error or request</summary>
		[MaxLength(128)]
		public required string Name { get; init; }

		public required DateTime TimestampUtc { get; init; }
		public required LogEventLevel Level { get; init; }

		/// <summary>Topic-specific values as JSON</summary>
		public string? Data { get; init; }

		public List<Reference> References { get; init; } = [];
	}

	public class EventEntityMap : EntityMap<Event> {
		public override void Map(EntityTypeBuilder<Event> builder) {
			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id).ValueGeneratedNever();
			builder.Property(x => x.Data).IsUnicode(true);
			builder.HasOne(x => x.LogFile).WithMany().HasForeignKey(x => x.LogFileId).OnDelete(DeleteBehavior.Restrict);
			builder.HasOne(x => x.Topic).WithMany().HasForeignKey(x => x.TopicId).OnDelete(DeleteBehavior.Restrict);
			builder.HasIndex(x => new { x.LogFileId, x.TopicId });
			builder.HasIndex(x => new { x.TopicId, x.Name, x.TimestampUtc });
		}
	}
}