<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="Devengados.aspx.cs" Inherits="WebForms.Devengados" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

	<asp:GridView ID="dgvDevengados" DataKeyNames="Id" CssClass="table1  table-bordered table-hover"
		AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
		<Columns>
			<asp:BoundField HeaderText="ID" DataField="Id" Visible="false" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Ejercicio" DataField="Ejercicio" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Tipo Devengado" DataField="TipoDev" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Número Devengado" DataField="NumeroDevengado" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<%--<asp:BoundField HeaderText="Estado Firma" DataField="EstadoFirma" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Expediente" DataField="Expediente" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Ente" DataField="Ente" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			--%><asp:BoundField HeaderText="CUIT" DataField="Cuit" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Descripción" DataField="Descripcion" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<%--<asp:BoundField HeaderText="Obra" DataField="Obra" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />--%>
			<asp:BoundField HeaderText="Importe Devengado" DataField="ImporteDevengado" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<%--			<asp:BoundField HeaderText="Jurisdiccion" DataField="Jurisdiccion" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Subjurisdiccion" DataField="SubJurisdiccion" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Entidad" DataField="Entidad" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Programa" DataField="Programa" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Subprograma" DataField="SubPrograma" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Proyecto" DataField="Proyecto" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Actividad" DataField="Actividad" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Obra2" DataField="Obra2" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Inciso" DataField="Inciso" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Principal" DataField="Principal" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Parcial" DataField="Parcial" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Subparcial" DataField="SubParcial" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Fuente Financiera" DataField="FuenteFinanciera" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Ubicación Geo" DataField="UbicacionGeo" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />--%>
			<asp:BoundField HeaderText="Cuenta Escritural" DataField="CuentaEscritural" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Cuenta Pagadora" DataField="CuentaPagadora" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Importe PP" DataField="ImportePP" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Importe Pagado" DataField="ImportePagado" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Saldo a Pagar PP" DataField="SaldoAPagarPP" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Total Devengado" DataField="TotalDevengado" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Total Pagado" DataField="TotalPagado" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Deuda Dev" DataField="DeudaDev" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Fecha Imputación" DataField="FechaImputacion" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="EE Financiera" DataField="EeFinanciera" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="EE Autorizante" DataField="EeAutorizante" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Monto Devengado" DataField="MontoDevengado" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Concatenado" DataField="Concatenado" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
			<asp:BoundField HeaderText="Tipo" DataField="Tipo" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
		</Columns>
	</asp:GridView>
	<div class="text-center p-4">
		<asp:Label ID="lblMensaje" Text="" runat="server" />
	</div>
	
</asp:Content>
