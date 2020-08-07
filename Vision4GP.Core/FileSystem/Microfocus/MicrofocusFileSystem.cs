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
                throw new ApplicationException($"Error initializing legacy runtime.{Environment.NewLine}License file: {licenseFilePath}");
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