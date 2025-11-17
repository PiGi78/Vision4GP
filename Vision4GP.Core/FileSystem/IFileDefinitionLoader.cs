namespace Vision4GP.Core.FileSystem
{

    /// <summary>
    /// Loader of a file definition
    /// </summary>
    public interface IFileDefinitionLoader
    {

        /// <summary>
        /// Loads a file definition from a file
        /// </summary>
        /// <param name="filePath">Path of the file</param>
        /// <returns>Definition of the file</returns>
        VisionFileDefinition LoadFromFile(string filePath);
    }
}