﻿Param()
$ErrorActionPreference = "Stop"
try
{
	& .\NPloy.Samples.WindowsService.exe stop
}
catch
{
    $ErrorActionPreference = "Continue";   
    Write-Error $_

    Exit 1
}

