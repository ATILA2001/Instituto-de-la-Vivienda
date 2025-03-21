﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="CertificadosAdmin.aspx.cs" Inherits="WebForms.CertificadosAdmin" %>

<%@ Register Src="~/CustomControls/CheckBoxListSearch.ascx" TagPrefix="CustomControls" TagName="CheckBoxListSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
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
	<div id="section1" style="display: none;">
		<div class="row mt-4">
			<div class="col-md-12">
				<table class="table  table-3d">
					<thead class="thead-dark" >
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
							<customcontrols:checkboxlistsearch id="cblArea" runat="server" />
						</div>
					</div>

					<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblEmpresa">Empresa:</label>
						<div class="dropdown">
							<%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownEmpresa" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownEmpresa" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblEmpresa" runat="server" CssClass="dropdown-item form-check" />
							</ul>--%>
                            <CustomControls:CheckBoxListSearch ID="cblEmpresa" runat="server" />
						</div>
					</div>

					<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblAutorizante">Autorizante:</label>
						<div class="dropdown">
							<%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownAutorizante" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownAutorizante" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblAutorizante" runat="server" CssClass="dropdown-item form-check" />
							</ul>--%>
                            <CustomControls:CheckBoxListSearch ID="cblAutorizante" runat="server" />
						</div>
					</div>

					<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblTipo">Tipo:</label>
						<div class="dropdown">
							<%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownTipo" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownTipo" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblTipo" runat="server" CssClass="dropdown-item form-check" />
							</ul>--%>
                            <CustomControls:CheckBoxListSearch ID="cblTipo" runat="server" />
						</div>
					</div>

					<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblFecha">Fecha:</label>
						<div class="dropdown">
							<%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownFecha" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownFecha" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblFecha" runat="server" CssClass="dropdown-item form-check" />
							</ul>--%>
                            <CustomControls:CheckBoxListSearch ID="cblFecha" runat="server" />
						</div>
					</div>
					<div class="form-group">
						<label class="form-label lbl-left" for="cblEstadoExpediente">Estado:</label>
						<%--<asp:DropDownList ID="ddlExpediente" runat="server" CssClass="form-control form-control-sm">
							<asp:ListItem Text="Todos" Value="" />
							<asp:ListItem Text="Vacío" Value="vacio" />
							<asp:ListItem Text="No Vacío" Value="novacio" />
						</asp:DropDownList>--%>
                        <CustomControls:CheckBoxListSearch ID="cblEstadoExpediente" runat="server" />
					</div>

					<div class="form-group text-left" style="flex: 1; max-width: 300px;">
						<label class="form-label lbl-left" for="txtBuscar">Buscar:</label>
						<asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control form-control-uniform"></asp:TextBox>
					</div>


					<div class="form-group text-left" style="flex: 3; max-width: 300px;">
						<label class="form-label lbl-left" for="txtSubtotal">Subtotal:</label>
						<asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control form-control-uniform" ReadOnly="true" />
					</div>




					<div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">


						<div class="form-group d-flex align-items-end">
							<button class="btn btn-sm btn-outline-dark" id="visibilityMessage">
								<strong id="visibilityText">Agregar Certificado</strong>
							</button>
						</div>

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
			<asp:GridView ID="dgvCertificado" DataKeyNames="ID" CssClass="table1  table-bordered table-hover "
				OnSelectedIndexChanged="dgvCertificado_SelectedIndexChanged"
				OnRowDeleting="dgvCertificado_RowDeleting"
				AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
				<Columns>
					<asp:BoundField HeaderText="ID" DataField="Autorizante.Id" Visible="false" />

					<asp:BoundField HeaderText="Area" DataField="Autorizante.Obra.Area.Nombre" />
					<asp:BoundField HeaderText="Obra" DataField="Autorizante.Obra.Descripcion" />
					<asp:BoundField HeaderText="Contrata" DataField="Autorizante.Obra.Contrata.Nombre" />
					<asp:BoundField HeaderText="Detalle" DataField="Autorizante.Detalle" />

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

					<asp:CommandField ShowSelectButton="true" SelectText="Modificar" ControlStyle-CssClass="btn  btn-sm btn-outline-warning" />
					<asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-sm btn-outline-danger" />
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
