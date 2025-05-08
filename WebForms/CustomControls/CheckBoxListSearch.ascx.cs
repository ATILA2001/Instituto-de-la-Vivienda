using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms.CustomControls
{
    public partial class CheckBoxListSearch : UserControl
    {
        //public event EventHandler SelectedIndexChanged;
        // Modificar la definición del evento para coincidir con TreeView
        public event EventHandler<TreeNodeEventArgs> SelectedIndexChanged;
        public event EventHandler AcceptChanges;


        //protected CheckBoxList chkList;
        protected TreeView chkList;
        protected Button btnDeselectAll;
        protected Literal litTitle;

        private string _dataTextField;
        private string _dataValueField;

        // Clave para guardar los valores seleccionados en ViewState
        private const string SelectedValuesViewStateKey = "SelectedValuesList";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Es importante limpiar el ViewState si no es PostBack
                // para evitar mantener selecciones antiguas si la página se recarga completamente.
                ViewState.Remove(SelectedValuesViewStateKey);


                UpdateDeselectAllButtonState();
            }
            chkList.Attributes["data-title-id"] = litTitle.ClientID;
        }

        protected void BtnAccept_Click(object sender, EventArgs e)
        {
            // --- Modificación: Capturar y guardar valores en ViewState ---
            var checkedLeafValues = chkList.CheckedNodes
                .Cast<TreeNode>()
                .Where(node => node.ChildNodes.Count == 0) // Asegurarse de que son nodos hoja
                .Select(node => node.Value)
                .Distinct()
                .ToList();

            ViewState[SelectedValuesViewStateKey] = checkedLeafValues; // Guardar en ViewState

            System.Diagnostics.Debug.WriteLine($"[CheckBoxListSearch] BtnAccept_Click: Guardados {checkedLeafValues.Count} valores en ViewState: {string.Join(", ", checkedLeafValues)}");
            // --- Fin Modificación ---

            // Notificar que se aceptaron los cambios
            AcceptChanges?.Invoke(this, EventArgs.Empty);

            // Actualizar el estado del botón deseleccionar y el título
            UpdateDeselectAllButtonState();
            UpdateTitle(); // Asegura de que el título se actualice


        }

        public ListItem SelectedItem
        {
            get
            {
                var node = chkList.Nodes.Cast<TreeNode>()
                    .Skip(1)
                    .FirstOrDefault(n => n.Checked);
                return node != null ? new ListItem(node.Text, node.Value) { Selected = true } : null;
            }
            set
            {
                if (value != null)
                {
                    ClearSelection();
                    var node = chkList.Nodes.Cast<TreeNode>()
                        .FirstOrDefault(n => n.Value == value.Value);
                    if (node != null)
                    {
                        node.Checked = true;
                    }
                }
            }
        }

        private object _dataSource;
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

        public override void DataBind()
        {
            try
            {
                chkList.Nodes.Clear(); // Limpiar nodos existentes

                // Añadir nodo "Seleccionar todos"
                var selectAllNode = new TreeNode("Seleccionar todos", "select-all")
                {
                    SelectAction = TreeNodeSelectAction.None,
                    ShowCheckBox = true,
                    Expanded = true // Mantener expandido por defecto
                };
                chkList.Nodes.Add(selectAllNode);

                // Si no hay DataSource, actualizar estado visual y salir
                if (_dataSource == null)
                {
                    Debug.WriteLine("[CheckBoxListSearch] DataBind: DataSource es null.");
                    base.DataBind(); // Llamar al método base
                    // Modificación: Llamar a UpdateTitle y UpdateDeselectAllButtonState
                    // para asegurar que el estado visual sea correcto incluso sin datos.
                    UpdateTitle();
                    UpdateDeselectAllButtonState();
                    return;
                }

                var dates = new List<DateTime>();
                bool areDates = true; // Asumir que son fechas inicialmente

                // --- Paso 1: Validar si TODOS los items pueden ser parseados como DateTime ---
                // (La lógica de validación existente permanece sin cambios aquí)
                var itemsToCheck = new List<object>();
                if (_dataSource is DataTable dtCheck && dtCheck.Columns.Contains(DataTextField))
                {
                    itemsToCheck = dtCheck.AsEnumerable().Select(row => row[DataTextField]).ToList();
                }
                else if (_dataSource is IEnumerable<object> enumerableCheck)
                {
                    itemsToCheck = enumerableCheck.Select(item => GetPropertyValue(item, DataTextField)).Where(val => val != null).Cast<object>().ToList();
                }
                else if (_dataSource is System.Collections.IEnumerable genericEnumerableCheck)
                {
                    itemsToCheck = genericEnumerableCheck.Cast<object>().Select(item => GetPropertyValue(item, DataTextField)).Where(val => val != null).Cast<object>().ToList();
                }

                if (!itemsToCheck.Any())
                {
                    areDates = false;
                }
                else
                {
                    foreach (var itemValue in itemsToCheck)
                    {
                        if (itemValue == null || !DateTime.TryParse(itemValue.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime parsedDate))
                        {
                            areDates = false;
                            Debug.WriteLine($"[CheckBoxListSearch] DataBind: Item '{itemValue}' no es una fecha válida. Tratando como lista plana.");
                            break;
                        }
                        if (areDates)
                        {
                            dates.Add(parsedDate);
                        }
                    }
                }
                Debug.WriteLine($"[CheckBoxListSearch] DataBind: ¿Son todos los items fechas válidas? {areDates}");
                // --- Fin Paso 1 ---


                // --- Paso 2: Construir Jerarquía de Fechas o Lista Plana basado en la validación ---
                if (areDates)
                {
                    // --- Lógica existente para construir la jerarquía Año/Mes/Día ---
                    // (Esta sección permanece sin cambios)
                    Debug.WriteLine("[CheckBoxListSearch] DataBind: Construyendo jerarquía de fechas...");
                    var yearGroups = dates.GroupBy(d => d.Year)
                                        .OrderByDescending(g => g.Key);

                    foreach (var yearGroup in yearGroups)
                    {
                        var yearNode = new TreeNode($"{yearGroup.Key}", $"year_{yearGroup.Key}")
                        {
                            SelectAction = TreeNodeSelectAction.None,
                            ShowCheckBox = true,
                            Expanded = false
                        };

                        var monthGroups = yearGroup.GroupBy(d => d.Month)
                                                .OrderBy(g => g.Key);

                        foreach (var monthGroup in monthGroups)
                        {
                            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthGroup.Key);
                            monthName = char.ToUpper(monthName[0]) + monthName.Substring(1);

                            var monthNode = new TreeNode(monthName, $"{yearGroup.Key}_{monthGroup.Key}")
                            {
                                SelectAction = TreeNodeSelectAction.None,
                                ShowCheckBox = true,
                                Expanded = false
                            };

                            var daysInMonthList = monthGroup.OrderBy(d => d.Day).Distinct().ToList();

                            if (daysInMonthList.Any()) // Check if there are days before iterating
                            {
                                foreach (var date in daysInMonthList)
                                {
                                    var dayNode = new TreeNode(date.Day.ToString("00"), date.ToString("yyyy-MM-dd"))
                                    {
                                        SelectAction = TreeNodeSelectAction.None,
                                        ShowCheckBox = true
                                    };
                                    monthNode.ChildNodes.Add(dayNode);
                                }
                                yearNode.ChildNodes.Add(monthNode); // Add month node only if it has days
                            }
                        }
                        if (yearNode.ChildNodes.Count > 0) // Add year node only if it has months
                        {
                            selectAllNode.ChildNodes.Add(yearNode);
                        }
                    }
                    // --- Fin lógica de jerarquía ---
                }
                else // areDates es false, construir lista plana
                {
                    // --- Lógica existente para construir una lista plana ---
                    // (Esta sección permanece sin cambios)
                    Debug.WriteLine("[CheckBoxListSearch] DataBind: Construyendo lista plana...");
                    if (_dataSource is DataTable dtFlat && dtFlat.Columns.Contains(DataTextField))
                    {
                        foreach (DataRow row in dtFlat.Rows)
                        {
                            var text = row[DataTextField]?.ToString() ?? string.Empty;
                            var value = row[DataValueField]?.ToString() ?? text;
                            var node = new TreeNode(text, value)
                            {
                                SelectAction = TreeNodeSelectAction.None,
                                ShowCheckBox = true
                            };
                            selectAllNode.ChildNodes.Add(node);
                        }
                    }
                    else if (_dataSource is IEnumerable<object> itemsFlat)
                    {
                        foreach (var item in itemsFlat)
                        {
                            var text = GetPropertyValue(item, DataTextField) ?? string.Empty;
                            var value = GetPropertyValue(item, DataValueField) ?? text;
                            var node = new TreeNode(text, value)
                            {
                                SelectAction = TreeNodeSelectAction.None,
                                ShowCheckBox = true
                            };
                            selectAllNode.ChildNodes.Add(node);
                        }
                    }
                    else if (_dataSource is System.Collections.IEnumerable genericEnumerableFlat)
                    {
                        foreach (object item in genericEnumerableFlat)
                        {
                            var text = GetPropertyValue(item, DataTextField) ?? string.Empty;
                            var value = GetPropertyValue(item, DataValueField) ?? text;
                            var node = new TreeNode(text, value)
                            {
                                SelectAction = TreeNodeSelectAction.None,
                                ShowCheckBox = true
                            };
                            selectAllNode.ChildNodes.Add(node);
                        }
                    }
                    // --- Fin lógica de lista plana ---
                }
                // --- Fin Paso 2 ---

                // Modificación: Se eliminó la llamada a RestoreCheckedState().
                // El estado 'checked' se preserva por el ViewState del TreeView.
                // El estado 'indeterminate' y la actualización de padres/título se manejan en JS.

                // Actualizar Título y Estado del Botón Deseleccionar Todos
                // Se llaman después de poblar los nodos para reflejar el estado inicial.
                UpdateTitle();
                UpdateDeselectAllButtonState();

                // Llamar al DataBind base después de poblar los nodos
                base.DataBind();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CheckBoxListSearch] Error en DataBind: {ex.Message}\nStackTrace: {ex.StackTrace}");
                chkList.Nodes.Clear();
                // Considerar añadir un nodo indicando el error
                // throw; // Relanzar si es necesario
            }
            finally
            {
                Debug.WriteLine($"--- DataBind Finalizado para {this.ID} ---");
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
                {
                    Debug.WriteLine($"Advertencia: Propiedad '{propertyName}' no encontrada en tipo '{item.GetType().Name}'.");
                    return null;
                }
                return prop.GetValue(item)?.ToString();
            }
            catch (Exception ex)
            {
                // Log reflection errors if necessary
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
                // --- Modificación: Leer desde ViewState ---
                System.Diagnostics.Debug.WriteLine("[CheckBoxListSearch] Accediendo a SelectedValues (leyendo desde ViewState)...");
                if (ViewState[SelectedValuesViewStateKey] is List<string> selectedValues)
                {
                    System.Diagnostics.Debug.WriteLine($"[CheckBoxListSearch] SelectedValues: Encontrados {selectedValues.Count} valores en ViewState: {string.Join(", ", selectedValues)}");
                    return selectedValues;
                }
                System.Diagnostics.Debug.WriteLine("[CheckBoxListSearch] SelectedValues: No se encontraron valores en ViewState.");
                return new List<string>(); // Devolver lista vacía si no hay nada en ViewState
                // --- Fin Modificación ---
            }
        }

        // Método auxiliar para actualizar el título del botón dropdown
        private void UpdateTitle()
        {
            var selectedLeafNodesText = chkList.CheckedNodes
                                        .Cast<TreeNode>()
                                        .Where(n => n.ChildNodes.Count == 0) // Solo hojas
                                        .Select(n => n.Text)
                                        .ToList();

            if (selectedLeafNodesText.Count == 0)
            {
                litTitle.Text = "Todos <i class=\"bi bi-caret-down\"></i>";
            }
            else if (selectedLeafNodesText.Count == 1)
            {
                litTitle.Text = $"{selectedLeafNodesText[0]} <i class=\"bi bi-caret-down-fill\"></i>";
            }
            else
            {
                litTitle.Text = $"{selectedLeafNodesText.Count} Seleccionados <i class=\"bi bi-caret-down-fill\"></i>";
            }
            // Forzar la actualización del título en el cliente si es necesario (puede no ser requerido si el postback ya lo hace)
            // ScriptManager.RegisterStartupScript(this, GetType(), $"UpdateTitle_{this.ClientID}", $"updateDropdownTitle('{chkList.ClientID}_dropdown', '{litTitle.ClientID}');", true);
        }

        // Asegura que ClearSelection también limpie el ViewState
        public void ClearSelection()
        {
            foreach (TreeNode node in chkList.Nodes.Cast<TreeNode>().SelectMany(GetAllNodes))
            {
                node.Checked = false;
            }
            ViewState.Remove(SelectedValuesViewStateKey); // Limpiar ViewState
            UpdateTitle(); // Actualizar título a "Todos"
            UpdateDeselectAllButtonState(); // Actualizar estado del botón
            System.Diagnostics.Debug.WriteLine("[CheckBoxListSearch] ClearSelection: Selección y ViewState limpiados.");
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

            btnDeselectAll.Enabled = hasSelectedLeafItems;
            btnDeselectAll.Visible = hasSelectedLeafItems; // Ocultar/mostrar según si hay algo seleccionado
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

