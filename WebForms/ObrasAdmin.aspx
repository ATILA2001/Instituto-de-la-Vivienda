<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ObrasAdmin.aspx.cs" Inherits="WebForms.ObrasAdmin" %>

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
							<th>Area</th>
							<th>Empresa</th>
							<th>Contrata</th>
							<th>Número</th>
							<th>Año</th>
							<th>Etapa</th>
							<th>Obra N°</th>
							<th>Barrio</th>
							<th>Nombre de Obra</th>
							<th></th>
							<th></th>
						</tr>
					</thead>
					<tbody>
						<tr>
							<td>
								<asp:DropDownList ID="ddlArea" CssClass="form-control" runat="server"></asp:DropDownList>
							</td>

							<td>
								<asp:DropDownList ID="ddlEmpresa" CssClass="form-control" runat="server"></asp:DropDownList>
							</td>
							<td>
								<asp:DropDownList ID="ddlContrata" CssClass="form-control" runat="server"></asp:DropDownList>
							</td>
							<td>
								<asp:TextBox ID="txtNumero" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
							</td>
							<td>
								<asp:TextBox ID="txtAño" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
							</td>
							<td>
								<asp:TextBox ID="txtEtapa" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
							</td>
							<td>
								<asp:TextBox ID="txtObra" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
							</td>
							<td>
								<asp:DropDownList ID="ddlBarrio" CssClass="form-control" runat="server"></asp:DropDownList>
							</td>
							<td>
								<asp:TextBox ID="txtDescripcion" CssClass="form-control" runat="server" />
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
						<label class="form-label ms-2 mb-0" for="cblArea">Area:</label>
						<div>

							<CustomControls:CheckBoxListSearch ID="cblArea" runat="server" />
						</div>
					</div>

					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblEmpresa">Empresa:</label>
						<div>

							<CustomControls:CheckBoxListSearch ID="cblEmpresa" runat="server" />
						</div>
					</div>


					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblBarrio">Barrio:</label>
						<div>

							<CustomControls:CheckBoxListSearch ID="cblBarrio" runat="server" />
						</div>
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


		<asp:GridView ID="dgvObra" DataKeyNames="ID" CssClass="table1  table-bordered table-hover mb-4"
			OnSelectedIndexChanged="dgvObra_SelectedIndexChanged"
			OnRowDeleting="dgvObra_RowDeleting"
			AutoGenerateColumns="false" runat="server">

			<Columns>
				<asp:BoundField HeaderText="ID" DataField="Id" />
				<asp:BoundField HeaderText="Área" DataField="Area" />
				<asp:BoundField HeaderText="Empresa" DataField="Empresa" />
				<asp:TemplateField HeaderText="Contrata">
					<ItemTemplate>
						<%# Eval("Contrata") + " " + Eval("Numero") + "/" + Eval("Año") %>
					</ItemTemplate>
				</asp:TemplateField>
				<asp:BoundField HeaderText="Barrio" DataField="Barrio" />
				<asp:BoundField HeaderText="Nombre de Obra" DataField="Descripcion" />
				<asp:BoundField HeaderText="Linea de Gestion" DataField="Linea" />
				<asp:BoundField HeaderText="Proyecto" DataField="Proyecto" />

				<asp:BoundField HeaderText="Disponible Actual" DataField="AutorizadoNuevo" DataFormatString="{0:C}" />
				<asp:BoundField HeaderText="Planificacion 2025" DataField="MontoCertificado" DataFormatString="{0:C}" />
				<asp:BoundField HeaderText="Ejecucion Presupuesto 2025" DataField="Porcentaje" DataFormatString="{0:N2}%" />

				<asp:BoundField HeaderText="Monto de Obra inicial" DataField="MontoInicial" DataFormatString="{0:C}" />
				<asp:BoundField HeaderText="Monto de Obra actual" DataField="MontoActual" DataFormatString="{0:C}" />
				<asp:BoundField HeaderText="Faltante de Obra" DataField="MontoFaltante" DataFormatString="{0:C}" />
				<asp:BoundField HeaderText="Fecha Inicio" DataField="FechaInicio" DataFormatString="{0:dd-MM-yyyy}" />
				<asp:BoundField HeaderText="Fecha Fin" DataField="FechaFin" DataFormatString="{0:dd-MM-yyyy}" />

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


	<%--<script type="text/javascript">
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
			// Inicializamos la visibilidad según el valor almacenado en localStorage
			var sectionVisible = localStorage.getItem("sectionVisible");

			if (sectionVisible === "true") {
				$('#section1').show();
				$('#visibilityText').text("Ocultar sección");
			} else {
				$('#section1').hide();
				$('#visibilityText').text("Agregar Obra");
			}

			// Manejar clic en el botón para alternar la visibilidad
			$('#visibilityMessage').on('click', function () {
				var currentStatus = $('#visibilityText').text();

				if (currentStatus === "Agregar Obra") {
					localStorage.setItem("sectionVisible", "true");
					$('#section1').show();
					$('#visibilityText').text("Ocultar sección");
				} else {
					localStorage.setItem("sectionVisible", "false");
					$('#section1').hide();
					$('#visibilityText').text("Agregar Obra");
				}
			});
		});
		
	</script>--%>
</asp:Content>


