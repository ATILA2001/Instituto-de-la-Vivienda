<%@ Page Title="" Language="C#" MasterPageFile="~/Redet.Master" AutoEventWireup="true" CodeBehind="Redeterminaciones.aspx.cs" Inherits="WebForms.Redeterminaciones" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

	<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	
	<div id="section1" style="display: none;">
		<div class="row mt-4">
			<div class="col-md-12">
				<table class="table  table1">
					<thead class="thead-dark" >
						<tr>
							<th>Código Autorizante</th>
							<th>Expediente</th>
							<th>Salto</th>
							<th>Nro</th>
							<th>Tipo</th>
							<th>Etapa</th>
							<th>Porcentaje</th>
							<th>Observaciones</th>
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
								<asp:TextBox ID="txtSalto" CssClass="form-control" runat="server" TextMode="Date" />
							</td>

							<td>
								<asp:TextBox ID="txtNro" CssClass="form-control" runat="server" />
							</td>

							<td>
								<asp:TextBox ID="txtTipo" CssClass="form-control" runat="server"></asp:TextBox>
							</td>
							<td>
								<asp:DropDownList ID="ddlEtapa" CssClass="form-control" runat="server"></asp:DropDownList>
							</td>
							<td>
								<asp:TextBox ID="txtPorcentaje" CssClass="form-control" runat="server" />
							</td>
							<td>
								<asp:TextBox ID="txtObservacion" CssClass="form-control" runat="server" />
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
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblAutorizante">Autorizante:</label>
						<div class="dropdown">
							<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownAutorizante" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownAutorizante" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblAutorizante" runat="server" CssClass="dropdown-item form-check" />
							</ul>
						</div>
					</div>

					<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblEtapa">Etapa:</label>
						<div class="dropdown">
							<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownEtapa" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownEtapa" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblEtapa" runat="server" CssClass="dropdown-item form-check" />
							</ul>
						</div>
					</div>

					<%--<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblFecha">Fecha:</label>
						<div class="dropdown">
							<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownFecha" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownFecha" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblFecha" runat="server" CssClass="dropdown-item form-check" />
							</ul>
						</div>
					</div>--%>

					<div class="form-group text-left" style="flex: 1; max-width: 300px;">
						<label class="form-label lbl-left" for="txtBuscar">Buscar:</label>
						<asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control form-control-uniform"></asp:TextBox>
					</div>







					<div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">


						<div class="form-group d-flex align-items-end">
							<button class="btn btn-sm btn-secondary" id="visibilityMessage">
								<span id="visibilityText">Agregar Redet</span>
							</button>
						</div>

						<div class="form-group  d-flex align-items-end">
							<asp:Button CssClass="btn btn-sm btn-primary " ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClientClick="limpiarFiltros();" />
						</div>
						<div class="form-group d-flex align-items-end">
							<asp:Button CssClass="btn btn-sm btn-primary" ID="btnFiltrar" Text="Filtrar" runat="server" OnClick="btnFiltrar_Click" />
						</div>

					</div>
				</div>
			</div>

			<hr />
			<asp:GridView ID="dgvRedeterminacion" DataKeyNames="ID" CssClass="table1 table-bordered table-hover"
				OnSelectedIndexChanged="dgvRedeterminacion_SelectedIndexChanged"
				OnRowDeleting="dgvRedeterminacion_RowDeleting"
				 OnRowDataBound="dgvRedeterminacion_RowDataBound"
				AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
				<Columns>
					<asp:BoundField HeaderText="ID" DataField="ID" Visible="false" />
					<asp:BoundField HeaderText="Obra" DataField="Obra" />
					<asp:BoundField HeaderText="Código Autorizante" DataField="CodigoRedet" />
					<asp:BoundField HeaderText="Etapa" DataField="Etapa.Nombre" />
					<asp:TemplateField HeaderText="Etapa" >
						<ItemTemplate>
							<asp:DropDownList ID="ddlEtapas" runat="server" AutoPostBack="true"
								OnSelectedIndexChanged="ddlEtapas_SelectedIndexChanged" class="btn btn-sm dropdown-toggle" Style="background-color: white !important; color: #34495e !important; font-weight: normal; padding: 8px 12px; font-size: 14px;">
							</asp:DropDownList>
						</ItemTemplate>
					</asp:TemplateField>

					<asp:TemplateField HeaderText="Expediente" >
						<ItemTemplate>
							<asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("Expediente") %>' AutoPostBack="true"
								OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm"></asp:TextBox>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField HeaderText="Tipo" DataField="Tipo"  />
					<asp:BoundField HeaderText="Salto" DataField="Salto" DataFormatString="{0:dd-MM-yyyy}"  />
					<asp:BoundField HeaderText="Buzón SADE" DataField="BuzonSade"  />
					<asp:BoundField HeaderText="Fecha SADE" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}"  />
					<asp:BoundField HeaderText="Porcentaje" DataField="Porcentaje"  />
					<asp:BoundField HeaderText="Observaciones" DataField="Observaciones"  />
					<asp:BoundField HeaderText="Empresa" DataField="Empresa" />
					<asp:BoundField HeaderText="Área" DataField="Area"  />
					<asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade"  />
					<asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}"  />

					<asp:CommandField ShowSelectButton="true" SelectText="Modificar" ControlStyle-CssClass="btn btn-sm btn-warning text-dark"  />
					<asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-sm btn-danger text-light" />
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
				$('#visibilityText').text("Agregar Redet"); // Texto cuando la sección está oculta
			}

			// Manejar clic en el mensaje para alternar el estado de visibilidad
			$(document).on('click', '#visibilityMessage', function () {
				// Cambiamos el valor de visibilidad
				var currentStatus = $('#visibilityText').text();

				if (currentStatus === "Agregar Redet") {
					// Si está oculto, lo mostramos
					localStorage.setItem("sectionVisible", "true");
					$('#section1').show(); // Mostramos la sección
					$('#visibilityText').text("Ocultar sección"); // Cambiar el texto
				} else {
					// Si está visible, lo ocultamos
					localStorage.setItem("sectionVisible", "false");
					$('#section1').hide(); // Ocultamos la sección
					$('#visibilityText').text("Agregar Redet"); // Cambiar el texto
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
			var empresasSeleccionadas = JSON.parse(localStorage.getItem('selectedObras')) || [];
			actualizarSeleccion('<%= cblObra.ClientID %>', 'dropdownObra', 'selectedObra');


			$('#<%= cblObra.ClientID %> input[type=checkbox]').on('change', function () {
				actualizarSeleccion('<%= cblObra.ClientID %>', 'dropdownObra', 'selectedObras');
			});

			// Inicializar para Tipo 
			var TipoSeleccionados = JSON.parse(localStorage.getItem('selectedEtapas')) || [];
			actualizarSeleccion('<%= cblEtapa.ClientID %>', 'dropdownEtapa', 'selectedEtapas');

			$('#<%= cblEtapa.ClientID %> input[type=checkbox]').on('change', function () {
				actualizarSeleccion('<%= cblEtapa.ClientID %>', 'dropdownEtapa', 'selectedEtapas');
			});
			// Inicializar para  Autorizante 
			var AutorizanteSeleccionados = JSON.parse(localStorage.getItem('selectedAutorizantes')) || [];
			actualizarSeleccion('<%= cblAutorizante.ClientID %>', 'dropdownAutorizante', 'selectedAutorizantes');

			$('#<%= cblAutorizante.ClientID %> input[type=checkbox]').on('change', function () {
				actualizarSeleccion('<%= cblAutorizante.ClientID %>', 'dropdownAutorizante', 'selectedAutorizantes');
			});



			// Limpiar filtros
			$('#<%= btnLimpiarFiltros.ClientID %>').on('click', function () {
				// Limpiar localStorage
				localStorage.removeItem('selectedObras');
				localStorage.removeItem('selectedEtapas');
				localStorage.removeItem('selectedAutorizantes');
				// Restablecer checkboxes
				$('#<%= cblObra.ClientID %> input[type=checkbox]').prop('checked', false);
				$('#<%= cblEtapa.ClientID %> input[type=checkbox]').prop('checked', false);
				$('#<%= cblAutorizante.ClientID %> input[type=checkbox]').prop('checked', false);
				// Restablecer texto de los botones

				$('#dropdownObra').text('Todas');
				$('#dropdownEtapa').text('Todas');
				$('#dropdownAutorizante').text('Todos');
			});
		});



	</script>
	
</asp:Content>
