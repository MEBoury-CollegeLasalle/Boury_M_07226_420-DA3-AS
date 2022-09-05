using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Models {
    public interface IModel<TModel> where TModel : IModel<TModel>, new() {

        /// <summary>
        /// Inserts the values of a <see cref="IModel{TModel}"/> instance in the database as a
        /// new entry.
        /// </summary>
        /// <returns>The instance with the updated database identity property set.</returns>
        TModel Insert();

        /// <summary>
        /// Populates an instance of <see cref="IModel{TModel}"/> with property values retrieved from
        /// a database entry based on its identity identifier.
        /// </summary>
        /// <returns>The instance with updated property values from the database.</returns>
        TModel GetById();

        /// <summary>
        /// Updates the database entry with values from the instance's properties.
        /// </summary>
        /// <returns>The instance</returns>
        TModel Update();

        /// <summary>
        /// Deletes the database entry.
        /// </summary>
        void Delete();

    }
}
