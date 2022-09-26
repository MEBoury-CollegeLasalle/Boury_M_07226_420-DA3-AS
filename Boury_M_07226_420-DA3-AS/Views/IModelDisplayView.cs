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
    internal interface IModelDisplayView<TModel> : IView where TModel : IModel<TModel> {

        void Render(TModel modelInstance);
    }
}
