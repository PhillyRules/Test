<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GetBuildInfo.aspx.cs" Inherits="TFSTRx.GetBuildInfo"  AsyncTimeout="600000"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Unifia Build Details</title>
    <style type="text/css">
#overlay {
    position: fixed;
    z-index: 99;
    top: 0px;
    left: 0px;
    background-color: #000000;
    width: 100%;
    height: 100%;
    filter: Alpha(Opacity=90);
    opacity: 0.7;
    -moz-opacity: 0.7;
}            
#theprogress {
    background-color: #000000;
    padding:10px;
    width: 300px;
    height: 30px;
    filter: Alpha(Opacity=100);
    opacity: 1;
    -moz-opacity: 1;
}
#modalprogress {
    position: absolute;
    top: 40%;
    left: 50%;
    margin: -11px 0 0 -150px;
    color: #990000;
    font-weight:bold;
    font-size:14px;
}
.mGrid {   
    width: 100%;   
    background-color: #fff;   
    margin: 5px 0 10px 0;   
    border: solid 1px #525252;   
    border-collapse:collapse;   
}  
.mGrid td {   
    padding: 2px;   
    border: solid 1px #c1c1c1;   
    
}  
.mGrid th {   
    padding: 4px 2px;   
    color: #fff;   
    background: #000000 url(grd_head.png) repeat-x top;
    filter: Alpha(Opacity=100);   
    border-left: solid 1px #525252;   
    font-size: 0.9em;   
}  
.mGrid .alt { background: #fcfcfc url(grd_alt.png) repeat-x top; }  
.mGrid .pgr { background: #424242 url(grd_pgr.png) repeat-x top; }  
.mGrid .pgr table { margin: 5px 0; }  
.mGrid .pgr td {   
    border-width: 0;   
    padding: 0 6px;   
    border-left: solid 1px #666;   
    font-weight: bold;   
    color: #fff;   
    line-height: 12px;   
 }     
.mGrid .pgr a { color: #666; text-decoration: none; }  
.mGrid .pgr a:hover { color: #000; text-decoration: none; }  
    </style>

     
</head>

<body>
    
    <form id="form1" runat="server">
           
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <asp:UpdateProgress ID="prgLoadingStatus" runat="server" DynamicLayout="true" AssociatedUpdatePanelID="UpdatePanel1" >
            <ProgressTemplate>
           <div id="overlay">
                 <div id="modalprogress">
                    <div id="theprogress">
                        <span style="color:#B0EBE3;font-size:x-large;text-align:center;">LOADING, PLEASE WAIT</span><br />
                        </div>
                     <div style="height:30px"></div>
                        <div style="height:20px;left:25px;text-align:center;">
                    <asp:Image ID="imgWaitIcon" runat="server" ImageAlign="AbsMiddle" ImageUrl="images/BlackMover.gif" />
                </div>
            </div>
        </div>
    </ProgressTemplate>
</asp:UpdateProgress>  
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <Triggers>
                <asp:AsyncPostBackTrigger  ControlID="dd1" />
            </Triggers>
             <ContentTemplate>
     
          <div style="padding-bottom:10px;text-align:center;">
            <asp:Label ID="Label1" runat="server" Text="Unifia Build Dashboard" Font-Size="XX-Large" Font-Bold="False" ForeColor="#006699"></asp:Label>

        </div>
                 <div style="padding-bottom:10px;text-align:center;">
            <asp:Label ID="Label2" runat="server" Text="Select Build Def." Font-Size="Large" Font-Bold="False" ForeColor="#006699"></asp:Label>

        </div>
    <div id="Control Div" style="text-align:center" >
   
        
    
        <asp:DropDownList ID="dd1" runat="server" AutoPostBack="True" Height="28px" Width="319px" OnSelectedIndexChanged="dd1_SelectedIndexChanged">
        </asp:DropDownList>
   
       
    
    </div>       
        <div style="text-align:center;">
        <div style="text-align:center;padding-top:20px;padding-bottom:20px;">
            <asp:Label ID="lblCount" runat="server" Text="" Font-Size="Medium" Font-Bold="True"></asp:Label>
        </div>
        <div style="text-align:center;">
            <asp:GridView ID="gvBuilds" runat="server"  Height="134px" Width="313px" BackColor="White" BorderColor="#CCCCCC" BorderStyle="Solid" BorderWidth="1px" CellPadding="3"  OnRowCommand="gvBuilds_RowCommand" OnRowDataBound="gvBuilds_RowDataBound" AutoGenerateColumns="False" HorizontalAlign="Center" CssClass="mGrid">
                <AlternatingRowStyle BackColor="Black" ForeColor="White" Font-Bold="True" />
                <Columns>
                   
                   <asp:TemplateField>
                    <ItemTemplate>
                            <asp:Button runat="server" id="btnMoreInfo"
                            CommandName="MoreInfo" 
                            CommandArgument="<%# ((GridViewRow) Container).RowIndex %>"
                            Text="More Details" />
                        </ItemTemplate>
                       </asp:TemplateField>
                    <asp:TemplateField HeaderText="Build #">
                        <ItemTemplate>
                            <asp:Label ID="lblBuild" runat="server"></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="ChangeSet">
                        <ItemTemplate>
                            <div>
                              <asp:ImageButton ID="imgShowCS" runat="server" OnClick="Show_Hide_ChildGridCS" ImageUrl="~/images/add.png"
                              CommandArgument="Show" Visible="false" />
                            <asp:Panel ID="pnlCS" runat="server" Visible="false" Style="position: relative">
                            <asp:GridView ID="gvChild" runat="server" BackColor="White" BorderColor="#336666" BorderStyle="Double" BorderWidth="3px" CellPadding="4" GridLines="Horizontal" Visible="False" AutoGenerateColumns="False" OnRowDataBound="gvChild_RowDataBound">
                                <Columns>
                                    <asp:TemplateField HeaderText="Change ID">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="LinkChange" runat="server" OnClick="LinkChange_Click"></asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <FooterStyle BackColor="White" ForeColor="#333333" />
                                <HeaderStyle BackColor="#336666" Font-Bold="True" ForeColor="White" />
                                <PagerStyle BackColor="#336666" ForeColor="White" HorizontalAlign="Center" />
                                <RowStyle BackColor="White" ForeColor="#333333" />
                                <SelectedRowStyle BackColor="#339966" Font-Bold="True" ForeColor="White" />
                                <SortedAscendingCellStyle BackColor="#F7F7F7" />
                                <SortedAscendingHeaderStyle BackColor="#487575" />
                                <SortedDescendingCellStyle BackColor="#E5E5E5" />
                                <SortedDescendingHeaderStyle BackColor="#275353" />
                            </asp:GridView>
                                </asp:Panel>
                                </div>
                        </ItemTemplate>
                        <ItemStyle VerticalAlign="Top" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Work Items">
                        <ItemTemplate>
                              <asp:ImageButton ID="imgShowWI" runat="server" OnClick="Show_Hide_ChildGridWI" ImageUrl="~/images/add.png"
                              CommandArgument="Show" Visible="false"/>
                            <asp:Panel ID="pnlWI" runat="server" Visible="false" Style="position: relative">
                            <asp:GridView ID="gvWorkItems" runat="server" AutoGenerateColumns="False" OnRowDataBound="gvWorkItems_RowDataBound" BackColor="White" BorderColor="#999999" BorderStyle="Solid" BorderWidth="1px" CellPadding="3" ForeColor="Black" GridLines="Vertical">
                                <AlternatingRowStyle BackColor="#CCCCCC" />
                                <Columns>
                                    <asp:TemplateField HeaderText="WorkItem ID">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="LBWorkItem" runat="server"></asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <FooterStyle BackColor="#CCCCCC" />
                                <HeaderStyle BackColor="Black" Font-Bold="True" ForeColor="White" />
                                <PagerStyle BackColor="#999999" ForeColor="Black" HorizontalAlign="Center" />
                                <SelectedRowStyle BackColor="#000099" Font-Bold="True" ForeColor="White" />
                                <SortedAscendingCellStyle BackColor="#F1F1F1" />
                                <SortedAscendingHeaderStyle BackColor="#808080" />
                                <SortedDescendingCellStyle BackColor="#CAC9C9" />
                                <SortedDescendingHeaderStyle BackColor="#383838" />
                            </asp:GridView>
                                </asp:Panel>
                        </ItemTemplate>
                        <ItemStyle VerticalAlign="Top" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="TestRun Details">
                        <ItemTemplate>
                             <asp:ImageButton ID="imgShow" runat="server" OnClick="Show_Hide_ChildGrid" ImageUrl="~/images/add.png"
                              CommandArgument="Show"  Visible="false"/>
                            <asp:Panel ID="pnlTest" runat="server" Visible="false" Style="position: relative">
                            <asp:GridView ID="gvTestResults" runat="server" BackColor="#DEBA84" BorderColor="#DEBA84" BorderStyle="None" BorderWidth="1px" CellPadding="3" CellSpacing="2">
                                <FooterStyle BackColor="#F7DFB5" ForeColor="#8C4510" />
                                <HeaderStyle BackColor="#A55129" Font-Bold="True" ForeColor="White" />
                                <PagerStyle ForeColor="#8C4510" HorizontalAlign="Center" />
                                <RowStyle BackColor="#FFF7E7" ForeColor="#8C4510" HorizontalAlign="Left" VerticalAlign="Top" />
                                <SelectedRowStyle BackColor="#738A9C" Font-Bold="True" ForeColor="White" />
                                <SortedAscendingCellStyle BackColor="#FFF1D4" />
                                <SortedAscendingHeaderStyle BackColor="#B95C30" />
                                <SortedDescendingCellStyle BackColor="#F1E5CE" />
                                <SortedDescendingHeaderStyle BackColor="#93451F" />
                            </asp:GridView>
                                </asp:Panel>
                        </ItemTemplate>
                        <ItemStyle VerticalAlign="Top" />
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Build Url">
                        <ItemTemplate>
                            <asp:LinkButton ID="LinkButton1" runat="server" OnClick="LinkButton1_Click" BackColor="Black" ForeColor="#66FFFF" Height="24px" Width="274px">LinkButton</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Drop Loc.">
                        <ItemTemplate>
                            <asp:Label ID="lblDrop" runat="server"></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Label">
                        <ItemTemplate>
                            <asp:Label ID="lblLabel" runat="server"></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <asp:Label ID="lblStatus" runat="server"></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="LastChanged">
                        <ItemTemplate>
                            <asp:Label ID="lblLastChange" runat="server"></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Reason">
                        <ItemTemplate>
                            <asp:Label ID="lblReason" runat="server"></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="StartTime">
                        <ItemTemplate>
                            <asp:Label ID="lblStartTime" runat="server"></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="EndTime">
                        <ItemTemplate>
                            <asp:Label ID="lblEndTime" runat="server"></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="RequestedFor">
                        <ItemTemplate>
                            <asp:Label ID="lblRequestedFor" runat="server"></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="RequestedBy">
                        <ItemTemplate>
                            <asp:Label ID="lblRequestedBy" runat="server"></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <FooterStyle BackColor="White" ForeColor="#000066" />
                <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
                <PagerStyle BackColor="White" ForeColor="#000066" HorizontalAlign="Left" />
                <RowStyle ForeColor="Black" Font-Bold="True" />
                <SelectedRowStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
                <SortedAscendingCellStyle BackColor="#F1F1F1" />
                <SortedAscendingHeaderStyle BackColor="#007DBB" />
                <SortedDescendingCellStyle BackColor="#CAC9C9" />
                <SortedDescendingHeaderStyle BackColor="#00547E" />
            </asp:GridView>
        </div>
            </div>
           </ContentTemplate>
             </asp:UpdatePanel>
    </form>
</body>
</html>
