using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DataCore;
using RappelzClientUpdater.Events;

namespace RappelzClientUpdater {

    public class RappelzClientUpdater {

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
        public event EventHandler<MessageArgs> StatusUpdate;

        /// <summary>
        /// Occurs when the game client version has been updated
        /// </summary>
        public event EventHandler<GameClientVersionArgs> GameClientVersionUpdate;

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
        protected void OnAuthenticationAccepted(AuthenticationArgs e) { AuthenticationRequest?.Invoke(this, e); }

        /// <summary>
        /// Raises an event that informs the caller of a blocked or denied authentication request that has occured
        /// </summary>
        /// <param name="e"></param>
        protected void OnAuthenticationDenied(AuthenticationArgs e) { AuthenticationRequest?.Invoke(this, e); }

        /// <summary>
        /// Raises an event that informs the caller of a transfer process that has occured
        /// </summary>
        /// <param name="e"></param>
        protected void OnTransferProcess(TransferProcessArgs e) { TransferProcess?.Invoke(this, e); }

        /// <summary>
        /// Raises an event that informs the caller of a message that has occured
        /// </summary>
        /// <param name="e"></param>
        protected void OnStatusUpdate(MessageArgs e) { StatusUpdate?.Invoke(this, e); }

        /// <summary>
        /// Raises an event that informs the caller of a game client change that has occured
        /// </summary>
        /// <param name="e"></param>
        protected void OnGameClientVersionUpdate(GameClientVersionArgs e) { GameClientVersionUpdate?.Invoke(this, e); }

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
        /// Gets the local game client path
        /// </summary>
        public string ClientPath { get; private set; }

        /// <summary>
        /// Gets the updater operational path
        /// </summary>
        public string OperationalPath { get; private set; }

        /// <summary>
        /// Gets the network stream buffer size
        /// </summary>
        public long NetworkBufferSize { get; private set; } = 1024;

        /// <summary>
        /// Gets or sets the segmented client update method
        /// When segmented update has been set the client version will be incrementally updated
        /// When non-segmented update has been set the client version will be updated from lowest version to highest possible
        /// </summary>
        public bool SegmentedUpdate { get; set; } = true;

        /// <summary>
        /// Gets the DataCore instance associated with the updater
        /// </summary>
        public Core DataCore { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Dummy constructor
        /// </summary>
        public RappelzClientUpdater() { }

        /// <summary>
        /// Instantiates the client updater
        /// </summary>
        /// <param name="serverIp"></param>
        /// <param name="serverPort"></param>
        /// <param name="clientPath"></param>
        /// <param name="operationalPath"></param>
        /// <param name="networkBufferSize"></param>
        /// <param name="segmentedUpdate"></param>
        public RappelzClientUpdater(
            IPAddress serverIp,
            short serverPort,
            string clientPath,
            string operationalPath = "",
            long networkBufferSize = 1024,
            bool segmentedUpdate = true
        ) {
            ServerIp = serverIp;
            ServerPort = serverPort;
            ClientPath = (string.IsNullOrEmpty(clientPath) ? throw new DirectoryNotFoundException() : clientPath);
            OperationalPath = (string.IsNullOrEmpty(operationalPath) ? Directory.GetCurrentDirectory() : operationalPath);
            NetworkBufferSize = networkBufferSize;
            SegmentedUpdate = segmentedUpdate;
            DataCore = new Core();
        }

        #endregion

    }
}
