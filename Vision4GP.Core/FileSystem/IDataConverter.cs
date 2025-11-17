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
        /// <returns>Content of the empty record</returns>
        byte[] GetEmptyRecordContent();


        /// <summary>
        /// Gets a field value from the record
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="record">Record from where extract data</param>
        /// <returns>Requested data</returns>
        T? GetValue<T>(string fieldName, Span<byte> record);


        /// <summary>
        /// Sets a field value in the record
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="record">Record where put data</param>
        /// <param name="value">Value to set</param>
        void SetValue<T>(string fieldName, Span<byte> record, T? value);
    }
}