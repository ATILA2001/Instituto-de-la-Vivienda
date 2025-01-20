<%@ Page Title="" Language="C#" MasterPageFile="~/User.Master" AutoEventWireup="true" CodeBehind="Obras.aspx.cs" Inherits="WebForms.Obras" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

	<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<style>
		.table-3d {
			box-shadow: 0 10px 15px rgba(0, 0, 0, 0.2), 0 4px 6px rgba(0, 0, 0, 0.1);
			transform: translateY(-5px);
			transition: transform 0.3s ease, box-shadow 0.3s ease;
			border-radius: 15px; /* Borde redondeado */
			overflow: hidden; /* Para que el contenido no sobresalga de los bordes */
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
										<asp:TextBox ID="txtDescripcion" CssClass="form-control" runat="server" TextMode="Multiline" />
									</td>
									<td class="text-right">
										<asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click"
											CssClass="btn btn-outline-success" runat="server" />
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
		<strong id="visibilityText">Agregar Obra</strong>
	</div>
	<div class="row mt-4">
		<div class="col-md-12">

			<div class="text-end">
				<div class="d-flex flex-wrap justify-content-between p-3 gap-3">
				<div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">
					<div class="form-group">
						<label class="form-label lbl-left" for="ddlFiltroEmpresa">Empresa:</label>
						<asp:DropDownList ID="ddlFiltroEmpresa" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlFiltroEmpresa_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White">
						</asp:DropDownList>
					</div>
					<div class="form-group">
						<label class="form-label lbl-left" for="ddlBarrioFiltro">Barrio:</label>
						<asp:DropDownList ID="ddlBarrioFiltro" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlBarrioFiltro_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White">
						</asp:DropDownList>
					</div></div>
				</div>
			</div>


			<asp:GridView ID="dgvObra" DataKeyNames="ID" CssClass="table"
				OnSelectedIndexChanged="dgvObra_SelectedIndexChanged"
				OnRowDeleting="dgvObra_RowDeleting"
				AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto; white-space: nowrap; width: 100%;">
				<Columns>
					<asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />
					<asp:BoundField HeaderText="Área" DataField="Area" Visible="false" />
					<asp:BoundField HeaderText="Empresa" DataField="Empresa" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Contrata" DataField="Contrata" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Número" DataField="Numero" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Año" DataField="Año" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Etapa" DataField="Etapa" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Obra N°" DataField="ObraNumero" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Barrio" DataField="Barrio" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Nombre de Obra" DataField="Descripcion" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

					<asp:BoundField HeaderText="Disponible Inicial" DataField="AutorizadoInicial" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Disponible Actual" DataField="AutorizadoNuevo" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Suma Autorizantes" DataField="MontoAutorizante" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Suma Certificados" DataField="MontoCertificado" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Ejecucion actual" DataField="Porcentaje" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:N2}%" />

					<asp:CommandField ShowSelectButton="true" SelectText="Modificar" ControlStyle-CssClass="btn btn-outline-warning" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-outline-danger" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
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
				$('#visibilityText').text("Agregar Obra"); // Texto cuando la sección está oculta
			}

			// Manejar clic en el mensaje para alternar el estado de visibilidad
			$(document).on('click', '#visibilityMessage', function () {
				// Cambiamos el valor de visibilidad
				var currentStatus = $('#visibilityText').text();

				if (currentStatus === "Agregar Obra") {
					// Si está oculto, lo mostramos
					localStorage.setItem("sectionVisible", "true");
					$('#section1').show(); // Mostramos la sección
					$('#visibilityText').text("Ocultar sección"); // Cambiar el texto
				} else {
					// Si está visible, lo ocultamos
					localStorage.setItem("sectionVisible", "false");
					$('#section1').hide(); // Ocultamos la sección
					$('#visibilityText').text("Agregar Obra"); // Cambiar el texto
				}
			});
		});
	</script>
	<style>
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



