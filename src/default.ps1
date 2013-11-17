Task Default -depends Test

Task Build {
   Exec { msbuild "NPloy.sln" }
}

Task Test -depends Build {
   Exec { packages\NUnit.Runners.2.6.3\tools\nunit-console.exe NPloy.Console.UnitTests\bin\debug\NPloy.Console.UnitTests.dll /xml=TestResult.xml  }   
}