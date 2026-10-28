using Albatross.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Albatross.LogAnalysis.Models {
	/// <summary>
	/// A summary a topic built from multiple events, such as the distinct errors in one batching window or the
	/// requests to one endpoint in one hour.
	/// </summary>
	public class Summary {
		public required Guid Id { get; init; }

		/// <summary>The kind of summary within the topic, such as error-batch or hourly-usage</summary>
		[MaxLength(128)]
		public required string Name { get; init; }

		/// <summary>What the summarized events have in common, such as an error group or an endpoint</summary>
		[MaxLength(512)]
		public string? Key { get; init; }

		public required DateTime StartUtc { get; init; }
		public required DateTime EndUtc { get; init; }

		/// <summary>Topic-specific values as JSON</summary>
		public string? Data { get; init; }

		public required DateTime CreatedUtc { get; init; }

		public List<SummaryEvent> Events { get; init; } = [];
	}

	public class SummaryEvent {
		public required Guid SummaryId { get; init; }
		public Summary Summary { get; init; } = default!;
		public required Guid EventId { get; init; }
		public Event Event { get; init; } = default!;
	}

	public class SummaryEntityMap : EntityMap<Summary> {
		public override void Map(EntityTypeBuilder<Summary> builder) {
			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id).ValueGeneratedNever();
			builder.Property(x => x.Key).IsUnicode(true);
			builder.Property(x => x.Data).IsUnicode(true);
			builder.HasMany(x => x.Events).WithOne(x => x.Summary).HasForeignKey(x => x.SummaryId).OnDelete(DeleteBehavior.Cascade);
		}
	}

	public class SummaryEventEntityMap : EntityMap<SummaryEvent> {
		public override void Map(EntityTypeBuilder<SummaryEvent> builder) {
			builder.HasKey(x => new { x.SummaryId, x.EventId });
			builder.HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
			builder.HasIndex(x => x.EventId);
		}
	}
}