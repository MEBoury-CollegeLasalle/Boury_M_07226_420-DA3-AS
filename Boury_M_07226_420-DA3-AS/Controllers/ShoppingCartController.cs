using Boury_M_07226_420_DA3_AS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
            ShoppingCart shoppingCart = ShoppingCart.GetById(shoppingCartId);
            shoppingCart.Customer = customer;
            shoppingCart.BillingAddress = billingAddress;
            shoppingCart.ShippingAddress = shippingAddress;
            shoppingCart.Update();
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
