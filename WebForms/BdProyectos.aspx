<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="BdProyectos.aspx.cs" Inherits="WebForms.BdProyectos" %>

<%@ Register Src="~/CustomControls/CheckBoxListSearch.ascx" TagPrefix="CustomControls" TagName="CheckBoxListSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">


	<!-- Modal -->
	<div class="modal fade" id="modalAgregar" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
		<div class="modal-dialog modal-dialog-centered modal-lg">
			<div class="modal-content">
				<div class="modal-header">
					<h1 class="modal-title fs-5" id="staticBackdropLabel">Agregar Proyecto</h1>
					<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
				</div>
				<div class="modal-body">

					<div class="form-group">
						<div class="container">
							<div class="row">
								<div class="col-12">
									<div class="mb-3">
										<label for="ddlObra" class="form-label">Obra</label>
										<asp:DropDownList ID="ddlObra" CssClass="form-select" runat="server"></asp:DropDownList>
									</div>
								</div>
								<div class="col-6">

									<div class="mb-3">
										<label for="txtProyecto" class="form-label">Proyecto</label>
										<asp:TextBox ID="txtProyecto" CssClass="form-control" runat="server" />
									</div>

									<div class="mb-3">
										<label for="ddlLineaGestion" class="form-label">Linea de Gestión</label>
										<asp:DropDownList ID="ddlLineaGestion" CssClass="form-select" runat="server" />
									</div>


								</div>

								<div class="col-6">

									<div class="mb-3">
										<label for="txtSubProyecto" class="form-label">SubProyecto</label>
										<asp:TextBox ID="txtSubProyecto" CssClass="form-control" runat="server" />
									</div>

									<div class="mb-3">
										<label for="txtMontoAutorizadoInicial" class="form-label">Monto Autorizado</label>
										<asp:TextBox ID="txtMontoAutorizadoInicial" CssClass="form-control" runat="server" />
									</div>


								</div>
							</div>
						</div>

					</div>


				</div>
				<div class="modal-footer">
					<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
					<asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" />

				</div>
			</div>
		</div>
	</div>
	<!-- /Modal -->


	<%--    <div id="section12" class="modal modal-fade">
        <div class="modal-body">
            <div class="col-md-12">
                <table class="table table1">
                    <thead class="thead-dark">
                        <tr>
                            <th>Obra</th>
                            <th>Proyecto</th>
                            <th>SubProyecto</th>
                            <th>Linea de Gestión</th>
                            <th>Monto Autorizado Inicial</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>
                                <asp:DropDownList ID="ddlObra" CssClass="form-control" runat="server"></asp:DropDownList>
                            </td>
                            <td>
                                <asp:TextBox ID="txtProyecto" CssClass="form-control" runat="server" />
                            </td>
                            <td>
                                <asp:TextBox ID="txtSubProyecto" CssClass="form-control" runat="server" />
                            </td>
                            <td>
                                <asp:DropDownList ID="ddlLineaGestion" CssClass="form-control" runat="server"></asp:DropDownList>
                            </td>
                            <td>
                                <asp:TextBox ID="txtMontoAutorizadoInicial" CssClass="form-control" runat="server" />
                            </td>

                        </tr>
                    </tbody>
                </table>

                <div class="modal-footer">
                    <asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" />
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                </div>

            </div>
        </div>
    </div>--%>


	<div class="row mt-4 mb-3">
		<div class="col-12">
			<div class="d-flex justify-content-between align-items-end flex-wrap gap-3">
				<!-- Contenedor de Filtros alineados a la izquierda -->
				<div class="d-flex flex-wrap gap-3">

					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblArea">Area:</label>
						<div>
							<CustomControls:CheckBoxListSearch ID="cblArea" runat="server" />
						</div>
					</div>

					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblProyecto">Proyecto:</label>
						<div>
							<CustomControls:CheckBoxListSearch ID="cblProyecto" runat="server" />
						</div>
					</div>

					<div class="form-group mb-2 ">
						<label class="form-label ms-2 mb-0" for="cblLinea">Linea:</label>
						<div>
							<CustomControls:CheckBoxListSearch ID="cblLinea" runat="server" />
						</div>
					</div>

					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="txtSubtotal">Subtotal:</label>
						<asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control" ReadOnly="true" />
					</div>
				</div>

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

		<asp:GridView ID="dgvBdProyecto" DataKeyNames="Id" CssClass="table1 table-bordered table-hover mb-4"
			OnSelectedIndexChanged="dgvBdProyecto_SelectedIndexChanged"
			OnRowDeleting="dgvBdProyecto_RowDeleting"
			AutoGenerateColumns="false" runat="server">
			<Columns>
				<asp:BoundField HeaderText="Area " DataField="Obra.Area.Nombre" />
				<asp:BoundField HeaderText="Contrata " DataField="Obra.Contrata.Nombre" />
				<asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" />
				<asp:BoundField HeaderText="Proyecto" DataField="Proyecto" />
				<asp:BoundField HeaderText="SubProyecto" DataField="SubProyecto" />
				<asp:BoundField HeaderText="Linea de Gestión" DataField="LineaGestion.Nombre" />
				<asp:BoundField HeaderText="Monto Inicial" DataField="AutorizadoInicial" DataFormatString="{0:C}" />
				<asp:BoundField HeaderText="Monto Nuevo" DataField="AutorizadoNuevo" DataFormatString="{0:C}" />
				<asp:TemplateField HeaderText="Acciones">
					<ItemTemplate>
						<div class="d-flex justify-content-center gap-2">
							<asp:LinkButton ID="btnModificar" runat="server"
								CommandName="Select"
								CssClass="btn btn-sm btn-warning"
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

	<%--<script type="text/javascript">
		$(document).ready(function () {
			// Inicializamos la visibilidad según el valor almacenado en localStorage
			var sectionVisible = localStorage.getItem("sectionVisible");

			if (sectionVisible === "true") {
				$('#section1').show();
				$('#visibilityText').text("Ocultar sección");
			} else {
				$('#section1').hide();
				$('#visibilityText').text("Agregar Proyecto");
			}

			// Manejar clic en el botón para alternar la visibilidad
			$('#visibilityMessage').on('click', function () {
				var currentStatus = $('#visibilityText').text();

				if (currentStatus === "Agregar Proyecto") {
					localStorage.setItem("sectionVisible", "true");
					$('#section1').show();
					$('#visibilityText').text("Ocultar sección");
				} else {
					localStorage.setItem("sectionVisible", "false");
					$('#section1').hide();
					$('#visibilityText').text("Agregar Proyecto");
				}
			});
		});
	</script>--%>

	<script type="text/javascript">
		const myModal = document.getElementById('myModal')
		const myInput = document.getElementById('myInput')

		myModal.addEventListener('shown.bs.modal', () => {
			myInput.focus()
		})
	</script>
</asp:Content>
