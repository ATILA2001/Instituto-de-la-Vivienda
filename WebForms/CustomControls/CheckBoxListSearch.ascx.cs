using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms.CustomControls
{
    public partial class CheckBoxListSearch : UserControl
    {
        protected CheckBoxList chkList;

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


    }
}

