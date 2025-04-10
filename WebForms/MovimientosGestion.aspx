<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="MovimientosGestion.aspx.cs" Inherits="WebForms.MovimientosGestion" %>

<%@ Register Src="~/CustomControls/CheckBoxListSearch.ascx" TagPrefix="CustomControls" TagName="CheckBoxListSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
   
    <div id="section1" style="display: none;">
        <div class="row mt-4">
            <div class="col-md-12">
                <table class="table  table1">
                    <thead class="thead-dark">
                        <tr>
                            <th>Obra</th>
                            <th>Movimiento</th>
                            <th>Fecha</th>
                            <th></th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>
                                <asp:DropDownList ID="ddlObra" CssClass="form-control" runat="server"></asp:DropDownList>
                            </td>
                            <td>
                                <asp:TextBox ID="txtMovimiento" CssClass="form-control" runat="server" />
                            </td>
                            <td>
                                <asp:TextBox ID="txtFecha" CssClass="form-control" runat="server" TextMode="Date" />
                            </td>
                            <td class="text-right">
                                <asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click"
                                    CssClass="btn btn-primary" runat="server" />
                            </td>
                            <td class="text-right">
                                <asp:Button Text="Limpiar" ID="btnLimpiar" OnClick="btnLimpiar_Click"
                                    CssClass="btn btn-primary" runat="server" /></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>



    <div class="row mt-4 mb-3">
	<div class="col-12">
		<div class="d-flex justify-content-between align-items-end flex-wrap gap-3">
			<!-- Contenedor de Filtros alineados a la izquierda -->
			<div class="d-flex flex-wrap gap-3">


                            <div class="form-group mb-2">
				<label class="form-label ms-2 mb-0" for="cblObra">Obra:</label>
                                <div >
                                   
                                    <CustomControls:CheckBoxListSearch ID="cblObra" runat="server" />
                                </div>
                            </div>

                            <div class="form-group mb-2">
				<label class="form-label ms-2 mb-0" for="cblFecha">Fecha:</label>
                                <div >
                                  
                                    <CustomControls:CheckBoxListSearch ID="cblFecha" runat="server" />
                                </div>
                            </div></div>

                           		<!-- Contenedor de Botones alineados a la derecha -->
		<div class="d-flex gap-3">

			<div class="form-group mb-2">
				<asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control" placeholder="Buscar..."></asp:TextBox>
			</div>
			<div class="form-group mb-2">
				<%--<asp:Button CssClass="btn btn-primary" ID="btnFiltrar" Text="Filtrar" runat="server" OnClick="btnFiltrar_Click" />--%>
				<asp:LinkButton ID="btnFiltrar" runat="server" CssClass="btn btn-primary" OnClick="btnFiltrar_Click"
					data-bs-toggle="tooltip" data-bs-placement="top" title="Filtrar">
					<i class="bi bi-search"></i>
				</asp:LinkButton>
			</div>


			<%-- logica que aparezca o desaparezca. copiar de david --%>
			<div class="form-group mb-2">
				<asp:LinkButton CssClass="btn btn-primary" ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClick="BtnClearFilters_Click">
					<i class="bi bi-funnel"></i>
				</asp:LinkButton>
			</div>


			<%-- no hace falta logica script, abriria un modal --%>
			<div class="form-group mb-2">
				<%--<button class="btn btn-secondary" id="visibilityMessage">
					<span id="visibilityText">Agregar Obra</span>
				</button>--%>
				<asp:LinkButton runat="server" CssClass="btn btn-primary" data-bs-toggle="modal" data-bs-target="#modalAgregar">
					<i class="bi bi-plus-lg" ></i> Agregar
				</asp:LinkButton>
			</div>

		</div>
	</div>
</div>

<hr class="mb-3" />
                    <asp:GridView ID="dgvMovimiento" DataKeyNames="ID" CssClass="table1  table-bordered table-hover mb-4"
                        OnSelectedIndexChanged="dgvMovimiento_SelectedIndexChanged"
                        OnRowDeleting="dgvMovimiento_RowDeleting"
                        AutoGenerateColumns="false" runat="server" >
                        <Columns>
                            <asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />
                            <asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" />
                                             <asp:BoundField HeaderText="Proyecto" DataField="Proyecto" />
                            <asp:BoundField HeaderText="SubProyecto" DataField="SubProyecto" />
                            <asp:BoundField HeaderText="Linea de Gestion" DataField="Linea" />


                            <asp:BoundField HeaderText="Movimiento" DataField="Monto" DataFormatString="{0:C}" />
                            <asp:BoundField HeaderText="Fecha" DataField="Fecha" DataFormatString="{0:dd-MM-yyyy}" />
                                    <asp:BoundField HeaderText="Autorizado Nuevo" DataField="AutorizadoNuevo" DataFormatString="{0:C}" />
                    <asp:TemplateField HeaderText="Acciones">
                        <ItemTemplate>
                            <div class="d-flex justify-content-center gap-2">
                                <asp:LinkButton ID="btnModificar" runat="server"
                                    CommandName="Select"
                                    CssClass="btn btn-sm btn-warning "
                                    ToolTip="Modificar">
                                    <i class="bi bi-pencil-square"></i>
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnEliminar" runat="server"
                                    CommandName="Delete"
                                    CssClass="btn btn-sm btn-danger "
                                    ToolTip="Eliminar"
                                    OnClientClick="return confirm('¿Está seguro que desea eliminar este registro?');">
                                    <i class="bi bi-trash"></i>
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>


                        </Columns>
                    </asp:GridView>

                    <div class="text-center p-4">
                        <asp:Label ID="lblMensaje" Text="" runat="server" />
                    </div>
                </div>




   <%-- <script type="text/javascript">
        function soloNumeros(e) {
            var tecla = (document) ? e.keyCode : e.which;
            if (tecla == 8 || tecla == 46) {
                return true;
            }
            var patron = /^[0-9]$/;
            var te = String.fromCharCode(tecla);
            return patron.test(te);
        }

        $(document).ready(function () {
            // Textos constantes
            const mostrarTexto = "Cargar Movimiento";
            const ocultarTexto = "Ocultar sección";

            // Variables DOM
            const section = $('#section1');
            const visibilityText = $('#visibilityText');
            const visibilityMessage = $('#visibilityMessage');

            // Asegúrate de que los elementos existan
            if (!section.length || !visibilityText.length || !visibilityMessage.length) {
                console.error("Uno o más elementos no se encontraron en el DOM.");
                return;
            }

            // Función para inicializar el estado
            const initializeVisibility = () => {
                // Lee el estado del localStorage o usa "false" por defecto
                const sectionVisible = localStorage.getItem("sectionVisible") === "true";

                // Aplica visibilidad y actualiza el texto del botón
                section.toggle(sectionVisible);
                visibilityText.text(sectionVisible ? ocultarTexto : mostrarTexto);
            };

            // Función para alternar la visibilidad
            const toggleVisibility = () => {
                // Comprueba el estado actual de la sección
                const sectionVisible = section.is(':visible');

                // Guarda el nuevo estado en localStorage
                localStorage.setItem("sectionVisible", !sectionVisible);

                // Alterna la visibilidad de la sección
                section.toggle();

                // Actualiza el texto del botón
                visibilityText.text(sectionVisible ? mostrarTexto : ocultarTexto);
            };

            // Inicializa al cargar la página
            initializeVisibility();

            // Asigna el evento al botón
            visibilityMessage.off('click').on('click', toggleVisibility);
        });



    </script>
   --%>
</asp:Content>
