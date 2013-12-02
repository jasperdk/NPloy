Param($PackagePath)

$ErrorActionPreference = "stop"

try
{	
	Import-Module ApplicationServer
	Import-Module WebAdministration
	
	#[Reflection.Assembly]::LoadWithPartialName("System.Messaging")


	$appName = "NPloySample"
	$appPool = "DefaultAppPool"
	$appPath = $PackagePath
	$site = Get-WebSite | where { $_.Name -eq $appName }
	if($site -eq $null)
	{
	#	New-WebApplication -Name $appName -ApplicationPool $appPool -PhysicalPath $appPath -Site "Default Web Site" -force
	#	Invoke-Expression -command "c:\windows\system32\inetsrv\appcmd.exe set app 'Default Web Site/$appName' /enabledprotocols:'$protocols'"
	}
	else
	{
	#	WriteToHost "Web application $appName already exists"
	}
}
catch
{
    $ErrorActionPreference = "Continue";   
    Write-Error $_

    Exit 1
}
