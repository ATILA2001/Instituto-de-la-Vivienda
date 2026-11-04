using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebForms.CustomControls;

namespace WebForms.Infraestructura
{
    /// <summary>
    /// Mecánica común de las páginas de grilla (Obras, Autorizantes, Certificados, Legítimos, Redeterminaciones,
    /// Formulación): snapshot completo en sesión, filtro de texto, filtros de cabecera (TreeViewSearch),
    /// paginación externa (PaginationControl) con subtotal, y exportación a Excel.
    ///
    /// La página concreta define qué datos carga, cómo se filtran y qué controles usa; esta clase se ocupa
    /// de la paginación y del rebind. Los manejadores de eventos de paginación y filtros tienen los nombres
    /// que ya usan los markups (OnAcceptChanges, paginationControl_PageChanged, btnFiltrar_Click, ...).
    /// </summary>
    /// <typeparam name="TDto">Fila de la grilla.</typeparam>
    public abstract class GridPageBase<TDto> : Page where TDto : class
    {
        // ------------------------------------------------------------------
        // Lo que define cada página
        // ------------------------------------------------------------------

        /// <summary>GridView principal.</summary>
        protected abstract GridView Grilla { get; }

        /// <summary>Caja de búsqueda de texto libre, o null si la página no tiene.</summary>
        protected abstract TextBox CajaBusqueda { get; }

        /// <summary>Control de paginación externo, o null si la página no tiene.</summary>
        protected abstract PaginationControl Paginador { get; }

        /// <summary>Clave de Session donde vive el snapshot completo (por ejemplo "ObrasCompleto").</summary>
        protected abstract string ClaveSesion { get; }

        /// <summary>Carga el snapshot completo desde la base, ya restringido a lo que el usuario puede ver.</summary>
        protected abstract List<TDto> CargarListaCompleta();

        /// <summary>Filtro de texto libre. Recibe el texto ya recortado y en mayúsculas.</summary>
        protected abstract bool CoincideTexto(TDto item, string filtroMayusculas);

        /// <summary>Aplica los filtros de cabecera (TreeViewSearch) sobre la lista. Por defecto no filtra.</summary>
        protected virtual List<TDto> AplicarFiltrosHeader(List<TDto> datos) => datos;

        /// <summary>Puebla los TreeViewSearch de la cabecera. Se llama después del DataBind, cuando HeaderRow existe.</summary>
        protected virtual void PoblarFiltrosHeader(List<TDto> listaCompleta) { }

        /// <summary>Subtotal que muestra el paginador para los datos filtrados. Por defecto 0.</summary>
        protected virtual decimal Subtotal(List<TDto> datosFiltrados) => 0m;

        protected virtual int TamanoPaginaInicial => 12;

        protected virtual string MensajeErrorCarga => "Error al cargar los datos. Intente nuevamente.";

        /// <summary>Valor de la DataKey "Id" en el DTO. Es la clave con la que se resuelve la fila editada.</summary>
        protected abstract int IdDe(TDto item);

        /// <summary>
        /// Nombre de la segunda DataKey para las filas virtuales (reliquidaciones, redeterminaciones), o null si
        /// la grilla no tiene. Debe figurar en DataKeyNames del markup y ser estable entre recargas del snapshot:
        /// un contador por carga no sirve, porque otra pestaña puede regenerar el snapshot y correr la numeración.
        /// </summary>
        protected virtual string NombreClaveVirtual => null;

        /// <summary>Valor de la clave virtual en el DTO; null o vacío si la fila es real.</summary>
        protected virtual string ClaveVirtualDe(TDto item) => null;

        // ------------------------------------------------------------------
        // Estado de paginación (ViewState)
        // ------------------------------------------------------------------

        protected int PaginaActual
        {
            get => (int)(ViewState["CurrentPageIndex"] ?? 0);
            set => ViewState["CurrentPageIndex"] = value;
        }

        protected int TamanoPagina
        {
            get => (int)(ViewState["PageSize"] ?? TamanoPaginaInicial);
            set => ViewState["PageSize"] = value;
        }

        // ------------------------------------------------------------------
        // Datos
        // ------------------------------------------------------------------

        /// <summary>Snapshot completo: lo toma de Session o lo carga y lo guarda.</summary>
        protected List<TDto> ListaCompleta
        {
            get
            {
                if (Session[ClaveSesion] is List<TDto> lista)
                    return lista;

                var cargada = CargarListaCompleta() ?? new List<TDto>();
                Session[ClaveSesion] = cargada;
                return cargada;
            }
        }

        /// <summary>Descarta el snapshot; la próxima lectura vuelve a la base.</summary>
        protected void InvalidarListaCompleta() => Session[ClaveSesion] = null;

        /// <summary>Snapshot con el filtro de texto y los filtros de cabecera aplicados.</summary>
        protected List<TDto> DatosFiltrados()
        {
            var datos = ListaCompleta;

            string filtro = CajaBusqueda?.Text?.Trim().ToUpperInvariant() ?? string.Empty;
            if (filtro.Length > 0)
                datos = datos.Where(d => CoincideTexto(d, filtro)).ToList();

            return AplicarFiltrosHeader(datos) ?? new List<TDto>();
        }

        /// <summary>
        /// DTO del snapshot que corresponde a una fila de la grilla, resuelto por las DataKeys de la fila
        /// (persistidas en ViewState): identifican lo que está renderizado aunque el usuario haya tocado la
        /// búsqueda o los filtros sin aplicarlos, o aunque otra pestaña haya cambiado el snapshot.
        /// No se usa la posición: con el snapshot refiltrado, el índice podía apuntar a otro registro.
        /// Devuelve null si la fila no tiene clave válida o el registro ya no está en el snapshot.
        /// </summary>
        protected TDto ItemDeFila(int indiceEnPagina)
        {
            var claves = Grilla.DataKeys;
            if (claves == null || indiceEnPagina < 0 || indiceEnPagina >= claves.Count)
                return null;

            var valores = claves[indiceEnPagina]?.Values;
            if (valores == null) return null;

            int id = ValorDeClave(valores, "Id");
            string claveVirtual = NombreClaveVirtual != null && valores.Contains(NombreClaveVirtual)
                ? valores[NombreClaveVirtual]?.ToString()
                : null;
            if (id <= 0 && string.IsNullOrEmpty(claveVirtual)) return null;

            var lista = ListaCompleta;
            return (id > 0 ? lista.FirstOrDefault(d => IdDe(d) == id) : null)
                ?? (!string.IsNullOrEmpty(claveVirtual)
                    ? lista.FirstOrDefault(d => string.Equals(ClaveVirtualDe(d), claveVirtual, StringComparison.Ordinal))
                    : null);
        }

        /// <summary>DTO de la fila que contiene al control (TextBox, DropDownList...) que disparó un evento inline.</summary>
        protected TDto ItemDelControlInline(object sender)
        {
            var fila = (sender as Control)?.NamingContainer as GridViewRow;
            return fila == null ? null : ItemDeFila(fila.RowIndex);
        }

        private static int ValorDeClave(System.Collections.Specialized.IOrderedDictionary valores, string nombre)
        {
            var valor = valores.Contains(nombre) ? valores[nombre] : null;
            return valor != null && int.TryParse(valor.ToString(), out int id) ? id : 0;
        }

        // ------------------------------------------------------------------
        // Rebind
        // ------------------------------------------------------------------

        /// <summary>Enlaza la página actual de los datos filtrados, puebla los filtros de cabecera y el paginador.</summary>
        protected void CargarPaginaActual()
        {
            try
            {
                var filtrados = DatosFiltrados();

                // Si la página quedó fuera de rango (por ejemplo, se borró la única fila de la última), volver a la última.
                int totalPaginas = (int)Math.Ceiling(filtrados.Count / (double)TamanoPagina);
                if (PaginaActual >= totalPaginas)
                    PaginaActual = Math.Max(0, totalPaginas - 1);

                Grilla.VirtualItemCount = filtrados.Count;
                Grilla.PageSize = TamanoPagina;
                Grilla.PageIndex = PaginaActual;
                Grilla.DataSource = filtrados.Skip(PaginaActual * TamanoPagina).Take(TamanoPagina).ToList();
                Grilla.DataBind();

                PoblarFiltrosHeader(ListaCompleta);

                if (Paginador != null)
                {
                    Paginador.TotalRecords = filtrados.Count;
                    Paginador.CurrentPageIndex = PaginaActual;
                    Paginador.PageSize = TamanoPagina;
                    Paginador.UpdatePaginationControls();
                    Paginador.UpdateSubtotal(Subtotal(filtrados), filtrados.Count);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[{0}] Error al cargar la grilla: {1}", GetType().Name, ex);
                ToastService.Show(Page, MensajeErrorCarga, ToastService.ToastType.Error);
            }
        }

        /// <summary>Vuelve a la primera página y rebindea. Para cambios de filtro.</summary>
        protected void RecargarDesdePrimeraPagina()
        {
            PaginaActual = 0;
            CargarPaginaActual();
        }

        /// <summary>Descarta el snapshot y rebindea en la misma página. Para altas, bajas y modificaciones.</summary>
        protected void RecargarTrasCambio()
        {
            InvalidarListaCompleta();
            CargarPaginaActual();
        }

        // ------------------------------------------------------------------
        // Manejadores que los markups ya referencian por nombre
        // ------------------------------------------------------------------

        protected void OnAcceptChanges(object sender, EventArgs e) => RecargarDesdePrimeraPagina();

        protected void paginationControl_PageChanged(object sender, PaginationEventArgs e)
        {
            PaginaActual = e.PageIndex;
            CargarPaginaActual();
        }

        protected void paginationControl_PageSizeChanged(object sender, PaginationEventArgs e)
        {
            TamanoPagina = e.PageSize;
            RecargarDesdePrimeraPagina();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e) => RecargarDesdePrimeraPagina();

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            if (CajaBusqueda != null)
                CajaBusqueda.Text = string.Empty;
            TreeViewSearch.ClearAllFiltersOnPage(Page);
            RecargarDesdePrimeraPagina();
        }

        // ------------------------------------------------------------------
        // Exportación
        // ------------------------------------------------------------------

        /// <summary>
        /// Descarga como Excel lo que la grilla muestra: el snapshot con el texto de búsqueda y los filtros de
        /// cabecera aplicados (todas las páginas), con las columnas del mapeo (título -> propiedad).
        /// </summary>
        protected void ExportarExcel(Dictionary<string, string> mapeoColumnas, string nombreArchivo)
        {
            try
            {
                var datos = DatosFiltrados();
                if (datos.Count == 0)
                {
                    ToastService.Show(Page, "No hay datos para exportar", ToastService.ToastType.Warning);
                    return;
                }

                DescargaArchivo.Excel(Response, Negocio.ExcelHelper.GenerarXlsx(datos, mapeoColumnas), nombreArchivo);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw; // Response.End() dentro de la descarga: no es un error.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[{0}] Error al exportar: {1}", GetType().Name, ex);
                ToastService.Show(Page, "Error al exportar los datos. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        // ------------------------------------------------------------------
        // Helpers para las páginas
        // ------------------------------------------------------------------

        /// <summary>TreeViewSearch de la cabecera de la grilla por ID, o null si no existe todavía.</summary>
        protected TreeViewSearch FiltroHeader(string id) =>
            Grilla.HeaderRow?.FindControl(id) as TreeViewSearch;

        /// <summary>Ids numéricos seleccionados en un filtro de cabecera; null si no hay selección (no filtrar).</summary>
        protected List<int> IdsSeleccionados(string idFiltro)
        {
            var filtro = FiltroHeader(idFiltro);
            if (filtro == null || !filtro.ExpandedSelectedValues.Any())
                return null;

            return filtro.ExpandedSelectedValues
                .Select(v => int.TryParse(v, out int id) ? id : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();
        }

        /// <summary>Valores de texto seleccionados en un filtro de cabecera; null si no hay selección (no filtrar).</summary>
        protected List<string> ValoresSeleccionados(string idFiltro)
        {
            var filtro = FiltroHeader(idFiltro);
            return filtro != null && filtro.ExpandedSelectedValues.Any() ? filtro.ExpandedSelectedValues : null;
        }

        /// <summary>Fechas seleccionadas en un filtro de cabecera enlazado a una lista de fechas; null si no hay selección.</summary>
        protected List<DateTime> FechasSeleccionadas(string idFiltro)
        {
            var filtro = FiltroHeader(idFiltro);
            if (filtro == null || !filtro.SelectedValues.Any())
                return null;

            return filtro.SelectedValues
                .Select(s => DateTime.TryParse(s, out DateTime fecha) ? (DateTime?)fecha.Date : null)
                .Where(f => f.HasValue)
                .Select(f => f.Value)
                .ToList();
        }

        /// <summary>Enlaza un filtro de cabecera con una lista de opciones (propiedades Nombre e Id).</summary>
        protected void BindFiltroHeader(string id, object datos, string campoTexto = "Nombre", string campoValor = "Id")
        {
            var filtro = FiltroHeader(id);
            if (filtro == null) return;

            filtro.DataSource = datos;
            filtro.DataTextField = campoTexto;
            filtro.DataValueField = campoValor;
            filtro.DataBind();
        }

        /// <summary>Muestra u oculta columnas de la grilla por DataField o por HeaderText.</summary>
        protected void MostrarColumnas(bool visible, params string[] columnas)
        {
            var nombres = new HashSet<string>(columnas ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            foreach (DataControlField columna in Grilla.Columns)
            {
                bool coincide = columna is BoundField bf && !string.IsNullOrWhiteSpace(bf.DataField)
                    ? nombres.Contains(bf.DataField)
                    : !string.IsNullOrWhiteSpace(columna.HeaderText) && nombres.Contains(columna.HeaderText);

                if (coincide)
                    columna.Visible = visible;
            }
        }

        /// <summary>Selecciona el ítem con ese valor, o deja la lista sin selección si no existe.</summary>
        protected static void SeleccionarValor(DropDownList lista, string valor)
        {
            if (lista == null) return;
            lista.ClearSelection();
            var item = valor == null ? null : lista.Items.FindByValue(valor);
            if (item != null)
                item.Selected = true;
        }
    }
}
