using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace WebForms.CustomControls
{
    public partial class CheckBoxListSearch : UserControl
    {
        //public event EventHandler SelectedIndexChanged;
        // Modificar la definición del evento para coincidir con TreeView
        public event EventHandler<TreeNodeEventArgs> SelectedIndexChanged;
        public event EventHandler AcceptChanges;

        protected TreeView chkList;
        protected LinkButton chkList_btnDeselectAll;
        protected HtmlGenericControl litTitle;
        protected HtmlButton dropdownButton;


        private string _dataTextField;
        private string _dataValueField;
        private object _dataSource;


        public string HeaderText { get; set; } = "Filtro";

        public object DataSource
        {
            get => _dataSource;
            set => _dataSource = value;
        }

        public string DataTextField
        {
            get => _dataTextField;
            set => _dataTextField = value;
        }

        public string DataValueField
        {
            get => _dataValueField;
            set => _dataValueField = value;
        }

        private string GetSessionKeyForSelectedValues() 
        { 
            return $"CheckBoxListSearch_SelectedValues_{this.ID}"; 
        }
        
        private void SaveSelectedValuesToSession(List<string> values)
        {
            string sessionKey = GetSessionKeyForSelectedValues();
            HttpContext.Current.Session[sessionKey] = values;
        }

        private List<string> LoadSelectedValuesFromSession()
        {
            string sessionKey = GetSessionKeyForSelectedValues();
            var loadedValues = HttpContext.Current.Session[sessionKey] as List<string>;
            return loadedValues ?? new List<string>();
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (dropdownButton != null && chkList != null)
            {
                dropdownButton.Attributes["onclick"] = $"toggleDropdown('{chkList.ClientID}_dropdown'); return false;"; // Añadido return false;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (dropdownButton != null && !string.IsNullOrEmpty(HeaderText))
                {
                    dropdownButton.Attributes["data-default-text"] = HeaderText;
                }
                if (litTitle != null)
                {
                    if (!string.IsNullOrEmpty(HeaderText))
                    {
                        litTitle.Attributes["data-default-text"] = HeaderText;
                        // Contenido inicial del span
                        litTitle.InnerHtml = $"{HeaderText} <i class=\"bi bi-caret-down\"></i>";
                    }
                    else // Fallback si HeaderText está vacío
                    {
                        // Valor por defecto
                        litTitle.Attributes["data-default-text"] = "Filtro";
                        litTitle.InnerHtml = $"Filtro <i class=\"bi bi-caret-down\"></i>";
                    }
                }

                if (chkList != null && litTitle != null)
                {
                    chkList.Attributes["data-title-id"] = litTitle.ClientID;
                }
            }
        }

        protected void BtnAccept_Click(object sender, EventArgs e)
        {
            List<string> currentSelectedValues = this.SelectedValues;

            string contextKey = $"CheckBoxListSearch_{this.ID}_ContextSelectedValues";
            HttpContext.Current.Items[contextKey] = currentSelectedValues;

            SaveSelectedValuesToSession(currentSelectedValues);

            AcceptChanges?.Invoke(this, EventArgs.Empty);
        }

        public override void DataBind()
        {
            try
            {
                bool populateNodes = false;
                List<string> selectedValuesToRestore = null;

                // Determine if nodes need to be (re)populated and load persisted selections
                if (!IsPostBack)
                {
                    // Initial page load
                    populateNodes = (_dataSource != null);
                    // For initial display, load any previously saved selections from session
                    selectedValuesToRestore = LoadSelectedValuesFromSession();
                }
                else // Is PostBack
                {
                    string contextKey = $"CheckBoxListSearch_{this.ID}_ContextSelectedValues";
                    if (HttpContext.Current.Items.Contains(contextKey))
                    {
                        // This control's "Accept" button triggered the postback, use values from current request context
                        selectedValuesToRestore = HttpContext.Current.Items[contextKey] as List<string>;
                    }
                    else
                    {
                        // Postback triggered by something else (another control, or this control's checkbox auto-postback)
                        // Load this control's persisted selections from session
                        selectedValuesToRestore = LoadSelectedValuesFromSession();
                    }

                    // If TreeView is empty on postback (e.g., ViewState might be off for TreeView or dynamic creation)
                    if (chkList.Nodes.Count == 0)
                    {
                        populateNodes = (_dataSource != null);
                    }
                }

                if (populateNodes)
                {
                    chkList.Nodes.Clear();
                    TreeNode selectAllNode = new TreeNode("Seleccionar todos", "select-all")
                    {
                        SelectAction = TreeNodeSelectAction.None,
                        ShowCheckBox = true,
                        Expanded = true // Consider managing expansion state if needed
                    };
                    chkList.Nodes.Add(selectAllNode);

                    if (_dataSource != null)
                    {
                        bool areDates = false;
                        // Date detection logic (simplified for brevity, use your existing logic)
                        if (!string.IsNullOrEmpty(DataValueField) && _dataSource is IEnumerable<object> enumerableForDateCheck)
                        {
                            var firstItem = enumerableForDateCheck.FirstOrDefault();
                            if (firstItem != null)
                            {
                                string sampleValue = GetPropertyValue(firstItem, DataValueField);
                                if (DateTime.TryParseExact(sampleValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                                {
                                    areDates = true;
                                }
                            }
                        }
                        else if (_dataSource is DataTable dtForDateCheck && dtForDateCheck.Rows.Count > 0 && !string.IsNullOrEmpty(DataValueField) && dtForDateCheck.Columns.Contains(DataValueField))
                        {
                            string sampleValue = dtForDateCheck.Rows[0][DataValueField]?.ToString();
                            if (DateTime.TryParseExact(sampleValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                            {
                                areDates = true;
                            }
                        }

                        if (areDates)
                        {
                            var dates = new List<DateTime>();
                            if (_dataSource is DataTable dtDateSource)
                            {
                                if (!string.IsNullOrEmpty(DataValueField) && dtDateSource.Columns.Contains(DataValueField))
                                {
                                    foreach (DataRow row in dtDateSource.Rows.Cast<DataRow>())
                                    {
                                        string dateStr = GetPropertyValue(row, DataValueField); // Use helper for DataRow
                                        if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                                        {
                                            dates.Add(parsedDate);
                                        }
                                    }
                                }
                            }
                            else if (_dataSource is IEnumerable<object> dateEnumerable)
                            {
                                foreach (var item in dateEnumerable)
                                {
                                    string dateStr = GetPropertyValue(item, DataValueField);
                                    if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                                    {
                                        dates.Add(parsedDate);
                                    }
                                }
                            }

                            dates = dates.Distinct().ToList();

                            var yearGroups = dates.GroupBy(d => d.Year).OrderByDescending(g => g.Key);
                            foreach (var yearGroup in yearGroups)
                            {
                                var yearNode = new TreeNode($"{yearGroup.Key}", $"year_{yearGroup.Key}")
                                { ShowCheckBox = true, SelectAction = TreeNodeSelectAction.None, Expanded = false }; // Consider persisting/restoring Expanded state
                                var monthGroups = yearGroup.GroupBy(d => d.Month).OrderBy(g => g.Key);
                                foreach (var monthGroup in monthGroups)
                                {
                                    var monthNode = new TreeNode(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthGroup.Key), $"month_{yearGroup.Key}_{monthGroup.Key}")
                                    { ShowCheckBox = true, SelectAction = TreeNodeSelectAction.None, Expanded = false }; // Consider persisting/restoring Expanded state
                                    foreach (var day in monthGroup.OrderBy(d => d.Day))
                                    {
                                        monthNode.ChildNodes.Add(new TreeNode(day.ToString("dd"), day.ToString("yyyy-MM-dd"))
                                        { ShowCheckBox = true, SelectAction = TreeNodeSelectAction.None });
                                    }
                                    if (monthNode.ChildNodes.Count > 0) yearNode.ChildNodes.Add(monthNode);
                                }
                                if (yearNode.ChildNodes.Count > 0) selectAllNode.ChildNodes.Add(yearNode);
                            }
                        }
                        else // Build flat list
                        {
                            if (_dataSource is DataTable dtSource)
                            {
                                foreach (DataRow row in dtSource.Rows.Cast<DataRow>())
                                {
                                    var text = GetPropertyValue(row, DataTextField) ?? string.Empty; // Use helper for DataRow
                                    var value = (!string.IsNullOrEmpty(DataValueField) && dtSource.Columns.Contains(DataValueField))
                                                ? GetPropertyValue(row, DataValueField) ?? text
                                                : text;
                                    if (!string.IsNullOrEmpty(text))
                                    {
                                        selectAllNode.ChildNodes.Add(new TreeNode(text, value)
                                        {
                                            ShowCheckBox = true,
                                            SelectAction = TreeNodeSelectAction.None
                                        });
                                    }
                                }
                            }
                            else if (_dataSource is IEnumerable<object> enumerableDataSource)
                            {
                                foreach (var item in enumerableDataSource)
                                {
                                    var text = GetPropertyValue(item, DataTextField) ?? string.Empty;
                                    var value = !string.IsNullOrEmpty(DataValueField)
                                                ? GetPropertyValue(item, DataValueField) ?? text
                                                : text;
                                    if (!string.IsNullOrEmpty(text))
                                    {
                                        selectAllNode.ChildNodes.Add(new TreeNode(text, value)
                                        {
                                            ShowCheckBox = true,
                                            SelectAction = TreeNodeSelectAction.None
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                // Apply selected states to the TreeView nodes
                if (chkList.Nodes.Count > 0)
                {
                    // First, reset all nodes to unchecked. This is important if nodes came from ViewState or were just repopulated.
                    foreach (TreeNode node in chkList.Nodes.Cast<TreeNode>().SelectMany(GetAllNodes))
                    {
                        node.Checked = false;
                    }

                    // Apply the restored selections
                    if (selectedValuesToRestore != null && selectedValuesToRestore.Any())
                    {
                        foreach (TreeNode node in chkList.Nodes.Cast<TreeNode>().SelectMany(GetAllNodes))
                        {
                            if (selectedValuesToRestore.Contains(node.Value))
                            {
                                node.Checked = true;
                            }
                        }
                    }

                    // Update the "select-all" node's checked state based on its children's states
                    if (chkList.Nodes.Count > 0 && chkList.Nodes[0].Value == "select-all")
                    {
                        TreeNode selectAllServerNode = chkList.Nodes[0];
                        var leafNodes = selectAllServerNode.ChildNodes
                            .Cast<TreeNode>()
                            .SelectMany(GetAllNodes)
                            .Where(n => n.ChildNodes.Count == 0 && n.Value != "select-all")
                            .ToList();

                        if (leafNodes.Any())
                        {
                            selectAllServerNode.Checked = leafNodes.All(n => n.Checked);
                        }
                        else
                        {
                            // No actual items to select, so "select-all" should be unchecked
                            // unless it was explicitly checked and there are no children (edge case, typically means "select all of nothing")
                            // For consistency, if no leaf nodes, "select-all" is unchecked.
                            selectAllServerNode.Checked = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckBoxListSearch] Error in DataBind ({this.ID}): {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
            finally
            {
                UpdateTitleAndIcon();
                UpdateDeselectAllButtonState();
            }
        }


        // Helper method to get property value using reflection
        private string GetPropertyValue(object item, string propertyName)
        {
            if (item == null || string.IsNullOrEmpty(propertyName)) return null;
            try
            {
                var prop = item.GetType().GetProperty(propertyName);
                if (prop == null) 
                    return null;
                
                return prop.GetValue(item)?.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error de reflexión obteniendo propiedad '{propertyName}' de {item.GetType().Name}: {ex.Message}");
                System.Diagnostics.Trace.TraceWarning($"Reflection error getting property '{propertyName}' from {item.GetType().Name}: {ex.Message}");
                return null;
            }
        }

        public ListItemCollection Items
        {
            get
            {
                var listItems = new ListItemCollection();
                foreach (TreeNode node in chkList.Nodes.Cast<TreeNode>().Skip(1)) // Skip "Seleccionar todos"
                {
                    listItems.Add(new ListItem(node.Text, node.Value)
                    {
                        Selected = node.Checked
                    });
                }
                return listItems;
            }
        }

        public List<string> SelectedValues
        {
            get
            {
                var values = new List<string>();
                if (chkList != null)
                {
                    int nodesProcessed = 0;
                    int checkedNodesFound = 0;
                    // Usar GetAllNodes para asegurar que se revisen todos los nodos.
                    foreach (TreeNode node in chkList.Nodes.Cast<TreeNode>().SelectMany(GetAllNodes))
                    {
                        nodesProcessed++;
                        if (node.Checked) // Primero verificar si está marcado
                        {
                            if (node.Value != "select-all") // Luego aplicar la condición de no ser "select-all"
                            {
                                values.Add(node.Value);
                                checkedNodesFound++;
                            }
                        }
                    }
                }
                return values;
            }
        }

        // Método auxiliar para actualizar el título del botón dropdown
        private void UpdateTitleAndIcon()
        {
            if (litTitle == null) return;

            // Contar nodos HOJA marcados. El nodo "select-all" se maneja de forma especial.
            int selectedLeafNodesCount = 0;
            bool selectAllIsEffectivelyChecked = false;

            if (chkList.Nodes.Count > 0 && chkList.Nodes[0].Value == "select-all" && chkList.Nodes[0].Checked)
            {
                // Si "select-all" está marcado y no hay otros nodos hijos (o todos están marcados),
                // se considera como una selección completa.
                if (chkList.Nodes[0].ChildNodes.Count == 0)
                {
                    selectAllIsEffectivelyChecked = true;
                }
                else
                {
                    // Si "select-all" está marcado, contamos todos los nodos hoja como seleccionados.
                    // Esto puede necesitar ajuste si "select-all" puede estar checked pero no todos los hijos.
                    // Por ahora, si "select-all" está checked, el ícono será "fill".
                    selectAllIsEffectivelyChecked = true;
                }
            }

            if (selectAllIsEffectivelyChecked)
            {
                selectedLeafNodesCount = 1; // Para que el ícono cambie a "fill"
            }
            else
            {
                selectedLeafNodesCount = chkList.Nodes
                                            .Cast<TreeNode>()
                                            .SelectMany(GetAllNodes) // Obtener todos los nodos
                                            .Count(n => n.Checked && n.ChildNodes.Count == 0 && n.Value != "select-all");
            }


            string iconClass = (selectedLeafNodesCount == 0) ? "bi-caret-down" : "bi-caret-down-fill";

            string baseText = litTitle.Attributes["data-default-text"];
            if (string.IsNullOrEmpty(baseText))
            {
                baseText = HeaderText ?? "Filtro";
            }

            litTitle.InnerHtml = $"{baseText} <i class=\"bi {iconClass}\"></i>";
        }

        public void ClearSelection()
        {
            foreach (TreeNode node in chkList.Nodes.Cast<TreeNode>().SelectMany(GetAllNodes))
            {
                node.Checked = false;
            }

            UpdateTitleAndIcon();
            UpdateDeselectAllButtonState();
        }

        private void ClearAllNodesRecursive(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes.Cast<TreeNode>())
            {
                // Desmarcar el nodo actual.
                node.Checked = false;
                // Si tiene hijos, llamar recursivamente.
                if (node.ChildNodes.Count > 0)
                {
                    ClearAllNodesRecursive(node.ChildNodes);
                }
            }
        }

        private void UpdateDeselectAllButtonState()
        {
            // Usamos GetAllNodes para obtener todos los nodos y luego filtramos los hoja.
            bool hasSelectedLeafItems = chkList.Nodes
                .Cast<TreeNode>()
                .Skip(1) // Ignorar "Seleccionar todos"
                .SelectMany(GetAllNodes) // Obtener todos los nodos descendientes (y el propio nodo)
                .Where(node => node.ChildNodes.Count == 0) // Filtrar solo nodos hoja
                .Any(node => node.Checked); // Verificar si alguno está marcado

            chkList_btnDeselectAll.Enabled = hasSelectedLeafItems;
            chkList_btnDeselectAll.Visible = hasSelectedLeafItems; // Ocultar/mostrar según si hay algo seleccionado
        }

        // Obtiene todos los nodos descendientes y el propio nodo.
        private IEnumerable<TreeNode> GetAllNodes(TreeNode node)
        {
            yield return node; // Devolver el nodo actual
            foreach (TreeNode childNode in node.ChildNodes.Cast<TreeNode>())
            {
                // Devolver todos los nodos del hijo recursivamente
                foreach (var descendantNode in GetAllNodes(childNode))
                {
                    yield return descendantNode;
                }
            }
        }

        protected void BtnDeselectAll_Click(object sender, EventArgs e)
        {
            ClearSelection();
        }
    }
}

