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
	.form-group label {
		font-size: 14px;
		color: #212529;
		font-weight: 600;
	}

	.form-group .dropdown-toggle {
		background-color: #f8f9fa;
		color: #212529;
		border-radius: 0.375rem;
		width: 100%;
		text-align: left;
		font-size: 14px;
		font-weight: normal;
	}

		.form-group .dropdown-toggle:hover {
			background-color: #e2e6ea;
			border-color: #adb5bd;
			color: #212529;
		}

	.form-group .dropdown-menu {
		border: 1px solid;
		border-radius: 0.375rem;
		padding: 0.5rem;
		background-color: #ffffff;
		max-height: 200px;
		overflow-y: auto;
	}

		.form-group .dropdown-menu .form-check:hover {
			background-color: transparent;
		}

		.form-group .dropdown-menu .form-check input[type="checkbox"]:focus {
			outline: none;
			box-shadow: none;
		}

		.form-group .dropdown-menu .form-check {
			margin-bottom: 0.5rem;
		}

			.form-group .dropdown-menu .form-check label {
				font-size: 14px;
				color: #495057;
				background-color: transparent;
			}

			.form-group .dropdown-menu .form-check input[type="checkbox"] {
				margin-right: 8px;
			}

				.form-group .dropdown-menu .form-check input[type="checkbox"]:focus + label,
				.form-group .dropdown-menu .form-check input[type="checkbox"]:checked + label {
					color: #495057;
					background-color: transparent;
					font-weight: normal;
				}

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

	.table1 {
		border-radius: 10px;
		box-shadow: 0 2px 5px rgba(0, 0, 0, 0.15);
	}

		.table1 th {
			text-align: center;
			font-weight: bold;
			background: #153244;
			color: white;
			border: 1px solid #153244;
		}

	.form-label {
		margin-bottom: 0;
	}

	.d-flex.align-items-end > .form-control {
		margin-right: 8px;
	}

	.form-control-uniform {
		display: inline-block;
		font-size: 14px;
		padding: 4px 12px;
		border: 1px solid;
	}

	.btn {
		background-color: #153244;
		text-align: center;
		transition: all 0.3s ease-in-out;
		color: #ecf0f1;
		border: none;
		padding: 8px 12px;
		font-size: 14px;
		cursor: pointer;
		border-radius: 4px;
		display: inline-block;
		font-weight: bold;
	}

		.btn:hover {
			background-color: #8DE2D6;
			color: #153244;
		}

	.lbl-left {
		text-align: left;
		display: block;
		font-weight: bold;
	}

	#visibilityMessage {
		background-color: #8DE2D6;
		text-align: center;
		transition: all 0.3s ease-in-out;
		color: #153244;
	}

	#visibilityText {
		cursor: pointer;
		display: inline-block;
		transition: color 0.3s ease-in-out;
	}

	#visibilityMessage:hover {
		background-color: #153244;
	}

		#visibilityMessage:hover #visibilityText {
			color: white;
		}
</style>
</asp:Content>
