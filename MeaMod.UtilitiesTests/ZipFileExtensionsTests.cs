using System.IO.Compression;
using MeaMod.Utilities.Zip;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class ZipFileExtensionsTests
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

        private static MemoryStream CreateArchive(params (string Name, string Content)[] entries)
        {
            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var (name, content) in entries)
                {
                    using var writer = new StreamWriter(archive.CreateEntry(name).Open());
                    writer.Write(content);
                }
            }

            stream.Position = 0;
            return stream;
        }

        [TestMethod]
        public void EntryEscapingToASiblingDirectoryWithTheSamePrefixIsRejected()
        {
            // "out-evil" starts with "out", which the old StartsWith check without a trailing separator accepted.
            string destination = Path.Combine(_root, "out");
            string escaped = Path.Combine(_root, "out-evil", "escaped.txt");

            using var stream = CreateArchive(("../out-evil/escaped.txt", "escaped"));
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            Assert.ThrowsExactly<IOException>(() => archive.ExtractToDirectory(destination, (IProgress<ZipProgress>?)null));
            Assert.IsFalse(File.Exists(escaped), "the entry was written outside the destination directory");
        }

        [TestMethod]
        public void EntryEscapingToTheParentDirectoryIsRejected()
        {
            string destination = Path.Combine(_root, "out");

            using var stream = CreateArchive(("../escaped.txt", "escaped"));
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            Assert.ThrowsExactly<IOException>(() => archive.ExtractToDirectory(destination, (IProgress<ZipProgress>?)null));
            Assert.IsFalse(File.Exists(Path.Combine(_root, "escaped.txt")));
        }

        [TestMethod]
        public void NormalEntriesExtractAndReportProgress()
        {
            string destination = Path.Combine(_root, "out");
            var reported = new List<ZipProgress>();
            var progress = new SynchronousProgress(reported.Add);

            using var stream = CreateArchive(("a.txt", "A"), ("sub/b.txt", "B"));
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            archive.ExtractToDirectory(destination, progress);

            Assert.AreEqual("A", File.ReadAllText(Path.Combine(destination, "a.txt")));
            Assert.AreEqual("B", File.ReadAllText(Path.Combine(destination, "sub", "b.txt")));
            Assert.HasCount(2, reported);
            Assert.AreEqual(2, reported[1].Total);
            Assert.AreEqual(2, reported[1].Processed);
            Assert.AreEqual("sub/b.txt", reported[1].CurrentItem);
        }

        [TestMethod]
        public void NullProgressIsAllowed()
        {
            string destination = Path.Combine(_root, "out");

            using var stream = CreateArchive(("a.txt", "A"));
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            archive.ExtractToDirectory(destination, (IProgress<ZipProgress>?)null, overwrite: true);

            Assert.AreEqual("A", File.ReadAllText(Path.Combine(destination, "a.txt")));
        }

        /// <summary>Progress&lt;T&gt; posts to a synchronization context; this one is synchronous so the test can inspect it.</summary>
        private sealed class SynchronousProgress(Action<ZipProgress> handler) : IProgress<ZipProgress>
        {
            public void Report(ZipProgress value) => handler(value);
        }
    }
}
