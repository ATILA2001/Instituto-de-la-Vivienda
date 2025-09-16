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
    public partial class TreeViewSearch : UserControl
    {
        public event EventHandler AcceptChanges;

        protected TreeView chkList;
        protected HtmlButton chkList_btnAccept;
        protected LinkButton chkList_btnDeselectAll;
        protected HtmlGenericControl litTitle;
        protected HtmlButton dropdownButton;


        private string _dataTextField;
        private string _dataValueField;
        private object _dataSource;
        private static Dictionary<string, bool> _pageFiltersApplied = new Dictionary<string, bool>();




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
            string pageName = Page?.GetType().Name ?? "Unknown";
            return $"TreeViewSearch_SelectedValues_{pageName}_{this.ID}";
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
                string pageName = Page?.GetType().Name ?? "Unknown";
                if (_pageFiltersApplied.ContainsKey(pageName))
                {
                    _pageFiltersApplied.Remove(pageName);
                }
                if (HasStoredFilters())
                {
                    Page.LoadComplete += ApplyStoredFiltersOnLoadComplete;
                }

            }
        }
        private void ApplyStoredFiltersOnLoadComplete(object sender, EventArgs e)
        {
            string pageName = Page?.GetType().Name ?? "Unknown";

            if (!_pageFiltersApplied.ContainsKey(pageName))
            {
                _pageFiltersApplied[pageName] = true;

                // Remover el handler para evitar múltiples ejecuciones
                Page.LoadComplete -= ApplyStoredFiltersOnLoadComplete;

                // Aplicar los filtros
                AcceptChanges?.Invoke(this, EventArgs.Empty);
            }
        }

        protected void Page_PreRenderComplete(object sender, EventArgs e)
        {
            string pageName = Page?.GetType().Name ?? "Unknown";

            // Solo aplicar una vez por página y solo en primera carga
            if (!Page.IsPostBack && !_pageFiltersApplied.ContainsKey(pageName))
            {
                _pageFiltersApplied[pageName] = true;

                // Verificar si este control tiene filtros guardados
                if (HasStoredFilters())
                {
                    // Ejecutar directamente el click del botón para aplicar filtros
                    BtnAccept_Click(this, EventArgs.Empty);
                }
            }
        }


        protected void BtnAccept_Click(object sender, EventArgs e)
        {
            List<string> currentSelectedValues = this.SelectedValues;

            string pageName = Page?.GetType().Name ?? "Unknown";
            string contextKey = $"TreeViewSearch_{pageName}_{this.ID}_ContextSelectedValues";
            HttpContext.Current.Items[contextKey] = currentSelectedValues;

            SaveSelectedValuesToSession(currentSelectedValues);

            AcceptChanges?.Invoke(this, EventArgs.Empty);

        }

        public override void DataBind()
        {
            // Paso 1: Guardar selecciones actuales antes de repoblar
            var seleccionesActuales = GuardarSeleccionesPorValor();

            try
            {
                if (chkList != null)
                {
                    // Paso 2: Limpiar y repoblar con la lógica original completa
                    chkList.Nodes.Clear();
                    var selectAllNode = new TreeNode("Seleccionar todos", "select-all")
                    {
                        ShowCheckBox = true,
                        SelectAction = TreeNodeSelectAction.None
                    };
                    chkList.Nodes.Add(selectAllNode);

                    if (_dataSource != null)
                    {
                        // Caso 1: DataSource es una colección de DateTime (filtro "Mes Certificado")
                        if (_dataSource is IEnumerable<DateTime> dates)
                        {
                            PopulateDateNodes(dates.Distinct().ToList(), selectAllNode);
                        }
                        // Caso 2: DataSource es una lista plana de strings (filtro "Estado")
                        else if (_dataSource is IEnumerable<string> stringList)
                        {
                            foreach (var item in stringList.Distinct().OrderBy(s => s))
                            {
                                var node = new TreeNode(item, item)
                                {
                                    ShowCheckBox = true,
                                    SelectAction = TreeNodeSelectAction.None
                                };
                                selectAllNode.ChildNodes.Add(node);
                            }
                        }
                        // Caso 3: DataSource es una colección de objetos complejos (caso general)
                        else
                        {
                            if (_dataSource is DataTable dtSource)
                            {
                                _dataSource = dtSource.AsEnumerable();
                            }

                            if (_dataSource is IEnumerable<object> enumerableDataSource)
                            {
                                // Agrupar elementos por DataTextField
                                var groupedItems = enumerableDataSource
                                    .GroupBy(item => GetPropertyValue(item, DataTextField))
                                    .OrderBy(g => string.IsNullOrEmpty(g.Key) ? 0 : 1)
                                    .ThenBy(g => g.Key);

                                // Iterar sobre los grupos
                                foreach (var group in groupedItems)
                                {
                                    var itemsInGroup = group.ToList();

                                    // Manejar valores vacíos con "(Vacíos)"
                                    var text = string.IsNullOrEmpty(group.Key) ? "(Vacíos)" : group.Key;

                                    // Recolectar todos los valores distintos del grupo y concatenarlos por comas.
                                    var distinctValues = itemsInGroup
                                        .Select(item => GetPropertyValue(item, DataValueField))
                                        .Where(v => !string.IsNullOrEmpty(v))
                                        .Distinct()
                                        .ToList();

                                    string value;
                                    if (!distinctValues.Any())
                                    {
                                        value = string.Empty;
                                    }
                                    else
                                    {
                                        value = string.Join(",", distinctValues);
                                    }

                                    var node = new TreeNode(text, value)
                                    {
                                        ShowCheckBox = true,
                                        SelectAction = TreeNodeSelectAction.None
                                    };
                                    selectAllNode.ChildNodes.Add(node);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error en DataBind del TreeViewSearch: " + ex.Message);
            }

            // Prioriza selecciones guardadas antes del repoblado
            if (seleccionesActuales != null && seleccionesActuales.Any())
            {
                // Restaura selecciones preservadas del repoblado
                RestaurarSeleccionesPorValor(seleccionesActuales);
                // Asegurar que el nodo "select-all" se sincronice con los hijos tras restaurar
                SyncSelectAllNodeState();
            }
            else
            {

                try
                {
                    var selectedValues = LoadSelectedValuesFromSession();
                    if (selectedValues.Any())
                    {
                        var savedValuesSet = new HashSet<string>(selectedValues);
                        foreach (var node in chkList.Nodes.Cast<TreeNode>().SelectMany(GetAllNodes))
                        {
                            var nodeValue = node.Value ?? string.Empty;
                            if (string.IsNullOrEmpty(nodeValue)) continue;

                            var idsInNode = nodeValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());

                            if (idsInNode.Any(id => savedValuesSet.Contains(id)) || savedValuesSet.Contains(nodeValue))
                            {
                                node.Checked = true;

                                // Expandir nodos padre
                                var parent = node.Parent;
                                while (parent != null)
                                {
                                    parent.Expanded = true;
                                    parent = parent.Parent;
                                }
                            }
                        }
                        // Después de marcar por session, sincronizar select-all
                        SyncSelectAllNodeState();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error restaurando el estado del TreeViewSearch: " + ex.Message);
                }
            }

            // Paso 4: Actualizar UI para reflejar el estado final
            try
            {
                UpdateTitleAndIcon();
                UpdateDeselectAllButtonState();

                if (chkList.Nodes.Count > 0)
                {
                    chkList.Nodes[0].Expanded = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error actualizando interfaz: " + ex.Message);
            }
        }

        /// <summary>
        /// Guarda las selecciones actuales usando el valor del nodo, no su posición
        /// </summary>
        private HashSet<string> GuardarSeleccionesPorValor()
        {
            var selecciones = new HashSet<string>();

            try
            {
                if (chkList?.Nodes != null)
                {
                    // Obtener todos los nodos seleccionados y guardar sus valores
                    var todosLosNodos = chkList.Nodes.Cast<TreeNode>()
                        .SelectMany(GetAllNodesRecursive)
                        .Where(n => n.Checked)
                        .Select(n => n.Value)
                        .ToList();

                    foreach (var valor in todosLosNodos)
                    {
                        selecciones.Add(valor);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error guardando selecciones por valor: {ex.Message}");
            }

            return selecciones;
        }

        /// <summary>
        /// Restaura selecciones basándose en valores, independientemente del orden actual
        /// </summary>
        private void RestaurarSeleccionesPorValor(HashSet<string> valoresSeleccionados)
        {
            if (valoresSeleccionados == null || !valoresSeleccionados.Any())
                return;

            try
            {
                if (chkList?.Nodes != null)
                {
                    // Obtener todos los nodos después del repoblado
                    var todosLosNodosActuales = chkList.Nodes.Cast<TreeNode>()
                        .SelectMany(GetAllNodesRecursive)
                        .ToList();

                    // Marcar como seleccionados los nodos cuyos valores contengan alguno de los ids guardados
                    // o, alternativamente, cuando el valor guardado sea el conjunto concatenado que coincide con el nodo.
                    var valoresBuscados = new HashSet<string>(valoresSeleccionados);

                    foreach (var nodo in todosLosNodosActuales)
                    {
                        var nodeValue = nodo.Value ?? string.Empty;

                        // Si el valor del nodo está vacío, ignorar
                        if (string.IsNullOrEmpty(nodeValue)) continue;

                        // Separar los ids del nodo (si vienen concatenados por comas)
                        var idsEnNodo = nodeValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());

                        // Si alguno de los ids del nodo está en los valores guardados, marcar el nodo
                        if (idsEnNodo.Any(id => valoresBuscados.Contains(id)))
                        {
                            nodo.Checked = true;

                            // Expandir nodos padre para hacer visible la selección
                            var padre = nodo.Parent;
                            while (padre != null)
                            {
                                padre.Expanded = true;
                                padre = padre.Parent;
                            }
                        }
                        else
                        {
                            // También soportar el caso donde el valor guardado es exactamente el conjunto concatenado
                            if (valoresBuscados.Contains(nodeValue))
                            {
                                nodo.Checked = true;
                                var padre = nodo.Parent;
                                while (padre != null)
                                {
                                    padre.Expanded = true;
                                    padre = padre.Parent;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restaurando selecciones por valor: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene todos los nodos de forma recursiva
        /// </summary>
        private IEnumerable<TreeNode> GetAllNodesRecursive(TreeNode nodoRaiz)
        {
            yield return nodoRaiz;

            foreach (TreeNode nodoHijo in nodoRaiz.ChildNodes)
            {
                foreach (TreeNode nodoNieto in GetAllNodesRecursive(nodoHijo))
                {
                    yield return nodoNieto;
                }
            }
        }

        /// <summary>
        /// Expande todos los nodos padre de un nodo dado
        /// </summary>
        private void ExpandirNodosPadre(TreeNode nodo)
        {
            var padre = nodo.Parent;
            while (padre != null)
            {
                padre.Expanded = true;
                padre = padre.Parent;
            }
        }

        /// <summary>
        /// Actualiza la interfaz después de restaurar selecciones
        /// </summary>
        private void ActualizarInterfazDespuesDeRestaurar()
        {
            try
            {
                // Actualizar título y contador
                UpdateTitleAndIcon();

                // Actualizar estado del botón "Deseleccionar todo"
                UpdateDeselectAllButtonState();

                // Expandir nodo raíz si tiene contenido
                if (chkList.Nodes.Count > 0)
                {
                    chkList.Nodes[0].Expanded = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error actualizando interfaz: {ex.Message}");
            }
        }

        /// <summary>
        /// Llena el TreeView con nodos de fecha agrupados por año y mes, manteniendo la jerarquía Año -> Mes -> Día.
        /// Este método mejora la claridad al aislar la lógica de manejo de fechas.
        /// </summary>
        /// <param name="dates">Lista de fechas únicas para mostrar.</param>
        /// <param name="parentNode">El nodo "Seleccionar Todos" al que se agregarán los años.</param>
        private void PopulateDateNodes(List<DateTime> dates, TreeNode parentNode)
        {
            var yearGroups = dates.GroupBy(d => d.Year).OrderByDescending(g => g.Key);
            foreach (var yearGroup in yearGroups)
            {
                var yearNode = new TreeNode($"{yearGroup.Key}", $"year_{yearGroup.Key}")
                { ShowCheckBox = true, SelectAction = TreeNodeSelectAction.None, Expanded = false };

                var monthGroups = yearGroup.GroupBy(d => d.Month).OrderBy(g => g.Key);
                foreach (var monthGroup in monthGroups)
                {
                    var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthGroup.Key);
                    var monthNode = new TreeNode(monthName, $"month_{yearGroup.Key}_{monthGroup.Key}")
                    { ShowCheckBox = true, SelectAction = TreeNodeSelectAction.None, Expanded = false };

                    // Se añaden los días como nodos finales.
                    foreach (var day in monthGroup.OrderBy(d => d.Day))
                    {
                        // El valor del nodo se guarda en un formato estándar para facilitar el parseo posterior.
                        monthNode.ChildNodes.Add(new TreeNode(day.ToString("dd"), day.ToString("yyyy-MM-dd"))
                        { ShowCheckBox = true, SelectAction = TreeNodeSelectAction.None });
                    }

                    if (monthNode.ChildNodes.Count > 0)
                    {
                        yearNode.ChildNodes.Add(monthNode);
                    }
                }

                if (yearNode.ChildNodes.Count > 0)
                {
                    parentNode.ChildNodes.Add(yearNode);
                }
            }
        }


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

        public List<string> ExpandedSelectedValues
        {
            get
            {
                var selectedValues = this.SelectedValues;
                if (!selectedValues.Any())
                {
                    return selectedValues;
                }

                var expandedList = new HashSet<string>();

                // Obtiene todos los nodos hoja que han sido seleccionados.
                var selectedNodes = chkList.Nodes
                    .Cast<TreeNode>()
                    .SelectMany(GetAllNodes) // Usa el helper para aplanar el árbol.
                    .Where(n => n.Checked && n.ChildNodes.Count == 0 && n.Value != "select-all")
                    .ToList();

                foreach (var node in selectedNodes)
                {
                    // Separa el string de la propiedad 'Value' para obtener todos los IDs individuales.
                    var idsFromValue = node.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var id in idsFromValue)
                    {
                        expandedList.Add(id);
                    }
                }

                return expandedList.ToList();
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
            try
            {
                // Limpiar checkboxes del TreeView
                ClearAllNodesRecursive(chkList.Nodes);

                // Limpiar estado de sesión
                ClearSessionState();

                // Limpiar estado de contexto
                ClearContextState();

                // Actualizar UI
                UpdateTitleAndIcon();
                UpdateDeselectAllButtonState();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ClearSelection: {ex.ToString()}");
            }
        }

        private void ClearSessionState()
        {
            string sessionKey = GetSessionKeyForSelectedValues();
            if (HttpContext.Current.Session[sessionKey] != null)
            {
                HttpContext.Current.Session.Remove(sessionKey);
            }
        }

        private void ClearContextState()
        {
            string pageName = Page?.GetType().Name ?? "Unknown";
            string contextKey = $"TreeViewSearch_{pageName}_{this.ID}_ContextSelectedValues";
            if (HttpContext.Current.Items.Contains(contextKey))
            {
                HttpContext.Current.Items.Remove(contextKey);
            }
        }

        // Método estático para limpiar todos los filtros de una página
        public static void ClearAllFiltersOnPage(Page page)
        {
            try
            {
                if (page == null) return;

                // 1. Encuentra todos los controles TreeViewSearch en la página
                List<TreeViewSearch> allTvsControls = new List<TreeViewSearch>();
                FindAllTvsControlsRecursive(page, allTvsControls); // Usaremos un nuevo helper estático

                // 2. Llama a ClearSelection en cada instancia de control.
                foreach (TreeViewSearch tvsControl in allTvsControls)
                {
                    tvsControl.ClearSelection();
                }

                // 3. Establece un flag del lado del cliente para que JavaScript realice sus actualizaciones de UI (por ejemplo, títulos de dropdown).
                // El JavaScript escuchará este flag y llamará a clearLocalStatesForTreeView.
                ScriptManager.RegisterStartupScript(page, page.GetType(), "setFiltersCleared",
                    "sessionStorage.setItem('filtersCleared', 'true'); console.log('[TreeViewSearch.ClearAllFiltersOnPage] filtersCleared flag set for client-side refresh.');", true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en TreeViewSearch.ClearAllFiltersOnPage: {ex.ToString()}");
                // Considerar un logging más formal para producción.
            }
        }

        // Auxiliar estático para encontrar recursivamente todos los controles TreeViewSearch en un control padre.
        private static void FindAllTvsControlsRecursive(Control parent, List<TreeViewSearch> foundControls)
        {
            foreach (Control ctl in parent.Controls)
            {
                if (ctl is TreeViewSearch tvs)
                {
                    foundControls.Add(tvs);
                }
                if (ctl.HasControls())
                {
                    FindAllTvsControlsRecursive(ctl, foundControls);
                }
            }
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

        private List<TreeViewSearch> FindAllTreeViewSearchControls(Control parent)
        {
            var controls = new List<TreeViewSearch>();

            foreach (Control control in parent.Controls)
            {
                if (control is TreeViewSearch treeViewSearch)
                {
                    controls.Add(treeViewSearch);
                }
                else
                {
                    controls.AddRange(FindAllTreeViewSearchControls(control));
                }
            }

            return controls;
        }

        public bool HasStoredFilters()
        {
            var savedValues = LoadSelectedValuesFromSession();
            return savedValues != null && savedValues.Any();
        }

        /// <summary>
        /// Sincroniza el estado del nodo "select-all" en el servidor según los hijos marcados.
        /// Marca el select-all si todas las hojas están marcadas.
        /// </summary>
        private void SyncSelectAllNodeState()
        {
            try
            {
                if (chkList == null || chkList.Nodes.Count == 0) return;

                var root = chkList.Nodes[0];
                if (root == null || root.Value != "select-all") return;

                // Obtener todos los nodos hoja (excluyendo el propio select-all)
                var leafNodes = chkList.Nodes
                    .Cast<TreeNode>()
                    .Skip(1)
                    .SelectMany(GetAllNodes)
                    .Where(n => n.ChildNodes.Count == 0)
                    .ToList();

                if (!leafNodes.Any())
                {
                    root.Checked = false;
                    return;
                }

                bool allChecked = leafNodes.All(n => n.Checked);
                root.Checked = allChecked;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error synchronizing select-all state: {ex.Message}");
            }
        }


    }
}