using System.IO;
using Moq;
using NPloy.Commands;
using NPloy.Support;
using NUnit.Framework;

namespace NPloy.Console.UnitTests.Commands
{
    [TestFixture]
    public class InstallRoleCommandTests
    {
        private Mock<ICommandFactory> _commandFactory;
        private InstallRoleCommand _command;
        private Mock<InstallPackageCommand> _installPackageCommandMock;
        private Mock<INPloyConfiguration> _nployConfigurationMock;
        private string _currentDirectory;
        private const string RoleCommandFile = "test.role";

        [SetUp]
        public void SetUp()
        {
            _installPackageCommandMock = new Mock<InstallPackageCommand>();
            _nployConfigurationMock = new Mock<INPloyConfiguration>();

            _currentDirectory = Directory.GetCurrentDirectory();

            _commandFactory = new Mock<ICommandFactory>();

            _command = new InstallRoleCommand(_nployConfigurationMock.Object,_commandFactory.Object);
        }

        [Test]
        public void ShouldMethod()
        {
            // Arrange
            var roleFile = Path.Combine(_currentDirectory, @"roles\" + RoleCommandFile);
            _nployConfigurationMock.Setup(x => x.FileExists(roleFile)).Returns(true);
            var roleConfig = new RoleConfig();
            roleConfig.Packages.Add(new PackageConfig{Id="Test.Package"});
            _nployConfigurationMock.Setup(x => x.GetRoleConfig(roleFile)).Returns(roleConfig);

            _commandFactory.Setup(x => x.GetCommand<InstallPackageCommand>()).Returns(_installPackageCommandMock.Object);

            // Act
            var result = _command.Run(new[] { RoleCommandFile });

            // Assert
            Assert.That(result, Is.EqualTo(0));
            _installPackageCommandMock.Verify(c => c.Run(new[] { "Test.Package" }), Times.Once());
        }
    }


}