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

        public static readonly string DATASET_TABLE_NAME = "Customer";
        private static readonly string DATABASE_TABLE_NAME = "dbo.Customer";
        private static SqlDataAdapter DATA_ADAPTER;
        private static DataSet DATA_SET;

        private string _firstName;
        private string _lastName;
        private string _email;


        public int Id { get; private set; } 
        public string FirstName { 
            get { return this._firstName; } 
            set {
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


        public static DataTable GetDataTable(SqlConnection connection) {
            return Customer.GetDataSet(connection).Tables[Customer.DATASET_TABLE_NAME];
        }

        public static void UpdateDataTable(SqlConnection connection) {
            Customer.GetDataAdapter(connection).Update(Customer.GetDataSet(connection), Customer.DATASET_TABLE_NAME);
            Customer.GetDataSet(connection).Tables[Customer.DATASET_TABLE_NAME].AcceptChanges();
        }

        private static SqlDataAdapter GetDataAdapter(SqlConnection connection) {
            // Similar to the previous GetDataSet method, this is also a universal accessor
            // It ensures that the static field DATA_ADAPTER is initiated correctly
            // and returns it.
            if (Customer.DATA_ADAPTER == null
                || Customer.DATA_ADAPTER.GetType() != typeof(SqlDataAdapter)) {
                Customer.DATA_ADAPTER = InitDataAdapter(connection);
            }
            return Customer.DATA_ADAPTER;
        }

        private static SqlDataAdapter InitDataAdapter(SqlConnection connection) {

            // Create a new DataAdapter object
            SqlDataAdapter adapter = new SqlDataAdapter {
                // Set the data adapter to load the database schema and primary key information.
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

                // this line tells the adapter to update the created row with the values
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
                "email = oldEmail);", connection);

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
            SqlCommand deleteCommand = new SqlCommand($"DELETE FROM {DATABASE_TABLE_NAME} WHERE id = @id;");
            deleteCommand.Parameters.Add("@id", SqlDbType.Int, 4, "id");

            // Add the created commands to the data adapter object
            adapter.SelectCommand = selectCommand;
            adapter.InsertCommand = insertCommand;
            adapter.UpdateCommand = updateCommand;
            adapter.DeleteCommand = deleteCommand;

            // This is a bit of a tricky bit:
            // We add event handlers that react to specific events that are raised when the
            // DataSet pushes updates to the database. These event handlers are methods defined
            // in this class.
            // RowUpdating is raised before the modifications are pushed
            // RowUpdated is raised after the modifications have been pushed
            adapter.RowUpdating += new SqlRowUpdatingEventHandler(DataAdapterOnRowUpdatingHandler);
            adapter.RowUpdated += new SqlRowUpdatedEventHandler(DataAdapterOnRowUpdatedHandler);

            return adapter;
        }

        private static DataSet GetDataSet(SqlConnection connection) {
            // This method is a universal accessor for the "Customer" DataSet.
            // This ensures that the same DataSet object is used across the board.
            // If the dataSet field is not instantiated, is not a DataSet-class object
            // or if it doesn't contain a "Customer" table, then it is created, filled
            // and stored.
            if (Customer.DATA_SET == null
                || Customer.DATA_SET.GetType() != typeof(DataSet)
                || !Customer.DATA_SET.Tables.Contains(Customer.DATASET_TABLE_NAME)) {

                Customer.DATA_SET = Customer.InitDataSet(connection);

            }
            // Then the dataSet, either an already existing one or a newly created one (see above)
            // is returned.
            return Customer.DATA_SET;
        }

        private static DataSet InitDataSet(SqlConnection connection) {

            DataSet dataSet = new DataSet();

            // Open the connection if it is not open alredy
            if (connection.State != ConnectionState.Open) {
                connection.Open();
            }

            // Fill the data set with tha data. It will be stored in a table named
            // with the value of DATASET_TABLE_NAME
            Customer.GetDataAdapter(connection).Fill(dataSet, DATASET_TABLE_NAME);

            // Set which column(s) is the primary key of the DataTable
            DataColumn[] keys = new DataColumn[1];
            keys[0] = dataSet.Tables[Customer.DATASET_TABLE_NAME].Columns["id"];
            dataSet.Tables[Customer.DATASET_TABLE_NAME].PrimaryKey = keys;

            // Set specific modifiers so the grid view blocks some abilities or so the update
            // allows empty values etc.
            dataSet.Tables[DATASET_TABLE_NAME].Columns["id"].ReadOnly = true;
            dataSet.Tables[DATASET_TABLE_NAME].Columns["id"].AutoIncrementStep = -1; // necessary when retrieving the id from database
            dataSet.Tables[DATASET_TABLE_NAME].Columns["id"].AutoIncrementSeed = 0; // necessary when retrieving the id from database
            dataSet.Tables[DATASET_TABLE_NAME].Columns["firstName"].AllowDBNull = true;
            dataSet.Tables[DATASET_TABLE_NAME].Columns["lastName"].AllowDBNull = true;
            dataSet.Tables[DATASET_TABLE_NAME].Columns["createdAt"].ReadOnly = true;
            dataSet.Tables[DATASET_TABLE_NAME].Columns["createdAt"].AllowDBNull = true;
            dataSet.Tables[DATASET_TABLE_NAME].Columns["deletedAt"].ReadOnly = true;
            dataSet.Tables[DATASET_TABLE_NAME].Columns["deletedAt"].AllowDBNull = true;

            return dataSet;
        }

        private static void DataAdapterOnRowUpdatingHandler(object sender, SqlRowUpdatingEventArgs args) {
            if (args.StatementType == StatementType.Insert) {
                args.Command.Parameters["@createdAt"].Value = DateTime.Now;
            }
        }

        private static void DataAdapterOnRowUpdatedHandler(object sender, SqlRowUpdatedEventArgs args) {
            if (args.StatementType == StatementType.Insert) {
                args.Status = UpdateStatus.SkipCurrentRow;
                if (args.RowCount == 0) {
                    throw new Exception($"Failed to update {typeof(Customer).FullName}: " +
                        $"no database entry found for Id# {args.Row["id"]}.");
                }
            } else if (args.StatementType == StatementType.Delete) {
                if (args.RowCount == 0) {
                    throw new Exception($"Failed to delete {typeof(Customer).FullName}: " +
                        $"no database entry found for Id# {args.Row["id"]}.");
                }
            } else if (args.StatementType == StatementType.Update) {
                if (args.RowCount == 0) {
                    throw new Exception($"Failed to update {typeof(Customer).FullName}: " +
                        $"no database entry found for Id# {args.Row["id"]} with values " +
                        $".");
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
