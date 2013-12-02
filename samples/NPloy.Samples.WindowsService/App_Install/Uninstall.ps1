Param()
$ErrorActionPreference = "Stop"
try
{
	& .\NPloy.Samples.WindowsService.exe uninstall
}
catch
{
    $ErrorActionPreference = "Continue";   
    Write-Error $_

    Exit 1
}

