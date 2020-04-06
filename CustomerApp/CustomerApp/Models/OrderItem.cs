﻿using System;
using System.Collections.Generic;
using System.Text;

using Realms;

namespace CustomerApp.Models
{
    // This class is very similar to MenuFoodItem, but is used to store items which are in the user's order
    public class OrderItem : RealmObject
    {
        public IList<string> ingredients { get; }

        //[PrimaryKey]
        //public string newID { get; set; }

        public string _id { get; set; } // Original item's ID



        public string name { get; set; }

        //public string picture { get; set; }

        //public string nutrition { get; set; } // Just manually input formatted text to be displayed in an alert, lmao

        public double price { get; set; }
        public string StringPrice => price.ToString("C");

        //public string description { get; set; }

        public string special_instruct { get; set; }

        public bool paid { get; set; }

        //public string category { get; set; }

        // Default constructor
        public OrderItem() { }

        // Copy constructor
        public OrderItem(MenuFoodItem m)
        {
            _id = m._id;

            foreach(Ingredient i in m.ingredients)
                ingredients.Add(i._id);

            name = m.name;

            price = m.price;

            special_instruct = m.special_instruct;

            paid = m.paid;
        }
        public OrderItem(OrderItem o)
        {
            _id = o._id;

            ingredients = o.ingredients;

            name = o.name;

            price = o.price;

            special_instruct = o.special_instruct;

            paid = o.paid;
        }
    }
}
