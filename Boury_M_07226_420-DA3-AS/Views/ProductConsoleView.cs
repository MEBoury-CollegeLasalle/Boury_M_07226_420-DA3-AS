/*
 * (c) Copyright 2022 Marc-Eric Boury
 */

using Boury_M_07226_420_DA3_AS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Views {
    internal class ProductConsoleView : IModelView<Product> {

        public ProductConsoleView() { 
        }

        public void Render(Product modelInstance) {
            Console.WriteLine($"Id: {modelInstance.Id} " +
                $"\nName: {modelInstance.Name} " +
                $"\nQty In Stock: {modelInstance.QtyInStock} " +
                $"\nGTIN code: {modelInstance.GtinCode} " +
                $"\nDescription: {modelInstance.Description}");
        }
    }
}
