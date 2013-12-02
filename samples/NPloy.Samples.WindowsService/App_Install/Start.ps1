Param()
$ErrorActionPreference = "Stop"
try
{
	& .\NPloy.Samples.WindowsService.exe start
}
catch
{
    $ErrorActionPreference = "Continue";   
    Write-Error $_

    Exit 1
}

