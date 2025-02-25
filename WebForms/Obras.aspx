<%@ Page Title="" Language="C#" MasterPageFile="~/User.Master" AutoEventWireup="true" CodeBehind="Obras.aspx.cs" Inherits="WebForms.Obras" %>

<%@ Register Src="~/CustomControls/CheckBoxListSearch.ascx" TagPrefix="CustomControls" TagName="CheckBoxListSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

	<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<style>
		.table, .table-3d, .table-3d thead, .table-3d tbody, .table-3d tr, .table-3d th, .table-3d td {
			color: #ecf0f1 !important; /* Blanco suave para texto */
		}

		.table-3d {
			border-radius: 10px; /* Borde redondeado */
			overflow: hidden; /* Para que el contenido no sobresalga de los bordes */
			border-collapse: collapse;
		}

			.table-3d thead th {
				background-color: #34495e;
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
					<thead class="thead-dark" style="color: #fad404">
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
	</div>



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
							<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownBarrio" data-bs-toggle="dropdown" aria-expanded="false">
								Todos
                   
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownBarrio" style="max-height: 200px; overflow-y: auto;">
								<!-- Rendimos la CheckBoxList aquí -->
								<asp:CheckBoxList ID="cblBarrio" runat="server" CssClass="dropdown-item form-check" />
							</ul>
						</div>
					</div>

					<div class="form-group text-left" style="flex: 1; max-width: 300px;">
						<label class="form-label lbl-left" for="txtBuscar">Buscar:</label>
						<asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control form-control-uniform"></asp:TextBox>
					</div>

					<div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">





						<div class="form-group d-flex align-items-end">
							<button class="btn btn-sm btn-outline-dark" id="visibilityMessage">
								<strong id="visibilityText">Agregar Obra</strong>
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
			<asp:GridView ID="dgvObra" DataKeyNames="ID" CssClass="table1  table-bordered table-hover"
				OnSelectedIndexChanged="dgvObra_SelectedIndexChanged"
				OnRowDeleting="dgvObra_RowDeleting"
				AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
				<Columns>
					<asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />
					<asp:BoundField HeaderText="Área" DataField="Area" Visible="false" />
					<asp:BoundField HeaderText="Empresa" DataField="Empresa" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:TemplateField HeaderText="Contrata" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center">
						<ItemTemplate>
							<%# Eval("Contrata") + " " + Eval("Numero") + "/" + Eval("Año") %>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField HeaderText="Etapa" DataField="Etapa" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Obra N°" DataField="ObraNumero" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Barrio" DataField="Barrio" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Nombre de Obra" DataField="Descripcion" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Disponible Actual" DataField="AutorizadoNuevo" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Planificacion 2025" DataField="MontoCertificado" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Ejecucion Presupuesto 2025" DataField="Porcentaje" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:N2}%" />

					<asp:BoundField HeaderText="Monto de Obra inicial" DataField="MontoInicial" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Monto de Obra actual" DataField="MontoActual" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Faltante de Obra" DataField="MontoFaltante" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Fecha Inicio" DataField="FechaInicio" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:dd-MM-yyyy}" />
					<asp:BoundField HeaderText="Fecha Fin" DataField="FechaFin" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:dd-MM-yyyy}" />

					<%--		<asp:CommandField ShowSelectButton="true" SelectText="Modificar" ControlStyle-CssClass="btn btn-outline-warning" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-outline-danger" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					--%>
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
			actualizarSeleccion('<%= cblEmpresa.ClientID %>', 'dropdownEmpresa', 'selectedEmpresas');presas');

			$('#<%= cblEmpresa.ClientID %> input[type=checkbox]').on('change', function () {
				actualizarSeleccion('<%= cblEmpresa.ClientID %>', 'dropdownEmpresa', 'selectedEmpresas');
			});

			// Inicializar para barrios
			var barriosSeleccionados = JSON.parse(localStorage.getItem('selectedBarrios')) || [];
			actualizarSeleccion('<%= cblBarrio.ClientID %>', 'dropdownBarrio', 'selectedBarrios');

			$('#<%= cblBarrio.ClientID %> input[type=checkbox]').on('change', function () {
				actualizarSeleccion('<%= cblBarrio.ClientID %>', 'dropdownBarrio', 'selectedBarrios');
			});

			// Limpiar filtros
			$('#<%= btnLimpiarFiltros.ClientID %>').on('click', function () {
				// Limpiar localStorage
				localStorage.removeItem('selectedEmpresas');
				localStorage.removeItem('selectedBarrios');

				// Restablecer checkboxes
				$('#<%= cblEmpresa.ClientID %> input[type=checkbox]').prop('checked', false);
				$('#<%= cblBarrio.ClientID %> input[type=checkbox]').prop('checked', false);

				// Restablecer texto de los botones
				$('#dropdownEmpresa').text('Todas');
				$('#dropdownBarrio').text('Todos');
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
			background-color: #34495e; /* Azul institucional */
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
			background-color: #34495e; /* Fondo más neutro (gris claro) */
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
			background-color: #2c3e50; /* Gris oscuro cuando se pasa el cursor */
		}


			#visibilityMessage:hover #visibilityText {
				color: white; /* Resaltar cuando se pasa el mouse */
			}
	</style>

</asp:Content>



