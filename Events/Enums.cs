namespace RappelzClientUpdater.Events {

    /// <summary>
    /// Defines the type of message to be set with MessageArgs
    /// </summary>
    public enum MessageType {
        /// <summary>
        /// Informative type
        /// </summary>
        Information = 0,
        /// <summary>
        /// Error type
        /// </summary>
        Error = 1,
        /// <summary>
        /// Warning type
        /// </summary>
        Warning = 2,
        /// <summary>
        /// Successful type
        /// </summary>
        Success = 3
    }

}
