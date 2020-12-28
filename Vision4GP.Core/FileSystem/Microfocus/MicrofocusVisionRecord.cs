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
        internal MicrofocusVisionRecord(VisionFileDefinition fileDefinition, MicrofocusDataConverter dataConverter)
        {
            FileDefinition = fileDefinition ?? throw new ArgumentNullException(nameof(fileDefinition));
            DataConverter = dataConverter ?? throw new ArgumentNullException(nameof(dataConverter));
            RawContent = DataConverter.GetEmptyRecordContent();
        }


        /// <summary>
        /// File definition
        /// </summary>
        private VisionFileDefinition FileDefinition { get; }


        /// <summary>
        /// Data converter
        /// </summary>
        private MicrofocusDataConverter DataConverter { get; }


        /// <summary>
        /// Raw content of the record
        /// </summary>
        private byte[] RawContent { get; set; }


        /// <summary>
        /// Raw content of the record
        /// </summary>
        public byte[] GetRawContent()
        {
            return RawContent;
        }


        /// <summary>
        /// Set the content of the record
        /// </summary>
        /// <param name="rawContent">Record content</param>
        public void SetRawContent(byte[] rawContent)
        {
            if (rawContent == null) throw new ArgumentNullException(nameof(rawContent));
            if (rawContent.Length > FileDefinition.MaxRecordSize)
            {
                throw new ArgumentOutOfRangeException($"Maximum record size is of {FileDefinition.MaxRecordSize} bytes");
            }

            RawContent = rawContent;
        }

        
        /// <summary>
        /// Gets the value of a property of type string
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Value of the property</returns>
        public string GetStringValue(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            return DataConverter.GetStringValue(propertyName, RawContent);
        }


        /// <summary>
        /// Set the value of a string property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Property value</param>
        public void SetStringValue(string propertyName, string value)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            DataConverter.SetValue(propertyName, RawContent, value);
        }


        
        /// <summary>
        /// Gets the value of a property of type int
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Value of the property</returns>
        public int GetIntValue(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            return DataConverter.GetIntValue(propertyName, RawContent);
        }


        /// <summary>
        /// Set the value of an int property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Property value</param>
        public void SetIntValue(string propertyName, int value)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            DataConverter.SetValue(propertyName, RawContent, value);
        }


        /// <summary>
        /// Gets the value of a property of type long
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Value of the property</returns>
        public long GetLongValue(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            return DataConverter.GetLongValue(propertyName, RawContent);
        }


        /// <summary>
        /// Set the value of a long property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Property value</param>
        public void SetLongValue(string propertyName, long value)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            DataConverter.SetValue(propertyName, RawContent, value);
        }


        
        /// <summary>
        /// Gets the value of a property of type decimal
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Value of the property</returns>
        public decimal GetDecimalValue(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            return DataConverter.GetDecimalValue(propertyName, RawContent);
        }


        /// <summary>
        /// Set the value of a decimal property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Property value</param>
        public void SetDecimalValue(string propertyName, decimal value)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            DataConverter.SetValue(propertyName, RawContent, value);
        }


        
        /// <summary>
        /// Gets the value of a property of type date
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Value of the property</returns>
        public DateTime? GetDateValue(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            return DataConverter.GetDateValue(propertyName, RawContent);
        }


        /// <summary>
        /// Set the value of a date property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Property value</param>
        public void SetDateValue(string propertyName, DateTime? value)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            DataConverter.SetValue(propertyName, RawContent, value);
        }




        
        /// <summary>
        /// Gets the value of a property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Value of the property</returns>
        public object GetValue(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            return DataConverter.GetValue(propertyName, RawContent);
        }


        /// <summary>
        /// Set the value of  property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Property value</param>
        public void SetValue(string propertyName, object value)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            DataConverter.SetValue(propertyName, RawContent, value);
        }


        /// <summary>
        /// Clone the recod
        /// </summary>
        /// <returns>Cloned record</returns>
        public object Clone()
        {
            var clone = new MicrofocusVisionRecord(FileDefinition, DataConverter);
            var newContent = new byte[FileDefinition.MaxRecordSize];
            Array.Copy(RawContent, newContent, FileDefinition.MaxRecordSize);
            clone.SetRawContent(newContent);
            return clone;
        }
    }
}