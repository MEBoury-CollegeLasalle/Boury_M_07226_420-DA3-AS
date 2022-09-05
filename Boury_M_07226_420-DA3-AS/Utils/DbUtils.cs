using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Utils {

    /// <summary>
    /// Class designed to easily create connection object instances for a specific ADO.NET data source.<br/>
    /// Suggested use is to have a static instance of this somewhere that can be accessed by DAL classes.
    /// </summary>
    /// <typeparam name="TConnection"></typeparam> The type of the concrete ADO.NET connection objects to create. Must extend <see cref="DbConnection"/>
    public class DbUtils<TConnection> where TConnection : DbConnection, new() {

        private static readonly string DEFAULT_SERVER = ".\\SQL2019Express";
        private static readonly string DEFAULT_SERVER_DB_NAME = "db_lab";
        /// <summary>
        /// Yes, its voluntary that I put those here knowing that the code goes to a public repository.
        /// Don't try funky stuff, it wont work.
        /// </summary>
        private static readonly string DEFAULT_SERVER_USER = "<appDbUser>";
        private static readonly string DEFAULT_SERVER_PASSWD = "<appDbPassword>";
        private static readonly string DEFAULT_DBFILE_NAME = "lab.mdf";

        private Type _type;
        public Type ConnectionType {
            get { return _type; }
            private set { this._type = value; }
        }

        public DbUtils() {
            this.ConnectionType = new TConnection().GetType();
        }


        #region Static Methods


        /// <summary>
        /// 
        /// </summary>
        /// <param name="connStringKeyValuePairs"></param>
        /// <returns></returns>
        public static string MakeConnectionString(Dictionary<string, string> connStringKeyValuePairs) {

            string connString = "";
            foreach (KeyValuePair<string, string> pair in connStringKeyValuePairs) {
                connString += pair.Key + "=" + pair.Value + ";";
            }
            return connString;

        }


        public static TConnection GetDefaultFileDbConnection() {

            string dbFilePath = DirectoryUtils.CODEBASE_ROOT_DIRECTORY + Path.DirectorySeparatorChar + DEFAULT_DBFILE_NAME;

            if (!File.Exists(dbFilePath)) {
                throw new FileNotFoundException($"Database file at [{dbFilePath}] does not exist.");
            }

            Dictionary<string, string> connDict = new Dictionary<string, string> {
                { "Server", DEFAULT_SERVER },
                { "Integrated security", "true" },
                { "AttachDbFilename", Path.GetFullPath(dbFilePath) },
                { "User Instance", "true" }
            };
            TConnection connection = new TConnection {
                ConnectionString = MakeConnectionString(connDict)
            };
            return connection;
        }


        #endregion


        #region Methods


        /// <summary>
        /// Creates and returns a connection object for the default database on the default server.
        /// <br/><br/>
        /// Uses <seealso cref="Testbed_420_DA3_AS.Utils.DbUtils.GetDefaultServerConnection"/>
        /// </summary>
        /// <returns></returns>
        public TConnection GetDefaultDatabaseServerConnection() {
            return GetDefaultServerConnection(DEFAULT_SERVER_DB_NAME);
        }

        /// <summary>
        /// Creates and returns a connection object based on the passed database name and specific type.<br/>
        /// The <paramref name="databaseName"/> string parameter represents the name of the database for the connection.
        /// <br/><br/>
        /// Uses <seealso cref="Testbed_420_DA3_AS.Utils.DbUtils.GetServerConnection"/>
        /// </summary>
        /// <param name="databaseName"></param> The name of the database on the default server.
        /// <returns></returns>
        public TConnection GetDefaultServerConnection(string databaseName) {
            Dictionary<string, string> connDict = new Dictionary<string, string> {
                { "Server", DEFAULT_SERVER },
                { "Database", databaseName },
                { "User ID", DEFAULT_SERVER_USER },
                { "Password", DEFAULT_SERVER_PASSWD }
            };
            return GetServerConnection(connDict);

        }

        /// <summary>
        /// Creates and returns a connection object for the default database file at the root of the project.
        /// </summary>
        /// <returns></returns>
        public TConnection GetDefaultFileDbConnection() {
            string dbFilePath = DirectoryUtils.CODEBASE_ROOT_DIRECTORY + Path.DirectorySeparatorChar + DEFAULT_DBFILE_NAME;

            return GetFileDbConnection(dbFilePath);
        }

        /// <summary>
        /// Creates and returns a connection object for a database file.
        /// </summary>
        /// <param name="dbFilePath"></param> The path to the database file.
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">Thrown if the requested database file does not exist.</exception>
        public TConnection GetFileDbConnection(string dbFilePath) {
            if (!File.Exists(dbFilePath)) {
                throw new FileNotFoundException($"Database file at [{dbFilePath}] does not exist.");
            }

            Dictionary<string, string> connDict = new Dictionary<string, string> {
                { "Server", DEFAULT_SERVER },
                { "Integrated security", "true" },
                { "AttachDbFilename", Path.GetFullPath(dbFilePath) },
                { "User Instance", "true" }
            };

            return this.GetServerConnection(connDict);
        }

        /// <summary>
        /// Creates and returns a connection object based on the passed dictionary of key-value pairs and specific type.<br/>
        /// The <paramref name="connStringKeyValuePairs"/> dictionary parameter represents the key=value pairs of the connection string.
        /// </summary>
        /// <param name="connStringKeyValuePairs"></param> Dictionnary of key-value pairs for the connection string.
        /// <returns></returns>
        public TConnection GetServerConnection(Dictionary<string, string> connStringKeyValuePairs) {

            TConnection connection = new TConnection {
                ConnectionString = MakeConnectionString(connStringKeyValuePairs)
            };
            return connection;
        }


        #endregion


    }
}
