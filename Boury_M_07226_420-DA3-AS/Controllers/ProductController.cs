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

        private ProductConsoleView consoleView;

        public ProductController() {
            this.consoleView = new ProductConsoleView();
        }



        public Product CreateProduct(string name, int qtyInStock) {
            return CreateProduct(name, qtyInStock, 0L);
        }

        public Product CreateProduct(string name, int qtyInStock, long gtinCode) {
            return CreateProduct(name, qtyInStock, gtinCode, "");
        }

        public Product CreateProduct(string name, int qtyInStock, long gtinCode, string description) {
            Product newProduct = new Product(name, qtyInStock, gtinCode, description);
            newProduct.Insert();

            Console.WriteLine("CREATED PRODUCT:");
            this.DisplayProduct(newProduct);

            return newProduct;
        }

        public void DisplayProduct(int productId) {
            Product product = new Product(productId);
            product.GetById();
            this.DisplayProduct(product);
        }

        public void DisplayProduct(Product product) {
            this.consoleView.Render(product);
        }

        public Product UpdateProduct(int productId, string name, int qtyInStock, long gtinCode, string description) {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                try {
                    Product product = Product.GetById(productId, transaction, true);

                    Console.WriteLine("PRODUCT TO UPDATE:");
                    this.DisplayProduct(product);

                    product.Name = name;
                    product.QtyInStock = qtyInStock;
                    product.GtinCode = gtinCode;
                    product.Description = description;
                    product.Update();
                    transaction.Commit();

                    Console.WriteLine("UPDATED PRODUCT:");
                    this.DisplayProduct(product);

                    return product;

                } catch (Exception e) {
                    transaction.Rollback();
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                    throw e;
                }
            }
        }

        public void DeleteProduct(int productId) {
            Product product = new Product(productId);
            this.DeleteProduct(product);
        }

        public void DeleteProduct(Product product) {
            product.Delete();

            Console.WriteLine("DELETED PRODUCT:");
            this.DisplayProduct(product);
        }

        //public void DisplayProducts(List<Product> productList) {
        //    foreach (Product product in productList) {
        //        this.DisplayProduct(product);
        //    }
        //}

    }
}
