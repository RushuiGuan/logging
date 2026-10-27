using Albatross.Json;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.LogAnalysis.Admin.Services {
	public static class Extensions {
		static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions {
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = true,
		};

		public static async Task SetConfigValue<T>(this FileInfo file, string[] path, T value, ILogger logger, CancellationToken cancellationToken) {
			using (FileStream stream = File.Open(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
				JsonNode? json = null;
				if (stream.Length > 0) {
					try {
						json = await JsonNode.ParseAsync(stream, cancellationToken: cancellationToken);
					} catch (JsonException err) {
						logger.LogError(err, "Failed to parse config file {name}", file.FullName);
					}
				}
				json = json.SetValue(path, value, serializerOptions);
				if (json != null) {
					stream.SetLength(0);
					await using StreamWriter writer = new StreamWriter(stream);
					await writer.WriteAsync(json.ToJsonString(serializerOptions));
				}
			}
		}
	}
}