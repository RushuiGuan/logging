using Albatross.Config;
using Albatross.LogAnalysis.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json.Serialization;

namespace Albatross.LogAnalysis.Daemon {
	public class DaemonConfig : ConfigBase, IAnalysisDbConfig {
		public const string Key = "logAnalysis";
		public const string ConnectionStringKey = "logAnalysis";

		public DaemonConfig(IConfiguration configuration) : base(configuration, Key) {
			ConnectionString = configuration.GetConnectionString(ConnectionStringKey);
		}

		public string? DatabaseProvider { get; init; }
		public string? ConnectionString { get; }

		[JsonIgnore]
		public string RequiredConnectionString => ConnectionString ?? throw new ConfigurationException($"ConnectionStrings:{ConnectionStringKey}");

		public string GetDatabaseProvider() {
			var text = DatabaseProvider?.ToLowerInvariant();
			if (text == Constants.PostgresProvider || text == Constants.SqlServerProvider) {
				return text;
			} else if (string.IsNullOrEmpty(text)) {
				throw new ConfigurationException($"{Key}:databaseProvider");
			} else {
				throw new NotSupportedException($"Database provider {DatabaseProvider} is not supported");
			}
		}
	}
}