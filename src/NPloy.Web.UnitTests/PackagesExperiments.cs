using System.Linq;
using NUnit.Framework;
using NuGet.Server.DataServices;

namespace NPloy.Web.UnitTests
{
    [TestFixture]
    public class PackagesExperiments
    {
        [Test]
        public void ShouldMethod()
        {
            // Arrange
            var packages = new Packages();
            
            // Act
            var search = packages.Search("", "", false);

            // Assert
            Assert.That(search.Count(),Is.EqualTo(2));
        }
    }
}