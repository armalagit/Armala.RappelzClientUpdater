using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DataCore;
using RappelzClientUpdater.Events;
using RappelzClientUpdater.Enums;
using System.Collections.Generic;
using DataCore.Functions;

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
        /// Occurs when a NetworkStream byte transfer is in process
        /// </summary>
        public event EventHandler<CurrentTransferProcessArgs> CurrentTransferProcess;

        /// <summary>
        /// Occurs when a NetworkStream byte transfer length has been determined
        /// </summary>
        public event EventHandler<MaxTransferProcessArgs> MaxTransferProcess;

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
        protected void OnCurrentTransferProcess(CurrentTransferProcessArgs e) { CurrentTransferProcess?.Invoke(this, e); }

        /// <summary>
        /// Raises an event that informs the caller of a new transfer length that has occured
        /// </summary>
        /// <param name="e"></param>
        protected void OnMaxTransferProcess(MaxTransferProcessArgs e) { MaxTransferProcess?.Invoke(this, e); }

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
        /// Gets or sets value whether to keep the update files or delete the after packing
        /// </summary>
        public bool KeepUpdateFiles { get; set; } = true;

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
        public ClientUpdater(
            IPAddress serverIp,
            short serverPort,
            string clientPath,
            string clientIdentifier = "",
            string operationalPath = "",
            string locale = "us"
        ) {
            ServerIp = serverIp;
            ServerPort = serverPort;
            ClientPath = (string.IsNullOrEmpty(clientPath) ? throw new ArgumentNullException(nameof(clientPath)) : clientPath);
            DataCore.Load(ClientPath);
            Fingerprint = (string.IsNullOrEmpty(clientIdentifier) ? "Armala.RappelzClientUpdater" : clientIdentifier);
            OperationalPath = (string.IsNullOrEmpty(operationalPath) ? Directory.GetCurrentDirectory() : operationalPath);
            Locale = (string.IsNullOrEmpty(locale) ? throw new ArgumentNullException(nameof(locale)) : locale);

            DataCore.MessageOccured += (e, args) => { OnStatusUpdate(new StatusUpdateArgs(args.Message, MessageType.Information)); };
            DataCore.WarningOccured += (e, args) => { OnStatusUpdate(new StatusUpdateArgs(args.Warning, MessageType.Warning)); };
            DataCore.CurrentMaxDetermined += (e, args) => { OnMaxTransferProcess(new MaxTransferProcessArgs(args.Maximum)); };
            DataCore.CurrentProgressChanged += (e, args) => { OnCurrentTransferProcess(new CurrentTransferProcessArgs(args.Status, args.Value)); };
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

            BinaryReader br = new BinaryReader(Stream);
            BinaryWriter bw = new BinaryWriter(Stream);

            // Loop until loop break or method exit
            bool authenticated = false;
            while (!authenticated) {
                
                // Convert byte array to an integer value
                int responseCode = br.ReadInt32();
                switch (responseCode) {
                    // Authentication requested
                    case 511:

                        // Invoke OnAuthenticationRequest event
                        OnAuthenticationRequest(new AuthenticationArgs());

                        // Send password back to the server
                        byte[] fingerprintBytes = Encoding.ASCII.GetBytes(Fingerprint);

                        // Write header and authentication bytes
                        bw.Write(fingerprintBytes.Length);
                        bw.Write(fingerprintBytes);

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

            BinaryReader br = new BinaryReader(Stream);
            BinaryWriter bw = new BinaryWriter(Stream);

            // Request for latest client versions
            byte[] messageBytes = Encoding.ASCII.GetBytes("update-seek");
            bw.Write(messageBytes.Length);
            bw.Write(messageBytes);
            
            int messageLength = br.ReadInt32();
            byte[] responseBytes = br.ReadBytes(messageLength);
            string message = Encoding.UTF8.GetString(responseBytes);

            // Deserialize client versions into dictionary
            string[] langVersions = message.Split(':');
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            for (int i = 0; i < langVersions.Length; i += 2) {
                _ = int.TryParse(langVersions[i + 1], out int version);
                dictionary.Add(langVersions[i], version);
            }

            if (dictionary.ContainsKey(Locale) && dictionary[Locale] > Version) {

                DataCore.Load(ClientPath);

                int latestVersion = dictionary[Locale];
                while (Version < latestVersion) {

                    // Invoke OnStatusUpdate event
                    OnStatusUpdate(new StatusUpdateArgs("Requesting game client updates", MessageType.Information));

                    messageBytes = Encoding.ASCII.GetBytes($"update-get:{SegmentedUpdate}:{Version}:{Locale}");
                    bw.Write(messageBytes.Length);
                    bw.Write(messageBytes);

                    int incomingVersion = br.ReadInt32();
                    messageLength = br.ReadInt32();
                    messageBytes = br.ReadBytes(messageLength);

                    // Invoke OnStatusUpdate event
                    OnStatusUpdate(new StatusUpdateArgs($"Received update info for version {incomingVersion}", MessageType.Information));

                    // Save patch info file to directory
                    string savePath = Path.Combine(localPatchInfoDir, $"{Locale.ToUpper()}{incomingVersion}.tpf");
                    if (File.Exists(savePath)) File.Delete(savePath);
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

                    BinaryReader br = new BinaryReader(Stream);
                    BinaryWriter bw = new BinaryWriter(Stream);

                    // Request for latest client versions
                    byte[] messageBytes = Encoding.ASCII.GetBytes($"update-download:{Locale}{patchFile}");
                    bw.Write(messageBytes.Length);
                    bw.Write(messageBytes);

                    string saveDirectory = Path.Combine(localPatchFilesDir, $"{version}");
                    if (!Directory.Exists(saveDirectory)) Directory.CreateDirectory(saveDirectory);
                    string savePath = Path.Combine(saveDirectory, hashFileName);
                    if (File.Exists(savePath)) File.Delete(savePath);

                    long messageLength = br.ReadInt64();

                    // Invoke OnMaxTransferProcess
                    OnMaxTransferProcess(new MaxTransferProcessArgs(messageLength));

                    // Read the file from stream
                    long byteCountRead = 0;
                    using (FileStream fileStream = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                        while (byteCountRead < messageLength) {

                            // This selects either the buffer size or if remaining byte length is smaller than buffer length it write that
                            int bytesLeftToRead = (int)Math.Min(NetworkBufferSize, messageLength - byteCountRead);

                            // Read a chunk of bytes from the stream into the message buffer
                            byte[] messageBuffer = br.ReadBytes(bytesLeftToRead);

                            // Write the chunk to the file stream
                            fileStream.Write(messageBuffer, 0, messageBuffer.Length);

                            // Increment read byte count
                            byteCountRead += messageBuffer.Length;

                            // Invoke OnCurrentTransferProcess
                            OnCurrentTransferProcess(new CurrentTransferProcessArgs(hashFileName, byteCountRead));
                        }
                    }

                    File.SetAttributes(savePath, File.GetAttributes(savePath) | FileAttributes.Hidden);
                }

                PushUpdates(version);

            }
        }

        private void PushUpdates(int version) {

            // Invoke OnStatusUpdate event
            OnStatusUpdate(new StatusUpdateArgs($"Packing version {version} updates", MessageType.Information));

            string patchFileDirectory = Path.Combine(OperationalPath, ".patch-files", $"{version}");
            if (!Directory.Exists(patchFileDirectory)) throw new DirectoryNotFoundException(patchFileDirectory);

            string[] files = Directory.GetFiles(patchFileDirectory);
            foreach (string fileFullName in files) {

                string cipheredFileName = Path.GetFileName(fileFullName);
                string fileName = StringCipher.Decode(cipheredFileName);

                // Invoke OnStatusUpdate event
                OnStatusUpdate(new StatusUpdateArgs($"Packing \"{fileName}\"", MessageType.Information));

                DataCore.ImportFileEntry(fileName, File.ReadAllBytes(fileFullName));

                if (!KeepUpdateFiles) File.Delete(fileFullName);
            }

            if (Directory.GetFiles(patchFileDirectory).Length < 1) Directory.Delete(patchFileDirectory);

            Version = version;
        }

        #endregion

    }
}
