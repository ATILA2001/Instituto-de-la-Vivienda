<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ModificarUsuario.aspx.cs" Inherits="WebForms.ModificarUsuario" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h1 class="text-center p-2">Modificar Usuario</h1>



    <div class="container-fluid" style="max-width: 50%;">
        <div class="card mb-2">
            <div class="card-body">


                <asp:UpdatePanel runat="server">
                    <ContentTemplate>
                        <div class="container overflow-hidden text-center">
                            <div class="row">
                                <div class="col-6">
                                    <div class="p-3">
                                        <asp:Label ID="lblNombre" CssClass="fs-5" Text="Nombre completo" runat="server" />
                                        <asp:TextBox ID="txtNombre" onkeydown="return (event.keyCode >= 65 && event.keyCode <= 90 || event.keyCode >= 97 && event.keyCode <= 122 || event.keyCode == 08 || event.keyCode == 9)" CssClass="form-control" placeholder="Nombre" runat="server" />
                                    </div>
                                </div>

                                <div class="col-6">
                                    <div class="p-3">
                                        <asp:Label ID="lblEmail" CssClass="fs-5" Text="Email" runat="server" />
                                        <asp:TextBox type="email" CssClass="form-control" ID="txtEmail" placeholder="Email@Ejemplo.com" runat="server" ReadOnly="true" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="p-3">
                                        <asp:Label ID="lblTipo" CssClass="fs-5" Text="Tipo" runat="server" />
                                        <asp:TextBox CssClass="form-control" ID="txtTipo" runat="server" ReadOnly="true" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="p-3">
                                        <asp:Label ID="lblEstado" CssClass="fs-5" Text="Estado" runat="server" />
                                        <br />
                                        <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-control">
                                            <asp:ListItem Text="Habilitado" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="Bloqueado" Value="0"></asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>

                                <div class="mb-2 text-center p-2">
                                    <asp:Button Text="Volver" ID="btnVolver" CssClass="btn btn-outline-secondary" OnClick="btnVolver_Click" runat="server" />
                                    <asp:Button Text="Modificar" ID="btnModificar" OnClick="btnModificar_Click" CssClass="btn btn-outline-success" runat="server" />
                                </div>
                            </div>
                        </div>

                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
   
    <div class="text-center p-4">
        <asp:Label ID="lblMensaje" Text="" runat="server" />

    </div>
 </div>

</asp:Content>
