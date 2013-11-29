using System.IO;
using Moq;
using NPloy.Commands;
using NUnit.Framework;

namespace NPloy.Console.UnitTests.Commands
{
    [TestFixture]
    public class InstallRoleCommandTests
    {
        private InstallRoleCommand _command;
        private Mock<IInstallPackageCommand> _installPackageCommandMock;
        private const string RoleCommandFile="test.role";

        [SetUp]
        public void SetUp()
        {
            _installPackageCommandMock = new Mock<IInstallPackageCommand>();
            _command = new InstallRoleCommand();

            if (File.Exists(RoleCommandFile))
                File.Delete(RoleCommandFile);
        }

        [Test, Ignore("Not implemented")]
        public void ShouldMethod()
        {
            // Arrange

            // Act
            _command.Role = RoleCommandFile;
            _command.Run(new string[0]);

            // Assert
            _installPackageCommandMock.VerifySet(c => c.Package="test.role",Times.Once());
        }
    }

    
}