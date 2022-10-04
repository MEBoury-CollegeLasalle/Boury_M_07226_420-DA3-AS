/*
 * (c) Copyright 2022 Marc-Eric Boury
 */

using Boury_M_07226_420_DA3_AS.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Models {
    public class Product : IModel<Product> {

        public static readonly string DATASET_TABLE_NAME = "Product";
        private static readonly string DATABASE_TABLE_NAME = "dbo.Product";
        private static SqlDataAdapter DATA_ADAPTER;
        private static DataSet DATA_SET;

        public int Id { get; private set; }
        public long GtinCode { get; set; }
        public int QtyInStock { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }


        #region Constructors


        public Product(int id) {
            this.Id = id;
        }

        public Product(string name) : this(name, 0, 0L, "") {
        }

        public Product(string name, int qtyInStock) : this(name, qtyInStock, 0L, "") {
        }

        public Product(string name, int qtyInStock, long gtinCode) : this(name, qtyInStock, gtinCode, "") {
        }

        public Product(string name, int qtyInStock, string description) : this(name, qtyInStock, 0L, description) {
        }

        public Product(string name, int qtyInStock, long gtinCode, string description) {
            this.Name = name;
            this.QtyInStock = qtyInStock;
            this.GtinCode = gtinCode;
            this.Description = description;
        }


        #endregion



        #region DataSet Methods


        public static DataTable GetDataTable(SqlConnection connection) {
            return Product.GetDataSet(connection).Tables[Product.DATASET_TABLE_NAME];
        }

        public static void UpdateDataTable(SqlConnection connection) {
            Product.GetDataAdapter(connection).Update(Product.GetDataSet(connection), Product.DATASET_TABLE_NAME);
            Product.GetDataSet(connection).Tables[Product.DATASET_TABLE_NAME].AcceptChanges();
        }

        public static SqlDataAdapter GetDataAdapter(SqlConnection connection) {
            // Similar to the previous GetDataSet method, this is also a universal accessor
            // It ensures that the static field DATA_ADAPTER is initiated correctly
            // and returns it.
            if (Product.DATA_ADAPTER == null
                || Product.DATA_ADAPTER.GetType() != typeof(SqlDataAdapter)) {
                Product.DATA_ADAPTER = InitDataAdapter(connection);
            }
            return Product.DATA_ADAPTER;
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
                $"(gtinCode, qtyInStock, name, description) " +
                $"VALUES (@gtinCode, @qtyInStock, @name, @description); " +
                $"SELECT * FROM {DATABASE_TABLE_NAME} WHERE id = SCOPE_IDENTITY();", connection);
            // this line tells the adapter to update the created row with the values
            // returned by the select part of the command (including created id).
            // Basically the inserted row's values are replaced by the returned row's
            insertCommand.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord;

            // Add standard parameters with values to the insert command
            insertCommand.Parameters.Add("@gtinCode", SqlDbType.BigInt, 8, "gtinCode");
            insertCommand.Parameters.Add("@qtyInStock", SqlDbType.Int, 4, "qtyInStock");
            insertCommand.Parameters.Add("@name", SqlDbType.Text, -1, "name");
            insertCommand.Parameters.Add("@description", SqlDbType.Text, -1, "description");

            // Create an "update" command. I have voluntary made my update command quite complicated
            // for you to see the way to block concurrent modification problems: The rows
            // will only be updated if all the conditions of the WHERE clause are met: if no
            // value has changed since the DataSet was loaded.
            SqlCommand updateCommand = new SqlCommand($"UPDATE {DATABASE_TABLE_NAME} SET " +
                "gtinCode = @gtinCode, " +
                "qtyInStock = @qtyInStock, " +
                "name = @name " +
                "description = @description " +
                "WHERE (id = @id AND " +
                "gtinCode = @oldGtinCode AND " +
                "qtyInStock = @oldQtyInStock AND " +
                "name = @oldName AND " +
                "description = @oldDescription);", connection);

            // Add the normal parameters for the update values
            updateCommand.Parameters.Add("@gtinCode", SqlDbType.BigInt, 8, "gtinCode");
            updateCommand.Parameters.Add("@qtyInStock", SqlDbType.Int, 4, "qtyInStock");
            updateCommand.Parameters.Add("@name", SqlDbType.Text, -1, "name");
            updateCommand.Parameters.Add("@description", SqlDbType.Text, -1, "description");
            // Add parameters for the WHERE clause. Note the use of the "SourceVersion" properties
            // set to "DataRowVersion.Original" to signify that the values passed must be the original
            // values from before any modification to the row. You can see how having a single
            // updatedAt column would simplify this as we would only need to test on that column
            // instead all the other columns (as long as the updatedAt value is updated upon each modification).
            updateCommand.Parameters.Add("@id", SqlDbType.Int, 4, "id");
            updateCommand.Parameters.Add("@oldGtinCode", SqlDbType.BigInt, 8, "gtinCode").SourceVersion = DataRowVersion.Original;
            updateCommand.Parameters.Add("@oldQtyInStock", SqlDbType.Int, 4, "qtyInStock").SourceVersion = DataRowVersion.Original;
            updateCommand.Parameters.Add("@oldName", SqlDbType.Text, -1, "name").SourceVersion = DataRowVersion.Original;
            updateCommand.Parameters.Add("@oldDescription", SqlDbType.Text, -1, "description").SourceVersion = DataRowVersion.Original;

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
            // This method is a universal accessor for the "Product" DataSet.
            // This ensures that the same DataSet object is used across the board.
            // If the dataSet field is not instantiated, is not a DataSet-class object
            // or if it doesn't contain a "Product" table, then it is created, filled
            // and stored.
            if (Product.DATA_SET == null
                || Product.DATA_SET.GetType() != typeof(DataSet)
                || !Product.DATA_SET.Tables.Contains(Product.DATASET_TABLE_NAME)) {

                Product.DATA_SET = Product.InitDataSet(connection);
            }
            // Then the dataSet, either an already existing one or a newly created one (see above)
            // is returned.
            return Product.DATA_SET;
        }

        private static DataSet InitDataSet(SqlConnection connection = null) {

            DataSet dataSet = new DataSet();

            // Open the connection if it is not open alredy
            if (connection.State != ConnectionState.Open) {
                connection.Open();
            }

            // Fill the data set with tha data. It will be stored in a table named
            // with the value of DATASET_TABLE_NAME
            Product.GetDataAdapter(connection).Fill(dataSet, Product.DATASET_TABLE_NAME);

            // Set which column(s) is the primary key of the DataTable
            DataColumn[] keys = new DataColumn[1];
            keys[0] = dataSet.Tables[Product.DATASET_TABLE_NAME].Columns["id"];
            dataSet.Tables[Product.DATASET_TABLE_NAME].PrimaryKey = keys;


            dataSet.Tables[Product.DATASET_TABLE_NAME].Columns["id"].ReadOnly = true;
            dataSet.Tables[Product.DATASET_TABLE_NAME].Columns["id"].AutoIncrementStep = -1;
            dataSet.Tables[Product.DATASET_TABLE_NAME].Columns["id"].AutoIncrementSeed = 0;
            dataSet.Tables[Product.DATASET_TABLE_NAME].Columns["gtinCode"].AllowDBNull = true;
            dataSet.Tables[Product.DATASET_TABLE_NAME].Columns["description"].AllowDBNull = true;

            return dataSet;
        }

        private static void DataAdapterOnRowUpdatingHandler(object sender, SqlRowUpdatingEventArgs args) {

        }

        private static void DataAdapterOnRowUpdatedHandler(object sender, SqlRowUpdatedEventArgs args) {
            if (args.StatementType == StatementType.Insert) {
                args.Status = UpdateStatus.SkipCurrentRow;

            } else if (args.StatementType == StatementType.Delete) {
                if (args.RecordsAffected == 0) {
                    throw new Exception($"Failed to delete {typeof(Product).FullName}: " +
                        $"no database entry found for Id# {args.Row["id"]}.");
                }
            } else if (args.StatementType == StatementType.Update) {
                if (args.RecordsAffected == 0) {
                    throw new Exception($"Failed to update {typeof(Product).FullName}: " +
                        $"no database entry found for Id# {args.Row["id"]} with matching originalvalues.");
                }
            }

        }


        #endregion



        #region Static Methods



        public static Product GetById(int id) {
            Product product = new Product(id);
            return product.GetById();
        }

        public static Product GetById(int id, SqlTransaction transaction, bool withExclusiveLock = false) {
            Product product = new Product(id);
            return product.GetById(transaction, withExclusiveLock);
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



        public Product GetById() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteGetByIdCommand(connection.CreateCommand());
            }
        }

        public Product GetById(SqlTransaction transaction, bool withExclusiveLock = false) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteGetByIdCommand(cmd, withExclusiveLock);
        }

        private Product ExecuteGetByIdCommand(SqlCommand cmd, bool withExclusiveLock = false) {
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

                // gtinCode in the database can be NULL
                if (!reader.IsDBNull(1)) {
                    this.GtinCode = reader.GetInt64(1);
                }
                this.QtyInStock = reader.GetInt32(2);
                this.Name = reader.GetString(3);
                // gtinCode in the database can be NULL
                if (!reader.IsDBNull(4)) {
                    this.Description = reader.GetString(4);
                }

                return this;

            } else {
                throw new Exception($"No database entry for {this.GetType().FullName} with id# {this.Id}.");
            }
        }



        public Product Insert() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteInsertCommand(connection.CreateCommand());
            }
        }

        public Product Insert(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteInsertCommand(cmd);
        }

        private Product ExecuteInsertCommand(SqlCommand cmd) {
            if (this.Id > 0) {
                // Id has been set, cannot insert a product with a specific Id without risking
                // to mess up the database.
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteInsertCommand() : Id value is not 0 [{this.Id}].");
            }

            // Create the INSERT statement. We do not pass any Id value since this is insertion
            // and the id is auto-generated by the database on insertion (identity).
            string statement = $"INSERT INTO {DATABASE_TABLE_NAME} (gtinCode, qtyInStock, name, description) " +
                "VALUES (@gtinCode, @qtyInStock, @name, @description); SELECT CAST(SCOPE_IDENTITY() AS int);";
            cmd.CommandText = statement;

            // Create and add parameters
            SqlParameter gtinCodeParam = cmd.CreateParameter();
            gtinCodeParam.ParameterName = "@gtinCode";
            gtinCodeParam.DbType = DbType.Int64;
            if (this.GtinCode == 0L) {
                gtinCodeParam.Value = DBNull.Value;
            } else {
                gtinCodeParam.Value = this.GtinCode;
            }
            cmd.Parameters.Add(gtinCodeParam);

            SqlParameter qtyInStockParam = cmd.CreateParameter();
            qtyInStockParam.ParameterName = "@qtyInStock";
            qtyInStockParam.DbType = DbType.Int32;
            qtyInStockParam.Value = this.QtyInStock;
            cmd.Parameters.Add(qtyInStockParam);

            SqlParameter nameParam = cmd.CreateParameter();
            nameParam.ParameterName = "@name";
            nameParam.DbType = DbType.String;
            nameParam.Value = this.Name;
            cmd.Parameters.Add(nameParam);

            SqlParameter descriptionParam = cmd.CreateParameter();
            descriptionParam.ParameterName = "@description";
            descriptionParam.DbType = DbType.String;
            if (String.IsNullOrEmpty(this.Description)) {
                descriptionParam.Value = DBNull.Value;
            } else {
                descriptionParam.Value = this.Description;
            }
            cmd.Parameters.Add(descriptionParam);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            this.Id = (Int32)cmd.ExecuteScalar();

            return this;

        }



        public Product Update() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteUpdateCommand(connection.CreateCommand());
            }
        }

        public Product Update(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteUpdateCommand(cmd);
        }

        private Product ExecuteUpdateCommand(SqlCommand cmd) {
            if (this.Id == 0) {
                // Id has not been set, cannot update a product with no specific Id to track the correct db row.
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteUpdateCommand() : Id value is 0.");
            }

            // Create the Update statement.
            string statement = $"UPDATE {DATABASE_TABLE_NAME} SET " +
                "gtinCode = @gtinCode, " +
                "qtyInStock = @qtyInStock, " +
                "name = @name, " +
                "description = @description " +
                "WHERE Id = @id;";
            cmd.CommandText = statement;

            // Create and add parameters
            SqlParameter whereIdParam = cmd.CreateParameter();
            whereIdParam.ParameterName = "@id";
            whereIdParam.DbType = DbType.Int32;
            whereIdParam.Value = this.Id;
            cmd.Parameters.Add(whereIdParam);

            SqlParameter gtinCodeParam = cmd.CreateParameter();
            gtinCodeParam.ParameterName = "@gtinCode";
            gtinCodeParam.DbType = DbType.Int64;
            if (this.GtinCode == 0L) {
                gtinCodeParam.Value = DBNull.Value;
            } else {
                gtinCodeParam.Value = this.GtinCode;
            }
            cmd.Parameters.Add(gtinCodeParam);

            SqlParameter qtyInStockParam = cmd.CreateParameter();
            qtyInStockParam.ParameterName = "@qtyInStock";
            qtyInStockParam.DbType = DbType.Int32;
            qtyInStockParam.Value = this.QtyInStock;
            cmd.Parameters.Add(qtyInStockParam);

            SqlParameter nameParam = cmd.CreateParameter();
            nameParam.ParameterName = "@name";
            nameParam.DbType = DbType.String;
            nameParam.Value = this.Name;
            cmd.Parameters.Add(nameParam);

            SqlParameter descriptionParam = cmd.CreateParameter();
            descriptionParam.ParameterName = "@description";
            descriptionParam.DbType = DbType.String;
            if (String.IsNullOrEmpty(this.Description)) {
                descriptionParam.Value = DBNull.Value;
            } else {
                descriptionParam.Value = this.Description;
            }
            cmd.Parameters.Add(descriptionParam);


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
