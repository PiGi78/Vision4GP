using System;
using Vision4GP.Core.FileSystem;

namespace Vision4GP.Core.Microfocus
{

    /// <summary>
    /// Vision record with microfocus rules
    /// </summary>
    internal class MicrofocusVisionRecord : IVisionRecord
    {

        /// <summary>
        /// Creates a new instance of Microfocus Vision record
        /// </summary>
        /// <param name="fileDefinition">Definition of the file</param>
        /// <param name="dataConverter">Data converter between byte array (raw data) and .NET types</param>
        internal MicrofocusVisionRecord(VisionFileDefinition fileDefinition, IDataConverter dataConverter)
        {
            FileDefinition = fileDefinition ?? throw new ArgumentNullException(nameof(fileDefinition));
            DataConverter = dataConverter ?? throw new ArgumentNullException(nameof(dataConverter));
            RawContent = DataConverter.GetEmptyRecordContent();
            rawContent = dataConverter.GetEmptyRecordContent();
        }


        private byte[] rawContent;


        /// <summary>
        /// Raw content of the record
        /// </summary>
        public Span<byte> RawContent
        {
            get
            {
                return rawContent.AsSpan();
            }
            set
            {
                rawContent = value.ToArray();
            }
        }


        /// <summary>
        /// File definition
        /// </summary>
        private VisionFileDefinition FileDefinition { get; }


        /// <summary>
        /// Data converter
        /// </summary>
        private IDataConverter DataConverter { get; }


        /// <summary>
        /// Clone the recod
        /// </summary>
        /// <returns>Cloned record</returns>
        public object Clone()
        {
            var clone = new MicrofocusVisionRecord(FileDefinition, DataConverter);
            var newContent = new byte[FileDefinition.MaxRecordSize];
            RawContent.CopyTo(newContent);
            clone.RawContent = newContent;
            return clone;
        }

        /// <summary>
        /// Retrieves the value of the specified property, converted to the requested type.
        /// </summary>
        /// <remarks>If the requested type is not supported, the method returns the default value of T.
        /// This method does not throw an exception for unsupported types or conversion failures; instead, it returns
        /// the default value. Property names are case-sensitive.</remarks>
        /// <typeparam name="T">The type to which the property value should be converted. Supported types include int, long, decimal,
        /// string, DateTime, and DateOnly, as well as their nullable counterparts.</typeparam>
        /// <param name="propertyName">The name of the property whose value is to be retrieved. Cannot be null or empty.</param>
        /// <returns>The value of the specified property converted to type T, or the default value of T if the property is not
        /// found or cannot be converted.</returns>
        public T? GetPropertyValue<T>(string propertyName)
        {
            return DataConverter.GetValue<T>(propertyName, RawContent);
        }


        /// <summary>
        /// Sets the value of the specified property to the provided value.
        /// </summary>
        /// <typeparam name="T">The type of the property value to set.</typeparam>
        /// <param name="propertyName">The name of the property whose value will be set. Cannot be null or empty.</param>
        /// <param name="value">The value to assign to the property. May be null for reference types or nullable value types.</param>
        public void SetPropertyValue<T>(string propertyName, T? value)
        {
            DataConverter.SetValue<T>(propertyName, RawContent, value);
        }
    }
}