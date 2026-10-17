using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using MmAuthorityKeyIdentifier = MeaMod.Utilities.Cryptography.X509AuthorityKeyIdentifierExtension;
using BclAuthorityKeyIdentifier = System.Security.Cryptography.X509Certificates.X509AuthorityKeyIdentifierExtension;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class X509AuthorityKeyIdentifierExtensionTests
    {
        private static X509Certificate2 CreateSelfSigned(bool withSubjectKeyIdentifier)
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest("CN=MeaMod Test CA", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            if (withSubjectKeyIdentifier)
                request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
            return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        }

        [TestMethod]
        public void EncodingMatchesTheBuiltInImplementation()
        {
            using var certificate = CreateSelfSigned(withSubjectKeyIdentifier: true);
            var subjectKeyIdentifier = certificate.Extensions.OfType<X509SubjectKeyIdentifierExtension>().Single();

            var ours = new MmAuthorityKeyIdentifier(certificate, false);
            var expected = BclAuthorityKeyIdentifier.CreateFromSubjectKeyIdentifier(subjectKeyIdentifier);

            // The previous implementation copied from offset 0 of the SKI raw data, so the key id was wrong.
            CollectionAssert.AreEqual(expected.RawData, ours.RawData, Convert.ToHexString(ours.RawData));
            Assert.AreEqual("2.5.29.35", ours.Oid?.Value);
        }

        [TestMethod]
        public void KeyIdentifierIsTheSubjectKeyIdentifierOfTheIssuer()
        {
            using var certificate = CreateSelfSigned(withSubjectKeyIdentifier: true);
            var subjectKeyIdentifier = certificate.Extensions.OfType<X509SubjectKeyIdentifierExtension>().Single();

            var ours = new MmAuthorityKeyIdentifier(certificate, false);
            var decoded = new BclAuthorityKeyIdentifier(ours.RawData, false);

            Assert.IsTrue(decoded.KeyIdentifier.HasValue);
            Assert.AreEqual(subjectKeyIdentifier.SubjectKeyIdentifier, Convert.ToHexString(decoded.KeyIdentifier!.Value.Span));
        }

        [TestMethod]
        public void MissingSubjectKeyIdentifierThrowsArgumentException()
        {
            using var certificate = CreateSelfSigned(withSubjectKeyIdentifier: false);

            Assert.ThrowsExactly<ArgumentException>(() => new MmAuthorityKeyIdentifier(certificate, false));
        }

        [TestMethod]
        public void NullCertificateThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new MmAuthorityKeyIdentifier(null!, false));
        }
    }
}
