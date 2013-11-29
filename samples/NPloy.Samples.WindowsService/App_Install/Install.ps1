Param($ServiceUsername, $ServicePassword)

$ErrorActionPreference = "stop"

try
{
	
	& .\NPloy.Samples.WindowsService.exe install -username $ServiceUsername -password $ServicePassword
	Exit 100
}
catch
{
    $ErrorActionPreference = "Continue";   
    Write-Error $_

    Exit 1
}
