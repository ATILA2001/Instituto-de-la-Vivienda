<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ModificarAutorizanteAdmin.aspx.cs" Inherits="WebForms.ModificarAutorizanteAdmin" %>

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
									<th>Aprobacion</th>
									<th></th>
									<th></th>
								</tr>
							</thead>
							<tbody>
								<tr>
									<td>
										<asp:DropDownList ID="ddlObra" CssClass="form-control" runat="server" Enabled="false"></asp:DropDownList>
									</td>
									<td>
										<asp:TextBox ID="txtConcepto" CssClass="form-control" runat="server" />
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
										<asp:DropDownList ID="ddlAutorizacionGG" runat="server">
											<asp:ListItem Text="A AUTORIZAR" Value="0"></asp:ListItem>
											<asp:ListItem Text="AUTORIZADO" Value="1"></asp:ListItem>
										</asp:DropDownList>
									</td>
									<td class="text-right">
										<asp:Button Text="Modificar" ID="btnModificar" OnClick="btnModificar_Click" CssClass="btn btn-outline-success" runat="server" />
									</td>

								</tr>
							</tbody>
						</table>
					</div>
				</div>
			</div>
			<div class="text-center p-4">
				<asp:Label ID="lblMensaje" Text="" runat="server" />
			</div>
		</div>
</asp:Content>
