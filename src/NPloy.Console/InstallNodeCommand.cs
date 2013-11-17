using System.Collections.Generic;
using System.IO;
using System.Xml;
using ManyConsole;

namespace NPloy
{
    public class InstallNodeCommand : ConsoleCommand
    {
        public InstallNodeCommand()
        {
            IsCommand("InstallNode", "InstallNode");
            HasAdditionalArguments(1, "Node");
            HasOption("d|directory=", "Install to this directory", s => WorkingDirectory = s);
        }

        public string Node;
        public string WorkingDirectory;

        public override int Run(string[] remainingArguments)
        {
            var roles = new List<string>();

            Node = remainingArguments[0];
            if (Node.ToLower().EndsWith(".node") && File.Exists(Node))
            {
                var doc = new XmlDocument();
                doc.Load(Node);
                var docRoles = doc.GetElementsByTagName("role");
                foreach (XmlNode docRole in docRoles)
                {
                    roles.Add(docRole.Attributes["name"].Value);
                }
            }
            else
            {
                roles.Add(Node);
            }

            foreach (var role in roles)
            {
                var installRoleCommand = new InstallRoleCommand { WorkingDirectory = WorkingDirectory };
                if (role.ToLower().EndsWith(".role"))
                    installRoleCommand.Run(new[] { @"roles\" + role });
                else
                    installRoleCommand.Run(new[] { role });
            }

            return 0;
        }
    }
}