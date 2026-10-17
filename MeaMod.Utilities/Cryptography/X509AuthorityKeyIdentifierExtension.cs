using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Linq;

namespace MeaMod.Utilities.Cryptography
{
    /// <summary>
    /// Add X509AuthorityKeyIdentifierExtension to .net standard 2.0
    /// <para><see href="https://shawinnes.com/dotnet-x509-extensions/"/></para>
    /// </summary>
    public class X509AuthorityKeyIdentifierExtension : X509Extension
    {
        private static Oid AuthorityKeyIdentifierOid => new Oid("2.5.29.35");
        private static Oid SubjectKeyIdentifierOid => new Oid("2.5.29.14");

        /// <summary>
        /// Initializes a new instance of the X509AuthorityKeyIdentifierExtension class using a X509Certificate2 and a value indicating whether the extension is critical.
        /// </summary>
        /// <param name="certificateAuthority">X509Certificate2</param>
        /// <param name="critical">Boolean</param>
        /// <exception cref="ArgumentNullException"><paramref name="certificateAuthority"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="certificateAuthority"/> has no Subject Key Identifier extension, or it is not in the expected form.</exception>
        public X509AuthorityKeyIdentifierExtension(X509Certificate2 certificateAuthority, bool critical)
            : base(AuthorityKeyIdentifierOid, EncodeExtension(certificateAuthority), critical)
        {
        }

        private static byte[] EncodeExtension(X509Certificate2 certificateAuthority)
        {
            if (certificateAuthority == null)
                throw new ArgumentNullException(nameof(certificateAuthority));

            var subjectKeyIdentifier = certificateAuthority.Extensions.Cast<X509Extension>().FirstOrDefault(p => p.Oid?.Value == SubjectKeyIdentifierOid.Value);
            if (subjectKeyIdentifier == null)
                throw new ArgumentException("The certificate does not contain a Subject Key Identifier extension.", nameof(certificateAuthority));

            // SubjectKeyIdentifier ::= KeyIdentifier (OCTET STRING), i.e. 04 <len> <keyId>
            var rawData = subjectKeyIdentifier.RawData;
            if (rawData == null || rawData.Length < 2 || rawData[0] != 0x04 || rawData[1] >= 0x80 || rawData[1] != rawData.Length - 2)
                throw new ArgumentException("The Subject Key Identifier extension is not in the expected form.", nameof(certificateAuthority));

            int keyIdLength = rawData.Length - 2;
            if (keyIdLength + 2 >= 0x80)
                throw new ArgumentException("The Subject Key Identifier is too long to encode.", nameof(certificateAuthority));

            // AuthorityKeyIdentifier ::= SEQUENCE { keyIdentifier [0] IMPLICIT KeyIdentifier }
            var authorityKeyIdentifier = new byte[keyIdLength + 4];
            authorityKeyIdentifier[0] = 0x30;                       // SEQUENCE
            authorityKeyIdentifier[1] = (byte)(keyIdLength + 2);    // length of the sequence contents
            authorityKeyIdentifier[2] = 0x80;                       // [0] IMPLICIT keyIdentifier
            authorityKeyIdentifier[3] = (byte)keyIdLength;          // length of the key identifier
            // Copy from offset 2 of the SKI raw data (skipping the OCTET STRING tag and length),
            // not from offset 0 of the underlying array.
            Array.Copy(rawData, 2, authorityKeyIdentifier, 4, keyIdLength);
            return authorityKeyIdentifier;
        }
    }
}
