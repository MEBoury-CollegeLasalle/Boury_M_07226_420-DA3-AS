/*
 * (c) Copyright 2022 Marc-Eric Boury
 */

using Boury_M_07226_420_DA3_AS.Models;
using Boury_M_07226_420_DA3_AS.Utils;
using Boury_M_07226_420_DA3_AS.Views;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Controllers {
    internal class ProductController : IController {

        public void CreateProduct(string name, int qtyInStock) {
            CreateProduct(name, qtyInStock, 0L);
        }

        public void CreateProduct(string name, int qtyInStock, long gtinCode) {
            CreateProduct(name, qtyInStock, gtinCode, "");
        }

        public void CreateProduct(string name, int qtyInStock, long gtinCode, string description) {
            Product newProduct = new Product(name, qtyInStock, gtinCode, description);
            newProduct.Insert();
        }

        public void DisplayProduct(int productId) {
            Product product = new Product(productId);
            product.GetById();
            this.DisplayProduct(product);
        }

        public void DisplayProduct(Product product) {
            // do something later in the course when we have views, or, if you want,
            // dump the data to the console.
        }

        public void UpdateProduct(int productId, string name, int qtyInStock, long gtinCode, string description) {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                try {
                    Product product = Product.GetById(productId, transaction, true);
                    product.Name = name;
                    product.QtyInStock = qtyInStock;
                    product.GtinCode = gtinCode;
                    product.Description = description;
                    product.Update();
                    transaction.Commit();

                } catch (Exception e) {
                    transaction.Rollback();
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                }
            }
        }

        public void DeleteProduct(int productId) {
            Product product = new Product(productId);
            this.DeleteProduct(product);
        }

        public void DeleteProduct(Product product) {
            product.Delete();
        }

        //public void DisplayProducts(List<Product> productList) {
        //    foreach (Product product in productList) {
        //        this.DisplayProduct(product);
        //    }
        //}

    }
}
