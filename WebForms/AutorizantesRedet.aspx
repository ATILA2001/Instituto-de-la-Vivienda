<%@ Page Title="" Language="C#" MasterPageFile="~/Redet.Master" AutoEventWireup="true" CodeBehind="AutorizantesRedet.aspx.cs" Inherits="WebForms.AutorizantesRedet" %>

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


	<div class="row mt-4">
		<div class="col-md-12">
			<div class="text-end">


				<div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">
					<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblEmpresa">Empresa:</label>
						<div class="dropdown">
							<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownEmpresa" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownEmpresa" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblEmpresa" runat="server" CssClass="dropdown-item form-check" />
							</ul>
						</div>
					</div>

					<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblObra">Obra:</label>
						<div class="dropdown">
							<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownObra" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownObra" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblObra" runat="server" CssClass="dropdown-item form-check" />
							</ul>
						</div>
					</div>


					<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblEstado">Estado:</label>
						<div class="dropdown">
							<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownEstado" data-bs-toggle="dropdown" aria-expanded="false">
								Todos
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownEstado" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblEstado" runat="server" CssClass="dropdown-item form-check" />
							</ul>
						</div>
					</div>

					<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblConcepto">Concepto:</label>
						<div class="dropdown">
							<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownConcepto" data-bs-toggle="dropdown" aria-expanded="false">
								Todos
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownConcepto" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblConcepto" runat="server" CssClass="dropdown-item form-check" />
							</ul>
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

						<div class="form-group  d-flex align-items-end">
							<asp:Button CssClass="btn btn-sm btn-outline-dark " ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClientClick="limpiarFiltros();" />
						</div>
						<div class="form-group d-flex align-items-end">
							<asp:Button CssClass="btn btn-sm btn-outline-dark" ID="btnFiltrar" Text="Filtrar" runat="server" OnClick="btnFiltrar_Click" />
						</div>
					</div>
				</div>
			</div>
			<hr />

			<asp:GridView ID="dgvAutorizante" DataKeyNames="CodigoAutorizante" CssClass="table1  table-bordered table-hover "
				AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
				<Columns>
					<asp:BoundField HeaderText="Obra" DataField="Obra.Id" />

					<asp:BoundField HeaderText="Área" DataField="Obra.Area.Nombre"  />

					<asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" />
					<asp:BoundField HeaderText="Contrata" DataField="Obra.Contrata.Nombre"  />
					<asp:BoundField HeaderText="Empresa" DataField="Empresa" />

					<asp:BoundField HeaderText="Código Autorizante" DataField="CodigoAutorizante"/>
					<asp:BoundField HeaderText="Concepto" DataField="Concepto.Nombre" />
					<asp:BoundField HeaderText="Detalle" DataField="Detalle" />

					<asp:BoundField HeaderText="Expediente" DataField="Expediente"  />

					<asp:BoundField HeaderText="Estado" DataField="Estado"/>

					<asp:BoundField HeaderText="Monto Autorizado" DataField="MontoAutorizado" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Mes Aprobacion" DataField="Fecha" DataFormatString="{0:dd-MM-yyyy}" />
					<asp:BoundField HeaderText="Mes Base" DataField="MesBase" DataFormatString="{0:dd-MM-yyyy}" />

					<asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade" />
					<asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" />

					<asp:TemplateField HeaderText="Autorización GG" >
						<ItemTemplate>
							<%# Eval("AutorizacionGG") != null && (bool)Eval("AutorizacionGG") ? "AUTORIZADO" : "A AUTORIZAR" %>
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
			function actualizarSeleccion(checkBoxListId, dropdownId, localStorageKey) {
				var seleccionados = [];
				var $checkBoxList = $('#' + checkBoxListId);
				var $dropdown = $('#' + dropdownId);

				// Procesar checkboxes seleccionados
				$checkBoxList.find('input[type=checkbox]:checked').each(function () {
					seleccionados.push($(this).next('label').text());
				});

				// Guardar en localStorage
				localStorage.setItem(localStorageKey, JSON.stringify(seleccionados));

				// Actualizar el texto del botón
				var textoBoton = seleccionados.length > 0 ? seleccionados.length + ' seleccionado' + (seleccionados.length > 1 ? 's' : '') : 'Sin seleccionar';
				$dropdown.html(textoBoton);
			}

			// Inicializar para empresas
			var empresasSeleccionadas = JSON.parse(localStorage.getItem('selectedEmpresas')) || [];
			actualizarSeleccion('<%= cblEmpresa.ClientID %>', 'dropdownEmpresa', 'selectedEmpresas');


			$('#<%= cblEmpresa.ClientID %> input[type=checkbox]').on('change', function () {
				actualizarSeleccion('<%= cblEmpresa.ClientID %>', 'dropdownEmpresa', 'selectedEmpresas');
			});

			// Inicializar para obra 
			var obraSeleccionados = JSON.parse(localStorage.getItem('selectedObras')) || [];
			actualizarSeleccion('<%= cblObra.ClientID %>', 'dropdownObra', 'selectedObras');

			$('#<%= cblObra.ClientID %> input[type=checkbox]').on('change', function () {
				actualizarSeleccion('<%= cblObra.ClientID %>', 'dropdownObra', 'selectedObras');
			});
			// Inicializar para  estado 
			var estadoSeleccionados = JSON.parse(localStorage.getItem('selectedEstados')) || [];
			actualizarSeleccion('<%= cblEstado.ClientID %>', 'dropdownEstado', 'selectedEstados');

			$('#<%= cblEstado.ClientID %> input[type=checkbox]').on('change', function () {
				actualizarSeleccion('<%= cblEstado.ClientID %>', 'dropdownEstado', 'selectedEstados');
			});
			// Inicializar para  concepto
			var conceptoSeleccionados = JSON.parse(localStorage.getItem('selectedConceptos')) || [];
			actualizarSeleccion('<%= cblConcepto.ClientID %>', 'dropdownConcepto', 'selectedConcepto');

			$('#<%= cblConcepto.ClientID %> input[type=checkbox]').on('change', function () {
				actualizarSeleccion('<%= cblConcepto.ClientID %>', 'dropdownConcepto', 'selectedConcepto');
			});



			// Limpiar filtros
			$('#<%= btnLimpiarFiltros.ClientID %>').on('click', function () {
				// Limpiar localStorage
				localStorage.removeItem('selectedEmpresas');
				localStorage.removeItem('selectedObras');
				localStorage.removeItem('selectedEstados');
				localStorage.removeItem('selectedConceptos');

				// Restablecer checkboxes
				$('#<%= cblEmpresa.ClientID %> input[type=checkbox]').prop('checked', false);
				$('#<%= cblObra.ClientID %> input[type=checkbox]').prop('checked', false);
				$('#<%= cblEstado.ClientID %> input[type=checkbox]').prop('checked', false);
				$('#<%= cblConcepto.ClientID %> input[type=checkbox]').prop('checked', false);
				// Restablecer texto de los botones

				$('#dropdownEmpresa').text('Todas');
				$('#dropdownObra').text('Todas');
				$('#dropdownEstado').text('Todos');
				$('#dropdownConcepto').text('Todos');

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
