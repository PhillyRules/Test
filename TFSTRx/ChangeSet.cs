using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFSTRx
{
    public class ChangeSet
    {
        public int ID { get; set; }
        public Uri CSUri{ get; set; }
        public string CheckedInBy { get; set; }
        public string Comments { get; set; }
    }
}