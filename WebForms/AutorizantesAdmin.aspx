<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AutorizantesAdmin.aspx.cs" Inherits="WebForms.AutorizantesAdmin" %>

<%@ Register Src="~/CustomControls/CheckBoxListSearch.ascx" TagPrefix="CustomControls" TagName="CheckBoxListSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	
	<div id="section1" style="display: none;">
		<div class="row mt-4">
			<div class="col-md-12">
				<table class="table  table-3d">
					<thead class="thead-dark">
						<tr>
							<th>Obra</th>
							<th>Concepto</th>
							<th>Detalle</th>
							<th>Expediente</th>
							<th>Estado</th>
							<th>Monto Autorizado</th>
							<th>Mes Aprobacion</th>
							<th>Mes Base</th>

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
								<asp:DropDownList ID="ddlConcepto" CssClass="form-control" runat="server"></asp:DropDownList>
							</td>
							<td>
								<asp:TextBox ID="txtDetalle" CssClass="form-control" runat="server" />
							</td>
							<td>
								<asp:TextBox ID="txtExpediente" CssClass="form-control" runat="server" />
							</td>
							<td>
								<asp:DropDownList ID="ddlEstado" CssClass="form-control" runat="server"></asp:DropDownList>
							</td>
							<td>
								<asp:TextBox ID="txtMontoAutorizado" CssClass="form-control" runat="server" />
							</td>
							<td>
								<asp:TextBox ID="txtFecha" CssClass="form-control" runat="server" TextMode="Date" />
							</td>
							<td>
								<asp:TextBox ID="txtMes" CssClass="form-control" runat="server" TextMode="Date" />
							</td>
							<td class="text-right">
								<asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" />
							</td>
							<td class="text-right">
								<asp:Button Text="Limpiar" ID="btnLimpiar" OnClick="btnLimpiar_Click"
									CssClass="btn btn-primary" runat="server" />
							</td>
						</tr>
					</tbody>
				</table>
			</div>
		</div>
	</div>


	<div class="row mt-4">
		<div class="col-md-12">
			<div class="text-end">
				<div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">

					<div class="form-group ">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblArea">Area:</label>
						<div class="dropdown">
							<CustomControls:CheckBoxListSearch ID="cblArea" runat="server" />
						</div>
					</div>

					<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblObra">Obra:</label>
						<div class="dropdown">
							<CustomControls:CheckBoxListSearch ID="cblObra" runat="server" />
						</div>
					</div>

					<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblEmpresa">Empresa:</label>
						<div class="dropdown">
							<CustomControls:CheckBoxListSearch ID="cblEmpresa" runat="server" />
						</div>
					</div>

					<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblConcepto">Concepto:</label>
						<div class="dropdown">
							<CustomControls:CheckBoxListSearch ID="cblConcepto" runat="server" />
						</div>
					</div>

					<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblEstado">Estado:</label>
						<div class="dropdown">
							<%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownEstado" data-bs-toggle="dropdown" aria-expanded="false">
								Todos
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownEstado" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblEstado" runat="server" CssClass="dropdown-item form-check" />
							</ul>--%>
							<CustomControls:CheckBoxListSearch ID="cblEstado" runat="server" />
						</div>
					</div>




					<div class="form-group text-left" style="flex: 1; max-width: 300px;">
						<label class="form-label lbl-left" for="txtBuscar">Buscar:</label>
						<asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control form-control-uniform"></asp:TextBox>
					</div>

					<div class="form-group text-left" style="flex: 1; max-width: 300px;">
						<label class="form-label lbl-left" for="txtSubtotal">Subtotal:</label>
						<asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control form-control-uniform" ReadOnly="true" />
					</div>

					<div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">

						<div class="form-group d-flex align-items-end">
							<button class="btn btn-sm btn-primary" id="visibilityMessage">
								<strong id="visibilityText">Agregar Autorizante</strong>
							</button>
						</div>
						<div class="form-group  d-flex align-items-end">
							<%--<asp:Button CssClass="btn btn-sm btn-primary " ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClientClick="limpiarFiltros();" />--%>
							<asp:Button CssClass="btn btn-sm btn-primary " ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClick="BtnClearFilters_Click" />
						</div>
						<div class="form-group d-flex align-items-end">
							<asp:Button CssClass="btn btn-sm btn-primary" ID="btnFiltrar" Text="Filtrar" runat="server" OnClick="btnFiltrar_Click" />
						</div>
					</div>
				</div>
			</div>

			<hr />

			<asp:GridView ID="dgvAutorizante" DataKeyNames="CodigoAutorizante" CssClass="table1  table-bordered table-hover "
				OnSelectedIndexChanged="dgvAutorizante_SelectedIndexChanged"
				OnRowDeleting="dgvAutorizante_RowDeleting" OnRowDataBound="dgvAutorizante_RowDataBound"
				AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
				<Columns>
					<asp:BoundField HeaderText="Obra" DataField="Obra.Id" Visible="false" />

					<asp:BoundField HeaderText="Área" DataField="Obra.Area.Nombre" />

					<asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" />
					<asp:BoundField HeaderText="Contrata" DataField="Obra.Contrata.Nombre" />
					<asp:BoundField HeaderText="Empresa" DataField="Empresa" />

					<asp:BoundField HeaderText="Código Autorizante" DataField="CodigoAutorizante" />
					<asp:BoundField HeaderText="Concepto" DataField="Concepto.Nombre" />
					<asp:BoundField HeaderText="Detalle" DataField="Detalle" />
					<asp:TemplateField HeaderText="Expediente">
						<ItemTemplate>
							<asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("Expediente") %>' AutoPostBack="true"
								OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm" Style="background-color: white !important; color: #34495e !important; font-weight: normal; padding: 8px 12px; font-size: 14px;">
							</asp:TextBox>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="Estado">
						<ItemTemplate>
							<asp:DropDownList ID="ddlEstadoAutorizante" runat="server" AutoPostBack="true"
								OnSelectedIndexChanged="ddlEstadoAutorizante_SelectedIndexChanged" class="btn btn-sm dropdown-toggle" Style="background-color: white !important; color: #34495e !important; font-weight: normal; padding: 8px 12px; font-size: 14px;">
							</asp:DropDownList>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField HeaderText="Monto Autorizado" DataField="MontoAutorizado" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Mes Aprobacion" DataField="Fecha" DataFormatString="{0:dd-MM-yyyy}" />
					<asp:BoundField HeaderText="Mes Base" DataField="MesBase" DataFormatString="{0:dd-MM-yyyy}" />

					<asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade" HeaderStyle-ForeColor="White" />
					<asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" />

					<asp:TemplateField HeaderText="Autorización GG">
						<ItemTemplate>
							<%# Eval("AutorizacionGG") != null && (bool)Eval("AutorizacionGG") ? "AUTORIZADO" : "A AUTORIZAR" %>
						</ItemTemplate>
					</asp:TemplateField>
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
			</asp:GridView>


			<div class="text-center p-4">
				<asp:Label ID="lblMensaje" Text="" runat="server" />
			</div>
		</div>
	</div>
	

	<script type="text/javascript">
	
		$(document).ready(function () {
			// Inicializamos la visibilidad según el valor de localStorage
			var sectionVisible = localStorage.getItem("sectionVisible");

			// Si está marcado como 'true', mostramos la sección
			if (sectionVisible === "true") {
				$('#section1').show(); // Mostramos la sección
				$('#visibilityText').text("Ocultar sección"); // Texto cuando la sección es visible
			} else {
				$('#section1').hide(); // Ocultamos la sección
				$('#visibilityText').text("Agregar Autorizante"); // Texto cuando la sección está oculta
			}

			// Manejar clic en el mensaje para alternar el estado de visibilidad
			$(document).on('click', '#visibilityMessage', function () {
				// Cambiamos el valor de visibilidad
				var currentStatus = $('#visibilityText').text();

				if (currentStatus === "Agregar Autorizante") {
					// Si está oculto, lo mostramos
					localStorage.setItem("sectionVisible", "true");
					$('#section1').show(); // Mostramos la sección
					$('#visibilityText').text("Ocultar sección"); // Cambiar el texto
				} else {
					// Si está visible, lo ocultamos
					localStorage.setItem("sectionVisible", "false");
					$('#section1').hide(); // Ocultamos la sección
					$('#visibilityText').text("Agregar Autorizante"); // Cambiar el texto
				}
			});
		});


	</script>
	
</asp:Content>
