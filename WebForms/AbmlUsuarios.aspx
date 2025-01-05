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
        <asp:GridView ID="dgvUsuario" DataKeyNames="ID" CssClass="table " OnSelectedIndexChanged="dgvUsuario_SelectedIndexChanged" OnRowDeleting="dgvUsuario_RowDeleting" AutoGenerateColumns="false" runat="server">
            <Columns>
                
                <asp:BoundField HeaderText="ID" DataField="ID" HeaderStyle-CssClass="oculto" ItemStyle-CssClass="oculto" />
                <asp:BoundField HeaderText="Usuario" DataField="NOMBRE"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center"  />
                <asp:BoundField HeaderText="Correo" DataField="CORREO"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center"  />

                <asp:TemplateField HeaderText="Tipo"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" >
            <ItemTemplate>
                <%# Convert.ToInt32(Eval("TIPO")) == 1 ? "Admin" : "User" %>
           </ItemTemplate>
        </asp:TemplateField>              
               
                <asp:TemplateField HeaderText="Estado"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" >
            <ItemTemplate>
                <%# Convert.ToBoolean(Eval("ESTADO")) ? "Habilitado" : "Bloqueado" %>
            </ItemTemplate>
            </asp:TemplateField><asp:CommandField ShowSelectButton="true" SelectText="Modificar"  ControlStyle-CssClass="btn btn-outline-warning"  HeaderText="Modificar"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
               <asp:CommandField ShowDeleteButton="true"  ControlStyle-CssClass="btn btn-outline-danger"  HeaderText="Eliminar"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
            
            </Columns>

        </asp:GridView>
    </div>

    <asp:Label ID="lblMensaje" Text="" runat="server" />
</asp:Content>
