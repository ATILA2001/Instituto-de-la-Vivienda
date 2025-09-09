<%@ Page Title="" Language="C#" MasterPageFile="~/LoginRegister.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="WebForms.Register" %>

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
						<h2 class="mb-4">Registro de usuario</h2>
						<asp:Panel runat="server">
							<div class="row" style="max-height: 450px; overflow-y: auto;">
								<div class="col-md-6 mb-3">
									<div class="form-label-container">
										<asp:Label ID="lblNombre" CssClass="form-label" Text="Nombre completo" runat="server" />
									</div>
									<asp:TextBox
										ID="txtNombre"
										CssClass="form-control"
										placeholder="Nombre y Apellido"
										aria-required="true"
										runat="server" />
									<asp:RequiredFieldValidator
										ID="rfvNombre"
										ControlToValidate="txtNombre"
										ErrorMessage="El nombre completo es requerido"
										CssClass="text-danger small"
										Display="Dynamic"
										runat="server" />

								</div>

								<div class="col-md-6 mb-3">
									<div class="form-label-container">
										<asp:Label ID="lblArea" CssClass="form-label" Text="Área" runat="server" />
									</div>
									<asp:DropDownList
										ID="ddlAreas"
										CssClass="form-control form-select"
										aria-label="Selección de área"
										aria-required="true"
										runat="server">
									</asp:DropDownList>
									<asp:RequiredFieldValidator
										ID="rfvArea"
										ControlToValidate="ddlAreas"
										InitialValue="0"
										ErrorMessage="Debe seleccionar un área"
										CssClass="text-danger small"
										Display="Dynamic"
										runat="server" />
								</div>


								<div class="col-md-6 mb-3">
									<div class="form-label-container">
										<asp:Label ID="lblEmail" CssClass="form-label" Text="Email" runat="server" />
									</div>
									<asp:TextBox
										type="email"
										CssClass="form-control"
										ID="txtEmail"
										placeholder="Email@Ejemplo.com"
										aria-required="true"
										runat="server" />
									<asp:RequiredFieldValidator
										ID="rfvEmail"
										ControlToValidate="txtEmail"
										ErrorMessage="El email es requerido"
										CssClass="text-danger small"
										Display="Dynamic"
										runat="server" />

								</div>

								<div class="col-md-6 mb-3">
									<div class="form-label-container">
										<asp:Label ID="lblEmailRep" CssClass="form-label" Text="Repetir Email" runat="server" />
									</div>
									<asp:TextBox
										ID="txtEmailRep"
										CssClass="form-control"
										placeholder="Email@Ejemplo.com"
										aria-required="true"
										runat="server" />
									<asp:RequiredFieldValidator
										ID="rfvEmailRep"
										ControlToValidate="txtEmailRep"
										ErrorMessage="Debe repetir el email"
										CssClass="text-danger small"
										Display="Dynamic"
										runat="server" />
									<asp:CompareValidator
										ID="cvEmailMatch"
										ControlToValidate="txtEmailRep"
										ControlToCompare="txtEmail"
										Operator="Equal"
										Type="String"
										ErrorMessage="Los emails no coinciden"
										CssClass="text-danger small"
										Display="Dynamic"
										runat="server" />
								</div>

								</div>

							<div class="mt-3">
								<asp:Label ID="lblMensaje" CssClass="d-block text-center" runat="server" />
							</div>

							<div class="d-flex justify-content-end align-items-center mt-4">
								<a href="Login.aspx" class="access" style="margin-right: auto;">Volver a iniciar sesión</a>
								<asp:Button ID="Button2" Text="Registrar" runat="server" OnClick="btnRegistrar_Click" CssClass="btn btn-primary" />
							</div>
						</asp:Panel>
					</div>
				</div>
			</div>
		</div>
	</div>
</asp:Content>
