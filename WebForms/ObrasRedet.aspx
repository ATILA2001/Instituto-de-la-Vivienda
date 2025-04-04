<%@ Page Title="" Language="C#" MasterPageFile="~/Redet.Master" AutoEventWireup="true" CodeBehind="ObrasRedet.aspx.cs" Inherits="WebForms.ObrasRedet" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

	<div class="row mt-4">
		<div class="col-md-12">
			<div class="text-end">

				<div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">


					<div class="form-group ">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblArea">Area:</label>
						<div class="dropdown">
							<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownArea" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
               
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownArea" style="max-height: 200px; overflow-y: auto;">
								<!-- Rendimos la CheckBoxList aquí -->
								<asp:CheckBoxList ID="cblArea" runat="server" CssClass="dropdown-item form-check" />
							</ul>
						</div>
					</div>

					<div class="form-group ">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblEmpresa">Empresa:</label>
						<div class="dropdown">
							<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownEmpresa" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
               
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownEmpresa" style="max-height: 200px; overflow-y: auto;">
								<!-- Rendimos la CheckBoxList aquí -->
								<asp:CheckBoxList ID="cblEmpresa" runat="server" CssClass="dropdown-item form-check" />
							</ul>
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
 runat="server"
				Style="display: block; overflow-x: auto;">

				<Columns>
					<asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />
					<asp:BoundField HeaderText="Área" DataField="Area"  />
					<asp:BoundField HeaderText="Empresa" DataField="Empresa"  />
					<asp:BoundField HeaderText="Contrata" DataField="Contrata"  />
					<asp:BoundField HeaderText="Número" DataField="Numero"  />
					<asp:BoundField HeaderText="Año" DataField="Año" />
					<asp:BoundField HeaderText="Etapa" DataField="Etapa"  />
					<asp:BoundField HeaderText="Obra N°" DataField="ObraNumero"  />
					<asp:BoundField HeaderText="Barrio" DataField="Barrio"  />
					<asp:BoundField HeaderText="Nombre de Obra" DataField="Descripcion" />
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
				$dropdown.text(textoBoton);
			}

			// Inicializar para empresas
			var empresasSeleccionadas = JSON.parse(localStorage.getItem('selectedEmpresas')) || [];
			actualizarSeleccion('<%= cblEmpresa.ClientID %>', 'dropdownEmpresa', 'selectedEmpresas');

			$('#<%= cblEmpresa.ClientID %> input[type=checkbox]').on('change', function () {
				actualizarSeleccion('<%= cblEmpresa.ClientID %>', 'dropdownEmpresa', 'selectedEmpresas');
			});

			// Inicializar para barrios
			var barriosSeleccionados = JSON.parse(localStorage.getItem('selectedBarrios')) || [];
			actualizarSeleccion('<%= cblBarrio.ClientID %>', 'dropdownBarrio', 'selectedBarrios');

			$('#<%= cblBarrio.ClientID %> input[type=checkbox]').on('change', function () {
				actualizarSeleccion('<%= cblBarrio.ClientID %>', 'dropdownBarrio', 'selectedBarrios');
			});
			// Inicializar para areas
			var areasSeleccionados = JSON.parse(localStorage.getItem('selectedAreas')) || [];
			actualizarSeleccion('<%= cblArea.ClientID %>', 'dropdownArea', 'selectedAreas');

			$('#<%= cblArea.ClientID %> input[type=checkbox]').on('change', function () {
				actualizarSeleccion('<%= cblArea.ClientID %>', 'dropdownArea', 'selectedAreas');
			});

			// Limpiar filtros
			$('#<%= btnLimpiarFiltros.ClientID %>').on('click', function () {
				// Limpiar localStorage
				localStorage.removeItem('selectedEmpresas');
				localStorage.removeItem('selectedBarrios');
				localStorage.removeItem('selectedAreas');


				// Restablecer checkboxes
				$('#<%= cblEmpresa.ClientID %> input[type=checkbox]').prop('checked', false);
				$('#<%= cblBarrio.ClientID %> input[type=checkbox]').prop('checked', false);

				// Restablecer texto de los botones
				$('#dropdownEmpresa').text('Todas');
				$('#dropdownBarrio').text('Todos');
				$('#dropdownArea').text('Todas');
			});
		});

	</script>

</asp:Content>



