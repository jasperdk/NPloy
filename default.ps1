Task Install {
   Exec { & install\nploy.exe installnode install\.nploy\localhost -d d:\temp\apps -p d:\projects\privat\nploy\packages -s }
}

Task Uninstall {
   Exec {  & install\nploy.exe uninstallnode -d d:\temp\apps -r }
}