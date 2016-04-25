using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TFSTRx
{
    public class WorkItem
    {
        public string AssignedTo { get; set; }
        public int ParentID{ get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string WIType { get; set; }
        public int ID { get; set; }
        public Uri WorkItemUri { get; set; }
    }
}