<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ObrasAdmin.aspx.cs" Inherits="WebForms.ObrasAdmin" %>

<%@ Register Src="~/CustomControls/CheckBoxListSearch.ascx" TagPrefix="CustomControls" TagName="CheckBoxListSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

	<div id="section1" style="display: none;">
		<div class="row mt-4">
			<div class="col-md-12">
				<table class="table  table-3d">
					<thead class="thead-dark" >
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
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblEmpresa">Empresa:</label>
						<div class="dropdown">
							<%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownEmpresa" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
               
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownEmpresa" style="max-height: 200px; overflow-y: auto;">
								<!-- Rendimos la CheckBoxList aquí -->
								<asp:CheckBoxList ID="cblEmpresa" runat="server" CssClass="dropdown-item form-check" />
							</ul>--%>
							<CustomControls:CheckBoxListSearch ID="cblEmpresa" runat="server" />
						</div>
					</div>

					<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblBarrio">Barrio:</label>
						<div class="dropdown">
							<%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownBarrio" data-bs-toggle="dropdown" aria-expanded="false">
								Todos
               
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownBarrio" style="max-height: 200px; overflow-y: auto;">
								<!-- Rendimos la CheckBoxList aquí -->
								<asp:CheckBoxList ID="cblBarrio" runat="server" CssClass="dropdown-item form-check" />
							</ul>--%>
                            <CustomControls:CheckBoxListSearch ID="cblBarrio" runat="server" />
						</div>
					</div>

					<div class="form-group text-left" style="flex: 1; max-width: 300px;">
						<label class="form-label lbl-left" for="txtBuscar">Buscar:</label>
						<asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control form-control-uniform"></asp:TextBox>
					</div>

					<div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">





						<div class="form-group d-flex align-items-end">
							<button class="btn btn-sm btn-primary" id="visibilityMessage">
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


			<asp:GridView ID="dgvObra" DataKeyNames="ID" CssClass="table1  table-bordered table-hover"
				OnSelectedIndexChanged="dgvObra_SelectedIndexChanged"
				OnRowDeleting="dgvObra_RowDeleting"
				AutoGenerateColumns="false" runat="server"
				Style="display: block; overflow-x: auto;">

				<Columns>
					<asp:BoundField HeaderText="ID" DataField="Id" />
					<asp:BoundField HeaderText="Área" DataField="Area"  />
					<asp:BoundField HeaderText="Empresa" DataField="Empresa"/>
					<asp:TemplateField HeaderText="Contrata" >
						<ItemTemplate>
							<%# Eval("Contrata") + " " + Eval("Numero") + "/" + Eval("Año") %>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField HeaderText="Barrio" DataField="Barrio" />
					<asp:BoundField HeaderText="Nombre de Obra" DataField="Descripcion" />
					<asp:BoundField HeaderText="Linea de Gestion" DataField="Linea"  />
					<asp:BoundField HeaderText="Proyecto" DataField="Proyecto"  />

					<asp:BoundField HeaderText="Disponible Actual" DataField="AutorizadoNuevo" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Planificacion 2025" DataField="MontoCertificado"  DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Ejecucion Presupuesto 2025" DataField="Porcentaje" DataFormatString="{0:N2}%" />

					<asp:BoundField HeaderText="Monto de Obra inicial" DataField="MontoInicial"  DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Monto de Obra actual" DataField="MontoActual" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Faltante de Obra" DataField="MontoFaltante"  DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Fecha Inicio" DataField="FechaInicio" DataFormatString="{0:dd-MM-yyyy}" />
					<asp:BoundField HeaderText="Fecha Fin" DataField="FechaFin"  DataFormatString="{0:dd-MM-yyyy}" />

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
		
	</script>
	

</asp:Content>


