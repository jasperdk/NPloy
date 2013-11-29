using System.IO;
using Moq;
using NPloy.Commands;
using NUnit.Framework;

namespace NPloy.Console.UnitTests.Commands
{
    [TestFixture]
    public class InstallNodeCommandTests
    {
        private InstallNodeCommand _command;
        private Mock<IInstallRoleCommand> _installRoleCommandMock;

        [SetUp]
        public void SetUp()
        {
            _installRoleCommandMock = new Mock<IInstallRoleCommand>();
            _command = new InstallNodeCommand(_installRoleCommandMock.Object);

            if (File.Exists(InstallNodeCommand.PackageFileName))
                File.Delete(InstallNodeCommand.PackageFileName);
        }

        [Test]
        public void Run_WhenArgumentIsNodeFile_ShouldReturn0()
        {
            // Arrange
            CreateNodeFile("test.node", "test.role");

            // Act
            var result = _command.Run(new[] { "test.node" });

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void Run_WhenArgumentIsNodeFile_ShouldInstall()
        {
            // Arrange
            CreateNodeFile("test.node", "test1.role");

            // Act
            _command.Run(new[] { "test.node" });

            // Assert
            _installRoleCommandMock.Verify(c => c.Run(new []{"test1.role"}), Times.Once());
        }

        [Test]
        public void Run_WhenArgumentIsNodeFileWithoutFileExtension_ShouldInstall()
        {
            // Arrange
            CreateNodeFile("test.node", "test1.role");

            // Act
            _command.Run(new[] { "test" });

            // Assert
            _installRoleCommandMock.Verify(c => c.Run(new[] { "test1.role" }), Times.Once());
        }

        [Test]
        public void Run_ShouldPassOnEnvironment()
        {
            // Arrange
            CreateNodeFileForEnvironment("test.node","test", "test1.role");

            // Act
            _command.Run(new[] { "test.node" });

            // Assert
            _installRoleCommandMock.VerifySet(c => c.Environment = "test", Times.Once());
        }


        [Test]
        public void Run_WhenArgumentIsNodeFile_ShouldInstallAllRoles()
        {
            // Arrange
            CreateNodeFile("test.node", "test1.role", "test2.role");

            // Act
            _command.Run(new[] { "test.node" });

            // Assert
            _installRoleCommandMock.Verify(c => c.Run(new[] { "test1.role" }), Times.Once());
            _installRoleCommandMock.Verify(c => c.Run(new[] { "test2.role" }), Times.Once());
        }

        [Test]
        public void Run_WhenArgumentFileDoesNotExists_ShouldThrowException()
        {
            var result = _command.Run(new[] { "filedoesntexists.node" });

            Assert.That(result,Is.EqualTo(1));
        }

        [Test]
        public void Run_WhenPackageFileExists_ShouldReturnMinus1AndNotCallCommand()
        {
            // Arrange
            File.WriteAllText(InstallNodeCommand.PackageFileName, "test");

            // Act
            var result = _command.Run(new[] { "test.node" });

            // Assert
            Assert.That(result, Is.EqualTo(-1));
            _installRoleCommandMock.Verify(c => c.Run(It.IsAny<string[]>()), Times.Never);
        }

        private static void CreateNodeFile(string node, params string[] roles)
        {
           CreateNodeFileForEnvironment(node,"dev",roles);
        }

        private static void CreateNodeFileForEnvironment(string node, string enviroment, params string[] roles)
        {
            var content = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?><node environment=""{0}""><roles>", enviroment);
            foreach (var role in roles)
            {
                content += @"<role name=""" + role + @""" />";
            }
            content += @"</roles></node>";
            File.WriteAllText(node, content);
        }
    }
}