namespace Vision4GP.Microfocus
{

    /// <summary>
    /// Settings for Microfocus library
    /// </summary>
    public class MicrofocusSettings
    {

        /// <summary>
        /// File prefix
        /// </summary>
        /// <remarks>
        /// It's used to find the path of a file.
        /// If not specified, the program will look for the current directory
        /// </remarks>
        public string? FilePrefix { get; set; }


        /// <summary>
        /// Directory where to find the XFD files
        /// </summary>
        public string? XfdDirectory { get; set; }


        /// <summary>
        /// File where to find the Microfocus license file
        /// </summary>
        /// <remarks>
        /// If not specified, the program will try to find it in the current directory
        /// </remarks>
        public string? LicenseFilePath { get; set; }

    }
}
