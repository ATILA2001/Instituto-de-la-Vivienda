<%@ Page Title="" Language="C#" MasterPageFile="~/User.Master" AutoEventWireup="true" CodeBehind="Certificados.aspx.cs" Inherits="WebForms.Certificados" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<style>
		.table-3d {
			box-shadow: 0 10px 15px rgba(0, 0, 0, 0.2), 0 4px 6px rgba(0, 0, 0, 0.1);
			transform: translateY(-5px);
			transition: transform 0.3s ease, box-shadow 0.3s ease;
			border-radius: 10px; /* Borde redondeado */
			overflow: hidden; /* Para que el contenido no sobresalga de los bordes */
		}

			.table-3d:hover {
				box-shadow: 0 15px 25px rgba(0, 0, 0, 0.3), 0 8px 10px rgba(0, 0, 0, 0.2);
				transform: translateY(-10px);
			}
	</style>
	<div class="container-fluid mt-4">
		<div class="row">
			<!-- Sección de Agregar Autorizante -->
			<div class="col-md-12  rounded-3 p-3">
				<div class="mx-auto p-2">
					<div class="card-body">
						<table class="table  table-3d">
							<thead class="thead-dark">
								<tr>

									<th>Código Autorizante</th>
									<th>Expediente</th>
									<th>Tipo</th>
									<th>Monto Autorizado</th>
									<th>Mes Aprobacion</th>
									<th></th>
									<th></th>
								</tr>
							</thead>
							<tbody>
								<tr>
									<!-- Dropdowns y TextBoxes para agregar -->


									<td>
										<asp:DropDownList ID="ddlAutorizante" CssClass="form-control" runat="server"></asp:DropDownList>
									</td>

									<td>
										<asp:TextBox ID="txtExpediente" CssClass="form-control" runat="server" />
									</td>

									<td>
										<asp:DropDownList ID="ddlTipo" CssClass="form-control" runat="server"></asp:DropDownList>
									</td>

									<td>
										<asp:TextBox ID="txtMontoAutorizado" CssClass="form-control" runat="server" />
									</td>
									<td>
										<asp:TextBox ID="txtFecha" CssClass="form-control" runat="server" TextMode="Date" />
									</td>
									<td class="text-right">
										<asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-outline-success" runat="server" />
									</td>
									<td class="text-right">
										<asp:Button Text="Limpiar" ID="btnLimpiar" OnClick="btnLimpiar_Click"
											CssClass="btn btn-outline-secondary ml-2" runat="server" /></td>
								</tr>
							</tbody>
						</table>
					</div>
				</div>
			</div>
		</div>
	</div>
	<hr />
	<div class="container-fluid mt-4">
		<div class="row mt-4">
			<div class="col-md-12">

				<div class="text-end">
					<!-- Alineación a la derecha -->
					<asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control form-control-uniform" />
					<asp:DropDownList ID="ddlAutorizanteFiltro" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlAutorizanteFiltro_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White">
					</asp:DropDownList>
					<asp:DropDownList ID="ddlTipoFiltro" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlTipoFiltro_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White">
					</asp:DropDownList>
					<asp:TextBox ID="txtMesAprobacionFiltro" runat="server" CssClass="form-control form-control-uniform" TextMode="Date" />
					<asp:Button ID="btnFiltrarMes" runat="server" CssClass="btn btn-sm btn-outline-dark ms-3" Text="Filtrar por Mes" OnClick="btnFiltrarMes_Click" />


					<br />
					<br />
				</div>



				<asp:GridView ID="dgvCertificado" DataKeyNames="ID" CssClass="table "
					OnSelectedIndexChanged="dgvCertificado_SelectedIndexChanged"
					OnRowDeleting="dgvCertificado_RowDeleting"
					AutoGenerateColumns="false" runat="server" style="display: block;  overflow-x: auto; white-space: nowrap; width: 100%;">
					<Columns>
						<asp:BoundField HeaderText="Obra" DataField="Autorizante.Obra.Descripcion" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Contrata" DataField="Autorizante.Obra.Contrata.Nombre" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Código Autorizante" DataField="Autorizante.CodigoAutorizante" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Expediente" DataField="ExpedientePago" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Tipo" DataField="Tipo.Nombre" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Monto Autorizado" DataField="MontoTotal" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Mes Aprobacion" DataField="MesAprobacion" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Porcentaje" DataField="Porcentaje" DataFormatString="{0:N2}%" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Sigaf" DataField="Sigaf" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

						<asp:CommandField ShowSelectButton="true" SelectText="Modificar" ControlStyle-CssClass="btn btn-outline-warning" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-outline-danger" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					</Columns>
				</asp:GridView>

				<div class="text-center p-4">
					<asp:Label ID="lblMensaje" Text="" runat="server" />
				</div>
			</div>
		</div>
	</div>

	<script type="text/javascript">
		function soloNumeros(e) {
			var tecla = (document.all) ? e.keyCode : e.which;
			if (tecla == 8 || tecla == 46) {
				return true;
			}
			var patron = /^[0-9]$/;
			var te = String.fromCharCode(tecla);
			return patron.test(te);
		}
	</script>
	<style>
		.form-control-uniform {
			display: inline-block;
			width: 135px;
			height: 33px; /* Altura similar a los DropDownList */
			font-size: 14px; /* Tamaño de texto uniforme */
			padding: 6px 12px;
			margin-top: 5px;
		}

		.btn {
			margin-top: -4px;
		}
	</style>
</asp:Content>
