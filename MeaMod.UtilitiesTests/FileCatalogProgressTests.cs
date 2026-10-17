using System.Collections.ObjectModel;
using System.Reflection;
using MeaMod.Utilities.Security;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class FileCatalogProgressTests
    {
        private static readonly string TestFiles = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "testfiles");

        [TestMethod]
        public void ValidateAcceptsANullProgressCallback()
        {
            // The progress callback used to be dereferenced without a null check.
            var paths = new Collection<string> { Path.Combine(TestFiles, "vlc") };

            CatalogInformation info = FileCatalog.Validate(paths, Path.Combine(TestFiles, "vlc_valid.cat"), null!);

            Assert.IsNotNull(info);
            Assert.AreEqual(CatalogValidationStatus.Valid, info.Status);
        }
    }
}
