using Microsoft.Extensions.Configuration;

namespace Albatross.LogAnalysis.Daemon {
	public class Startup : Albatross.Hosting.Startup {
		public Startup(IConfiguration configuration) : base(configuration) {
		}
	}
}