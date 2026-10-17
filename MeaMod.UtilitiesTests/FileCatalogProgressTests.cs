using System.Collections.ObjectModel;
using System.Reflection;
using MeaMod.Utilities.Security;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class FileCatalogProgressTests
    {
        private static readonly string TestFiles = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "testfiles");

        [TestMethod]
        public void ValidateAcceptsANullProgressCallback()
        {
            // The progress callback used to be dereferenced without a null check.
            var paths = new Collection<string> { Fixtures.Require("vlc") };

            CatalogInformation info = FileCatalog.Validate(paths, Fixtures.Require("vlc_valid.cat"), null!);

            Assert.IsNotNull(info);
            Assert.AreEqual(CatalogValidationStatus.Valid, info.Status);
        }
    }
}
