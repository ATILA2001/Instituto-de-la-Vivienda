﻿<%@ Page Title="" Language="C#" MasterPageFile="~/User.Master" AutoEventWireup="true" CodeBehind="ModificarAutorizante.aspx.cs" Inherits="WebForms.ModificarAutorizante" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

	<div class="container-fluid mt-4">
		<div class="row">
			<div class="col-md-12  rounded-3 p-3">
				<div class="mx-auto p-2">
					<div class="card-body">
						<table class="table  table1">
							<thead class="thead-dark">
								<tr>
									<th>Obra</th>
									<th>Concepto</th>
									<th>Detalle</th>
									<th>Monto Autorizado</th>
									<th>Mes Aprobacion</th>
									<th>Mes Base</th>
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
										<asp:DropDownList ID="ddlConcepto" CssClass="form-control" runat="server"></asp:DropDownList>
									</td>
									<td>
										<asp:TextBox ID="txtDetalle" CssClass="form-control" runat="server" />
									</td>
									<td>
										<asp:TextBox ID="txtMontoAutorizado" CssClass="form-control" runat="server" />
									</td>
									<td>
										<asp:TextBox ID="txtFecha" CssClass="form-control" runat="server" TextMode="Date" />
									</td>
									<td>
										<asp:TextBox ID="txtMes" CssClass="form-control" runat="server" TextMode="Date" />
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
		</div>
		<div class="text-center p-4">
			<asp:Label ID="lblMensaje" Text="" runat="server" />
		</div>
	</div>
</asp:Content>
