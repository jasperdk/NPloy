using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPloy.Support;
using NUnit.Framework;

namespace NPloy.Console.UnitTests.Support
{
    [TestFixture]
    public class NPloyConfigurationTests
    {
        [Test]
        public void ShouldMethod()
        {
            // Arrange

            // Act
            var nployConfiguration = new NPloyConfiguration();
            var files = nployConfiguration.GetFiles("App_Install");

            // Assert
            Assert.That(files,Is.Empty);
        }
    }
}