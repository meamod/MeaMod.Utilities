using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using MmAuthorityKeyIdentifier = MeaMod.Utilities.Cryptography.X509AuthorityKeyIdentifierExtension;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class X509AuthorityKeyIdentifierExtensionMoreTests
    {
        private static X509Certificate2 CreateWithRawSubjectKeyIdentifier(byte[] rawData)
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest("CN=MeaMod Bad SKI", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(new X509Extension(new Oid("2.5.29.14"), rawData, false));
            return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        }

        [TestMethod]
        public void MalformedSubjectKeyIdentifierIsRejected()
        {
            // An ASN.1 NULL rather than an OCTET STRING.
            using var certificate = CreateWithRawSubjectKeyIdentifier(new byte[] { 0x05, 0x00 });

            Assert.ThrowsExactly<ArgumentException>(() => new MmAuthorityKeyIdentifier(certificate, false));
        }

        [TestMethod]
        public void SubjectKeyIdentifierTooLongForShortFormLengthIsRejected()
        {
            // 04 7E + 126 bytes is a valid OCTET STRING, but the wrapping SEQUENCE would need a long-form length.
            var rawData = new byte[128];
            rawData[0] = 0x04;
            rawData[1] = 0x7E;
            using var certificate = CreateWithRawSubjectKeyIdentifier(rawData);

            Assert.ThrowsExactly<ArgumentException>(() => new MmAuthorityKeyIdentifier(certificate, false));
        }

        [TestMethod]
        public void ShortSubjectKeyIdentifierIsEncodedWithItsRealLength()
        {
            var rawData = new byte[] { 0x04, 0x04, 0xDE, 0xAD, 0xBE, 0xEF };
            using var certificate = CreateWithRawSubjectKeyIdentifier(rawData);

            var extension = new MmAuthorityKeyIdentifier(certificate, true);

            CollectionAssert.AreEqual(new byte[] { 0x30, 0x06, 0x80, 0x04, 0xDE, 0xAD, 0xBE, 0xEF }, extension.RawData);
            Assert.IsTrue(extension.Critical);
        }
    }
}
