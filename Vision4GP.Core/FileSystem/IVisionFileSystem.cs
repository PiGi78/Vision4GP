using System;
using System.Collections.Generic;

namespace Vision4GP.Core.FileSystem
{

    /// <summary>
    /// Vision file system
    /// </summary>
    public interface IVisionFileSystem : IDisposable
    {

        /// <summary>
        /// Get a Vision file
        /// </summary>
        /// <param name="filePath">Path of the file</param>
        /// <returns>Requested file</returns>
        IVisionFile GetVisionFile(string filePath);


        /// <summary>
        /// Get the list of all file definitions
        /// </summary>
        /// <returns>List of files managed by the File System</returns>
        IEnumerable<VisionFileDefinition> GetFileDefinitions();
    }
}