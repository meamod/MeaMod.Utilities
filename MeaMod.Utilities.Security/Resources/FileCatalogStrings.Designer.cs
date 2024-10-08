﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MeaMod.Utilities.Security.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class FileCatalogStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal FileCatalogStrings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MeaMod.Utilities.Security.Resources.FileCatalogStrings", typeof(FileCatalogStrings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Paths for catalog contains multiple files with same relative path {0}. .
        /// </summary>
        internal static string FoundDuplicateFilesRelativePath {
            get {
                return ResourceManager.GetString("FoundDuplicateFilesRelativePath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to acquire a handle to a catalog administrator context for a given hash algorithm {0}..
        /// </summary>
        internal static string UnableToAcquireHashAlgorithmContext {
            get {
                return ResourceManager.GetString("UnableToAcquireHashAlgorithmContext", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to create the hash for file {0}. .
        /// </summary>
        internal static string UnableToCreateFileHash {
            get {
                return ResourceManager.GetString("UnableToCreateFileHash", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to open catalog file {0}..
        /// </summary>
        internal static string UnableToOpenCatalogFile {
            get {
                return ResourceManager.GetString("UnableToOpenCatalogFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to read file {0} to calculate its hash..
        /// </summary>
        internal static string UnableToReadFileToHash {
            get {
                return ResourceManager.GetString("UnableToReadFileToHash", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Catalog version is not valid. We only support catalog version {0} and version {1}..
        /// </summary>
        internal static string UnKnownCatalogVersion {
            get {
                return ResourceManager.GetString("UnKnownCatalogVersion", resourceCulture);
            }
        }
    }
}
