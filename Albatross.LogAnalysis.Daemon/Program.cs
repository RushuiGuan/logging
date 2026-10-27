using System;
using System.Threading.Tasks;

namespace Albatross.LogAnalysis.Daemon {
	public class Program {
		public static Task Main(string[] args) {
			Environment.CurrentDirectory = AppContext.BaseDirectory;
			return new MySetup(args)
				.ConfigureServiceHost<MyHostedService>()
				.RunAsService()
				.RunAsync();
		}
	}
}