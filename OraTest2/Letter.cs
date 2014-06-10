using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OraTest2
{
    class Letter
    {
        private string _chr;

        public string Chr
        {
            get { return this._chr; }
            set { this._chr = value; }
        }

        public Letter(string ltr) { this._chr = ltr; }
    }
}
