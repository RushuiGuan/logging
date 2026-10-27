using Albatross.LogAnalysis.Admin.Config;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.LogAnalysis.Admin.Services {
	public interface IAdminConfigService {
		Task SetDatabase(string provider, string connectionString, CancellationToken cancellationToken);
	}

	public class AdminConfigService : IAdminConfigService {
		public const string Filename = "appsettings.json";

		private readonly ILogger<AdminConfigService> logger;
		private readonly FileInfo file;

		public AdminConfigService(ILogger<AdminConfigService> logger) {
			this.logger = logger;
			this.file = new FileInfo(Path.Join(AppContext.BaseDirectory, Filename));
		}

		public async Task SetDatabase(string provider, string connectionString, CancellationToken cancellationToken) {
			await file.SetConfigValue([AdminConfig.Key, nameof(AdminConfig.DatabaseProvider)], provider, logger, cancellationToken);
			await file.SetConfigValue(["ConnectionStrings", AdminConfig.ConnectionStringKey], connectionString, logger, cancellationToken);
		}
	}
}