using System.IO.Compression;
using MeaMod.Utilities.Zip;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class ZipFileExtensionsMoreTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Initialize()
        {
            _root = Path.Combine(Path.GetTempPath(), "MeaMod.Utilities.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, true);
        }

        private static MemoryStream CreateArchive(params (string Name, byte[] Content)[] entries)
        {
            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var (name, content) in entries)
                {
                    using Stream entryStream = archive.CreateEntry(name).Open();
                    entryStream.Write(content, 0, content.Length);
                }
            }

            stream.Position = 0;
            return stream;
        }

        [TestMethod]
        public void DirectoryEntriesAreCreated()
        {
            string destination = Path.Combine(_root, "out");
            using var stream = CreateArchive(("dir/", Array.Empty<byte>()), ("dir/file.txt", new byte[] { 65 }), ("empty/", Array.Empty<byte>()));
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            archive.ExtractToDirectory(destination, (IProgress<ZipProgress>?)null);

            Assert.IsTrue(Directory.Exists(Path.Combine(destination, "dir")));
            Assert.IsTrue(Directory.Exists(Path.Combine(destination, "empty")));
            Assert.AreEqual("A", File.ReadAllText(Path.Combine(destination, "dir", "file.txt")));
        }

        [TestMethod]
        public void DirectoryEntryWithDataIsRejected()
        {
            string destination = Path.Combine(_root, "out");
            using var stream = CreateArchive(("dir/", new byte[] { 1, 2, 3 }));
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            Assert.ThrowsExactly<IOException>(() => archive.ExtractToDirectory(destination, (IProgress<ZipProgress>?)null));
        }

        [TestMethod]
        public void NullArgumentsAreRejected()
        {
            using var stream = CreateArchive(("a.txt", new byte[] { 65 }));
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            Assert.ThrowsExactly<ArgumentNullException>(() => ((ZipArchive)null!).ExtractToDirectory(_root, (IProgress<ZipProgress>?)null));
            Assert.ThrowsExactly<ArgumentNullException>(() => archive.ExtractToDirectory(null!, (IProgress<ZipProgress>?)null));
        }

        [TestMethod]
        public void ExistingFilesAreOnlyOverwrittenWhenAsked()
        {
            string destination = Path.Combine(_root, "out");
            using var stream = CreateArchive(("a.txt", new byte[] { 65 }));
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            archive.ExtractToDirectory(destination, (IProgress<ZipProgress>?)null);
            Assert.ThrowsExactly<IOException>(() => archive.ExtractToDirectory(destination, (IProgress<ZipProgress>?)null, overwrite: false));
            archive.ExtractToDirectory(destination, (IProgress<ZipProgress>?)null, overwrite: true);

            Assert.AreEqual("A", File.ReadAllText(Path.Combine(destination, "a.txt")));
        }

        [TestMethod]
        public void ZipProgressExposesItsValues()
        {
            var progress = new ZipProgress(10, 3, "item");

            Assert.AreEqual(10, progress.Total);
            Assert.AreEqual(3, progress.Processed);
            Assert.AreEqual("item", progress.CurrentItem);
        }
    }
}
