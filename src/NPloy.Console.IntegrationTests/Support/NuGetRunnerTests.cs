using System.IO;
using NPloy.Support;
using NUnit.Framework;

namespace NPloy.Console.IntegrationTests.Support
{
    [TestFixture]
    public class NuGetRunnerTests
    {
        [SetUp]
        public void SetUp()
        {
            var path = Directory.GetCurrentDirectory();
            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                Directory.Delete(directory, true);
            }
        }

        [Test]
        public void RunNuGetInstall_ShouldReturnInstalledPackages()
        {
            // Arrange
            var nuGetRunner = new NuGetRunner();

            // Act
            var result = nuGetRunner.RunNuGetInstall("FluentMigrator.Tools", null, null, Directory.GetCurrentDirectory());


            // Assert
            Assert.That(result.Count,Is.EqualTo(2));
            Assert.That(result[0], Is.EqualTo("FluentMigrator.1.1.1.0"));
            Assert.That(result[1], Is.EqualTo("FluentMigrator.Tools.1.1.1.0"));
        }
    }
}