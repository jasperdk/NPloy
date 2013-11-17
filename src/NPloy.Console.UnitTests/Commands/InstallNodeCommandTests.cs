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

            if(File.Exists(InstallNodeCommand.PackageFileName))
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
        public void Run_WhenArgumentIsNodeFile_ShouldInstallAllRoles()
        {
            // Arrange
            CreateNodeFile("test.node", "test1.role", "test2.role");

            // Act
            _command.Run(new[] { "test.node" });

            // Assert
            _installRoleCommandMock.Verify(c => c.Run(It.Is<string[]>(s => s[0] == @"roles\test1.role")), Times.Once());
            _installRoleCommandMock.Verify(c => c.Run(It.Is<string[]>(s => s[0] == @"roles\test2.role")), Times.Once());
        }

        [Test]
        public void Run_WhenArgumentFileDoesNotExists_ShouldThrowException()
        {
            Assert.Throws<FileNotFoundException>(() => _command.Run(new[] { "filedoesntexists.node" }));
        }

        [Test]
        public void Run_WhenPackageFileExists_ShouldReturnMinus1AndNotCallCommand()
        {
            // Arrange
            File.WriteAllText(InstallNodeCommand.PackageFileName, "test");

            // Act
            var result = _command.Run(new[] { "test.node" });

            // Assert
            Assert.That(result , Is.EqualTo(-1));
            _installRoleCommandMock.Verify(c=>c.Run(It.IsAny<string[]>()),Times.Never);
        }

        private static void CreateNodeFile(string node, params string[] roles)
        {
            var content = @"<?xml version=""1.0"" encoding=""utf-8""?><node><roles>";
            foreach (var role in roles)
            {
                content += @"<role name=""" + role + @""" />";
            }
            content += @"</roles></node>";
            File.WriteAllText(node, content);
        }
    }
}