using Boury_M_07226_420_DA3_AS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Views {
    internal class CustomerConsoleView : IModelView<Customer> {



        public void Render(Customer modelInstance) {
            Console.WriteLine($"Id: {modelInstance.Id} " +
                $"\nEmail: {modelInstance.Email} " +
                $"\nFirst Name: {modelInstance.FirstName} " +
                $"\nLast Name: {modelInstance.LastName}."
            );
        }
    }
}
