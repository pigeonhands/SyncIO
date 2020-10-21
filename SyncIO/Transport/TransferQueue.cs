namespace SyncIO.Transport
{
    using SyncIO.Network;
    using SyncIO.Transport.Packets.Internal;
    using System;
    using System.IO;
    using System.Threading;

    public class TransferQueue
    {
        #region Variables

        // Size of our read buffer.
        private const int FileBufferSize = 1024 * 1024 * 1;

        // Single read buffer every transfer queue will use to save memory.
        private static readonly byte[] _fileBuffer = new byte[FileBufferSize];

        // Used for pausing uploads.
        private readonly ManualResetEvent _pauseEvent;

        // Upload thread.
        private Thread _thread;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the generated ID for each transfer.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// File transfer socket client.
        /// </summary>
        public ISyncIOClient Client { get; }

        /// <summary>
        /// Progress and last progress (For checks) for the queues.
        /// </summary>
        public double Progress, LastProgress;

        /// <summary>
        /// Transferred bytes, current read/write index and the size of the file.
        /// </summary>
        public long Transferred, Index, Length;

        /// <summary>
        /// Gets a value determining if the file transfer is running or not.
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// Gets a value determining if the file transfer is paused or not.
        /// </summary>
        public bool Paused { get; private set; }

        /// <summary>
        /// Filename for reading/writing.
        /// </summary>
        public string FileName, DestinationName;

        /// <summary>
        /// Gets or sets the full file path of the file to transfer.
        /// </summary>
        public string FullFilePath { get; private set; }

        /// <summary>
        /// Gets the file transfer type.
        /// </summary>
        public FileTransferType Type { get; private set; }

        /// <summary>
        /// File stream for reading/writing.
        /// </summary>
        public FileStream Stream { get; private set; }

        public DateTime TimeStarted, TimeCompleted;

        #endregion

        #region Constructor

        private TransferQueue(ISyncIOClient client)
        {
            Client = client;
            // When the instance is created, create a new ManualResetEvent.
            _pauseEvent = new ManualResetEvent(true);
            Running = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the file transfer.
        /// </summary>
        public void Start()
        {
            // Start our upload thread with the current instance as the parameter.
            Running = true;
            if (_thread == null)
            {
                _thread = new Thread(TransferHandler);
            }
            _thread.IsBackground = true;
            _thread.Start(this);

            //PluginInvokers.InvokePluginFileTransferStateChanged(this, FileTransferStatus.Queued);
        }

        /// <summary>
        /// Stop the file transfer.
        /// </summary>
        public void Stop()
        {
            Running = false;

            //PluginInvokers.InvokePluginFileTransferStateChanged(this, FileTransferStatus.Canceled);
        }

        /// <summary>
        /// Pause or resume the file transfer.
        /// </summary>
        public void Pause()
        {
            // If it is not paused, reset the event so the upload thread will block.
            if (!Paused)
            {
                _pauseEvent.Reset();

                // PluginInvokers.InvokePluginFileTransferStateChanged(this, FileTransferStatus.Paused);
            }
            else // If it is already paused, set the event so the thread can continue.
            {
                _pauseEvent.Set();

                //PluginInvokers.InvokePluginFileTransferStateChanged(this, FileTransferStatus.Active);
            }

            Paused = !Paused; // Flip the paused variable.
        }

        /// <summary>
        /// Close and remove the file transfer.
        /// </summary>
        public void Close()
        {
            try
            {
                TimeCompleted = DateTime.Now;
                // Remove the current queue from the client transfer list.
                Client.Transfers.Remove(Id);
            }
            catch (Exception ex)
            {
                //Client.ExceptionThrown(this, new ExceptionThrownEventArgs(Client.Listener, Client, ex, "Close"));
            }
            Running = false;
            //Close the stream
            Stream.Close();
            ////Dispose the ResetEvent.
            ////pauseEvent.Dispose();

            //Client = null;
        }

        public void Write(byte[] bytes, long index)
        {
            // Lock the current instance, so only one write at a time is permitted.
            lock (this)
            {
                // Set the stream position to our current write index we receive.
                Stream.Position = index;
                // Write the bytes to the stream.
                Stream.Write(bytes, 0, bytes.Length);
                // Increase the amount of data we received
                Transferred += bytes.Length;
            }
        }

        public void SetId(Guid id)
        {
            Id = id;
        }

        #endregion

        /// <summary>
        /// Reads and sends file data chunk until all data is read.
        /// </summary>
        /// <param name="o">State object.</param>
        private static void TransferHandler(object o)
        {
            // Cast our transfer queue from the parameter.
            var queue = o as TransferQueue;

            // If Running is true, the thread will keep going
            // If queue.Index is not the file length, the thread will continue.
            while (queue.Running && queue.Index < queue.Length)
            {
                // We will call WaitOne to see if we're paused or not.
                // If we are, it will block until notified.
                queue._pauseEvent.WaitOne();

                // Just in case the transfer was paused then stopped, check to see if we're still running
                if (!queue.Running) break;

                // Lock the file buffer so only one queue can use it at a time.
                lock (_fileBuffer)
                {
                    // Set the read position to our current position
                    queue.Stream.Position = queue.Index;

                    // Read a chunk into our buffer.
                    var read = queue.Stream.Read(_fileBuffer, 0, _fileBuffer.Length);

                    queue.Client.Send(new FileDataPacket(queue.Id, queue.FullFilePath, _fileBuffer, queue.Index, queue.Stream.Position == queue.Stream.Length));

                    queue.Transferred += read;
                    queue.Index += read;
                    queue.Progress = queue.Transferred * 100.00 / queue.Length;

                    if (Math.Round(queue.Progress, 0) == 100)
                    {
                        queue.TimeCompleted = DateTime.Now;
                    }

                    if (queue.LastProgress < queue.Progress)
                    {
                        queue.LastProgress = queue.Progress;
                        //queue.Client.OnProgressChanged(queue);
                    }

                    Thread.Sleep(10);
                }
            }
            queue.Close();
        }

        #region Static Methods

        /// <summary>
        /// Generates a new unique transfer id.
        /// </summary>
        /// <returns>Guid transfer id.</returns>
        public static Guid GenerateTransferId()
        {
            return Guid.NewGuid();
        }

        /// <summary>
        /// Create upload file transfer from server to client.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="fileName"></param>
        /// <param name="destinationName"></param>
        /// <returns></returns>
        public static TransferQueue CreateUploadQueue(ISyncIOClient client, string fileName, string destinationName)
        {
            try
            {
                // Create a new upload queue
                var queue = new TransferQueue(client)
                {
                    // Set our filename
                    FileName = Path.GetFileName(fileName),
                    // Set our full file path
                    FullFilePath = fileName,
                    // Set out destination filename
                    DestinationName = destinationName,
                    // Set our queue type to upload.
                    Type = FileTransferType.Upload,
                    // Create our file stream for reading.
                    Stream = new FileStream(fileName, FileMode.Open),
                    // Generate our ID
                    Id = GenerateTransferId(),
                    TimeStarted = DateTime.Now
                };
                // Set our length to the size of the file.
                queue.Length = queue.Stream.Length;
                // Add the upload transfer queue to the client's transfers
                client.Transfers.Add(queue.Id, queue);
                return queue;
            }
            catch
            {
                // If something went wrong, return null
                return null;
            }
        }

        /// <summary>
        /// Create download file transfer from client to server.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="id"></param>
        /// <param name="saveName"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static TransferQueue CreateDownloadQueue(ISyncIOClient client, Guid id, string saveName, long length)
        {
            try
            {
                var downloadsDirectory = Path.Combine(Path.Combine("Files", client.EndPoint.Address.ToString()), saveName); //client.DownloadsDirectory
                // Create a new upload queue
                var queue = new TransferQueue(client)
                {
                    FileName = Path.GetFileName(saveName),
                    // Set our full file path
                    FullFilePath = Path.Combine(downloadsDirectory, saveName),
                    Type = FileTransferType.Download,
                    // Create our file stream for writing.
                    Stream = new FileStream(downloadsDirectory, FileMode.Create),
                    Length = length,
                    TimeStarted = DateTime.Now
                };
                // Fill the stream will 0 bytes based on the real size. So we can index write.
                queue.Stream.SetLength(length);
                // Instead of generating an ID, we will set the ID that has been sent.
                queue.SetId(id);
                // Add the download transfer queue to the client's transfers
                client.Transfers.Add(queue.Id, queue);
                return queue;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}