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
        /// Initialize the system
        /// </summary>
        /// <param name="configurationFiles">Configuration files (optional)</param>
        void Initialize(List<string> configurationFiles = null);

    }
}