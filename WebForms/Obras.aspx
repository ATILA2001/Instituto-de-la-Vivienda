<%@ Page Title="" Language="C#" MasterPageFile="~/User.Master" AutoEventWireup="true" CodeBehind="Obras.aspx.cs" Inherits="WebForms.Obras" %>

<%@ Register Src="~/CustomControls/CheckBoxListSearch.ascx" TagPrefix="CustomControls" TagName="CheckBoxListSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

	<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<style>
	.table, .table-3d, .table-3d thead, .table-3d tbody, .table-3d tr, .table-3d th, .table-3d td {
		color: #ecf0f1 !important; 
	}

	.table-3d {
		border-radius: 10px; 
		overflow: hidden; 
		border-collapse: collapse;
	}

		.table-3d thead th {
			background-color: #153244;
			color: #ecf0f1;
			font-weight: bold;
			text-align: center;
			text-transform: uppercase;
			border: none;
		}
</style>
	<%--<div id="section1" style="display: none;">
		<div class="row mt-4">
			<div class="col-md-12">
				<table class="table  table-3d">
					<thead class="thead-dark" >
						<tr>
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
									CssClass="btn btn-outline-light" runat="server" />
							</td>
							<td class="text-right">
								<asp:Button Text="Limpiar" ID="btnLimpiar" OnClick="btnLimpiar_Click"
									CssClass="btn btn-outline-light" runat="server" /></td>
						</tr>
					</tbody>
				</table>
			</div>
		</div>
	</div>--%>
    
	<asp:UpdatePanel ID="UpdatePanel1" runat="server">
		<ContentTemplate>


	<div class="row mt-4">
		<div class="col-md-12">
			<div class="text-end">

				<div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">


					<div class="form-group ">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblEmpresa">Empresa:</label>
						<div class="dropdown">
							<%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownEmpresa" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
                   
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownEmpresa" style="max-height: 200px; overflow-y: auto;">
								<!-- Rendimos la CheckBoxList aquí -->
								<asp:CheckBoxList ID="cblEmpresa" runat="server" CssClass="dropdown-item form-check" />
							</ul>
							--%>
							<CustomControls:CheckBoxListSearch ID="cblEmpresa" runat="server" />
						</div>
					</div>

					<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblBarrio">Barrio:</label>
						<div class="dropdown">

<%--							<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownBarrio" data-bs-toggle="dropdown" aria-expanded="false">
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





						<%--<div class="form-group d-flex align-items-end">
							<button class="btn btn-sm btn-outline-dark" id="visibilityMessage">
								<strong id="visibilityText">Agregar Obra</strong>
							</button>
						</div>--%>


						<div class="form-group  d-flex align-items-end">
							<%--<asp:Button CssClass="btn btn-sm btn-outline-dark " ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClientClick="limpiarFiltros();" />--%>
							<asp:Button CssClass="btn btn-sm btn-outline-dark " ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClick="BtnClearFilters_Click" />
						</div>
						<div class="form-group d-flex align-items-end">
							<asp:Button CssClass="btn btn-sm btn-outline-dark" ID="btnFiltrar" Text="Filtrar" runat="server" OnClick="btnFiltrar_Click" />
						</div>
					</div>
				</div>
			</div>

			<hr />
			<asp:GridView ID="dgvObra" DataKeyNames="ID" CssClass="table1  table-bordered table-hover"
				OnSelectedIndexChanged="dgvObra_SelectedIndexChanged"
				OnRowDeleting="dgvObra_RowDeleting"
				AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
				<Columns>
					<asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />
					<asp:BoundField HeaderText="Área" DataField="Area" Visible="false" />
					<asp:BoundField HeaderText="Empresa" DataField="Empresa" />
					<asp:TemplateField HeaderText="Contrata" >
						<ItemTemplate>
							<%# Eval("Contrata") + " " + Eval("Numero") + "/" + Eval("Año") %>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField HeaderText="Etapa" DataField="Etapa"/>
					<asp:BoundField HeaderText="Obra N°" DataField="ObraNumero" />
					<asp:BoundField HeaderText="Barrio" DataField="Barrio" />
					<asp:BoundField HeaderText="Nombre de Obra" DataField="Descripcion"  />
					<asp:BoundField HeaderText="Disponible Actual" DataField="AutorizadoNuevo" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Planificacion 2025" DataField="MontoCertificado" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Ejecucion Presupuesto 2025" DataField="Porcentaje" DataFormatString="{0:N2}%" />

					<asp:BoundField HeaderText="Monto de Obra inicial" DataField="MontoInicial" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Monto de Obra actual" DataField="MontoActual"  DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Faltante de Obra" DataField="MontoFaltante"  DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Fecha Inicio" DataField="FechaInicio"  DataFormatString="{0:dd-MM-yyyy}" />
					<asp:BoundField HeaderText="Fecha Fin" DataField="FechaFin" DataFormatString="{0:dd-MM-yyyy}" />

					<asp:CommandField ShowSelectButton="true" SelectText="Modificar" ControlStyle-CssClass="btn btn-outline-warning"  />
					
				</Columns>
			</asp:GridView>

			<div class="text-center p-4">
				<asp:Label ID="lblMensaje" Text="" runat="server" />
			</div>
		</div>
	</div>
	
		
		</ContentTemplate>
	</asp:UpdatePanel>

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
<style>
	.form-group label {
		font-size: 14px;
		color: #212529;
		font-weight: 600;
	}

	.form-group .dropdown-toggle {
		background-color: #f8f9fa;
		color: #212529;
		border-radius: 0.375rem;
		width: 100%;
		text-align: left;
		font-size: 14px;
		font-weight: normal;
	}

		.form-group .dropdown-toggle:hover {
			background-color: #e2e6ea;
			border-color: #adb5bd;
			color: #212529;
		}

	.form-group .dropdown-menu {
		border: 1px solid;
		border-radius: 0.375rem;
		padding: 0.5rem;
		background-color: #ffffff;
		max-height: 200px;
		overflow-y: auto;
	}

		.form-group .dropdown-menu .form-check:hover {
			background-color: transparent;
		}

		.form-group .dropdown-menu .form-check input[type="checkbox"]:focus {
			outline: none;
			box-shadow: none;
		}

		.form-group .dropdown-menu .form-check {
			margin-bottom: 0.5rem;
		}

			.form-group .dropdown-menu .form-check label {
				font-size: 14px;
				color: #495057;
				background-color: transparent;
			}

			.form-group .dropdown-menu .form-check input[type="checkbox"] {
				margin-right: 8px;
			}

				.form-group .dropdown-menu .form-check input[type="checkbox"]:focus + label,
				.form-group .dropdown-menu .form-check input[type="checkbox"]:checked + label {
					color: #495057;
					background-color: transparent;
					font-weight: normal;
				}

	.table-bordered th, .table-bordered td {
		border: 1px solid #dddddd;
		text-align: center;
	}

	.table-hover tbody tr:hover {
		background-color: #f2f2f2;
	}

	.table1 th, .table1 td {
		padding: 12px;
		font-size: 14px;
	}

	.table1 {
		border-radius: 10px;
		box-shadow: 0 2px 5px rgba(0, 0, 0, 0.15);
	}

		.table1 th {
			text-align: center;
			font-weight: bold;
			background: #153244;
			color: white;
			border: 1px solid #153244;
		}

	.form-label {
		margin-bottom: 0;
	}

	.d-flex.align-items-end > .form-control {
		margin-right: 8px;
	}

	.form-control-uniform {
		display: inline-block;
		font-size: 14px;
		padding: 4px 12px;
		border: 1px solid;
	}

	.btn {
		background-color: #153244;
		text-align: center;
		transition: all 0.3s ease-in-out;
		color: #ecf0f1;
		border: none;
		padding: 8px 12px;
		font-size: 14px;
		cursor: pointer;
		border-radius: 4px;
		display: inline-block;
		font-weight: bold;
	}

		.btn:hover {
			background-color: #8DE2D6;
			color: #153244;
		}

	.lbl-left {
		text-align: left;
		display: block;
		font-weight: bold;
	}

	#visibilityMessage {
		background-color: #8DE2D6;
		text-align: center;
		transition: all 0.3s ease-in-out;
		color: #153244;
	}

	#visibilityText {
		cursor: pointer;
		display: inline-block;
		transition: color 0.3s ease-in-out;
	}

	#visibilityMessage:hover {
		background-color: #153244;
	}

		#visibilityMessage:hover #visibilityText {
			color: white;
		}
</style>

</asp:Content>



