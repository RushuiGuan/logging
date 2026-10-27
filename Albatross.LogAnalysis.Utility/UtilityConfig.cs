using Albatross.Config;
using Albatross.LogAnalysis.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Albatross.LogAnalysis.Utility {
	public class UtilityConfig : ConfigBase, ILogAnalysisClientConfig {
		public const string DaemonEndPointKey = "log-analysis";

		public UtilityConfig(IConfiguration configuration) : base(configuration, "logAnalysisCli") {
			DaemonUri = configuration.GetEndPoint(DaemonEndPointKey);
		}

		public string? DaemonUri { get; }

		[JsonIgnore]
		public string RequiredDaemonUri => DaemonUri ?? throw new ConfigurationException($"endpoints:{DaemonEndPointKey}");
	}
}