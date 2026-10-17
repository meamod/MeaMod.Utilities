// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if !UNIX

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using MeaMod.Utilities.Security.Resources;

namespace MeaMod.Utilities.Security
{
    /// <summary>
    /// Defines the possible status when validating integrity of catalog.
    /// </summary>
    public enum CatalogValidationStatus
    {
        /// <summary>
        /// Status when catalog is not tampered.
        /// </summary>
        Valid,

        /// <summary>
        /// Status when catalog is tampered.
        /// </summary>
        ValidationFailed
    }

    /// <summary>
    /// Object returned by Catalog Cmdlets.
    /// </summary>
    public class CatalogInformation
    {
        /// <summary>
        /// Status of catalog.
        /// </summary>
        public CatalogValidationStatus Status { get; set; }

        /// <summary>
        /// Hash Algorithm used to calculate the hashes of files in Catalog.
        /// </summary>
        public string HashAlgorithm { get; set; }

        /// <summary>
        /// Dictionary mapping files relative paths to their hash values found from Catalog.
        /// </summary>
        public Dictionary<string, string> CatalogItems { get; set; }

        /// <summary>
        /// Dictionary mapping files relative paths to their hash values.
        /// </summary>
        public Dictionary<string, string> PathItems { get; set; }

        /// <summary>
        /// Signature for the catalog.
        /// </summary>
        public Signature Signature { get; set; }
    }

    /// <summary>
    /// Helper functions for Windows Catalog functionality.
    /// </summary>
    public static class FileCatalog
    {
        // Catalog Version is (0X100 = 256) for Catalog Version 1
        private const int catalogVersion1 = 256;

        // Catalog Version is (0X200 = 512) for Catalog Version 2
        private const int catalogVersion2 = 512;

        // Hash Algorithms supported by Windows Catalog
        private const string HashAlgorithmSHA1 = "SHA1";
        private const string HashAlgorithmSHA256 = "SHA256";

        /// <summary>
        /// Find out the Version of Catalog by reading its Meta data. We can have either version 1 or version 2 catalog.
        /// </summary>
        /// <param name="catalogHandle">Handle to open catalog file.</param>
        /// <returns>Version of the catalog.</returns>
        private static int GetCatalogVersion(SafeCATHandle catalogHandle)
        {
            WinTrustMethods.CRYPTCATSTORE catalogInfo = WinTrustMethods.CryptCATStoreFromHandle(catalogHandle);

            int catalogVersion;
            if (catalogInfo.dwPublicVersion == catalogVersion2)
            {
                catalogVersion = 2;
            }
            // One Windows 7 this API sent version information as decimal 1 not hex (0X100 = 256)
            // so we are checking for that value as well. Reason we are not checking for version 2 above in
            // this scenario because catalog version 2 is not supported on win7.
            else if (catalogInfo.dwPublicVersion == catalogVersion1 || catalogInfo.dwPublicVersion == 1)
            {
                catalogVersion = 1;
            }
            else
            {
                // catalog version we don't understand
                Exception exception = new InvalidOperationException(string.Format(FileCatalogStrings.UnKnownCatalogVersion,
                                      catalogVersion1.ToString("X"),
                                      catalogVersion2.ToString("X")));

                throw exception;
            }

            return catalogVersion;
        }

        /// <summary>
        /// HashAlgorithm used by the Catalog. It is based on the version of Catalog.
        /// </summary>
        /// <param name="catalogVersion">Path of the output catalog file.</param>
        /// <returns>Version of the catalog.</returns>
        internal static string GetCatalogHashAlgorithm(int catalogVersion)
        {
            string hashAlgorithm;

            if (catalogVersion == 1)
            {
                hashAlgorithm = HashAlgorithmSHA1;
            }
            else if (catalogVersion == 2)
            {
                hashAlgorithm = HashAlgorithmSHA256;
            }
            else
            {
                // version we don't understand
                Exception exception = new InvalidOperationException(string.Format(FileCatalogStrings.UnKnownCatalogVersion,
                                      "1.0",
                                      "2.0"));

                throw exception;
            }

            return hashAlgorithm;
        }

        /// <summary>
        /// Get file attribute (Relative path in our case) from catalog.
        /// </summary>
        /// <param name="memberAttrInfo">Pointer to current attribute of catalog member.</param>
        /// <returns>Value of the attribute.</returns>
        internal static string ProcessFilePathAttributeInCatalog(IntPtr memberAttrInfo)
        {
            string relativePath = string.Empty;

            WinTrustMethods.CRYPTCATATTRIBUTE currentMemberAttr = Marshal.PtrToStructure<WinTrustMethods.CRYPTCATATTRIBUTE>(memberAttrInfo);

            // check if this is the attribute we are looking for
            // catalog generated other way not using New-FileCatalog can have attributes we don't understand
            if (currentMemberAttr.pwszReferenceTag.Equals("FilePath", StringComparison.OrdinalIgnoreCase))
            {
                // find the size for the current attribute value and then allocate buffer and copy from byte array
                int attrValueSize = (int)currentMemberAttr.cbValue;
                byte[] attrValue = new byte[attrValueSize];
                Marshal.Copy(currentMemberAttr.pbValue, attrValue, 0, attrValueSize);
                relativePath = System.Text.Encoding.Unicode.GetString(attrValue);
                relativePath = relativePath.TrimEnd('\0');
            }

            return relativePath;
        }

        /// <summary>
        /// Make a hash for the file.
        /// </summary>
        /// <param name="filePath">Path of the file.</param>
        /// <param name="hashAlgorithm">Used to calculate Hash.</param>
        /// <returns>HashValue for the file.</returns>
        internal static string CalculateFileHash(string filePath, string hashAlgorithm)
        {
            string hashValue = string.Empty;

            // To get handle to the hash algorithm to be used to calculate hashes
            SafeCATAdminHandle catAdmin;
            try
            {
                catAdmin = WinTrustMethods.CryptCATAdminAcquireContext2(hashAlgorithm);
            }
            catch (Win32Exception e)
            {
                throw new InvalidOperationException(
                    string.Format(FileCatalogStrings.UnableToAcquireHashAlgorithmContext, hashAlgorithm), e);

                // The method returns an empty string on a failure.
                //return hashValue;
            }

            // Open the file that is to be hashed for reading and get its handle
            FileStream fileStream;
            try
            {
                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception e)
            {
                // If we are not able to open file that is to be hashed we can not continue with catalog validation
                throw new InvalidOperationException(string.Format(FileCatalogStrings.UnableToReadFileToHash, filePath), e);

                // The method returns an empty string on a failure.
                //return hashValue;
            }

            using (catAdmin)
            using (fileStream)
            {
                byte[] hashBytes = [];
                try
                {
                    hashBytes = WinTrustMethods.CryptCATAdminCalcHashFromFileHandle2(catAdmin, fileStream.SafeFileHandle);
                }
                catch (Win32Exception e)
                {
                    throw new InvalidOperationException(string.Format(FileCatalogStrings.UnableToCreateFileHash, filePath),
                        e);
                }

                hashValue = BitConverter.ToString(hashBytes).Replace("-", "");
            }

            return hashValue;
        }

        /// <summary>
        /// Make list of hashes for given Catalog File.
        /// </summary>
        /// <param name="catalogFilePath">Path to the folder having catalog file.</param>
        /// <param name="catalogVersion">The version of input catalog we read from catalog meta data after opening it.</param>
        /// <returns>Dictionary mapping files relative paths to HashValues.</returns>
        internal static Dictionary<string, string> GetHashesFromCatalog(string catalogFilePath, out int catalogVersion)
        {
            Dictionary<string, string> catalogHashes = new(StringComparer.CurrentCultureIgnoreCase);
            catalogVersion = 0;

            SafeCATHandle resultCatalog;
            try
            {
                resultCatalog = WinTrustMethods.CryptCATOpen(catalogFilePath, 0, IntPtr.Zero, 1, 0);
            }
            catch (Win32Exception e)
            {
                throw new InvalidOperationException(string.Format(FileCatalogStrings.UnableToOpenCatalogFile, catalogFilePath), e);
                // return catalogHashes;
            }

            using (resultCatalog)
            {
                IntPtr catAttrInfo = IntPtr.Zero;

                // First traverse all catalog level attributes to get information about zero size file.
                do
                {
                    catAttrInfo = WinTrustMethods.CryptCATEnumerateCatAttr(resultCatalog, catAttrInfo);

                    // If we found attribute it is a file information retrieve its relative path
                    // and add it to catalog hash collection if its not in excluded files criteria
                    if (catAttrInfo != IntPtr.Zero)
                    {
                        string relativePath = ProcessFilePathAttributeInCatalog(catAttrInfo);
                        if (!string.IsNullOrEmpty(relativePath))
                        {
                            ProcessCatalogFile(relativePath, string.Empty, ref catalogHashes);
                        }
                    }
                } while (catAttrInfo != IntPtr.Zero);

                catalogVersion = GetCatalogVersion(resultCatalog);

                IntPtr memberInfo = IntPtr.Zero;
                // Next Navigate all members in Catalog files and get their relative paths and hashes
                do
                {
                    memberInfo = WinTrustMethods.CryptCATEnumerateMember(resultCatalog, memberInfo);
                    if (memberInfo != IntPtr.Zero)
                    {
                        WinTrustMethods.CRYPTCATMEMBER currentMember = Marshal.PtrToStructure<WinTrustMethods.CRYPTCATMEMBER>(memberInfo);
                        WinTrustMethods.SIP_INDIRECT_DATA pIndirectData = Marshal.PtrToStructure<WinTrustMethods.SIP_INDIRECT_DATA>(currentMember.pIndirectData);

                        // For Catalog version 2 CryptoAPI puts hashes of file attributes(relative path in our case) in Catalog as well
                        // We validate those along with file hashes so we are skipping duplicate entries
                        if (!(catalogVersion == 2 && pIndirectData.DigestAlgorithm.pszObjId.Equals(new Oid("SHA1").Value, StringComparison.OrdinalIgnoreCase)))
                        {
                            string relativePath = string.Empty;
                            IntPtr memberAttrInfo = IntPtr.Zero;
                            do
                            {
                                memberAttrInfo = WinTrustMethods.CryptCATEnumerateAttr(resultCatalog, memberInfo, memberAttrInfo);

                                if (memberAttrInfo != IntPtr.Zero)
                                {
                                    relativePath = ProcessFilePathAttributeInCatalog(memberAttrInfo);
                                    if (!string.IsNullOrEmpty(relativePath))
                                    {
                                        break;
                                    }
                                }
                            }
                            while (memberAttrInfo != IntPtr.Zero);

                            // If we did not find any Relative Path for the item in catalog we should quit
                            // This catalog must not be valid for our use as catalogs generated using New-FileCatalog
                            // always contains relative file Paths
                            if (string.IsNullOrEmpty(relativePath))
                            {
                                throw new InvalidOperationException(string.Format(FileCatalogStrings.UnableToOpenCatalogFile, catalogFilePath));
                            }

                            ProcessCatalogFile(relativePath, currentMember.pwszReferenceTag, ref catalogHashes);
                        }
                    }
                } while (memberInfo != IntPtr.Zero);
            }

            return catalogHashes;
        }

        /// <summary>
        /// Process file in path for its relative paths.
        /// </summary>
        /// <param name="relativePath">Relative path of file found in catalog.</param>
        /// <param name="fileHash">Hash of file found in catalog.</param>
        /// <param name="catalogHashes">Collection of hashes of catalog.</param>
        /// <returns>Void.</returns>
        internal static void ProcessCatalogFile(string relativePath, string fileHash, ref Dictionary<string, string> catalogHashes)
        {
            // Found the attribute we are looking for
            //_cmdlet.WriteVerbose(string.Format(FileCatalogStrings.FoundFileHashInCatalogItem, relativePath, fileHash));

            // Add relativePath mapping to hashvalue for each file
            catalogHashes.Add(relativePath, fileHash);

        }
        /// <summary>
        /// Process file in path for its relative paths.
        /// </summary>
        /// <param name="fileToHash">File to hash.</param>
        /// <param name="dirInfo">Directory information about file needed to calculate relative file path.</param>
        /// <param name="hashAlgorithm">Used to calculate Hash.</param>
        /// <param name="excludedPatterns">Skip file if it matches these patterns.</param>
        /// <param name="fileHashes">Collection of hashes of files.</param>
        /// <returns>Void.</returns>
        internal static void ProcessPathFile(FileInfo fileToHash, DirectoryInfo dirInfo, string hashAlgorithm, ref Dictionary<string, string> fileHashes)
        {
            string relativePath;

            // Relative path of the file is the path inside the containing folder excluding folder Name
            relativePath = dirInfo != null ? fileToHash.FullName.AsSpan(dirInfo.FullName.Length).TrimStart('\\').ToString() : fileToHash.Name;

            string fileHash = string.Empty;

            if (fileToHash.Length != 0)
            {
                fileHash = CalculateFileHash(fileToHash.FullName, hashAlgorithm);
            }

            if (!fileHashes.TryAdd(relativePath, fileHash))
            {
                throw new InvalidOperationException(string.Format(FileCatalogStrings.FoundDuplicateFilesRelativePath, relativePath));
            }
        }

        /// <summary>
        /// Generate the hashes of all the files in given folder.
        /// </summary>
        /// <param name="folderPaths">Path to folder or File.</param>
        /// <param name="catalogFilePath">Catalog file path it should be skipped when calculating the hashes.</param>
        /// <param name="hashAlgorithm">Used to calculate Hash.</param>
        /// <param name="catalogProgress">Used to report progress</param>
        /// <returns>Dictionary mapping file relative paths to hashes..</returns>
        internal static Dictionary<string, string> CalculateHashesFromPath(Collection<string> folderPaths, string catalogFilePath, string hashAlgorithm, IProgress<CatalogProgress> catalogProgress)
        {
            // Create a HashTable of file Hashes
            Dictionary<string, string> fileHashes = new(StringComparer.CurrentCultureIgnoreCase);
            int count = 0;
            foreach (string folderPath in folderPaths)
            {
                count++;
                if (Directory.Exists(folderPath))
                {
                    var directoryItems = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories).ToList();
                    int countDirectoryItems = 0;
                    foreach (string fileItem in directoryItems)
                    {
                        countDirectoryItems++;
                        // if its the catalog file we are validating we will skip it
                        if (string.Equals(fileItem, catalogFilePath, StringComparison.OrdinalIgnoreCase))
                            continue;

                        ProcessPathFile(new FileInfo(fileItem), new DirectoryInfo(folderPath), hashAlgorithm, ref fileHashes);
                        catalogProgress?.Report(new CatalogProgress("Calculating Hashes", directoryItems.Count(), countDirectoryItems, new FileInfo(fileItem).Name, hashAlgorithm));
                    }
                }
                else if (File.Exists(folderPath))
                {
                    ProcessPathFile(new FileInfo(folderPath), null, hashAlgorithm, ref fileHashes);
                    catalogProgress?.Report(new CatalogProgress("Calculating Hashes", folderPaths.Count(), count, new FileInfo(folderPath).Name, hashAlgorithm));
                }


            }



            return fileHashes;
        }

        /// <summary>
        /// Compare Dictionary objects.
        /// </summary>
        /// <param name="catalogItems">Hashes extracted from Catalog.</param>
        /// <param name="pathItems">Hashes created from folders path.</param>
        /// <param name="catalogProgress">Used to report progress</param>
        /// <returns>True if both collections are same.</returns>
        internal static bool CompareDictionaries(Dictionary<string, string> catalogItems, Dictionary<string, string> pathItems, IProgress<CatalogProgress> catalogProgress)
        {
            bool Status = true;

            List<string> relativePathsFromFolder = [.. pathItems.Keys];
            List<string> relativePathsFromCatalog = [.. catalogItems.Keys];

            // Find entries that are not in both lists. These should be empty lists for success
            // Hashes in Catalog should be exactly similar to the ones from folder
            List<string> relativePathsNotInFolder = [.. relativePathsFromFolder.Except(relativePathsFromCatalog, StringComparer.CurrentCultureIgnoreCase)];
            List<string> relativePathsNotInCatalog = [.. relativePathsFromCatalog.Except(relativePathsFromFolder, StringComparer.CurrentCultureIgnoreCase)];

            // Found extra hashes in Folder
            if (relativePathsNotInFolder.Count != 0 || relativePathsNotInCatalog.Count != 0)
            {
                Status = false;
                catalogProgress?.Report(new CatalogProgress("Comparing Files", relativePathsFromCatalog.Count, relativePathsFromFolder.Count, "Count Mismatch", ""));

            }

            int count = 0;
            foreach (KeyValuePair<string, string> item in catalogItems)
            {
                count++;
                string catalogHashValue = catalogItems[item.Key];
                if (pathItems.ContainsKey(item.Key))
                {
                    string folderHashValue = pathItems[item.Key];
                    if (folderHashValue.Equals(catalogHashValue))
                    {
                        continue;
                    }
                    else
                    {
                        Status = false;
                    }
                }
                catalogProgress?.Report(new CatalogProgress("Comparing Files", relativePathsFromCatalog.Count, count, catalogHashValue, ""));
            }

            return Status;
        }

        /// <summary>
        /// To Validate the Integrity of Catalog.
        /// </summary>
        /// <param name="catalogFolders">Folder for which catalog is created.</param>
        /// <param name="catalogFilePath">File Name of the Catalog.</param>
        /// <param name="catalogProgress">Report Progress</param>
        /// <returns>Information about Catalog.</returns>
        public static CatalogInformation Validate(Collection<string> catalogFolders, string catalogFilePath, IProgress<CatalogProgress> catalogProgress)
        {
            Dictionary<string, string> catalogHashes = GetHashesFromCatalog(catalogFilePath, out var catalogVersion);
            string hashAlgorithm = GetCatalogHashAlgorithm(catalogVersion);

            //Return if no hash algorithms are  found
            if (string.IsNullOrEmpty(hashAlgorithm)) return null;

            Dictionary<string, string> fileHashes = CalculateHashesFromPath(catalogFolders, catalogFilePath, hashAlgorithm, catalogProgress);

            CatalogInformation catalog = new()
            {
                CatalogItems = catalogHashes,
                PathItems = fileHashes
            };

            bool status = CompareDictionaries(catalogHashes, fileHashes, catalogProgress);

            catalog.Status = status ? CatalogValidationStatus.Valid : CatalogValidationStatus.ValidationFailed;

            catalog.HashAlgorithm = hashAlgorithm;
            catalog.Signature = SignatureHelper.GetSignature(catalogFilePath, null);
            return catalog;

        }

    }
}

#endif
