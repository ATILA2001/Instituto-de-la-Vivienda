<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AbmlUsuarios.aspx.cs" Inherits="WebForms.AbmlUsuarios" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
	
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

	<h2 class="text-center p-2">Listado Usuarios</h2>

	<div class="container-fluid mt-4 d-flex justify-content-center">
		<asp:GridView ID="dgvUsuario" DataKeyNames="ID" CssClass="table1  table-bordered table-hover " OnSelectedIndexChanged="dgvUsuario_SelectedIndexChanged" OnRowDeleting="dgvUsuario_RowDeleting" AutoGenerateColumns="false" runat="server">
			<Columns>

				<asp:BoundField HeaderText="ID" DataField="ID" HeaderStyle-CssClass="oculto" ItemStyle-CssClass="oculto" />
				<asp:BoundField HeaderText="Area" DataField="AREA"/>
				<asp:BoundField HeaderText="Usuario" DataField="NOMBRE" />
				<asp:BoundField HeaderText="Correo" DataField="CORREO"  />

				<asp:TemplateField HeaderText="Tipo" >
					<ItemTemplate>
						<%# Convert.ToInt32(Eval("TIPO")) == 1 ? "Admin" : "User" %>
					</ItemTemplate>
				</asp:TemplateField>

				<asp:TemplateField HeaderText="Estado" >
					<ItemTemplate>
						<%# Convert.ToBoolean(Eval("ESTADO")) ? "Habilitado" : "Bloqueado" %>
					</ItemTemplate>
				</asp:TemplateField>
				<asp:CommandField ShowSelectButton="true" SelectText="Modificar" ControlStyle-CssClass="btn btn-outline-warning" HeaderText="Modificar"  />
				<asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-outline-danger" HeaderText="Eliminar" />

			</Columns>

		</asp:GridView>
	</div>

	<asp:Label ID="lblMensaje" Text="" runat="server" />
	<hr />	
		

</asp:Content>
