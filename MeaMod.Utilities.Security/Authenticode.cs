// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace MeaMod.Utilities.Security
{
    /// <summary>
    /// Helper functions for signature functionality.
    /// </summary>
    internal static class SignatureHelper
    {
        private static Guid WINTRUST_ACTION_GENERIC_VERIFY_V2 = new("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");

        /// <summary>
        /// Get signature on the specified file.
        /// </summary>
        /// <param name="fileName">Name of file to check.</param>
        /// <param name="fileContent">Content of file to check.</param>
        /// <returns>Signature object.</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown if argument fileName is empty.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if argument fileName is null
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// Thrown if the file specified by argument fileName is not found.
        /// </exception>
        internal static Signature GetSignature(string fileName, byte[] fileContent)
        {
            return GetSignatureFromWinVerifyTrust(fileName, fileContent); ;
        }

        private static Signature GetSignatureFromWinVerifyTrust(string fileName, byte[] fileContent)
        {
            if (fileContent == null)
            {
                Utils.CheckArgForNullOrEmpty(fileName, "fileName");
                SecuritySupport.CheckIfFileExists(fileName);

                // SecurityUtils.CheckIfFileSmallerThan4Bytes(fileName);
            }

            Signature signature;
            try
            {
                uint error = GetWinTrustData(fileName, fileContent, out WinTrustMethods.WINTRUST_DATA wtd);
                if (error != Win32Errors.NO_ERROR)
                {
                    System.Diagnostics.Debug.WriteLine("GetWinTrustData failed: {0:x}", error);
                }

                signature = GetSignatureFromWintrustData(fileName, error, wtd);

                wtd.dwStateAction = WinTrustAction.WTD_STATEACTION_CLOSE;
                error = WinTrustMethods.WinVerifyTrust(
                    IntPtr.Zero,
                    ref WINTRUST_ACTION_GENERIC_VERIFY_V2,
                    ref wtd);

                if (error != Win32Errors.NO_ERROR)
                {
                    System.Diagnostics.Debug.WriteLine("DestroyWinTrustDataStruct failed: {0:x}", error);
                }
            }
            catch (AccessViolationException)
            {
                signature = new Signature(fileName, Win32Errors.TRUST_E_NOSIGNATURE);
            }

            return signature;
        }

        private static uint GetWinTrustData(
            string fileName,
            byte[] fileContent,
            out WinTrustMethods.WINTRUST_DATA wtData)
        {
            wtData = new()
            {
                cbStruct = (uint)Marshal.SizeOf<WinTrustMethods.WINTRUST_DATA>(),
                dwUIChoice = WinTrustUIChoice.WTD_UI_NONE,
                dwStateAction = WinTrustAction.WTD_STATEACTION_VERIFY,
            };

            unsafe
            {
                fixed (char* fileNamePtr = fileName)
                {
                    if (fileContent == null)
                    {
                        WinTrustMethods.WINTRUST_FILE_INFO wfi = new()
                        {
                            cbStruct = (uint)Marshal.SizeOf<WinTrustMethods.WINTRUST_FILE_INFO>(),
                            pcwszFilePath = fileNamePtr,
                        };
                        wtData.dwUnionChoice = WinTrustUnionChoice.WTD_CHOICE_FILE;
                        wtData.pChoice = &wfi;

                        return WinTrustMethods.WinVerifyTrust(
                            IntPtr.Zero,
                            ref WINTRUST_ACTION_GENERIC_VERIFY_V2,
                            ref wtData);
                    }

                    fixed (byte* contentPtr = fileContent)
                    {
                        Guid pwshSIP = new("603BCC1F-4B59-4E08-B724-D2C6297EF351");
                        WinTrustMethods.WINTRUST_BLOB_INFO wbi = new()
                        {
                            cbStruct = (uint)Marshal.SizeOf<WinTrustMethods.WINTRUST_BLOB_INFO>(),
                            gSubject = pwshSIP,
                            pcwszDisplayName = fileNamePtr,
                            cbMemObject = (uint)fileContent.Length,
                            pbMemObject = contentPtr,
                        };
                        wtData.dwUnionChoice = WinTrustUnionChoice.WTD_CHOICE_BLOB;
                        wtData.pChoice = &wbi;

                        return WinTrustMethods.WinVerifyTrust(
                            IntPtr.Zero,
                            ref WINTRUST_ACTION_GENERIC_VERIFY_V2,
                            ref wtData);
                    }
                }
            }
        }

        private static X509Certificate2 GetCertFromChain(IntPtr pSigner)
        {
            try
            {
                IntPtr pCert = WinTrustMethods.WTHelperGetProvCertFromChain(pSigner, 0);
                NativeMethods.CRYPT_PROVIDER_CERT provCert =
                    Marshal.PtrToStructure<NativeMethods.CRYPT_PROVIDER_CERT>(pCert);
                return new X509Certificate2(provCert.pCert);
            }
            catch (Win32Exception)
            {
                // We don't care about the Win32 error code here, so return
                // null on a failure and let the caller handle it.
                return null;
            }
        }

        private static Signature GetSignatureFromWintrustData(
            string filePath,
            uint error,
            WinTrustMethods.WINTRUST_DATA wtd)
        {
            System.Diagnostics.Debug.WriteLine("GetSignatureFromWintrustData: error: {0}", error);

            Signature signature = null;
            if (TryGetProviderSigner(wtd.hWVTStateData, out IntPtr pProvSigner, out X509Certificate2 timestamperCert))
            {
                //
                // get cert of the signer
                //
                X509Certificate2 signerCert = GetCertFromChain(pProvSigner);

                if (signerCert != null)
                {
                    if (timestamperCert != null)
                    {
                        signature = new Signature(filePath,
                                                  error,
                                                  signerCert,
                                                  timestamperCert);
                    }
                    else
                    {
                        signature = new Signature(filePath,
                                                  error,
                                                  signerCert);
                    }

                    signature.SignatureType = SignatureType.Authenticode;
                }
            }

            //System.Diagnostics.Assert(error != 0 || signature != null, "GetSignatureFromWintrustData: general crypto failure");

            if ((signature == null) && (error != 0))
            {
                signature = new Signature(filePath, error);
            }

            return signature;
        }

        private static bool TryGetProviderSigner(IntPtr wvtStateData, out IntPtr pProvSigner, out X509Certificate2 timestamperCert)
        {
            pProvSigner = IntPtr.Zero;
            timestamperCert = null;

            try
            {
                IntPtr pProvData = WinTrustMethods.WTHelperProvDataFromStateData(wvtStateData);

                pProvSigner = WinTrustMethods.WTHelperGetProvSignerFromChain(
                    pProvData,
                    signerIdx: 0,
                    counterSigner: false,
                    counterSignerIdx: 0);

                NativeMethods.CRYPT_PROVIDER_SGNR provSigner =
                    Marshal.PtrToStructure<NativeMethods.CRYPT_PROVIDER_SGNR>(pProvSigner);
                if (provSigner.csCounterSigners == 1)
                {
                    //
                    // time stamper cert available
                    //
                    timestamperCert = GetCertFromChain(provSigner.pasCounterSigners);
                }

                return true;
            }
            catch (Win32Exception)
            {
                return false;
            }
        }
    }
}
