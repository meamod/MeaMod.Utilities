using System;
using System.Runtime.InteropServices;

namespace MeaMod.Utilities
{
    /// <summary>
    /// AuthenticodeTools class.
    /// <para><see href="http://stackoverflow.com/a/6597017/1837988"/></para>
    /// <para>Licence: CC BY-SA 3.0</para>
    /// </summary>
    public static class AuthenticodeTools
    {
        [DllImport("Wintrust.dll", PreserveSig = true, SetLastError = false)]
        private static extern uint WinVerifyTrust(IntPtr hWnd, IntPtr pgActionID, IntPtr pWinTrustData);

        private static uint WinVerifyTrust(string fileName)
        {
            var wintrust_action_generic_verify_v2 = new Guid("{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}");
            uint result = 0;
            using (var fileInfo = new WINTRUST_FILE_INFO(fileName,
                Guid.Empty))
            using (var guidPtr = new UnmanagedPointer(Marshal.AllocHGlobal(Marshal.SizeOf<Guid>()),
                AllocMethod.HGlobal))
            using (var wvtDataPtr = new UnmanagedPointer(Marshal.AllocHGlobal(Marshal.SizeOf<WINTRUST_DATA>()),
                AllocMethod.HGlobal))
            // WINTRUST_DATA owns an unmanaged copy of fileInfo; it must be disposed or that copy leaks on every call.
            using (var data = new WINTRUST_DATA(fileInfo))
            {
                IntPtr pGuid = guidPtr;
                IntPtr pData = wvtDataPtr;
                // fDeleteOld must be false: the destination buffers are freshly allocated and hold no marshalled data yet.
                Marshal.StructureToPtr(wintrust_action_generic_verify_v2,
                    pGuid,
                    false);
                Marshal.StructureToPtr(data,
                    pData,
                    false);
                result = WinVerifyTrust(IntPtr.Zero,
                    pGuid,
                    pData);
            }
            return result;
        }

        /// <summary>Check if <paramref name="filepath"/> has a valid Authenticode certificate</summary>
        /// <param name="filepath"></param>
        /// <returns>Boolean if file has valid Authenticode certificate</returns>
        public static bool IsTrusted(string filepath)
        {
            return WinVerifyTrust(filepath) == 0;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WINTRUST_FILE_INFO : IDisposable
    {
        public WINTRUST_FILE_INFO(string fileName, Guid subject)
        {
            cbStruct = (uint)Marshal.SizeOf<WINTRUST_FILE_INFO>();

            pcwszFilePath = fileName;


            if (subject != Guid.Empty)
            {
                pgKnownSubject = Marshal.AllocHGlobal(Marshal.SizeOf<Guid>());

                Marshal.StructureToPtr(subject, pgKnownSubject, false);
            }

            else
            {
                pgKnownSubject = IntPtr.Zero;
            }

            hFile = IntPtr.Zero;
        }

        public uint cbStruct;

        [MarshalAs(UnmanagedType.LPTStr)] public string pcwszFilePath;

        public IntPtr hFile;

        public IntPtr pgKnownSubject;

        #region IDisposable Members

        public void Dispose()
        {
            if (pgKnownSubject != IntPtr.Zero)
            {
                Marshal.DestroyStructure<Guid>(pgKnownSubject);

                Marshal.FreeHGlobal(pgKnownSubject);

                pgKnownSubject = IntPtr.Zero;
            }
        }

        #endregion
    }

    internal enum AllocMethod
    {
        HGlobal,
        CoTaskMem
    }

    internal enum UnionChoice
    {
        File = 1,
        Catalog,
        Blob,
        Signer,
        Cert
    }

    internal enum UiChoice
    {
        All = 1,
        NoUI,
        NoBad,
        NoGood
    }

    internal enum RevocationCheckFlags
    {
        None = 0,
        WholeChain
    }

    internal enum StateAction
    {
        Ignore = 0,
        Verify,
        Close,
        AutoCache,
        AutoCacheFlush
    }

    internal enum TrustProviderFlags
    {
        UseIE4Trust = 1,
        NoIE4Chain = 2,
        NoPolicyUsage = 4,
        RevocationCheckNone = 16,
        RevocationCheckEndCert = 32,
        RevocationCheckChain = 64,
        RecovationCheckChainExcludeRoot = 128,
        Safer = 256,
        HashOnly = 512,
        UseDefaultOSVerCheck = 1024,
        LifetimeSigning = 2048
    }

    internal enum UIContext
    {
        Execute = 0,
        Install
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WINTRUST_DATA : IDisposable
    {
        public WINTRUST_DATA(WINTRUST_FILE_INFO fileInfo)
        {
            cbStruct = (uint)Marshal.SizeOf<WINTRUST_DATA>();

            pInfoStruct = Marshal.AllocHGlobal(Marshal.SizeOf<WINTRUST_FILE_INFO>());

            // This marshals a copy of the file path string into unmanaged memory; Dispose releases it.
            Marshal.StructureToPtr(fileInfo, pInfoStruct, false);

            dwUnionChoice = UnionChoice.File;


            pPolicyCallbackData = IntPtr.Zero;

            pSIPCallbackData = IntPtr.Zero;


            dwUIChoice = UiChoice.NoUI;

            fdwRevocationChecks = RevocationCheckFlags.None;

            dwStateAction = StateAction.Ignore;

            hWVTStateData = IntPtr.Zero;

            pwszURLReference = IntPtr.Zero;

            dwProvFlags = TrustProviderFlags.Safer;


            dwUIContext = UIContext.Execute;
        }


        public uint cbStruct;

        public IntPtr pPolicyCallbackData;

        public IntPtr pSIPCallbackData;

        public UiChoice dwUIChoice;

        public RevocationCheckFlags fdwRevocationChecks;

        public UnionChoice dwUnionChoice;

        public IntPtr pInfoStruct;

        public StateAction dwStateAction;

        public IntPtr hWVTStateData;

        private readonly IntPtr pwszURLReference;

        public TrustProviderFlags dwProvFlags;

        public UIContext dwUIContext;

        #region IDisposable Members

        public void Dispose()
        {
            if (pInfoStruct == IntPtr.Zero)
                return;

            if (dwUnionChoice == UnionChoice.File)
            {
                // Frees the unmanaged string allocated by StructureToPtr for pcwszFilePath.
                Marshal.DestroyStructure<WINTRUST_FILE_INFO>(pInfoStruct);
            }

            Marshal.FreeHGlobal(pInfoStruct);

            pInfoStruct = IntPtr.Zero;
        }

        #endregion
    }

    internal sealed class UnmanagedPointer : IDisposable
    {
        private readonly AllocMethod m_meth;

        private IntPtr m_ptr;

        internal UnmanagedPointer(IntPtr ptr, AllocMethod method)
        {
            m_meth = method;

            m_ptr = ptr;
        }


        ~UnmanagedPointer()
        {
            ReleaseMemory();
        }


        public static implicit operator IntPtr(UnmanagedPointer ptr)
        {
            return ptr.m_ptr;
        }

        #region IDisposable Members

        private void ReleaseMemory()
        {
            if (m_ptr != IntPtr.Zero)
            {
                if (m_meth == AllocMethod.HGlobal)
                {
                    Marshal.FreeHGlobal(m_ptr);
                }

                else if (m_meth == AllocMethod.CoTaskMem)
                {
                    Marshal.FreeCoTaskMem(m_ptr);
                }

                m_ptr = IntPtr.Zero;
            }
        }


        public void Dispose()
        {
            ReleaseMemory();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
