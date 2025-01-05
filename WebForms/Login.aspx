<%@ Page Title="" Language="C#" MasterPageFile="~/LoginRegister.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="WebForms.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <!-- Título principal de la página -->
   

    <!-- Contenedor principal del formulario centrado verticalmente -->
    <div class="d-flex justify-content-center align-items-center" style="min-height: 80vh;">

        <div class="container-fluid" style="max-width: 28rem;">
             <h1 class="text-center p-4 fs-1">Inicia Sesión</h1>
            <div class="card shadow-lg border-0">
                <div class="card-body">
                    <!-- Email -->
                    <div class="mb-4">
                        <asp:Label ID="lblEmail" CssClass="fs-5" Text="Email" runat="server" />
                        <asp:TextBox type="email" CssClass="form-control" ID="txtEmail" placeholder="Email@Ejemplo.com" runat="server" />
                    </div>

                    <!-- Contraseña -->
                    <div class="mb-4">
                        <asp:Label ID="lblContrasenia" CssClass="fs-5" Text="Contraseña" runat="server" />
                        <asp:TextBox type="password" CssClass="form-control" ID="txtPass" placeholder="Contraseña" runat="server" />
                    </div>

                    <!-- Botones de acción -->
                    <div class="d-flex justify-content-between">
                                            <!-- Espacio a la izquierda -->
                    <div class="ms-auto">
                        <!-- Botón Iniciar alineado a la derecha -->
                        <asp:Button ID="btnIniciar" OnClick="btnIniciar_Click" class="btn btn-outline-dark" style="background-color: #f1c40f; border-color: #f1c40f;" Text="Iniciar" runat="server" />
                    </div>
                </div>
            </div>
        </div>


            <!-- Enlace para registro -->
            <div class="text-center mt-3">

                ¿Eres nuevo? <a href="Register.aspx" style="color: #34495e; font-weight: 500;">Crear cuenta</a>
            </div>
        </div>
    </div>

    <!-- Script para detección de la tecla Enter -->
    <asp:ScriptManager runat="server" />
    <script type="text/javascript">
        function detectEnterKeyPress(event) {
            if (event.keyCode === 13) {
                event.preventDefault();
                document.getElementById('<%= btnIniciar.ClientID %>').click();
            }
        }
        document.getElementById('<%= txtEmail.ClientID %>').addEventListener('keypress', detectEnterKeyPress);
        document.getElementById('<%= txtPass.ClientID %>').addEventListener('keypress', detectEnterKeyPress);
    </script>

</asp:Content>
