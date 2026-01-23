using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.CommandLine {
	[Verb<Test>("test")]
	public class TestOptions { }
	public class Test : IAsyncCommandHandler{
		private readonly ILogger<Test> logger;

		public Test(ILogger<Test> logger) {
			this.logger = logger;
		}
		public Task<int> InvokeAsync(CancellationToken cancellationToken) {
			logger.LogInformation("An info msg");
			logger.LogWarning("A warning msg");
			logger.LogError ("An err msg");
			return Task.FromResult(0);
		}
	}
}