using Vision4GP.Core.Microfocus;

namespace Vision4GP.Core.FileSystem
{

    /// <summary>
    /// Vision file system
    /// </summary>
    public class VisionFileSystem
    {


        /// <summary>
        /// Cannot be initialized. Use GetInstance method instead
        /// </summary>
        private VisionFileSystem() {}



        /// <summary>
        /// Gets the instance of the Vision file system currently used
        /// </summary>
        /// <returns>Instance of the Vision file system</returns>
        public static IVisionFileSystem GetInstance()
        {
            return MicrofocusFileSystem.GetInstance();
        }

    }

}