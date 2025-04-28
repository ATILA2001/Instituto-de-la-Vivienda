<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="CertificadosAdmin.aspx.cs" Inherits="WebForms.CertificadosAdmin" %>

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

							<th>Código Autorizante</th>
							<th>Expediente</th>
							<th>Tipo</th>
							<th>Monto Autorizado</th>
							<th>Mes Aprobacion</th>
							<th></th>
							<th></th>
						</tr>
					</thead>
					<tbody>
						<tr>


							<td>
								<asp:DropDownList ID="ddlAutorizante" CssClass="form-control" runat="server"></asp:DropDownList>
							</td>

							<td>
								<asp:TextBox ID="txtExpediente" CssClass="form-control" runat="server" />
							</td>

							<td>
								<asp:DropDownList ID="ddlTipo" CssClass="form-control" runat="server"></asp:DropDownList>
							</td>

							<td>
								<asp:TextBox ID="txtMontoAutorizado" CssClass="form-control" runat="server" />
							</td>
							<td>
								<asp:TextBox ID="txtFecha" CssClass="form-control" runat="server" TextMode="Date" />
							</td>
							<td class="text-right">
								<asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" />
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
						<label class="form-label ms-2 mb-0" for="cblBarrio">Barrio:</label>
						<div>

							<CustomControls:CheckBoxListSearch ID="cblBarrio" runat="server" />
						</div>
					</div>


					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblProyecto">Proyecto:</label>
						<div>

							<CustomControls:CheckBoxListSearch ID="cblProyecto" runat="server" />
						</div>
					</div>

					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblEmpresa">Empresa:</label>
						<div>

							<CustomControls:CheckBoxListSearch ID="cblEmpresa" runat="server" />
						</div>
					</div>

					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblAutorizante">Autorizante:</label>
						<div>

							<CustomControls:CheckBoxListSearch ID="cblAutorizante" runat="server" />
						</div>
					</div>

					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblTipo">Tipo:</label>
						<div>

							<CustomControls:CheckBoxListSearch ID="cblTipo" runat="server" />
						</div>
					</div>

					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblFecha">Mes:</label>
						<div>

							<CustomControls:CheckBoxListSearch ID="cblFecha" runat="server" />
						</div>
					</div>
					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblEstadoExpediente">Estado:</label>
						<div>
							<CustomControls:CheckBoxListSearch ID="cblEstadoExpediente" runat="server" />
						</div>
					</div>


					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblLinea">Linea:</label>
						<div>
							<CustomControls:CheckBoxListSearch ID="cblLinea" runat="server" />
						</div>
					</div>


					<div class="form-group mb-2">
				<label class="form-label ms-2 mb-0" for="txtSubtotal">Subtotal:</label>
				<asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control form-control-uniform" ReadOnly="true" />
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

			<asp:GridView ID="dgvCertificado" DataKeyNames="ID" CssClass="table1  table-bordered table-hover  mb-4 "
				OnSelectedIndexChanged="dgvCertificado_SelectedIndexChanged"
				OnRowDeleting="dgvCertificado_RowDeleting"
				AutoGenerateColumns="false" runat="server">
				<Columns>
					<asp:BoundField HeaderText="ID" DataField="Autorizante.Id" Visible="false" />

					<asp:BoundField HeaderText="Area" DataField="Autorizante.Obra.Area.Nombre" />
					<asp:BoundField HeaderText="Contrata" DataField="Autorizante.Obra.Contrata.Nombre" />
					<asp:BoundField HeaderText="Obra" DataField="Autorizante.Obra.Descripcion" />
					<asp:BoundField HeaderText="Barrio" DataField="Autorizante.Obra.Barrio.Nombre" />
					<asp:BoundField HeaderText="Proyecto" DataField="Autorizante.Obra.Proyecto" />
					<asp:BoundField HeaderText="Empresa" DataField="Empresa" />
					<asp:BoundField HeaderText="Código Autorizante" DataField="Autorizante.CodigoAutorizante" />
					<asp:TemplateField HeaderText="Expediente">
						<ItemTemplate>
							<asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("ExpedientePago") %>' AutoPostBack="true"
								OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm"></asp:TextBox>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField HeaderText="Estado" DataField="Estado" />

					<asp:BoundField HeaderText="Tipo" DataField="Tipo.Nombre" />
					<asp:BoundField HeaderText="Monto Certificado" DataField="MontoTotal" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Mes Certificado" DataField="MesAprobacion" DataFormatString="{0:dd-MM-yyyy}" />
					<asp:BoundField HeaderText="Porcentaje" DataField="Porcentaje" />
					<asp:BoundField HeaderText="Sigaf" DataField="Sigaf" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade" />
					<asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" />

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


<%--	<script type="text/javascript">

		$(document).ready(function () {
			// Inicializamos la visibilidad según el valor de localStorage
			var sectionVisible = localStorage.getItem("sectionVisible");

			// Si está marcado como 'true', mostramos la sección
			if (sectionVisible === "true") {
				$('#section1').show(); // Mostramos la sección
				$('#visibilityText').text("Ocultar sección"); // Texto cuando la sección es visible
			} else {
				$('#section1').hide(); // Ocultamos la sección
				$('#visibilityText').text("Agregar Certificado"); // Texto cuando la sección está oculta
			}

			// Manejar clic en el mensaje para alternar el estado de visibilidad
			$(document).on('click', '#visibilityMessage', function () {
				// Cambiamos el valor de visibilidad
				var currentStatus = $('#visibilityText').text();

				if (currentStatus === "Agregar Certificado") {
					// Si está oculto, lo mostramos
					localStorage.setItem("sectionVisible", "true");
					$('#section1').show(); // Mostramos la sección
					$('#visibilityText').text("Ocultar sección"); // Cambiar el texto
				} else {
					// Si está visible, lo ocultamos
					localStorage.setItem("sectionVisible", "false");
					$('#section1').hide(); // Ocultamos la sección
					$('#visibilityText').text("Agregar Certificado"); // Cambiar el texto
				}
			});
		});

	</script>--%>

</asp:Content>
