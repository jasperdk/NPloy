using System.IO;
using NPloy.Support;
using NUnit.Framework;

namespace NPloy.Console.IntegrationTests
{
    [TestFixture]
    public class IntegrationTests
    {
        private CommandRunner _commandRunner;

        [SetUp]
        public void SetUp()
        {
            _commandRunner = new CommandRunner();

            // Uninstall
            System.Console.WriteLine(_commandRunner.RunCommand("nploy.exe", @"uninstallnode", @"..\..\..\..\install"));

            // Delete files
            var directories = Directory.GetDirectories(@"..\..\..\..\install");
            foreach (var directory in directories)
            {
                if (Path.GetFileName(directory) != ".nploy")
                    Directory.Delete(directory, true);
            }
        }

        [Test]
        public void ShouldInstallAndUninstall()
        {

            // Install node
            System.Console.WriteLine(_commandRunner.RunCommand("nploy.exe", @"installnode .nploy\localhost -p d:\projects\privat\nploy\packages", @"..\..\..\..\install"));
            
            // Verify installation

            // Uninstall
            System.Console.WriteLine(_commandRunner.RunCommand("nploy.exe", @"uninstallnode", @"..\..\..\..\install"));

            // Verify uninstallation
        }
    }
}