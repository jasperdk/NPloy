Param()
$ErrorActionPreference = "Stop"
try
{
Import-Module .\psake.psm1
pushd src
Invoke-Psake default.ps1 pack
popd

pushd samples
Invoke-Psake default.ps1 pack
popd
}
catch
{
    $ErrorActionPreference = "Continue";   
    Write-Error $_

    Exit 1
}