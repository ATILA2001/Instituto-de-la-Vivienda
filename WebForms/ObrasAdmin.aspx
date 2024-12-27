<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ObrasAdmin.aspx.cs" Inherits="WebForms.ObrasAdmin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
        <div class="row mt-4">
        <div class="col-md-12">
            <asp:GridView ID="dgvObra" DataKeyNames="ID" CssClass="table table-bordered table-hover"
                OnSelectedIndexChanged="dgvObra_SelectedIndexChanged"
                OnRowDeleting="dgvObra_RowDeleting"
                AutoGenerateColumns="false" runat="server" AllowPaging="true"
                PageSize="10" OnPageIndexChanging="dgvObra_PageIndexChanging">
                <Columns>
                    <asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />
                    <asp:BoundField HeaderText="Área" DataField="Area" />
                    <asp:BoundField HeaderText="Empresa" DataField="Empresa" />                    
                    <asp:BoundField HeaderText="Contrata" DataField="Contrata" />
                    <asp:BoundField HeaderText="Número" DataField="Numero" />
                    <asp:BoundField HeaderText="Año" DataField="Año" />
                    <asp:BoundField HeaderText="Etapa" DataField="Etapa" />
                    <asp:BoundField HeaderText="Obra" DataField="ObraNumero" />
                    <asp:BoundField HeaderText="Barrio" DataField="Barrio" />
                    <asp:BoundField HeaderText="Descripción" DataField="Descripcion" />
                        <asp:BoundField HeaderText="Autorizado Inicial" DataField="AutorizadoInicial" />
                        
                        <asp:BoundField HeaderText="Autorizado Nuevo" DataField="AutorizadoNuevo" />
                          <asp:BoundField HeaderText="Monto Certificado" DataField="MontoCertificado" />
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
