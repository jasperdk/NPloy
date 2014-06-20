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

        [TestCase("propertyValueSubstitutionTag", "test_hasbeensubstituted")]
        [TestCase("propertyWithMultipleValueSubstitutionTag", "test_environment_hasbeensubstituted")]
        public void GetProperties_ShouldReturnPropertyWithSubstitutedValue(string key, string expectedValue)
        {
            // Arrange

            // Act
            var nployConfiguration = new NPloyConfiguration();
            var properties = nployConfiguration.GetProperties("package", "test", ".nploy");

            // Assert
            Assert.That(properties[key], Is.EqualTo(expectedValue));
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

        [Test]
        public void GetProperties_ShouldReturnDefaultPropertyWithSubstitutedValue()
        {
            // Arrange

            // Act
            var nployConfiguration = new NPloyConfiguration();
            var properties = nployConfiguration.GetProperties("package", "test", ".nploy");

            // Assert
            Assert.That(properties["defaultPropertyValueSubstitutionTag"], Is.EqualTo("test_testvalue"));
        }
    }
}