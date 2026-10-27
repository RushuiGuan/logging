using Albatross.Config;
using Albatross.Hosting;
using Albatross.LogAnalysis.Database.Postgres;
using Albatross.LogAnalysis.Database.SqlServer;
using Albatross.LogAnalysis.Models;
using Albatross.Messaging.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Albatross.LogAnalysis.Daemon {
	public class MySetup : Setup {
		public MySetup(string[] args) : base(args, AppContext.BaseDirectory) { }

		public override void ConfigureServices(IServiceCollection services, IConfiguration cfg) {
			base.ConfigureServices(services, cfg);
			services.AddCommandBus();
			services.AddConfig<DaemonConfig>();
			services.AddConfig<IAnalysisDbConfig, DaemonConfig>();
			var config = new DaemonConfig(cfg);
			if (config.GetDatabaseProvider() == Constants.SqlServerProvider) {
				services.AddAnalysisSqlServerDbSession();
			} else {
				services.AddAnalysisPostgresDbSession();
			}
		}
	}
}