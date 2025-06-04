<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="FormulacionAdmin.aspx.cs" Inherits="WebForms.FormulacionAdmin" %>

<%@ Register Src="~/CustomControls/TreeViewSearch.ascx" TagPrefix="CustomControls" TagName="TreeViewSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

	<!-- Modal -->
	<div class="modal fade" id="modalAgregar" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
		<div class="modal-dialog modal-dialog-centered modal-lg">
			<div class="modal-content">
				<div class="modal-header">
					<h1 class="modal-title fs-5" id="staticBackdropLabel">Agregar Formulación</h1>
					<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
				</div>
				<div class="modal-body">
					<div class="form-group">
						<div class="container">
							<div class="row">
								<div class="col-12">
									<div class="mb-3">
										<label for="ddlObra" class="form-label">Obra</label>
										<asp:DropDownList ID="ddlObra" CssClass="form-select" runat="server" AppendDataBoundItems="true">
											<asp:ListItem Value="" Text="Seleccione una obra" Selected="True"></asp:ListItem>
										</asp:DropDownList>
										<asp:RequiredFieldValidator ID="rfvObra"
											ControlToValidate="ddlObra"
											ValidationGroup="AgregarFormulacion"
											runat="server"
											ErrorMessage="Seleccione una obra"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true"
											InitialValue="" />
									</div>
								</div>

								<div class="col-6">
									<div class="mb-3">
										<label for="txtTechos" class="form-label">Techos 2026</label>

										<asp:TextBox ID="txtTechos" CssClass="form-control" runat="server" onkeypress="return soloNumerosDecimales(event)" />
										<asp:RequiredFieldValidator ID="rfvTechos"
											ControlToValidate="txtTechos"
											ValidationGroup="AgregarFormulacion"
											runat="server"
											ErrorMessage="Ingrese los techos"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true" />
										<asp:RegularExpressionValidator ID="revTechos"
											ControlToValidate="txtTechos"
											ValidationGroup="AgregarFormulacion"
											runat="server"
											ErrorMessage="Ingrese un valor numérico válido"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true"
											ValidationExpression="^\d+(\,\d{1,2})?$" />
									</div>
								</div>


								<div class="col-6">
									<div class="mb-3">
										<label for="txtMesBase" class="form-label">Mes Base</label>
										<asp:TextBox ID="txtMesBase" CssClass="form-control" runat="server" TextMode="Date" />

									</div>
								</div>


								<div class="col-6">
									<div class="mb-3">
										<label for="txtMonto26" class="form-label">Monto 26</label>
										<asp:TextBox ID="txtMonto26" CssClass="form-control" runat="server" onkeypress="return soloNumerosDecimales(event)" />
										<asp:RequiredFieldValidator ID="rfvMonto26"
											ControlToValidate="txtMonto26"
											ValidationGroup="AgregarFormulacion"
											runat="server"
											ErrorMessage="Ingrese el monto 26"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true" />
										<asp:RegularExpressionValidator ID="revMonto26"
											ControlToValidate="txtMonto26"
											ValidationGroup="AgregarFormulacion"
											runat="server"
											ErrorMessage="Ingrese un valor numérico válido"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true"
											ValidationExpression="^\d+(\,\d{1,2})?$" />
									</div>
								</div>
								<div class="col-6">
									<div class="mb-3">
										<label for="txtMonto27" class="form-label">Monto 27</label>
										<asp:TextBox ID="txtMonto27" CssClass="form-control" runat="server" onkeypress="return soloNumerosDecimales(event)" />
										<asp:RequiredFieldValidator ID="rfvMonto27"
											ControlToValidate="txtMonto27"
											ValidationGroup="AgregarFormulacion"
											runat="server"
											ErrorMessage="Ingrese el monto 27"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true" />
										<asp:RegularExpressionValidator ID="revMonto27"
											ControlToValidate="txtMonto27"
											ValidationGroup="AgregarFormulacion"
											runat="server"
											ErrorMessage="Ingrese un valor numérico válido"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true"
											ValidationExpression="^\d+(\,\d{1,2})?$" />
									</div>
								</div>
								<div class="col-6">
									<div class="mb-3">
										<label for="txtMonto28" class="form-label">Monto 28</label>
										<asp:TextBox ID="txtMonto28" CssClass="form-control" runat="server" onkeypress="return soloNumerosDecimales(event)" />
										<asp:RequiredFieldValidator ID="rfvMonto28"
											ControlToValidate="txtMonto28"
											ValidationGroup="AgregarFormulacion"
											runat="server"
											ErrorMessage="Ingrese el monto 28"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true" />
										<asp:RegularExpressionValidator ID="revMonto28"
											ControlToValidate="txtMonto28"
											ValidationGroup="AgregarFormulacion"
											runat="server"
											ErrorMessage="Ingrese un valor numérico válido"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true"
											ValidationExpression="^\d+(\,\d{1,2})?$" />
									</div>
								</div>
								<div class="col-6">
									<div class="mb-3">
										<label for="txtPPI" class="form-label">PPI</label>
										<asp:TextBox ID="txtPPI" CssClass="form-control" runat="server" onkeypress="return soloNumerosDecimales(event)" />
										<asp:RequiredFieldValidator ID="rfvPPI"
											ControlToValidate="txtPPI"
											ValidationGroup="AgregarFormulacion"
											runat="server"
											ErrorMessage="Ingrese el valor PPI"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true" />
										<asp:RegularExpressionValidator ID="revPPI"
											ControlToValidate="txtPPI"
											ValidationGroup="AgregarFormulacion"
											runat="server"
											ErrorMessage="Ingrese un valor numérico válido"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true"
											ValidationExpression="^\d+(\,\d{1,2})?$" />
									</div>
								</div>
								<div class="col-6">
									<div class="mb-3">
										<label for="ddlUnidadMedida" class="form-label">Unidad de Medida</label>
										<asp:DropDownList ID="ddlUnidadMedida" CssClass="form-select" runat="server" AppendDataBoundItems="true">
											<asp:ListItem Value="" Text="Seleccione una unidad de medida" Selected="True"></asp:ListItem>
										</asp:DropDownList>
										<asp:RequiredFieldValidator ID="rfvUnidadMedida"
											ControlToValidate="ddlUnidadMedida"
											ValidationGroup="AgregarFormulacion"
											runat="server"
											ErrorMessage="Seleccione una unidad de medida"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true"
											InitialValue="" />
									</div>
								</div>
								<div class="col-6">
									<div class="mb-3">
										<label for="txtValorMedida" class="form-label">Valor de Medida</label>
										<asp:TextBox ID="txtValorMedida" CssClass="form-control" runat="server" onkeypress="return soloNumerosDecimales(event)" />
										<asp:RequiredFieldValidator ID="rfvValorMedida"
											ControlToValidate="txtValorMedida"
											ValidationGroup="AgregarFormulacion"
											runat="server"
											ErrorMessage="Ingrese el valor de medida"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true" />
										<asp:RegularExpressionValidator ID="revValorMedida"
											ControlToValidate="txtValorMedida"
											ValidationGroup="AgregarFormulacion"
											runat="server"
											ErrorMessage="Ingrese un valor numérico válido"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true"
											ValidationExpression="^\d+(\,\d{1,2})?$" />
									</div>
								</div>
								<div class="col-6">
									<div class="mb-3">
										<label for="ddlPrioridades" class="form-label">Prioridad</label>
										<asp:DropDownList ID="ddlPrioridades" CssClass="form-select" runat="server" AppendDataBoundItems="true">
											<asp:ListItem Value="" Text="Seleccione una prioridad" Selected="True"></asp:ListItem>
										</asp:DropDownList>
										<asp:RequiredFieldValidator ID="rfvPrioridades"
											ControlToValidate="ddlPrioridades"
											ValidationGroup="AgregarFormulacion"
											runat="server"
											ErrorMessage="Seleccione una prioridad"
											Display="Dynamic"
											CssClass="text-danger"
											EnableClientScript="true"
											InitialValue="" />
									</div>
								</div>

								<div class="col-12">
									<div class="mb-3">
										<label for="txtObservaciones" class="form-label">Observaciones</label>
										<asp:TextBox ID="txtObservaciones" CssClass="form-control" runat="server" TextMode="MultiLine" Rows="3" />
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
						<asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" ValidationGroup="AgregarFormulacion" />
					</div>
				</div>
			</div>
		</div>
	</div>
	<!-- /Modal -->

	<div class="row mt-4 mb-3">
		<div class="col-12">
			<div class="d-flex justify-content-between align-items-end flex-wrap gap-3">
				<!-- Contenedor de Filtros alineados a la izquierda -->
				<div class="d-flex flex-wrap gap-3">
					



				</div>

				<!-- Contenedor de Botones alineados a la derecha -->
				<div class="d-flex gap-3">
					<div class="form-group mb-2">
						<asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control" placeholder="Buscar..."></asp:TextBox>
					</div>
					<div class="form-group mb-2">
						<asp:LinkButton ID="btnFiltrar" runat="server" CssClass="btn btn-primary" OnClick="btnFiltrar_Click"
							data-bs-toggle="tooltip" data-bs-placement="top" title="Filtrar">
                            <i class="bi bi-search"></i>
                        </asp:LinkButton>
					</div>
					<div class="form-group mb-2">
						<asp:LinkButton CssClass="btn btn-primary" ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClick="BtnClearFilters_Click"
							data-bs-toggle="tooltip" data-bs-placement="top" title="Quita todos los filtros">
                            <i class="bi bi-funnel"></i>
                        </asp:LinkButton>
					</div>
					<div class="form-group mb-2">
						<asp:LinkButton ID="btnShowAddModal" runat="server" CssClass="btn btn-primary" OnClick="btnShowAddModal_Click">
                            <i class="bi bi-plus-lg"></i> Agregar
                        </asp:LinkButton>
					</div>
				</div>
			</div>
		</div>
		<hr class="mb-3" />

		<asp:GridView ID="dgvFormulacion" DataKeyNames="ID" CssClass="table1 table-bordered table-hover mb-4"
			OnSelectedIndexChanged="dgvFormulacion_SelectedIndexChanged"
			OnRowDeleting="dgvFormulacion_RowDeleting"
			OnRowDataBound="dgvFormulacion_DataBound"
			AutoGenerateColumns="false" runat="server">
			<Columns>
				<asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />

                <%--<asp:BoundField HeaderText="Área" DataField="Obra.Area.Nombre" />--%>
                <asp:TemplateField HeaderText="Área">
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblsHeaderArea" runat="server"
                            HeaderText="Área"
                            DataTextField="Nombre"
                            DataValueField="Id"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("Obra.Area.Nombre") %>
                    </ItemTemplate>
                </asp:TemplateField>

				<asp:BoundField HeaderText="Empresa" DataField="Obra.Empresa.Nombre" />
				<asp:TemplateField HeaderText="Contrata">
					<ItemTemplate>
						<%# Eval("Obra.Contrata.Nombre") + " " + Eval("Obra.Numero") + "/" + Eval("Obra.Año") %>
					</ItemTemplate>
				</asp:TemplateField>
				<asp:BoundField HeaderText="Barrio" DataField="Obra.Barrio.Nombre" />
				<asp:BoundField HeaderText="Nombre de Obra" DataField="Obra.Descripcion" />
                <%--<asp:BoundField HeaderText="Linea de Gestión" DataField="Obra.LineaGestion.Nombre" />--%>
                <asp:TemplateField HeaderText="Linea de Gestión">
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblsHeaderLineaGestion" runat="server"
                            HeaderText="Línea de Gestión"
                            DataTextField="Nombre"
                            DataValueField="Id"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("Obra.LineaGestion.Nombre") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <%--<asp:BoundField HeaderText="Proyecto" DataField="Obra.Proyecto.Proyecto" />--%>
                <asp:TemplateField HeaderText="Proyecto">
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblsHeaderProyecto" runat="server"
                            HeaderText="Proyecto"
                            DataTextField="Nombre"
                            DataValueField="Id"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("Obra.Proyecto.Proyecto") %>
                    </ItemTemplate>
                </asp:TemplateField>


				<asp:BoundField HeaderText="Plurianual (2026,2027,2028)" DataField="Plurianual" DataFormatString="{0:C}" />

                <%--<asp:BoundField HeaderText="Monto 2026" DataField="Monto_26" DataFormatString="{0:C}" />--%>
                <asp:TemplateField HeaderText="Monto 2026">
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblsHeaderMonto2026" runat="server"
                            HeaderText="Monto 2026"
                            DataTextField="Nombre"
                            DataValueField="Id"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("Monto_26", "{0:C}") %>
                    </ItemTemplate>
                </asp:TemplateField>

				<asp:BoundField HeaderText="Monto 2027" DataField="Monto_27" DataFormatString="{0:C}" />
				<asp:BoundField HeaderText="Monto 2028" DataField="Monto_28" DataFormatString="{0:C}" />
				<asp:BoundField HeaderText="Mes Base" DataField="MesBase" DataFormatString="{0:dd-MM-yyyy}" />
				<asp:BoundField HeaderText="PPI (Presupuesto)" DataField="Ppi" />
				<asp:BoundField HeaderText="Unidad de Medida" DataField="UnidadMedida.Nombre" />
				<asp:BoundField HeaderText="Valor de Medida" DataField="ValorMedida" />

				<asp:BoundField HeaderText="Techos 2026" DataField="Techos2026" DataFormatString="{0:C}" />
				<asp:BoundField HeaderText="Observaciones" DataField="Observacion" />
                <%--<asp:BoundField HeaderText="Prioridad" DataField="Prioridad.Nombre" />--%>
                <asp:TemplateField HeaderText="Prioridad">
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblsHeaderPrioridad" runat="server"
                            HeaderText="Prioridad"
                            DataTextField="Nombre"
                            DataValueField="Id"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("Prioridad.Nombre") %>
                    </ItemTemplate>
                </asp:TemplateField>


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

	<script type="text/javascript">
		function soloNumerosDecimales(e) {
			var key = window.event ? e.which : e.keyCode;
			if (key == 8 || key == 46 || key == 44 || key == 9) return true;
			if (key >= 48 && key <= 57) return true;
			if (key == 110 || key == 190) return true;
			return false;
		}

		function limpiarFormulario() {
			document.getElementById('<%= txtMonto26.ClientID %>').value = '';
			document.getElementById('<%= txtMonto27.ClientID %>').value = '';
			document.getElementById('<%= txtMonto28.ClientID %>').value = '';
			document.getElementById('<%= txtTechos.ClientID %>').value = '';
			document.getElementById('<%= txtPPI.ClientID %>').value = '';
			document.getElementById('<%= txtValorMedida.ClientID %>').value = '';
			document.getElementById('<%= txtMesBase.ClientID %>').value = '';
			document.getElementById('<%= txtObservaciones.ClientID %>').value = '';
			document.getElementById('<%= ddlObra.ClientID %>').selectedIndex = 0;
			document.getElementById('<%= ddlUnidadMedida.ClientID %>').selectedIndex = 0;
			document.getElementById('<%= ddlPrioridades.ClientID %>').selectedIndex = 0;
		}
    </script>
</asp:Content>
