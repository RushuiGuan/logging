using Albatross.CommandLine;
using Albatross.CommandLine.Annotations;
using Albatross.CommandLine.Inputs;
using Albatross.LogAnalysis.Models;
using Microsoft.EntityFrameworkCore;
using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Albatross.LogAnalysis.Admin.Database {
	[Verb<CreateTableScript>("database create-script", Description = "Create the table script")]
	public record class CreateTableScriptParams : BaseParams {
		[UseOption<OutputFileOption>]
		public FileInfo? OutputFile { get; init; }
	}

	public class CreateTableScript : BaseHandler<CreateTableScriptParams> {
		private readonly IAnalysisDbSession session;

		public CreateTableScript(IAnalysisDbSession session, ParseResult result, CreateTableScriptParams parameters) : base(result, parameters) {
			this.session = session;
		}

		public override async Task<int> InvokeAsync(CancellationToken cancellationToken) {
			string script = session.DbContext.Database.GenerateCreateScript();
			if (parameters.OutputFile != null) {
				if (parameters.OutputFile.Directory != null) {
					Directory.CreateDirectory(parameters.OutputFile.Directory.FullName);
				}
				await File.WriteAllTextAsync(parameters.OutputFile.FullName, script, cancellationToken);
			} else {
				await this.Writer.WriteLineAsync(script);
			}
			return 0;
		}
	}
}