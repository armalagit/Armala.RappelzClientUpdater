namespace Armala.RappelzClientUpdater.EventLogger
{

    public class Logger
    {
        private readonly string _logFile;

        public Logger(string logFile)
        {
            _logFile = logFile;
        }

        public void Information(string messageTemplate, params object[] propertyValues)
        {
            LogEvent(LogLevel.Information, messageTemplate, propertyValues);
        }

        public void Debug(string messageTemplate, params object[] propertyValues)
        {
            LogEvent(LogLevel.Debug, messageTemplate, propertyValues);
        }

        public void Error(string messageTemplate, params object[] propertyValues)
        {
            LogEvent(LogLevel.Error, messageTemplate, propertyValues);
        }

        private void LogEvent(LogLevel logLevel, string messageTemplate, object[] propertyValues)
        {
            var properties = GetProperties(propertyValues);

            var logEvent = new LogEvent(DateTimeOffset.Now, logLevel, messageTemplate, properties);

            WriteLog(logEvent);
        }

        private Dictionary<string, object> GetProperties(object[] propertyValues)
        {
            var properties = new Dictionary<string, object>();

            for (int i = 0; i < propertyValues.Length; i += 2)
            {
                if (propertyValues[i] is string key)
                {
                    properties[key] = propertyValues[i + 1];
                }
            }

            return properties;
        }

        private void WriteLog(LogEvent logEvent)
        {
            var logMessage = $"{logEvent.Timestamp} {logEvent.Level} {logEvent.MessageTemplate}";

            foreach (var property in logEvent.Properties)
            {
                logMessage += $" {property.Key}={property.Value}";
            }

            logMessage += Environment.NewLine;

            File.AppendAllText(_logFile, logMessage);
        }
    }

    public class LogEvent
    {
        public DateTimeOffset Timestamp { get; }
        public LogLevel Level { get; }
        public string MessageTemplate { get; }
        public Dictionary<string, object> Properties { get; }

        public LogEvent(DateTimeOffset timestamp, LogLevel level, string messageTemplate, Dictionary<string, object> properties)
        {
            Timestamp = timestamp;
            Level = level;
            MessageTemplate = messageTemplate;
            Properties = properties;
        }
    }

    public enum LogLevel
    {
        Information,
        Debug,
        Error
    }
}