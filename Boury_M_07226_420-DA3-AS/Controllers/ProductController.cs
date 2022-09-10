using Boury_M_07226_420_DA3_AS.Models;
using Boury_M_07226_420_DA3_AS.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Controllers {
    internal class ProductController : IController {

        IModelView<Product> CreationView;
        readonly IModelView<Product> DisplayView;

        public ProductController() {
            this.DisplayView = new ProductConsoleDisplayView();
        }

        public void CreateProduct(string name, int qtyInStock, long gtinCode, string description) {
            Product newProduct = new Product(name, qtyInStock, gtinCode, description);
            newProduct.Insert();
            this.DisplayProduct(newProduct);
        }

        public void DisplayProducts(List<Product> productList) {
            foreach (Product product in productList) {
                this.DisplayProduct(product);
            }
        }

        public void DisplayProduct(int productId) {
            Product product = new Product(productId);
            product.GetById();
            this.DisplayView.Render(product);
        }

        public void DisplayProduct(Product product) {
            this.DisplayView.Render(product);
        }

        public void UpdateProduct(int productId, string name, int qtyInStock, long gtinCode, string description) {
            Product product = Product.GetById(productId);
            product.Name = name;
            product.QtyInStock = qtyInStock;
            product.GtinCode = gtinCode;
            product.Description = description;
            product.Update();
        }

    }
}
