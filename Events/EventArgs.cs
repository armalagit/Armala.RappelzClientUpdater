using System;

namespace RappelzClientUpdater.Events {

    /// <summary>
    /// Houses arguments passed to caller during raising of ErrorOccured event
    /// </summary>
    public class AuthenticationArgs : EventArgs {
        /// <summary>
        /// Constructor for the AuthenticationArgs, inheriting from Eventargs
        /// </summary>
        public AuthenticationArgs() { }
    }

    /// <summary>
    /// Houses arguments passed to caller during raising of ErrorOccured event
    /// </summary>
    public class MessageArgs : EventArgs {
        /// <summary>
        /// string containing the message
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// MessageType enum containing the type of the message
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// Constructor for the MessageArgs, inheriting from Eventargs
        /// Assigns the Message/MessageType properties
        /// </summary>
        /// <param name="message">Message to be set</param>
        /// <param name="messageType">Message type enum to be set</param>
        public MessageArgs(string message, MessageType messageType) { 
            Message = message; 
            MessageType = messageType; 
        }
    }

    /// <summary>
    /// Houses arguments passed to caller during raising of ErrorOccured event
    /// </summary>
    public class TransferProcessArgs : EventArgs {
        /// <summary>
        /// string containing the file name
        /// If file name returns empty the event indicates a generic information exchange
        /// </summary>
        public string FileName { get; set; } = string.Empty;
        /// <summary>
        /// string containing the file MD5 checksum
        /// If file hash returns empty the event indicates a generic information exchange
        /// </summary>
        public string Hash { get; set; } = string.Empty;
        /// <summary>
        /// long containing the total transfer byte length
        /// </summary>
        public long TotalLength { get; set; }
        /// <summary>
        /// long containing the received byte length
        /// </summary>
        public long ReceivedLength { get; set; }

        /// <summary>
        /// Constructor for the TransferProcessArgs, inheriting from Eventargs
        /// Assigns the FileName/Hash/TotalLength/ReceivedLength properties
        /// </summary>
        /// <param name="name">File name to be set</param>
        /// <param name="hash">MD5 checksum to be set</param>
        /// <param name="totalLength">Total byte length to be set</param>
        /// <param name="receivedLength">Received byte length to be set</param>
        public TransferProcessArgs(string name, string hash, long totalLength, long receivedLength) { 
            FileName = name; 
            Hash = hash; 
            TotalLength = totalLength; 
            ReceivedLength = receivedLength; 
        }
    }

    /// <summary>
    /// Houses arguments passed to caller during raising of ErrorOccured event
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
        /// Constructor for the AuthenticationArgs, inheriting from Eventargs
        /// </summary>
        public GameClientVersionArgs(int previousVersion, int newVersion) {
            PreviousVersion = previousVersion;
            NewVersion = newVersion;
        }
    }

}
