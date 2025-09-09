<%@ Page Title="" Language="C#" MasterPageFile="~/LoginRegister.Master" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="WebForms.Error" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
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
						<h1 class="text-center fs-2 p-2">Hubo un problema</h1>
						<br />
						<figure class="text-center">
							<asp:Label Text="" ID="lblMensaje" runat="server" />
							<br />
							<br />
							<br />
							<a href="Login.aspx" class="btn btn-primary">Volver</a>
						</figure>


					</div>
				</div>
			</div>
		</div>
	</div>
</asp:Content>
<%--<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <br />
    <h1 class="text-center fs-2 p-2">Hubo un problema</h1>
    <br />
    <figure class="text-center">
    <asp:Label Text="" ID="lblMensaje" runat="server" />
<br />
        <br />
        <br />
<a href="Login.aspx" class="btn btn-primary" Style="background-color: #99bbc2;" >Volver al inicio</a>
</figure>

</asp:Content>--%>
