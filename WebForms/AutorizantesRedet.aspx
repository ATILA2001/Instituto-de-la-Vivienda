<%@ Page Title="" Language="C#" MasterPageFile="~/Redet.Master" AutoEventWireup="true" CodeBehind="AutorizantesRedet.aspx.cs" Inherits="WebForms.AutorizantesRedet" %>

<%@ Register Src="~/CustomControls/TreeViewSearch.ascx" TagPrefix="CustomControls" TagName="TreeViewSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="row mt-4 mb-3">
        <div class="col-12">
            <div class="d-flex justify-content-between align-items-end flex-wrap gap-3">
                <!-- Contenedor de Filtros alineados a la izquierda -->
                <div class="d-flex flex-wrap gap-3">



                    <div class="form-group mb-2">
                        <label class="form-label ms-2 mb-0" for="txtSubtotal">Subtotal:</label>
                        <asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control form-control-uniform" ReadOnly="true" />
                    </div>
                </div>

                <!-- Contenedor de Botones alineados a la derecha -->
                <div class="d-flex gap-3">

                    <div class="form-group mb-2">
                        <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control" placeholder="Buscar..."></asp:TextBox>
                    </div>
                    <div class="form-group mb-2">
                        <%--<asp:Button CssClass="btn btn-primary" ID="btnFiltrar" Text="Filtrar" runat="server" OnClick="btnFiltrar_Click" />--%>
                        <asp:LinkButton ID="btnFiltrar" runat="server" CssClass="btn btn-primary" OnClick="btnFiltrar_Click"
                            data-bs-toggle="tooltip" data-bs-placement="top" title="Filtrar">
			<i class="bi bi-search"></i>
                        </asp:LinkButton>
                    </div>


                    <%-- logica que aparezca o desaparezca. copiar de david --%>
                    <div class="form-group mb-2">
                        <asp:LinkButton CssClass="btn btn-primary" ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClick="BtnClearFilters_Click"
                            data-bs-toggle="tooltip" data-bs-placement="top" title="Quita todos los filtros">
			<i class="bi bi-funnel"></i>
                        </asp:LinkButton>
                    </div>



                </div>
            </div>
        </div>

        <hr class="mb-3" />
        <div class="gridview-scroll-container">

            <asp:GridView ID="dgvAutorizante" DataKeyNames="CodigoAutorizante" CssClass="table1  table-bordered table-hover mb-4"
                OnRowDataBound="dgvAutorizante_RowDataBound"
                ShowHeaderWhenEmpty="true"
                AutoGenerateColumns="false" AllowPaging="true" PageSize="12" OnPageIndexChanging="dgvAutorizante_PageIndexChanging" runat="server">
                <Columns>
                    <asp:TemplateField HeaderText="Área">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderArea" runat="server" HeaderText="Área" DataTextField="Nombre" DataValueField="Id" OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Obra.Area.Nombre") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Obra">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderObra" runat="server" HeaderText="Obra" DataTextField="Nombre" DataValueField="Id" OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Obra.Descripcion") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField HeaderText="Contrata" DataField="Obra.Contrata.Nombre" />
                    <asp:TemplateField HeaderText="Empresa">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderEmpresa" runat="server"
                                HeaderText="Empresa"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Empresa") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField HeaderText="Código Autorizante" DataField="CodigoAutorizante" />
                    <asp:TemplateField HeaderText="Concepto">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderConcepto" runat="server" HeaderText="Concepto" DataTextField="Nombre" DataValueField="Id" OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Concepto.Nombre") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField HeaderText="Detalle" DataField="Detalle" />

                    <asp:BoundField HeaderText="Expediente" DataField="Expediente" />

                    <asp:TemplateField HeaderText="Estado">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderEstado" runat="server" HeaderText="Estado" DataTextField="Nombre" DataValueField="Id" OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Estado.Nombre") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField HeaderText="Monto Autorizado" DataField="MontoAutorizado" DataFormatString="{0:C}" />
                    <asp:BoundField HeaderText="Mes Aprobacion" DataField="Fecha" DataFormatString="{0:dd-MM-yyyy}" />
                </Columns>
                <EmptyDataTemplate>
                    <div class="text-center m-3">
                        <i class="bi bi-info-circle fs-4"></i>
                        <p class="mb-0">No hay elementos para mostrar o registros que coincidan con los filtros aplicados.</p>
                    </div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>

        <div class="text-center p-4">
            <asp:Label ID="lblMensaje" Text="" runat="server" />
        </div>
    </div>


</asp:Content>
