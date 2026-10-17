using System.Collections.ObjectModel;
using System.Reflection;
using MeaMod.Utilities.Security;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class SignatureHelperMoreTests
    {
        private static readonly string TestFiles = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "testfiles");

        [TestMethod]
        public void EmbeddedSignatureWithATimestampExposesTheTimestamper()
        {
            string dotnet = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles") ?? @"C:\Program Files", "dotnet", "dotnet.exe");
            if (!File.Exists(dotnet))
                Assert.Inconclusive("dotnet.exe not found");

            Signature signature = SignatureHelper.GetSignature(dotnet, null!);

            if (signature.Status != SignatureStatus.Valid)
                Assert.Inconclusive("dotnet.exe signature status is " + signature.Status);
            Assert.AreEqual(SignatureType.Authenticode, signature.SignatureType);
            Assert.IsNotNull(signature.SignerCertificate);
            Assert.IsNotNull(signature.TimeStamperCertificate, "dotnet.exe is timestamped by Microsoft");
        }

        [TestMethod]
        public void CatalogSignedSystemFileVerifiesThroughTheCatalogDatabase()
        {
            // kernel32.dll has no embedded signature; WinVerifyTrust resolves it through the system catalogs.
            string kernel32 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "kernel32.dll");

            Signature signature = SignatureHelper.GetSignature(kernel32, null!);

            Assert.AreEqual(SignatureStatus.Valid, signature.Status);
            Assert.IsNotNull(signature.SignerCertificate);
        }

        [TestMethod]
        public void SelfSignedFileWithoutATimestampHasASignerButNoTimestamper()
        {
            string path = Fixtures.Require("selfsigned_notimestamp.ps1");

            Signature signature = SignatureHelper.GetSignature(path, null!);

            Assert.AreNotEqual(SignatureStatus.NotSigned, signature.Status);
            Assert.IsNotNull(signature.SignerCertificate);
            StringAssert.Contains(signature.SignerCertificate.Subject, "MeaMod Utilities Test Signing");
            Assert.IsNull(signature.TimeStamperCertificate);
            Assert.AreEqual(SignatureType.Authenticode, signature.SignatureType);
            // The throw-away root is not trusted, so the simpler trust check must say no.
            Assert.IsFalse(AuthenticodeTools.IsTrusted(path));
        }

        [TestMethod]
        public void CatalogWithoutFilePathAttributesIsRejected()
        {
            // Driver catalogs under CatRoot record members by hash with "File"/"OSAttr" attributes, never "FilePath".
            string catRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "CatRoot");
            string? catalog = null;
            try
            {
                catalog = Directory.Exists(catRoot)
                    ? Directory.EnumerateFiles(catRoot, "*.cat", SearchOption.AllDirectories).FirstOrDefault()
                    : null;
            }
            catch (UnauthorizedAccessException)
            {
            }

            if (catalog == null)
                Assert.Inconclusive("no readable catalog found under CatRoot");

            Assert.ThrowsExactly<InvalidOperationException>(() => FileCatalog.GetHashesFromCatalog(catalog, out _));
        }

        [TestMethod]
        public void ZeroLengthFilesAreRecordedAsCatalogAttributes()
        {
            // New-FileCatalog stores empty files as catalog-level attributes with an empty hash.
            var paths = new Collection<string> { Fixtures.Require("zerofile") };

            CatalogInformation info = FileCatalog.Validate(paths, Fixtures.Require("zerofile_valid.cat"), null!);

            Assert.AreEqual(CatalogValidationStatus.Valid, info.Status);
            Assert.AreEqual(string.Empty, info.CatalogItems["empty.txt"]);
            Assert.AreEqual(string.Empty, info.PathItems["empty.txt"]);
            Assert.IsFalse(string.IsNullOrEmpty(info.CatalogItems["hello.txt"]));
            Assert.AreEqual(info.CatalogItems["hello.txt"], info.PathItems["hello.txt"]);
        }
    }
}
