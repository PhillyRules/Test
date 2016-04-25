using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using System.Configuration;
using System.Collections.ObjectModel;
namespace TFSTRx
{
    public class SpecificBuildInfo
    {
        public string BuildNo { get; set; }
        public Uri BuildUri { get; set; }
        public string DropLocation { get; set; }
        public string LabelName { get; set; }
        public string Status { get; set; }
        public DateTime LastChanged{get;set;}
        public string Reason{get;set;}
        public string SourceChangeSet{get;set;}
        public DateTime StartTime{get;set;}
        public DateTime EndTime{get;set;}
        public string RequestedBy{get;set;}
        public string RequestedFor{get;set;}

        public List<IBuildInformationNode> InfoNodes { get; set; }
        public List<IBuildInformationNode> ChangeSetNodes { get; set; }
        public List<IBuildInformationNode> WorkItemNodes { get; set; }
        public List<IBuildInformationNode> ConfigurationNodes { get; set; }

        public List<Dictionary<string, string>> ChangeSetInfo {get;set;}
        public List<Dictionary<string, string>> WorkItemInfo { get; set; }
        public List<ChangeSet> ChangesetDetail { get; set; }

        public List<WorkItem> WorkItemDetail { get; set; }

        public SpecificBuildInfo()
        {

        }

        public SpecificBuildInfo(string buildNo,Uri buildUri,string dropLocation,string labelName,string status,DateTime lastChange,string reason,string sourceGet,
            DateTime startTime,DateTime endTime,string requestedBy,string requestedFor)
        {
            BuildNo = buildNo;
            BuildUri = buildUri;
            DropLocation = dropLocation;
            LabelName = labelName;
            Status = status;
            LastChanged = lastChange;
            Reason = reason;
            SourceChangeSet = SourceChangeSet;
            StartTime = startTime;
            EndTime = endTime;
            RequestedBy = requestedBy;
            RequestedFor = requestedFor;
        }

    }
}