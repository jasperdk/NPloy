Param($ServiceUsername, $ServicePassword)

$ErrorActionPreference = "stop"

try
{

	Write-Host "Running install script"	
	& .\NPloy.Samples.WindowsService.exe install 
	#-username $ServiceUsername -password $ServicePassword
	
}
catch
{
    $ErrorActionPreference = "Continue";   
    Write-Error $_

    Exit 1
}
