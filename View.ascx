<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="Plugghest.Modules.EditCoursePluggs.View" %>

<script src="/DesktopModules/EditSubjects/js/tree.jquery.js"></script>
<link href="/DesktopModules/EditSubjects/js/jqtree.css" rel="stylesheet" />
<link href="/DesktopModules/EditSubjects/module.css" rel="stylesheet" />

<asp:HiddenField ID="hdnTreeData" runat="server" Value="" />
<asp:HiddenField ID="hdnDragAndDrop" runat="server" Value="false" />
<asp:HiddenField ID="hdnSelectable" runat="server" Value="false" />
<asp:HiddenField ID="hdnGetJosnResult" runat="server" />
<asp:HiddenField ID="hdnNodeCPId" runat="server" />

<div class="tree">
    <div id="tree2"></div>
</div>

<asp:Label ID="lblNoCP" resourcekey="NoCP.Text" runat="server" Visible="false" />
<asp:Hyperlink ID="hlIncorrectCC" resourcekey="IncorrectCC.Text" runat ="server" visible="false"/><br />
<asp:Button ID="btnReorder" runat="server" resourcekey="Reorder.Text" OnClick="btnReorder_Click" />
<asp:Button ID="btnAddNewPluggs" runat="server" resourcekey="AddNew.Text" OnClick="btnAddNewPluggs_Click" />
<asp:Button ID="btnRemovePluggs" runat="server" resourcekey="Remove.Text" OnClick="btnRemovePluggs_Click" />
<asp:Button ID="btnSaveReordering" runat="server" resourcekey="SaveReorder.Text" Visible="False" OnClientClick ="getjson();" OnClick="btnSaveReordering_Click"/>
<asp:Button ID="btnCancelReordering" resourcekey="ExitReorder.Text" runat="server" Visible="False" OnClick="btnCancelReordering_Click"/>
<asp:Button ID="btnRemoveSelectedPlugg" resourcekey="RemovePlugg.Text" runat="server" Visible="False" OnClientClick ="return getCPid();" OnClick="btnRemoveSelectedPlugg_Click"/>
<asp:Button ID="btnCancelRemove" resourcekey="CancelRemove.Text" runat="server" Visible="False" OnClick="btnCancelRemove_Click"/>
<br />
<asp:Label ID="lblCannotDelete" resourcekey="CannotDelete.Text" runat="server" Visible="false"  />

<asp:Panel ID="pnlAddPluggs" runat="server" Visible="False">
    <div>
        <div class="CPdiv">
            <asp:Label ID="lblAddPluggId" resourcekey="AddPluggId.Text" runat="server"  /> <br />
            <asp:TextBox ID="txtAddPlugg" runat="server" Width="316px"></asp:TextBox>
            <asp:Button ID="btnCheckPluggs" resourcekey="CheckPluggs.Text" runat="server" OnClick="btnCheckPluggs_Click" />
        </div>
        <asp:Label ID="lblPluggInfo" Text="" runat="server"  /> <br />
        <asp:Button ID="btnAddAfter" resourcekey="AddAfter.Text" runat="server" OnClientClick="return getCPid();" OnClick="btnAddAfter_Click" />
        <asp:Button ID="btnAddBefore" resourcekey="AddBefore.Text" runat="server" OnClientClick="return getCPid();" OnClick="btnAddBefore_Click"/>
        <asp:Button ID="btnAddChild" resourcekey="AddChild.Text" runat="server" OnClientClick="return getCPid();" OnClick="btnAddChild_Click"/>
        <asp:Button ID="btnAdd" resourcekey="Add.Text" runat="server" OnClientClick="return ChecktxtAddPlugg();" OnClick="btnAdd_Click"/>
        <asp:Button ID="btnCancelAdd" resourcekey="CancelAdd.Text" runat="server" OnClick="btnCancelAdd_Click"/> <br />
        <asp:Label ID="lblCannotAdd" resourcekey="CannotAdd.Text" runat="server"  Visible ="false"/>
    </div>
    <br />
</asp:Panel> 

<script type="text/javascript">

    $(document).ready(function () {

        $('#tree2').tree({
            data: eval($("#" + '<%=hdnTreeData.ClientID%>').attr('value')),
            dragAndDrop: eval($("#" + '<%=hdnDragAndDrop.ClientID%>').attr('value')),
            selectable: eval($("#" + '<%=hdnSelectable.ClientID%>').attr('value')),
            autoEscape: false,
            autoOpen: false
        });
    });

    function getjson() {
        var record = $('#tree2').tree('toJson');
        $("#<%=hdnGetJosnResult.ClientID%>").val(record);
        return true;
    }

    function getCPid() {
        var node = $('#tree2').tree('getSelectedNode');
        var Error = "";

        if (!node)
            Error = 'Please Select Node \n';
        if ($("#<%=txtAddPlugg.ClientID%>").val() == '')
            Error += 'Please Enter the ID of the Plugg you would like to add';

        if (Error != "") {
            alert(Error);
            return false;
        }
        $("#<%=hdnNodeCPId.ClientID%>").val(node.CoursePluggId);
    }

    function ChecktxtAddPlugg() {
        var Error = "";
        if ($("#<%=txtAddPlugg.ClientID%>").val() == '')
            Error += 'Please Enter the ID of the Plugg you would like to add';

        if (Error != "") {
            alert(Error);
            return false;
        }
    }

</script>


