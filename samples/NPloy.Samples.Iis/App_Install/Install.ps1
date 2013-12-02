Param($ServiceUsername, $ServicePassword)

$ErrorActionPreference = "stop"

try
{	
	Import-Module servermanager
	Add-WindowsFeature Web-Server
	Add-WindowsFeature Application-Server
	Add-WindowsFeature MSMQ
}
catch
{
    $ErrorActionPreference = "Continue";   
    Write-Error $_

    Exit 1
}
