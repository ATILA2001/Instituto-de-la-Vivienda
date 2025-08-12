<%@ Page Title="" Language="C#" MasterPageFile="~/LoginRegister.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="WebForms.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
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
                        <h2 class="mb-4">Iniciá sesión</h2>

                        <asp:Panel ID="pnlLogin" runat="server" DefaultButton="btnIniciar">
                            <div class="row" style="max-height: 450px; overflow-y: auto;">
                                <div class="col-12">
                                    <div class="mb-4">
                                        <div class="form-label-container">
                                            <asp:Label ID="lblEmail" CssClass="form-label" Text="Correo electrónico" runat="server" />
                                            <span class="badge-forms badge-required-forms">Requerido</span>
                                        </div>
                                        <asp:TextBox 
                                            type="email"
                                            CssClass="form-control" 
                                            ID="txtEmail" 
                                            placeholder="Ingrese su correo electrónico" 
                                            aria-required="true"
                                            runat="server" />
                                        <p class="form-label-description">Ingrese el email con el que se registró</p>
                                    </div>
                                    
                                    <div class="mb-5">
                                        <div class="form-label-container">
                                            <asp:Label ID="lblContrasenia" CssClass="form-label" Text="Contraseña" runat="server" />
                                            <span class="badge-forms badge-required-forms">Requerido</span>
                                        </div>
                                        <asp:TextBox 
                                            type="password" 
                                            CssClass="form-control" 
                                            ID="txtPass" 
                                            placeholder="Ingrese su contraseña" 
                                            aria-required="true"
                                            runat="server" />
                                        <p class="form-label-description">Ingrese su contraseña de acceso</p>
                                    </div>
                                </div>
                            </div>
                            
                            <div class="mt-3">
                                <asp:Label ID="lblMensaje" CssClass="d-block text-center" runat="server" />
                            </div>
                            
                            <div class="d-flex justify-content-end align-items-center mt-4">
                                <a href="Register.aspx" class="access" style="margin-right: auto;">Crear una cuenta</a>
                                <asp:Button ID="btnIniciar" OnClick="btnIniciar_Click" CssClass="btn btn-primary" Text="Ingresar" runat="server" />
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>