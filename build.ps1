Import-Module .\psake.psm1
pushd src
Invoke-Psake default.ps1
popd