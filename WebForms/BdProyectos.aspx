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
			<!-- Sección de Agregar BdProyecto -->
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

		<hr />

		<!-- GridView de BdProyecto -->
		<div class="row mt-4">
			<div class="col-md-12">
				<asp:GridView ID="dgvBdProyecto" DataKeyNames="ID" CssClass="table"
					OnSelectedIndexChanged="dgvBdProyecto_SelectedIndexChanged"
					OnRowDeleting="dgvBdProyecto_RowDeleting"
					AutoGenerateColumns="false" runat="server" style="display: block;  overflow-x: auto; white-space: nowrap; width: 100%;">
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
	</div>

	<script type="text/javascript">
		function soloNumeros(e) {
			var tecla = (document.all) ? e.keyCode : e.which;
			if (tecla == 8 || tecla == 46) {
				return true;
			}
			var patron = /^[0-9]$/;
			var te = String.fromCharCode(tecla);
			return patron.test(te);
		}
	</script>


</asp:Content>
