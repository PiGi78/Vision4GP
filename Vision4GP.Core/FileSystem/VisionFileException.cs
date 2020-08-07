using System.IO;

namespace Vision4GP.Core.FileSystem
{

    /// <summary>
    /// Vision file exception
    /// </summary>
    public class VisionFileException : IOException
    {

        /// <summary>
        /// Creates a new instance of VisionFileException
        /// </summary>
        public VisionFileException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// File status code
        /// </summary>
        public int StatusCode { get; }

    }

}