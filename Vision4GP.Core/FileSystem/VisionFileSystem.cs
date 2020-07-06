using System;
using System.Collections.Generic;
using System.IO;
using Vision4GP.Core.FileSystem.Microfocus;

namespace Vision4GP.Core.FileSystem
{


    /// <summary>
    /// Vision file system
    /// </summary>
    public class VisionFileSystem : IVisionFileSystem
    {

        /// <summary>
        /// Cannot be initialized. Use GetInstance method instead
        /// </summary>
        private VisionFileSystem() {}



        /// <summary>
        /// Gets the instance of the Vision file system currently used
        /// </summary>
        /// <returns>Instance of the Vision file system</returns>
        public static VisionFileSystem GetInstance()
        {
            return CurrentInstance.Value;
        }


        private static Lazy<VisionFileSystem> CurrentInstance = new Lazy<VisionFileSystem>(() =>
        {
            return new VisionFileSystem();
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



        #region Initialize
        
        
        /// <summary>
        /// True if the Initialize method is already called
        /// </summary>
        private static bool IsInitialized { get; set; }

        /// <summary>
        /// Object for manage concurrently initializations
        /// </summary>
        private static object SyncInitObj { get; } = new object();

        /// <summary>
        /// Initialize the system
        /// </summary>
        /// <param name="configurationFiles">Configuration files (optional)</param>
        public void Initialize(List<string> configurationFiles = null)
        {
            if (!IsInitialized)
            {
                lock (SyncInitObj)
                {
                    if (!IsInitialized)
                    {
                        // Load license file
                        string licenseFile = GetLicenseFileName();
                        MicrofocusVisionLibrary.SetLArgv0(licenseFile);

                        // Load other configuration files
                        if (configurationFiles != null)
                        {
                            foreach (var configurationFile in configurationFiles)
                            {
                                MicrofocusVisionLibrary.AcLoadFile(configurationFile);
                            }
                        }

                        // Initialize the runtime
                        var initResult = MicrofocusVisionLibrary.V6_init();
                        if (initResult == 0)
                        {
                            throw new ApplicationException($"Error initializing legacy runtime.{Environment.NewLine}License file: {licenseFile}");
                        }
                        IsInitialized = true;
                    }
                }
            }

        }




        /// <summary>
        /// Gets the license file name for the current process
        /// </summary>
        private static string GetLicenseFileName()
        {
            string licenseFile = null;

            // Environment
            var envLicensePath = Environment.GetEnvironmentVariable("VISION_LICENSE_FILE");
            if (!string.IsNullOrEmpty(envLicensePath) &&
                File.Exists(envLicensePath))
            {
                licenseFile = envLicensePath;
            }

            // Current directory
            var currentFile = Path.Combine(Environment.CurrentDirectory, "vision.vlc");
            if (string.IsNullOrEmpty(licenseFile) &&
                File.Exists(currentFile))
            {
                licenseFile = currentFile;
            }

            // If not found, throw an exception
            if (string.IsNullOrEmpty(licenseFile))
            {
                var errorText = "Can't find any Microfocus license file. Set environment variable VISION_LICENSE_FILE or " +
                                "create a vision.vlc file in the current directory";
                throw new ApplicationException(errorText);
            }
            return licenseFile;

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
                    if (IsInitialized)
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