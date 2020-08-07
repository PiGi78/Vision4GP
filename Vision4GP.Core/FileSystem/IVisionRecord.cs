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
        byte[] GetRawContent();


        /// <summary>
        /// Set the new content of the record
        /// </summary>
        /// <param name="newContent">New content</param>
        void SetRawContent(byte[] newContent);
        
        
        /// <summary>
        /// Gets the value of a property of type string
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Value of the property</returns>
        string GetStringValue(string propertyName);


        /// <summary>
        /// Set the value of a string property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Property value</param>
        void SetStringValue(string propertyName, string value);


        
        /// <summary>
        /// Gets the value of a property of type int
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Value of the property</returns>
        int GetIntValue(string propertyName);


        /// <summary>
        /// Set the value of an int property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Property value</param>
        void SetIntValue(string propertyName, int value);


        /// <summary>
        /// Gets the value of a property of type long
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Value of the property</returns>
        long GetLongValue(string propertyName);


        /// <summary>
        /// Set the value of a long property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Property value</param>
        void SetLongValue(string propertyName, long value);


        
        /// <summary>
        /// Gets the value of a property of type decimal
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Value of the property</returns>
        decimal GetDecimalValue(string propertyName);


        /// <summary>
        /// Set the value of a decimal property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Property value</param>
        void SetDecimalValue(string propertyName, string value);


        
        /// <summary>
        /// Gets the value of a property of type date
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Value of the property</returns>
        DateTime? GetDateValue(string propertyName);


        /// <summary>
        /// Set the value of a date property
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="value">Property value</param>
        void SetDateValue(string propertyName, DateTime? value);

    }
}