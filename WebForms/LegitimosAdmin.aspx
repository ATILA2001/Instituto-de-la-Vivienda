<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="LegitimosAdmin.aspx.cs" Inherits="WebForms.LegitimosAdmin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    	<div class="row mt-4">
		<div class="col-md-12">
			<asp:GridView ID="dgvLegitimos" DataKeyNames="ID" CssClass="table table-bordered table-hover table-3d"
    OnRowDeleting="dgvLegitimos_RowDeleting" AutoGenerateColumns="false" runat="server"
    AllowPaging="true" PageSize="10" OnPageIndexChanging="dgvLegitimos_PageIndexChanging">
    <Columns>
        <asp:BoundField HeaderText="Area" DataField="Obra.Area.Nombre" />
        
        <asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" />
        <asp:BoundField HeaderText="Código Autorizante" DataField="CodigoAutorizante" />
        <asp:BoundField HeaderText="Expediente" DataField="Expediente" />
        <asp:BoundField HeaderText="Inicio Ejecución" DataField="InicioEjecucion" DataFormatString="{0:dd-MM-yyyy}" />
        <asp:BoundField HeaderText="Fin Ejecución" DataField="FinEjecucion" DataFormatString="{0:dd-MM-yyyy}" />
        <asp:BoundField HeaderText="Certificado" DataField="Certificado" DataFormatString="{0:C}" />
        <asp:BoundField HeaderText="Mes Aprobación" DataField="MesAprobacion" DataFormatString="{0:dd-MM-yyyy}" />
        <asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-outline-danger" DeleteText="Eliminar" />
    </Columns>
</asp:GridView>

			<div class="text-center p-4">
				<asp:Label ID="lblMensaje" Text="" runat="server" />
			</div>
		</div>
	</div>
</asp:Content>
