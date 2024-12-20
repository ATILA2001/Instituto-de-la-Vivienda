<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AbmlUsuarios.aspx.cs" Inherits="WebForms.AbmlUsuarios" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
     <style>
        .oculto {
            display: none;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <h2 class="text-center p-2">Listado Usuarios</h2>

    <div class=" container-fluid mx-auto p-2" style="width: 50%;">
        <asp:GridView ID="dgvUsuario" DataKeyNames="ID" CssClass="table table-bordered table-hover" OnSelectedIndexChanged="dgvUsuario_SelectedIndexChanged" OnRowDeleting="dgvUsuario_RowDeleting" AutoGenerateColumns="false" runat="server">
            <Columns>
                
                <asp:BoundField HeaderText="ID" DataField="ID" HeaderStyle-CssClass="oculto" ItemStyle-CssClass="oculto" />
                <asp:BoundField HeaderText="Usuario" DataField="NOMBRE" HeaderStyle-BackColor="DarkGray" />
                <asp:BoundField HeaderText="Correo" DataField="CORREO" HeaderStyle-BackColor="DarkGray" />

                <asp:TemplateField HeaderText="Tipo" HeaderStyle-BackColor="DarkGray">
            <ItemTemplate>
                <%# Convert.ToInt32(Eval("TIPO")) == 1 ? "Admin" : "User" %>
           </ItemTemplate>
        </asp:TemplateField>              
               
                <asp:TemplateField HeaderText="Estado" HeaderStyle-BackColor="DarkGray">
            <ItemTemplate>
                <%# Convert.ToBoolean(Eval("ESTADO")) ? "Habilitado" : "Bloqueado" %>
            </ItemTemplate>
            </asp:TemplateField><asp:CommandField ShowSelectButton="true" SelectText="Modificar" HeaderStyle-BackColor="DarkGray" ControlStyle-CssClass="btn btn-outline-warning" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderText="Modificar" />
               <asp:CommandField ShowDeleteButton="true" HeaderStyle-BackColor="DarkGray" ControlStyle-CssClass="btn btn-outline-danger" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" HeaderText="Eliminar" />
            
            </Columns>

        </asp:GridView>
    </div>

    <asp:Label ID="lblMensaje" Text="" runat="server" />
</asp:Content>
