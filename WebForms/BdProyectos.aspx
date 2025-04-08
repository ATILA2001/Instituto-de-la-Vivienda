<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="BdProyectos.aspx.cs" Inherits="WebForms.BdProyectos" %>

<%@ Register Src="~/CustomControls/CheckBoxListSearch.ascx" TagPrefix="CustomControls" TagName="CheckBoxListSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">


	<div id="section1" style="display: none;">
		<div class="row mt-4">
			<div class="col-md-12">
				<table class="table  table1">
					<thead class="thead-dark" >
						<tr>
							<th>Obra</th>
							<th>Proyecto</th>
							<th>SubProyecto</th>
							<th>Linea de Gestión</th>
							<th>Monto Autorizado Inicial</th>
							<th></th>
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
							<td class="text-right">
								<asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" />
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
							<%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownArea" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownArea" style="max-height: 200px; overflow-y: auto;">
								<!-- Rendimos la CheckBoxList aquí -->
								<asp:CheckBoxList ID="cblArea" runat="server" CssClass="dropdown-item form-check" />
							</ul>--%>
                            <CustomControls:CheckBoxListSearch ID="cblArea" runat="server" />
						</div>
					</div>

					<div class="form-group ">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblProyecto">Proyecto:</label>
						<div class="dropdown">
                            <CustomControls:CheckBoxListSearch ID="cblProyecto" runat="server" />
						</div>
					</div>

					<div class="form-group ">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblLinea">Linea:</label>
						<div class="dropdown">
                            <CustomControls:CheckBoxListSearch ID="cblLinea" runat="server" />
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
							<button class="btn btn-sm btn-secondary" id="visibilityMessage">
								<span id="visibilityText">Agregar Obra</span>
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

			<asp:GridView ID="dgvBdProyecto" DataKeyNames="Id" CssClass="table1  table-bordered table-hover"
				OnSelectedIndexChanged="dgvBdProyecto_SelectedIndexChanged"
				OnRowDeleting="dgvBdProyecto_RowDeleting"
				AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
				<Columns>
					<asp:BoundField HeaderText="Area " DataField="Obra.Area.Nombre" SortExpression="Obra.Area.Nombre"/>

					<asp:BoundField HeaderText="Contrata " DataField="Obra.Contrata.Nombre" SortExpression="Obra.Contrata.Nombre"  />
					<asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" SortExpression="Obra.Descripcion" />
					<asp:BoundField HeaderText="Proyecto" DataField="Proyecto" SortExpression="Proyecto"  />
					<asp:BoundField HeaderText="SubProyecto" DataField="SubProyecto" SortExpression="SubProyecto" />
					<asp:BoundField HeaderText="Linea de Gestión" DataField="LineaGestion.Nombre" SortExpression="LineaGestion.Nombre"/>
					<asp:BoundField HeaderText="Monto Inicial" DataField="AutorizadoInicial" DataFormatString="{0:C}"  />
					<asp:BoundField HeaderText="Monto Nuevo" DataField="AutorizadoNuevo" DataFormatString="{0:C}" />
                    <asp:TemplateField HeaderText="Acciones">
                        <ItemTemplate>
                            <div class="d-flex justify-content-center gap-2">
                                <asp:LinkButton ID="btnModificar" runat="server"
                                    CommandName="Select"
                                    CssClass="btn btn-sm btn-warning text-dark"
                                    ToolTip="Modificar">
                                    <i class="bi bi-pencil-square"></i>
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnEliminar" runat="server"
                                    CommandName="Delete"
                                    CssClass="btn btn-sm btn-danger text-light"
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
		
		</script>

</asp:Content>



