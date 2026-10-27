$location = Get-Location;
Set-Location $PSScriptRoot;

if(Test-Path (Join-Path $PSScriptRoot Migrations SqlServer)){
	$count = (Get-ChildItem (Join-Path $PSScriptRoot Migrations SqlServer) -Recurse -Filter *_AnalysisSqlServerMigration_v*.Designer.cs | Measure-Object).Count + 1;
}else{
	$count = 1;
}

$name = "v$count";
"Creating migration $name";
dotnet ef migrations add AnalysisSqlServerMigration_$name --context AnalysisSqlServerMigration --output-dir (Join-Path Migrations SqlServer)
Set-Location $location;