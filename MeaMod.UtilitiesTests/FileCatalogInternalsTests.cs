using System.Collections.ObjectModel;
using System.Reflection;
using System.Security.Cryptography;
using MeaMod.Utilities.Security;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class FileCatalogInternalsTests
    {
        private static readonly string TestFiles = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "testfiles");
        private static string ValidCatalog => Path.Combine(TestFiles, "vlc_valid.cat");
        private static string VlcFolder => Path.Combine(TestFiles, "vlc");

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

        private string CreateFile(string name, byte[] content)
        {
            string path = Path.Combine(_root, name);
            File.WriteAllBytes(path, content);
            return path;
        }

        [TestMethod]
        public void HashAlgorithmFollowsTheCatalogVersion()
        {
            Assert.AreEqual("SHA1", FileCatalog.GetCatalogHashAlgorithm(1));
            Assert.AreEqual("SHA256", FileCatalog.GetCatalogHashAlgorithm(2));
            Assert.ThrowsExactly<InvalidOperationException>(() => FileCatalog.GetCatalogHashAlgorithm(3));
        }

        [TestMethod]
        public void CalculateFileHashMatchesThePlainHashForFlatFiles()
        {
            byte[] content = Enumerable.Range(0, 1000).Select(i => (byte)(i * 7)).ToArray();
            string path = CreateFile("flat.bin", content);

            Assert.AreEqual(Convert.ToHexString(SHA256.HashData(content)), FileCatalog.CalculateFileHash(path, "SHA256"));
            Assert.AreEqual(Convert.ToHexString(SHA1.HashData(content)), FileCatalog.CalculateFileHash(path, "SHA1"));
        }

        [TestMethod]
        public void CalculateFileHashRejectsAnUnknownAlgorithm()
        {
            string path = CreateFile("flat.bin", new byte[] { 1, 2, 3 });

            Assert.ThrowsExactly<InvalidOperationException>(() => FileCatalog.CalculateFileHash(path, "NOPE"));
        }

        [TestMethod]
        public void CalculateFileHashRejectsAMissingFile()
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => FileCatalog.CalculateFileHash(Path.Combine(_root, "missing.bin"), "SHA256"));
        }

        [TestMethod]
        public void NonCatalogFileYieldsNoEntriesAndFailsValidation()
        {
            // CryptCATOpen treats unparseable content as an empty catalog rather than failing, so validation
            // against it can never succeed.
            string path = CreateFile("not-a-catalog.cat", new byte[] { 1, 2, 3, 4 });

            Dictionary<string, string> hashes = FileCatalog.GetHashesFromCatalog(path, out _);
            Assert.AreEqual(0, hashes.Count);

            CatalogInformation info = FileCatalog.Validate(new Collection<string> { VlcFolder }, path, null!);
            Assert.AreEqual(CatalogValidationStatus.ValidationFailed, info.Status);
        }

        [TestMethod]
        public void GetHashesFromCatalogReadsTheVersionAndEntries()
        {
            Fixtures.Require("vlc_valid.cat");

            Dictionary<string, string> hashes = FileCatalog.GetHashesFromCatalog(ValidCatalog, out int version);

            Assert.IsTrue(version == 1 || version == 2, version.ToString());
            Assert.IsTrue(hashes.Count > 0);
        }

        [TestMethod]
        public void ProcessPathFileRejectsDuplicateRelativePaths()
        {
            string path = CreateFile("dup.bin", new byte[] { 1 });
            var hashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            FileCatalog.ProcessPathFile(new FileInfo(path), null!, "SHA256", ref hashes);

            Assert.ThrowsExactly<InvalidOperationException>(() => FileCatalog.ProcessPathFile(new FileInfo(path), null!, "SHA256", ref hashes));
        }

        [TestMethod]
        public void ProcessPathFileRecordsAnEmptyHashForAnEmptyFile()
        {
            string path = CreateFile("empty.bin", Array.Empty<byte>());
            var hashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            FileCatalog.ProcessPathFile(new FileInfo(path), new DirectoryInfo(_root), "SHA256", ref hashes);

            Assert.AreEqual(string.Empty, hashes["empty.bin"]);
        }

        [TestMethod]
        public void CompareDictionariesCoversEveryOutcome()
        {
            var catalog = new Dictionary<string, string> { ["a"] = "1", ["b"] = "2" };

            Assert.IsTrue(FileCatalog.CompareDictionaries(catalog, new Dictionary<string, string> { ["a"] = "1", ["b"] = "2" }, null!));
            Assert.IsFalse(FileCatalog.CompareDictionaries(catalog, new Dictionary<string, string> { ["a"] = "1" }, null!), "missing from folder");
            Assert.IsFalse(FileCatalog.CompareDictionaries(catalog, new Dictionary<string, string> { ["a"] = "1", ["b"] = "x" }, null!), "hash differs");
            Assert.IsFalse(FileCatalog.CompareDictionaries(catalog, new Dictionary<string, string> { ["a"] = "1", ["b"] = "2", ["c"] = "3" }, null!), "extra in folder");
        }

        [TestMethod]
        public void ValidateWithASingleFileHashesJustThatFile()
        {
            Fixtures.Require("vlc");
            Fixtures.Require("vlc_valid.cat");
            string file = Directory.EnumerateFiles(VlcFolder, "*", SearchOption.AllDirectories).First();

            CatalogInformation info = FileCatalog.Validate(new Collection<string> { file }, ValidCatalog, null!);

            Assert.AreEqual(1, info.PathItems.Count);
            Assert.IsTrue(info.PathItems.ContainsKey(Path.GetFileName(file)));
            Assert.AreEqual(CatalogValidationStatus.ValidationFailed, info.Status);
        }

        [TestMethod]
        public void ValidateSkipsTheCatalogFileItself()
        {
            // testfiles contains the catalog being validated; it must not be hashed as part of the folder.
            Fixtures.Require("vlc_valid.cat");
            CatalogInformation info = FileCatalog.Validate(new Collection<string> { TestFiles }, ValidCatalog, null!);

            Assert.IsFalse(info.PathItems.ContainsKey(Path.GetFileName(ValidCatalog)));
            Assert.AreEqual(CatalogValidationStatus.ValidationFailed, info.Status);
        }

        [TestMethod]
        public void ValidateWithAMissingCatalogThrows()
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => FileCatalog.Validate(new Collection<string> { VlcFolder }, Path.Combine(_root, "missing.cat"), null!));
        }

        [TestMethod]
        public void ValidateIgnoresPathsThatDoNotExist()
        {
            Fixtures.Require("vlc_valid.cat");
            CatalogInformation info = FileCatalog.Validate(new Collection<string> { Path.Combine(_root, "nothing-here") }, ValidCatalog, null!);

            Assert.AreEqual(0, info.PathItems.Count);
            Assert.AreEqual(CatalogValidationStatus.ValidationFailed, info.Status);
        }
    }
}
