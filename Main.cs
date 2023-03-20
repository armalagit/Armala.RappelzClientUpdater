using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DataCore;
using RappelzClientUpdater.Events;
using RappelzClientUpdater.Enums;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.ComponentModel;
using System.Diagnostics;

namespace RappelzClientUpdater {

    public class ClientUpdater : TcpClient {

        #region Events

        /// <summary>
        /// Occurs when the authentication process is requested
        /// </summary>
        public event EventHandler<AuthenticationArgs> AuthenticationRequest;

        /// <summary>
        /// Occurs when the authentication is accepted
        /// </summary>
        public event EventHandler<AuthenticationArgs> AuthenticationAccepted;

        /// <summary>
        /// Occurs when the authentication is denied
        /// </summary>
        public event EventHandler<AuthenticationArgs> AuthenticationDenied;

        /// <summary>
        /// Occurs when a NetworkStream byte transfer is commencing
        /// </summary>
        public event EventHandler<TransferProcessArgs> TransferProcess;

        /// <summary>
        /// Occurs when a new message is available to be displayed
        /// </summary>
        public event EventHandler<StatusUpdateArgs> StatusUpdate;

        /// <summary>
        /// Occurs when the game client version has been updated
        /// </summary>
        public event EventHandler<GameClientVersionArgs> GameClientVersionUpdate;

        /// <summary>
        /// Occurs when the TcpClient has been connected
        /// </summary>
        public event EventHandler<ConnectedArgs> ConnectedToServer;

        /// <summary>
        /// Occurs when the TcpClient has been disconnected
        /// </summary>
        public event EventHandler<DisconnectedArgs> DisconnectedFromServer;

        #endregion

        #region Event Delegates

        /// <summary>
        /// Raises an event that informs the caller of an authentication request that has occured
        /// </summary>
        /// <param name="e"></param>
        protected void OnAuthenticationRequest(AuthenticationArgs e) { AuthenticationRequest?.Invoke(this, e); }

        /// <summary>
        /// Raises an event that informs the caller of an accepted authentication request that has occured
        /// </summary>
        /// <param name="e"></param>
        protected void OnAuthenticationAccepted(AuthenticationArgs e) { AuthenticationAccepted?.Invoke(this, e); }

        /// <summary>
        /// Raises an event that informs the caller of a blocked or denied authentication request that has occured
        /// </summary>
        /// <param name="e"></param>
        protected void OnAuthenticationDenied(AuthenticationArgs e) { AuthenticationDenied?.Invoke(this, e); }

        /// <summary>
        /// Raises an event that informs the caller of a transfer process that has occured
        /// </summary>
        /// <param name="e"></param>
        protected void OnTransferProcess(TransferProcessArgs e) { TransferProcess?.Invoke(this, e); }

        /// <summary>
        /// Raises an event that informs the caller of a message that has occured
        /// </summary>
        /// <param name="e"></param>
        protected void OnStatusUpdate(StatusUpdateArgs e) { StatusUpdate?.Invoke(this, e); }

        /// <summary>
        /// Raises an event that informs the caller of a game client change that has occured
        /// </summary>
        /// <param name="e"></param>
        protected void OnGameClientVersionUpdate(GameClientVersionArgs e) { GameClientVersionUpdate?.Invoke(this, e); }

        /// <summary>
        /// Raises an event that informs the caller of a server connection that has occured
        /// </summary>
        /// <param name="e"></param>
        protected void OnConnected(ConnectedArgs e) { ConnectedToServer?.Invoke(this, e); }

        /// <summary>
        /// Raises an event that informs the caller of a server disconnection that has occured
        /// </summary>
        /// <param name="e"></param>
        protected void OnDisconnected(DisconnectedArgs e) { DisconnectedFromServer?.Invoke(this, e); }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the patch server machine IP address
        /// </summary>
        public IPAddress ServerIp { get; private set; }

        /// <summary>
        /// Gets the patch server machine port
        /// </summary>
        public short ServerPort { get; private set; }

        /// <summary>
        /// Gets the network stream from server connection
        /// </summary>
        private NetworkStream Stream { get; set; }

        /// <summary>
        /// Gets or sets the client fingerprint
        /// </summary>
        public string Fingerprint { get; private set; }

        /// <summary>
        /// Gets the local game client path
        /// </summary>
        public string ClientPath { get; set; }

        /// <summary>
        /// Gets the updater operational path
        /// </summary>
        public string OperationalPath { get; private set; }

        /// <summary>
        /// Gets the game client locale
        /// </summary>
        public string Locale { get; private set; }

        /// <summary>
        /// Gets the network stream buffer size
        /// </summary>
        public int NetworkBufferSize { get; set; } = 8096;

        /// <summary>
        /// Gets or sets the segmented client update method
        /// When segmented update has been set the client version will be incrementally updated from version to version
        /// When non-segmented update has been set the client version will be updated from lowest version to highest possible
        /// </summary>
        public bool SegmentedUpdate { get; set; } = true;

        /// <summary>
        /// Gets the DataCore instance associated with the updater
        /// </summary>
        public Core DataCore { get; } = new Core();

        /// <summary>
        /// Gets or sets the game client version
        /// </summary>
        public int Version {
            get {

                // Construct version file path
                string versionPath = Path.Combine(ClientPath, "data.00A");

                // Check if the version file exists
                if (File.Exists(versionPath)) {

                    // Read the version file bytes
                    byte[] versionFileBytes = File.ReadAllBytes(versionPath);

                    // Check for files integrity
                    if (versionFileBytes == null || versionFileBytes.Length == 0) {

                        // Invoke OnStatusUpdate
                        OnStatusUpdate(new StatusUpdateArgs($"Invalid path to file \"{versionPath}\"", MessageType.Error));

                        // Return fallbakc
                        return int.MinValue;
                    }

                    // Convert the bytes to an integer and return it
                    return BitConverter.ToInt32(versionFileBytes, 0);
                } else {

                    // Invoke OnStatusUpdate
                    OnStatusUpdate(new StatusUpdateArgs($"Invalid path to file \"{versionPath}\"", MessageType.Error));

                    // Return fallbakc
                    return int.MinValue;
                }
            }

            set {

                // Check if the new version is different from the current version
                int currentVersion = Version;
                if (currentVersion == value) return;

                // Construct version file path
                string versionPath = Path.Combine(ClientPath, "data.00A");

                // Check if the version file exists
                if (File.Exists(versionPath)) {

                    // Invoke OnStatusUpdate
                    OnStatusUpdate(new StatusUpdateArgs($"Invalid path to file \"{versionPath}\"", MessageType.Error));
                }

                // Convert the new version to bytes
                byte[] newVersion = BitConverter.GetBytes(value);

                // Write the new version to the version file
                File.WriteAllBytes(versionPath, newVersion);
                
                // Invoke OnGameClientVersionUpdate
                OnGameClientVersionUpdate(new GameClientVersionArgs(currentVersion, value));
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Dummy constructor
        /// </summary>
        public ClientUpdater() { }

        /// <summary>
        /// Instantiates the client updater
        /// </summary>
        /// <param name="serverIp"></param>
        /// <param name="serverPort"></param>
        /// <param name="clientPath"></param>
        /// <param name="operationalPath"></param>
        /// <param name="locale"></param>
        /// <param name="networkBufferSize"></param>
        /// <param name="segmentedUpdate"></param>
        public ClientUpdater(
            IPAddress serverIp,
            short serverPort,
            string clientPath,
            string clientIdentifier = "",
            string operationalPath = "",
            string locale = "us",
            int networkBufferSize = 8096,
            bool segmentedUpdate = true
        ) {
            ServerIp = serverIp;
            ServerPort = serverPort;
            ClientPath = (string.IsNullOrEmpty(clientPath) ? throw new ArgumentNullException(nameof(clientPath)) : clientPath);
            Fingerprint = (string.IsNullOrEmpty(clientIdentifier) ? "Armala.RappelzClientUpdater" : clientIdentifier);
            OperationalPath = (string.IsNullOrEmpty(operationalPath) ? Directory.GetCurrentDirectory() : operationalPath);
            Locale = (string.IsNullOrEmpty(locale) ? throw new ArgumentNullException(nameof(locale)) : locale);
            NetworkBufferSize = networkBufferSize;
            SegmentedUpdate = segmentedUpdate;
        }

        #endregion

        #region Methods

        public void ConnectAndAuthenticate() {

            try {
                // Connect to the server
                Connect(ServerIp, ServerPort);

                // Invoke OnConnected event
                OnConnected(new ConnectedArgs());
            } catch (Exception ex) {
                // Invoke OnStatusUpdate event
                OnStatusUpdate(new StatusUpdateArgs(ex.Message, MessageType.Error));

                // Invoke OnDisconnected event
                OnDisconnected(new DisconnectedArgs());

                // Exit method
                return;
            }

            // Set NetworkStream property
            Stream = GetStream();

            BinaryReader binaryReader = new BinaryReader(Stream);
            BinaryWriter binaryWriter = new BinaryWriter(Stream);

            // Loop until loop break or method exit
            bool authenticated = false;
            while (!authenticated) {
                
                // Convert byte array to an integer value
                int responseCode = binaryReader.ReadInt32();
                switch (responseCode) {
                    // Authentication requested
                    case 511:

                        // Invoke OnAuthenticationRequest event
                        OnAuthenticationRequest(new AuthenticationArgs());

                        // Send password back to the server
                        byte[] fingerprintBytes = Encoding.ASCII.GetBytes(Fingerprint);

                        // Write header and authentication bytes
                        binaryWriter.Write(fingerprintBytes.Length);
                        binaryWriter.Write(fingerprintBytes);

                        break;
                        
                    // Authentication successful
                    case 202:

                        // Invoke OnAuthenticationAccepted event
                        OnAuthenticationAccepted(new AuthenticationArgs());

                        // Set boolean to break the loop
                        authenticated = true;

                        break;
                        
                    // Authentication failed or unexpected response
                    default:

                        // Invoke OnAuthenticationDenied event
                        OnAuthenticationDenied(new AuthenticationArgs());

                        // Close socket
                        if (Connected) Close();

                        // Invoke OnStatusUpdate event
                        OnStatusUpdate(new StatusUpdateArgs("Authentication failed or received unexpected result from the server", MessageType.Error));

                        // Invoke OnDisconnected event
                        OnDisconnected(new DisconnectedArgs());

                        break;
                }
            }
        }

        public void BeginUpdate() {

            // Ensure local patch info directory exists
            string localPatchInfoDir = Path.Combine(OperationalPath, ".patch-info");
            if (!Directory.Exists(localPatchInfoDir)) {
                Directory.CreateDirectory(localPatchInfoDir);
                File.SetAttributes(localPatchInfoDir, File.GetAttributes(localPatchInfoDir) | FileAttributes.Hidden);
            }

            BinaryReader binaryReader = new BinaryReader(Stream);
            BinaryWriter binaryWriter = new BinaryWriter(Stream);

            // Request for latest client versions
            byte[] messageBytes = Encoding.ASCII.GetBytes("update-seek");
            binaryWriter.Write(messageBytes.Length);
            binaryWriter.Write(messageBytes);
            
            int messageLength = binaryReader.ReadInt32();
            byte[] responseBytes = binaryReader.ReadBytes(messageLength);
            string message = Encoding.UTF8.GetString(responseBytes);

            // Deserialize client versions into dictionary
            string[] langVersions = message.Split(':');
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            for (int i = 0; i < langVersions.Length; i += 2) {
                _ = int.TryParse(langVersions[i + 1], out int version);
                dictionary.Add(langVersions[i], version);
            }

            if (dictionary.ContainsKey(Locale) && dictionary[Locale] > Version) {

                int latestVersion = dictionary[Locale];
                while (Version < latestVersion) {

                    // Invoke OnStatusUpdate event
                    OnStatusUpdate(new StatusUpdateArgs("Requesting game client updates", MessageType.Information));

                    messageBytes = Encoding.ASCII.GetBytes($"update-get:{SegmentedUpdate}:{Version}:{Locale}");
                    binaryWriter.Write(messageBytes.Length);
                    binaryWriter.Write(messageBytes);

                    int incomingVersion = binaryReader.ReadInt32();
                    messageLength = binaryReader.ReadInt32();
                    messageBytes = new byte[messageLength];

                    // Loop while there are bytes to read
                    int byteCountRead = 0;
                    while (byteCountRead < messageLength) {

                        // Calculate bytes left to read
                        int bytesLeftToRead = Math.Min(NetworkBufferSize, messageLength - byteCountRead);
                        
                        // Read a chunk of bytes from the stream into the message buffer
                        byte[] messageBuffer = binaryReader.ReadBytes(bytesLeftToRead);

                        // Copy byte array from the buffer to the actual message byte array
                        Array.Copy(messageBuffer, 0, messageBytes, byteCountRead, bytesLeftToRead);

                        // Increment read byte count
                        byteCountRead += messageBuffer.Length;

                        // Invoke OnTransferProcess
                        OnTransferProcess(new TransferProcessArgs(string.Empty, string.Empty, messageLength, byteCountRead));
                    }

                    // Save patch info file to directory
                    string savePath = Path.Combine(localPatchInfoDir, $"{Locale.ToUpper()}{incomingVersion}.tpf");
                    File.WriteAllText(savePath, Encoding.ASCII.GetString(messageBytes));
                    File.SetAttributes(savePath, File.GetAttributes(savePath) | FileAttributes.Hidden);

                    // Convert message byte array to readable string
                    DownloadUpdates(incomingVersion);
                }
            }

            // Invoke OnStatusUpdate event
            OnStatusUpdate(new StatusUpdateArgs("Game client up to date", MessageType.Information));
        }

        private void DownloadUpdates(int version) {

            // Invoke OnStatusUpdate event
            OnStatusUpdate(new StatusUpdateArgs($"Updating client to version {version}", MessageType.Information));

            // Get patch info directory
            string localPatchInfoDir = Path.Combine(OperationalPath, ".patch-info");
            string filePath = Path.Combine(localPatchInfoDir, $"{Locale.ToUpper()}{version}.tpf");

            // Ensure local patch file directory exists
            string localPatchFilesDir = Path.Combine(OperationalPath, ".patch-files");
            if (!Directory.Exists(localPatchFilesDir)) {
                Directory.CreateDirectory(localPatchFilesDir);
                File.SetAttributes(localPatchFilesDir, File.GetAttributes(localPatchFilesDir) | FileAttributes.Hidden);
            }

            // open the file
            using (StreamReader reader = new StreamReader(filePath)) {
                // loop through each line in the file
                string line;
                while ((line = reader.ReadLine()) != null) {
                    // empty:RZ_US:191:XM!+%tAWA03F&Ad{)N!oIJVwCrg;J:18003153:46A84BC2:10188969:538A2B3F:/090/::

                    if (string.IsNullOrEmpty(line)) continue;

                    string[] patchFileArray = line.Split(':');
                    string hashFileName = patchFileArray[3];
                    string patchFile = $"{patchFileArray[8]}{patchFileArray[3]}";

                    BinaryReader binaryReader = new BinaryReader(Stream);
                    BinaryWriter binaryWriter = new BinaryWriter(Stream);

                    // Request for latest client versions
                    byte[] messageBytes = Encoding.ASCII.GetBytes($"update-download:{Locale}{patchFile}");
                    binaryWriter.Write(messageBytes.Length);
                    binaryWriter.Write(messageBytes);

                    // Prepare local file
                    string savePath = Path.Combine(localPatchFilesDir, hashFileName);
                    if (File.Exists(savePath)) File.Delete(savePath);

                    // Create buffer
                    int messageLength = binaryReader.ReadInt32();

                    // Read the file from stream
                    int byteCountRead = 0;
                    using (FileStream fileStream = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                        while (byteCountRead < messageLength) {

                            // This selects either the buffer size or if remaining byte length is smaller than buffer length it write that
                            int bytesLeftToRead = Math.Min(NetworkBufferSize, messageLength - byteCountRead);

                            // Read a chunk of bytes from the stream into the message buffer
                            byte[] messageBuffer = binaryReader.ReadBytes(bytesLeftToRead);

                            // Write the chunk to the file stream
                            fileStream.Write(messageBuffer, 0, messageBuffer.Length);

                            // Increment read byte count
                            byteCountRead += messageBuffer.Length;

                            // Invoke OnTransferProcess
                            OnTransferProcess(new TransferProcessArgs(hashFileName, string.Empty, messageLength, byteCountRead));
                        }
                    }

                    File.SetAttributes(savePath, File.GetAttributes(savePath) | FileAttributes.Hidden);
                }

                Version = version;
            }
        }

        #endregion

    }
}
