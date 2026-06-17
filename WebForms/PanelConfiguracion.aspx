<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="PanelConfiguracion.aspx.cs" Inherits="WebForms.PanelConfiguracion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

	<h2 class="text-center p-2">Configuración de Planificación y Formulación</h2>

	<div class="container mt-4" style="max-width: 800px;">

		<!-- Sección 1: Estado global -->
		<div class="card mb-4">
			<div class="card-header">
				<h5 class="mb-0">Estado global</h5>
			</div>
			<div class="card-body">
				<div class="form-check form-switch mb-3">
					<input type="checkbox" class="form-check-input" id="chkPlanificacion" name="chkPlanificacion" runat="server"
						style="cursor: pointer;"
						onserverchange="chkPlanificacion_ServerChange"
						onchange="__doPostBack('<%= chkPlanificacion.UniqueID %>', '')" />
					<label class="form-check-label ms-1" for="chkPlanificacion">Planificación abierta</label>
				</div>
				<div class="form-check form-switch">
					<input type="checkbox" class="form-check-input" id="chkFormulacion" name="chkFormulacion" runat="server"
						style="cursor: pointer;"
						onserverchange="chkFormulacion_ServerChange"
						onchange="__doPostBack('<%= chkFormulacion.UniqueID %>', '')" />
					<label class="form-check-label ms-1" for="chkFormulacion">Formulación abierta</label>
				</div>
			</div>
		</div>

		<!-- Sección 2: Overrides por usuario (solo visible cuando planificación global está cerrada) -->
		<asp:Panel ID="panelOverrides" runat="server">
			<div class="card">
				<div class="card-header">
					<h5 class="mb-0">Configuración de planificación por usuario</h5>
					<small class="text-muted">Permite que usuarios específicos planifiquen aunque la planificación global esté cerrada.</small>
				</div>
				<div class="card-body p-0">
				<asp:GridView ID="dgvUsuariosVinculados" DataKeyNames="AuthUserId" CssClass="table table-bordered table-hover mb-0"
					AutoGenerateColumns="false" OnRowDataBound="dgvUsuariosVinculados_RowDataBound" runat="server">
						<Columns>
							<asp:BoundField HeaderText="Usuario" DataField="Nombre" />
							<asp:TemplateField HeaderText="Planificación habilitada" ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center">
								<ItemTemplate>
									<div class="form-check form-switch d-flex justify-content-center">
										<input type="checkbox" class="form-check-input" id="chkOverride" runat="server"
											style="cursor: pointer;"
											onserverchange="chkOverride_ServerChange"
											onchange="__doPostBack(this.name, '')" />
									</div>
								</ItemTemplate>
							</asp:TemplateField>
						</Columns>
					</asp:GridView>
					<asp:Panel ID="panelSinUsuarios" runat="server" Visible="false">
						<p class="text-muted text-center p-3 mb-0">No hay usuarios registrados aún. Los usuarios aparecen aquí al ingresar por primera vez al sistema.</p>
					</asp:Panel>
				</div>
			</div>
		</asp:Panel>

		<asp:Panel ID="panelPlanificacionAbierta" runat="server" Visible="false">
			<div class="alert alert-info">
				La planificación está abierta para todos — las configuraciones individuales no tienen efecto.
			</div>
		</asp:Panel>

	</div>

	<asp:Label ID="lblMensaje" Text="" runat="server" CssClass="container mt-2 d-block" />

</asp:Content>
