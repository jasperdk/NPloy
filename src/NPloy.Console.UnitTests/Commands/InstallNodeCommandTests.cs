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
        private Mock<ICommandFactory> _commandFactory;
        private Mock<InstallRoleCommand> _installRoleCommandMock;
        private Mock<StartNodeCommand> _startCommandMock;

        [SetUp]
        public void SetUp()
        {
            _commandFactory = new Mock<ICommandFactory>();
            _command = new InstallNodeCommand(_commandFactory.Object);

            _installRoleCommandMock = new Mock<InstallRoleCommand>();
            _commandFactory.Setup(x => x.GetCommand<InstallRoleCommand>()).Returns(_installRoleCommandMock.Object);

            _startCommandMock = new Mock<StartNodeCommand>();
            _commandFactory.Setup(x => x.GetCommand<StartNodeCommand>()).Returns(_startCommandMock.Object);

            if (File.Exists(InstallNodeCommand.PackageFileName))
                File.Delete(InstallNodeCommand.PackageFileName);
        }

        [Test]
        public void Run_WhenArgumentIsNodeFile_ShouldInstall()
        {
            // Arrange
            CreateNodeFile("test.node", "test1.role");

            // Act
            var result = _command.Run(new[] { "test.node" });

            // Assert
            Assert.That(result, Is.EqualTo(0));
            _installRoleCommandMock.Verify(c => c.Run(new[] { "test1.role" }), Times.Once());
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
            CreateNodeFileForEnvironment("test.node", "test", "test1.role");

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
        public void Run_WhenArgumentFileDoesNotExists_ShouldReturnError()
        {
            var result = _command.Run(new[] { "filedoesntexists.node" });

            Assert.That(result, Is.EqualTo(1));
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

        [Test]
        public void Run_WhenStart_ShouldCallStartNode()
        {
            // Arrange

            // Act
            _command.AutoStart = true;
            var result = _command.Run(new[] { "test.node" });

            // Assert
            Assert.That(result, Is.EqualTo(0));
            _startCommandMock.Verify(c => c.Run(new[] { "test.node" }), Times.Once);
        }

        [Test]
        public void Run_ShouldPassOnParametersToNodeCommand()
        {
            // Arrange
            CreateNodeFile(@"test.node", "test1.role");

            // Act
            _command.PackageSources = "packagesources";
            _command.NuGetPath = "nugetpath";
            _command.Run(new[] { @"test.node" });

            // Assert
            _installRoleCommandMock.VerifySet(c => c.PackageSources = "packagesources", Times.Once());
            _installRoleCommandMock.VerifySet(c => c.AutoStart = true, Times.Never);
            _installRoleCommandMock.VerifySet(c => c.NuGetPath = "nugetpath", Times.Once());
            _installRoleCommandMock.VerifySet(c => c.ConfigurationDirectory = "", Times.Once());
        }

        [Test]
        public void Run_WhenNPloyConfigIsInOtherFolder_ShouldSetConfigurationProperty()
        {
            // Arrange
            CreateNodeFile(@".nploy\test.node", "test1.role");

            // Act
            _command.Run(new[] { @".nploy\test.node" });

            // Assert
            _installRoleCommandMock.VerifySet(c => c.ConfigurationDirectory = ".nploy", Times.Once());
        }

        private static void CreateNodeFile(string node, params string[] roles)
        {
            CreateNodeFileForEnvironment(node, "dev", roles);
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