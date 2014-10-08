Task Default -depends Build

Task Build {
   Exec { msbuild "NPloy.Samples.sln" }
}

Task Pack -depends Build {
   Exec { & .\.nuget\nuget pack NPloy.Samples.WindowsService\NPloy.Samples.WindowsService.nuspec -NoPackageAnalysis -OutputDirectory ..\output\packages }
   Exec { & .\.nuget\nuget pack NPloy.Samples.SmokeTests\NPloy.Samples.SmokeTests.nuspec -NoPackageAnalysis -OutputDirectory ..\output\packages }
   Exec { & .\.nuget\nuget pack NPloy.Samples.Iis\NPloy.Samples.Iis.nuspec -NoPackageAnalysis -OutputDirectory ..\output\packages }
   Exec { & .\.nuget\nuget pack NPloy.Samples.Web\NPloy.Samples.Web.nuspec -NoPackageAnalysis -OutputDirectory ..\output\packages }
   Exec { & xcopy /y /e .nploy\*.* ..\install\.nploy\}
}