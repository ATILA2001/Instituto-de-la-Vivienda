<%@ Page Title="" Language="C#" MasterPageFile="~/Redet.Master" AutoEventWireup="true" CodeBehind="Redeterminaciones.aspx.cs" Inherits="WebForms.Redeterminaciones" %>

<%@ Register Src="~/CustomControls/CheckBoxListSearch.ascx" TagPrefix="CustomControls" TagName="CheckBoxListSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

	<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

	<%--	<div id="section1" style="display: none;">
		<div class="row mt-4">
			<div class="col-md-12">
				<table class="table  table1">
					<thead class="thead-dark">
						<tr>
							<th>Código Autorizante</th>
							<th>Expediente</th>
							<th>Salto</th>
							<th>Nro</th>
							<th>Tipo</th>
							<th>Etapa</th>
							<th>Porcentaje</th>
							<th>Observaciones</th>
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
								<asp:TextBox ID="txtSalto" CssClass="form-control" runat="server" TextMode="Date" />
							</td>

							<td>
								<asp:TextBox ID="txtNro" CssClass="form-control" runat="server" />
							</td>

							<td>
								<asp:TextBox ID="txtTipo" CssClass="form-control" runat="server"></asp:TextBox>
							</td>
							<td>
								<asp:DropDownList ID="ddlEtapa" CssClass="form-control" runat="server"></asp:DropDownList>
							</td>
							<td>
								<asp:TextBox ID="txtPorcentaje" CssClass="form-control" runat="server" />
							</td>
							<td>
								<asp:TextBox ID="txtObservacion" CssClass="form-control" runat="server" />
							</td>
							<td class="text-right">
								<asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-outline-success" runat="server" />
							</td>
							<td class="text-right">
								<asp:Button Text="Limpiar" ID="btnLimpiar" OnClick="btnLimpiar_Click"
									CssClass="btn btn-outline-secondary ml-2" runat="server" /></td>
						</tr>
					</tbody>
				</table>
			</div>
		</div>
	</div>--%>

	<div class="row mt-4 mb-3">
		<div class="col-12">
			<div class="d-flex justify-content-between align-items-end flex-wrap gap-3">
				<!-- Contenedor de Filtros alineados a la izquierda -->
				<div class="d-flex flex-wrap gap-3">

					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblObra">Obra:</label>
						<div>
							<CustomControls:CheckBoxListSearch ID="cblObra" runat="server" />
						</div>
					</div>

					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblAutorizante">Autorizante:</label>
						<div>
							<CustomControls:CheckBoxListSearch ID="cblAutorizante" runat="server" />
						</div>
					</div>

					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblEtapa">Estado:</label>
						<div>

							<CustomControls:CheckBoxListSearch ID="cblEtapa" runat="server" />
						</div>
					</div>

				</div>

				<%--<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblFecha">Fecha:</label>
						<div class="dropdown">
							<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownFecha" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownFecha" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblFecha" runat="server" CssClass="dropdown-item form-check" />
							</ul>
						</div>
					</div>--%>
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

		<asp:GridView ID="dgvRedeterminacion" DataKeyNames="ID" CssClass="table1 table-bordered table-hover  mb-4"
			OnSelectedIndexChanged="dgvRedeterminacion_SelectedIndexChanged"
			OnRowDeleting="dgvRedeterminacion_RowDeleting"
			OnRowDataBound="dgvRedeterminacion_RowDataBound"
			AutoGenerateColumns="false" runat="server">
			<Columns>
				<asp:BoundField HeaderText="ID" DataField="ID" Visible="false" />
				<asp:BoundField HeaderText="Obra" DataField="Autorizante.Obra.Descripcion" />
				<asp:BoundField HeaderText="Código Autorizante" DataField="CodigoRedet" />
				<asp:BoundField HeaderText="Etapa" DataField="Etapa.Nombre" />
				<asp:TemplateField HeaderText="Etapa">
					<ItemTemplate>
						<asp:DropDownList ID="ddlEtapas" runat="server" AutoPostBack="true"
							OnSelectedIndexChanged="ddlEtapas_SelectedIndexChanged" class="btn btn-sm dropdown-toggle" Style="background-color: white !important; color: #34495e !important; font-weight: normal; padding: 8px 12px; font-size: 14px;">
						</asp:DropDownList>
					</ItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField HeaderText="Expediente">
					<ItemTemplate>
						<asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("Expediente") %>' AutoPostBack="true"
							OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm"></asp:TextBox>
					</ItemTemplate>
				</asp:TemplateField>
				<asp:BoundField HeaderText="Tipo" DataField="Tipo" />
				<asp:BoundField HeaderText="Salto" DataField="Salto" DataFormatString="{0:dd-MM-yyyy}" />
				<asp:BoundField HeaderText="Porcentaje" DataField="Porcentaje" />
				<asp:BoundField HeaderText="Observaciones" DataField="Observaciones" />
				<asp:BoundField HeaderText="Empresa" DataField="Empresa" />
				<asp:BoundField HeaderText="Área" DataField="Area" />
				<asp:BoundField HeaderText="Buzon SADE" DataField="BuzonSade" />
				<asp:BoundField HeaderText="Fecha SADE" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" />

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
	</div>


</asp:Content>
