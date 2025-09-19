<%@ Page Title="" Language="C#" MasterPageFile="~/LoginRegister.Master" AutoEventWireup="true" CodeBehind="Authentication.aspx.cs" Inherits="WebForms.Authentication" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
	<link href="Styles/loading-animation.css" rel="stylesheet" />
	<link href="Styles/flip-animation.css" rel="stylesheet" />
	<script src="https://cdnjs.cloudflare.com/ajax/libs/gsap/3.11.4/gsap.min.js"></script>
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

				<!-- Right panel with flip container -->
				<div class="col-md-8 bg-white">
					<asp:ScriptManager ID="ScriptManager1" runat="server">
						<Scripts>
							<asp:ScriptReference Path="~/Scripts/loading-animation.js" />
						</Scripts>
					</asp:ScriptManager>

					<div id="flipContainer" class="flip-container">
						<div class="flipper">
							<!-- Front side - Login -->
							<div class="front">
								<asp:UpdatePanel ID="UpdatePanelLogin" runat="server">
									<ContentTemplate>
										<asp:Panel ID="pnlLogin" runat="server" DefaultButton="btnIniciar">
											<div class="p-4 p-md-5">
												<h2 class="mb-4">Iniciá sesión</h2>
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
																ValidationGroup="Login"
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
																ValidationGroup="Login"
																runat="server" />
														</div>
													</div>
												</div>

												<div class="mt-3">
													<asp:Label ID="lblLoginMensaje" CssClass="d-block text-center" runat="server" />
												</div>

												<div class="d-flex justify-content-end align-items-center mt-4">
													<a href="javascript:void(0);" onclick="flipCard();" class="access" style="margin-right: auto;">Crear una cuenta
                                                    </a>
													<asp:Button ID="btnIniciar" OnClick="btnIniciar_Click"
														OnClientClick="return showLoadingAnimation('Login');"
														CssClass="btn btn-primary" Text="Ingresar"
														ValidationGroup="Login" runat="server" />
												</div>
											</div>
										</asp:Panel>
									</ContentTemplate>
								</asp:UpdatePanel>
							</div>

							<!-- Back side - Register -->
							<div class="back">
								<asp:UpdatePanel ID="UpdatePanelRegister" runat="server">
									<ContentTemplate>
										<asp:Panel ID="pnlRegister" runat="server" DefaultButton="btnRegistrar">
											<div class="p-4 p-md-5">
												<h2 class="mb-4">Registro de usuario</h2>
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
															ValidationGroup="Register"
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
															ValidationGroup="Register"
															runat="server" />
													</div>

													<div class="col-md-6 mb-3">
														<div class="form-label-container">
															<asp:Label ID="lblRegisterEmail" CssClass="form-label" Text="Email" runat="server" />
														</div>
														<asp:TextBox
															type="email"
															CssClass="form-control"
															ID="txtRegisterEmail"
															placeholder="Email@Ejemplo.com"
															aria-required="true"
															runat="server" />
														<asp:RequiredFieldValidator
															ID="rfvRegisterEmail"
															ControlToValidate="txtRegisterEmail"
															ErrorMessage="El email es requerido"
															CssClass="text-danger small"
															Display="Dynamic"
															ValidationGroup="Register"
															runat="server" />
														<asp:RegularExpressionValidator
															ID="revEmail"
															ControlToValidate="txtRegisterEmail"
															ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
															ErrorMessage="Formato de email inválido"
															CssClass="text-danger small"
															Display="Dynamic"
															ValidationGroup="Register"
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
															ValidationGroup="Register"
															runat="server" />
														<asp:CompareValidator
															ID="cvEmailMatch"
															ControlToValidate="txtEmailRep"
															ControlToCompare="txtRegisterEmail"
															Operator="Equal"
															Type="String"
															ErrorMessage="Los emails no coinciden"
															CssClass="text-danger small"
															Display="Dynamic"
															ValidationGroup="Register"
															runat="server" />
													</div>
												</div>

												<div class="mt-3">
													<asp:Label ID="lblRegisterMensaje" CssClass="d-block text-center" runat="server" />
												</div>

												<div class="d-flex justify-content-end align-items-center mt-4">
													<a href="javascript:void(0);" onclick="flipCard();" class="access" style="margin-right: auto;">Volver a iniciar sesión
                                                    </a>
													<asp:Button ID="btnRegistrar" Text="Registrar"
														OnClick="btnRegistrar_Click"
														CssClass="btn btn-primary"
														ValidationGroup="Register" runat="server" />
												</div>
											</div>
										</asp:Panel>
									</ContentTemplate>
								</asp:UpdatePanel>
							</div>
						</div>
					</div>
				</div>
			</div>
		</div>
	</div>

	<script type="text/javascript">
		// Function to flip the card
		function flipCard() {
			document.getElementById('flipContainer').classList.toggle('flipped');
			return false;
		}

		// Function to run when async postback completes
		function pageLoad() {
			// Nothing special needed here since we're handling animations via direct JS functions
		}
    </script>
</asp:Content>
