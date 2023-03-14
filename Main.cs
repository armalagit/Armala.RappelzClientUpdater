using System;
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
        /// Raises an event that informs the caller of a authentication request that has occured
        /// </summary>
        /// <param name="e"></param>
        protected void OnAuthenticationRequest(AuthenticationArgs e) { AuthenticationRequest?.Invoke(this, e); }

        /// <summary>
        /// Raises an event that informs the caller of a commencing transfer that has occured
        /// </summary>
        /// <param name="e"></param>
        protected void OnTransferCommencing(TransferCommencingArgs e) { TransferCommencing?.Invoke(this, e); }

        #endregion

    }
}
