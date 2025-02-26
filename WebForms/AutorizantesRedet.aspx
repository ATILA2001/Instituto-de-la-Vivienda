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
		color: #212529; /* Gris oscuro para un aspecto formal */
		font-weight: 600; /* Peso semibold */
	}

	/* Estilo del botón dropdown */
	.form-group .dropdown-toggle {
		background-color: #f8f9fa; /* Fondo claro */
		color: #212529; /* Texto negro */
		border-radius: 0.375rem; /* Bordes redondeados suaves */
		width: 100%; /* Ocupa todo el ancho */
		text-align: left; /* Alineación del texto a la izquierda */
		font-size: 14px; /* Tamaño de texto claro y sobrio */
		font-weight: normal; /* Si necesitas enfatizar el texto */
	}

		.form-group .dropdown-toggle:hover {
			background-color: #e2e6ea; /* Color de fondo en hover */
			border-color: #adb5bd; /* Color del borde en hover */
			color: #212529;
		}

	/* Estilo del menú desplegable */
	.form-group .dropdown-menu {
		border: 1px solid; /* Mismo borde que el botón */
		border-radius: 0.375rem; /* Bordes redondeados */
		padding: 0.5rem; /* Espaciado interno */
		background-color: #ffffff; /* Fondo blanco */
		max-height: 200px;
		overflow-y: auto; /* Scroll para contenido largo */
	}

		.form-group .dropdown-menu .form-check:hover {
			background-color: transparent;
		}

		/* Opcional: elimina el contorno (outline) en estados de foco */
		.form-group .dropdown-menu .form-check input[type="checkbox"]:focus {
			outline: none;
			box-shadow: none;
		}
		/* Estilo de los items en el dropdown */
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
					color: #495057; /* Mantener el color gris */
					background-color: transparent; /* Sin fondo azul */
					font-weight: normal; /* Opcional, mantiene el estilo regular */
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

	/* Bordes redondeados y sombras en el GridView */
	.table1 {
		border-radius: 10px;
		box-shadow: 0 2px 5px rgba(0, 0, 0, 0.15);
	}

		/* Mejorar el estilo de las celdas del encabezado */
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
		margin-right: 8px; /* Margen entre el campo y el botón */
	}

	.form-control-uniform {
		display: inline-block;
		font-size: 14px; /* Tamaño de texto uniforme */
		padding: 4px 12px;
		border: 1px solid;
	}

	.btn {
		background-color: #153244; /* Azul institucional */
		text-align: center;
		transition: all 0.3s ease-in-out;
		color: #ecf0f1; /* Blanco suave para el texto */
		border: none;
		padding: 8px 12px;
		font-size: 14px;
		cursor: pointer;
		border-radius: 4px; /* Bordes redondeados */
		display: inline-block;
		font-weight: bold; /* Si necesitas enfatizar el texto */
	}

		.btn:hover {
			background-color: #2c3e50; /* Gris oscuro al pasar el cursor */
			color: white;
		}




	.lbl-left {
		text-align: left;
		display: block; /* Asegura que el label ocupe toda la línea si es necesario */
		font-weight: bold; /* Si necesitas enfatizar el texto */
	}


	#visibilityMessage {
		background-color: #FFCC00; /* Fondo más neutro (gris claro) */
		text-align: center; /* Centrar el contenido */
		transition: all 0.3s ease-in-out; /* Suaviza las animaciones */
		color: #ecf0f1; /* Blanco suave para el texto */
	}

	#visibilityText {
		cursor: pointer;
		display: inline-block;
		transition: color 0.3s ease-in-out;
	}

	#visibilityMessage:hover {
		background-color: #FFCC00; /* Gris oscuro cuando se pasa el cursor */
	}


		#visibilityMessage:hover #visibilityText {
			color: white; /* Resaltar cuando se pasa el mouse */
		}
</style>

</asp:Content>
