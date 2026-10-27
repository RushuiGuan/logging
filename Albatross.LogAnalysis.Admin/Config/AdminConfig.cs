using Albatross.Config;
using Albatross.LogAnalysis.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;

namespace Albatross.LogAnalysis.Admin.Config {
	public class AdminConfig : ConfigBase, IAnalysisDbConfig {
		public const string Key = "log-analysis-admin";
		public const string ConnectionStringKey = "log-analysis-admin";

		public AdminConfig(IConfiguration configuration) : base(configuration, Key) {
			ConnectionString = configuration.GetConnectionString(ConnectionStringKey);
		}

		public string? DatabaseProvider { get; init; }
		public string? ConnectionString { get; }

		[JsonIgnore]
		public string RequiredConnectionString => ConnectionString ?? throw new ConfigurationException($"ConnectionStrings:{ConnectionStringKey}");

		public string GetDatabaseProvider() {
			string? text = DatabaseProvider?.ToLowerInvariant();
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