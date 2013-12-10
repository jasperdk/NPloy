Task Default -depends Test

Task Build {
   Exec { msbuild "NPloy.sln" }
   Exec { & copy NPloy.Console\bin\debug\NPloy.Console.exe NPloy.Console\bin\debug\NPloy.exe}
   Exec { & .\packages\LibZ.Bootstrap.1.0.3.7\tools\libz.exe inject-dll --assembly NPloy.Console\bin\debug\NPloy.exe --include NPloy.Console\bin\debug\*.dll --move }
   Exec { & xcopy /y /e NPloy.Console\bin\debug\nploy.exe ..\install\}
}

Task UnitTest {
   Exec { & packages\NUnit.Runners.2.6.3\tools\nunit-console.exe NPloy.Console.UnitTests\bin\debug\NPloy.Console.UnitTests.dll /xml=UnitTestResult.xml  }      
}

Task IntegrationTest {
   Exec { & packages\NUnit.Runners.2.6.3\tools\nunit-console.exe NPloy.Console.IntegrationTests\bin\debug\NPloy.Console.IntegrationTests.dll /xml=IntegrationTestResult.xml  }      
}

Task Test -depends Build, UnitTest  {
}

Task Pack -depends Test {
	Exec { & .nuget\nuget pack NPloy.Console\NPloy.Console.nuspec -NoPackageAnalysis -OutputDirectory ..\packages }  
}