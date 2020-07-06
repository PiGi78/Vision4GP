namespace Vision4GP.Core.FileSystem
{

    /// <summary>
    /// Factory for create a file definition
    /// </summary>
    public interface IVisionFileDefinitionFactory
    {
        
        /// <summary>
        /// Extract the file definition from XFD
        /// </summary>
        /// <param name="xfdFilePath">Path of the Xml File Definition file</param>
        /// <returns>Requested file definition</returns>
        VisionFileDefinition CreateFromXfd(string xfdFilePath);

    }
}