using Microsoft.Extensions.Options;
using Vision4GP.Core.FileSystem;

namespace Vision4GP.Microfocus
{


    /// <summary>
    /// Vision file system
    /// </summary>
    public class MicrofocusFileSystem : IVisionFileSystem
    {


        /// <summary>
        /// Creates a new instance of Microfocus File System
        /// </summary>
        /// <param name="dataConverter">Data converter</param>
        public MicrofocusFileSystem(IOptions<MicrofocusSettings> settings, IDataConverter dataConverter)
        {
            ArgumentNullException.ThrowIfNull(dataConverter);
            ArgumentNullException.ThrowIfNull(settings);
            DataConverter = dataConverter;
            Settings = settings.Value;
        }

        /// <summary>
        /// Settings for Microfocus
        /// </summary>
        private MicrofocusSettings Settings { get; }


        /// <summary>
        /// DataConverter
        /// </summary>
        private IDataConverter DataConverter { get; }


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
            ArgumentNullException.ThrowIfNullOrEmpty(filePath);

            // Check for the file
            var pathToUse = LoadFullFilePath(filePath);

            // Look for the definition
            var fileName = Path.GetFileName(pathToUse)!.ToUpperInvariant();
            if (!FileDefinitions.ContainsKey(fileName))
            {
                throw new ApplicationException($"XFD not found for file {fileName}");
            }
            var definition = FileDefinitions[fileName];

            // Load the file
            return new MicrofocusVisionFile(definition, pathToUse, MicrofocusVisionLibrary.GetInstance(), DataConverter);
        }


        /// <summary>
        /// Load the file path based on FILE_PREFIX environment variable
        /// </summary>
        /// <param name="filePath">Full file path</param>
        private string LoadFullFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (Path.IsPathFullyQualified(filePath)) return filePath;

            var filePrefix = Settings.FilePrefix;
            if (string.IsNullOrEmpty(filePrefix)) 
            {
                filePrefix = Environment.CurrentDirectory;
            }

            string? firstPath = null;
            foreach (var dir in filePrefix.Split(Path.PathSeparator))
            {
                var path = Path.Combine(dir, filePath);
                if (firstPath == null) firstPath = path;
                if (File.Exists(path)) return path;
            }

            return firstPath!;
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
        public void Initialize()
        {
            // Get the library instance
            var library = MicrofocusVisionLibrary.GetInstance();

            // Load license file
            var licenseFilePath = GetLicenseFilePath();
            library.SetLArgv0(licenseFilePath);

            // Initialize the runtime
            var initResult = library.V6_init();
            if (initResult == 0)
            {
                throw new ApplicationException($"Error initializing microfocus runtime.{Environment.NewLine}License file: {licenseFilePath}");
            }

            // Loads file definitions
            LoadFileDefinitions();
        }

        
        /// <summary>
        /// Gets the license file name of the current process
        /// </summary>
        private string GetLicenseFilePath()
        {
            // Gets name from settings
            var settingLicensePath = Settings.LicenseFilePath;
            if (!string.IsNullOrEmpty(settingLicensePath) &&
                File.Exists(settingLicensePath))
            {
                return settingLicensePath;
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
                            "Settings: " + settingLicensePath + Environment.NewLine +
                            "Local license file: " + currentFile + Environment.NewLine;
            throw new ApplicationException(errorText);
        }


        /// <summary>
        /// Load all XFDs file
        /// </summary>
        private void LoadFileDefinitions()
        {
            var directory = Settings.XfdDirectory;
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
                        var definition = loader.LoadFromFile(file);
                        FileDefinitions.Add(definition.FileName.ToUpperInvariant(), definition);
                    }
                    catch { }
                }
            }
        }



        #endregion



        #region Dispose


        private bool disposedValue;

        /// <summary>
        /// Release all resources used by the instance
        /// </summary>
        /// <param name="disposing">True if called by dispose method</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    MicrofocusVisionLibrary.GetInstance().V6_exit();
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

        /// <summary>
        /// Release all resources used by the instance
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        #endregion


    }

}