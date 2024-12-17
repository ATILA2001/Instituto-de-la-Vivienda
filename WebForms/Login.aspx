<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="WebForms.Login" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
 <h1 class="text-center p-2">Inicia Sesión</h1>

    <div class="container-fluid" style="max-width: 28rem;">
        <div class="card mb-3">
            <div class="card-body">
                <div class="mb-3">
                    <asp:Label ID="lblEmail" CssClass="fs-5" Text="Email" runat="server" />
                    <asp:TextBox type="email" CssClass="form-control" ID="txtEmail" placeholder="Email@Ejemplo.com" runat="server" />
                </div>
                <div class="mb-3">
                    <asp:Label ID="lblContrasenia" CssClass="fs-5" Text="Contraseña" runat="server" />
                    <asp:TextBox type="password" CssClass="form-control" ID="txtPass" placeholder="Contraseña" runat="server" />
                </div>
                <div class="text-end">
                    <asp:Button Text="Volver" ID="btnVolver" CssClass="btn btn-outline-secondary" OnClick="btnVolver_Click" runat="server" />
                    <asp:Button ID="btnIniciar" OnClick="btnIniciar_Click" class="btn btn-outline-dark" Style="background-color: #99bbc2;" Text="Iniciar" runat="server" />
                </div>
            </div>
        </div>
        ¿Eres nuevo? <a href="Register.aspx">Crear cuenta</a>

    </div>
    <asp:ScriptManager runat="server" />
    <script type="text/javascript">
        // Función para detectar la tecla Enter
        function detectEnterKeyPress(event) {
            if (event.keyCode === 13) {
                event.preventDefault(); // Prevenir el comportamiento predeterminado del Enter
                document.getElementById('<%= btnIniciar.ClientID %>').click(); // Simular clic en el botón Iniciar sesión
            }
        }
        document.getElementById('<%= txtEmail.ClientID %>').addEventListener('keypress', detectEnterKeyPress);
        document.getElementById('<%= txtPass.ClientID %>').addEventListener('keypress', detectEnterKeyPress);
    </script>



</asp:Content>