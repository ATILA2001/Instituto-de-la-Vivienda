<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="LineasGestionFF.aspx.cs" Inherits="WebForms.LineasGestionFF" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<div class="container-fluid mt-4">
		<div class="row">
			<div class="col-md-8 border-end">
				<h2 class="text-center p-2">Listado de Líneas de Gestión</h2>

				<asp:GridView ID="dgvLineaGestionFF" DataKeyNames="ID" CssClass="table1 table-bordered table-hover"
					OnSelectedIndexChanged="dgvLineaGestionFF_SelectedIndexChanged"
					OnRowDeleting="dgvLineaGestionFF_RowDeleting"
					AutoGenerateColumns="false" runat="server"
					Style="display: block; overflow-x: auto; white-space: nowrap;">
					<Columns>
						<asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />

						<asp:BoundField HeaderText="Línea de Gestión" DataField="LineaGestion.Nombre" />
						<asp:BoundField HeaderText="Nombre FF" DataField="Nombre"/>
						<asp:BoundField HeaderText="Grupo" DataField="LineaGestion.Grupo"/>
						<asp:BoundField HeaderText="Reparticion" DataField="LineaGestion.Reparticion"  />
						<asp:BoundField HeaderText="Tipo" DataField="LineaGestion.Tipo"  />

						<asp:BoundField HeaderText="Fuente" DataField="Fuente"  />

						<asp:CommandField ShowSelectButton="true" SelectText="Modificar"
							ControlStyle-CssClass="btn btn-outline-warning"
							 />

						<asp:CommandField ShowDeleteButton="true"
							ControlStyle-CssClass="btn btn-outline-danger"
							 />

					</Columns>
				</asp:GridView>

			</div>
			<div class="col-md-4 bg-light rounded-3 p-3">
				<h2 class="text-center p-2">Agregar Línea de Gestión</h2>
				<div class="mx-auto p-2">
					<div class="card">
						<div class="card-body">
							<div class="mb-2">
								<label for="ddlLineaGestion" class="form-label">Línea de Gestión</label>
								<asp:DropDownList ID="ddlLineaGestion" CssClass="form-select" runat="server"></asp:DropDownList>
							</div>

							<div class="mb-2">
								<label for="txtNombreFF" class="form-label">Nombre FF</label>
								<asp:TextBox ID="txtNombreFF" CssClass="form-control" runat="server" />
							</div>

							<div class="mb-2">
								<label for="txtFuente" class="form-label">Fuente</label>
								<asp:TextBox ID="txtFuente" CssClass="form-control" runat="server" />
							</div>

							<div class="mb-2 text-center p-2">
								<asp:Button Text="Agregar" ID="btnAgregarFF" OnClick="btnAgregarFF_Click"
									CssClass="btn btn-outline-success" runat="server" />
							</div>
						</div>
					</div>
				</div>

				<div class="text-center p-4">
					<asp:Label ID="lblMensaje" Text="" runat="server" />
				</div>
			</div>
		</div>
	</div>
	<style>
		.table-bordered th, .table-bordered td {
			border: 1px solid #dddddd;
			text-align: center;
		}

		.table-hover tbody tr:hover {
			background-color: #f2f2f2;
		}

		.table1 th, .table1 td {
			padding: 12px;
			font-size: 14px;
		}

		/* Bordes redondeados y sombras en el GridView */
		.table1 {
			border-radius: 10px;
			box-shadow: 0 2px 5px rgba(0, 0, 0, 0.15);
		}

			/* Mejorar el estilo de las celdas del encabezado */
			.table1 th {
				text-align: center;
				font-weight: bold;
				background: #f1c40f;
				color: white;
				border: 1px solid #f39c11;
			}
	</style>
</asp:Content>
