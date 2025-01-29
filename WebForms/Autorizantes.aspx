<%@ Page Title="" Language="C#" MasterPageFile="~/User.Master" AutoEventWireup="true" CodeBehind="Autorizantes.aspx.cs" Inherits="WebForms.Autorizantes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
	<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<style>
		.table-3d {
			box-shadow: 0 10px 15px rgba(0, 0, 0, 0.2), 0 4px 6px rgba(0, 0, 0, 0.1);
			transform: translateY(-5px);
			transition: transform 0.3s ease, box-shadow 0.3s ease;
			border-radius: 10px;
			overflow: hidden;
		}

			.table-3d:hover {
				box-shadow: 0 15px 25px rgba(0, 0, 0, 0.3), 0 8px 10px rgba(0, 0, 0, 0.2);
				transform: translateY(-10px);
			}
	</style>

	<div id="section1" class="container-fluid mt-4" style="display: none;">
		<div class="row">
			<div class="col-md-12  rounded-3 p-3">
				<div class="mx-auto p-2">
					<div class="card-body">
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
		</div>
	</div>
	<hr />
	<div class="alert" id="visibilityMessage" role="alert">
		<strong id="visibilityText">Agregar Autorizante</strong>
	</div>




	<div class="row mt-4">
		<div class="col-md-12">
			<div class="text-end">
				<div class="d-flex flex-wrap justify-content-between p-3 gap-3">

					<div class="form-group text-left" style="flex: 1; max-width: 300px;">
						<label class="form-label lbl-left" for="txtSubtotal">Subtotal:</label>
						<asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control form-control-uniform" ReadOnly="true" />
					</div>

					<div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">

						<div class="form-group">
							<label class="form-label lbl-left" for="cblEmpresa">Empresa:</label>
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
							<label class="form-label lbl-left" for="cblObra">Obra:</label>
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
							<label class="form-label lbl-left" for="cblEstado">Estado:</label>
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
							<label class="form-label lbl-left" for="cblConcepto">Concepto:</label>
							<div class="dropdown">
								<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownConcepto" data-bs-toggle="dropdown" aria-expanded="false">
									Todos
								</button>
								<ul class="dropdown-menu p-2" aria-labelledby="dropdownConcepto" style="max-height: 200px; overflow-y: auto;">
									<asp:CheckBoxList ID="cblConcepto" runat="server" CssClass="dropdown-item form-check" />
								</ul>
							</div>
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
			<asp:GridView ID="dgvAutorizante" DataKeyNames="CodigoAutorizante" CssClass="table1  table-bordered table-hover "
				OnSelectedIndexChanged="dgvAutorizante_SelectedIndexChanged"
				OnRowDeleting="dgvAutorizante_RowDeleting" OnRowDataBound="dgvAutorizante_RowDataBound"
				AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
				<columns>
					<asp:BoundField HeaderText="Obra" DataField="Obra.Id" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" Visible="false" />

					<asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Contrata" DataField="Obra.Contrata.Nombre" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Empresa" DataField="Empresa" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

					<asp:BoundField HeaderText="Código Autorizante" DataField="CodigoAutorizante" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Concepto" DataField="Concepto.Nombre" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Detalle" DataField="Detalle" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:TemplateField HeaderText="Expediente" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center">
						<itemtemplate>
							<asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("Expediente") %>' AutoPostBack="true"
								OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm">
							</asp:TextBox>
						</itemtemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="Estado" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center">
						<itemtemplate>
							<asp:DropDownList ID="ddlEstadoAutorizante" runat="server" AutoPostBack="true"
								OnSelectedIndexChanged="ddlEstadoAutorizante_SelectedIndexChanged" class="btn btn-sm dropdown-toggle">
							</asp:DropDownList>
						</itemtemplate>
					</asp:TemplateField>
					<asp:BoundField HeaderText="Monto Autorizado" DataField="MontoAutorizado" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Mes Aprobacion" DataField="Fecha" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

					<asp:TemplateField HeaderText="Autorización GG" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center">
						<itemtemplate>
							<%# Eval("AutorizacionGG") != null && (bool)Eval("AutorizacionGG") ? "AUTORIZADO" : "A AUTORIZAR" %>
						</itemtemplate>
					</asp:TemplateField>
					<asp:CommandField ShowSelectButton="true" SelectText="Modificar" ControlStyle-CssClass="btn btn-outline-warning" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-outline-danger" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
				</columns>
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
				$dropdown.text(textoBoton);
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
				border: 1px solid; /* Borde gris claro */
				border-radius: 0.375rem; /* Bordes redondeados suaves */
				width: 100%; /* Ocupa todo el ancho */
				text-align: left; /* Alineación del texto a la izquierda */
				font-size: 14px; /* Tamaño de texto claro y sobrio */
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
				background: #f1c40f;
				color: white;
				border: 1px solid #f39c11;
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
			border: 1px solid;
		}



		.lbl-left {
			text-align: left;
			display: block; /* Asegura que el label ocupe toda la línea si es necesario */
			font-weight: bold; /* Si necesitas enfatizar el texto */
		}


		#visibilityMessage {
			background-color: #f0f0f0; /* Fondo más neutro (gris claro) */
			border-color: #ccc; /* Borde gris */
			color: #333; /* Color de texto oscuro */
			padding: 10px; /* Algo de espacio alrededor */
			margin-top: 15px; /* Espacio por encima */
			border-radius: 8px; /* Bordes redondeados */
			text-align: center; /* Centrar el contenido */
			font-size: 16px; /* Tamaño de fuente más legible */
		}

		#visibilityText {
			font-weight: bold; /* Resaltar el texto */
			font-size: 18px; /* Ajustar el tamaño del texto */
			padding: 5px 10px; /* Algo de espacio */
			cursor: pointer; /* Apuntar como botón */
		}

		#visibilityMessage:hover #visibilityText {
			color: #0b5ed7; /* Resaltar cuando se pasa el mouse */
		}
	</style>

</asp:Content>
