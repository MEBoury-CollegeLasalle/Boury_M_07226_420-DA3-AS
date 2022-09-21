using Boury_M_07226_420_DA3_AS.Controllers;
using Boury_M_07226_420_DA3_AS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Services {
    internal class TestService {

        CustomerController customerController = new CustomerController();
        ProductController productController = new ProductController();
        ShoppingCartController shoppingCartController = new ShoppingCartController();

        public TestService() { 
        }


        public void ExecuteFullTest() {
            this.TestProduct();
            this.TestShoppingCart();
            this.TestCustomer();
        }

        public void TestCustomer() {
            Guid guid = Guid.NewGuid();
            Customer customer = this.customerController.CreateCustomer($"{guid}@test.com", "test", "test");
            customer = this.customerController.UpdateCustomer(customer.Id, "testUpdate", "testUpdate", customer.Email);
            this.customerController.DeleteCustomer(customer);
            Console.WriteLine("+++ CUSTOMER TEST COMPLETED +++");
        }

        public void TestProduct() {
            Product product = this.productController.CreateProduct("TestProduct", 2, 1000000001L, "TestDescription");
            product = this.productController.UpdateProduct(product.Id, "TestProductUpdated", 4, 2000000002L, "TestDescriptionUpdated");
            this.productController.DeleteProduct(product);
            Console.WriteLine("+++ PRODUCT TEST COMPLETED +++");
        }

        public void TestShoppingCart() {
            Guid guid = Guid.NewGuid();
            Customer customer = this.customerController.CreateCustomer($"{guid}@test.com", "ShoppingCartCustomer", "ShoppingCartCustomer");
            ShoppingCart shoppingCart = this.shoppingCartController.CreateShoppingCart(customer.Id, "SCBillingAddress", "SCShippingAddress");
            shoppingCart = this.shoppingCartController.UpdateShoppingCart(shoppingCart.Id, customer, "SCBillingAddressUpdated", "SCShippingAddressUpdated");
            this.shoppingCartController.DeleteShoppingCart(shoppingCart);
            Console.WriteLine("+++ SHOPPING CART TEST COMPLETED +++");
        }
    }
}
