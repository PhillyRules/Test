using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Runtime;
using System.Security;
using System.Web.UI.WebControls;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using System.Configuration;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Net;

namespace TFSTRx
{
    public partial class GetBuildInfo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ScriptManager.RegisterClientScriptBlock(this.UpdatePanel1, typeof(UpdatePanel), "clientscript6", "", true);
            if (!(IsPostBack))
            {
                Uri tfsUri = new Uri("http://mit-tfs-prod:8080/tfs");
                TfsServiceWrapper swrapper = new TfsServiceWrapper(tfsUri, "Unifia", "Unifia Collection");
                Session["TFSServiceWrapper"] = swrapper;
                List<string> BuildDefNames = new List<string>();
                BuildDefNames.Add("--- SELECT BUILD DEF ---");
                for (int i = 0; i < swrapper.BuildDefs.Length; i++)
                {
                    BuildDefNames.Add(swrapper.BuildDefs[i].Name);
                }
                BuildDefNames.Sort();

                // Small code block to print out builddefs to a text file
                
                //using (StreamWriter sw = new StreamWriter(@"c:\users\glen\documents\unifiabuilddefs.txt"))
                //{
                //    for (int i = 0; i < BuildDefNames.Count; i++)
                //    {
                //        sw.WriteLine(BuildDefNames[i].ToString());
                //    }
                //}
                dd1.DataSource = BuildDefNames;
                dd1.DataBind();


            }
        }

        protected void dd1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string builddefname = dd1.SelectedItem.Text;
            PopulateBuildsGridView(builddefname);
        }

       

        public List<IBuildDetail> GetBuildDetail(string BuildDefName)
        {

            TfsServiceWrapper swrapper = (TfsServiceWrapper)Session["TFSServiceWrapper"];
            IBuildServer bs = swrapper.BuildServer;
            IBuildDefinition[] builddef = bs.QueryBuildDefinitions(swrapper.TeamProject);
            List<IBuildDefinition> builddefList = builddef.Cast<IBuildDefinition>().ToList();
            IBuildDefinition specificBuildDef = builddefList.Where(p => p.Name.ToUpper() == BuildDefName.ToUpper()).FirstOrDefault();
            List<IBuildDetail> buildDetail = specificBuildDef.QueryBuilds().OrderByDescending(p => p.LastChangedOn).ToList();
            Session["SpecificBuildDetail"] = buildDetail;
            return buildDetail;

        }
        public void PopulateBuildsGridView(string BuildDefName)
        {

            List<IBuildDetail> buildDetail = GetBuildDetail(BuildDefName);
            gvBuilds.DataSource = buildDetail;
            gvBuilds.DataBind();
            lblCount.Text = buildDetail.Count.ToString() + " entries.";
        }
        //public XDocument LatestTestResultFile(ITestManagementService tmc,string TProject, IBuildDetail LatestBuildDetail,TfsTeamProjectCollection tpc)
        //{
            
          

        //        var latestRun = tmc.GetTeamProject(TProject).TestRuns.ByBuild(LatestBuildDetail.Uri).First(run => run.QueryResults().Any());
        //        var resolver = new XmlUrlResolver { Credentials = CredentialCache.DefaultCredentials };
        //        var settings = new XmlReaderSettings { XmlResolver = resolver };
        //        var reader = XmlReader.Create(latestRun.Attachments[0].Uri.ToString(), settings);
        //        return XDocument.Load(reader);
             
        //}
        protected void gvBuilds_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if(e.CommandName=="MoreInfo")
            {
                TfsServiceWrapper swrapper = (TfsServiceWrapper)Session["TFSServiceWrapper"];
                int GridRow = Convert.ToInt32(e.CommandArgument);
                Label lblBuildToDetail = (Label)gvBuilds.Rows[GridRow].Cells[1].FindControl("lblBuild");
                string BuildToDetail = lblBuildToDetail.Text;
                //Label lblUri = (Label)gvBuilds.Rows[GridRow].FindControl("lblUri");
                //string BuildUri = lblUri.Text;
                List<IBuildDetail> buildDetails = (List<IBuildDetail>)Session["SpecificBuildDetail"];
                IBuildDetail specificbuild = buildDetails.Where(p => p.BuildNumber == BuildToDetail).FirstOrDefault();

                SpecificBuildInfo specinfo = new SpecificBuildInfo(specificbuild.BuildNumber, specificbuild.Uri, specificbuild.DropLocation, specificbuild.LabelName,
                    specificbuild.Status.ToString(), specificbuild.LastChangedOn, specificbuild.Reason.ToString(), specificbuild.SourceGetVersion, specificbuild.StartTime,
                    specificbuild.FinishTime, specificbuild.RequestedBy, specificbuild.RequestedFor);
                TfsTeamProjectCollection tpc = swrapper.BuildServer.TeamProjectCollection;
                var _tms = tpc.GetService<ITestManagementService>();
                var testRuns = _tms.GetTeamProject(swrapper.TeamProject).TestRuns.ByBuild(specificbuild.Uri);
               List<TestResult> trList=GetTestResult(tpc,swrapper.TeamProject,specificbuild.Uri);
               //XDocument xdocker = LatestTestResultFile(_tms,swrapper.TeamProject,specificbuild,tpc);
               
              //  string ChangeSetID = specificbuild.Information.GetNodesByType("AssociatedChangeset")[0].Fields["ChangesetId"] != null ? specificbuild.Information.GetNodesByType("AssociatedChangeset")[0].Fields["ChangesetId"] : string.Empty;
                //string AssociatedWorkItemID = specificbuild.Information.GetNodesByType("AssociatedWorkItem")[0].Fields["WorkItemId"]!=null?specificbuild.Information.GetNodesByType("AssociatedWorkItem")[0].Fields["WorkItemId"]:string.Empty;
                IBuildInformationNode[] nodes = specificbuild.Information.Nodes;
                List<IBuildInformationNode> BuildInfoList = nodes.Cast<IBuildInformationNode>().ToList();
                specinfo.InfoNodes = BuildInfoList;
                specinfo.ChangeSetNodes = BuildInfoList.Where(p => p.Type == "AssociatedChangeset").ToList();
                specinfo.WorkItemNodes = BuildInfoList.Where(p => p.Type == "AssociatedWorkItem").ToList();
                specinfo.ConfigurationNodes = BuildInfoList.Where(p => p.Type == "ConfigurationSummary").ToList();
                specinfo.ChangeSetInfo = BuildInfoList.Where(p => p.Type == "AssociatedChangeset").Select(p => p.Fields).ToList();
                specinfo.WorkItemInfo = BuildInfoList.Where(p => p.Type == "AssociatedWorkItem").Select(p => p.Fields).ToList();
                List<ChangeSet> cList = new List<ChangeSet>();
                List<WorkItem> wiList = new List<WorkItem>();
                for (int v = 0; v < specinfo.ChangeSetInfo.Count; v++)
                {
                    Dictionary<string, string> valuedict = specinfo.ChangeSetInfo[v];
                    ChangeSet singlechange = new ChangeSet();
                    foreach (KeyValuePair<string, string> kvp in valuedict)
                    {
                        if(kvp.Key=="ChangesetId")
                        {singlechange.ID=Convert.ToInt32(kvp.Value);}
                        if(kvp.Key=="ChangesetUri")
                        { singlechange.CSUri = new Uri(kvp.Value); }
                        if(kvp.Key=="CheckedInBy")
                        { singlechange.CheckedInBy = kvp.Value; }
                        if(kvp.Key=="Comment")
                        { singlechange.Comments = kvp.Value; }
                        

                    }
                    cList.Add(singlechange);
                }
                for (int v = 0; v < specinfo.WorkItemInfo.Count;v++ )
                {

                    Dictionary<string, string> valuedict = specinfo.WorkItemInfo[v];
                    WorkItem singleworkitem = new WorkItem();
                    foreach(KeyValuePair<string,string> kvp in valuedict)
                    {
                        if(kvp.Key=="AssignedTo")
                        {
                            singleworkitem.AssignedTo = kvp.Value;
                        }

                        if (kvp.Key == "ParentWorkItemId")
                        {
                            singleworkitem.ParentID = Convert.ToInt32(kvp.Value);
                        }

                        if (kvp.Key == "Status")
                        {
                            singleworkitem.Status = kvp.Value;
                        }

                        if (kvp.Key == "Title")
                        {
                            singleworkitem.Title = kvp.Value;
                        }

                        if (kvp.Key == "Type")
                        {
                            singleworkitem.WIType = kvp.Value;
                        }

                        if (kvp.Key == "WorkItemId")
                        {
                            singleworkitem.ID = Convert.ToInt32(kvp.Value);
                        }

                        if (kvp.Key == "WorkItemUri")
                        {
                            singleworkitem.WorkItemUri = new Uri(kvp.Value.ToString());
                        }
                    }
                    wiList.Add(singleworkitem);
                        
                }
                    specinfo.ChangesetDetail = cList;
                    specinfo.WorkItemDetail = wiList;
                //Make a GridView of Changeset Ids
                    List<string> ChangeIdList = new List<string>();
                    List<string> WorkItemIdList = new List<string>();
                for(int z=0;z<cList.Count;z++)
                {
                    ChangeIdList.Add(cList[z].ID.ToString());
                }
                    GridView gridchild = (GridView)gvBuilds.Rows[GridRow].Cells[2].FindControl("gvChild");
                if(!(gridchild==null))
                {
                    if (ChangeIdList.Count > 0)
                    {
                        ImageButton ib = (ImageButton)gvBuilds.Rows[GridRow].Cells[3].FindControl("imgShowCS");
                        ib.Visible = true;
                        Panel pn = (Panel)gvBuilds.Rows[GridRow].Cells[3].FindControl("pnlCS");
                        pn.Visible = true;
                        gridchild.Visible = true;
                        gridchild.DataSource = ChangeIdList;
                        gridchild.DataBind();
                        pn.Visible = false;
                    }
                }

                //Make a GridView of Work Item Ids

                //for (int z = 0; z < wiList.Count; z++)
                //{

                //    if(wiList[z].ParentID!=0)
                //    {
                //        WorkItemIdList.Add("<font color=\"Red\">" + wiList[z].ParentID.ToString()+ "</font>");
                //    }
                //    WorkItemIdList.Add(wiList[z].ID.ToString());
                //}
                GridView gridchild2 = (GridView)gvBuilds.Rows[GridRow].Cells[3].FindControl("gvWorkItems");
                if (!(gridchild2 == null))
                {
                    if (wiList.Count > 0)
                    {
                        ImageButton ib = (ImageButton)gvBuilds.Rows[GridRow].Cells[3].FindControl("imgShowWI");
                        ib.Visible = true;
                        Panel pn = (Panel)gvBuilds.Rows[GridRow].Cells[3].FindControl("pnlWI");
                        pn.Visible = true;
                        gridchild2.Visible = true;
                        gridchild2.DataSource = wiList;
                        gridchild2.DataBind();
                        pn.Visible = false;
                    }
                }

                //Make a GridView of Test Results
                GridView gridchild3 = (GridView)gvBuilds.Rows[GridRow].Cells[4].FindControl("gvTestResults");
                if (!(gridchild3 == null))
                {
                    if (!(trList == null))
                    {
                        if (trList.Count > 0)
                        {
                            ImageButton ib = (ImageButton)gvBuilds.Rows[GridRow].Cells[3].FindControl("imgShow");
                            ib.Visible = true;
                            Panel pn = (Panel)gvBuilds.Rows[GridRow].Cells[4].FindControl("pnlTest");
                            pn.Visible = true;
                            gridchild3.Visible = true;
                            gridchild3.DataSource = trList;
                            gridchild3.DataBind();
                            pn.Visible = false;
                        }
                    }
                }
            }
            else if (e.CommandName=="GenExcel")
            {
                GridViewRow gvr = gvBuilds.Rows[Convert.ToInt32(e.CommandArgument)];
                System.Diagnostics.Process[] AllProcesses = System.Diagnostics.Process.GetProcessesByName("excel");
                foreach (System.Diagnostics.Process ExcelProcess in AllProcesses)
                {
                    ExcelProcess.Kill();
                }
                string fileName = "OlympusReport" + DateTime.Now.ToString("MMddyyyyhhmm") + ".xls";
                DataTable dt = BringBackDataTable(gvBuilds);
            }
        }
        protected void DoRegister(object sender, EventArgs e)
        {
            ScriptManager.RegisterClientScriptBlock(this.UpdatePanel1, typeof(UpdatePanel), "clientscript6", "", true);
        }
        protected void gvBuilds_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                IBuildDetail buildetail = (IBuildDetail)e.Row.DataItem;
                Uri BuildDefinitionUri=buildetail.BuildDefinitionUri;
                string BuildDefString = BuildDefinitionUri.ToString();
                string TeamProject = buildetail.TeamProject;
                BuildDefString = BuildDefString.Replace(":", "%3A");
                BuildDefString = BuildDefString.Replace("/", "%2F");
                string BuildString = buildetail.Uri.ToString();
                BuildString = BuildString.Replace(":", "%3A");
                BuildString = BuildString.Replace("/", "%2F");
                Uri BuildUri=buildetail.Uri;

                //Build No.

                String Buildno = buildetail.BuildNumber;
                Label lb1 = (Label)e.Row.FindControl("lblBuild");
                if (!(lb1 == null))
                {
                    lb1.Text = Buildno;
                }
                //URI
                Uri test = buildetail.Uri;
                Label lb2 = (Label)e.Row.FindControl("lblUri");
                if(!(lb2==null))
                {
                    lb2.Text = test.AbsoluteUri;
                }

                //Hyperlink to TWA

                LinkButton hl = (LinkButton)e.Row.FindControl("LinkButton1");

                if (!(hl == null))
                {
                    hl.Text = buildetail.BuildNumber;
                    string NavString="http://mit-tfs-prod:8080/tfs/Unifia%20Collection/" + TeamProject + "/" + "_build#definitionUri=" + BuildDefString + "&_a=summary&buildUri=" + BuildString;
                    hl.OnClientClick="javascript:window.open('" + NavString + "',null,'resizable=no,toolbar=no,scrollbars=no,menubar=yes,status=yes,width=1200,height=800')";
                   // hl.NavigateUrl = NavString;
                    
                }




                //Drop Location
                string DropLocation = buildetail.DropLocation;
                Label lb3 = (Label)e.Row.FindControl("lblDrop");
                if (!(lb3 == null))
                {
                    lb3.Text = DropLocation;
                }

                //Build Label

                string bLabel=buildetail.LabelName;
                Label lb4 = (Label)e.Row.FindControl("lblLabel");
                if (!(lb4 == null))
                {
                    lb4.Text = bLabel;
                }

                //Status

                string status = buildetail.Status.ToString();
                Label lb5 = (Label)e.Row.FindControl("lblStatus");
                if (!(lb5 == null))
                {
                    lb5.Text = status;
                }

                //Last Changed

                String LastChanged = buildetail.LastChangedOn.ToString();
                Label lb6 = (Label)e.Row.FindControl("lblLastChange");
                if (!(lb6 == null))
                {
                    lb6.Text = LastChanged;
                }

                //Reason

                string Reason = buildetail.Reason.ToString();
                Label lb7 = (Label)e.Row.FindControl("lblReason");
                if (!(lb7 == null))
                {
                    lb7.Text = Reason;
                }

                //SourceGet

                string SourceGet = buildetail.SourceGetVersion;
                Label lb8 = (Label)e.Row.FindControl("lblSourceGet");
                if (!(lb8 == null))
                {
                    lb8.Text = SourceGet;
                }

                //StartTime

                string startTime = buildetail.StartTime.ToString();
                Label lb9 = (Label)e.Row.FindControl("lblStartTime");
                if (!(lb9 == null))
                {
                    lb9.Text = startTime;
                }
               
                //End Time

                string endTime = buildetail.FinishTime.ToString();
                Label lb10 = (Label)e.Row.FindControl("lblEndTime");
                if (!(lb10 == null))
                {
                    lb10.Text = endTime;
                }

                
                //Requested By

                string requestedBy = buildetail.RequestedBy;
                Label lb11 = (Label)e.Row.FindControl("lblRequestedBy");
                if (!(lb11 == null))
                {
                    lb11.Text = requestedBy;
                }

                //Requested For

                string requestedFor = buildetail.RequestedFor;
                Label lb12 = (Label)e.Row.FindControl("lblRequestedFor");
                if (!(lb12 == null))
                {
                    lb12.Text = requestedFor;
                }

            }
        }
         private List<TestResult> GetTestResult(TfsTeamProjectCollection _tfs,string tpp,Uri buildUri)
        {
            ITestManagementService _tms = _tfs.GetService<ITestManagementService>();
            var testRuns = _tms.GetTeamProject(tpp).TestRuns.ByBuild(buildUri);

            foreach (var testRun in testRuns)
            {
                List<TestResult> TestResultList = new List<TestResult>();
                ListBox lstTestRunDetails=new ListBox();
                lstTestRunDetails.Items.Add(string.Format("{0}", testRun.Title));
                lstTestRunDetails.Items.Add(string.Format("TestRunId: {0} | TestPlanId: {1}", testRun.Id, testRun.TestPlanId));
                lstTestRunDetails.Items.Add(string.Format("TestSettingsId: {0} | TestEnvironmentId {1} ", testRun.TestSettingsId, testRun.TestEnvironmentId));

                var totalTests = testRun.Statistics.TotalTests;

                foreach (var et in testRun.QueryResultsByOutcome(TestOutcome.Error))
                {
                    lstTestRunDetails.Items.Add(string.Format("{0}: {1} - {2}", et.Outcome, et.TestCaseTitle, et.ErrorMessage));
                    TestResult tr = new TestResult();
                    tr.Outcome = et.Outcome.ToString();
                    tr.Title = et.TestCaseTitle;
                    tr.Error = et.ErrorMessage;
                    TestResultList.Add(tr);

                }

                foreach (var tp in testRun.QueryResultsByOutcome(TestOutcome.Passed))
                {
                    lstTestRunDetails.Items.Add(string.Format("{0}: {1} ", tp.Outcome, tp.TestCaseTitle));
                    TestResult tr = new TestResult();
                    tr.Outcome = tp.Outcome.ToString();
                    tr.Title = tp.TestCaseTitle;
                    tr.Error = tp.ErrorMessage;
                    TestResultList.Add(tr);
                }

                foreach (var tf in testRun.QueryResultsByOutcome(TestOutcome.Failed))
                {
                    lstTestRunDetails.Items.Add(string.Format("{0}: {1} - {2}", tf.Outcome, tf.TestCaseTitle, tf.ErrorMessage));
                    TestResult tr = new TestResult();
                    tr.Outcome = tf.Outcome.ToString();
                    tr.Title = tf.TestCaseTitle;
                    tr.Error = tf.ErrorMessage;
                    TestResultList.Add(tr);
                }

                foreach (var tw in testRun.QueryResultsByOutcome(TestOutcome.Warning))
                {
                    lstTestRunDetails.Items.Add(string.Format("{0}: {1} - {2}", tw.Outcome, tw.TestCaseTitle, tw.ErrorMessage));
                    TestResult tr = new TestResult();
                    tr.Outcome = tw.Outcome.ToString();
                    tr.Title = tw.TestCaseTitle;
                    tr.Error = tw.ErrorMessage;
                    TestResultList.Add(tr);
                }

                foreach (var ta in testRun.QueryResultsByOutcome(TestOutcome.Aborted))
                {
                    lstTestRunDetails.Items.Add(string.Format("{0}: {1} - {2}", ta.Outcome, ta.TestCaseTitle, ta.ErrorMessage));
                    TestResult tr = new TestResult();
                    tr.Outcome = ta.Outcome.ToString();
                    tr.Title = ta.TestCaseTitle;
                    tr.Error = ta.ErrorMessage;
                    TestResultList.Add(tr);
                }

                foreach (var tb in testRun.QueryResultsByOutcome(TestOutcome.Blocked))
                {
                    lstTestRunDetails.Items.Add(string.Format("{0}: {1} - {2}", tb.Outcome, tb.TestCaseTitle, tb.ErrorMessage));
                    TestResult tr = new TestResult();
                    tr.Outcome = tb.Outcome.ToString();
                    tr.Title = tb.TestCaseTitle;
                    tr.Error = tb.ErrorMessage;
                    TestResultList.Add(tr);
                }

                foreach (var ti in testRun.QueryResultsByOutcome(TestOutcome.Inconclusive))
                {
                    lstTestRunDetails.Items.Add(string.Format("{0}: {1} - {2}", ti.Outcome, ti.TestCaseTitle, ti.ErrorMessage));
                    TestResult tr = new TestResult();
                    tr.Outcome = ti.Outcome.ToString();
                    tr.Title = ti.TestCaseTitle;
                    tr.Error = ti.ErrorMessage;
                    TestResultList.Add(tr);
                }

                foreach (var to in testRun.QueryResultsByOutcome(TestOutcome.Timeout))
                {
                    lstTestRunDetails.Items.Add(string.Format("{0}: {1} - {2}", to.Outcome, to.TestCaseTitle, to.ErrorMessage));
                    TestResult tr = new TestResult();
                    tr.Outcome = to.Outcome.ToString();
                    tr.Title = to.TestCaseTitle;
                    tr.Error = to.ErrorMessage;
                    TestResultList.Add(tr);
                }

                // Get the test results by user by passing in the Test Foundation Identity
                // testRun.QueryResultsByOwner(TeamFoundationIdentity);
                return TestResultList;
            }

           // if(testRuns.Count() == 0)
                //lstTestRunDetails.Items.Add("No Test Results have been associated with the selected build");
            return null;
        }
    


        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            ScriptManager.RegisterClientScriptBlock(this.UpdatePanel1, typeof(UpdatePanel), "clientscript6", "", true);
        }

        protected void LinkChange_Click(object sender, EventArgs e)
        {
            ScriptManager.RegisterClientScriptBlock(this.UpdatePanel1, typeof(UpdatePanel), "clientscript6", "", true);
        }

        protected void gvChild_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string theID = e.Row.DataItem.ToString();
                LinkButton lb = (LinkButton)e.Row.FindControl("LinkChange");
                if(!(lb==null))
                {
                    lb.Text = e.Row.DataItem.ToString();
                    string navString = "http://mit-tfs-prod:8080/tfs/Unifia%20Collection/Unifia/_versionControl/changeset/" + lb.Text;
                    lb.OnClientClick = "window.open('" + navString + "',null,'resizable=no,toolbar=no,scrollbars=no,menubar=yes,status=yes,width=1200,height=800')";
                }


            }
        }

        protected void gvWorkItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                WorkItem widata = (WorkItem)e.Row.DataItem;
                LinkButton lb = (LinkButton)e.Row.FindControl("LBWorkItem");
                if (!(lb == null))
                {
                    if(widata.ParentID==0)
                    {
                        lb.ForeColor = System.Drawing.Color.DarkCyan;
                    }

                    if(widata.WIType=="Bug")
                    {
                        lb.ForeColor = System.Drawing.Color.Red;
                    }

                    if (widata.WIType == "Task")
                    {
                        lb.ForeColor = System.Drawing.Color.Green;
                    }
                    lb.Text = widata.ID.ToString();
                    lb.ToolTip = widata.WIType + ":  " + widata.Title;

                    string navString = "http://mit-tfs-prod:8080/tfs/Unifia%20Collection/Unifia/_workitems#_a=edit&id=" + lb.Text + "&triage=true";
                    //string navString = "http://mit-tfs-prod:8080/tfs/Unifia%20Collection/Unifia/_versionControl/changeset/" + lb.Text;
                    lb.OnClientClick = "window.open('" + navString + "',null,'resizable=no,toolbar=no,scrollbars=no,menubar=yes,status=yes,width=1200,height=800')";
                }


            }
        }
        protected void Show_Hide_ChildGridCS(object sender, EventArgs e)
        {
            ImageButton imgShowHide = (sender as ImageButton);
            GridViewRow row = (imgShowHide.NamingContainer as GridViewRow);
            if (imgShowHide.CommandArgument == "Show")
            {
                row.FindControl("pnlCS").Visible = true;
                imgShowHide.CommandArgument = "Hide";
                imgShowHide.ImageUrl = "~/images/minus_red_button.png";
                GridView gresults = (GridView)row.FindControl("gvChild");
                gresults.Visible = true;


            }
            else
            {
                row.FindControl("pnlCS").Visible = false;
                imgShowHide.CommandArgument = "Show";
                imgShowHide.ImageUrl = "~/images/add.png";
            }
        }
        protected void Show_Hide_ChildGridWI(object sender, EventArgs e)
        {
            ImageButton imgShowHide = (sender as ImageButton);
            GridViewRow row = (imgShowHide.NamingContainer as GridViewRow);
            if (imgShowHide.CommandArgument == "Show")
            {
                row.FindControl("pnlWI").Visible = true;
                imgShowHide.CommandArgument = "Hide";
                imgShowHide.ImageUrl = "~/images/minus_red_button.png";
                GridView gresults = (GridView)row.FindControl("gvWorkItems");
                gresults.Visible = true;


            }
            else
            {
                row.FindControl("pnlWI").Visible = false;
                imgShowHide.CommandArgument = "Show";
                imgShowHide.ImageUrl = "~/images/add.png";
            }
        }
        protected void Show_Hide_ChildGrid(object sender, EventArgs e)
        {
            ImageButton imgShowHide = (sender as ImageButton);
            GridViewRow row = (imgShowHide.NamingContainer as GridViewRow);
            if (imgShowHide.CommandArgument == "Show")
            {
                row.FindControl("pnlTest").Visible = true;
                imgShowHide.CommandArgument = "Hide";
                imgShowHide.ImageUrl = "~/images/minus_red_button.png";
                GridView gresults = (GridView)row.FindControl("gvTestResults");
                gresults.Visible = true;

               
            }
            else
            {
                row.FindControl("pnlTest").Visible = false;
                imgShowHide.CommandArgument = "Show";
                imgShowHide.ImageUrl = "~/images/add.png";
            }
        }
        protected void btnHide_Click(object sender, EventArgs e)
        {
          
        }
        DataTable BringBackDataTable(GridView gv)
        {
            gvBuilds.AllowPaging = false;
            gvBuilds.DataBind();
            double tot = 0;
            double ctot = 0;
            DataTable newDT = new DataTable();
            //newDT.Columns.Add("Report_Date", typeof(string));
            newDT.Columns.Add("TaskID", typeof(string));
            newDT.Columns.Add("ReportDate", typeof(string));
            newDT.Columns.Add("Description", typeof(string));
            newDT.Columns.Add("ParentID", typeof(int));
            newDT.Columns.Add("Parent Title", typeof(string));
            newDT.Columns.Add("Hours", typeof(double));
            newDT.Columns.Add("DBS", typeof(string));
            newDT.Columns.Add("Grant", typeof(string));
            newDT.Columns.Add("EO", typeof(string));
            newDT.Columns.Add("BudgetProdIndicator", typeof(string));
            newDT.Columns.Add("Name", typeof(string));
            newDT.Columns.Add("SR Task Order ID", typeof(string));
            newDT.Columns.Add("SR ID", typeof(string));
            newDT.Columns.Add("Task State", typeof(string));


            //DateTime dt = ri.ReportDate;
            //string name = ri.Name;
            //double hrs = ri.Hours;
            //int taskid = ri.TaskID;
            //string SRID = ri.SR_ID;
            //string tDescript = ri.Description;
            //string SRTaskID = ri.SRTaskOrder_ID;
            //Label lb1 = (Label)e.Row.Cells[1].FindControl("Label11");
            //lb1.Text = dt.ToShortDateString();
            //Label lb2 = (Label)e.Row.Cells[10].FindControl("Label12");
            //lb2.Text = name;
            //Label lb3 = (Label)e.Row.Cells[5].FindControl("Label13");
            //lb3.Text = hrs.ToString("0.00");
            //Label lb4 = (Label)e.Row.Cells[0].FindControl("Label14");
            //lb4.Text = taskid.ToString();
            //Label lb5 = (Label)e.Row.Cells[11].FindControl("Label18");
            //lb5.Text = SRID;
            //Label lb6 = (Label)e.Row.Cells[12].FindControl("Label16");
            //lb6.Text = SRTaskID;
            //TextBox lb7 = (TextBox)e.Row.Cells[2].FindControl("Label17");
            //lb7.Text = tDescript;
            //Label lb8 = (Label)e.Row.Cells[6].FindControl("lblDBS");
            //lb8.Text = ri.DBS;
            //Label lb9 = (Label)e.Row.Cells[7].FindControl("lblGrant");
            //lb9.Text = ri.Grant;
            //Label lb10 = (Label)e.Row.Cells[8].FindControl("lblEO");
            //lb10.Text = ri.EO;
            //Label lb11 = (Label)e.Row.Cells[3].FindControl("Label20");
            //lb11.Text = ri.ParentID.ToString();
            //Label lb12 = (Label)e.Row.Cells[4].FindControl("Label21");
            //lb12.Text = ri.ParentTitle;
            //Label lb13 = (Label)e.Row.Cells[9].FindControl("Label22");
            //lb13.Text = ri.BudgetIndicator;
            for (int x = 0; x < gvBuilds.Rows.Count; x++)
            {
                DataRow dr = newDT.NewRow();
                Label lp3 = (Label)gvBuilds.Rows[x].Cells[0].FindControl("Label14");
                dr[0] = lp3 == null ? String.Empty : lp3.Text;
                Label lp0 = (Label)gvBuilds.Rows[x].Cells[1].FindControl("Label11");
                dr[1] = lp0 == null ? String.Empty : lp0.Text;

                Label lp1 = (Label)gvBuilds.Rows[x].Cells[4].FindControl("Label12");
                dr[10] = lp1 == null ? string.Empty : lp1.Text;

                TextBox rtt = (TextBox)gvBuilds.Rows[x].Cells[2].FindControl("Label17");
                if (rtt != null)
                {
                    if (!(string.IsNullOrEmpty(rtt.Text)))
                    {
                        //dr[2] = Server.HtmlEncode(rtt.Text);
                        dr[2] = rtt.Text;
                    }
                }
                else
                {
                    dr[2] = String.Empty;
                }

                //{

                dr[3] = lp3 == null ? String.Empty : lp3.Text;
                Label lp4 = (Label)gvBuilds.Rows[x].Cells[5].FindControl("Label13");
                dr[5] = lp4 == null ? String.Empty : lp4.Text;

                tot += Convert.ToDouble(dr[5]);
                //ctot = tot;
                //dr[4] = Math.Round(ctot,2);
                Label lp5 = (Label)gvBuilds.Rows[x].Cells[12].FindControl("Label16");
                dr[11] = lp5 == null ? String.Empty : lp5.Text;
                Label lp6 = (Label)gvBuilds.Rows[x].Cells[11].FindControl("Label18");
                dr[12] = lp6 == null ? String.Empty : lp6.Text;
                Label lp7 = (Label)gvBuilds.Rows[x].Cells[6].FindControl("lblDBS");
                dr[6] = lp6 == null ? String.Empty : lp7.Text;
                Label lp8 = (Label)gvBuilds.Rows[x].Cells[7].FindControl("lblGrant");
                dr[7] = lp8 == null ? String.Empty : lp8.Text;
                Label lp9 = (Label)gvBuilds.Rows[x].Cells[8].FindControl("lblEO");
                dr[8] = lp9 == null ? String.Empty : lp9.Text;
                Label lp10 = (Label)gvBuilds.Rows[x].Cells[3].FindControl("Label20");
                dr[3] = lp10 == null ? string.Empty : lp10.Text;
                Label lp11 = (Label)gvBuilds.Rows[x].Cells[4].FindControl("Label21");
                dr[4] = lp11 == null ? string.Empty : lp11.Text;
                Label lp12 = (Label)gvBuilds.Rows[x].Cells[9].FindControl("Label22");
                dr[9] = lp12 == null ? string.Empty : lp12.Text;
                Label lp13 = (Label)gvBuilds.Rows[x].Cells[13].FindControl("lblTaskState");
                dr[13] = lp13 == null ? string.Empty : lp13.Text;


                newDT.Rows.Add(dr);
            }


            DataRow holder = newDT.NewRow();
            DataRow dr2 = newDT.NewRow();
            dr2[0] = "TOTAL";
            dr2[5] = tot;
            newDT.Rows.Add(holder);
            newDT.Rows.Add(dr2);
            gvBuilds.AllowPaging = true;
            gvBuilds.SetPageIndex(0);
            gvBuilds.DataBind();
            return newDT;
        }
        DataTable BringBackDataTable(GridViewRow gv)
        {
            gvBuilds.AllowPaging = false;
            gvBuilds.DataBind();
            double tot = 0;
            double ctot = 0;
            DataTable newDT = new DataTable();
            //newDT.Columns.Add("Report_Date", typeof(string));
            newDT.Columns.Add("BuildNo.", typeof(string));
            newDT.Columns.Add("Changeset", typeof(string));
            newDT.Columns.Add("WorkItems", typeof(string));
            newDT.Columns.Add("Test Run", typeof(string));
            newDT.Columns.Add("BuildURL", typeof(string));
            newDT.Columns.Add("Drop Location", typeof(string));
            newDT.Columns.Add("Label", typeof(string));
            newDT.Columns.Add("Status", typeof(string));
            newDT.Columns.Add("Start", typeof(string));
            newDT.Columns.Add("End", typeof(string));
            newDT.Columns.Add("Requested For", typeof(string));
            newDT.Columns.Add("Requested By", typeof(string));


            //DateTime dt = ri.ReportDate;
            //string name = ri.Name;
            //double hrs = ri.Hours;
            //int taskid = ri.TaskID;
            //string SRID = ri.SR_ID;
            //string tDescript = ri.Description;
            //string SRTaskID = ri.SRTaskOrder_ID;
            //Label lb1 = (Label)e.Row.Cells[1].FindControl("Label11");
            //lb1.Text = dt.ToShortDateString();
            //Label lb2 = (Label)e.Row.Cells[10].FindControl("Label12");
            //lb2.Text = name;
            //Label lb3 = (Label)e.Row.Cells[5].FindControl("Label13");
            //lb3.Text = hrs.ToString("0.00");
            //Label lb4 = (Label)e.Row.Cells[0].FindControl("Label14");
            //lb4.Text = taskid.ToString();
            //Label lb5 = (Label)e.Row.Cells[11].FindControl("Label18");
            //lb5.Text = SRID;
            //Label lb6 = (Label)e.Row.Cells[12].FindControl("Label16");
            //lb6.Text = SRTaskID;
            //TextBox lb7 = (TextBox)e.Row.Cells[2].FindControl("Label17");
            //lb7.Text = tDescript;
            //Label lb8 = (Label)e.Row.Cells[6].FindControl("lblDBS");
            //lb8.Text = ri.DBS;
            //Label lb9 = (Label)e.Row.Cells[7].FindControl("lblGrant");
            //lb9.Text = ri.Grant;
            //Label lb10 = (Label)e.Row.Cells[8].FindControl("lblEO");
            //lb10.Text = ri.EO;
            //Label lb11 = (Label)e.Row.Cells[3].FindControl("Label20");
            //lb11.Text = ri.ParentID.ToString();
            //Label lb12 = (Label)e.Row.Cells[4].FindControl("Label21");
            //lb12.Text = ri.ParentTitle;
            //Label lb13 = (Label)e.Row.Cells[9].FindControl("Label22");
            //lb13.Text = ri.BudgetIndicator;
            for (int x = 0; x < gv.Cells.Count; x++)
            {
                DataRow dr = newDT.NewRow();
                Label lp3 = (Label)gv.Cells[2].FindControl("lblBuild");
                dr[0] = lp3 == null ? String.Empty : lp3.Text;
                Label lp0 = (Label)gv.Cells[3].FindControl("Label11");
                dr[1] = lp0 == null ? String.Empty : lp0.Text;

                Label lp1 = (Label)gvBuilds.Rows[x].Cells[4].FindControl("Label12");
                dr[10] = lp1 == null ? string.Empty : lp1.Text;

                TextBox rtt = (TextBox)gvBuilds.Rows[x].Cells[2].FindControl("Label17");
                if (rtt != null)
                {
                    if (!(string.IsNullOrEmpty(rtt.Text)))
                    {
                        //dr[2] = Server.HtmlEncode(rtt.Text);
                        dr[2] = rtt.Text;
                    }
                }
                else
                {
                    dr[2] = String.Empty;
                }

                //{

                dr[3] = lp3 == null ? String.Empty : lp3.Text;
                Label lp4 = (Label)gvBuilds.Rows[x].Cells[5].FindControl("Label13");
                dr[5] = lp4 == null ? String.Empty : lp4.Text;

                tot += Convert.ToDouble(dr[5]);
                //ctot = tot;
                //dr[4] = Math.Round(ctot,2);
                Label lp5 = (Label)gvBuilds.Rows[x].Cells[12].FindControl("Label16");
                dr[11] = lp5 == null ? String.Empty : lp5.Text;
                Label lp6 = (Label)gvBuilds.Rows[x].Cells[11].FindControl("Label18");
                dr[12] = lp6 == null ? String.Empty : lp6.Text;
                Label lp7 = (Label)gvBuilds.Rows[x].Cells[6].FindControl("lblDBS");
                dr[6] = lp6 == null ? String.Empty : lp7.Text;
                Label lp8 = (Label)gvBuilds.Rows[x].Cells[7].FindControl("lblGrant");
                dr[7] = lp8 == null ? String.Empty : lp8.Text;
                Label lp9 = (Label)gvBuilds.Rows[x].Cells[8].FindControl("lblEO");
                dr[8] = lp9 == null ? String.Empty : lp9.Text;
                Label lp10 = (Label)gvBuilds.Rows[x].Cells[3].FindControl("Label20");
                dr[3] = lp10 == null ? string.Empty : lp10.Text;
                Label lp11 = (Label)gvBuilds.Rows[x].Cells[4].FindControl("Label21");
                dr[4] = lp11 == null ? string.Empty : lp11.Text;
                Label lp12 = (Label)gvBuilds.Rows[x].Cells[9].FindControl("Label22");
                dr[9] = lp12 == null ? string.Empty : lp12.Text;
                Label lp13 = (Label)gvBuilds.Rows[x].Cells[13].FindControl("lblTaskState");
                dr[13] = lp13 == null ? string.Empty : lp13.Text;


                newDT.Rows.Add(dr);
            }


            DataRow holder = newDT.NewRow();
            DataRow dr2 = newDT.NewRow();
            dr2[0] = "TOTAL";
            dr2[5] = tot;
            newDT.Rows.Add(holder);
            newDT.Rows.Add(dr2);
            gvBuilds.AllowPaging = true;
            gvBuilds.SetPageIndex(0);
            gvBuilds.DataBind();
            return newDT;
        }
        protected void Button1_Click(object sender, EventArgs e)
        {
              System.Diagnostics.Process[] AllProcesses = System.Diagnostics.Process.GetProcessesByName("excel");
                    foreach (System.Diagnostics.Process ExcelProcess in AllProcesses)
                    {
                        ExcelProcess.Kill();
                    }
                    string fileName = "BECReport" + DateTime.Now.ToString("MMddyyyyhhmm") + ".xls";
                    DataTable dt = BringBackDataTable(gvBuilds);
                    string attachment = "attachment; filename=" + fileName;
                    Response.ClearContent();
                    Response.AddHeader("content-disposition", attachment);
                  // Response.ContentType = "application/vnd.ms-excel";
                   Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    string tab = "";
                    foreach (DataColumn dc in dt.Columns)
                    {
       
                        Response.Write(tab + dc.ColumnName);
                        tab = "\t";
                    }
                    Response.Write("\n");
                    int i;
                    foreach (DataRow dr in dt.Rows)
                    {
                        tab = "";
                        for (i = 0; i < dt.Columns.Count; i++)
                        {
                            if(dr[i].ToString().Contains("\n"))
                            {
                                dr[i]=dr[i].ToString().Replace("\n", " ");
                            }
                            if (dr[i].ToString().Contains("\r"))
                            {
                                dr[i] = dr[i].ToString().Replace("\r", " ");
                            }

                            if (dr[i].ToString().Contains("\t"))
                            {
                                dr[i] = dr[i].ToString().Replace("\t", " ");
                            }
                            if (dr[i].ToString().Contains("\f"))
                            {
                                dr[i] = dr[i].ToString().Replace("\f", " ");
                            }
                            if (dr[i].ToString().Contains((char)182))
                            {
                                dr[i] = dr[i].ToString().Replace((char)182, (char)32);
                            }
                            if(dr[i].ToString().Contains("&nbsp;"))
                                {
                                    dr[i] = dr[i].ToString().Replace(@"\&nbsp;", " ");
                                }
                            if(!(dr[i].ToString().Contains("\t")))
                            {
                            Response.Write(tab + dr[i].ToString());
                            }
                            else
                            {
                                 Response.Write(dr[i].ToString());
                            }
                            tab = "\t";
                        }
                        Response.Write("\n");
                    }
                    Response.End();
}

        protected void btnSpreadSheet_Click(object sender, EventArgs e)
        {

        }

        

       
       
    }
}