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
            _nPloyConfiguration.DefaultValue = DefaultValue.Mock;
            _powershellRunner = new Mock<IPowershellRunner>();
            _nugetRunner = new Mock<INuGetRunner>();
            _command = new InstallPackageCommand(_nPloyConfiguration.Object, _powershellRunner.Object, _nugetRunner.Object);
        }

        [Test]
        public void Run_ShouldCallPowershellScript()
        {
            // Arrange

            _nugetRunner.Setup(n => n.RunNuGetInstall("NPloy.Samples.WindowsService", null, null, It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(new List<string> { "NPloy.Samples.WindowsService.1.0.0.0" });

            _nPloyConfiguration.Setup(f => f.GetFiles(It.Is<string>(s => s.EndsWith(@"NPloy.Samples.WindowsService.1.0.0.0\App_Install"))))
                            .Returns(new List<string> { @"d:\NPloy.Samples.WindowsService.1.0.0.0\app_install\install.ps1" });

            // Act
            _command.Run(new[] { @"NPloy.Samples.WindowsService" });

            // Assert
            _powershellRunner.Verify(
                p =>
                p.RunPowershellScript(@"App_Install\install.ps1", It.IsAny<string>(),
                                      It.Is<string>(s => s.EndsWith(@"\NPloy.Samples.WindowsService.1.0.0.0"))), Times.Once());

        }

        [Test]
        public void Run_ShouldCallPowershellScriptWithPropertyParameter()
        {
            // Arrange

            _nugetRunner.Setup(n => n.RunNuGetInstall( It.IsAny<string>(),null, null, It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(new List<string> { "NPloy.Samples.WindowsService.1.0.0.0" });

            _nPloyConfiguration.Setup(f => f.GetFiles(It.Is<string>(s => s.EndsWith(@"NPloy.Samples.WindowsService.1.0.0.0\App_Install"))))
                            .Returns(new List<string> { @"d:\NPloy.Samples.WindowsService.1.0.0.0\app_install\install.ps1" });

            var properties = new Dictionary<string, string>();
            properties.Add("propkey", "propvalue");
            _nPloyConfiguration.Setup(n => n.GetProperties("NPloy.Samples.WindowsService", "dev", It.IsAny<string>())).Returns(properties);


            // Act
            _command.Run(new[] { @"NPloy.Samples.WindowsService" });

            // Assert
            _powershellRunner.Verify(
                p =>
                p.RunPowershellScript(It.IsAny<string>(), It.Is<string>(s => s.Contains(@" -propkey ""propvalue""")),
                                      It.IsAny<string>()), Times.Once());

        }

        [Test, Ignore("TODO")]
        public void ShouldCallSubstitute()
        {
            // Arrange

            // Act

            // Assert
            Assert.Fail("Not implemented");
            
        }

        [Test, Ignore("TODO")]
        public void ShouldAddAdditionalProperties()
        {
            // Arrange

            // Act

            // Assert
            Assert.Fail("Not implemented");
            
        }

        [Test]
        public void Run_ShouldCallPowershellScriptWithEnvironmentParameter()
        {
            // Arrange

            _nugetRunner.Setup(n => n.RunNuGetInstall(It.IsAny<string>(), null, null, It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(new List<string> { "NPloy.Samples.WindowsService.1.0.0.0" });

            _nPloyConfiguration.Setup(f => f.GetFiles(It.Is<string>(s => s.EndsWith(@"NPloy.Samples.WindowsService.1.0.0.0\App_Install"))))
                            .Returns(new List<string> { @"d:\NPloy.Samples.WindowsService.1.0.0.0\app_install\install.ps1" });

            // Act
            _command.Environment = "test";
            _command.Run(new[] { @"NPloy.Samples.WindowsService" });

            // Assert
            _powershellRunner.Verify(
                p =>
                p.RunPowershellScript(It.IsAny<string>(), It.Is<string>(s => s.Contains(@" -Environment ""test""")),
                                      It.IsAny<string>()), Times.Once());

        }

        [Test]
        public void Run_WhenVersion_CallNugetWithVersionParameter()
        {
            // Arrange

            _nugetRunner.Setup(
                n =>
                n.RunNuGetInstall(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                                  It.IsAny<string>())).Returns(new List<string>());
            _nPloyConfiguration.Setup(f => f.GetFiles(It.Is<string>(s => s.EndsWith(@"NPloy.Samples.WindowsService.1.0.0.0\App_Install"))))
                            .Returns(new List<string> { @"d:\NPloy.Samples.WindowsService.1.0.0.0\app_install\install.ps1" });

            // Act
            _command.Version = "1.1.1.1";
            _command.Run(new[] { @"NPloy.Samples.WindowsService" });

            // Assert
            _nugetRunner.Verify(n => n.RunNuGetInstall("NPloy.Samples.WindowsService", "1.1.1.1", null, It.IsAny<string>(), It.IsAny<string>()),Times.Once());
                        


        }
    }
}