<%@ Page Title="" Language="C#" MasterPageFile="~/LoginRegister.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="WebForms.Register" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    
    <div class="container-fluid d-flex justify-content-center align-items-center">
        <div class="card" style="width: 100%; max-width: 1200px;">
            <div class="row g-0 h-100">
                <!-- Left panel -->
                <div class="col-md-4 text-white bg-dark d-flex flex-column justify-content-center">
                    <div class="p-4 p-md-5 text-center">
                        <div class="mb-5 d-flex justify-content-center align-items-center">
                            <img src="Images/IVC-Blanco.png" alt="Logo" class="img-fluid" />
                        </div>
                    </div>
                </div>
                
                <!-- Right panel -->
                <div class="col-md-8 bg-white">
                    <div class="p-4 p-md-5">
                        <h2 class="mb-4">Registro de usuario</h2>
                        <asp:UpdatePanel runat="server">
                            <ContentTemplate>
                                <div class="row" style="max-height: 450px; overflow-y: auto;">
                                    <div class="col-md-6 mb-3">
                                        <div class="form-label-container">
                                            <asp:Label ID="lblNombre" CssClass="form-label" Text="Nombre completo" runat="server" />
                                            <span class="badge-forms badge-required-forms">Requerido</span>
                                        </div>
                                        <asp:TextBox 
                                            ID="txtNombre" 
                                            onkeydown="return (event.keyCode >= 65 && event.keyCode <= 90 || event.keyCode >= 97 && event.keyCode <= 122 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 32)" 
                                            CssClass="form-control" 
                                            placeholder="Nombre y Apellido" 
                                            aria-required="true"
                                            runat="server" />
                                        <p class="form-label-description">Ingrese su nombre completo</p>
                                    </div>

                                    <div class="col-md-6 mb-3">
                                        <div class="form-label-container">
                                            <asp:Label ID="lblArea" CssClass="form-label" Text="Área" runat="server" />
                                            <span class="badge-forms badge-required-forms">Requerido</span>
                                        </div>
                                        <asp:DropDownList 
                                            ID="ddlAreas" 
                                            CssClass="form-control form-select" 
                                            aria-label="Selección de área" 
                                            aria-required="true"
                                            runat="server">
                                        </asp:DropDownList>
                                        <p class="form-label-description">Seleccione su área de trabajo</p>
                                        <asp:Panel ID="panelError" runat="server" CssClass="invalid-feedback" Visible="false">
                                            <p><asp:Label ID="lblError" runat="server" /></p>
                                        </asp:Panel>
                                    </div>

                                    <div class="col-md-6 mb-3">
                                        <div class="form-label-container">
                                            <asp:Label ID="lblEmail" CssClass="form-label" Text="Email" runat="server" />
                                            <span class="badge-forms badge-required-forms">Requerido</span>
                                        </div>
                                        <asp:TextBox 
                                            type="email" 
                                            CssClass="form-control" 
                                            ID="txtEmail" 
                                            placeholder="Email@Ejemplo.com" 
                                            aria-required="true"
                                            runat="server" />
                                        <p class="form-label-description">Ingrese su correo electrónico</p>
                                    </div>

                                    <div class="col-md-6 mb-3">
                                        <div class="form-label-container">
                                            <asp:Label ID="lblEmailRep" CssClass="form-label" Text="Repetir Email" runat="server" />
                                            <span class="badge-forms badge-required-forms">Requerido</span>
                                        </div>
                                        <asp:TextBox 
                                            AutoPostBack="true" 
                                            ID="txtEmailRep" 
                                            CssClass="form-control" 
                                            OnTextChanged="txtEmailRep_TextChanged" 
                                            placeholder="Email@Ejemplo.com" 
                                            aria-required="true"
                                            runat="server" />
                                        <p class="form-label-description">Confirme su correo electrónico</p>
                                        <asp:Panel ID="panelErrorMail" runat="server" CssClass="invalid-feedback" Visible="false">
                                            <p><asp:Label ID="lblErrorMail" runat="server" /></p>
                                        </asp:Panel>
                                    </div>

                                    <div class="col-md-6 mb-3">
                                        <div class="form-label-container">
                                            <asp:Label ID="lblContrasenia" CssClass="form-label" Text="Contraseña" runat="server" />
                                            <span class="badge-forms badge-required-forms">Requerido</span>
                                        </div>
                                        <asp:TextBox 
                                            type="password" 
                                            CssClass="form-control" 
                                            ID="txtPass" 
                                            placeholder="Contraseña" 
                                            aria-required="true"
                                            runat="server" />
                                        <p class="form-label-description">Ingrese una contraseña segura</p>
                                    </div>
                                    
                                    <div class="col-md-6 mb-3">
                                        <div class="form-label-container">
                                            <asp:Label ID="lblPassRep" CssClass="form-label" Text="Repetir Contraseña" runat="server" />
                                            <span class="badge-forms badge-required-forms">Requerido</span>
                                        </div>
                                        <asp:TextBox 
                                            AutoPostBack="true" 
                                            type="password" 
                                            ID="txtPassRep" 
                                            placeholder="Contraseña" 
                                            CssClass="form-control" 
                                            OnTextChanged="txtPassRep_TextChanged" 
                                            aria-required="true"
                                            runat="server" />
                                        <p class="form-label-description">Confirme su contraseña</p>
                                        <asp:Panel ID="panelErrorPass" runat="server" CssClass="invalid-feedback" Visible="false">
                                            <p><asp:Label ID="lblErrorPass" runat="server" /></p>
                                        </asp:Panel>
                                    </div>
                                </div>

                                <div class="mt-3">
                                    <asp:Label ID="lblMensaje" CssClass="d-block text-center" runat="server" />
                                </div>

                                <div class="d-flex justify-content-end align-items-center mt-4">
                                    <a href="Login.aspx" class="access" style="margin-right: auto;">Volver a iniciar sesión</a>
                                    <asp:Button ID="Button2" Text="Registrar" runat="server" OnClick="btnRegistrar_Click" CssClass="btn btn-primary"/>
                                </div>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>