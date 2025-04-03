<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ModificarProyecto.aspx.cs" Inherits="WebForms.ModificarProyecto" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

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
										<asp:TextBox ID="txtObra" CssClass="form-control" runat="server" ReadOnly="true" />
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
										<asp:Button Text="Modificar" ID="btnModificar" OnClick="btnModificar_Click" CssClass="btn btn-outline-success" runat="server" />
									</td>

								</tr>
							</tbody>
						</table>
						
			<div class="text-center p-4">
				<asp:Label ID="lblMensaje" Text="" runat="server" />
			</div>
		
					</div>
				</div>
			</div>
		</div>


</asp:Content>
