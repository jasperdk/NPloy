Param()
$ErrorActionPreference = "Stop"
try
{
	& ..\NUnit.Runners.2.6.3\tools\nunit-console.exe NPloy.Samples.SmokeTests.dll /xml=SmokeTestResult.xml
}
catch
{
    $ErrorActionPreference = "Continue";   
    Write-Error $_

    Exit 1
}