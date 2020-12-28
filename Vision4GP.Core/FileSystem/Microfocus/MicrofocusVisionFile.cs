using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading;
using Vision4GP.Core.FileSystem;

namespace Vision4GP.Core.Microfocus
{


    /// <summary>
    /// Vision file for Microfocus
    /// </summary>
    internal class MicrofocusVisionFile : IVisionFile
    {


        /// <summary>
        /// Creates a new instance of a Microfocus vision file
        /// </summary>
        /// <param name="fileDefinition">Definition of the file</param>
        /// <param name="filePath">File path</param>
        /// <param name="visionLibrary">Library for manage vision file</param>
        internal MicrofocusVisionFile(VisionFileDefinition fileDefinition, string filePath, IMicrofocusVisionLibrary visionLibrary)
        {
            FileDefinition = fileDefinition ?? throw new ArgumentNullException(nameof(fileDefinition));
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            VisionLibrary = visionLibrary ?? throw new ArgumentNullException(nameof(visionLibrary));
            DataConverter = new MicrofocusDataConverter(FileDefinition);
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
        /// File path
        /// </summary>
        public string FilePath { get; }


        /// <summary>
        /// Library to work with vision file
        /// </summary>
        /// <value></value>
        private IMicrofocusVisionLibrary VisionLibrary { get; }


        /// <summary>
        /// File pointer
        /// </summary>
        private IntPtr FilePointer { get; set; } = IntPtr.Zero;


        /// <summary>
        /// True if the file is open
        /// </summary>
        private bool IsOpen => FilePointer != IntPtr.Zero;


        /// <summary>
        /// How the file was open
        /// </summary>
        /// <remarks>
        /// It is used for check if the file can be wrote
        /// </remarks>
        private FileOpenMode? CurrentOpenMode { get; set; }


        /// <summary>
        /// Get an empty record
        /// </summary>
        public IVisionRecord GetNewRecord()
        {
            return new MicrofocusVisionRecord(FileDefinition, DataConverter);
        }

        /// <summary>
        /// Opens a file
        /// </summary>
        /// <param name="mode">File open mode</param>
        public void Open(FileOpenMode mode) 
        {
            if (IsOpen) throw new IOException($"File {FilePath} is already opened");

            if (!File.Exists(FilePath)) throw new FileNotFoundException("Cannot open the file", FilePath);

            var microfocusResult = VisionLibrary.V6_open(FilePath, (int)mode);

            // Open sucessful
            if (microfocusResult.StatusCode.IsOkStatus() &&
                microfocusResult.Result != IntPtr.Zero)
            {
                FilePointer = microfocusResult.Result;
                CurrentOpenMode = mode;
                return;
            }

            // Error
            throw new VisionFileException((int)microfocusResult.StatusCode, $"Error {(int)microfocusResult.StatusCode} opening file {FilePath}");
        }


        /// <summary>
        /// Set a pointer for the start
        /// </summary>
        /// <param name="keyIndex">Index of the key to use</param>
        /// <param name="record">Record to use for start (null = start from the beginning)</param>
        /// <param name="mode">Vision start mode</param>
        /// <returns>True if the starts is ok, otherwise false</returns>
        public bool Start(int keyIndex = 0, IVisionRecord record = null, FileStartMode mode = FileStartMode.GreaterOrEqual)
        {
            if (!IsOpen) throw new IOException($"File not opened. File name: {FilePath}");
            EnsureKeyIndexIsValid(keyIndex);

            var recordToUse = record ?? new MicrofocusVisionRecord(FileDefinition, DataConverter);

            var microfocusResult = VisionLibrary.V6_start(FilePointer, recordToUse.GetRawContent(), keyIndex, 0, (int)mode);

            if (microfocusResult.StatusCode.IsOkStatus()) return true;
            if (microfocusResult.StatusCode == MicrofocusFileStatusCodes.NotFound) return false;

            throw new VisionFileException((int)microfocusResult.StatusCode, $"Error {(int)microfocusResult.StatusCode} on start on file {FilePath}");
        }


        /// <summary>
        /// Ensures that the index of the key is valid
        /// </summary>
        /// <param name="keyIndex">Index to validate</param>
        private void EnsureKeyIndexIsValid(int keyIndex)
        {
            if (keyIndex >= FileDefinition.NumberOfKeys) 
            {
                throw new ArgumentOutOfRangeException(nameof(keyIndex), 
                                                      $"The key index must be between {0} and {FileDefinition.NumberOfKeys - 1}");
            }
        }


        /// <summary>
        /// Read the next record without lock
        /// </summary>
        /// <returns>Next record or null if no more records</returns>
        public IVisionRecord ReadNext()
        {
            if (!IsOpen) throw new IOException($"File not opened. File name: {FilePath}");

            var content = new byte[FileDefinition.MaxRecordSize];

            var microfocusResult = VisionLibrary.V6_next(FilePointer, content, withLock: false);

            // OK
            if (microfocusResult.StatusCode.IsOkStatus())
            {
                var result = new MicrofocusVisionRecord(FileDefinition, DataConverter);
                result.SetRawContent(content);
                return result;
            }

            // NOT FOUND
            if (microfocusResult.StatusCode == MicrofocusFileStatusCodes.NotFound) return null;

            // Error
            throw new VisionFileException((int)microfocusResult.StatusCode, $"Error {(int)microfocusResult.StatusCode} on read next on file {FilePath}");
        }


        /// <summary>
        /// Read the next record with lock
        /// </summary>
        /// <returns>Next record or null if no more records</returns>
        public IVisionRecord ReadNextLock()
        {
            if (!IsOpen) throw new IOException($"File not opened. File name: {FilePath}");
            if (CurrentOpenMode == FileOpenMode.Input) throw new IOException($"File {FilePath} cannot be locked since it was open in read-only mode");

            var content = new byte[FileDefinition.MaxRecordSize];


            var microfocusResult = VisionLibrary.V6_next(FilePointer, content, withLock: true);

            // OK
            if (microfocusResult.StatusCode.IsOkStatus())
            {
                var result = new MicrofocusVisionRecord(FileDefinition, DataConverter);
                result.SetRawContent(content);
                return result;
            }

            // NOT FOUND
            if (microfocusResult.StatusCode == MicrofocusFileStatusCodes.NotFound) return null;

            // Error
            throw new VisionFileException((int)microfocusResult.StatusCode, $"Error {(int)microfocusResult.StatusCode} on read next lock on file {FilePath}");
        
        }


        /// <summary>
        /// Read the previous record without lock
        /// </summary>
        /// <returns>Previous record or null if no more records</returns>
        public IVisionRecord ReadPrevious()
        {
            if (!IsOpen) throw new IOException($"File not opened. File name: {FilePath}");
            
            var content = new byte[FileDefinition.MaxRecordSize];


            var microfocusResult = VisionLibrary.V6_previous(FilePointer, content, withLock: false);

            // OK
            if (microfocusResult.StatusCode.IsOkStatus())
            {
                var result = new MicrofocusVisionRecord(FileDefinition, DataConverter);
                result.SetRawContent(content);
                return result;
            }

            // NOT FOUND
            if (microfocusResult.StatusCode == MicrofocusFileStatusCodes.NotFound) return null;

            // Error
            throw new VisionFileException((int)microfocusResult.StatusCode, $"Error {(int)microfocusResult.StatusCode} on read previous on file {FilePath}");
        }


        /// <summary>
        /// Read the previous record with lock
        /// </summary>
        /// <returns>Previous record or null if no more records</returns>
        public IVisionRecord ReadPreviousLock()
        {
            if (!IsOpen) throw new IOException($"File not opened. File name: {FilePath}");
            if (CurrentOpenMode == FileOpenMode.Input) throw new IOException($"File {FilePath} cannot be locked since it was open in read-only mode");

            var content = new byte[FileDefinition.MaxRecordSize];


            var microfocusResult = VisionLibrary.V6_previous(FilePointer, content, withLock: true);

            // OK
            if (microfocusResult.StatusCode.IsOkStatus())
            {
                var result = new MicrofocusVisionRecord(FileDefinition, DataConverter);
                result.SetRawContent(content);
                return result;
            }

            // NOT FOUND
            if (microfocusResult.StatusCode == MicrofocusFileStatusCodes.NotFound) return null;

            // Error
            throw new VisionFileException((int)microfocusResult.StatusCode, $"Error {(int)microfocusResult.StatusCode} on read previous lock on file {FilePath}");
        }


        /// <summary>
        /// Read a record with lock
        /// </summary>
        /// <param name="keyValue">Value of the key</param>
        /// <param name="keyIndex">Index of the key</param>
        /// <returns>Locked record, null if not found</returns>
        public IVisionRecord ReadLock(IVisionRecord keyValue, int keyIndex = 0)
        {
            if (!IsOpen) throw new IOException($"File not opened. File name: {FilePath}");
            if (CurrentOpenMode == FileOpenMode.Input) throw new IOException($"File {FilePath} cannot be locked since it was open in read-only mode");
            EnsureKeyIndexIsValid(keyIndex);

            var content = new byte[FileDefinition.MaxRecordSize];
            Array.Copy(keyValue.GetRawContent(), content, keyValue.GetRawContent().Length);

            var microfocusResult = VisionLibrary.V6_read(FilePointer, content, keyIndex, withLock: true);

            // OK
            if (microfocusResult.StatusCode.IsOkStatus())
            {
                var result = new MicrofocusVisionRecord(FileDefinition, DataConverter);
                result.SetRawContent(content);
                return result;
            }

            // NOT FOUND
            if (microfocusResult.StatusCode == MicrofocusFileStatusCodes.NotFound) return null;

            // Error
            throw new VisionFileException((int)microfocusResult.StatusCode, $"Error {(int)microfocusResult.StatusCode} on read lock on file {FilePath}");
        }


        /// <summary>
        /// Read a record without lock
        /// </summary>
        /// <param name="keyValue">Value of the key</param>
        /// <param name="keyIndex">Index of the key</param>
        /// <returns>readed record, null if not found</returns>
        public IVisionRecord Read(IVisionRecord keyValue, int keyIndex = 0)
        {
            if (!IsOpen) throw new IOException($"File not opened. File name: {FilePath}");
            EnsureKeyIndexIsValid(keyIndex);

            var content = new byte[FileDefinition.MaxRecordSize];
            Array.Copy(keyValue.GetRawContent(), content, keyValue.GetRawContent().Length);

            var microfocusResult = VisionLibrary.V6_read(FilePointer, content, keyIndex, withLock: false);

            // OK
            if (microfocusResult.StatusCode.IsOkStatus())
            {
                var result = new MicrofocusVisionRecord(FileDefinition, DataConverter);
                result.SetRawContent(content);
                return result;
            }

            // NOT FOUND
            if (microfocusResult.StatusCode == MicrofocusFileStatusCodes.NotFound) return null;

            // Error
            throw new VisionFileException((int)microfocusResult.StatusCode, $"Error {(int)microfocusResult.StatusCode} on read on file {FilePath}");
        }


        /// <summary>
        /// Unlock the last locked record
        /// </summary>
        public void Unlock()
        {
            if (!IsOpen) throw new IOException($"File not opened. File name: {FilePath}");
            VisionLibrary.V6_unlock(FilePointer);
        }


        /// <summary>
        /// Insert of a new record
        /// </summary>
        /// <param name="record">Record to insert</param>
        public void Write(IVisionRecord record)
        {
            if (!IsOpen) throw new IOException($"File not opened. File name: {FilePath}");
            if (CurrentOpenMode == FileOpenMode.Input) throw new IOException($"File {FilePath} cannot be writed since it was open in read-only mode");

            var microfocusResult = VisionLibrary.V6_write(FilePointer, record.GetRawContent(), FileDefinition.MaxRecordSize);

            if (!microfocusResult.StatusCode.IsOkStatus())
            {
                throw new VisionFileException((int)microfocusResult.StatusCode, $"Error {(int)microfocusResult.StatusCode} on write on file {FilePath}");  
            }
        }


        /// <summary>
        /// Update a record
        /// </summary>
        /// <param name="record">Record to update</param>
        public void Rewrite(IVisionRecord record)
        {
            if (!IsOpen) throw new IOException($"File not opened. File name: {FilePath}");
            if (CurrentOpenMode == FileOpenMode.Input) throw new IOException($"File {FilePath} cannot be rewrited since it was open in read-only mode");

            var microfocusResult = VisionLibrary.V6_rewrite(FilePointer, record.GetRawContent(), FileDefinition.MaxRecordSize);

            if (!microfocusResult.StatusCode.IsOkStatus())
            {
                throw new VisionFileException((int)microfocusResult.StatusCode, $"Error {(int)microfocusResult.StatusCode} on rewrite on file {FilePath}");  
            }
        }


        /// <summary>
        /// Delete e record
        /// </summary>
        /// <param name="record">Record to delete</param>
        public void Delete(IVisionRecord record)
        {
            if (!IsOpen) throw new IOException($"File not opened. File name: {FilePath}");
            if (CurrentOpenMode == FileOpenMode.Input) throw new IOException($"File {FilePath} cannot be deleted since it was open in read-only mode");

            var microfocusResult = VisionLibrary.V6_delete(FilePointer, record.GetRawContent());

            if (!microfocusResult.StatusCode.IsOkStatus())
            {
                throw new VisionFileException((int)microfocusResult.StatusCode, $"Error {(int)microfocusResult.StatusCode} on delete on file {FilePath}");  
            }
        }


        /// <summary>
        /// Closes the file
        /// </summary>
        public void Close()
        {
            if (IsOpen)
            {
                var microfocusResult = VisionLibrary.V6_close(FilePointer);
                if (!microfocusResult.StatusCode.IsOkStatus())
                {
                    throw new VisionFileException((int)microfocusResult.StatusCode, $"Error {(int)microfocusResult.StatusCode} closing file {FilePath}");
                }
                CurrentOpenMode = null;
                FilePointer = IntPtr.Zero;
            }
        }


        #region Dispose


        private bool disposedValue;


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        Close();
                    }
                    catch {}
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MicrofocusVisionFile()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }


        #endregion


    }



}