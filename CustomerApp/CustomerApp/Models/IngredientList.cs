﻿using Realms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerApp.Models
{
    class IngredientList : RealmObject
    {
        public IList<Ingredient> doc { get; }
    }
}
