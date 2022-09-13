using Boury_M_07226_420_DA3_AS.Models;
using Boury_M_07226_420_DA3_AS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Views {
    internal class ShoppingCartConsoleView : IModelView<ShoppingCart> {

        public ShoppingCartConsoleView() { 
        }

        public void Render(ShoppingCart modelInstance) {
            Console.WriteLine($"Id: {modelInstance.Id} " +
                $"\nCustomer Id: {modelInstance.Customer.Id} " +
                $"\nBilling address: {modelInstance.BillingAddress} " +
                $"\nShipping address: {modelInstance.ShippingAddress} " +
                $"\nDate created: {modelInstance.DateCreated.ToString(DbUtils.DATETIME_DEFAULT_FORMAT)} " +
                $"\nDate updated: {modelInstance.DateUpdated.ToString(DbUtils.DATETIME_DEFAULT_FORMAT)} " +
                $"\nDate ordered: {modelInstance.DateOrdered.ToString(DbUtils.DATETIME_DEFAULT_FORMAT)} " +
                $"\nDate shipped: {modelInstance.DateShipped.ToString(DbUtils.DATETIME_DEFAULT_FORMAT)}."
                );
        }
    }
}
