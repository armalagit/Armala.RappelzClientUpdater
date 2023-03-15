using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DataCore;
using RappelzClientUpdater.Events;
using RappelzClientUpdater.Enums;

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
        /// Gets the network stream buffer size
        /// </summary>
        public long NetworkBufferSize { get; set; } = 1024;

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

                        // Invoke data file missing error
                        OnStatusUpdate(new StatusUpdateArgs($"Invalid path to file \"{versionPath}\"", MessageType.Error));

                        // Return fallbakc
                        return int.MinValue;
                    }

                    // Convert the bytes to an integer and return it
                    return BitConverter.ToInt32(versionFileBytes, 0);
                } else {

                    // Invoke data file missing error
                    OnStatusUpdate(new StatusUpdateArgs($"Invalid path to file \"{versionPath}\"", MessageType.Error));

                    // Return fallbakc
                    return int.MinValue;
                }
            }

            set {

                // Check if the new version is different from the current version
                if (Version == value) return;

                // Construct version file path
                string versionPath = Path.Combine(ClientPath, "data.00A");

                // Check if the version file exists
                if (File.Exists(versionPath)) {

                    // Invoke data file missing error
                    OnStatusUpdate(new StatusUpdateArgs($"Invalid path to file \"{versionPath}\"", MessageType.Error));
                }

                // Convert the new version to bytes
                byte[] newVersion = BitConverter.GetBytes(value);

                // Write the new version to the version file
                File.WriteAllBytes(versionPath, newVersion);
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
        /// <param name="networkBufferSize"></param>
        /// <param name="segmentedUpdate"></param>
        public ClientUpdater(
            IPAddress serverIp,
            short serverPort,
            string clientPath,
            string clientIdentifier = "",
            string operationalPath = "",
            long networkBufferSize = 1024,
            bool segmentedUpdate = true
        ) {
            ServerIp = serverIp;
            ServerPort = serverPort;
            ClientPath = (string.IsNullOrEmpty(clientPath) ? throw new ArgumentNullException(nameof(clientPath)) : clientPath);
            Fingerprint = (string.IsNullOrEmpty(clientIdentifier) ? "Armala.RappelzClientUpdater" : clientIdentifier);
            OperationalPath = (string.IsNullOrEmpty(operationalPath) ? Directory.GetCurrentDirectory() : operationalPath);
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

                // Exit method
                return;
            }

            // Set NetworkStream property
            Stream = GetStream();

            // Loop until loop break or method exit
            while (Connected) {
                
                // Check if stream exists and is readable
                if (Stream == null || !Stream.CanRead) {

                    // Close socket
                    if (Connected) Close();

                    // Invoke OnStatusUpdate event
                    OnStatusUpdate(new StatusUpdateArgs("Server stream null or unreadable", MessageType.Error));

                    // Invoke OnDisconnected event
                    OnDisconnected(new DisconnectedArgs());

                    // Exit method
                    return;

                }

                // Create byte container for server response
                byte[] responseBytes = new byte[4];

                // Read server response to the container
                Stream.Read(responseBytes, 0, 4);

                // Convert byte array to an integer value
                int responseCode = BitConverter.ToInt32(responseBytes, 0);

                // Authentication requested
                if (responseCode == 511) {

                    // Invoke OnAuthenticationRequest event
                    OnAuthenticationRequest(new AuthenticationArgs());

                    // Check if stream exists and is writable
                    if (Stream == null || !Stream.CanWrite) {

                        // Close socket
                        if (Connected) Close();

                        // Invoke OnStatusUpdate event
                        OnStatusUpdate(new StatusUpdateArgs("Server stream null or unwritable", MessageType.Error));

                        // Invoke OnDisconnected event
                        OnDisconnected(new DisconnectedArgs());

                        // Exit method
                        return;

                    }

                    // Send password back to the server
                    byte[] fingerprintBytes = Encoding.ASCII.GetBytes(Fingerprint);

                    // Create message length header bytes
                    byte[] headerBytes = BitConverter.GetBytes(fingerprintBytes.Length);

                    // Write header and authentication bytes
                    Stream.Write(headerBytes, 0, 4);
                    Stream.Write(fingerprintBytes, 0, fingerprintBytes.Length);

                // Authentication successful
                } else if (responseCode == 202) {

                    // Invoke OnAuthenticationAccepted event
                    OnAuthenticationAccepted(new AuthenticationArgs());

                    // Break loop
                    break;

                // Authentication failed or unexpected response
                } else {

                    // Invoke OnAuthenticationDenied event
                    OnAuthenticationDenied(new AuthenticationArgs());

                    // Close socket
                    if (Connected) Close();

                    // Invoke OnStatusUpdate event
                    OnStatusUpdate(new StatusUpdateArgs("Authentication failed or received unexpected result from the server", MessageType.Error));

                    // Invoke OnDisconnected event
                    OnDisconnected(new DisconnectedArgs());

                    // Exit method
                    return;
                }
            }
        }

        public void BeginUpdate() {
                
            // Check if stream exists and is readable
            if (Stream == null || !Stream.CanWrite) {

                // Close socket
                if (Connected) Close();

                // Invoke OnStatusUpdate event
                OnStatusUpdate(new StatusUpdateArgs("Server stream null or unwritable", MessageType.Error));

                // Invoke OnDisconnected event
                OnDisconnected(new DisconnectedArgs());

                // Exit method
                return;

            }

            // Construct update info parameters
            string updateParameters = $"{Version}:{SegmentedUpdate}:us";

            // Send password back to the server
            byte[] messageBytes = Encoding.ASCII.GetBytes(updateParameters);

            // Create message length header bytes
            byte[] headerBytes = BitConverter.GetBytes(messageBytes.Length);

            // Write update request
            Stream.Write(headerBytes, 0, 4);
            Stream.Write(messageBytes, 0, messageBytes.Length);

        }

        #endregion

    }
}
