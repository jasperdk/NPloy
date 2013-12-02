using Moq;
using NPloy.Commands;
using NPloy.Support;
using NUnit.Framework;

namespace NPloy.Console.UnitTests.Commands
{
    [TestFixture]
    public class StartPackageCommandTests
    {
        private StartPackageCommand _command;
        private Mock<INPloyConfiguration> _nPloyConfiguration;
        private Mock<IPowershellRunner> _powershellRunner;

        [SetUp]
        public void SetUp()
        {
            _nPloyConfiguration = new Mock<INPloyConfiguration>();
            _powershellRunner = new Mock<IPowershellRunner>();
            _command = new StartPackageCommand(_nPloyConfiguration.Object, _powershellRunner.Object);
        }

        [Test]
        public void Run_ShouldCallPowershellScript()
        {
            // Arrange
            _nPloyConfiguration.Setup(f => f.FileExists(It.Is<string>(s => s.EndsWith(@"\NPloy.Samples.WindowsService.1.0.0.0\App_Install\Start.ps1"))))
                            .Returns(true);

            // Act
            _command.Run(new[] { @"NPloy.Samples.WindowsService 1.0.0.0" });

            // Assert
            _powershellRunner.Verify(
                p =>
                p.RunPowershellScript(@".\App_Install\Start.ps1", null, It.Is<string>(s => s.EndsWith(@"\NPloy.Samples.WindowsService.1.0.0.0"))),
                Times.Once());

        }
    }
}