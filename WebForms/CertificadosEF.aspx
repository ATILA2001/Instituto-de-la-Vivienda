<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CertificadosEF.aspx.cs" Inherits="WebForms.CertificadosEF" %>

<%@ Register Src="~/CustomControls/TreeViewSearch/TreeViewSearch.ascx" TagPrefix="CustomControls" TagName="TreeViewSearch" %>
<%@ Register Src="~/CustomControls/PaginationControl/PaginationControl.ascx" TagPrefix="CustomControls" TagName="PaginationControl" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

	<!-- Modal -->
	<div class="modal fade" id="modalAgregar" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
		<div class="modal-dialog modal-dialog-centered modal-lg">
			<div class="modal-content">
				<div class="modal-header">
					<h1 class="modal-title fs-5" id="staticBackdropLabel">Agregar Certificado</h1>
					<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
				</div>
				<div class="modal-body">
					<div class="form-group">
						<div class="container">
							<div class="row">
								<div id="autorizanteContainer" class="col-12">
									<div class="mb-3">
										<label for="ddlAutorizante" class="form-label">Código Autorizante</label>
										<asp:DropDownList ID="ddlAutorizante" CssClass="form-select" runat="server" AppendDataBoundItems="true">
											<asp:ListItem Value="" Text="Seleccione un autorizante" Selected="True"></asp:ListItem>
										</asp:DropDownList>
										<asp:RequiredFieldValidator ID="rfvAutorizante"
											ControlToValidate="ddlAutorizante"
											ValidationGroup="AgregarCertificado"
											runat="server"
											ErrorMessage="Seleccione un autorizante"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true"
											InitialValue="" />
									</div>
								</div>
								<div class="col-6">
									<div class="mb-3">
										<label for="txtExpediente" class="form-label">Expediente</label>
										<asp:TextBox ID="txtExpediente" CssClass="form-control" runat="server" placeHolder="xxxxxxxx/25" />

									</div>
								</div>
								<div class="col-6">
									<div class="mb-3">
										<label for="ddlTipo" class="form-label">Tipo</label>
										<asp:DropDownList ID="ddlTipo" CssClass="form-select" runat="server" AppendDataBoundItems="true">
											<asp:ListItem Value="" Text="Seleccione un tipo" Selected="True"></asp:ListItem>
										</asp:DropDownList>
										<asp:RequiredFieldValidator ID="rfvTipo"
											ControlToValidate="ddlTipo"
											ValidationGroup="AgregarCertificado"
											runat="server"
											ErrorMessage="Seleccione un tipo"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true"
											InitialValue="" />
									</div>
								</div>
								<div class="col-6">
									<div class="mb-3">
										<label for="txtMontoCertificado" class="form-label">Monto Certificado</label>
										<asp:TextBox ID="txtMontoCertificado" CssClass="form-control" runat="server" placeHolder="0,00" />
										<asp:RequiredFieldValidator ID="rfvMontoCertificado"
											ControlToValidate="txtMontoCertificado"
											ValidationGroup="AgregarCertificado"
											runat="server"
											ErrorMessage="El monto es requerido"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true" />
										<asp:RegularExpressionValidator ID="revMontoCertificado"
											ControlToValidate="txtMontoCertificado"
											ValidationGroup="AgregarCertificado"
											runat="server"
											ValidationExpression="^[0-9]+(\,[0-9]{1,2})?$"
											ErrorMessage="Solo números positivos con hasta 2 decimales"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true" />
									</div>
								</div>
								<div class="col-6">
									<div class="mb-3">
										<label for="txtFecha" class="form-label">Mes Aprobacion</label>
										<asp:TextBox ID="txtFecha" CssClass="form-control" runat="server" TextMode="Date" />
										<asp:RequiredFieldValidator ID="rfvFecha"
											ControlToValidate="txtFecha"
											ValidationGroup="AgregarCertificado"
											runat="server"
											ErrorMessage="El mes de aprobación es requerido"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true" />
									</div>
								</div>
							</div>
						</div>
					</div>
				</div>
				<div class="modal-footer d-flex justify-content-between px-4">
					<button type="button" class="btn btn-secondary" onclick="limpiarFormulario()">Limpiar</button>
					<div class="d-flex gap-4">
						<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
						<asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" ValidationGroup="AgregarCertificado" />
					</div>
				</div>
			</div>
		</div>
	</div>
	<!-- /Modal -->

	<div class="row mt-4 mb-3">
		<div class="col-12">
			<div class="d-flex justify-content-end align-items-end flex-wrap gap-3">

				<!-- Contenedor de Botones alineados a la derecha -->
				<div class="d-flex gap-3">

					<div class="form-group mb-2">
						<asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control" placeholder="Buscar..."></asp:TextBox>
					</div>
					<div class="form-group mb-2">
						<%--<asp:Button CssClass="btn btn-primary" ID="btnFiltrar" Text="Filtrar" runat="server" OnClick="btnFiltrar_Click" />--%>
						<asp:LinkButton ID="btnFiltrar" runat="server" CssClass="btn btn-primary" OnClick="btnFiltrar_Click"
							data-bs-toggle="tooltip" data-bs-placement="top" title="Buscar">
					<i class="bi bi-search"></i>
						</asp:LinkButton>
					</div>


					<%-- logica que aparezca o desaparezca. copiar de david --%>
					<div class="form-group mb-2">
						<asp:LinkButton CssClass="btn btn-primary" ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClick="BtnClearFilters_Click"
							data-bs-toggle="tooltip" data-bs-placement="top" title="Quita todos los filtros">
					<i class="bi bi-funnel"></i>
						</asp:LinkButton>
					</div>
					<div class="form-group mb-2">
						<asp:LinkButton ID="btnExportarExcel" runat="server" CssClass="btn btn-success" OnClick="btnExportarExcel_Click"
							data-bs-toggle="tooltip" data-bs-placement="top" title="Exportar a Excel">
    <i class="bi bi-download"></i>
						</asp:LinkButton>
					</div>

					<asp:Panel ID="panelShowAddButton" runat="server">
						<div class="form-group mb-2">
							<asp:LinkButton ID="btnShowAddModal" runat="server" CssClass="btn btn-primary" OnClick="btnShowAddModal_Click">
                                <i class="bi bi-plus-lg"></i> Agregar
							</asp:LinkButton>
						</div>
					</asp:Panel>

				</div>
			</div>
		</div>

		<hr class="mb-3" />

		<div class="gridview-scroll-container">
			<asp:GridView ID="gridviewRegistros" DataKeyNames="Id" CssClass="table1  table-bordered table-hover  mb-4"
				OnSelectedIndexChanged="gridviewRegistros_SelectedIndexChanged"
				OnRowDeleting="gridviewRegistros_RowDeleting"
				OnRowDataBound="gridviewRegistros_RowDataBound"
				ShowHeaderWhenEmpty="true"
				AutoGenerateColumns="false"
				AllowPaging="false"
				AllowCustomPaging="false"
				runat="server">
				<Columns>
					<asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />

					<asp:TemplateField>
						<HeaderTemplate>
							<CustomControls:TreeViewSearch ID="cblsHeaderArea" runat="server"
								HeaderText="Area"
								DataTextField="Nombre"
								DataValueField="Id"
								OnAcceptChanges="OnAcceptChanges" />
						</HeaderTemplate>
						<ItemTemplate>
							<%# Eval("AreaNombre") %>
						</ItemTemplate>
					</asp:TemplateField>

					<asp:BoundField HeaderText="Contrata" DataField="Contrata" />

					<asp:TemplateField HeaderText="Obra">
						<HeaderTemplate>
							<CustomControls:TreeViewSearch ID="cblsHeaderObra" runat="server"
								HeaderText="Obra"
								DataTextField="Descripcion"
								DataValueField="Id"
								OnAcceptChanges="OnAcceptChanges" />
						</HeaderTemplate>
						<ItemTemplate>
							<%# Eval("ObraDescripcion") %>
						</ItemTemplate>
					</asp:TemplateField>

					<asp:TemplateField HeaderText="Barrio">
						<HeaderTemplate>
							<CustomControls:TreeViewSearch ID="cblsHeaderBarrio" runat="server"
								HeaderText="Barrio"
								DataTextField="Nombre"
								DataValueField="Id"
								OnAcceptChanges="OnAcceptChanges" />
						</HeaderTemplate>
						<ItemTemplate>
							<%# Eval("BarrioNombre") %>
						</ItemTemplate>
					</asp:TemplateField>

					<asp:TemplateField HeaderText="Proyecto">
						<HeaderTemplate>
							<CustomControls:TreeViewSearch ID="cblsHeaderProyecto" runat="server"
								HeaderText="Proyecto"
								DataTextField="Nombre"
								DataValueField="Id"
								OnAcceptChanges="OnAcceptChanges" />
						</HeaderTemplate>
						<ItemTemplate>
							<%# Eval("ProyectoNombre") %>
						</ItemTemplate>
					</asp:TemplateField>

					<asp:TemplateField HeaderText="Empresa">
						<HeaderTemplate>
							<CustomControls:TreeViewSearch ID="cblsHeaderEmpresa" runat="server"
								HeaderText="Empresa"
								DataTextField="Nombre"
								DataValueField="Id"
								OnAcceptChanges="OnAcceptChanges" />
						</HeaderTemplate>
						<ItemTemplate>
							<%# Eval("EmpresaNombre") %>
						</ItemTemplate>
					</asp:TemplateField>

					<asp:TemplateField>
						<HeaderTemplate>
							<CustomControls:TreeViewSearch ID="cblsHeaderCodigoAutorizante" runat="server"
								HeaderText="Codigo Autorizante"
								DataTextField="CodigoAutorizante"
								DataValueField="Id"
								OnAcceptChanges="OnAcceptChanges" />
						</HeaderTemplate>
						<ItemTemplate>
							<%# Eval("CodigoAutorizante") %>
						</ItemTemplate>
					</asp:TemplateField>

					<asp:TemplateField HeaderText="Estado Autorizante">
						<HeaderTemplate>
							<CustomControls:TreeViewSearch ID="cblsHeaderAutorizanteEstado" runat="server"
								HeaderText="Estado Autorizante"
								DataTextField="Estado"
								DataValueField="Id"
								OnAcceptChanges="OnAcceptChanges" />
						</HeaderTemplate>
						<ItemTemplate>
							<%# Eval("AutorizanteEstado") %>
						</ItemTemplate>
					</asp:TemplateField>

					<asp:TemplateField HeaderText="Expediente">
						<ItemTemplate>
							<asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("ExpedientePago") %>' AutoPostBack="true"
								OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm w-auto text-center"></asp:TextBox>
						</ItemTemplate>
					</asp:TemplateField>



					<asp:TemplateField HeaderText="Tipo">
						<HeaderTemplate>
							<CustomControls:TreeViewSearch ID="cblsHeaderTipo" runat="server"
								HeaderText="Tipo"
								DataTextField="Nombre"
								DataValueField="Id"
								OnAcceptChanges="OnAcceptChanges" />
						</HeaderTemplate>
						<ItemTemplate>
							<%# Eval("TipoPagoNombre") %>
						</ItemTemplate>
					</asp:TemplateField>

					<asp:BoundField HeaderText="Monto Certificado" DataField="MontoTotal" DataFormatString="{0:C}" />

					<asp:TemplateField HeaderText="Mes Certificado">
						<HeaderTemplate>
							<CustomControls:TreeViewSearch ID="cblsHeaderMesCertificado" runat="server"
								HeaderText="Mes Certificado"
								OnAcceptChanges="OnAcceptChanges" />
						</HeaderTemplate>
						<ItemTemplate>
							<%# Eval("MesAprobacion", "{0:dd-MM-yyyy}") %>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="Estado Pago">
						<HeaderTemplate>
							<CustomControls:TreeViewSearch ID="cblsHeaderEstado" runat="server"
								HeaderText="Estado Pago"
								OnAcceptChanges="OnAcceptChanges" />
						</HeaderTemplate>
						<ItemTemplate>
							<%# Eval("Estado") %>
						</ItemTemplate>
					</asp:TemplateField>

					<asp:TemplateField>
						<HeaderTemplate>
							<CustomControls:TreeViewSearch ID="cblsHeaderLinea" runat="server"
								HeaderText="Linea"
								DataTextField="Nombre"
								DataValueField="Id"
								OnAcceptChanges="OnAcceptChanges" />
						</HeaderTemplate>
						<ItemTemplate>
							<%# Eval("LineaGestionNombre") %>
						</ItemTemplate>
					</asp:TemplateField>

					<asp:BoundField HeaderText="Porcentaje" DataField="Porcentaje" DataFormatString="{0:F2}%" />
					<asp:BoundField HeaderText="Sigaf" DataField="Sigaf" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Buzon SADE" DataField="BuzonSade" />
					<asp:BoundField HeaderText="Fecha SADE" DataField="FechaSade" DataFormatString="{0:dd/MM/yyyy}" />


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
									CssClass="btn btn-sm btn-danger"
									ToolTip="Eliminar"
									OnClientClick="return confirm('¿Está seguro que desea eliminar este registro?');">
                                    <i class="bi bi-trash"></i>
								</asp:LinkButton>
							</div>
						</ItemTemplate>
					</asp:TemplateField>
				</Columns>
				<EmptyDataTemplate>
					<div class="text-center m-3">
						<i class="bi bi-info-circle fs-4"></i>
						<p class="mb-0">No hay elementos para mostrar o registros que coincidan con los filtros aplicados.</p>
					</div>
				</EmptyDataTemplate>
			</asp:GridView>
		</div>

		<!-- Control de paginación reutilizable -->
		<CustomControls:PaginationControl ID="paginationControl" runat="server"
			OnPageChanged="paginationControl_PageChanged"
			OnPageSizeChanged="paginationControl_PageSizeChanged" />

		<div class="text-center p-4">
			<asp:Label ID="lblMensaje" Text="" runat="server" />
		</div>
	</div>


	<script type="text/javascript">
		function limpiarFormulario() {
			document.getElementById('<%= txtExpediente.ClientID %>').value = '';
			document.getElementById('<%= txtMontoCertificado.ClientID %>').value = '';
			document.getElementById('<%= txtFecha.ClientID %>').value = '';
			document.getElementById('<%= ddlAutorizante.ClientID %>').selectedIndex = 0;
			document.getElementById('<%= ddlTipo.ClientID %>').selectedIndex = 0;
		}
	</script>

</asp:Content>
