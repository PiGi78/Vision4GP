using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Vision4GP.Core.FileSystem
{

    /// <summary>
    /// Vision file
    /// </summary>
    public interface IVisionFile : IDisposable
    {

        /// <summary>
        /// Get a new record
        /// </summary>
        IVisionRecord GetNewRecord();

        /// <summary>
        /// Opens a file
        /// </summary>
        /// <param name="mode">File open mode</param>
        void Open(FileOpenMode mode);


        /// <summary>
        /// Set a pointer for the start
        /// </summary>
        /// <param name="keyIndex">Index of the key to use</param>
        /// <param name="record">Record to use for start (null = start from the beginning)</param>
        /// <param name="mode">Vision start mode</param>
        /// <returns>True if the starts is ok, otherwise false</returns>
        bool Start(int keyIndex = 0, IVisionRecord record = null, FileStartMode mode = FileStartMode.GreaterOrEqual);


        /// <summary>
        /// Read the next record without lock
        /// </summary>
        /// <returns>Next record or null if no more records</returns>
        IVisionRecord ReadNext();


        /// <summary>
        /// Read the next record with lock
        /// </summary>
        /// <returns>Next record or null if no more records</returns>
        IVisionRecord ReadNextLock();


        /// <summary>
        /// Read the previous record without lock
        /// </summary>
        /// <returns>Previous record or null if no more records</returns>
        IVisionRecord ReadPrevious();


        /// <summary>
        /// Read the previous record with lock
        /// </summary>
        /// <returns>Previous record or null if no more records</returns>
        IVisionRecord ReadPreviousLock();


        /// <summary>
        /// Read a record without lock
        /// </summary>
        /// <param name="keyValue">Value of the key</param>
        /// <param name="keyIndex">Index of the key</param>
        /// <returns>Locked record, null if not found</returns>
        IVisionRecord Read(IVisionRecord keyValue, int keyIndex = 0);


        /// <summary>
        /// Read a record with lock
        /// </summary>
        /// <param name="keyValue">Value of the key</param>
        /// <param name="keyIndex">Index of the key</param>
        /// <returns>Locked record, null if not found</returns>
        IVisionRecord ReadLock(IVisionRecord keyValue, int keyIndex = 0);


        /// <summary>
        /// Unlock the last locked record
        /// </summary>
        void Unlock();


        /// <summary>
        /// Insert of a new record
        /// </summary>
        /// <param name="record">Record to insert</param>
        void Write(IVisionRecord record);


        /// <summary>
        /// Update a record
        /// </summary>
        /// <param name="record">Record to update</param>
        void Rewrite(IVisionRecord record);


        /// <summary>
        /// Delete e record
        /// </summary>
        /// <param name="record">Record to delete</param>
        void Delete(IVisionRecord record);


        /// <summary>
        /// Closes the file
        /// </summary>
        void Close();

        

    }
}