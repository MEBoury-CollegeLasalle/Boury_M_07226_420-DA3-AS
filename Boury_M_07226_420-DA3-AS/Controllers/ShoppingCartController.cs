/*
 * (c) Copyright 2022 Marc-Eric Boury
 */

using Boury_M_07226_420_DA3_AS.Models;
using Boury_M_07226_420_DA3_AS.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Boury_M_07226_420_DA3_AS.Controllers {
    internal class ShoppingCartController : IController {

        public void CreateShoppingCart(int customerId, string billingAddress, string shippingAddress) {
            Customer customer = new Customer(customerId);
            customer.GetById();
            CreateShoppingCart(customer, billingAddress, shippingAddress);
        }

        public void CreateShoppingCart(Customer customer, string billingAddress, string shippingAddress) {
            ShoppingCart newShoppingCart = new ShoppingCart(customer, billingAddress, shippingAddress);
            newShoppingCart.Insert();
        }

        public void DisplayShoppingCart(int shoppingCartId) {
            ShoppingCart shoppingCart = new ShoppingCart(shoppingCartId);
            shoppingCart.GetById();
            this.DisplayShoppingCart(shoppingCart);
        }

        public void DisplayShoppingCart(ShoppingCart shoppingCart) {
            // do something later in the course when we have views, or, if you want,
            // dump the data to the console.
        }

        public void UpdateShoppingCart(int shoppingCartId, int customerId, string billingAddress, string shippingAddress) {
            Customer customer = new Customer(customerId);
            customer.GetById();
            UpdateShoppingCart(shoppingCartId, customer, billingAddress, shippingAddress);
        }

        public void UpdateShoppingCart(int shoppingCartId, Customer customer, string billingAddress, string shippingAddress) {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                try {
                    ShoppingCart shoppingCart = ShoppingCart.GetById(shoppingCartId, transaction, true);
                    shoppingCart.Customer = customer;
                    shoppingCart.BillingAddress = billingAddress;
                    shoppingCart.ShippingAddress = shippingAddress;
                    shoppingCart.Update();
                    transaction.Commit();

                } catch (Exception e) {
                    transaction.Rollback();
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                }
            }
        }

        public void DeleteShoppingCart(int shoppingCartId) {
            ShoppingCart shoppingCart = new ShoppingCart(shoppingCartId);
            this.DeleteShoppingCart(shoppingCart);
        }

        public void DeleteShoppingCart(ShoppingCart shoppingCart) {
            shoppingCart.Delete();
        }
    }
}
