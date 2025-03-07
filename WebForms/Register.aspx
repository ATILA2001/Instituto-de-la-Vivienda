<%@ Page Title="" Language="C#" MasterPageFile="~/LoginRegister.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="WebForms.Register" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <div class="d-flex justify-content-center align-items-center" style="min-height: 80vh;">

        <div class="container-fluid" style="max-width: 70rem;">
              <h1 class="text-center p-4 fs-1">Registrate</h1>
            <div class="card shadow-lg border-0">
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
                                                runat="server" />
                                        </div>
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
        <div class="text-center p-4">
            <asp:Label ID="lblMensaje" Text="" runat="server" />
        </div>

        <div class="text-end m-2">
            <asp:Button Text="Volver" ID="Button1" class="btn btn-outline-dark m-1" OnClick="btnVolver_Click" runat="server" />
            <asp:Button ID="Button2"  Text="Registrar" runat="server" OnClick="btnRegistrar_Click" class="btn btn-outline-dark mx-3"/>
        </div>

                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>



    </div>
    <style>
	.btn {
		background-color: #153244;
		text-align: center;
		transition: all 0.3s ease-in-out;
		color: #ecf0f1;
		border: none;
		padding: 8px 12px;
		font-size: 14px;
		cursor: pointer;
		border-radius: 4px;
		display: inline-block;
		font-weight: bold;
	}

		.btn:hover {
			background-color: #8DE2D6;
			color: #153244;
		}
</style>
</asp:Content>
