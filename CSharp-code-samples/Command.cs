using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;

namespace InWebo.ApiDemo
{
    public class Command
    {
        public delegate TResult Func<out TResult>();
        public string Name { get; set; }
        public Func<int> Action { get; set; }
        public string Description { get; set; }
        public string Key { get { return Name.ToLower(); } }
    }

    public class Commands : KeyedCollection<string, Command>
    {
        public Commands() : base() {}

        protected override string GetKeyForItem(Command item)
        {
            return item.Key;
        }
    }

}
