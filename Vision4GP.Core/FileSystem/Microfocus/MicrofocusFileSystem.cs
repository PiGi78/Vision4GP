using System;
using System.Collections.Generic;
using System.IO;
using Vision4GP.Core.FileSystem;

namespace Vision4GP.Core.Microfocus
{


    /// <summary>
    /// Vision file system
    /// </summary>
    internal class MicrofocusFileSystem : IVisionFileSystem
    {

        /// <summary>
        /// Cannot be initialized. Use GetInstance method instead
        /// </summary>
        private MicrofocusFileSystem() {}



        /// <summary>
        /// Gets the instance of the Vision file system currently used
        /// </summary>
        /// <returns>Instance of the Vision file system</returns>
        internal static MicrofocusFileSystem GetInstance()
        {
            return CurrentInstance.Value;
        }


        private static Lazy<MicrofocusFileSystem> CurrentInstance = new Lazy<MicrofocusFileSystem>(() =>
        {
            var result = new MicrofocusFileSystem();
            result.Initialize();
            result.LoadFileDefinitions();
            return result;
        });


        /// <summary>
        /// Microfocus vision library
        /// </summary>
        private IMicrofocusVisionLibrary MicrofocusVisionLibrary 
        { 
            get
            {
                return Microfocus.MicrofocusVisionLibrary.GetInstance();
            } 
        }


        /// <summary>
        /// File definitions
        /// </summary>
        private Dictionary<string, VisionFileDefinition> FileDefinitions = new Dictionary<string, VisionFileDefinition>();


        /// <summary>
        /// Get a Vision file
        /// </summary>
        /// <param name="filePath">Path of the file</param>
        /// <returns>Requested file</returns>
        public IVisionFile GetVisionFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            // Check for the file
            var pathToUse = LoadFullFilePath(filePath);
            if (string.IsNullOrEmpty(pathToUse) || 
                !File.Exists(pathToUse))
            {
                throw new FileNotFoundException($"File {filePath} not found", filePath);
            }

            // Look for the definition
            var fileName = Path.GetFileName(pathToUse).ToUpperInvariant();
            if (!FileDefinitions.ContainsKey(fileName))
            {
                throw new ApplicationException($"XFD not found for file {fileName}");
            }
            var definition = FileDefinitions[fileName];

            // Load the file
            return new MicrofocusVisionFile(definition, pathToUse, MicrofocusVisionLibrary);
        }


        /// <summary>
        /// Load the file path based on FILE_PREFIX environment variable
        /// </summary>
        /// <param name="filePath">Full file path</param>
        private string LoadFullFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (Path.IsPathFullyQualified(filePath)) return filePath;

            var filePrefix = Environment.GetEnvironmentVariable("FILE_PREFIX");
            if (string.IsNullOrEmpty(filePrefix)) 
            {
                filePrefix = Environment.CurrentDirectory;
            }

            foreach (var dir in filePrefix.Split(Path.PathSeparator))
            {
                var path = Path.Combine(dir, filePath);
                if (File.Exists(path)) return path;
            }

            return null;
        }


        /// <summary>
        /// Get the list of all file definitions
        /// </summary>
        /// <returns>List of files managed by the File System</returns>
        public IEnumerable<VisionFileDefinition> GetFileDefinitions()
        {
            return FileDefinitions.Values;
        }


        #region Initialize

        /// <summary>
        /// Initialize the system
        /// </summary>
        private void Initialize()
        {
            // Load license file
            var licenseFilePath = GetLicenseFilePath();
            MicrofocusVisionLibrary.SetLArgv0(licenseFilePath);

            // Initialize the runtime
            var initResult = MicrofocusVisionLibrary.V6_init();
            if (initResult == 0)
            {
                throw new ApplicationException($"Error initializing microfocus runtime.{Environment.NewLine}License file: {licenseFilePath}");
            }
        }

        
        /// <summary>
        /// Gets the license file name of the current process
        /// </summary>
        private string GetLicenseFilePath()
        {
            // Gets name from Environment
            var envLicensePath = Environment.GetEnvironmentVariable("VISION_LICENSE_FILE");
            if (!string.IsNullOrEmpty(envLicensePath) &&
                File.Exists(envLicensePath))
            {
                return envLicensePath;
            }

            // Current directory
            var currentFile = Path.Combine(Environment.CurrentDirectory, "vision.vlc");
            if (string.IsNullOrEmpty(currentFile) &&
                File.Exists(currentFile))
            {
                return currentFile;
            }
            
            // If not found, license file is missing
            var errorText = "Cannot find any Microfocus license file." + Environment.NewLine +
                            "VISION_LICENSE_FILE: " + envLicensePath + Environment.NewLine +
                            "Local license file: " + currentFile + Environment.NewLine;
            throw new ApplicationException(errorText);
        }


        /// <summary>
        /// Load all XFDs file
        /// </summary>
        private void LoadFileDefinitions()
        {
            var directory = Environment.GetEnvironmentVariable("XFD_DIRECTORY");
            if (string.IsNullOrEmpty(directory))
            {
                directory = Environment.CurrentDirectory;
            }

            if (Directory.Exists(directory))
            {
                var loader = new MicrofocusFileDefinitionLoader();
                foreach (var file in Directory.GetFiles(directory, "*.xfd"))
                {
                    try
                    {
                        var definition = loader.LoadFromXfd(file);
                        FileDefinitions.Add(definition.FileName.ToUpperInvariant(), definition);
                    }
                    catch { }
                }
            }
        }



        #endregion



        #region Dispose


        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (CurrentInstance.IsValueCreated)
                    {
                        MicrofocusVisionLibrary.V6_exit();
                    }
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                disposedValue = true;
            }
        }

        // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~VisionFileSystem()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        #endregion


    }

}