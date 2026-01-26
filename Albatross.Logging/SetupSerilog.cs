using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;

namespace Albatross.Logging {
	public class SetupSerilog {
		public const string DefaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:sszzz} [{Level:w3}] {SourceContext} {Message:lj}{NewLine}{Exception}";
		Action<LoggerConfiguration>? configActions = null;

		[Obsolete("Use the overload that includes the 'optional' parameter")]
		public SetupSerilog UseConfigFile(string environment, string? basePath, string[]? commandLineArgs) {
			Action<LoggerConfiguration> action = cfg => UseConfigFile(cfg, environment, basePath, commandLineArgs);
			configActions += action;
			return this;
		}
		public SetupSerilog UseConfigFile(string environment, string? basePath, string[]? commandLineArgs, bool optional) {
			Action<LoggerConfiguration> action = cfg => UseConfigFile(cfg, environment, basePath, commandLineArgs, optional);
			configActions += action;
			return this;
		}

		/// <summary>
		/// Adds a console sink to the Serilog configuration pipeline with the specified minimum logging level.
		/// The logging level is controlled by a shared <see cref="LoggingLevelSwitch"/> that can be changed at
		/// runtime via <see cref="SwitchConsoleLoggingLevel(LogEventLevel)"/>. The console output uses the
		/// <see cref="DefaultOutputTemplate"/> format and log events are enriched from
		/// <see cref="Serilog.Context.LogContext"/>.
		/// <para>
		/// Note: This method sets the global <see cref="LoggerConfiguration.MinimumLevel"/> to
		/// <see cref="LogEventLevel.Verbose"/>, which affects all sinks, not just the console sink.
		/// The console sink itself is filtered by the shared <see cref="LoggingLevelSwitch"/>.
		/// </para>
		/// </summary>
		/// <param name="loggingLevel">The minimum <see cref="LogEventLevel"/> for events written to the console.</param>
		/// <returns>The current <see cref="SetupSerilog"/> instance for fluent method chaining.</returns>
		public SetupSerilog UseConsole(LogEventLevel loggingLevel) {
			Action<LoggerConfiguration> action = cfg => UseConsole(cfg, loggingLevel);
			configActions += action;
			return this;
		}

		public SetupSerilog Configure(Action<LoggerConfiguration> action) {
			this.configActions += action;
			return this;
		}

		public Logger Create(bool setDefault = true) {
			LoggerConfiguration cfg = new LoggerConfiguration();
			configActions?.Invoke(cfg);
			var logger = cfg.CreateLogger();
			if (setDefault) {
				Log.Logger = logger;
			}
			return logger;
		}

		[Obsolete("Use the overload that includes the 'optional' parameter")]
		public static void UseConfigFile(LoggerConfiguration cfg, string environment, string? basePath, string[]? commandlineArgs)
			=> UseConfigFile(cfg, environment, basePath, commandlineArgs, false);

		public static void UseConfigFile(LoggerConfiguration cfg, string environment, string? basePath, string[]? commandlineArgs, bool optional) {
			if (string.IsNullOrEmpty(basePath)) {
				basePath = AppContext.BaseDirectory;
			}
			var configBuilder = new ConfigurationBuilder()
				.SetBasePath(basePath!)
				.AddJsonFile("serilog.json", optional, true);
			if (!string.IsNullOrEmpty(environment)) { configBuilder.AddJsonFile($"serilog.{environment}.json", true, true); }
			configBuilder.AddEnvironmentVariables();
			var configuration = configBuilder.Build();
			cfg.ReadFrom.Configuration(configuration);
		}

		private static LoggingLevelSwitch consoleLoggingLevelSwitch = new LoggingLevelSwitch();

		/// <summary>
		/// Configures a console sink on the provided <see cref="LoggerConfiguration"/>. If <paramref name="loggingLevel"/>
		/// is not null, the shared <see cref="LoggingLevelSwitch"/> is updated to the specified level.
		/// Output is formatted using <see cref="DefaultOutputTemplate"/> and log events are enriched from
		/// <see cref="Serilog.Context.LogContext"/>.
		/// <para>
		/// Note: This method sets the global <see cref="LoggerConfiguration.MinimumLevel"/> to
		/// <see cref="LogEventLevel.Verbose"/>, which affects all sinks, not just the console sink.
		/// The console sink itself is filtered by the shared <see cref="LoggingLevelSwitch"/>.
		/// </para>
		/// </summary>
		/// <param name="cfg">The <see cref="LoggerConfiguration"/> to configure.</param>
		/// <param name="loggingLevel">The minimum <see cref="LogEventLevel"/> for the console sink, or null to keep the current level.</param>
		public static void UseConsole(LoggerConfiguration cfg, LogEventLevel? loggingLevel) {
			if (loggingLevel != null) {
				consoleLoggingLevelSwitch.MinimumLevel = loggingLevel.Value;
			}
			cfg.MinimumLevel.Verbose()
				.WriteTo.Console(levelSwitch: consoleLoggingLevelSwitch, outputTemplate: DefaultOutputTemplate)
				.Enrich.FromLogContext();
		}
		public static void SwitchConsoleLoggingLevel(LogEventLevel loggingLevel) {
			consoleLoggingLevelSwitch.MinimumLevel = loggingLevel;
		}
	}
}