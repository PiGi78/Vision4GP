using System;

namespace Vision4GP.Core.FileSystem
{


    /// <summary>
    /// Data converter
    /// </summary>
    public interface IDataConverter
    {


        /// <summary>
        /// Gets an empty record content
        /// </summary>
        /// <param name="fileDefinition">File definition</param>
        /// <returns>Content of the empty record</returns>
        byte[] GetEmptyRecordContent(VisionFileDefinition fileDefinition);


        /// <summary>
        /// Gets a field value from the record
        /// </summary>
        /// <param name="field">The field you're asking the value</param>
        /// <param name="record">Record from where extract data</param>
        /// <returns>Requested data</returns>
        T? GetValue<T>(VisionFieldDefinition field, Span<byte> record);


        /// <summary>
        /// Sets a field value in the record
        /// </summary>
        /// <param name="field">The field you want to set the value</param>
        /// <param name="record">Record where put data</param>
        /// <param name="value">Value to set</param>
        void SetValue<T>(VisionFieldDefinition field, Span<byte> record, T? value);
    }
}