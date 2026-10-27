using Albatross.Messaging.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.LogAnalysis.Daemon {
	public class MyHostedService : IHostedService {
		private readonly RouterServer server;
		private readonly ILogger<MyHostedService> logger;

		public MyHostedService(RouterServer server, ILogger<MyHostedService> logger) {
			this.server = server;
			this.logger = logger;
		}

		public Task StartAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Starting router server");
			server.Start();
			NetMQConfig.Linger = TimeSpan.FromSeconds(60);
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken) {
			logger.LogInformation("Stopping router server");
			server.Dispose();
			NetMQConfig.Cleanup(true);
			return Task.CompletedTask;
		}
	}
}