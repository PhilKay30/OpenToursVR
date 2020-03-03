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
        internal string Username { get; set; }
        internal string Password { get; set; }
        internal string DatabaseName { get; set; }
        internal string Timeout { get; set; }

        /// <summary>
        /// Debug initializer to load testing values.
        /// TODO : Read these in from a config file.
        /// </summary>
        public void DebugInit()
        {
            Host = "192.0.203.84";
            Port = "5432";
            Username = "doctor";
            Password = "wh0";
            DatabaseName = "capstone";
            Timeout = "8";
        }

        /// <summary>
        /// Generates a connection string for the database.
        /// </summary>
        /// <returns>The connection string</returns>
        public override string ToString()
        {
            return new StringBuilder().Append("Host=")
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
                .Append(Timeout)
                .Append(";Command Timeout=")
                .Append(Timeout)
                .ToString();
        }
    }
}
