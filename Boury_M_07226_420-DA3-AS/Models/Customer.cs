/*
 * (c) Copyright 2022 Marc-Eric Boury
 */

using Boury_M_07226_420_DA3_AS.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Models {
    public class Customer : IModel<Customer> {

        /// <summary>
        /// A field representing the name of the table for this class in the dataset.
        /// </summary>
        private static readonly string DATASET_TABLE_NAME = "Customer";
        /// <summary>
        /// The name of the database table this class is associated with.
        /// </summary>
        private static readonly string DATABASE_TABLE_NAME = "dbo.Customer";
        /// <summary>
        /// the single SqlDataAdapter object for this class.
        /// </summary>
        private static SqlDataAdapter DATA_ADAPTER;
        /// <summary>
        /// The single DataSet object for this class.
        /// </summary>
        private static DataSet DATA_SET;

        private string _firstName;
        private string _lastName;
        private string _email;


        public int Id { get; private set; } 
        public string FirstName { 
            get { return this._firstName; } 
            set {
                // Validation for maximum length of this property, according to the database column.
                if (value.Length > 50) {
                    throw new Exception($"Field validation exception: length of value " +
                        $"for {this.GetType().FullName}.FirstName must be less or equal " +
                        $"than 50 charaters. Received: {value.Length}.");
                }
                this._firstName = value;
            }
        }
        public string LastName {
            get { return this._lastName; }
            set {
                // Validation for maximum length of this property, according to the database column.
                if (value.Length > 50) {
                    throw new Exception($"Field validation exception: length of value " +
                        $"for {this.GetType().FullName}.LastName must be less or equal " +
                        $"than 50 charaters. Received: {value.Length}.");
                }
                this._lastName = value;
            }
        }
        public string Email {
            get { return this._email; }
            set {
                // Validation for maximum length of this property, according to the database column.
                if (value.Length > 128) {
                    throw new Exception($"Field validation exception: length of value " +
                        $"for {this.GetType().FullName}.Email must be less or equal " +
                        $"than 128 charaters. Received: {value.Length}.");
                }
                this._email = value;
            }
        }
        public DateTime CreatedAt { get; set; }
        public DateTime DeletedAt { get; set; }


        public Customer(int id) {
            this.Id = id;
        }

        public Customer(string email) : this(email, "", "") {

        }

        public Customer(string email, string firstName, string lastName) {
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
        }


        #region DataSet Methods

        /// <summary>
        /// Retrieves the <see cref="DataTable"/> for this class from its <see cref="DataSet"/>. Calls 
        /// <see cref="GetDataSet(SqlConnection)"/> so if the <see cref="DataSet"/> is not created, it 
        /// will be created and filled, as well as the <see cref="SqlDataAdapter"/>.
        /// </summary>
        /// <param name="connection">The <see cref="SqlConnection"/> needed for dataset and dataadapter automatic creation.</param>
        /// <returns>The <see cref="DataTable"/> retrieved.</returns>
        public static DataTable GetDataTable(SqlConnection connection) {
            return Customer.GetDataSet(connection).Tables[Customer.DATASET_TABLE_NAME];
        }

        /// <summary>
        /// Updates the data source with all changes made into the <see cref="DataSet"/> of this class.
        /// Also finalizes the changes in the <see cref="DataSet"/> after the update was made.
        /// </summary>
        /// <param name="connection"></param>
        public static void UpdateDataTable(SqlConnection connection) {

            // Retrieve the SqlDataAdapter object of this class then call its Update method on this class's
            // DataSet object and table name.
            // Basically updates the data source with the modifications made in the DataTable associated
            // with this class.
            Customer.GetDataAdapter(connection).Update(Customer.GetDataSet(connection), Customer.DATASET_TABLE_NAME);

            // Finalize modifications (change status to unmodified) after said modifications have been pushed
            // to the data source. This is so we do not push the same modifications multiple times.
            Customer.GetDataSet(connection).Tables[Customer.DATASET_TABLE_NAME].AcceptChanges();
        }

        /// <summary>
        /// This method is a universal accessor for the "Customer" <see cref="SqlDataAdapter"/>.
        /// It ensures that the static field <see cref="DATA_ADAPTER"/> is initiated correctly
        /// and returns its value: if the field already has a <see cref="SqlDataAdapter"/> object in it,
        /// then it is returned, otherwise it creates it via the method <see cref="InitDataAdapter(SqlConnection)"/>, 
        /// sets it in the field and returns it.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private static SqlDataAdapter GetDataAdapter(SqlConnection connection) {
            // Check if the field is null or if its value is not of type SqlDataAdapter.
            // If so, it creates a SqlDataAdapter and populates the field with it.
            if (Customer.DATA_ADAPTER == null
                || Customer.DATA_ADAPTER.GetType() != typeof(SqlDataAdapter)) {

                Customer.DATA_ADAPTER = InitDataAdapter(connection);

            }
            return Customer.DATA_ADAPTER;
        }

        /// <summary>
        /// This large method creates and configures a <see cref="SqlDataAdapter"/> object for the class.
        /// This method is used by <see cref="GetDataAdapter(SqlConnection)"/> when ensuring the existence of the
        /// object in the <see cref="DATA_ADAPTER"/> field.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private static SqlDataAdapter InitDataAdapter(SqlConnection connection) {

            // Create a new DataAdapter object
            SqlDataAdapter adapter = new SqlDataAdapter() {
                // Set the data adapter to load the database schema and primary key information if it is missing.
                MissingSchemaAction = MissingSchemaAction.AddWithKey
            };

            // Create a "select" command object to load the data into the table.
            // Selects everything in the database table.
            SqlCommand selectCommand = new SqlCommand($"SELECT * FROM {DATABASE_TABLE_NAME};", connection);

            // Create an "insert" command to insert a new entry in the database.
            // The command also selects the newly created row based on the id value
            // given to it by the database. This is important for the DataSet to match
            // the created rows and have a correct id value.
            SqlCommand insertCommand = new SqlCommand($"INSERT INTO {DATABASE_TABLE_NAME} " +
                $"(firstName, lastName, email, createdAt) " +
                $"VALUES (@firstName, @lastName, @email, @createdAt); " +
                $"SELECT * FROM {DATABASE_TABLE_NAME} WHERE id = SCOPE_IDENTITY();", connection) {

                // This line tells the adapter to update the created row with the values
                // returned by the select part of the command (including created id).
                // Basically the inserted row's values are replaced by the returned row's
                UpdatedRowSource = UpdateRowSource.FirstReturnedRecord
            };

            // Add standard parameters with values to the insert command
            insertCommand.Parameters.Add("@firstName", SqlDbType.NVarChar, 50, "firstName");
            insertCommand.Parameters.Add("@lastName", SqlDbType.NVarChar, 50, "lastName");
            insertCommand.Parameters.Add("@email", SqlDbType.NVarChar, 128, "email");

            // Add the createdAt parameter, but with no value. If we set the value here
            // (to DateTime.Now) then the same DateTime value would be passed for every
            // insertion. We shall set the value upon execution of insertions through an
            // event handler (see method DataAdapterOnRowUpdatingHandler).
            insertCommand.Parameters.Add("@createdAt", SqlDbType.DateTime);

            // Create an "update" command. I have voluntary made my update command quite complicated
            // for you to see the way to block concurrent modification problems: The rows
            // will only be updated if all the conditions of the WHERE clause are met: if no
            // value has changed since the DataSet was loaded.
            SqlCommand updateCommand = new SqlCommand($"UPDATE {DATABASE_TABLE_NAME} SET " +
                "firstName = @firstName, " +
                "lastName = @lastName, " +
                "email = @email " +
                "WHERE (id = @id AND " +
                "firstName = @oldFirstName AND " +
                "lastName = @oldLastName AND " +
                "email = @oldEmail);", connection);

            // Add the normal parameters for the update values
            updateCommand.Parameters.Add("@firstName", SqlDbType.NVarChar, 50, "firstName");
            updateCommand.Parameters.Add("@lastName", SqlDbType.NVarChar, 50, "lastName");
            updateCommand.Parameters.Add("@email", SqlDbType.NVarChar, 128, "email");

            // Add parameters for the WHERE clause. Note the use of the "SourceVersion" properties
            // set to "DataRowVersion.Original" to signify that the values passed must be the original
            // values from before any modification to the row. You can see how having a single
            // updatedAt column would simplify this as we would only need to test on that column
            // instead all the other columns (as long as the updatedAt value is updated for each modification).

            updateCommand.Parameters.Add("@id", SqlDbType.Int, 4, "id");
            updateCommand.Parameters.Add("@oldFirstName", SqlDbType.NVarChar, 50, "firstName").SourceVersion = DataRowVersion.Original;
            updateCommand.Parameters.Add("@oldLastName", SqlDbType.NVarChar, 50, "lastName").SourceVersion = DataRowVersion.Original;
            updateCommand.Parameters.Add("@oldEmail", SqlDbType.NVarChar, 128, "email").SourceVersion = DataRowVersion.Original;

            // Create a "delete" command.
            SqlCommand deleteCommand = new SqlCommand($"DELETE FROM {DATABASE_TABLE_NAME} WHERE id = @id;", connection);
            deleteCommand.Parameters.Add("@id", SqlDbType.Int, 4, "id");

            // Add the created commands to the data adapter object
            adapter.SelectCommand = selectCommand;
            adapter.InsertCommand = insertCommand;
            adapter.UpdateCommand = updateCommand;
            adapter.DeleteCommand = deleteCommand;

            // This is a tricky bit:
            // We add event handlers that react to specific events that are raised when the
            // DataSet pushes updates to the database. These event handlers are methods defined
            // in this class.
            // RowUpdating is raised before the modifications are pushed
            // RowUpdated is raised after the modifications have been pushed
            adapter.RowUpdating += new SqlRowUpdatingEventHandler(DataAdapterOnRowUpdatingHandler);
            adapter.RowUpdated += new SqlRowUpdatedEventHandler(DataAdapterOnRowUpdatedHandler);

            return adapter;
        }

        /// <summary>
        /// This method is a universal accessor for the "Customer" <see cref="DataSet"/>. 
        /// It ensures that the static field <see cref="DATA_SET"/> is initiated correctly 
        /// and returns its value: if the field already has a <see cref="DataSet"/> object in it, 
        /// then it is returned, otherwise it creates it via the method <see cref="InitDataSet(SqlConnection)"/>, 
        /// sets it in the field and returns it.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private static DataSet GetDataSet(SqlConnection connection) {
            // Check if the field is null, if its value is not of type DataSet or if it
            // Doesn't contain a table of the correct name.
            if (Customer.DATA_SET == null
                || Customer.DATA_SET.GetType() != typeof(DataSet)
                || !Customer.DATA_SET.Tables.Contains(Customer.DATASET_TABLE_NAME)) {

                Customer.DATA_SET = Customer.InitDataSet(connection);

            }
            return Customer.DATA_SET;
        }

        /// <summary>
        /// This method creates and configures a <see cref="DataSet"/> object for the class.
        /// This method is used by <see cref="GetDataSet(SqlConnection)"/> when ensuring the existence of the
        /// object in the <see cref="DATA_SET"/> field.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private static DataSet InitDataSet(SqlConnection connection) {

            // create a new DataSet object
            DataSet dataSet = new DataSet();

            // Open the connection if it is not open alredy
            if (connection.State != ConnectionState.Open) {
                connection.Open();
            }

            // Fill the data set with tha data. It will be stored in a table named
            // with the value of the DATASET_TABLE_NAME static field.
            SqlDataAdapter adapter = Customer.GetDataAdapter(connection);
            adapter.Fill(dataSet, Customer.DATASET_TABLE_NAME);

            // Set which column(s) is the primary key of the DataTable for this class
            DataColumn[] keys = new DataColumn[1];
            DataTable customerTable = dataSet.Tables[Customer.DATASET_TABLE_NAME];
            DataColumn idColumn = customerTable.Columns["id"];
            keys[0] = idColumn;
            customerTable.PrimaryKey = keys;

            // Set specific modifiers so the grid view blocks some abilities or so the update
            // allows empty values etc.
            dataSet.Tables[Customer.DATASET_TABLE_NAME].Columns["id"].ReadOnly = true;
            dataSet.Tables[Customer.DATASET_TABLE_NAME].Columns["id"].AutoIncrementStep = -1; // necessary when retrieving the id from database
            dataSet.Tables[Customer.DATASET_TABLE_NAME].Columns["id"].AutoIncrementSeed = 0; // necessary when retrieving the id from database
            dataSet.Tables[Customer.DATASET_TABLE_NAME].Columns["firstName"].AllowDBNull = true;
            dataSet.Tables[Customer.DATASET_TABLE_NAME].Columns["lastName"].AllowDBNull = true;
            dataSet.Tables[Customer.DATASET_TABLE_NAME].Columns["deletedAt"].ReadOnly = true;
            dataSet.Tables[Customer.DATASET_TABLE_NAME].Columns["deletedAt"].AllowDBNull = true;
            dataSet.Tables[Customer.DATASET_TABLE_NAME].Columns["createdAt"].ReadOnly = true;
            // The createdAt parameter is set to allow NULLs even if the database doesn't because
            // the value is set in a RowUpdating event handler. Without this, an error would
            // be raised upon insertion
            dataSet.Tables[Customer.DATASET_TABLE_NAME].Columns["createdAt"].AllowDBNull = true;

            return dataSet;
        }

        /// <summary>
        /// Event handler that triggers when the <see cref="SqlDataAdapter.RowUpdating"/> event is raised
        /// (just before an actual push of modifications is done). It sets the value of the "createdAt" 
        /// to the current DateTime when the update happens and is an Insertion modification.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void DataAdapterOnRowUpdatingHandler(object sender, SqlRowUpdatingEventArgs args) {
            if (args.StatementType == StatementType.Insert) {
                // set the value of the "createdAt field to the current Datetime at insertion time.
                args.Command.Parameters["@createdAt"].Value = DateTime.Now;
            }
        }

        /// <summary>
        /// Event handler that triggers when the <see cref="SqlDataAdapter.RowUpdated"/> event is raised
        /// (just after an actual push of modifications is done). It throws exceptions if no changes have been made
        /// in the database during updates and deletions (WHERE clause failure). It also tells the <see cref="DataSet"/>
        /// to not finalize rows that are inserted (as they are reconstructed through a select method in the insert
        /// command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <exception cref="Exception"></exception>
        private static void DataAdapterOnRowUpdatedHandler(object sender, SqlRowUpdatedEventArgs args) {
            if (args.StatementType == StatementType.Insert) {
                // Tell the data set to skip the finalization of the current row being inserted.
                // The row is rebuilt with extra values from within the DataAdapter's insert command's
                // UpdatedRowSource property (set to the UpdateRowSource.FirstReturnedRow value).
                args.Status = UpdateStatus.SkipCurrentRow;

            } else if (args.StatementType == StatementType.Delete) {
                // Throw an exception if no row has been deleted when performing a deletion.
                if (args.RowCount == 0) {
                    throw new Exception($"Failed to delete {typeof(Customer).FullName}: " +
                        $"no database entry found for Id# {args.Row["id"]}.");
                }
            } else if (args.StatementType == StatementType.Update) {
                // throw an exception when no row has been updated when performing an update.
                if (args.RowCount == 0) {
                    throw new Exception($"Failed to update {typeof(Customer).FullName}: " +
                        $"no database entry found for Id# {args.Row["id"]} with matching originalvalues.");
                }
            }

        }


        #endregion



        #region Static Methods


        public static Customer GetById(int id) {
            Customer customer = new Customer(id);
            return customer.GetById();
        }


        public static Customer GetById(int id, SqlTransaction transaction, bool withExclusiveLock = false) {
            Customer customer = new Customer(id);
            return customer.GetById(transaction, withExclusiveLock);
        }


        #endregion



        #region Methods



        public void Delete() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                this.ExecuteDeleteCommand(connection.CreateCommand());
            }
        }

        public void Delete(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            this.ExecuteDeleteCommand(cmd);
        }

        private void ExecuteDeleteCommand(SqlCommand cmd) {
            if (this.Id == 0) {
                // Id has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteDeleteCommand() : Id value is 0.");
            }

            string statement = $"DELETE FROM {DATABASE_TABLE_NAME} WHERE Id = @id;";
            cmd.CommandText = statement;

            SqlParameter param = cmd.CreateParameter();
            param.ParameterName = "@id";
            param.DbType = DbType.Int32;
            param.Value = this.Id;
            cmd.Parameters.Add(param);

            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            int affectedRows = cmd.ExecuteNonQuery();

            if (!(affectedRows > 0)) {
                // No affected rows: no deletion occured -> row with matching Id not found
                throw new Exception($"Failed to delete {this.GetType().FullName}: no database entry found for Id# {this.Id}.");
            }
        }



        public Customer GetById() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteGetByIdCommand(connection.CreateCommand());
            }
        }

        public Customer GetById(SqlTransaction transaction, bool withExclusiveLock = false) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteGetByIdCommand(cmd, withExclusiveLock);
        }

        private Customer ExecuteGetByIdCommand(SqlCommand cmd, bool withExclusiveLock = false) {
            if (this.Id == 0) {
                // Id has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteGetByIdCommand() : Id value is 0.");
            }

            string statement = $"SELECT * FROM {DATABASE_TABLE_NAME} " +
                (withExclusiveLock ? "WITH  (XLOCK, ROWLOCK) " : "") +
                "WHERE Id = @id;";
            cmd.CommandText = statement;

            SqlParameter param = cmd.CreateParameter();
            param.ParameterName = "@id";
            param.DbType = DbType.Int32;
            param.Value = this.Id;
            cmd.Parameters.Add(param);

            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows) {
                reader.Read();

                // firstName and lastName can be NULL in the database
                if (!reader.IsDBNull(1)) {
                    this.FirstName = reader.GetString(1);
                }
                if (!reader.IsDBNull(2)) {
                    this.LastName = reader.GetString(2);
                }
                this.Email = reader.GetString(3);
                this.CreatedAt = reader.GetDateTime(4);
                // deletedAt can be NULL in the database
                if (!reader.IsDBNull(5)) {
                    this.DeletedAt = reader.GetDateTime(5);
                }

                return this;

            } else {
                throw new Exception($"No database entry for {this.GetType().FullName} with id# {this.Id}.");
            }

        }



        public Customer Insert() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteInsertCommand(connection.CreateCommand());
            }
        }

        public Customer Insert(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteInsertCommand(cmd);
        }

        private Customer ExecuteInsertCommand(SqlCommand cmd) {
            if (this.Id > 0) {
                // Id has been set, cannot insert a product with a specific Id without risking
                // to mess up the database.
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteInsertCommand() : Id value is not 0 [{this.Id}].");
            }

            // define the time of the creation (now). We will use it to set the value in the DB and
            // in the object here
            DateTime createTime = DateTime.Now;

            // Create the INSERT statement. We do not pass any Id value since this is insertion
            // and the id is auto-generated by the database on insertion (identity).
            string statement = $"INSERT INTO {DATABASE_TABLE_NAME} (firstName, lastName, email, createdAt) " +
                "VALUES (@firstName, @lastName, @email, @createdAt); SELECT CAST(SCOPE_IDENTITY() AS int);";
            cmd.CommandText = statement;

            // Create and add parameters
            SqlParameter firstNameParam = cmd.CreateParameter();
            firstNameParam.ParameterName = "@firstName";
            firstNameParam.DbType = DbType.String;
            if (String.IsNullOrEmpty(this.FirstName)) {
                firstNameParam.Value = DBNull.Value;
            } else {
                firstNameParam.Value = this.FirstName;
            }
            cmd.Parameters.Add(firstNameParam);

            SqlParameter lastNameParam = cmd.CreateParameter();
            lastNameParam.ParameterName = "@lastName";
            lastNameParam.DbType = DbType.String;
            if (String.IsNullOrEmpty(this.LastName)) {
                lastNameParam.Value = DBNull.Value;
            } else {
                lastNameParam.Value = this.LastName;
            }
            cmd.Parameters.Add(lastNameParam);

            SqlParameter emailParam = cmd.CreateParameter();
            emailParam.ParameterName = "@email";
            emailParam.DbType = DbType.String;
            emailParam.Value = this.Email;
            cmd.Parameters.Add(emailParam);

            SqlParameter createdAtParam = cmd.CreateParameter();
            createdAtParam.ParameterName = "@createdAt";
            createdAtParam.DbType = DbType.DateTime;
            createdAtParam.Value = createTime;
            cmd.Parameters.Add(createdAtParam);

            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            this.Id = (Int32)cmd.ExecuteScalar();
            this.CreatedAt = createTime;

            return this;

        }



        public Customer Update() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteUpdateCommand(connection.CreateCommand());
            }
        }

        public Customer Update(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteUpdateCommand(cmd);
        }

        private Customer ExecuteUpdateCommand(SqlCommand cmd) {
            if (this.Id == 0) {
                // Id has not been set, cannot update a product with no specific Id to track the correct db row.
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteUpdateCommand() : Id value is 0.");
            }

            // Create the Update statement.
            string statement = $"UPDATE {DATABASE_TABLE_NAME} SET " +
                "firstName = @firstName, " +
                "lastName = @lastName, " +
                "email = @email " +
                "WHERE Id = @id;";
            cmd.CommandText = statement;

            // Create and add parameters
            SqlParameter firstNameParam = cmd.CreateParameter();
            firstNameParam.ParameterName = "@firstName";
            firstNameParam.DbType = DbType.String;
            if (String.IsNullOrEmpty(this.FirstName)) {
                firstNameParam.Value = DBNull.Value;
            } else {
                firstNameParam.Value = this.FirstName;
            }
            cmd.Parameters.Add(firstNameParam);

            SqlParameter lastNameParam = cmd.CreateParameter();
            lastNameParam.ParameterName = "@lastName";
            lastNameParam.DbType = DbType.String;
            if (String.IsNullOrEmpty(this.LastName)) {
                firstNameParam.Value = DBNull.Value;
            } else {
                firstNameParam.Value = this.LastName;
            }
            cmd.Parameters.Add(lastNameParam);

            SqlParameter emailParam = cmd.CreateParameter();
            emailParam.ParameterName = "@email";
            emailParam.DbType = DbType.String;
            emailParam.Value = this.Email;
            cmd.Parameters.Add(emailParam);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            int affectedRows = cmd.ExecuteNonQuery();

            // Check that a row has been updated, if not, throw exception (no row with the id
            // value found in the database, thus no update done)
            if (!(affectedRows > 0)) {
                throw new Exception($"Failed to update {this.GetType().FullName}: no database entry found for Id# {this.Id}.");
            }

            return this;
        }

        #endregion


    }
}
