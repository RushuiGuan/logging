using Albatross.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sample.WebApi {
	public class Startup : Albatross.Hosting.Startup {
		public Startup(IConfiguration configuration) : base(configuration) { }
		public override void ConfigureServices(IServiceCollection services) {
			base.ConfigureServices(services);
			services.AddShortenLoggerName(true, "Microsoft", "System");
		}
	}
}