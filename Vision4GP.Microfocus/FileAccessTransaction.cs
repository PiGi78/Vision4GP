namespace Vision4GP.Microfocus
{

    /// <summary>
    /// Class to handle file access transactions for Microfocus file systems
    /// </summary>
    internal class FileAccessTransaction : IDisposable
    {

        /// <summary>
        /// Creates a new file access transaction and starts it
        /// </summary>
        public FileAccessTransaction()
        {
            StartTransaction();
        }

        /// <summary>
        /// Mutex to ensure exclusive access to the file system
        /// </summary>
        private Mutex Mutex { get; } = new Mutex(false, "MicrofocusFileAccessTransactionMutex");


        /// <summary>
        /// True if the mutex handle has been acquired
        /// </summary>
        private bool HasHandle = false;


        /// <summary>
        /// Takes the lock and waits up to the specified timeout in milliseconds
        /// </summary>
        /// <returns>True if the lock was acquired</returns>
        public void StartTransaction()
        {
            try
            {
                HasHandle = Mutex.WaitOne(1000);
                if (!HasHandle)
                {
                    throw new TimeoutException("Timeout waiting for exclusive access to Microfocus file system");
                }
            }
            catch (AbandonedMutexException)
            {
                // Mutex was abandoned by a terminated process
                HasHandle = true;
            }
        }



        /// <summary>
        /// Disposes the transaction, releasing the mutex if held
        /// </summary>
        public void Dispose()
        {
            if (HasHandle)
            {
                Mutex.ReleaseMutex();
            }
            Mutex.Dispose();
        }


    }
}
