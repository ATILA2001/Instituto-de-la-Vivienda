<%@ Page Title="" Language="C#" MasterPageFile="~/User.Master" AutoEventWireup="true" CodeBehind="Legitimos.aspx.cs" Inherits="WebForms.Legitimos" %>

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
			<div class="col-md-12 rounded-3 p-3">
				<div class=" mx-auto p-2">
					<div class="card-body">
						<table class="table  table-3d">
							<thead class="thead-dark">
								<tr>
									<th>Obra</th>
									<th>Código Autorizante</th>
									<th>Expediente</th>
									<th>Inicio Ejecución</th>
									<th>Fin Ejecución</th>
									<th>Monto Certificado</th>
									<th>Mes Aprobación</th>
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
										<asp:TextBox ID="txtAutorizante" CssClass="form-control" runat="server" />
									</td>
									<td>
										<asp:TextBox ID="txtExpediente" CssClass="form-control" runat="server" />
									</td>
									<td>
										<asp:TextBox ID="txtInicioEjecucion" CssClass="form-control" runat="server" TextMode="Date" />
									</td>
									<td>
										<asp:TextBox ID="txtFinEjecucion" CssClass="form-control" runat="server" TextMode="Date" />
									</td>
									<td>
										<asp:TextBox ID="txtCertificado" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
									</td>
									<td>
										<asp:TextBox ID="txtMesAprobacion" CssClass="form-control" runat="server" TextMode="Date" />
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
		</div>
	</div>
	<hr />
	<div class="alert" id="visibilityMessage" role="alert">
		<strong id="visibilityText">Agregar Legitimo</strong>
	</div>
	<div class="container-fluid mt-4">
		<div class="row mt-4">
			<div class="col-md-12">
				<div class="text-end">
					<!-- Contenedor para subtotal alineado a la izquierda -->
					<div class="d-flex flex-wrap justify-content-between p-3 gap-3">
						<!-- Subtotal alineado a la izquierda -->
						<div class="form-group text-left" style="flex: 1; max-width: 300px;">
							<label class="form-label lbl-left" for="txtSubtotal">Subtotal:</label>
							<asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control form-control-uniform" ReadOnly="true" />
						</div>

						<!-- Filtros alineados a la derecha -->
						<div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">
							<div class="form-group">
								<label class="form-label lbl-left" for="ddlEmpresa">Empresa:</label>
								<asp:DropDownList ID="ddlEmpresa" runat="server" AutoPostBack="True" Width="300px" OnSelectedIndexChanged="ddlEmpresa_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White">
								</asp:DropDownList>
							</div>
							<div class="form-group d-flex align-items-end">
								<div>
									<label class="form-label lbl-left" for="txtMesAprobacionFiltro">Mes Aprobación:</label>
									<asp:TextBox ID="txtMesAprobacionFiltro" runat="server" CssClass="form-control form-control-uniform" TextMode="Date" />
								</div>
								<asp:Button ID="btnFiltrarMes" runat="server" CssClass="btn btn-sm btn-outline-dark ms-2" Text="Filtrar por Mes" OnClick="btnFiltrarMes_Click" />
							</div>


						</div>
					</div>
				</div>
			</div>


			<asp:GridView ID="dgvLegitimos" DataKeyNames="CodigoAutorizante" CssClass="table1  table-bordered table-hover  "
				OnRowDeleting="dgvLegitimos_RowDeleting" AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
				<Columns>
					<asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Empresa" DataField="Empresa" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Código Autorizante" DataField="CodigoAutorizante" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:TemplateField HeaderText="Expediente" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center">
						<ItemTemplate>
							<asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("Expediente") %>' AutoPostBack="true"
								OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm"></asp:TextBox>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField HeaderText="Inicio Ejecución" DataField="InicioEjecucion" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Fin Ejecución" DataField="FinEjecucion" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Certificado" DataField="Certificado" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Mes Aprobación" DataField="MesAprobacion" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Sigaf" DataField="Sigaf" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

					<asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-outline-danger" DeleteText="Eliminar" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
				</Columns>
			</asp:GridView>
			<div class="text-center p-4">
				<asp:Label ID="lblMensaje" Text="" CssClass="text-success" runat="server" />
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
				$('#visibilityText').text("Agregar Legitimo"); // Texto cuando la sección está oculta
			}

			// Manejar clic en el mensaje para alternar el estado de visibilidad
			$(document).on('click', '#visibilityMessage', function () {
				// Cambiamos el valor de visibilidad
				var currentStatus = $('#visibilityText').text();

				if (currentStatus === "Agregar Legitimo") {
					// Si está oculto, lo mostramos
					localStorage.setItem("sectionVisible", "true");
					$('#section1').show(); // Mostramos la sección
					$('#visibilityText').text("Ocultar sección"); // Cambiar el texto
				} else {
					// Si está visible, lo ocultamos
					localStorage.setItem("sectionVisible", "false");
					$('#section1').hide(); // Ocultamos la sección
					$('#visibilityText').text("Agregar Legitimo"); // Cambiar el texto
				}
			});
		});
	</script>
	<style>
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

		.form-control-uniform {
			display: inline-block;
			font-size: 14px; /* Tamaño de texto uniforme */
			padding: 6px 12px;
			margin-top: -6px;
			border: 1px solid;
		}

		.btn {
			margin-top: -4px;
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
