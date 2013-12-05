﻿using NPloy.Support;
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

        [Test]
        public void GetProperties_ShouldReturnMostSpecificProperty()
        {
            // Arrange

            // Act
            var nployConfiguration = new NPloyConfiguration();
            var properties = nployConfiguration.GetProperties("package","test",".nploy");

            // Assert
            Assert.That(properties.Count, Is.GreaterThan(1));
            Assert.That(properties["property"], Is.EqualTo("environmentpackage"));
        }

        [TestCase("package","package")]
        [TestCase("default", "default")]
        [TestCase("defaultpackage", "defaultpackage")]
        [TestCase("environment", "environment")]
        public void GetProperties_ShouldReturnProperty(string property, string value)
        {
            // Arrange

            // Act
            var nployConfiguration = new NPloyConfiguration();
            var properties = nployConfiguration.GetProperties("package", "test", ".nploy");

            // Assert
            Assert.That(properties[property], Is.EqualTo(value));
        }

        [Test]
        public void GetProperties_ShouldReturnPropertyWithSubstitutedValue()
        {
            // Arrange

            // Act
            var nployConfiguration = new NPloyConfiguration();
            var properties = nployConfiguration.GetProperties("package", "test", ".nploy");

            // Assert
            Assert.That(properties["propertyValueSubstitutionTag"], Is.EqualTo("test_hasbeensubstituted"));
        }

        [Test]
        public void GetProperties_ShouldReturnPropertyWithDoubleSubstitutedValue()
        {
            // Arrange

            // Act
            var nployConfiguration = new NPloyConfiguration();
            var properties = nployConfiguration.GetProperties("package", "test", ".nploy");

            // Assert
            Assert.That(properties["propertyValueDoubleSubstitutionLoop"], Is.EqualTo("test_test_hasbeensubstituted"));
        }
    }
}