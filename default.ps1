Task Install -depends Uninstall{
   Exec { & install\nploy.exe installnode install\.nploy\localhost -d d:\temp\apps -p "d:\projects\privat\nploy\output\packages;https://www.nuget.org/api/v2/" -s -n samples\packages\NuGet.CommandLine.2.8.3\tools }
}

Task Uninstall {
   Exec {  & install\nploy.exe uninstallnode -d d:\temp\apps -r }
}