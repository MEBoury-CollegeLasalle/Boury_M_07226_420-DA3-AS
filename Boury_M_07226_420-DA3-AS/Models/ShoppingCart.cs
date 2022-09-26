/*
 * (c) Copyright 2022 Marc-Eric Boury
 */

using Boury_M_07226_420_DA3_AS.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Models {
    public class ShoppingCart : IModel<ShoppingCart> {

        public static readonly string DATASET_TABLE_NAME = "ShoppingCart";
        private static readonly string DATABASE_TABLE_NAME = "dbo.ShoppingCart";
        private static SqlDataAdapter DATA_ADAPTER;
        private static DataSet DATA_SET;

        public int Id { get; private set; }
        public Customer Customer { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime DateOrdered { get; set; }
        public DateTime DateShipped { get; set; }


        public ShoppingCart(int id) {
            this.Id = id;
        }

        public ShoppingCart(Customer customer, string billingAddress, string shippingAddress) {
            this.Customer = customer;
            this.BillingAddress = billingAddress;
            this.ShippingAddress = shippingAddress;
        }



        #region DataSet Methods


        public static DataTable GetDataTable(SqlConnection connection) {
            return ShoppingCart.GetDataSet(connection).Tables[ShoppingCart.DATASET_TABLE_NAME];
        }

        public static void UpdateDataTable(SqlConnection connection) {
            ShoppingCart.GetDataAdapter(connection).Update(ShoppingCart.GetDataSet(connection), ShoppingCart.DATASET_TABLE_NAME);
            ShoppingCart.GetDataSet(connection).Tables[ShoppingCart.DATASET_TABLE_NAME].AcceptChanges();
        }

        public static SqlDataAdapter GetDataAdapter(SqlConnection connection) {
            // Similar to the previous GetDataSet method, this is also a universal accessor
            // It ensures that the static field DATA_ADAPTER is initiated correctly
            // and returns it.
            if (ShoppingCart.DATA_ADAPTER == null
                || ShoppingCart.DATA_ADAPTER.GetType() != typeof(SqlDataAdapter)) {
                ShoppingCart.DATA_ADAPTER = InitDataAdapter(connection);
            }
            return ShoppingCart.DATA_ADAPTER;
        }

        private static SqlDataAdapter InitDataAdapter(SqlConnection connection) {

            // Create a new DataAdapter object
            SqlDataAdapter adapter = new SqlDataAdapter();

            // Set the data adapter to load the database schema and primary key information.
            adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

            // Create a "select" command object to load the data into the table.
            // Selects everything in the database table.
            SqlCommand selectCommand = new SqlCommand($"SELECT * FROM {DATABASE_TABLE_NAME};", connection);

            // Create an "insert" command to insert a new entry in the database.
            // The command also selects the newly created row based on the id value
            // given to it by the database. This is important for the DataSet to match
            // the created rows and have a correct id value.
            SqlCommand insertCommand = new SqlCommand($"INSERT INTO {DATABASE_TABLE_NAME} " +
                $"(customerId, billingAddress, shippingAddress, dateCreated) " +
                $"VALUES (@customerId, @billingAddress, @shippingAddress, @dateCreated); " +
                $"SELECT * FROM {DATABASE_TABLE_NAME} WHERE id = SCOPE_IDENTITY();", connection);
            // this line tells the adapter to update the created row with the values
            // returned by the select part of the command (including created id).
            // Basically the inserted row's values are replaced by the returned row's
            insertCommand.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord;

            // Add standard parameters with values to the insert command
            insertCommand.Parameters.Add("@customerId", SqlDbType.Int, 4, "customerId");
            insertCommand.Parameters.Add("@billingAddress", SqlDbType.Text, -1, "billingAddress");
            insertCommand.Parameters.Add("@shippingAddress", SqlDbType.Text, -1, "shippingAddress");

            // Add the dateCreated parameter, but with no value. If we set the value here
            // (to DateTime.Now) then the same DateTime value would be passed for every
            // insertion. We shall set the value upon execution of insertions through an
            // event handler (see method DataAdapterOnRowUpdatingHandler).
            insertCommand.Parameters.Add("@dateCreated", SqlDbType.DateTime);

            // Create an "update" command. I have voluntary made my update command quite complicated
            // for you to see the way to block concurrent modification problems: The rows
            // will only be updated if all the conditions of the WHERE clause are met: if no
            // value has changed since the DataSet was loaded.
            SqlCommand updateCommand = new SqlCommand($"UPDATE {DATABASE_TABLE_NAME} SET " +
                "customerId = @customerId, " +
                "billingAddress = @billingAddress, " +
                "shippingAddress = @shippingAddress " +
                "WHERE (id = @id AND " +
                "dateUpdated = @oldDateUpdated);", connection);

            // Add the normal parameters for the update values
            updateCommand.Parameters.Add("@customerId", SqlDbType.Int, 4, "customerId");
            updateCommand.Parameters.Add("@billingAddress", SqlDbType.Text, -1, "billingAddress");
            updateCommand.Parameters.Add("@shippingAddress", SqlDbType.Text, -1, "shippingAddress");
            // Add parameters for the WHERE clause. Note the use of the "SourceVersion" properties
            // set to "DataRowVersion.Original" to signify that the values passed must be the original
            // values from before any modification to the row.
            // In this case we can see how having a single dateUpdated column can make anti concurrent
            // modifications measures much more simple.
            updateCommand.Parameters.Add("@id", SqlDbType.Int, 4, "id");
            SqlParameter dateUpdParam = updateCommand.Parameters.Add("@dateUpdated", SqlDbType.DateTime);
            dateUpdParam.SourceColumn = "dateUpdated";
            dateUpdParam.SourceVersion = DataRowVersion.Original;

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

        public static DataSet GetDataSet(SqlConnection connection) {
            // This method is a universal accessor for the "ShoppingCart" DataSet.
            // This ensures that the same DataSet object is used across the board.
            // If the dataSet field is not instantiated, is not a DataSet-class object
            // or if it doesn't contain a "ShoppingCart" table, then it is created, filled
            // and stored.
            if (ShoppingCart.DATA_SET == null
                || ShoppingCart.DATA_SET.GetType() != typeof(DataSet)
                || !ShoppingCart.DATA_SET.Tables.Contains(ShoppingCart.DATASET_TABLE_NAME)) {

                ShoppingCart.DATA_SET = ShoppingCart.InitDataSet(connection);
            }
            // Then the dataSet, either an already existing one or a newly created one (see above)
            // is returned.
            return ShoppingCart.DATA_SET;
        }

        private static DataSet InitDataSet(SqlConnection connection = null) {

            DataSet dataSet = new DataSet();

            // Open the connection if it is not open alredy
            if (connection.State != ConnectionState.Open) {
                connection.Open();
            }

            // Fill the data set with tha data. It will be stored in a table named
            // with the value of DATASET_TABLE_NAME
            ShoppingCart.GetDataAdapter(connection).Fill(dataSet, ShoppingCart.DATASET_TABLE_NAME);

            // Set which column(s) is the primary key of the DataTable
            DataColumn[] keys = new DataColumn[1];
            keys[0] = dataSet.Tables[ShoppingCart.DATASET_TABLE_NAME].Columns["id"];
            dataSet.Tables[ShoppingCart.DATASET_TABLE_NAME].PrimaryKey = keys;


            dataSet.Tables[ShoppingCart.DATASET_TABLE_NAME].Columns["id"].ReadOnly = true;
            dataSet.Tables[ShoppingCart.DATASET_TABLE_NAME].Columns["id"].AutoIncrementStep = -1;
            dataSet.Tables[ShoppingCart.DATASET_TABLE_NAME].Columns["id"].AutoIncrementSeed = 0;
            dataSet.Tables[ShoppingCart.DATASET_TABLE_NAME].Columns["dateCreated"].ReadOnly = true;
            dataSet.Tables[ShoppingCart.DATASET_TABLE_NAME].Columns["dateUpdated"].ReadOnly = true;
            dataSet.Tables[ShoppingCart.DATASET_TABLE_NAME].Columns["dateOrdered"].ReadOnly = true;
            dataSet.Tables[ShoppingCart.DATASET_TABLE_NAME].Columns["dateShipped"].ReadOnly = true;
            dataSet.Tables[ShoppingCart.DATASET_TABLE_NAME].Columns["dateCreated"].AllowDBNull = true;
            dataSet.Tables[ShoppingCart.DATASET_TABLE_NAME].Columns["dateUpdated"].AllowDBNull = true;
            dataSet.Tables[ShoppingCart.DATASET_TABLE_NAME].Columns["dateOrdered"].AllowDBNull = true;
            dataSet.Tables[ShoppingCart.DATASET_TABLE_NAME].Columns["dateShipped"].AllowDBNull = true;

            return dataSet;
        }

        private static void DataAdapterOnRowUpdatingHandler(object sender, SqlRowUpdatingEventArgs args) {
            if (args.StatementType == StatementType.Insert) {
                args.Command.Parameters["@dateCreated"].Value = DateTime.Now;
            } else if (args.StatementType == StatementType.Update) {
                args.Command.Parameters["@dateUpdated"].Value = DateTime.Now;
            }
        }

        private static void DataAdapterOnRowUpdatedHandler(object sender, SqlRowUpdatedEventArgs args) {
            if (args.StatementType == StatementType.Insert) {
                args.Status = UpdateStatus.SkipCurrentRow;

            } else if (args.StatementType == StatementType.Delete) {
                if (args.RowCount == 0) {
                    throw new Exception($"Failed to delete {typeof(ShoppingCart).FullName}: " +
                        $"no database entry found for Id# {args.Row["id"]}.");
                }
            } else if (args.StatementType == StatementType.Update) {
                if (args.RowCount == 0) {
                    throw new Exception($"Failed to update {typeof(ShoppingCart).FullName}: " +
                        $"no database entry found for Id# {args.Row["id"]} with matching originalvalues.");
                }
            }

        }


        #endregion



        #region Static Methods


        public static ShoppingCart GetById(int id) {
            ShoppingCart cart = new ShoppingCart(id);
            return cart.GetById();
        }
        public static ShoppingCart GetById(int id, SqlTransaction transaction, bool withExclusiveLock = false) {
            ShoppingCart cart = new ShoppingCart(id);
            return cart.GetById(transaction, withExclusiveLock);
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



        public ShoppingCart GetById() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteGetByIdCommand(connection.CreateCommand());
            }
        }

        public ShoppingCart GetById(SqlTransaction transaction, bool withExclusiveLock = false) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteGetByIdCommand(cmd, withExclusiveLock);
        }

        private ShoppingCart ExecuteGetByIdCommand(SqlCommand cmd, bool withExclusiveLock = false) {
            if (this.Id == 0) {
                // Id has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteGetByIdCommand() : Id value is 0.");
            }

            string statement = $"SELECT * FROM {DATABASE_TABLE_NAME} " +
                (withExclusiveLock ? "WITH  (XLOCK, ROWLOCK) " : "") +
                $"WHERE Id = @id;";
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

                // check if we have a transaction set in the command passed as parameter argument
                if (cmd.Transaction != null) {
                    // If there is a transaction, use it when retrieving the customer associated with the shopping cart
                    this.Customer = new Customer(reader.GetInt32(1)).GetById(cmd.Transaction, withExclusiveLock);
                } else {
                    // If there is not, retrieve the customer without a transaction.
                    this.Customer = new Customer(reader.GetInt32(1)).GetById();
                }
                this.BillingAddress = reader.GetString(2);
                this.ShippingAddress = reader.GetString(3);
                this.DateCreated = reader.GetDateTime(4);
                if (!reader.IsDBNull(5)) {
                    this.DateUpdated = reader.GetDateTime(5);
                }
                if (!reader.IsDBNull(6)) {
                    this.DateOrdered = reader.GetDateTime(6);
                }
                if (!reader.IsDBNull(7)) {
                    this.DateShipped = reader.GetDateTime(7);
                }

                return this;

            } else {
                throw new Exception($"No database entry for {this.GetType().FullName} with id# {this.Id}.");
            }
        }



        public ShoppingCart Insert() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteInsertCommand(connection.CreateCommand());
            }
        }

        public ShoppingCart Insert(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteInsertCommand(cmd);
        }

        private ShoppingCart ExecuteInsertCommand(SqlCommand cmd) {
            if (this.Id != 0) {
                throw new Exception($"Cannot use method {this.GetType().FullName}.Insert() : Id value is not 0 [{this.Id}].");
            }

            // define the time of the creation (now). We will use it to set the value in the DB and
            // in the object here
            DateTime createTime = DateTime.Now;


            string insertCommandText = $"INSERT INTO {DATABASE_TABLE_NAME} (customerId, " +
                "billingAddress, shippingAddress, dateCreated) " +
                "VALUES (@customerId, @billingAddress, @shippingAddress, @dateCreated); " +
                "SELECT CAST(SCOPE_IDENTITY() AS int);";
            cmd.CommandText = insertCommandText;

            // Create and add parameters
            SqlParameter customerIdParam = cmd.CreateParameter();
            customerIdParam.ParameterName = "@customerId";
            customerIdParam.DbType = DbType.Int32;
            customerIdParam.Value = this.Customer.Id;
            cmd.Parameters.Add(customerIdParam);

            SqlParameter billingAddrParam = cmd.CreateParameter();
            billingAddrParam.ParameterName = "@billingAddress";
            billingAddrParam.DbType = DbType.String;
            billingAddrParam.Value = this.BillingAddress;
            cmd.Parameters.Add(billingAddrParam);

            SqlParameter shippingAddrParam = cmd.CreateParameter();
            shippingAddrParam.ParameterName = "@shippingAddress";
            shippingAddrParam.DbType = DbType.String;
            shippingAddrParam.Value = this.ShippingAddress;
            cmd.Parameters.Add(shippingAddrParam);

            SqlParameter dateUpdatedParam = cmd.CreateParameter();
            dateUpdatedParam.ParameterName = "@dateCreated";
            dateUpdatedParam.DbType = DbType.DateTime;
            dateUpdatedParam.Value = createTime;
            cmd.Parameters.Add(dateUpdatedParam);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            this.Id = (Int32)cmd.ExecuteScalar();
            this.DateCreated = createTime;

            return this;

        }



        public ShoppingCart Update() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteUpdateCommand(connection.CreateCommand());
            }
        }

        public ShoppingCart Update(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteUpdateCommand(cmd);
        }

        private ShoppingCart ExecuteUpdateCommand(SqlCommand cmd) {
            if (this.Id == 0) {
                // Id has not been set, cannot update a product with no specific Id to track the correct db row.
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteUpdateCommand() : Id value is 0.");
            }

            // define the time of the update (now). We will use it to set the value in the DB and
            // in the object here
            DateTime updateTime = DateTime.Now;

            // Create the Update statement.
            string updateCommandText = $"UPDATE {DATABASE_TABLE_NAME} SET " +
                "customerId = @customerId, " +
                "billingAddress = @billingAddress, " +
                "shippingAddress = @shippingAddress, " +
                "dateUpdated = @dateUpdated, " +
                "dateOrdered = @dateOrdered, " +
                "dateShipped = @dateShipped " +
                "WHERE Id = @id;";
            cmd.CommandText = updateCommandText;

            // Create and add parameters
            SqlParameter customerIdParam = cmd.CreateParameter();
            customerIdParam.ParameterName = "@customerId";
            customerIdParam.DbType = DbType.Int32;
            customerIdParam.Value = this.Customer.Id;
            cmd.Parameters.Add(customerIdParam);

            SqlParameter billingAddrParam = cmd.CreateParameter();
            billingAddrParam.ParameterName = "@billingAddress";
            billingAddrParam.DbType = DbType.String;
            billingAddrParam.Value = this.BillingAddress;
            cmd.Parameters.Add(billingAddrParam);

            SqlParameter shippingAddrParam = cmd.CreateParameter();
            shippingAddrParam.ParameterName = "@shippingAddress";
            shippingAddrParam.DbType = DbType.String;
            shippingAddrParam.Value = this.ShippingAddress;
            cmd.Parameters.Add(shippingAddrParam);

            SqlParameter dateUpdatedParam = cmd.CreateParameter();
            dateUpdatedParam.ParameterName = "@dateUpdated";
            dateUpdatedParam.DbType = DbType.DateTime;
            dateUpdatedParam.Value = updateTime;
            cmd.Parameters.Add(dateUpdatedParam);

            SqlParameter dateOrderedParam = cmd.CreateParameter();
            dateOrderedParam.ParameterName = "@dateOrdered";
            dateOrderedParam.DbType = DbType.DateTime;
            if (this.DateOrdered == DateTime.MinValue) {
                dateOrderedParam.Value = DBNull.Value;
            } else {
                dateOrderedParam.Value = this.DateOrdered;
            }
            cmd.Parameters.Add(dateOrderedParam);

            SqlParameter dateShippedParam = cmd.CreateParameter();
            dateShippedParam.ParameterName = "@dateShipped";
            dateShippedParam.DbType = DbType.DateTime;
            if (this.DateShipped == DateTime.MinValue) {
                dateOrderedParam.Value = DBNull.Value;
            } else {
                dateOrderedParam.Value = this.DateShipped;
            }
            cmd.Parameters.Add(dateShippedParam);

            SqlParameter wheteIdParam = cmd.CreateParameter();
            wheteIdParam.ParameterName = "@id";
            wheteIdParam.DbType = DbType.Int32;
            wheteIdParam.Value = this.Id;
            cmd.Parameters.Add(wheteIdParam);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            int affectedRows = cmd.ExecuteNonQuery();

            // Check that a row has been updated, if not, throw exception (no row with the id
            // value found in the database, thus no update done)
            if (affectedRows < 1) {
                throw new Exception($"Failed to update {this.GetType().FullName}: no database entry found for Id# {this.Id}.");
            }

            // database update has completed successfully at this point. update the property in the
            // object to the dateUpdated value we sent to the database
            this.DateUpdated = updateTime;

            return this;

        }


        #endregion


    }
}
