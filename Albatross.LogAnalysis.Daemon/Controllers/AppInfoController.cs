using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Albatross.LogAnalysis.Daemon.Controllers {
	[ApiController]
	[Route("api/v1/app-info")]
	public class AppInfoController : ControllerBase {
		[HttpGet("version")]
		public string Version() => Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? string.Empty;
	}
}