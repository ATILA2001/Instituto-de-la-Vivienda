<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ObrasAdmin.aspx.cs" Inherits="WebForms.ObrasAdmin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

	<style>
		.table-3d {
			box-shadow: 0 10px 15px rgba(0, 0, 0, 0.2), 0 4px 6px rgba(0, 0, 0, 0.1);
			transform: translateY(-5px);
			transition: transform 0.3s ease, box-shadow 0.3s ease;
			border-radius: 15px; /* Borde redondeado */
			overflow: hidden; /* Para que el contenido no sobresalga de los bordes */
		}

			.table-3d:hover {
				box-shadow: 0 15px 25px rgba(0, 0, 0, 0.3), 0 8px 10px rgba(0, 0, 0, 0.2);
				transform: translateY(-10px);
			}
	</style>
	<div class="container-fluid mt-4">
		<div class="row">
			<div class="col-md-12  rounded-3 p-3">
				<div class="mx-auto p-2">
					<div class="card-body">
						<table class="table  table-3d">
							<thead class="thead-dark" style="color: #fad404">
								<tr>
									<th>Area</th>
									<th>Empresa</th>
									<th>Contrata</th>
									<th>Número</th>
									<th>Año</th>
									<th>Etapa</th>
									<th>Obra N°</th>
									<th>Barrio</th>
									<th>Nombre de Obra</th>
									<th></th>
									<th></th>
								</tr>
							</thead>
							<tbody>
								<tr>
									<td>
										<asp:DropDownList ID="ddlArea" CssClass="form-control" runat="server"></asp:DropDownList>
									</td>
									<td>
										<asp:DropDownList ID="ddlEmpresa" CssClass="form-control" runat="server"></asp:DropDownList>
									</td>
									<td>
										<asp:DropDownList ID="ddlContrata" CssClass="form-control" runat="server"></asp:DropDownList>
									</td>
									<td>
										<asp:TextBox ID="txtNumero" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
									</td>
									<td>
										<asp:TextBox ID="txtAño" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
									</td>
									<td>
										<asp:TextBox ID="txtEtapa" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
									</td>
									<td>
										<asp:TextBox ID="txtObra" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
									</td>
									<td>
										<asp:DropDownList ID="ddlBarrio" CssClass="form-control" runat="server"></asp:DropDownList>
									</td>
									<td>
										<asp:TextBox ID="txtDescripcion" CssClass="form-control" runat="server" TextMode="Multiline" />
									</td>
									<td class="text-right">
										<asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click"
											CssClass="btn btn-outline-success" runat="server" />
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
		<hr />

		<div class="row mt-4">
			<div class="col-md-12">


				<div class="text-end">
					<div class="d-flex flex-wrap justify-content-between p-3 gap-3">
						<div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">
							<div class="form-group">
								<label class="form-label lbl-left" for="ddlFiltroEmpresa">Empresa:</label>
								<asp:DropDownList ID="ddlFiltroEmpresa" runat="server" AutoPostBack="True"  Width="300px" OnSelectedIndexChanged="ddlFiltroEmpresa_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White">
								</asp:DropDownList>
							</div>
							<div class="form-group">
								<label class="form-label lbl-left" for="ddlBarrioFiltro">Barrio:</label>
								<asp:DropDownList ID="ddlBarrioFiltro" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlBarrioFiltro_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White">
								</asp:DropDownList>
							</div>
							<div class="form-group">
								<label class="form-label lbl-left" for="ddlAreaFiltro">Area:</label>
								<asp:DropDownList ID="ddlAreaFiltro" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlAreaFiltro_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White">
								</asp:DropDownList>
							</div>
						</div>
					</div>
				</div>







				<asp:GridView ID="dgvObra" DataKeyNames="ID" CssClass="table"
					OnSelectedIndexChanged="dgvObra_SelectedIndexChanged"
					OnRowDeleting="dgvObra_RowDeleting"
					AutoGenerateColumns="false" runat="server"
					Style="display: block; overflow-x: auto; white-space: nowrap; width: 100%;">

					<Columns>
						<asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />
						<asp:BoundField HeaderText="Área" DataField="Area" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Empresa" DataField="Empresa" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Contrata" DataField="Contrata" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Número" DataField="Numero" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Año" DataField="Año" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Etapa" DataField="Etapa" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Obra N°" DataField="ObraNumero" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Barrio" DataField="Barrio" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Nombre de Obra" DataField="Descripcion" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Disponible Inicial" DataField="AutorizadoInicial" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:C}" />
						<asp:BoundField HeaderText="Disponible Actual" DataField="AutorizadoNuevo" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:C}" />
						<asp:BoundField HeaderText="Suma Autorizantes" DataField="MontoAutorizante" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:C}" />
						<asp:BoundField HeaderText="Suma Certificados" DataField="MontoCertificado" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:C}" />
						<asp:BoundField HeaderText="Ejecucion actual" DataField="Porcentaje" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" DataFormatString="{0:N2}%" />
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
<style>
	.form-control-uniform {
		display: inline-block;
		font-size: 14px; /* Tamaño de texto uniforme */
		padding: 6px 12px;
		margin-top: -6px;
		border: 1px solid;
	}

	.btn {
		margin-top: -4px;
		border: 1px solid;
	}



	.lbl-left {
		text-align: left;
		display: block; /* Asegura que el label ocupe toda la línea si es necesario */
		font-weight: bold; /* Si necesitas enfatizar el texto */
	}

</style>
</asp:Content>
