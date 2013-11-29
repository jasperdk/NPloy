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
            Assert.That(properties.Count, Is.EqualTo(1));
            Assert.That(properties["property"], Is.EqualTo("package"));
        }
    }
}