using System;

namespace Vision4GP.Core.FileSystem
{

    /// <summary>
    /// Vision record
    /// </summary>
    public interface IVisionRecord : ICloneable
    {

        /// <summary>
        /// Raw content of the record
        /// </summary>
        Span<byte> RawContent { get; set; }


        /// <summary>
        /// Gets the value of a property by its name
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Property value</returns>
        T? GetPropertyValue<T>(string propertyName);


        /// <summary>
        /// Sets the value of a property by its name
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Property value</param>
        void SetPropertyValue<T>(string propertyName, T? value);
    }
}