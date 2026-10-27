$location = Get-Location;
Set-Location $PSScriptRoot;

if(Test-Path (Join-Path $PSScriptRoot Migrations Postgres)){
	$count = (Get-ChildItem (Join-Path $PSScriptRoot Migrations Postgres) -Recurse -Filter *_AnalysisPostgresMigration_v*.Designer.cs | Measure-Object).Count + 1;
}else{
	$count = 1;
}

$name = "v$count";
"Creating migration $name";
dotnet ef migrations add AnalysisPostgresMigration_$name --context AnalysisPostgresMigration --output-dir (Join-Path Migrations Postgres)
Set-Location $location;