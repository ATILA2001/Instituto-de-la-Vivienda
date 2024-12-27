<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AutorizantesAdmin.aspx.cs" Inherits="WebForms.AutorizantesAdmin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

	<div class="row mt-4">
		<div class="col-md-12">
			<asp:GridView ID="dgvAutorizante" DataKeyNames="CodigoAutorizante" CssClass="table table-bordered table-hover"
				OnSelectedIndexChanged="dgvAutorizante_SelectedIndexChanged"
				OnRowDeleting="dgvAutorizante_RowDeleting"
				AutoGenerateColumns="false" runat="server" AllowPaging="true"
				PageSize="10" OnPageIndexChanging="dgvAutorizante_PageIndexChanging">
				<Columns>
					<asp:BoundField HeaderText="Área" DataField="Obra.Area.Nombre" />
					<asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" />

					<asp:BoundField HeaderText="Contrata" DataField="Obra.Contrata.Nombre" />
					<asp:BoundField HeaderText="Código Autorizante" DataField="CodigoAutorizante" />

					<asp:BoundField HeaderText="Concepto" DataField="Concepto" />
					<asp:BoundField HeaderText="Detalle" DataField="Detalle" />


					<asp:BoundField HeaderText="Expediente" DataField="Expediente" />

					<asp:BoundField HeaderText="Estado" DataField="Estado.Nombre" />

					<asp:BoundField HeaderText="Monto Autorizado" DataField="MontoAutorizado" DataFormatString="{0:C}" />

					<asp:BoundField HeaderText="Mes Aprobacion" DataField="Fecha" DataFormatString="{0:dd-MM-yyyy}" />

					<asp:TemplateField HeaderText="Autorización GG">
						<ItemTemplate>
							<%# Eval("AutorizacionGG") != null && (bool)Eval("AutorizacionGG") ? "AUTORIZADO" : "A AUTORIZAR" %>
						</ItemTemplate>
					</asp:TemplateField>



					<asp:CommandField ShowSelectButton="true" SelectText="Modificar" ControlStyle-CssClass="btn btn-outline-warning" />
					<asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-outline-danger" />
				</Columns>
			</asp:GridView>

			<div class="text-center p-4">
				<asp:Label ID="lblMensaje" Text="" runat="server" />
			</div>
		</div>
	</div>
</asp:Content>
