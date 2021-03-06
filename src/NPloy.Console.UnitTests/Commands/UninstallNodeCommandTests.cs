﻿using Moq;
using NPloy.Commands;
using NPloy.Support;
using NUnit.Framework;

namespace NPloy.Console.UnitTests.Commands
{
    [TestFixture]
    public class UninstallNodeCommandTests
    {
        private UninstallNodeCommand _command;
        private Mock<INPloyConfiguration> _nPloyConfiguration;

        [SetUp]
        public void SetUp()
        {
            _nPloyConfiguration = new Mock<INPloyConfiguration>();
            _command = new UninstallNodeCommand(_nPloyConfiguration.Object);
        }

        [Test]
        public void Run_ShouldCallStopPackages()
        {
            // Arrange
            _nPloyConfiguration.Setup(n => n.HasInstalledPackages(@"d:\")).Returns(true);
            _nPloyConfiguration.Setup(n => n.GetInstalledPackges(@"d:\"))
                               .Returns(new[] {new PackageConfig{Id= "NPloy.Samples.WindowsService",Version = "1.0.0.0"}});

            // Act
            _command.Run(new []{@"d:\"});

            // Assert
            Assert.True(true,"TODO");
        }
    }
}