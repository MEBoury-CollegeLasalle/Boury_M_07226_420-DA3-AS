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
    internal class CartProduct {

        private static readonly string DATABASE_TABLE_NAME = "dbo.CartProduct";

        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int ProductQuantity { get; set; }


        public CartProduct(int cartId, int productId) : this(cartId, productId, 1) { 
        }

        public CartProduct(int cartId, int productId, int productQuantity) {
            this.CartId = cartId;
            this.ProductId = productId;
            this.ProductQuantity = productQuantity;
        }


        #region Methods


        public static CartProduct GetByIds(int cartId, int productId) {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return ExecuteGetByIdsCommand(cartId, productId, connection.CreateCommand());
            }
        }

        public static CartProduct GetByIds(int cartId, int productId, SqlTransaction transaction, bool withExclusiveLock = false) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return ExecuteGetByIdsCommand(cartId, productId, cmd, withExclusiveLock);
        }

        private static CartProduct ExecuteGetByIdsCommand(int cartId, int productId, SqlCommand cmd, bool withExclusiveLock = false) {
            string statement = $"SELECT * FROM {DATABASE_TABLE_NAME} " +
                (withExclusiveLock ? "WITH  (XLOCK, ROWLOCK) " : "") +
                $"WHERE cartId = @cartId AND productId = @productId;";
            cmd.CommandText = statement;

            SqlParameter cartIdParam = cmd.CreateParameter();
            cartIdParam.ParameterName = "@catId";
            cartIdParam.DbType = DbType.Int32;
            cartIdParam.Value = cartId;
            cmd.Parameters.Add(cartIdParam);

            SqlParameter productIdParam = cmd.CreateParameter();
            productIdParam.ParameterName = "@productId";
            productIdParam.DbType = DbType.Int32;
            productIdParam.Value = productId;
            cmd.Parameters.Add(productIdParam);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows) {
                reader.Read();

                int productQuantity = reader.GetInt32(2);
                return new CartProduct(cartId, productId, productQuantity);

            } else {
                throw new Exception($"Failed to retrieve {typeof(CartProduct)}: no database entry found for CartId# {cartId} and ProductId# {productId}.");
            }

        }



        public static List<CartProduct> GetAllByCartId(int cartId) {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return ExecuteGetAllByCartIdCommand(cartId, connection.CreateCommand());
            }
        }

        public static List<CartProduct> GetAllByCartId(int cartId, SqlTransaction transaction, bool withExclusiveLock = false) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return ExecuteGetAllByCartIdCommand(cartId, cmd, withExclusiveLock);
        }

        private static List<CartProduct> ExecuteGetAllByCartIdCommand(int cartId, SqlCommand cmd, bool withExclusiveLock = false) {
            string statement = $"SELECT * FROM {DATABASE_TABLE_NAME} " +
                (withExclusiveLock ? "WITH  (XLOCK, ROWLOCK) " : "") +
                $"WHERE cartId = @cartId;";
            cmd.CommandText = statement;

            SqlParameter cartIdParam = cmd.CreateParameter();
            cartIdParam.ParameterName = "@catId";
            cartIdParam.DbType = DbType.Int32;
            cartIdParam.Value = cartId;
            cmd.Parameters.Add(cartIdParam);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            SqlDataReader reader = cmd.ExecuteReader();
            List<CartProduct> cartProducts = new List<CartProduct>();
            while (reader.Read()) {
                int productId = reader.GetInt32(1);
                int productQuantity = reader.GetInt32(2);
                cartProducts.Add(new CartProduct(cartId, productId, productQuantity));

            }
            return cartProducts;

        }



        public static List<CartProduct> GetAllByProductId(int productId) {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return ExecuteGetAllByProductIdCommand(productId, connection.CreateCommand());
            }
        }

        public static List<CartProduct> GetAllByProductId(int productId, SqlTransaction transaction, bool withExclusiveLock = false) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return ExecuteGetAllByProductIdCommand(productId, cmd, withExclusiveLock);
        }

        private static List<CartProduct> ExecuteGetAllByProductIdCommand(int productId, SqlCommand cmd, bool withExclusiveLock = false) {
            string statement = $"SELECT * FROM {DATABASE_TABLE_NAME} " +
                (withExclusiveLock ? "WITH  (XLOCK, ROWLOCK) " : "") +
                $"WHERE productId = @productId;";
            cmd.CommandText = statement;

            SqlParameter productIdParam = cmd.CreateParameter();
            productIdParam.ParameterName = "@productId";
            productIdParam.DbType = DbType.Int32;
            productIdParam.Value = productId;
            cmd.Parameters.Add(productIdParam);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            SqlDataReader reader = cmd.ExecuteReader();
            List<CartProduct> cartProducts = new List<CartProduct>();
            while (reader.Read()) {
                int cartId = reader.GetInt32(0);
                int productQuantity = reader.GetInt32(2);
                cartProducts.Add(new CartProduct(cartId, productId, productQuantity));

            }
            return cartProducts;
        }



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
            if (this.CartId == 0) {
                // CartId has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.Delete() : CartId value is 0.");
            } else if (this.ProductId == 0) {
                // ProductId has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.Delete() : ProductId value is 0.");
            }

            string statement = $"DELETE FROM {DATABASE_TABLE_NAME} " +
                $"WHERE cartId = @cartId AND productId = @productId;";
            cmd.CommandText = statement;

            SqlParameter cartIdParam = cmd.CreateParameter();
            cartIdParam.ParameterName = "@catId";
            cartIdParam.DbType = DbType.Int32;
            cartIdParam.Value = this.CartId;
            cmd.Parameters.Add(cartIdParam);

            SqlParameter productIdParam = cmd.CreateParameter();
            productIdParam.ParameterName = "@productId";
            productIdParam.DbType = DbType.Int32;
            productIdParam.Value = this.ProductId;
            cmd.Parameters.Add(productIdParam);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            int affectedRows = cmd.ExecuteNonQuery();

            if (!(affectedRows > 0)) {
                // No affected rows: no deletion occured -> row with matching Id not found
                throw new Exception($"Failed to delete {this.GetType().FullName}: no database entry found for CartId# {this.CartId} and ProductId# {this.ProductId}.");
            }
        }



        public CartProduct Insert() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteInsertCommand(connection.CreateCommand());
            }
        }

        public CartProduct Insert(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteInsertCommand(cmd);
        }

        private CartProduct ExecuteInsertCommand(SqlCommand cmd) {
            if (this.CartId == 0) {
                // CartId has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteInsertCommand() : CartId value is 0.");
            } else if (this.ProductId == 0) {
                // ProductId has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteInsertCommand() : ProductId value is 0.");
            }

            string insertCommandText = $"INSERT INTO {DATABASE_TABLE_NAME} (cartId, " +
                "productId, productQuantity) " +
                $"VALUES (@cartId, @productId, @productQuantity);";
            cmd.CommandText = insertCommandText;

            // Create and add parameters
            SqlParameter cartIdParam = cmd.CreateParameter();
            cartIdParam.ParameterName = "@cartId";
            cartIdParam.DbType = DbType.Int32;
            cartIdParam.Value = this.CartId;
            cmd.Parameters.Add(cartIdParam);

            SqlParameter productIdParam = cmd.CreateParameter();
            productIdParam.ParameterName = "@productId";
            productIdParam.DbType = DbType.Int32;
            productIdParam.Value = this.ProductId;
            cmd.Parameters.Add(productIdParam);

            SqlParameter productQuantityParam = cmd.CreateParameter();
            productQuantityParam.ParameterName = "@productQuantity";
            productQuantityParam.DbType = DbType.Int32;
            productQuantityParam.Value = this.ProductQuantity;
            cmd.Parameters.Add(productQuantityParam);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            cmd.ExecuteNonQuery();

            return this;

        }



        public CartProduct Update() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteUpdateCommand(connection.CreateCommand());
            }
        }

        public CartProduct Update(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteUpdateCommand(cmd);
        }

        private CartProduct ExecuteUpdateCommand(SqlCommand cmd) {
            if (this.CartId == 0) {
                // CartId has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteUpdateCommand() : CartId value is 0.");
            } else if (this.ProductId == 0) {
                // ProductId has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteUpdateCommand() : ProductId value is 0.");
            }

            string statement = $"UPDATE {DATABASE_TABLE_NAME} " +
                "SET productQuantity = @productQuantity" +
                "WHERE cartId = @cartId AND productId = @productId;";
            cmd.CommandText = statement;

            // Create and add parameters
            SqlParameter cartIdParam = cmd.CreateParameter();
            cartIdParam.ParameterName = "@cartId";
            cartIdParam.DbType = DbType.Int32;
            cartIdParam.Value = this.CartId;
            cmd.Parameters.Add(cartIdParam);

            SqlParameter productIdParam = cmd.CreateParameter();
            productIdParam.ParameterName = "@productId";
            productIdParam.DbType = DbType.Int32;
            productIdParam.Value = this.ProductId;
            cmd.Parameters.Add(productIdParam);

            SqlParameter productQuantityParam = cmd.CreateParameter();
            productQuantityParam.ParameterName = "@productQuantity";
            productQuantityParam.DbType = DbType.Int32;
            productQuantityParam.Value = this.ProductQuantity;
            cmd.Parameters.Add(productQuantityParam);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            int affectedRows = cmd.ExecuteNonQuery();

            if (affectedRows < 1) {
                // No affected rows: no update occured -> row with matching Ids not found
                throw new Exception($"Failed to update {this.GetType().FullName}: no database entry found for CartId# {this.CartId} and ProductId# {this.ProductId}.");
            }

            return this;
        }


        #endregion


    }
}
