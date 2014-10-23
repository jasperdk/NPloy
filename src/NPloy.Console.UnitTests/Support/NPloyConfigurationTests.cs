using System.Linq;
using NPloy.Support;
using NUnit.Framework;

namespace NPloy.Console.UnitTests.Support
{
    [TestFixture]
    public class NPloyConfigurationTests
    {
        private NPloyConfiguration _nployConfiguration;

        [SetUp]
        public void SetUp()
        {
            _nployConfiguration = new NPloyConfiguration();
        }

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
            var properties = _nployConfiguration.GetProperties("package","test",".nploy");

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
            var properties = _nployConfiguration.GetProperties("package", "test", ".nploy");

            // Assert
            Assert.That(properties[property], Is.EqualTo(value));
        }

        [TestCase("propertyValueSubstitutionTag", "test_hasbeensubstituted")]
        [TestCase("propertyWithMultipleValueSubstitutionTag", "test_environment_hasbeensubstituted")]
        public void SubstituteValues_ShouldSubstituteValue(string key, string expectedValue)
        {
            // Arrange
            var properties = _nployConfiguration.GetProperties("package", "test", ".nploy");

            // Act
            _nployConfiguration.SubstituteValues(properties);

            // Assert
            Assert.That(properties[key], Is.EqualTo(expectedValue));
        }

        [Test]
        public void SubstituteValues_ShouldSubstituteDoubleSubstitutedValue()
        {
            // Arrange
            var properties = _nployConfiguration.GetProperties("package", "test", ".nploy");

            // Act
            _nployConfiguration.SubstituteValues(properties);

            // Assert
            Assert.That(properties["propertyValueDoubleSubstitutionLoop"], Is.EqualTo("test_test_hasbeensubstituted"));
        }

        [Test]
        public void SubstituteValues_ShouldSubstituteDefaultPropertyWithSubstitutedValue()
        {
            // Arrange
            var properties = _nployConfiguration.GetProperties("package", "test", ".nploy");

            // Act
            _nployConfiguration.SubstituteValues(properties);
            
            // Assert
            Assert.That(properties["defaultPropertyValueSubstitutionTag"], Is.EqualTo("test_testvalue"));
        }

        [Test]
        public void GetRoleConfig_ShouldParsePackage()
        {
            // Arrange
            
            // Act
            var roleConfig = _nployConfiguration.GetRoleConfig(@".nploy\roles\Test.role");

            // Assert
            Assert.That(roleConfig.Packages.Select(p => p.Id), Is.EqualTo(new[] { "NPloy.Samples.Web" }));
        }

        [TestCase("NPloy.Samples.Web.1.0.0.44", "NPloy.Samples.Web", "1.0.0.44")]
        [TestCase("NPloy.Samples.Web.1.0.0.44-prerelease", "NPloy.Samples.Web", "1.0.0.44-prerelease")]
        public void GetPackageConfig_ShouldParsePackageVersion(string packageVersion,string expectedId, string expectedVersion)
        {
            // Arrange
            
            // Act
            var packageConfig = NPloyConfiguration.GetPackageConfig(packageVersion);

            // Assert
            Assert.That(packageConfig.Id, Is.EqualTo(expectedId));
            Assert.That(packageConfig.Version, Is.EqualTo(expectedVersion));
            Assert.That(packageConfig.FullName, Is.EqualTo(packageVersion));
        }
    }
}