using System;
using System.Runtime.InteropServices;

namespace Vision4GP.Core.Microfocus
{

    /// <summary>
    /// Library provided by Microfocus for vision file access
    /// </summary>
    /// <remarks>
    /// This is an internal class because who use the library has to reference it
    /// from the VisionFS.GetInstance method
    /// </remarks>
    internal partial class MicrofocusVisionLibrary
    {

        /// <summary>
        /// Never creates a new instance directly. Pass from the GetInstance method
        /// </summary>
        private MicrofocusVisionLibrary() { }
        

        /// <summary>
        /// Gets the current instance of the Microfocus vision library, depending on the underlying OS
        /// </summary>
        /// <returns>Microfocus vision library to use for the current OS</returns>
        public static IMicrofocusVisionLibrary GetInstance()
        {
            return CurrentInstance.Value;
        }


        /// <summary>
        /// Current instance
        /// </summary>
        private static readonly Lazy<IMicrofocusVisionLibrary> CurrentInstance = new Lazy<IMicrofocusVisionLibrary>(() =>
        {

            // Linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LinuxVisionLibrary();
            }

            // Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsVisionLibrary();
            }

            // Sistema operativo non supportato
            throw new NotSupportedException("Current OS is not supported");
        });

    }


    /// <summary>
    /// Microfocus vision library interfaces
    /// </summary>
    /// <remarks>
    /// Interface needed for use the right implementation depending on the underlying operating system.
    /// See the VisionFileSystemAPI documentation file to know how to use any method
    /// </remarks>
    internal interface IMicrofocusVisionLibrary
    {


        #region Runtime

        /// <summary>
        /// Initializes the runtime
        /// </summary>
        /// <returns>Zero if there are any error, otherwise an undefinded number different from zero</returns>
        int V6_init();


        /// <summary>
        /// Set license file name (es: ./microfocus.vlc)
        /// </summary>
        /// <param name="licenseFileName">License file name, usually the name of the program with vlc extension</param>
        void SetLArgv0(string licenseFileName);


        /// <summary>
        /// Loads settings from file
        /// </summary>
        /// <param name="filePath">Full path of the settings file</param>
        /// <returns>On success it will return 1, otherwise zero will returned</returns>
        int AcLoadFile(string filePath);


        /// <summary>
        /// Closes runtime
        /// </summary>
        void V6_exit();


        #endregion
        


        #region File operations


        /// <summary>
        /// Creates a new file
        /// </summary>
        /// <param name="fileName">Path of the file</param>
        /// <param name="l_params">Describes various logical caharacteristics of the file</param>
        /// <param name="keys">Describes the key structure of the file</param>
        /// <returns></returns>
        MicrofocusFileIntResult V6_make(string fileName, string l_params, string keys);

        /// <summary>
        /// Open a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="mode">Open mode</param>
        /// <returns>Pointer to the file</returns>
        MicrofocusFilePointerResult V6_open(string fileName, int mode);



        /// <summary>
        /// Read next record from the file
        /// </summary>
        /// <param name="filePointer">File pointer</param>
        /// <param name="record">Record data (array char size must match the maximum file size)</param>
        /// <param name="withLock">True for lock record</param>
        /// <returns>Number of readed chars (zero when the end of the file is reached)</returns>
        MicrofocusFileIntResult V6_next(IntPtr filePointer, byte[] record, bool withLock = false);


        /// <summary>
        /// Read previous record from the file
        /// </summary>
        /// <param name="filePointer">File pointer</param>
        /// <param name="record">Record data (array char size must match the maximum file size)</param>
        /// <param name="withLock">True for lock record</param>
        /// <returns>Number of readed chars (zero when the begin of the file is reached)</returns>
        MicrofocusFileIntResult V6_previous(IntPtr filePointer, byte[] record, bool withLock = false);


        /// <summary>
        /// Reads a record from the file
        /// </summary>
        /// <remarks>
        /// When file is open in IO, then the record will be locked. Otherwise no lock will be managed
        /// </remarks>
        /// <param name="filePointer">File pointer</param>
        /// <param name="record">Record data (array char size must match the maximum file size). In input there is the key to read, in output the full record</param>
        /// <param name="keyIndex">Key index to use (zero based)</param>
        /// <param name="withLock">True for lock record</param>
        /// <returns>Number of readed chars (zero if not found)</returns>
        MicrofocusFileIntResult V6_read(IntPtr filePointer, byte[] record, int keyIndex, bool withLock = false);


        /// <summary>
        /// Closes a file
        /// </summary>
        /// <param name="filePointer">File pointer</param>
        MicrofocusFileIntResult V6_close(IntPtr filePointer);


        /// <summary>
        /// Executes a start to the given file
        /// </summary>
        /// <param name="filePointer">File pointer</param>
        /// <param name="record">Record that contains all key fields needed for the start</param>
        /// <param name="keyIndex">Key used for start (zero based)</param>
        /// <param name="keySize">Key size for the start (zero = use all the key)</param>
        /// <param name="mode">What kind of starts will be executed</param>
        MicrofocusFileIntResult V6_start(IntPtr filePointer, byte[] record, int keyIndex, int keySize, int mode);


        /// <summary>
        /// Writes data into a file
        /// </summary>
        /// <param name="filePointer">File pointer</param>
        /// <param name="record">Record data</param>
        /// <param name="recordSize">Record size</param>
        /// <returns>Number of writed chars. If any error occurs, the number of writed char is zero and f_errno has the error code</returns>
        MicrofocusFileIntResult V6_write(IntPtr filePointer, byte[] record, int recordSize);


        /// <summary>
        /// Rewrite data into a file
        /// </summary>
        /// <param name="filePointer">File pointer</param>
        /// <param name="record">Record data</param>
        /// <param name="recordSize">Record size</param>
        /// <returns>Number of writed chars. If any error occurs, the number of writed char is zero and f_errno has the error code</returns>
        MicrofocusFileIntResult V6_rewrite(IntPtr filePointer, byte[] record, int recordSize);


        /// <summary>
        /// Delete data from file
        /// </summary>
        /// <param name="filePointer">File pointer</param>
        /// <param name="record">Record data</param>
        /// <returns>Number of deleted chars. If any error occurs, the number of writed char is zero and f_errno has the error code</returns>
        MicrofocusFileIntResult V6_delete(IntPtr filePointer, byte[] record);


        /// <summary>
        /// Unlocks any lock on the file
        /// </summary>
        /// <param name="filePointer">File pointer</param>
        MicrofocusFileIntResult V6_unlock(IntPtr filePointer);



        #endregion


        #region File info


        /// <summary>
        /// Info about a file
        /// </summary>
        /// <param name="filePointer">File pointer</param>
        /// <param name="mode">Mode</param>
        /// <param name="result">Result</param>
        /// <returns>Result status</returns>
        int V6_info(IntPtr filePointer, int mode, byte[] result);

        #endregion
    }



    /// <summary>
    /// Microfocus result for file operations that return an int
    /// </summary>
    internal class MicrofocusFileIntResult
    {

        /// <summary>
        /// Microfocus result
        /// </summary>
        public int Result { get; set; }

        /// <summary>
        /// Status code
        /// </summary>
        public MicrofocusFileStatusCodes StatusCode { get; set; }
    }

    /// <summary>
    /// Microfocus result for file operations that return a pointer
    /// </summary>
    internal class MicrofocusFilePointerResult
    {
        /// <summary>
        /// Microfocus result
        /// </summary>
        public IntPtr Result { get; set; }

        /// <summary>
        /// Status code
        /// </summary>
        public MicrofocusFileStatusCodes StatusCode { get; set; }

    }


    /// <summary>
    /// Microfocus file status code
    /// </summary>
    internal enum MicrofocusFileStatusCodes
    {
        Ok = 0,
        SysError = 1,
        ParamError = 2,
        TooManyFiles = 3,
        ModeClash = 4,
        RecordLocked = 5,
        Broken = 6,
        Duplicate = 7,
        NotFound = 8,
        UndefinedRecord = 9,
        DiskFull = 10,
        FileLocked = 11,
        RecordChanged = 12,
        Mismatch = 13,
        NoMemory = 14,
        MissingFile = 15,
        Permission = 16,
        NoSupportError = 17,
        NoLocks = 18,
        Interface = 19,
        LicenseError = 20,
        UnknownError = 21,
        Transaction = 22,
        CodeSet = 23,
        AcuFhAlreadyOpened = 24,
        AcuFhAlreadyClosed = 25,
        NotMe = 99,
        NoSupportWarning = 100,
        DuplicateOkWarning = 101,
        ExtFh21 = 102,
        ExtFh37 = 103,
        ExtFh49 = 104,
        InvalidFileOperation = 105,
        ClosedWithLock = 106,
        AcuFhCloseReel = 107,
        UnmanagedError = 999
    }



    internal static class LegasyStatusCodesExtensions
    {

        /// <summary>
        /// Verifica se lo status code rappresenta un errore
        /// </summary>
        /// <param name="statusCode">Status code</param>
        /// <returns>True if the status code is a lock status</returns>
        public static bool IsLockStatus(this MicrofocusFileStatusCodes statusCode)
        {
            return statusCode == MicrofocusFileStatusCodes.FileLocked ||
                   statusCode == MicrofocusFileStatusCodes.RecordLocked;
        }

        /// <summary>
        /// Verifica se lo status code può essere considerato ok (ignore warning messages)
        /// </summary>
        /// <param name="statusCode">Status code</param>
        /// <returns>True if the status code could be considered as ok</returns>
        public static bool IsOkStatus(this MicrofocusFileStatusCodes statusCode)
        {
            return statusCode == MicrofocusFileStatusCodes.Ok ||
                   statusCode == MicrofocusFileStatusCodes.DuplicateOkWarning ||
                   statusCode == MicrofocusFileStatusCodes.ClosedWithLock ||
                   statusCode == MicrofocusFileStatusCodes.NoSupportWarning;
        }
    }


}