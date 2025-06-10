<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="MovimientosGestion.aspx.cs" Inherits="WebForms.MovimientosGestion" %>

<%@ Register Src="~/CustomControls/TreeViewSearch.ascx" TagPrefix="CustomControls" TagName="TreeViewSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Modal de Agregar/Modificar Movimiento -->
    <div class="modal fade" id="modalAgregar" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header">
                    <h1 class="modal-title fs-5" id="staticBackdropLabel">Agregar Movimiento</h1>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <div class="container">
                            <div class="row">
                                <div id="obraContainer" class="col-12">
                                    <div class="mb-3">
                                        <label for="ddlObra" class="form-label">Obra</label>
                                        <asp:DropDownList ID="ddlObra" CssClass="form-select" runat="server"></asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvObra"
                                            ControlToValidate="ddlObra"
                                            ValidationGroup="AgregarMovimiento"
                                            runat="server"
                                            ErrorMessage="Seleccione una obra"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true"
                                            InitialValue="" />
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="mb-3">
                                        <label for="txtMovimiento" class="form-label">Movimiento (Monto)</label>
                                        <asp:TextBox ID="txtMovimiento" CssClass="form-control" runat="server" placeholder="0,00" />
                                        <asp:RequiredFieldValidator ID="rfvMovimiento"
                                            ControlToValidate="txtMovimiento"
                                            ValidationGroup="AgregarMovimiento"
                                            runat="server"
                                            ErrorMessage="El monto es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                        <asp:RegularExpressionValidator ID="revMovimiento"
                                            ControlToValidate="txtMovimiento"
                                            ValidationGroup="AgregarMovimiento"
                                            runat="server"
                                            ValidationExpression="^[0-9]+(\,[0-9]{1,2})?$"
                                            ErrorMessage="Solo números positivos con hasta 2 decimales"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="mb-3">
                                        <label for="txtFecha" class="form-label">Fecha</label>
                                        <asp:TextBox ID="txtFecha" CssClass="form-control" runat="server" TextMode="Date" />
                                        <asp:RequiredFieldValidator ID="rfvFecha"
                                            ControlToValidate="txtFecha"
                                            ValidationGroup="AgregarMovimiento"
                                            runat="server"
                                            ErrorMessage="La fecha es requerida"
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
                        <asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" ValidationGroup="AgregarMovimiento" />
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
                            data-bs-toggle="tooltip"
                            data-bs-placement="top"
                            title="Quita todos los filtros">
					<i class="bi bi-funnel"></i>
                        </asp:LinkButton>
                    </div>


                    <%-- no hace falta logica script, abriria un modal --%>
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

            <asp:GridView ID="dgvMovimiento" DataKeyNames="ID" CssClass="table1  table-bordered table-hover mb-4"
                OnSelectedIndexChanged="dgvMovimiento_SelectedIndexChanged"
                OnRowDeleting="dgvMovimiento_RowDeleting"
                OnRowDataBound="dgvMovimiento_RowDataBound"
                ShowHeaderWhenEmpty="true"
                AutoGenerateColumns="false" AllowPaging="true" PageSize="12" OnPageIndexChanging="dgvMovimiento_PageIndexChanging" runat="server">
                <Columns>
                    <asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />
                    <asp:TemplateField HeaderText="Obra">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderObra" runat="server"
                                HeaderText="Obra"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Obra.Descripcion") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField HeaderText="Proyecto" DataField="Proyecto" />
                    <asp:BoundField HeaderText="SubProyecto" DataField="SubProyecto" />
                    <asp:BoundField HeaderText="Linea de Gestion" DataField="Linea" />


                    <asp:BoundField HeaderText="Movimiento" DataField="Monto" DataFormatString="{0:C}" />

                    <asp:TemplateField HeaderText="Fecha">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderFecha" runat="server"
                                HeaderText="Fecha"
                                DataTextField="Nombre"
                                DataValueField="Valor"
                                OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Fecha", "{0:dd-MM-yyyy}") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField HeaderText="Autorizado Nuevo" DataField="AutorizadoNuevo" DataFormatString="{0:C}" />
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
            document.getElementById('<%= txtMovimiento.ClientID %>').value = '';
        document.getElementById('<%= txtFecha.ClientID %>').value = '';
        document.getElementById('<%= ddlObra.ClientID %>').selectedIndex = 0;
        }
    </script>

</asp:Content>
