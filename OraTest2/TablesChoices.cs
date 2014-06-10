using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OraTest2
{
    public class TablesChoices
    {
        private string _title;
        private string _value;

        public string Title
        {
            get { return this._title; }
            set { this._title = value; }
        }

        public string Value
        {
            get { return this._value; }
            set { this._value = value; }
        }

        public TablesChoices(string str)
        {
            Title = str;
            Value = str;
        }

        public TablesChoices() { }
    }
}
