<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="LegitimosAdmin.aspx.cs" Inherits="WebForms.LegitimosAdmin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    	<div class="row mt-4">
		<div class="col-md-12">
			<asp:GridView ID="dgvLegitimos" DataKeyNames="ID" CssClass="table"
    OnRowDeleting="dgvLegitimos_RowDeleting" AutoGenerateColumns="false" runat="server"
    style="display: block;overflow-x: auto;  white-space: nowrap; width: 100%;">
    <Columns>
        <asp:BoundField HeaderText="Area" DataField="Obra.Area.Nombre"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
        
        <asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
        <asp:BoundField HeaderText="Código Autorizante" DataField="CodigoAutorizante"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
        <asp:BoundField HeaderText="Expediente" DataField="Expediente"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
        <asp:BoundField HeaderText="Inicio Ejecución" DataField="InicioEjecucion" DataFormatString="{0:dd-MM-yyyy}"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
        <asp:BoundField HeaderText="Fin Ejecución" DataField="FinEjecucion" DataFormatString="{0:dd-MM-yyyy}"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
        <asp:BoundField HeaderText="Certificado" DataField="Certificado" DataFormatString="{0:C}"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
        <asp:BoundField HeaderText="Mes Aprobación" DataField="MesAprobacion" DataFormatString="{0:dd-MM-yyyy}"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
        <asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-outline-danger" DeleteText="Eliminar"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
    </Columns>
</asp:GridView>

			<div class="text-center p-4">
				<asp:Label ID="lblMensaje" Text="" runat="server" />
			</div>
		</div>
	</div>
</asp:Content>
