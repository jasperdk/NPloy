﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Moq;
using NPloy.Commands;
using NPloy.Support;
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
        private Mock<INPloyConfiguration> _nPloyConfiguration;

        [SetUp]
        public void SetUp()
        {
            _nPloyConfiguration = new Mock<INPloyConfiguration>();
            _nPloyConfiguration.Setup(c => c.FileExists(It.Is<string>(s => s.EndsWith("test.node")))).Returns(true);
            var nodeDocument = CreateNodeFileForEnvironment("test.node", "test", "test1.role");
            _nPloyConfiguration.Setup(c => c.GetNodeXml(It.Is<string>(s => s.EndsWith("test.node")))).Returns(nodeDocument);
            _commandFactory = new Mock<ICommandFactory>();
            _command = new InstallNodeCommand(_nPloyConfiguration.Object, _commandFactory.Object);

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
            var nodeDocument = CreateNodeFileForEnvironment("test.node", "test", "test1.role");
            _nPloyConfiguration.Setup(c => c.GetNodeXml(It.Is<string>(s => s.EndsWith("test.node")))).Returns(nodeDocument);

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
            var nodeDocument = CreateNodeFileForEnvironment("test.node", "test", "test1.role");
            _nPloyConfiguration.Setup(c => c.GetNodeXml(It.Is<string>(s => s.EndsWith("test.node")))).Returns(nodeDocument);

            // Act
            _command.Run(new[] { "test" });

            // Assert
            _installRoleCommandMock.Verify(c => c.Run(new[] { "test1.role" }), Times.Once());
        }

        [Test]
        public void Run_ShouldPassOnEnvironment()
        {
            // Arrange
            var nodeDocument = CreateNodeFileForEnvironment("test.node", "test", "test1.role");
            _nPloyConfiguration.Setup(c => c.GetNodeXml(It.Is<string>(s => s.EndsWith("test.node")))).Returns(nodeDocument);

            // Act
            _command.Run(new[] { "test.node" });

            // Assert
            _installRoleCommandMock.VerifySet(c => c.Environment = "test", Times.Once());
        }


        [Test]
        public void Run_WhenArgumentIsNodeFile_ShouldInstallAllRoles()
        {
            // Arrange
            var nodeDocument = CreateNodeFileForEnvironment("test.node","test", "test1.role", "test2.role");
            _nPloyConfiguration.Setup(c => c.GetNodeXml(It.Is<string>(s => s.EndsWith("test.node")))).Returns(nodeDocument);

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
            _nPloyConfiguration.Setup(c => c.FileExists(It.Is<string>(s => s.EndsWith(InstallNodeCommand.PackageFileName)))).Returns(true);
            
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
            
            // Act
            _command.PackageSources = "packagesources";
            _command.NuGetPath = "nugetpath";
            _command.Properties = "properties";
            _command.IncludePrerelease = true;
            _command.Run(new[] { @"test.node" });

            // Assert
            _installRoleCommandMock.VerifySet(c => c.PackageSources = "packagesources", Times.Once());
            _installRoleCommandMock.VerifySet(c => c.AutoStart = true, Times.Never);
            _installRoleCommandMock.VerifySet(c => c.NuGetPath = "nugetpath", Times.Once());
            _installRoleCommandMock.VerifySet(c => c.ConfigurationDirectory = "", Times.Once());
            _installRoleCommandMock.VerifySet(c => c.Properties = "properties", Times.Once());
            _installRoleCommandMock.VerifySet(c => c.IncludePrerelease = true, Times.Once());
        }

        [Test]
        public void Run_WhenNPloyConfigIsInOtherFolder_ShouldSetConfigurationProperty()
        {
            // Arrange
            
            // Act
            _command.Run(new[] { @".nploy\test.node" });

            // Assert
            _installRoleCommandMock.VerifySet(c => c.ConfigurationDirectory = ".nploy", Times.Once());
        }

        private static XDocument CreateNodeFile(string node, params string[] roles)
        {
            return CreateNodeFileForEnvironment(node, "dev", roles);
        }

        private static XDocument CreateNodeFileForEnvironment(string node, string enviroment, params string[] roles)
        {
            var roleElements = roles.Select(role => new XElement("role", new XAttribute("name", role))).ToList();

            var element = new XElement("node",
                new XAttribute("environment", enviroment),
                new XElement("roles",roleElements)
                );

            var doc = new XDocument(element);
            return doc;
        }
    }
}