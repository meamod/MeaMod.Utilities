using System.Diagnostics;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class AuthenticodeToolsTests
    {
        private static string SignedSystemFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "kernel32.dll");

        private static string CreateUnsignedFile()
        {
            string path = Path.Combine(Path.GetTempPath(), "MeaMod.Utilities.Tests." + Guid.NewGuid().ToString("N") + ".bin");
            File.WriteAllBytes(path, Enumerable.Range(0, 4096).Select(i => (byte)i).ToArray());
            return path;
        }

        [TestMethod]
        public void SignedSystemFileIsTrusted()
        {
            Assert.IsTrue(AuthenticodeTools.IsTrusted(SignedSystemFile));
        }

        [TestMethod]
        public void UnsignedFileIsNotTrusted()
        {
            string path = CreateUnsignedFile();
            try
            {
                Assert.IsFalse(AuthenticodeTools.IsTrusted(path));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        public void RepeatedCallsDoNotLeakUnmanagedMemory()
        {
            // Before the fix every call leaked the WINTRUST_FILE_INFO block plus the marshalled path string:
            // roughly 11 MB over 20,000 calls. The threshold leaves headroom for normal heap growth.
            const int iterations = 20_000;
            const long allowedGrowthBytes = 4L * 1024 * 1024;

            string path = CreateUnsignedFile();
            try
            {
                for (int i = 0; i < 100; i++)
                    AuthenticodeTools.IsTrusted(path);

                long before = PrivateBytes();
                for (int i = 0; i < iterations; i++)
                    AuthenticodeTools.IsTrusted(path);
                long after = PrivateBytes();

                Assert.IsTrue(after - before < allowedGrowthBytes, $"private bytes grew by {(after - before) / 1024} KB over {iterations} calls");
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static long PrivateBytes()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            using var process = Process.GetCurrentProcess();
            process.Refresh();
            return process.PrivateMemorySize64;
        }
    }
}
