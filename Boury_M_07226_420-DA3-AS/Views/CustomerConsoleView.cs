using Boury_M_07226_420_DA3_AS.Models;
using Boury_M_07226_420_DA3_AS.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Views {
    internal class CustomerConsoleView : IModelView<Customer> {

        public CustomerConsoleView() { 
        }

        public void Render(Customer modelInstance) {
            Console.WriteLine($"Id: {modelInstance.Id} " +
                $"\nEmail: {modelInstance.Email} " +
                $"\nFirst Name: {modelInstance.FirstName} " +
                $"\nLast Name: {modelInstance.LastName} " +
                $"\nCreated at: {modelInstance.CreatedAt.ToString(DbUtils.DATETIME_DEFAULT_FORMAT)} " +
                $"\nDeleted at: {modelInstance.DeletedAt.ToString(DbUtils.DATETIME_DEFAULT_FORMAT)}."
            );
        }
    }
}
