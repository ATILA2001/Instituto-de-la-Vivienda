<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AutorizantesAdmin.aspx.cs" Inherits="WebForms.AutorizantesAdmin" %>

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

	<%--	<div id="section1" style="display: none;">
		<div class="row mt-4">
			<div class="col-md-12">
				<table class="table  table-3d">
					<thead class="thead-dark" style="color: #fad404">
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
								<asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-outline-success" runat="server" />
							</td>
							<td class="text-right">
								<asp:Button Text="Limpiar" ID="btnLimpiar" OnClick="btnLimpiar_Click"
									CssClass="btn btn-outline-secondary ml-2" runat="server" />
							</td>
						</tr>
					</tbody>
				</table>
			</div>
		</div>
	</div>
	--%>

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

						<div class="form-group d-flex align-items-end">
							<button class="btn btn-sm btn-outline-dark" id="visibilityMessage">
								<strong id="visibilityText">Agregar Autorizante</strong>
							</button>
						</div>
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
				OnSelectedIndexChanged="dgvAutorizante_SelectedIndexChanged"
				OnRowDeleting="dgvAutorizante_RowDeleting" OnRowDataBound="dgvAutorizante_RowDataBound"
				AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
				<Columns>
					<asp:BoundField HeaderText="Obra" DataField="Obra.Id" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" Visible="false" />

					<asp:BoundField HeaderText="Área" DataField="Obra.Area.Nombre" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

					<asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Contrata" DataField="Obra.Contrata.Nombre" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Empresa" DataField="Empresa" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

					<asp:BoundField HeaderText="Código Autorizante" DataField="CodigoAutorizante" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Concepto" DataField="Concepto.Nombre" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Detalle" DataField="Detalle" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:TemplateField HeaderText="Expediente" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center">
						<ItemTemplate>
							<asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("Expediente") %>' AutoPostBack="true"
								OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm" Style="background-color: white !important; color: #34495e !important; font-weight: normal; padding: 8px 12px; font-size: 14px;">
							</asp:TextBox>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="Estado" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center">
						<ItemTemplate>
							<asp:DropDownList ID="ddlEstadoAutorizante" runat="server" AutoPostBack="true"
								OnSelectedIndexChanged="ddlEstadoAutorizante_SelectedIndexChanged" class="btn btn-sm dropdown-toggle" Style="background-color: white !important; color: #34495e !important; font-weight: normal; padding: 8px 12px; font-size: 14px;">
							</asp:DropDownList>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField HeaderText="Monto Autorizado" DataField="MontoAutorizado" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Mes Aprobacion" DataField="Fecha" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Mes Base" DataField="MesBase" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

					<asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

					<asp:TemplateField HeaderText="Autorización GG" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center">
						<ItemTemplate>
							<%# Eval("AutorizacionGG") != null && (bool)Eval("AutorizacionGG") ? "AUTORIZADO" : "A AUTORIZAR" %>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:CommandField ShowSelectButton="true" SelectText="Modificar" ControlStyle-CssClass="btn btn-sm btn-outline-warning" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-sm btn-outline-danger" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
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
