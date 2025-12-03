using Vision4GP.Core.FileSystem;

namespace Vision4GP.Microfocus
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
        /// <param name="content">Content of the new record</param>
        internal MicrofocusVisionRecord(VisionFileDefinition fileDefinition, IDataConverter dataConverter, byte[]? content = null)
        {
            FileDefinition = fileDefinition ?? throw new ArgumentNullException(nameof(fileDefinition));
            DataConverter = dataConverter ?? throw new ArgumentNullException(nameof(dataConverter));
            RawContent = content ?? dataConverter.GetEmptyRecordContent(fileDefinition);
        }


        #region Extract field


        private Dictionary<string, VisionFieldDefinition> _fieldsCache = new Dictionary<string, VisionFieldDefinition>();

        /// <summary>
        /// Extract the field of the file with the given name
        /// </summary>
        /// <param name="fieldName">Name of the file</param>
        /// <returns>Requested field, throw ApplicationException if not found</returns>
        private VisionFieldDefinition GetField(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentNullException(nameof(fieldName));
            if (_fieldsCache.ContainsKey(fieldName)) return _fieldsCache[fieldName];
            var nameToCheck = fieldName.ToUpper().Replace("-", "_");
            foreach (var field in FileDefinition.Fields)
            {
                if (field.Name.ToUpper().Replace("-", "_") == nameToCheck)
                {
                    _fieldsCache[fieldName] = field;
                    return field;
                }
            }

            throw new ApplicationException($"Cannot find field {fieldName} in file {FileDefinition.FileName}");
        }




        #endregion



        /// <summary>
        /// Raw content of the record
        /// </summary>
        public byte[] RawContent { get; set; }

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
            var newContent = new byte[FileDefinition.MaxRecordSize];
            RawContent.CopyTo(newContent);
            return new MicrofocusVisionRecord(FileDefinition, DataConverter, newContent);
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
            return DataConverter.GetValue<T>(GetField(propertyName), RawContent);
        }


        /// <summary>
        /// Sets the value of the specified property to the provided value.
        /// </summary>
        /// <typeparam name="T">The type of the property value to set.</typeparam>
        /// <param name="propertyName">The name of the property whose value will be set. Cannot be null or empty.</param>
        /// <param name="value">The value to assign to the property. May be null for reference types or nullable value types.</param>
        public void SetPropertyValue<T>(string propertyName, T? value)
        {
            DataConverter.SetValue<T>(GetField(propertyName), RawContent, value);
        }
    }
}