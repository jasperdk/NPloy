using System.Collections.Generic;
using Moq;
using NPloy.Commands;
using NPloy.Support;
using NUnit.Framework;

namespace NPloy.Console.UnitTests.Commands
{
    [TestFixture]
    public class InstallPackageCommandTests
    {
        private InstallPackageCommand _command;
        private Mock<INPloyConfiguration> _nPloyConfiguration;
        private Mock<IPowershellRunner> _powershellRunner;

        [SetUp]
        public void SetUp()
        {
            _nPloyConfiguration = new Mock<INPloyConfiguration>();
            _powershellRunner = new Mock<IPowershellRunner>();
            _command = new InstallPackageCommand(_nPloyConfiguration.Object, _powershellRunner.Object);
        }

        [Test]
        public void Run_ShouldCallPowershellScript()
        {
            // Arrange
            _nPloyConfiguration.Setup(f => f.GetFiles(@"\NPloy.Samples.WindowsService.1.0.0.0\App_Install\"))
                            .Returns(new List<string> { @"d:\NPloy.Samples.WindowsService.1.0.0.0\app_install\install.ps1" });

            var properties = new Dictionary<string, string>();
            properties.Add("propkey", "propvalue");
            _nPloyConfiguration.Setup(n => n.GetProperties(It.IsAny<string>(), "NPloy.Samples.WindowsService.1.0.0.0")).Returns(properties);

            // Act
            _command.Run(new []{@"NPloy.Samples.WindowsService 1.0.0.0"});

            // Assert
            _powershellRunner.Verify(
                p =>
                p.RunPowershellScript(@"App_Install\install.ps1 -propkey 'propvalue'",
                                      @"\NPloy.Samples.WindowsService.1.0.0.0"), Times.Once());

        }
    }
}