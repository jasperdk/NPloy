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
        private Mock<INuGetRunner> _nugetRunner;

        [SetUp]
        public void SetUp()
        {
            _nPloyConfiguration = new Mock<INPloyConfiguration>();
            _powershellRunner = new Mock<IPowershellRunner>();
            _nugetRunner = new Mock<INuGetRunner>();
            _command = new InstallPackageCommand(_nPloyConfiguration.Object, _powershellRunner.Object, _nugetRunner.Object);
        }

        [Test]
        public void Run_ShouldCallPowershellScript()
        {
            // Arrange

            _nugetRunner.Setup(n => n.RunNuGetInstall("NPloy.Samples.WindowsService", null, null, It.IsAny<string>()))
                        .Returns(new List<string> { "NPloy.Samples.WindowsService.1.0.0.0" });

            _nPloyConfiguration.Setup(f => f.GetFiles(It.Is<string>(s => s.EndsWith(@"NPloy.Samples.WindowsService.1.0.0.0\App_Install"))))
                            .Returns(new List<string> { @"d:\NPloy.Samples.WindowsService.1.0.0.0\app_install\install.ps1" });

            var properties = new Dictionary<string, string>();
            properties.Add("propkey", "propvalue");
            _nPloyConfiguration.Setup(n => n.GetProperties("NPloy.Samples.WindowsService", "test", It.IsAny<string>())).Returns(properties);


            // Act
            _command.Environment = "test";
            _command.Run(new[] { @"NPloy.Samples.WindowsService" });

            // Assert
            _powershellRunner.Verify(
                p =>
                p.RunPowershellScript(@"App_Install\install.ps1 -propkey 'propvalue'",
                                      It.Is<string>(s => s.EndsWith(@"\NPloy.Samples.WindowsService.1.0.0.0"))), Times.Once());

        }
    }
}