// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;
using DWORD = uint;
using BOOL = uint;

namespace MeaMod.Utilities.Security
{
    /// <summary>
    /// Pinvoke methods from crypt32.dll.
    /// </summary>
    internal static partial class NativeMethods
    {
        // -------------------------------------------------------------------
        // crypt32.dll stuff
        //

        [StructLayout(LayoutKind.Sequential)]
        internal struct CRYPT_PROVIDER_CERT
        {
#pragma warning disable IDE0044
            private DWORD _cbStruct;
#pragma warning restore IDE0044
            internal IntPtr pCert; // PCCERT_CONTEXT
            private readonly BOOL _fCommercial;
            private readonly BOOL _fTrustedRoot;
            private readonly BOOL _fSelfSigned;
            private readonly BOOL _fTestCert;
            private readonly DWORD _dwRevokedReason;
            private readonly DWORD _dwConfidence;
            private readonly DWORD _dwError;
            private readonly IntPtr _pTrustListContext; // CTL_CONTEXT*
            private readonly BOOL _fTrustListSignerCert;
            private readonly IntPtr _pCtlContext; // PCCTL_CONTEXT
            private readonly DWORD _dwCtlError;
            private readonly BOOL _fIsCyclic;
            private readonly IntPtr _pChainElement; // PCERT_CHAIN_ELEMENT
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CRYPT_PROVIDER_SGNR
        {
            private readonly DWORD _cbStruct;
            private FILETIME _sftVerifyAsOf;
            private readonly DWORD _csCertChain;
            private readonly IntPtr _pasCertChain; // CRYPT_PROVIDER_CERT*
            private readonly DWORD _dwSignerType;
            private readonly IntPtr _psSigner; // CMSG_SIGNER_INFO*
            private readonly DWORD _dwError;
            internal DWORD csCounterSigners;
            internal IntPtr pasCounterSigners; // CRYPT_PROVIDER_SGNR*
            private readonly IntPtr _pChainContext; // PCCERT_CHAIN_CONTEXT
        }

        //
        // stuff required for getting cert extensions
        //

        [StructLayout(LayoutKind.Sequential)]
        internal struct FILETIME
        {
            /// DWORD->unsigned int
            internal uint dwLowDateTime;

            /// DWORD->unsigned int
            internal uint dwHighDateTime;
        }

    }

}
