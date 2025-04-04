<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="LineasGestion.aspx.cs" Inherits="WebForms.LineasGestion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<div class="container-fluid mt-4">
		<div class="row">
			<div class="col-md-8 border-end">
				<h2 class="text-center p-2">Listado de Líneas de Gestión</h2>
				<asp:GridView ID="dgvLineaGestion" DataKeyNames="ID" CssClass="table1  table-bordered table-hover "
					OnSelectedIndexChanged="dgvLineaGestion_SelectedIndexChanged"
					OnRowDeleting="dgvLineaGestion_RowDeleting"
					AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
					<Columns>
						<asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />
						<asp:BoundField HeaderText="Nombre" DataField="Nombre"  />
						<asp:BoundField HeaderText="Tipo" DataField="Tipo" />
						<asp:BoundField HeaderText="Grupo" DataField="Grupo"  />
						<asp:BoundField HeaderText="Repartición" DataField="Reparticion"/>
						<asp:CommandField ShowSelectButton="true" SelectText="Modificar"
							ControlStyle-CssClass="btn btn-warning"  />
						<asp:CommandField ShowDeleteButton="true"
							ControlStyle-CssClass="btn btn-danger" />
					</Columns>
				</asp:GridView>
			</div>
			<div class="col-md-4 bg-light rounded-3 p-3">
				<h2 class="text-center p-2">Agregar Línea de Gestión</h2>
				<div class="mx-auto p-2">
					<div class="card">
						<div class="card-body">
							<div class="mb-2">
								<label for="txtNombre" class="form-label">Nombre</label>
								<asp:TextBox ID="txtNombre" CssClass="form-control" runat="server" />
							</div>
							<div class="mb-2">
								<label for="txtTipo" class="form-label">Tipo</label>
								<asp:TextBox ID="txtTipo" CssClass="form-control" runat="server" />
							</div>
							<div class="mb-2">
								<label for="txtGrupo" class="form-label">Grupo</label>
								<asp:TextBox ID="txtGrupo" CssClass="form-control" runat="server" />
							</div>
							<div class="mb-2">
								<label for="txtReparticion" class="form-label">Repartición</label>
								<asp:TextBox ID="txtReparticion" CssClass="form-control" runat="server" />
							</div>
							<div class="mb-2 text-center p-2">
								<asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click"
									CssClass="btn btn-primary" runat="server" />
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
	
</asp:Content>
