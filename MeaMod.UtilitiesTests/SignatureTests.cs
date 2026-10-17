using System.Collections.ObjectModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MeaMod.Utilities.Security;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class SignatureTests
    {
        private static readonly string TestFiles = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "testfiles");

        private static X509Certificate2 CreateCertificate(string subject)
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        }

        [TestMethod]
        public void StatusIsDerivedFromTheWin32Error()
        {
            var expectations = new (uint Error, SignatureStatus Status)[]
            {
                (Win32Errors.NO_ERROR, SignatureStatus.Valid),
                (Win32Errors.NTE_BAD_ALGID, SignatureStatus.Incompatible),
                (Win32Errors.TRUST_E_NOSIGNATURE, SignatureStatus.NotSigned),
                (Win32Errors.TRUST_E_BAD_DIGEST, SignatureStatus.HashMismatch),
                (Win32Errors.CRYPT_E_BAD_MSG, SignatureStatus.HashMismatch),
                (Win32Errors.TRUST_E_PROVIDER_UNKNOWN, SignatureStatus.NotSupportedFileFormat),
                (Win32Errors.TRUST_E_EXPLICIT_DISTRUST, SignatureStatus.NotTrusted),
                (Win32Errors.CERT_E_UNTRUSTEDROOT, SignatureStatus.UnknownError),
                (Win32Errors.E_FAIL, SignatureStatus.UnknownError),
            };

            foreach (var (error, status) in expectations)
            {
                var signature = new Signature(@"C:\somewhere\script.ps1", error);

                Assert.AreEqual(status, signature.Status, error.ToString("X8"));
                Assert.IsFalse(string.IsNullOrWhiteSpace(signature.StatusMessage), error.ToString("X8"));
                Assert.AreEqual(@"C:\somewhere\script.ps1", signature.Path);
                Assert.AreEqual(SignatureType.None, signature.SignatureType);
                Assert.IsFalse(signature.IsOSBinary);
                Assert.IsNull(signature.SignerCertificate);
                Assert.IsNull(signature.TimeStamperCertificate);
            }
        }

        [TestMethod]
        public void UnsupportedFormatMessageCopesWithAMissingExtension()
        {
            var withExtension = new Signature(@"C:\somewhere\file.xyz", Win32Errors.TRUST_E_PROVIDER_UNKNOWN);
            var withoutExtension = new Signature(@"C:\somewhere\file", Win32Errors.TRUST_E_PROVIDER_UNKNOWN);

            StringAssert.Contains(withExtension.StatusMessage, ".xyz");
            Assert.IsFalse(string.IsNullOrWhiteSpace(withoutExtension.StatusMessage));
            Assert.AreNotEqual(withExtension.StatusMessage, withoutExtension.StatusMessage);
        }

        [TestMethod]
        public void SignerAndTimestamperConstructors()
        {
            using var signer = CreateCertificate("CN=Signer");
            using var timestamper = CreateCertificate("CN=Timestamper");

            var valid = new Signature("file", signer);
            Assert.AreEqual(SignatureStatus.Valid, valid.Status);
            Assert.AreSame(signer, valid.SignerCertificate);
            Assert.IsNull(valid.TimeStamperCertificate);

            var withError = new Signature("file", Win32Errors.TRUST_E_BAD_DIGEST, signer);
            Assert.AreEqual(SignatureStatus.HashMismatch, withError.Status);
            Assert.AreSame(signer, withError.SignerCertificate);

            var timestamped = new Signature("file", Win32Errors.NO_ERROR, signer, timestamper);
            Assert.AreEqual(SignatureStatus.Valid, timestamped.Status);
            Assert.AreSame(timestamper, timestamped.TimeStamperCertificate);

            timestamped.SignatureType = SignatureType.Catalog;
            timestamped.IsOSBinary = true;
            Assert.AreEqual(SignatureType.Catalog, timestamped.SignatureType);
            Assert.IsTrue(timestamped.IsOSBinary);
        }

        [TestMethod]
        public void SignedCatalogExposesTheSignerCertificate()
        {
            var paths = new Collection<string> { Fixtures.Require("vlc") };

            CatalogInformation info = FileCatalog.Validate(paths, Fixtures.Require("vlc_valid_signed.cat"), null!);

            Assert.AreEqual(SignatureStatus.Valid, info.Signature.Status);
            Assert.AreEqual(SignatureType.Authenticode, info.Signature.SignatureType);
            Assert.IsNotNull(info.Signature.SignerCertificate);
            Assert.IsFalse(string.IsNullOrWhiteSpace(info.Signature.SignerCertificate.Subject));
            Assert.IsFalse(string.IsNullOrWhiteSpace(info.Signature.StatusMessage));
            Assert.AreEqual("SHA256", info.HashAlgorithm);
            Assert.IsTrue(info.CatalogItems.Count > 0);
        }

        [TestMethod]
        public void SignatureHelperValidatesItsArguments()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => SignatureHelper.GetSignature(null!, null!));
            Assert.ThrowsExactly<ArgumentNullException>(() => SignatureHelper.GetSignature(string.Empty, null!));
            Assert.ThrowsExactly<FileNotFoundException>(() => SignatureHelper.GetSignature(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".cat"), null!));
        }

        [TestMethod]
        public void SignatureHelperReportsAnUnsignedFile()
        {
            string path = Path.Combine(Path.GetTempPath(), "MeaMod.Utilities.Tests." + Guid.NewGuid().ToString("N") + ".ps1");
            File.WriteAllText(path, "Write-Host 'hello'");
            try
            {
                Signature signature = SignatureHelper.GetSignature(path, null!);

                Assert.AreEqual(SignatureStatus.NotSigned, signature.Status);
                Assert.AreEqual(path, signature.Path);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void SignatureHelperCanVerifyInMemoryContent()
        {
            // Content is verified through the PowerShell SIP as a blob rather than a file on disk.
            Signature signature = SignatureHelper.GetSignature("inline.ps1", Encoding.UTF8.GetBytes("Write-Host 'hello'"));

            Assert.IsNotNull(signature);
            Assert.AreNotEqual(SignatureStatus.Valid, signature.Status);
            Assert.AreEqual("inline.ps1", signature.Path);
        }

        [TestMethod]
        public void EnumValuesAreStable()
        {
            Assert.AreEqual(0, (int)SignatureType.None);
            Assert.AreEqual(1, (int)SignatureType.Authenticode);
            Assert.AreEqual(2, (int)SignatureType.Catalog);
            Assert.AreEqual(0, (int)CatalogValidationStatus.Valid);
            Assert.AreEqual(1, (int)CatalogValidationStatus.ValidationFailed);
        }
    }
}
