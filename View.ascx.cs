/*
' Copyright (c) 2014  Plugghest.com
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using System.Linq;
using Plugghest.Modules.EditCoursePluggs.Components;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using Plugghest.Base2;
using System.Text;

namespace Plugghest.Modules.EditCoursePluggs
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The View class displays the content
    /// 
    /// Typically your view control would be used to display content or functionality in your module.
    /// 
    /// View may be the only control you have in your project depending on the complexity of your module
    /// 
    /// Because the control inherits from EditCoursePluggsModuleBase you have access to any custom properties
    /// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
    /// 
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class View : EditCoursePluggsModuleBase, IActionable
    {
        public string Language;
        public int CourseId;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                Language = (Page as DotNetNuke.Framework.PageBase).PageCulture.Name;
                string courseStr = Page.Request.QueryString["c"];
                if (courseStr == null)
                {
                    HideButtons();
                    return;
                }
                bool isNum = int.TryParse(courseStr, out CourseId);
                if (!isNum)
                {
                    HideButtons();
                    return;
                }
                BaseHandler bh = new BaseHandler();
                CourseContainer cc = new CourseContainer(Language, CourseId);
                if (cc.TheCourse == null)
                {
                    HideButtons();
                    return;
                } 
                hlBackToCourse.NavigateUrl = DotNetNuke.Common.Globals.NavigateURL(cc.TheCourse.TabId, "", "");
                if (cc.TheCourse.CreatedInCultureCode != Language)
                {
                    hlIncorrectCC.Visible = true;
                    hlIncorrectCC.NavigateUrl = DotNetNuke.Common.Globals.NavigateURL(TabId, "", "language=" + cc.TheCourse.CreatedInCultureCode, "c=" + CourseId);
                    HideButtons();
                    return;
                }
                string editStr = Page.Request.QueryString["edit"];
                bool IsAuthorized = ((this.UserId != -1 && cc.TheCourse.WhoCanEdit == EWhoCanEdit.Anyone) || cc.TheCourse.CreatedByUserId == this.UserId || (UserInfo.IsInRole("Administator")));

                if (!IsAuthorized)
                {
                    HideButtons();
                    BindTree(); 
                }
                if (editStr == "reorder")
                {
                    HideButtons();
                    btnSaveReordering.Visible = true;
                    btnCancelReordering.Visible = true;
                    hdnSelectable.Value = "true";
                    hdnDragAndDrop.Value = "true";
                }
                if (editStr == "add")
                {
                    HideButtons();
                    hdnSelectable.Value = "true";
                    pnlAddPluggs.Visible = true;
                }
                if (editStr == "remove")
                {
                    HideButtons();
                    hdnSelectable.Value = "true";
                    btnRemoveSelectedPlugg.Visible = true;
                    btnCancelRemove.Visible = true;
                }
                BindTree();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void HideButtons()
        {
            btnReorder.Visible = false;
            btnAddNewPluggs.Visible = false;
            btnRemovePluggs.Visible = false;
        }

        public void BindTree()
        {
            BaseHandler bh = new BaseHandler();
            var tree = bh.GetCoursePluggsAsTree(CourseId, Language);
            if(tree.Count() == 0)
            {
                if (Page.Request.QueryString["edit"] == "add")
                {
                    btnAddAfter.Visible = false;
                    btnAddBefore.Visible = false;
                    btnAddChild.Visible = false;
                    btnAdd.Visible = true;
                }
                else
                {
                    lblNoCP.Visible = true;
                    btnRemovePluggs.Enabled = false;
                    btnReorder.Enabled = false;
                }
            }
            //JavaScriptSerializer TheSerializer = new JavaScriptSerializer();
            //hdnTreeData.Value = TheSerializer.Serialize(tree);
            hdnTreeData.Value = Newtonsoft.Json.JsonConvert.SerializeObject(tree);
        }

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection
                    {
                        {
                            GetNextActionID(), Localization.GetString("EditModule", LocalResourceFile), "", "", "",
                            EditUrl(), false, SecurityAccessLevel.Edit, true, false
                        }
                    };
                return actions;
            }
        }

        protected void btnReorder_Click(object sender, EventArgs e)
        {
            Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(TabId, "", "edit=reorder", "c=" + CourseId));
        }

        protected void btnAddNewPluggs_Click(object sender, EventArgs e)
        {
            Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(TabId, "", "edit=add", "c=" + CourseId));
        }

        protected void btnRemovePluggs_Click(object sender, EventArgs e)
        {
            Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(TabId, "", "edit=remove", "c=" + CourseId));
        }

        protected void btnSaveReordering_Click(object sender, EventArgs e)
        {
            //JavaScriptSerializer js = new JavaScriptSerializer();
            string json = hdnGetJosnResult.Value;
            //var tree = js.Deserialize<Plugghest.Base2.CoursePlugg[]>(json).ToList();
            var tree = Newtonsoft.Json.JsonConvert.DeserializeObject<CoursePlugg[]>(json).ToList();// js.Deserialize<Plugghest.Base2.CoursePlugg[]>(json).ToList();

            BaseHandler bh = new BaseHandler();
            bh.UpdateCourseTree(tree);
            BindTree();
        }

        protected void btnCancelReordering_Click(object sender, EventArgs e)
        {
            Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(TabId, "", "c=" + CourseId));
        }

        protected void btnRemoveSelectedPlugg_Click(object sender, EventArgs e)
        {
            BaseHandler bh = new BaseHandler();
            CoursePlugg cp = bh.FindCoursePlugg(Language, CourseId, Convert.ToInt32(hdnNodeCPId.Value));
            if (cp.children.Count == 0)
            {
                bh.DeleteCP(Convert.ToInt32(hdnNodeCPId.Value));
                lblCannotDelete.Visible = false;
            }
            else
                lblCannotDelete.Visible = true;
            BindTree();
        }

        protected void btnCancelRemove_Click(object sender, EventArgs e)
        {
            Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(TabId, "", "c=" + CourseId));
        }

        protected void btnAddAfter_Click(object sender, EventArgs e)
        {
            if(!CheckAddText())
            {
                lblCannotAdd.Visible = true;
                return;
            }
            BaseHandler bh = new BaseHandler();
            CoursePluggEntity SelectedCP = bh.GetCPEntity(Convert.ToInt32(hdnNodeCPId.Value));
            CreateCP(SelectedCP.CPOrder + 1, SelectedCP.MotherId);
        }

        protected void btnAddBefore_Click(object sender, EventArgs e)
        {
            if (!CheckAddText())
            {
                lblCannotAdd.Visible = true;
                return;
            }
            BaseHandler bh = new BaseHandler();
            CoursePluggEntity SelectedCP = bh.GetCPEntity(Convert.ToInt32(hdnNodeCPId.Value));
            CreateCP(SelectedCP.CPOrder, SelectedCP.MotherId);
        }

        protected void btnAddChild_Click(object sender, EventArgs e)
        {
            if (!CheckAddText())
            {
                lblCannotAdd.Visible = true;
                return;
            }
            BaseHandler bh = new BaseHandler();
            CoursePluggEntity SelectedCP = bh.GetCPEntity(Convert.ToInt32(hdnNodeCPId.Value));
            CreateCP(1, SelectedCP.CoursePluggId);
        }

        //Add to empty Course
        protected void btnAdd_Click(object sender, EventArgs e)
        {
            if (!CheckAddText())
            {
                lblCannotAdd.Visible = true;
                return;
            }
            CreateCP(1, 0);
            btnAddAfter.Visible = true;
            btnAddBefore.Visible = true;
            btnAddChild.Visible = true;
            btnAdd.Visible = false;
        }

        private void CreateCP(int order, int motherId)
        {
            string[] pluggIds = txtAddPlugg.Text.Trim().Split(',');
            BaseHandler bh = new BaseHandler();
            for (int i = 0; i < pluggIds.Length; i++)
            {
                CoursePluggEntity newCP = new CoursePluggEntity();
                newCP.CourseId = CourseId;
                newCP.MotherId = motherId;
                newCP.CreatedByUserId = UserId;
                newCP.PluggId = Convert.ToInt32(pluggIds[i]);
                newCP.CPOrder = order+i;
                bh.CreateCP(newCP);
            }
            txtAddPlugg.Text = "";
            lblPluggInfo.Text = "";
            BindTree();
        }

        protected void btnCancelAdd_Click(object sender, EventArgs e)
        {
            Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(TabId, "", "c=" + CourseId));
        }

        protected void btnCheckPluggs_Click(object sender, EventArgs e)
        {
            lblPluggInfo.Text = "";
            BaseHandler ph = new BaseHandler();

            //string pluggtext = "12,56,34,45,56";
            string pluggtext = txtAddPlugg.Text.Trim();
            if (!string.IsNullOrEmpty(pluggtext))
            {
                string[] itempluggs = pluggtext.Split(',');

                //Check that entered Pluggs is in the correct format "12,56,34,45,56"
                int num;
                bool isNumeric;
                bool success = true;
                for (int i = 0; i < itempluggs.Length; i++)
                {
                    isNumeric = int.TryParse(itempluggs[i], out num);
                    if (!isNumeric)
                        success = false;
                }
                if (!success)
                {
                    lblPluggInfo.Text = Localization.GetString("IncorrectPluggString.Text", LocalResourceFile);
                    return;
                }

                StringBuilder s = new StringBuilder();
                s.Append("<ul>");
                int pluggId;
                for (int i = 0; i < itempluggs.Length; i++)
                {
                    pluggId = Convert.ToInt32(itempluggs[i]);
                    PluggContainer p = new PluggContainer(Language, pluggId);
                    s.Append("<li><strong>ID</strong>: ").Append(pluggId).Append(". ");
                    if (p.ThePlugg != null)
                    {
                        p.LoadTitle();
                        p.LoadDescription();
                        s.Append("<strong>").Append(Localization.GetString("Title.Text", LocalResourceFile)).Append("</strong>: ");
                        s.Append(p.TheTitle.Text);
                        s.Append(". <strong>").Append(Localization.GetString("Description.Text", LocalResourceFile)).Append("</strong>: ");
                        if(p.TheDescription != null)
                            s.Append(p.TheDescription.Text);
                    }
                    else
                    {
                        s.Append(Localization.GetString("NoPlugg.Text", LocalResourceFile));
                    }
                    s.Append("</li>");
                }
                lblPluggInfo.Text = s.ToString();
            }

            //return ischecked;
        }

        private bool CheckAddText()
        {
            string[] itempluggs = txtAddPlugg.Text.Trim().Split(',');
            int num;
            for (int i = 0; i < itempluggs.Length; i++)
            {
                if (!int.TryParse(itempluggs[i], out num))
                    return false;
            }
            for (int i = 0; i < itempluggs.Length; i++)
            {
                PluggContainer p = new PluggContainer(Language, Convert.ToInt32(itempluggs[i]));
                if (p.ThePlugg == null)
                    return false;
            }
            return true;
        }


    }
}