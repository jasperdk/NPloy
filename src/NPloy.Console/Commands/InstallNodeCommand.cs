using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ManyConsole;

namespace NPloy.Commands
{
    public class InstallNodeCommand : ConsoleCommand
    {
        public const string PackageFileName = "packages.config";
        public InstallNodeCommand()
            : this(new InstallRoleCommand())
        {
        }

        public InstallNodeCommand(IInstallRoleCommand installRoleCommand)
        {
            IsCommand("InstallNode", "InstallNode");
            HasAdditionalArguments(1, "Node");
            HasOption("d|directory=", "Install to this directory", s => WorkingDirectory = s);
            _installRoleCommand = installRoleCommand;
        }

        public string Node;
        public string WorkingDirectory;
        private readonly IInstallRoleCommand _installRoleCommand;

        public override int Run(string[] remainingArguments)
        {
            if (File.Exists(PackageFileName))
            {
                Console.WriteLine("Node has already been installed. Uninstall or update!");
                return -1;
            }

            HandleArguments(remainingArguments);

            File.Delete(PackageFileName);

            if (!Node.ToLower().EndsWith(".node"))
                Node += ".node";
            if (!File.Exists(Node))
                throw new FileNotFoundException(Node);

            Console.WriteLine("Installing node: " + Node);
            var basePath = Path.GetDirectoryName(Node);
            var roles = GetRolesFromFile();
            foreach (var role in roles)
            {
                _installRoleCommand.WorkingDirectory = WorkingDirectory;
                _installRoleCommand.Run(new[] { basePath + @"\roles\" + role });
            }

            return 0;
        }

        private void HandleArguments(string[] remainingArguments)
        {
            _installRoleCommand.WorkingDirectory = WorkingDirectory;
            Node = remainingArguments[0];
        }

        private IEnumerable<string> GetRolesFromFile()
        {
            var roles = new List<string>();
            var doc = new XmlDocument();
            doc.Load(Node);
            var docRoles = doc.GetElementsByTagName("role");
            foreach (XmlNode docRole in docRoles)
            {
                roles.Add(docRole.Attributes["name"].Value);
            }
            return roles;
        }
    }


}