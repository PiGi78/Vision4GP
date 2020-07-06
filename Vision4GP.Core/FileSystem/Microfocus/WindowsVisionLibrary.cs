using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Vision4GP.Core.FileSystem.Microfocus
{

    /// <summary>
    /// Microfocus vision library
    /// </summary>
    internal partial class MicrofocusVisionLibrary
    {

        /// <summary>
        /// Microfocus vision library for Windows
        /// </summary>
        private class WindowsVisionLibrary : IMicrofocusVisionLibrary
        {
            /// <summary>
            /// Object for synchronize the file operations
            /// </summary>
            /// <remarks>
            /// Needed because Library is the same for all the process.
            /// So if the same process uses many thread (es: web server),
            /// we need to be sure that the status code is readed just after
            /// the operation it refers
            /// </remarks>
            private static readonly object SyncObj = new object();

            #region Microfocus Windows library call



            private const string MICROFOCUST_VISION_DLL = "avision6.dll";


            private const string MICROFOCUST_ACME_DLL = "acme.dll";


            [DllImport(MICROFOCUST_ACME_DLL, EntryPoint = "Astdlib_f_errno")]
            private static extern IntPtr MicrofocusAstdlibFErrno();

            [DllImport(MICROFOCUST_ACME_DLL, EntryPoint = "Astdlib_f_no_lock")]
            private static extern IntPtr MicrofocusAstdlibFNoLock();

            [DllImport(MICROFOCUST_ACME_DLL, EntryPoint = "ACLoadFile")]
            private static extern int MicrofocusACLoadFile(string filePath, int unused0, int unused1, int unused2);

            [DllImport(MICROFOCUST_ACME_DLL, EntryPoint = "SetLArgv0")]
            private static extern void MicrofocusSetLArgv0(string licenseFileName);




            [DllImport(MICROFOCUST_VISION_DLL, EntryPoint = "v6_init")]
            private static extern int MicrofocusV6Init();

            [DllImport(MICROFOCUST_VISION_DLL, EntryPoint = "v6_exit")]
            private static extern void MicrofocusV6Exit();

            [DllImport(MICROFOCUST_VISION_DLL, EntryPoint = "v6_open")]
            private static extern IntPtr MicrofocusV6Open(string fileName, int mode, string unused0);

            [DllImport(MICROFOCUST_VISION_DLL, EntryPoint = "v6_next")]
            private static extern uint MicrofocusV6Next(IntPtr filePointer, byte[] record);

            [DllImport(MICROFOCUST_VISION_DLL, EntryPoint = "v6_previous")]
            private static extern uint MicrofocusV6Previous(IntPtr filePointer, byte[] record);

            [DllImport(MICROFOCUST_VISION_DLL, EntryPoint = "v6_read")]
            private static extern uint MicrofocusV6Read(IntPtr filePointer, byte[] record, int keyNum);

            [DllImport(MICROFOCUST_VISION_DLL, EntryPoint = "v6_close")]
            private static extern int MicrofocusV6Close(IntPtr filePointer);

            [DllImport(MICROFOCUST_VISION_DLL, EntryPoint = "v6_start")]
            private static extern int MicrofocusV6Start(IntPtr filePointer, byte[] keyValue, int keyNum, int keySize,
                int mode);

            [DllImport(MICROFOCUST_VISION_DLL, EntryPoint = "v6_write")]
            private static extern int MicrofocusV6Write(IntPtr filePointer, byte[] record, int recordSize);

            [DllImport(MICROFOCUST_VISION_DLL, EntryPoint = "v6_rewrite")]
            private static extern int MicrofocusV6Rewrite(IntPtr filePointer, byte[] record, int recordSize);

            [DllImport(MICROFOCUST_VISION_DLL, EntryPoint = "v6_delete")]
            private static extern int MicrofocusV6Delete(IntPtr filePointer, byte[] record);

            [DllImport(MICROFOCUST_VISION_DLL, EntryPoint = "v6_unlock")]
            private static extern int MicrofocusV6Unlock(IntPtr filePointer);

            [DllImport(MICROFOCUST_VISION_DLL, EntryPoint = "v6_info")]
            private static extern int MicrofocusV6Info(IntPtr filePointer, int mode, byte[] result);

            #endregion


            #region Runtime

            /// <summary>
            /// Initializes the runtime
            /// </summary>
            /// <returns>Zero if there are any error, otherwise an undefinded number different from zero</returns>
            public int V6_init()
            {
                return MicrofocusV6Init();
            }

            /// <summary>
            /// Set license file name (es: ./microfocus.vlc)
            /// </summary>
            /// <param name="licenseFileName">License file name, usually the name of the program with vlc extension</param>
            public void SetLArgv0(string licenseFileName)
            {
                if (string.IsNullOrEmpty(licenseFileName)) throw new ArgumentNullException(nameof(licenseFileName));
                if (!File.Exists(licenseFileName)) throw new FileNotFoundException("File not found", licenseFileName);

                MicrofocusSetLArgv0(licenseFileName);
            }

            /// <summary>
            /// Loads settings from file
            /// </summary>
            /// <param name="filePath">Full path of the settings file</param>
            /// <returns>On success it will return 1, otherwise zero will returned</returns>
            public int AcLoadFile(string filePath)
            {
                if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
                if (!File.Exists(filePath)) throw new FileNotFoundException("File not found", filePath);

                return MicrofocusACLoadFile(filePath, 0, 0, 0);
            }

            /// <summary>
            /// Closes runtime
            /// </summary>
            public void V6_exit()
            {
                MicrofocusV6Exit();
            }

            #endregion





            /// <summary>
            /// Open a file
            /// </summary>
            /// <param name="filePath">Full file path</param>
            /// <param name="mode">Open mode</param>
            /// <returns>Pointer to the file</returns>
            public MicrofocusFilePointerResult V6_open(string filePath, int mode)
            {
                if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
                if (!File.Exists(filePath)) throw new FileNotFoundException("File not found", filePath);

                var result = new MicrofocusFilePointerResult();
                lock (SyncObj)
                {
                    result.Result = MicrofocusV6Open(filePath, mode, null);
                    result.StatusCode = GetLastOperationStatusCode();
                }
                return result;
            }

            /// <summary>
            /// Read next record from the file
            /// </summary>
            /// <param name="filePointer">File pointer</param>
            /// <param name="record">Record data (array char size must match the maximum file size)</param>
            /// <returns>Number of readed chars (zero when the end of the file is reached)</returns>
            public MicrofocusFileIntResult V6_next(IntPtr filePointer, byte[] record)
            {
                if (filePointer == IntPtr.Zero) throw new ArgumentOutOfRangeException("File pointer cannot be zero");
                if (record == null) throw new ArgumentNullException(nameof(record));

                var result = new MicrofocusFileIntResult();
                lock (SyncObj)
                {
                    result.Result = (int)MicrofocusV6Next(filePointer, record);
                    result.StatusCode = GetLastOperationStatusCode();
                }

                return result;
            }

            /// <summary>
            /// Read previous record from the file
            /// </summary>
            /// <param name="filePointer">File pointer</param>
            /// <param name="record">Record data (array char size must match the maximum file size)</param>
            /// <returns>Number of readed chars (zero when the begin of the file is reached)</returns>
            public MicrofocusFileIntResult V6_previous(IntPtr filePointer, byte[] record)
            {
                if (filePointer == IntPtr.Zero) throw new ArgumentOutOfRangeException("File pointer cannot be zero");
                if (record == null) throw new ArgumentNullException(nameof(record));

                var result = new MicrofocusFileIntResult();
                lock (SyncObj)
                {
                    result.Result = (int)MicrofocusV6Previous(filePointer, record);
                    result.StatusCode = GetLastOperationStatusCode();
                }
                return result;
            }

            /// <summary>
            /// Reads a record from the file
            /// </summary>
            /// <remarks>
            /// When file is open in IO, then the record will be locked. Otherwise no lock will be managed
            /// </remarks>
            /// <param name="filePointer">File pointer</param>
            /// <param name="record">Record data (array char size must match the maximum file size). In input there is the key to read, in output the full record</param>
            /// <param name="keyIndex">Key index to use (zero based)</param>
            /// <returns>Number of readed chars (zero if not found)</returns>
            public MicrofocusFileIntResult V6_read(IntPtr filePointer, byte[] record, int keyIndex)
            {
                if (filePointer == IntPtr.Zero) throw new ArgumentOutOfRangeException("File pointer cannot be zero");
                if (record == null) throw new ArgumentNullException(nameof(record));

                var result = new MicrofocusFileIntResult();
                lock (SyncObj)
                {
                    result.Result = (int)MicrofocusV6Read(filePointer, record, keyIndex);
                    result.StatusCode = GetLastOperationStatusCode();
                }
                return result;
            }

            /// <summary>
            /// Closes a file
            /// </summary>
            /// <param name="filePointer">File pointer</param>
            public MicrofocusFileIntResult V6_close(IntPtr filePointer)
            {
                if (filePointer == IntPtr.Zero) throw new ArgumentOutOfRangeException("File pointer cannot be zero");

                var result = new MicrofocusFileIntResult();
                lock (SyncObj)
                {
                    result.Result = MicrofocusV6Close(filePointer);
                    result.StatusCode = GetLastOperationStatusCode();
                }
                return result;
            }

            /// <summary>
            /// Executes a start to the given file
            /// </summary>
            /// <param name="filePointer">File pointer</param>
            /// <param name="record">Record that contains all key fields needed for the start</param>
            /// <param name="keyIndex">Key used for start (zero based)</param>
            /// <param name="keySize">Key size for the start (zero = use all the key)</param>
            /// <param name="mode">What kind of starts will be executed</param>
            public MicrofocusFileIntResult V6_start(IntPtr filePointer, byte[] record, int keyIndex, int keySize, int mode)
            {
                if (filePointer == IntPtr.Zero) throw new ArgumentOutOfRangeException("File pointer cannot be zero");
                if (record == null) throw new ArgumentNullException(nameof(record));

                // Eseguo l'operazione
                var result = new MicrofocusFileIntResult();
                lock (SyncObj)
                {
                    result.Result = MicrofocusV6Start(filePointer, record, keyIndex, keySize, mode);
                    result.StatusCode = GetLastOperationStatusCode();
                }
                return result;
            }

            /// <summary>
            /// Writes data into a file
            /// </summary>
            /// <param name="filePointer">File pointer</param>
            /// <param name="record">Record data</param>
            /// <param name="recordSize">Record size</param>
            /// <returns>Number of writed chars. If any error occurs, the number of writed char is zero and f_errno has the error code</returns>
            public MicrofocusFileIntResult V6_write(IntPtr filePointer, byte[] record, int recordSize)
            {
                if (filePointer == IntPtr.Zero) throw new ArgumentOutOfRangeException("File pointer cannot be zero");
                if (record == null) throw new ArgumentNullException(nameof(record));

                var result = new MicrofocusFileIntResult();
                lock (SyncObj)
                {
                    result.Result = MicrofocusV6Write(filePointer, record, recordSize);
                    result.StatusCode = GetLastOperationStatusCode();
                }
                return result;
            }

            /// <summary>
            /// Rewrite data into a file
            /// </summary>  
            /// <param name="filePointer">File pointer</param>
            /// <param name="record">Record data</param>
            /// <param name="recordSize">Record size</param>
            /// <returns>Number of writed chars. If any error occurs, the number of writed char is zero and f_errno has the error code</returns>
            public MicrofocusFileIntResult V6_rewrite(IntPtr filePointer, byte[] record, int recordSize)
            {
                if (filePointer == IntPtr.Zero) throw new ArgumentOutOfRangeException("File pointer cannot be zero");
                if (record == null) throw new ArgumentNullException(nameof(record));

                var result = new MicrofocusFileIntResult();
                lock (SyncObj)
                {
                    result.Result = MicrofocusV6Rewrite(filePointer, record, recordSize);
                    result.StatusCode = GetLastOperationStatusCode();
                }
                return result;
            }

            /// <summary>
            /// Delete data from file
            /// </summary>
            /// <param name="filePointer">File pointer</param>
            /// <param name="record">Record data</param>
            /// <returns>Number of deleted chars. If any error occurs, the number of writed char is zero and f_errno has the error code</returns>
            public MicrofocusFileIntResult V6_delete(IntPtr filePointer, byte[] record)
            {
                if (filePointer == IntPtr.Zero) throw new ArgumentOutOfRangeException("File pointer cannot be zero");
                if (record == null) throw new ArgumentNullException(nameof(record));

                var result = new MicrofocusFileIntResult();
                lock (SyncObj)
                {
                    result.Result = MicrofocusV6Delete(filePointer, record);
                    result.StatusCode = GetLastOperationStatusCode();
                }
                return result;
            }

            /// <summary>
            /// Unlocks any lock on the file
            /// </summary>
            /// <param name="filePointer">File pointer</param>
            public MicrofocusFileIntResult V6_unlock(IntPtr filePointer)
            {
                if (filePointer == IntPtr.Zero) throw new ArgumentOutOfRangeException("File pointer cannot be zero");
                
                var result = new MicrofocusFileIntResult();
                lock (SyncObj)
                {
                    result.Result = MicrofocusV6Unlock(filePointer);
                    result.StatusCode = GetLastOperationStatusCode();
                }
                return result;
            }


            /// <summary>
            /// Info about a file
            /// </summary>
            /// <param name="filePointer">File pointer</param>
            /// <param name="mode">Mode</param>
            /// <param name="result">Result</param>
            /// <returns>Result status</returns>
            public int V6_info(IntPtr filePointer, int mode, byte[] result)
            {
                if (filePointer == IntPtr.Zero) throw new ArgumentOutOfRangeException("File pointer cannot be zero");
                if (result == null) throw new ArgumentNullException(nameof(result));

                return MicrofocusV6Info(filePointer, mode, result);
            }


            #region Errors

            /// <summary>
            /// Gets the pointer to the Microfocus error code
            /// </summary>
            /// <returns>Pointer to the error code</returns>
            private IntPtr Astdlib_f_errno()
            {
                return MicrofocusAstdlibFErrno();
            }

            /// <summary>
            /// Gets the pointer to the Microfocus lock mode
            /// </summary>
            /// <returns>Pointer to the lock mode</returns>
            private IntPtr Astdlib_f_no_lock()
            {
                return MicrofocusAstdlibFNoLock();
            }

            /// <summary>
            /// Gets the status code of the last operation made on a file
            /// </summary>
            /// <returns><see cref="MicrofocusFileStatusCodes"/> of the last operation</returns>
            private MicrofocusFileStatusCodes GetLastOperationStatusCode()
            {
                var errPointer = Astdlib_f_errno();
                var result = MicrofocusFileStatusCodes.Ok;
                if (errPointer != IntPtr.Zero)
                {
                    var errValue = Marshal.ReadInt32(errPointer);
                    if (Enum.IsDefined(typeof(MicrofocusFileStatusCodes), errValue))
                    {
                        result = (MicrofocusFileStatusCodes)errValue;
                    }
                    else
                    {
                        result = MicrofocusFileStatusCodes.UnmanagedError;
                    }
                }
                return result;
            }

            #endregion

        }
    }
}