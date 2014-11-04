Param($ServiceUsername, $ServicePassword,$ServiceDisplayName,$ServiceDescription)

$ErrorActionPreference = "stop"

try
{

	Write-Host "Running install script"	
	& .\NPloy.Samples.WindowsService.exe install -displayname $ServiceDisplayName -description $ServiceDescription
	#-username $ServiceUsername -password $ServicePassword
	
}
catch
{
    $ErrorActionPreference = "Continue";   
    Write-Error $_

    Exit 1
}
