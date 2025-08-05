using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms.CustomControls
{
    public partial class PaginationControl : UserControl
    {
        #region Eventos Públicos
        public event EventHandler<PaginationEventArgs> PageChanged;
        public event EventHandler<PaginationEventArgs> PageSizeChanged;
        #endregion

        #region Propiedades Públicas
        
        /// <summary>
        /// Índice de la página actual (base 0)
        /// </summary>
        public int CurrentPageIndex
        {
            get { return ViewState["CurrentPageIndex"] != null ? (int)ViewState["CurrentPageIndex"] : 0; }
            set { ViewState["CurrentPageIndex"] = value; }
        }

        /// <summary>
        /// Cantidad de registros por página
        /// </summary>
        public int PageSize
        {
            get { return ViewState["PageSize"] != null ? (int)ViewState["PageSize"] : 12; }
            set 
            { 
                ViewState["PageSize"] = value;
                if (ddlPageSizeExternal.Items.FindByValue(value.ToString()) != null)
                {
                    ddlPageSizeExternal.SelectedValue = value.ToString();
                }
            }
        }

        /// <summary>
        /// Total de registros disponibles
        /// </summary>
        public int TotalRecords
        {
            get { return ViewState["TotalRecords"] != null ? (int)ViewState["TotalRecords"] : 0; }
            set { ViewState["TotalRecords"] = value; }
        }

        /// <summary>
        /// Total de páginas calculado
        /// </summary>
        public int TotalPages => TotalRecords > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;

        /// <summary>
        /// Texto del subtotal que se muestra en el centro
        /// </summary>
        public string SubtotalText
        {
            get { return lblSubtotalPaginacion.Text; }
            set { lblSubtotalPaginacion.Text = value; }
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Inicializa el control con los valores de paginación
        /// </summary>
        public void Initialize(int totalRecords, int currentPageIndex = 0, int pageSize = 12)
        {
            TotalRecords = totalRecords;
            CurrentPageIndex = currentPageIndex;
            PageSize = pageSize;
            UpdatePaginationControls();
        }

        /// <summary>
        /// Actualiza la visibilidad y estado de todos los controles de paginación
        /// </summary>
        public void UpdatePaginationControls()
        {
            if (TotalRecords == 0)
            {
                this.Visible = false;
                return;
            }

            this.Visible = true;
            
            // Actualizar información de página
            lblPaginaInfo.Text = $"Página {CurrentPageIndex + 1} de {Math.Max(TotalPages, 1)}";
            
            // LÓGICA ESPECIAL: Los botones de navegación solo se activan si hay más de 5 páginas
            bool hasMoreThan5Pages = TotalPages > 5;
            
            // Habilitar/deshabilitar botones de navegación
            lnkFirst.Enabled = hasMoreThan5Pages && CurrentPageIndex > 0;
            lnkPrev.Enabled = hasMoreThan5Pages && CurrentPageIndex > 0;
            lnkNext.Enabled = hasMoreThan5Pages && CurrentPageIndex < TotalPages - 1;
            lnkLast.Enabled = hasMoreThan5Pages && CurrentPageIndex < TotalPages - 1;
            
            // Aplicar estilos visuales según estado
            lnkFirst.CssClass = lnkFirst.Enabled ? "btn btn-sm btn-outline-primary" : "btn btn-sm btn-outline-secondary disabled";
            lnkPrev.CssClass = lnkPrev.Enabled ? "btn btn-sm btn-outline-primary" : "btn btn-sm btn-outline-secondary disabled";
            lnkNext.CssClass = lnkNext.Enabled ? "btn btn-sm btn-outline-primary" : "btn btn-sm btn-outline-secondary disabled";
            lnkLast.CssClass = lnkLast.Enabled ? "btn btn-sm btn-outline-primary" : "btn btn-sm btn-outline-secondary disabled";
            
            // Configurar botones de página numerados
            ConfigurarBotonesPagina();
        }

        /// <summary>
        /// Obtiene información de paginación para usar en consultas
        /// </summary>
        public PaginationInfo GetPaginationInfo()
        {
            return new PaginationInfo
            {
                CurrentPageIndex = CurrentPageIndex,
                PageSize = PageSize,
                TotalRecords = TotalRecords,
                Skip = CurrentPageIndex * PageSize,
                Take = PageSize
            };
        }

        /// <summary>
        /// Actualiza el texto del subtotal con el monto y cantidad de registros
        /// </summary>
        /// <param name="totalMonto">Monto total a mostrar</param>
        /// <param name="cantidadRegistros">Cantidad de registros</param>
        public void UpdateSubtotal(decimal totalMonto, int cantidadRegistros)
        {
            lblSubtotalPaginacion.Text = $"Total: {totalMonto:C} ({cantidadRegistros} registros)";
        }

        #endregion

        #region Métodos Privados

        /// <summary>
        /// Configura los botones de páginas numeradas con lógica de ventana deslizante
        /// </summary>
        private void ConfigurarBotonesPagina()
        {
            var botonesPagina = new[] { lnkPage1, lnkPage2, lnkPage3, lnkPage4, lnkPage5 };
            
            // Calcular rango de páginas a mostrar (ventana deslizante)
            int startPage = Math.Max(0, CurrentPageIndex - 2);
            int endPage = Math.Min(TotalPages - 1, startPage + 4);
            
            // Ajustar startPage si hay menos de 5 páginas al final
            if (endPage - startPage < 4 && TotalPages >= 5)
                startPage = Math.Max(0, endPage - 4);
            
            for (int i = 0; i < botonesPagina.Length; i++)
            {
                int pageIndex = startPage + i;
                var boton = botonesPagina[i];
                
                if (pageIndex < TotalPages)
                {
                    boton.Visible = true;
                    boton.Text = (pageIndex + 1).ToString();
                    boton.CommandArgument = pageIndex.ToString();
                    boton.CssClass = pageIndex == CurrentPageIndex 
                        ? "btn btn-sm btn-primary mx-1" 
                        : "btn btn-sm btn-outline-primary mx-1";
                }
                else
                {
                    boton.Visible = false;
                }
            }
        }

        /// <summary>
        /// Dispara el evento PageChanged
        /// </summary>
        private void OnPageChanged()
        {
            PageChanged?.Invoke(this, new PaginationEventArgs 
            { 
                PageIndex = CurrentPageIndex, 
                PageSize = PageSize,
                TotalRecords = TotalRecords
            });
        }

        /// <summary>
        /// Dispara el evento PageSizeChanged
        /// </summary>
        private void OnPageSizeChanged()
        {
            PageSizeChanged?.Invoke(this, new PaginationEventArgs 
            { 
                PageIndex = CurrentPageIndex, 
                PageSize = PageSize,
                TotalRecords = TotalRecords
            });
        }

        #endregion

        #region Event Handlers

        protected void lnkFirst_Click(object sender, EventArgs e)
        {
            if (TotalPages > 5) // Solo funciona si hay más de 5 páginas
            {
                CurrentPageIndex = 0;
                UpdatePaginationControls();
                OnPageChanged();
            }
        }

        protected void lnkPrev_Click(object sender, EventArgs e)
        {
            if (TotalPages > 5 && CurrentPageIndex > 0) // Solo funciona si hay más de 5 páginas
            {
                CurrentPageIndex--;
                UpdatePaginationControls();
                OnPageChanged();
            }
        }

        protected void lnkNext_Click(object sender, EventArgs e)
        {
            if (TotalPages > 5 && CurrentPageIndex < TotalPages - 1) // Solo funciona si hay más de 5 páginas
            {
                CurrentPageIndex++;
                UpdatePaginationControls();
                OnPageChanged();
            }
        }

        protected void lnkLast_Click(object sender, EventArgs e)
        {
            if (TotalPages > 5) // Solo funciona si hay más de 5 páginas
            {
                CurrentPageIndex = Math.Max(0, TotalPages - 1);
                UpdatePaginationControls();
                OnPageChanged();
            }
        }

        protected void lnkPage_Click(object sender, EventArgs e)
        {
            var button = sender as LinkButton;
            if (button != null && int.TryParse(button.CommandArgument, out int pageIndex))
            {
                CurrentPageIndex = pageIndex;
                UpdatePaginationControls();
                OnPageChanged();
            }
        }

        protected void ddlPageSizeExternal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(ddlPageSizeExternal.SelectedValue, out int newPageSize))
            {
                PageSize = newPageSize;
                CurrentPageIndex = 0; // Reiniciar a la primera página
                UpdatePaginationControls();
                OnPageSizeChanged();
            }
        }

        #endregion
    }

    #region Clases de Apoyo

    /// <summary>
    /// Argumentos para eventos de paginación
    /// </summary>
    public class PaginationEventArgs : EventArgs
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
    }

    /// <summary>
    /// Información de paginación para consultas
    /// </summary>
    public class PaginationInfo
    {
        public int CurrentPageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }

    #endregion
}
