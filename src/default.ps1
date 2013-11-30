Task Default -depends Test

Task Build {
   Exec { msbuild "NPloy.sln" }
   Exec { xcopy /y /e NPloy.Console\bin\debug\*.* ..\install\}
}

Task UnitTest {
   Exec { packages\NUnit.Runners.2.6.3\tools\nunit-console.exe NPloy.Console.UnitTests\bin\debug\NPloy.Console.UnitTests.dll /xml=UnitTestResult.xml  }      
}

Task IntegrationTest {
   Exec { packages\NUnit.Runners.2.6.3\tools\nunit-console.exe NPloy.Console.IntegrationTests\bin\debug\NPloy.Console.IntegrationTests.dll /xml=IntegrationTestResult.xml  }      
}

Task Test -depends Build, UnitTest, IntegrationTest {
}

Task Pack -depends Test {
   Exec { nuget pack NPloy.Console\NPloy.Console.nuspec -OutputDirectory ..\packages }  
}