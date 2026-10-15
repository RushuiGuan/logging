using Albatross.Hosting;
using Albatross.Messaging.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Albatross.Logging.Daemon {
	public class MySetup : Setup {
		public MySetup(string[] args) : base(args, AppContext.BaseDirectory) { }

		public override void ConfigureServices(IServiceCollection services, IConfiguration cfg) {
			base.ConfigureServices(services, cfg);
			services.AddCommandBus();
		}
	}
}