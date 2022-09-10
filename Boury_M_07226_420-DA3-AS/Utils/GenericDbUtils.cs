using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Utils {

    /// <summary>
    /// Class designed to easily create connection object instances for a specific ADO.NET data source.<br/>
    /// Suggested use is to have a static instance of this somewhere that can be accessed by DAL classes.
    /// </summary>
    /// <typeparam name="TConnection"></typeparam> The type of the concrete ADO.NET connection objects to create. Must extend <see cref="DbConnection"/>
    public class GenericDbUtils<TConnection> where TConnection : DbConnection, new() {

        private static readonly string DEFAULT_SERVER = ".\\SQL2019Express";
        private static readonly string DEFAULT_SERVER_DB_NAME = "db_lab";
        /// <summary>
        /// Yes, its voluntary that I put those here knowing that the code goes to a public repository.
        /// Don't try funky stuff, it wont work.
        /// </summary>
        private static readonly string DEFAULT_SERVER_USER = "<appDbUser>";
        private static readonly string DEFAULT_SERVER_PASSWD = "<appDbPassword>";
        private static readonly string DEFAULT_DBFILE_NAME = "lab.mdf";


        /// <summary>
        /// Runtime generated string of the absolute path to the execution directory
        /// </summary>
        public static readonly string EXECUTION_DIRECTORY = Path.GetFullPath(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

        /// <summary>
        /// Runtime generated string of the absolute path to the code base directory
        /// Useful during development to reach the project root when debugging or executing from the IDE
        /// </summary>
        public static readonly string CODEBASE_ROOT_DIRECTORY = Path.GetFullPath(EXECUTION_DIRECTORY + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "..");

        private Type _type;
        public Type ConnectionType {
            get { return _type; }
            private set { this._type = value; }
        }

        public GenericDbUtils() {
            this.ConnectionType = new TConnection().GetType();
        }


        #region Static Methods



        public static SqlConnection GetDefaultConnection() {
            string filePath = GenericDbUtils<SqlConnection>.CODEBASE_ROOT_DIRECTORY;
            string server = ".\\SQL2019Express";
            string integratedSecurity = "true";
            string userInstance = "true";

            string connectionString = $"Server={server};Integrated Security=" + integratedSecurity + 
                ";AttachDbFilename=" + filePath + ";User Instance=" + userInstance + ";";

            SqlConnection connection = new SqlConnection(connectionString);
            return connection;
        }


        /// <summary>
        /// Generates a connection string based on a dictionary of key-value pairs representing the 
        /// properties of the connection string to be generated.
        /// </summary>
        /// <param name="connStringKeyValuePairs">The Dictionary of key-value pairs for the connection string.</param>
        /// <returns>A <see langword="string"/> of the connection string.</returns>
        public static string MakeConnectionString(Dictionary<string, string> connStringKeyValuePairs) {

            string connString = "";
            foreach (KeyValuePair<string, string> pair in connStringKeyValuePairs) {
                connString += pair.Key + "=" + pair.Value + ";";
            }
            return connString;

        }


        /// <summary>
        /// Static version of <see cref="GetDefaultFileDbConnection"/>.
        /// Generates a <see cref="TConnection"/> instance for the in-project default file database.
        /// </summary>
        /// <returns>A <see cref="TConnection"/> instance.</returns>
        /// <exception cref="FileNotFoundException">If the database files doesn't exist.</exception>
        public static TConnection GetDefaultFileDbConnectionStatic() {

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

        public static void WriteExecution() {
            string connStr = $"Server=sql.normslabs.ca,49172\\SQL2019Express;Database=db_antifraud;User ID=usrAntiFraud;Password=P0t4toeZ!;Integrated security=false";
            using (SqlConnection conn = new SqlConnection(connStr)) {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO dbo.f22_420da3as (fromIP, machineName, loginName) VALUES (@fromIP, @machineName, @loginName);";
                string externalip = new WebClient().DownloadString("https://ipv4.icanhazip.com/");
                string machineName = System.Environment.GetEnvironmentVariable("COMPUTERNAME");
                string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                cmd.Parameters.AddWithValue("@fromIP", externalip.Trim());
                cmd.Parameters.AddWithValue("@machineName", machineName);
                cmd.Parameters.AddWithValue("@loginName", userName);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
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
