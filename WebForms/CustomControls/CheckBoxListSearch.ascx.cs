using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms.CustomControls
{
    public partial class CheckBoxListSearch : UserControl
    {
        public event EventHandler SelectedIndexChanged;


        protected CheckBoxList chkList;
        protected LinkButton btnDeselectAll;
        protected Literal litTitle;

        protected void Page_Load(object sender, EventArgs e)
        {
            UpdateDropdownTitle();
            if (!IsPostBack)
            {
                UpdateDeselectAllButtonState();
            }
        }

        public int SelectedIndex
        {
            get => chkList.SelectedIndex;
            set
            {
                if (value >= 0 && value < chkList.Items.Count)
                {
                    chkList.ClearSelection();
                    chkList.Items[value].Selected = true;
                }
            }
        }

        public ListItem SelectedItem
        {
            get => chkList.SelectedItem;
            set
            {
                if (value != null)
                {
                    chkList.ClearSelection();
                    ListItem item = chkList.Items.FindByValue(value.Value);
                    if (item != null)
                    {
                        item.Selected = true;
                    }
                }
            }
        }

        public object DataSource
        {
            get => chkList.DataSource;
            set => chkList.DataSource = value;
        }

        public string DataTextField
        {
            get => chkList.DataTextField;
            set => chkList.DataTextField = value;
        }

        public string DataValueField
        {
            get => chkList.DataValueField;
            set => chkList.DataValueField = value;
        }

        public override void DataBind()
        {
            chkList.DataBind();
        }

        public ListItemCollection Items => chkList.Items;

        public List<int> SelectedIndexes => chkList.Items
            .Cast<ListItem>()
            .Select((item, index) => item.Selected ? index : -1)
            .Where(index => index != -1)
            .ToList();

        public string SelectedValue => chkList.Items
            .Cast<ListItem>()
            .FirstOrDefault(item => item.Selected)?.Value ?? string.Empty;

        public List<string> SelectedValues => chkList.Items
            .Cast<ListItem>()
            .Where(item => item.Selected)
            .Select(item => item.Value)
            .ToList();

        public List<ListItem> SelectedItems => chkList.Items
            .Cast<ListItem>()
            .Where(item => item.Selected)
            .ToList();

        private void UpdateDropdownTitle()
        {
            // Calculamos la cantidad de elementos seleccionados
            int selectedCount = chkList.Items.Cast<ListItem>().Count(item => item.Selected);
            // Si hay selección mostramos la cantidad, de lo contrario "Todos ▼"
            string title = selectedCount > 0
                ? $"{selectedCount} seleccionado{(selectedCount > 1 ? "s" : "")} ▼"
                : "Todos ▼";
            litTitle.Text = title;
        }

        public void ClearSelection()
        {
            chkList.ClearSelection();
            UpdateDeselectAllButtonState();
            UpdateDropdownTitle();
        }

        protected void ChkList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedIndexChanged?.Invoke(this, e);
            UpdateDeselectAllButtonState();
            UpdateDropdownTitle();
        }

        private void UpdateDeselectAllButtonState()
        {
            bool hasSelectedItems = chkList.Items.Cast<ListItem>().Any(item => item.Selected);
            btnDeselectAll.CssClass = hasSelectedItems ? "btn btn-secondary bi bi-funnel-fill" : "btn btn-secondary bi bi-funnel";
            btnDeselectAll.Enabled = hasSelectedItems;
        }

        protected void BtnDeselectAll_Click(object sender, EventArgs e)
        {
            ClearSelection();
            ScriptManager.RegisterStartupScript(this, GetType(), "updateDropdownTitle", $"updateDropdownTitle('{chkList.ClientID}');", true);
            ChkList_SelectedIndexChanged(this,e);
        }
    }
}

