NPloy
=====
The simple release and configuration management system for Windows. NPloy is build on a foundation of NuGet and Powershell gluing it together with conventions and a bit of .NET-code. Check this short slide deck for an introduction https://speakerdeck.com/jasperdk/configuration-and-release-management-with-nploy

I needed something like OctopusDeploy/Puppet-ish with as few dependencies to 3rd party stuff - like Ruby - for a Windows-only environement.

If you can live with having your configuration in a non-versioned database behind a web-service then just go for OctopusDeploy. It's cool and you can probably buy some kind of support.

Until further you will have to go to the included samples to see how it works and they are also the only documentation. 

NPloy has a continuous integration job running on <a
href="http://www.jetbrains.com/teamcity" title="TeamCity by JetBrains">TeamCity by JetBrains</a>


