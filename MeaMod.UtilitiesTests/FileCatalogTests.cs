using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Diagnostics;
using MeaMod.Utilities.Security;

namespace MeaMod.Utilities.Tests
{
    [TestClass()]
    [OSCondition(OperatingSystems.Windows)]
    public class FileCatalogTests
    {
        private static readonly string Path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "testfiles");
        private static readonly string CatalogFolder = System.IO.Path.Combine(Path, "vlc");
        private static readonly Collection<string> Paths =
        [
            CatalogFolder
        ];
        private static readonly Progress<CatalogProgress> ProgressDownloadIndicator = new(ValidateProgress);

        public static void ValidateProgress(CatalogProgress obj)
        {
            Debug.WriteLine(obj.Stage);
            Debug.WriteLine(obj.HashAlgorithm);
            Debug.WriteLine(obj.CurrentItem);
            Debug.WriteLine(obj.Processed + @" / " + obj.Total);
        }

        [TestMethod()]
        public void ValidateTestValidUnsigned()
        {
            var catalogFile = System.IO.Path.Combine(Path, "vlc_valid.cat");
            CatalogInformation catalogInfo = FileCatalog.Validate(Paths, catalogFile, ProgressDownloadIndicator);
            Assert.IsNotNull(catalogInfo);
            Assert.AreEqual(SignatureStatus.NotSigned, catalogInfo.Signature.Status);
            Assert.AreEqual(CatalogValidationStatus.Valid, catalogInfo.Status);
        }
        [TestMethod()]
        public void ValidateTestValidSigned()
        {
            var catalogFile = System.IO.Path.Combine(Path, "vlc_valid_signed.cat");
            CatalogInformation catalogInfo = FileCatalog.Validate(Paths, catalogFile, ProgressDownloadIndicator);
            Assert.IsNotNull(catalogInfo);
            Assert.AreEqual(SignatureStatus.Valid, catalogInfo.Signature.Status);
            Assert.AreEqual(CatalogValidationStatus.Valid, catalogInfo.Status);
        }

        [TestMethod()]
        public void ValidateTestInvalidSigned()
        {
            var catalogFile = System.IO.Path.Combine(Path, "vlc_invalid_signed.cat");
            CatalogInformation catalogInfo = FileCatalog.Validate(Paths, catalogFile, ProgressDownloadIndicator);
            Assert.IsNotNull(catalogInfo);
            Assert.AreEqual(SignatureStatus.Valid, catalogInfo.Signature.Status);
            Assert.AreEqual(CatalogValidationStatus.ValidationFailed, catalogInfo.Status);
        }

        [TestMethod()]
        public void ValidateTestInvalidSignedInvalid()
        {
            var catalogFile = System.IO.Path.Combine(Path, "vlc_invalid_signed_invalid.cat");
            CatalogInformation catalogInfo = FileCatalog.Validate(Paths, catalogFile, ProgressDownloadIndicator);
            Assert.IsNotNull(catalogInfo);
            Assert.AreEqual(SignatureStatus.NotSigned, catalogInfo.Signature.Status);
            Assert.AreEqual(CatalogValidationStatus.ValidationFailed, catalogInfo.Status);
        }

        [TestMethod()]
        public void ValidateTestInvalidUnsigned()
        {
            var catalogFile = System.IO.Path.Combine(Path, "vlc_invalid.cat");
            CatalogInformation catalogInfo = FileCatalog.Validate(Paths, catalogFile, ProgressDownloadIndicator);
            Assert.IsNotNull(catalogInfo);
            Assert.AreEqual(SignatureStatus.NotSigned, catalogInfo.Signature.Status);
            Assert.AreEqual(CatalogValidationStatus.ValidationFailed, catalogInfo.Status);
        }
    }
}
