<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="UsuariosPendientes.aspx.cs" Inherits="WebForms.UsuariosPendientes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
	<style>
		.oculto {
			display: none;
		}
	</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

	<h2 class="text-center p-2">Listado Usuarios</h2>

<div class="container-fluid mt-4 d-flex justify-content-center">
		<asp:GridView ID="dgvUsuario" DataKeyNames="ID" CssClass="table1  table-bordered table-hover " OnSelectedIndexChanged="dgvUsuario_SelectedIndexChanged" OnRowDeleting="dgvUsuario_RowDeleting" AutoGenerateColumns="false" runat="server">
			<Columns>

				<asp:BoundField HeaderText="ID" DataField="ID" HeaderStyle-CssClass="oculto" ItemStyle-CssClass="oculto" />
				<asp:BoundField HeaderText="Area" DataField="AREA" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
				<asp:BoundField HeaderText="Usuario" DataField="NOMBRE" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
				<asp:BoundField HeaderText="Correo" DataField="CORREO" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

				<asp:TemplateField HeaderText="Tipo" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center">
					<ItemTemplate>
						<%# Convert.ToInt32(Eval("TIPO")) == 1 ? "Admin" : "User" %>
					</ItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField HeaderText="Estado" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center">
					<ItemTemplate>
						<%# Convert.ToBoolean(Eval("ESTADO")) ? "Habilitado" : "Bloqueado" %>
					</ItemTemplate>
				</asp:TemplateField>
				<asp:CommandField ShowSelectButton="true" SelectText="Modificar" ControlStyle-CssClass="btn btn-outline-warning" HeaderText="Modificar" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
				<asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-outline-danger" HeaderText="Eliminar" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

			</Columns>

		</asp:GridView>
	</div>

	<asp:Label ID="lblMensaje" Text="" runat="server" />
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
	</style>
</asp:Content>
