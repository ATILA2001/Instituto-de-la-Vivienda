<%@ Page Title="" Language="C#" MasterPageFile="~/LoginRegister.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="WebForms.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="<%= ResolveUrl("~/Styles/loading-animation.css") %>" />

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Pantalla de carga (inicialmente oculta) -->
    <div id="loading-animation" style="display: none;">
        <div class="loading-bg"></div>
        <img id="logo-3" src="<%= ResolveUrl("~/Images/IVC-Blanco.png") %>" alt="Logo IVC Blanco" />
        <img id="logo" src="<%= ResolveUrl("~/Images/IVC-Azul.png") %>" alt="Logo IVC Azul" class="logo-animation" />
        <img id="logo-2" src="<%= ResolveUrl("~/Images/IVC-Celeste.png") %>" alt="Logo IVC Celeste" class="logo-animation" />
    </div>

    <div class="container-fluid d-flex justify-content-center align-items-center">
        <div class="card" style="width: 100%; max-width: 1200px;">
            <div class="row g-0 h-100">
                <!-- Left panel -->
                <div class="col-md-4 text-white bg-dark d-flex flex-column justify-content-center rounded-2">
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
                                            <asp:Label ID="lblEmail" CssClass="form-label" Text="Correo electrónico o CUIL" runat="server" />
                                        </div>
                                        <asp:TextBox
                                            CssClass="form-control"
                                            ID="txtEmail"
                                            placeholder="Ingrese su correo electrónico"
                                            aria-required="true"
                                            runat="server" />
                                        <asp:RequiredFieldValidator
                                            ID="rfvEmail"
                                            ControlToValidate="txtEmail"
                                            ErrorMessage="Debe ingresar un correo válido o un CUIL correcto"
                                            CssClass="text-danger small"
                                            Display="Dynamic"
                                            ValidationExpression="^(([^@\s]+@[^@\s]+\.[^@\s]+)|((20|23|27|30|33)([0-9]{8})([0-9])))$"
                                            runat="server" />
                                    </div>

                                    <div class="mb-5">
                                        <div class="form-label-container">
                                            <asp:Label ID="lblContrasenia" CssClass="form-label" Text="Contraseña" runat="server" />
                                        </div>
                                        <asp:TextBox
                                            type="password"
                                            CssClass="form-control"
                                            ID="txtPass"
                                            placeholder="Ingrese su contraseña"
                                            aria-required="true"
                                            runat="server" />
                                        <asp:RequiredFieldValidator
                                            ID="rfvPassword"
                                            ControlToValidate="txtPass"
                                            ErrorMessage="La contraseña es requerida"
                                            CssClass="text-danger small"
                                            Display="Dynamic"
                                            runat="server" />
                                    </div>
                                </div>
                            </div>

                            <div class="mt-3">
                                <asp:Label ID="lblMensaje" CssClass="d-block text-center" runat="server" />
                            </div>

                            <div class="d-flex justify-content-end align-items-center mt-4">
                                <a href="Register.aspx" class="access" style="margin-right: auto;">Crear una cuenta</a>
                                <asp:Button ID="btnIniciar" OnClick="btnIniciar_Click" OnClientClick="return showLoadingAnimation();" CssClass="btn btn-primary" Text="Ingresar" runat="server" />
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <script src="<%= ResolveUrl("~/Scripts/loading-animation.js") %>"></script>

</asp:Content>
