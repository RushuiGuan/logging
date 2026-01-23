using Albatross.CommandLine;
using Albatross.CommandLine.Defaults;
using Albatross.Config;
using Albatross.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.CommandLine;
using System.Threading.Tasks;

namespace Sample.CommandLine {
	internal class Program {
		static async Task<int> Main(string[] args) {
			await using var host = new CommandHost("Logging Utility")
				.AddCommands()
				.RegisterServices((_, x) => x.RegisterCommands())
				.Parse(args, false)
				.ConfigureHost(Configure)
				.Build();
			return await host.InvokeAsync();
		}

		static void Configure(ParseResult result, IHostBuilder builder) {
			builder.UseSerilog();
			builder.ConfigureLogging((context, logging) => {
				var setupSerilog = new SetupSerilog();
				setupSerilog.UseConfigFile(EnvironmentSetting.DOTNET_ENVIRONMENT.Value, null, null, true);

				var verbosityOption = result.GetVerbosityOption();
				var logLevel = LogLevel.Error;
				if (verbosityOption != null) {
					logLevel = verbosityOption.GetLogLevel(result);
				}
				if (logLevel != LogLevel.None) {
					setupSerilog.UseConsole(logLevel.ToSerilogLevel());
				}
				setupSerilog.Create();
			});
		}
	}
}