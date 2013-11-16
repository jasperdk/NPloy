Task Default -depends Build

Task Build {
   Exec { msbuild "NPloy.Samples.sln" }
}

Task Pack -depends Build {
   Exec { nuget pack NPloy.Samples.WindowsService\NPloy.Samples.WindowsService.nuspec -NoPackageAnalysis -OutputDirectory d:\temp\packages }
   Exec { xcopy /y /e configuration\*.* d:\temp\apps }
}