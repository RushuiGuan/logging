$location = get-location
get-item $PSScriptRoot\*.Generated.cs | remove-item;

$project = get-item $PSScriptRoot\..\Albatross.LogAnalysis.Daemon\Albatross.LogAnalysis.Daemon.csproj
codegen csharp web-client -p $project.FullName -o $PSScriptRoot -s $PSScriptRoot/codegen-settings.json