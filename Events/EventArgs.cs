using RappelzClientUpdater.Enums;
using System;

namespace RappelzClientUpdater.Events {

    /// <summary>
    /// Houses arguments passed to caller during raising of Authentication event
    /// </summary>
    public class AuthenticationArgs : EventArgs {
        /// <summary>
        /// Constructor for the AuthenticationArgs, inheriting from Eventargs
        /// </summary>
        public AuthenticationArgs() { }
    }

    /// <summary>
    /// Houses arguments passed to caller during raising of StatusUpdate event
    /// </summary>
    public class StatusUpdateArgs : EventArgs {
        /// <summary>
        /// string containing the message
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// MessageType enum containing the type of the message
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// Constructor for the StatusUpdateArgs, inheriting from Eventargs
        /// Assigns the Message/MessageType properties
        /// </summary>
        /// <param name="message">Message to be set</param>
        /// <param name="messageType">Message type enum to be set</param>
        public StatusUpdateArgs(string message, MessageType messageType) {
            Message = message;
            MessageType = messageType;
        }
    }

    /// <summary>
    /// Houses arguments passed to caller during raising of CurrentTransferProcess event
    /// </summary>
    public class CurrentTransferProcessArgs : EventArgs {
        /// <summary>
        /// string containing the file name
        /// If file name returns empty the event indicates a generic information exchange
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        /// <summary>
        /// long containing the received byte length
        /// </summary>
        public long ReceivedLength { get; set; } = 0;

        /// <summary>
        /// Constructor for the TransferProcessArgs, inheriting from Eventargs
        /// Assigns the FileName/Hash/ReceivedLength properties
        /// </summary>
        /// <param name="name">File name to be set</param>
        /// <param name="receivedLength">Received byte length to be set</param>
        public CurrentTransferProcessArgs(string name, long receivedLength) {
            FileName = name;
            ReceivedLength = receivedLength;
        }
    }

    /// <summary>
    /// Houses arguments passed to caller during raising of MaxTransferProcess event
    /// </summary>
    public class MaxTransferProcessArgs : EventArgs {
        /// <summary>
        /// long containing the total transfer byte length
        /// </summary>
        public long TotalLength { get; set; } = 0;

        /// <summary>
        /// Constructor for the MaxTransferProcessArgs, inheriting from Eventargs
        /// Assigns the TotalLength property
        /// </summary>
        /// <param name="totalLength">Total byte length to be set</param>
        public MaxTransferProcessArgs(long totalLength) {
            TotalLength = totalLength;
        }
    }

    /// <summary>
    /// Houses arguments passed to caller during raising of GameClientVersion event
    /// </summary>
    public class GameClientVersionArgs : EventArgs {
        /// <summary>
        /// integer containing the previous version
        /// </summary>
        public int PreviousVersion { get; set; }
        /// <summary>
        /// integer containing the new updated version
        /// </summary>
        public int NewVersion { get; set; }

        /// <summary>
        /// Constructor for the GameClientVersionArgs, inheriting from Eventargs
        /// </summary>
        public GameClientVersionArgs(int previousVersion, int newVersion) {
            PreviousVersion = previousVersion;
            NewVersion = newVersion;
        }
    }

    /// <summary>
    /// Houses arguments passed to caller during raising of Connected event
    /// </summary>
    public class ConnectedArgs : EventArgs {
        /// <summary>
        /// Constructor for the ConnectedArgs, inheriting from Eventargs
        /// </summary>
        public ConnectedArgs() { }
    }

    /// <summary>
    /// Houses arguments passed to caller during raising of Disconnected event
    /// </summary>
    public class DisconnectedArgs : EventArgs {
        /// <summary>
        /// Constructor for the DisconnectedArgs, inheriting from Eventargs
        /// </summary>
        public DisconnectedArgs() { }
    }

    /// <summary>
    /// Houses arguments passed to caller during raising of LoginValidation event
    /// </summary>
    public class LoginValidationArgs : EventArgs {
        /// <summary>
        /// string containing the username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// string containing the password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Constructor for the LoginValidationArgs, inheriting from Eventargs
        /// </summary>
        public LoginValidationArgs(string username, string password) {
            Username = username;
            Password = password;
        }
    }

    /// <summary>
    /// Houses arguments passed to caller during raising of LoginValidation event
    /// </summary>
    public class LoginValidationSuccessArgs : EventArgs {
        /// <summary>
        /// string containing the username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// int containing the account id
        /// </summary>
        public int AccountId { get; set; }
        /// <summary>
        /// string containing the session token
        /// </summary>
        public string SessionToken { get; set; }

        /// <summary>
        /// Constructor for the LoginValidationArgs, inheriting from Eventargs
        /// </summary>
        public LoginValidationSuccessArgs(string username, int accountId, string sessionToken) {
            Username = username;
            AccountId = accountId;
            SessionToken = sessionToken;
        }
    }

    /// <summary>
    /// Houses arguments passed to caller during raising of LoginValidation event
    /// </summary>
    public class LoginValidationFailedArgs : EventArgs {
        /// <summary>
        /// string containing the username
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// string containing the password
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// string containing the server response message
        /// </summary>
        public string ResponseMessage { get; set; }

        /// <summary>
        /// Constructor for the LoginValidationArgs, inheriting from Eventargs
        /// </summary>
        public LoginValidationFailedArgs(string username, string password, string responseMessage) {
            Username = username;
            Password = password;
            ResponseMessage = responseMessage;
        }
    }

}
