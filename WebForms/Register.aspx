<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="WebForms.Register" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

     <h1 class="text-center p-2">Registrate</h1>

    <asp:ScriptManager ID="ScriptManager1" runat="server" />


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
<asp:TextBox 
    ID="txtNombre" 
    onkeydown="return (event.keyCode >= 65 && event.keyCode <= 90 || event.keyCode >= 97 && event.keyCode <= 122 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 32)" 
    CssClass="form-control" 
    placeholder="Nombre" 
    runat="server" /> </div>
                                </div>
                                <div class="col-6">
    <div class="p-3">
        <asp:Label ID="lblArea" CssClass="fs-5" Text="Área" runat="server" />
        <asp:DropDownList ID="ddlAreas" CssClass="form-control" runat="server"></asp:DropDownList>
        <asp:Label ID="lblError" CssClass="text-danger" runat="server" />
    </div>
</div>
                          
                                <div class="col-6">
                                    <div class="p-3">
                                        <asp:Label ID="lblEmail" CssClass="fs-5" Text="Email" runat="server" />
                                        <asp:TextBox type="email" CssClass="form-control" ID="txtEmail" placeholder="Email@Ejemplo.com" runat="server" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="p-3">
                                        <asp:Label ID="lblEmailRep" CssClass="fs-5" Text="Repetir Email" runat="server" />
                                        <asp:TextBox AutoPostBack="true" ID="txtEmailRep" CssClass="form-control" OnTextChanged="txtEmailRep_TextChanged" placeholder="Email@Ejemplo.com" runat="server" />
                                    </div>
                                    <asp:Label ID="lblErrorMail" runat="server" />
                                </div>

                                <div class="col-6">
                                    <div class="p-3">
                                        <asp:Label ID="lblContrasenia" CssClass="fs-5" Text="Contraseña" runat="server" />
                                        <asp:TextBox type="password" CssClass="form-control" ID="txtPass" placeholder="Contraseña" runat="server" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="p-3">
                                        <asp:Label ID="lblPassRep" CssClass="fs-5" Text="Repetir Contraseña" runat="server" />
                                        <asp:TextBox AutoPostBack="true" type="password" ID="txtPassRep" placeholder="Contraseña" CssClass="form-control" OnTextChanged="txtPassRep_TextChanged" runat="server" />
                                    </div>
                                    <asp:Label ID="lblErrorPass" Text="" runat="server" />
                                </div>
                      

                            </div>
                        </div>

                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
    <div class="text-end p-4">
        <asp:Label ID="lblMensaje" Text="" runat="server" />

    </div>
            <div class="text-center m-2">
               <asp:Button ID="Button2" CssClass="btn btn-outline-primary" Text="Registrar" runat="server" OnClick="btnRegistrar_Click" />
            </div>
        </div>
    </div>
</div>
<div class="text-end p-4">
    <asp:Label ID="Label9" Text="" runat="server" />

</div>
</asp:Content>
