using System.Runtime.InteropServices;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class AuthenticodeInternalsTests
    {
        [TestMethod]
        public void FileInfoWithAKnownSubjectAllocatesAndFreesTheGuid()
        {
            var info = new WINTRUST_FILE_INFO("file", Guid.NewGuid());
            Assert.AreNotEqual(IntPtr.Zero, info.pgKnownSubject);
            Assert.AreEqual("file", info.pcwszFilePath);

            info.Dispose();
            Assert.AreEqual(IntPtr.Zero, info.pgKnownSubject);

            // Disposing again must be harmless.
            info.Dispose();
        }

        [TestMethod]
        public void FileInfoWithoutASubjectAllocatesNothing()
        {
            var info = new WINTRUST_FILE_INFO("file", Guid.Empty);
            Assert.AreEqual(IntPtr.Zero, info.pgKnownSubject);
            info.Dispose();
        }

        [TestMethod]
        public void WinTrustDataDisposeFreesTheFileInfoBlockOnce()
        {
            using var fileInfo = new WINTRUST_FILE_INFO("file", Guid.Empty);
            var data = new WINTRUST_DATA(fileInfo);
            Assert.AreNotEqual(IntPtr.Zero, data.pInfoStruct);
            Assert.AreEqual(UnionChoice.File, data.dwUnionChoice);
            Assert.AreEqual(UiChoice.NoUI, data.dwUIChoice);

            data.Dispose();
            Assert.AreEqual(IntPtr.Zero, data.pInfoStruct);

            data.Dispose();
        }

        [TestMethod]
        public void UnmanagedPointerFreesCoTaskMemory()
        {
            var pointer = new UnmanagedPointer(Marshal.AllocCoTaskMem(16), AllocMethod.CoTaskMem);
            Assert.AreNotEqual(IntPtr.Zero, (IntPtr)pointer);

            pointer.Dispose();
            Assert.AreEqual(IntPtr.Zero, (IntPtr)pointer);

            pointer.Dispose();
        }

        [TestMethod]
        public void UnmanagedPointerFinalizerFreesHGlobalMemory()
        {
            CreateAndDrop();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private static void CreateAndDrop()
        {
            _ = new UnmanagedPointer(Marshal.AllocHGlobal(16), AllocMethod.HGlobal);
        }
    }
}
