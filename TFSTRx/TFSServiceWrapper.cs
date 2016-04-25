using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;


namespace TFSTRx
{
    public class TfsServiceWrapper
    {
        public TfsTeamProjectCollection TeamProjectCollection { get;set; }
        public string TeamProject { get; set; }
        public Uri TfsUri { get; set; }
        public IBuildDefinition[] BuildDefs { get; set; }

        public IBuildServer BuildServer { get; set; }


        public TfsServiceWrapper()
        {
            TfsUri = new Uri(ConfigurationManager.AppSettings["tfsUri"]);
            TeamProject = ConfigurationManager.AppSettings["teamProject"];
            TeamProjectCollection=ConnectToTeamProjectCollection(ConfigurationManager.AppSettings["ProjectCollectionName"]);
        }

        public TfsServiceWrapper(Uri tfsUri, string teamProject, string TeamProjectCollectionName)
        {
            TfsUri = tfsUri;
            TeamProject = teamProject;
            TfsTeamProjectCollection teamprojectCollection = ConnectToTeamProjectCollection(TeamProjectCollectionName);
            TeamProjectCollection = teamprojectCollection;
            //IBuildDefinition[] BuildDef = GetAllBuildDefinitionsFromTheTeamProject(new Microsoft.TeamFoundation.Client.TfsTeamProjectCollection(TfsUri),TeamProject);
            VersionControlServer vc=(VersionControlServer)TeamProjectCollection.GetService<VersionControlServer>();
            TeamProject project = vc.GetTeamProject(teamProject);
            IBuildServer bs = TeamProjectCollection.GetService<IBuildServer>();
            IBuildDefinition[] bd = bs.QueryBuildDefinitions(teamProject);
            BuildDefs = bd;
            BuildServer = bs;
            
        }
        //public IBuildDetail LatestBuildDetail
        //{
        //    get
        //    {
        //        var spec = BuildServer.CreateBuildDetailSpec(TeamProject, BuildName);
        //        spec.MaxBuildsPerDefinition = 1;
        //        spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;
        //        return BuildServer.QueryBuilds(spec).Builds.FirstOrDefault();
        //    }
        //}

        private TfsTeamProjectCollection ConnectToTeamProjectCollection(string TeamProjectCollectionName)
        {
            NetworkCredential cred = new NetworkCredential("tfsservice", "0lympu$123", "MITLab");
            Uri TeamProjectCollectionUri = new Uri(@"http://MIT-TFS-PROD:8080/tfs");
            TfsConfigurationServer configServer = new TfsConfigurationServer(TeamProjectCollectionUri);
           configServer.EnsureAuthenticated();
            CatalogNode rootNode = configServer.CatalogNode;
            ReadOnlyCollection<CatalogNode> tpcNodes = rootNode.QueryChildren(
            new Guid[] { CatalogResourceTypes.ProjectCollection }, false, CatalogQueryOptions.None);
            foreach (CatalogNode tpcNode in tpcNodes)
            {
                    Guid tpcId = new Guid(tpcNode.Resource.Properties["InstanceId"]);
                    TfsTeamProjectCollection tpc = configServer.GetTeamProjectCollection(tpcId);
                    if(tpcNode.Resource.DisplayName.ToUpper()==TeamProjectCollectionName.ToUpper())
                    {
                        return tpc;
                    }
            }
            return null;
            
        }

              
        public ITestManagementService TestManagementService
        {
            get
            {
                return (ITestManagementService)TeamProjectCollection.GetService(typeof(ITestManagementService));
            }
        }

        //public XDocument LatestTestResultFile
        //{
        //    get
        //    {
        //        var latestRun = TestManagementService.GetTeamProject(TeamProject).TestRuns.ByBuild(LatestBuildDetail.Uri).First(run => run.QueryResults().Any());
        //        var resolver = new XmlUrlResolver { Credentials = CredentialCache.DefaultCredentials };
        //        var settings = new XmlReaderSettings { XmlResolver = resolver };
        //        var reader = XmlReader.Create(latestRun.Attachments[0].Uri.ToString(), settings);
        //        return XDocument.Load(reader);
        //    }
        //}

    }
}