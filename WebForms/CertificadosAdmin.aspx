<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="CertificadosAdmin.aspx.cs" Inherits="WebForms.CertificadosAdmin" %>

<%@ Register Src="~/CustomControls/TreeViewSearch.ascx" TagPrefix="CustomControls" TagName="TreeViewSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Modal -->
    <div class="modal fade" id="modalAgregar" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h1 class="modal-title fs-5" id="staticBackdropLabel">Agregar Certificado</h1>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <div class="container">
                            <div class="row">
                                <div id="autorizanteContainer" class="col-12">
                                    <div class="mb-3">
                                        <label for="ddlAutorizante" class="form-label">Código Autorizante</label>
                                        <asp:DropDownList ID="ddlAutorizante" CssClass="form-select" runat="server" AppendDataBoundItems="true">
                                            <asp:ListItem Value="" Text="Seleccione un autorizante" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvAutorizante"
                                            ControlToValidate="ddlAutorizante"
                                            ValidationGroup="AgregarCertificado"
                                            runat="server"
                                            ErrorMessage="Seleccione un autorizante"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true"
                                            InitialValue="" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtExpediente" class="form-label">Expediente</label>
                                        <asp:TextBox ID="txtExpediente" CssClass="form-control" runat="server" placeHolder="xxxxxxxx/25" />
                                        
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="ddlTipo" class="form-label">Tipo</label>
                                        <asp:DropDownList ID="ddlTipo" CssClass="form-select" runat="server" AppendDataBoundItems="true">
                                            <asp:ListItem Value="" Text="Seleccione un tipo" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvTipo"
                                            ControlToValidate="ddlTipo"
                                            ValidationGroup="AgregarCertificado"
                                            runat="server"
                                            ErrorMessage="Seleccione un tipo"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true"
                                            InitialValue="" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtMontoAutorizado" class="form-label">Monto Autorizado</label>
                                        <asp:TextBox ID="txtMontoAutorizado" CssClass="form-control" runat="server" placeHolder="0,00" />
                                        <asp:RequiredFieldValidator ID="rfvMontoAutorizado"
                                            ControlToValidate="txtMontoAutorizado"
                                            ValidationGroup="AgregarCertificado"
                                            runat="server"
                                            ErrorMessage="El monto es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                        <asp:RegularExpressionValidator ID="revMontoAutorizado"
                                            ControlToValidate="txtMontoAutorizado"
                                            ValidationGroup="AgregarCertificado"
                                            runat="server"
                                            ValidationExpression="^[0-9]+(\,[0-9]{1,2})?$"
                                            ErrorMessage="Solo números positivos con hasta 2 decimales"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtFecha" class="form-label">Mes Aprobacion</label>
                                        <asp:TextBox ID="txtFecha" CssClass="form-control" runat="server" TextMode="Date" />
                                        <asp:RequiredFieldValidator ID="rfvFecha"
                                            ControlToValidate="txtFecha"
                                            ValidationGroup="AgregarCertificado"
                                            runat="server"
                                            ErrorMessage="El mes de aprobación es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer d-flex justify-content-between px-4">
                    <button type="button" class="btn btn-secondary" onclick="limpiarFormulario()">Limpiar</button>
                    <div class="d-flex gap-4">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" ValidationGroup="AgregarCertificado" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- /Modal -->

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
                            data-bs-toggle="tooltip" data-bs-placement="top" title="Buscar">
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


                    <div class="form-group mb-2">
                        <asp:LinkButton ID="btnShowAddModal" runat="server" CssClass="btn btn-primary" OnClick="btnShowAddModal_Click">
        <i class="bi bi-plus-lg"></i> Agregar
                        </asp:LinkButton>
                    </div>

                </div>
            </div>
        </div>

        <hr class="mb-3" />

        <div class="gridview-scroll-container">
            <asp:GridView ID="dgvCertificado" DataKeyNames="ID" CssClass="table1  table-bordered table-hover  mb-4"
                OnSelectedIndexChanged="dgvCertificado_SelectedIndexChanged"
                OnRowDeleting="dgvCertificado_RowDeleting"
                OnDataBound="dgvCertificado_DataBound"
                ShowHeaderWhenEmpty="true"
                AutoGenerateColumns="false" AllowPaging="true" PageSize="12" OnPageIndexChanging="dgvCertificado_PageIndexChanging" runat="server">
                <Columns>
                    <asp:BoundField HeaderText="ID" DataField="Autorizante.Id" Visible="false" />

                    <asp:TemplateField HeaderText="Area">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderArea" runat="server"
                                HeaderText="Área"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />

                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Autorizante.Obra.Area.Nombre") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField HeaderText="Contrata" DataField="Autorizante.Obra.Contrata.Nombre" />
                    <asp:BoundField HeaderText="Obra" DataField="Autorizante.Obra.Descripcion" />

                    <asp:TemplateField HeaderText="Barrio">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderBarrio" runat="server"
                                HeaderText="Barrio"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />

                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Autorizante.Obra.Barrio.Nombre") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Proyecto">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderProyecto" runat="server"
                                HeaderText="Proyecto"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />

                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Autorizante.Obra.Proyecto.Proyecto") %>
                        </ItemTemplate>
                    </asp:TemplateField>

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

                    <asp:TemplateField HeaderText="Codigo Autorizante">
                        <HeaderTemplate>

                            <CustomControls:TreeViewSearch ID="cblsHeaderCodigoAutorizante" runat="server"
                                HeaderText="Código Autorizante"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Autorizante.CodigoAutorizante") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Expediente">
                        <ItemTemplate>
                            <asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("ExpedientePago") %>' AutoPostBack="true"
                                OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Estado">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderEstado" runat="server"
                                HeaderText="Estado"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />

                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Estado") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Tipo">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderTipo" runat="server"
                                HeaderText="Tipo"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />

                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Tipo.Nombre") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField HeaderText="Monto Certificado" DataField="MontoTotal" DataFormatString="{0:C}" />

                    <asp:TemplateField HeaderText="Mes Certificado">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderMesCertificado" runat="server"
                                HeaderText="Mes Certificado"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("MesAprobacion", "{0:dd-MM-yyyy}") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Linea">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderLinea" runat="server"
                                HeaderText="Linea"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />

                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Autorizante.Obra.LineaGestion.Nombre") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField HeaderText="Porcentaje" DataField="Porcentaje" DataFormatString="{0:N2}%" />
                    <asp:BoundField HeaderText="Sigaf" DataField="Sigaf" DataFormatString="{0:C}" />
                    <asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade" />
                    <asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" />

                    <asp:TemplateField HeaderText="Acciones">
                        <ItemTemplate>
                            <div class="d-flex justify-content-center gap-2">
                                <asp:LinkButton ID="btnModificar" runat="server"
                                    CommandName="Select"
                                    CssClass="btn btn-sm btn-warning "
                                    ToolTip="Modificar">
                                    <i class="bi bi-pencil-square"></i>
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnEliminar" runat="server"
                                    CommandName="Delete"
                                    CssClass="btn btn-sm btn-danger "
                                    ToolTip="Eliminar"
                                    OnClientClick="return confirm('¿Está seguro que desea eliminar este registro?');">
                                    <i class="bi bi-trash"></i>
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>

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


    <script type="text/javascript">
        function limpiarFormulario() {
            document.getElementById('<%= txtExpediente.ClientID %>').value = '';
            document.getElementById('<%= txtMontoAutorizado.ClientID %>').value = '';
            document.getElementById('<%= txtFecha.ClientID %>').value = '';
            document.getElementById('<%= ddlAutorizante.ClientID %>').selectedIndex = 0;
            document.getElementById('<%= ddlTipo.ClientID %>').selectedIndex = 0;
        }
    </script>

</asp:Content>
