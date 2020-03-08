using System.Text;

namespace Mapping.Common
{
    /// <summary>
    /// Storage class for connection data to handle connections to the database.
    /// Created by Timothy J Cowen.
    /// </summary>
    internal class ConnectionData
    {
        internal string Host { get; set; }
        internal string Port { get; set; }
        internal string DatabaseName { get; set; }
        internal string Username { get; set; }
        internal string Password { get; set; }
        internal string ConnectionTimeout { get; set; }
        internal string CommandTimeout { get; set; }

        public ConnectionData(
            string host,
            string port,
            string databaseName,
            string username,
            string password,
            string connectionTimeout,
            string commandTimeout)
        {
            Host = host;
            Port = port;
            Username = username;
            Password = password;
            DatabaseName = databaseName;
            ConnectionTimeout = connectionTimeout;
            CommandTimeout = commandTimeout;
        }

        /// <summary>
        /// Generates a connection string for the database.
        /// </summary>
        /// <returns>The connection string</returns>
        public override string ToString()
        {
            return new StringBuilder()
                .Append("Host=")
                .Append(Host)
                .Append(";Port=")
                .Append(Port)
                .Append(";Database=")
                .Append(DatabaseName)
                .Append(";Username=")
                .Append(Username)
                .Append(";Password=")
                .Append(Password)
                .Append(";Timeout=")
                .Append(ConnectionTimeout)
                .Append(";Command Timeout=")
                .Append(CommandTimeout)
                .ToString();
        }
    }
}
