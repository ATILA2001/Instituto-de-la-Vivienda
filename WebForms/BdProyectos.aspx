<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="BdProyectos.aspx.cs" Inherits="WebForms.BdProyectos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
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

	<div class="container-fluid mt-4">
		<div class="row">
			<div class="col-md-12 rounded-3 p-3">
				<div class="mx-auto p-2">
					<div class="card-body">
						<table class="table table-3d">
							<thead class="thead-dark">
								<tr>
									<th>Obra</th>
									<th>Proyecto</th>
									<th>SubProyecto</th>
									<th>Linea de Gestión</th>
									<th>Monto Autorizado Inicial</th>
									<th></th>
								</tr>
							</thead>
							<tbody>
								<tr>
									<td>
										<asp:DropDownList ID="ddlObra" CssClass="form-control" runat="server"></asp:DropDownList>
									</td>
									<td>
										<asp:TextBox ID="txtProyecto" CssClass="form-control" runat="server" />
									</td>
									<td>
										<asp:TextBox ID="txtSubProyecto" CssClass="form-control" runat="server" />
									</td>
									<td>
										<asp:DropDownList ID="ddlLineaGestion" CssClass="form-control" runat="server"></asp:DropDownList>
									</td>
									<td>
										<asp:TextBox ID="txtMontoAutorizadoInicial" CssClass="form-control" runat="server" />
									</td>
									<td class="text-right">
										<asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-outline-success" runat="server" />
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
	<div class="container-fluid mt-4">
		<div class="row mt-4">
			<div class="col-md-12">
				<div class="text-end">
					<div class="d-flex flex-wrap justify-content-between p-3 gap-3">
						<div class="form-group text-left" style="flex: 1; max-width: 200px;">
							<label class="form-label lbl-left" for="txtSubtotal">Subtotal Inicial:</label>
							<asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control form-control-uniform" ReadOnly="true" />
						</div>

						<div class="form-group text-left" style="flex: 1; max-width: 200px;">

							<label class="form-label lbl-left" for="txtSubtotal1">Subtotal Nuevo:</label>
							<asp:TextBox ID="txtSubtotal1" runat="server" CssClass="form-control form-control-uniform" ReadOnly="true" />
						</div>
						<div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">
							<div class="form-group">
								<label class="form-label lbl-left" for="ddlLinea">Linea de gestion:</label>
								<asp:DropDownList ID="ddlLinea" runat="server" AutoPostBack="True" Width="300px" OnSelectedIndexChanged="ddlLinea_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White">
								</asp:DropDownList>
							</div>
							<div class="form-group">
								<label class="form-label lbl-left" for="ddlProyecto">Proyecto:</label>
								<asp:DropDownList ID="ddlProyecto" runat="server" AutoPostBack="True"  Width="300px" OnSelectedIndexChanged="ddlProyecto_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White">
								</asp:DropDownList>
							</div>
							<div class="form-group  d-flex align-items-end">
								<asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control form-control-uniform" Placeholder="Buscar..."></asp:TextBox>
								<asp:Button ID="btnBuscar" runat="server" Text="Buscar" CssClass="btn btn-sm btn-outline-dark ms-2" OnClick="btnBuscar_Click" />
							</div>
						</div>

					</div>
				</div>
			</div>


			<asp:GridView ID="dgvBdProyecto" DataKeyNames="Id" CssClass="table"
				OnSelectedIndexChanged="dgvBdProyecto_SelectedIndexChanged"
				OnRowDeleting="dgvBdProyecto_RowDeleting"
				AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto; white-space: nowrap; width: 100%;">
				<Columns>
					<asp:BoundField HeaderText="Contrata " DataField="Obra.Contrata.Nombre" SortExpression="Obra.Contrata.Nombre" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" SortExpression="Obra.Descripcion" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Proyecto" DataField="Proyecto" SortExpression="Proyecto" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="SubProyecto" DataField="SubProyecto" SortExpression="SubProyecto" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Linea de Gestión" DataField="LineaGestion.Nombre" SortExpression="LineaGestion.Nombre" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Monto Inicial" DataField="AutorizadoInicial" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Monto Nuevo" DataField="AutorizadoNuevo" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
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
		document.getElementById('<%= txtBuscar.ClientID %>').addEventListener('keydown', function (event) {
			if (event.key === 'Enter') {
				event.preventDefault(); // Evita el envío del formulario
				document.getElementById('<%= btnBuscar.ClientID %>').click(); // Simula el clic en el botón Buscar
			}
		});
	</script>
	<style>
		.d-flex.align-items-end > .form-control {
			margin-right: 8px; /* Margen entre el campo y el botón */
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
	</style>

</asp:Content>
