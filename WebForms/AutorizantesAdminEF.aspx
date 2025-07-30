<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AutorizantesAdminEF.aspx.cs" Inherits="WebForms.AutorizantesAdminEF" %>

<%@ Register Src="~/CustomControls/TreeViewSearch.ascx" TagPrefix="CustomControls" TagName="TreeViewSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Modal -->
    <div class="modal fade" id="modalAgregar" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h1 class="modal-title fs-5" id="staticBackdropLabel">Agregar Autorizante</h1>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">

                    <div class="form-group">
                        <div class="container">
                            <div class="row">
                                <div id="obraContainer" class="col-12">
                                    <div class="mb-3">
                                        <label for="ddlObra" class="form-label">Obra</label>
                                        <asp:DropDownList ID="ddlObra" CssClass="form-select" runat="server" AppendDataBoundItems="true">
                                            <asp:ListItem Value="" Text="Seleccione una obra" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvObra"
                                            ControlToValidate="ddlObra"
                                            ValidationGroup="AgregarAutorizante"
                                            runat="server"
                                            ErrorMessage="Seleccione una obra"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true"
                                            InitialValue="" />
                                    </div>
                                </div>

                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtCodigoAutorizante" class="form-label">Código Autorizante</label>
                                        <asp:TextBox ID="txtCodigoAutorizante" CssClass="form-control" runat="server" placeHolder="XXX-XXX-XXXX" />
                                        <asp:RequiredFieldValidator ID="rfvCodigoAutorizante"
                                            ControlToValidate="txtCodigoAutorizante"
                                            ValidationGroup="AgregarAutorizante"
                                            runat="server"
                                            ErrorMessage="El código autorizante es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>

                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="ddlConcepto" class="form-label">Concepto</label>
                                        <asp:DropDownList ID="ddlConcepto" CssClass="form-select" runat="server" AppendDataBoundItems="true">
                                            <asp:ListItem Value="" Text="Seleccione un concepto" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvConcepto"
                                            ControlToValidate="ddlConcepto"
                                            ValidationGroup="AgregarAutorizante"
                                            runat="server"
                                            ErrorMessage="Seleccione un concepto"
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
                                        <label for="ddlEstado" class="form-label">Estado</label>
                                        <asp:DropDownList ID="ddlEstado" CssClass="form-select" runat="server" AppendDataBoundItems="true">
                                            <asp:ListItem Value="" Text="Seleccione un estado" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvEstado"
                                            ControlToValidate="ddlEstado"
                                            ValidationGroup="AgregarAutorizante"
                                            runat="server"
                                            ErrorMessage="Seleccione un estado"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true"
                                            InitialValue="" />
                                    </div>
                                </div>

                                <div class="col-12">
                                    <div class="mb-3">
                                        <label for="txtDetalle" class="form-label">Detalle</label>
                                        <asp:TextBox ID="txtDetalle" CssClass="form-control" runat="server" TextMode="MultiLine" Rows="3" />
                                        <asp:RequiredFieldValidator ID="rfvDetalle"
                                            ControlToValidate="txtDetalle"
                                            ValidationGroup="AgregarAutorizante"
                                            runat="server"
                                            ErrorMessage="El detalle es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>

                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtMontoAutorizado" class="form-label">Monto Autorizado</label>
                                        <asp:TextBox ID="txtMontoAutorizado" CssClass="form-control" runat="server" placeHolder="0.00" />
                                        <asp:RequiredFieldValidator ID="rfvMontoAutorizado"
                                            ControlToValidate="txtMontoAutorizado"
                                            ValidationGroup="AgregarAutorizante"
                                            runat="server"
                                            ErrorMessage="El monto es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                        <asp:RegularExpressionValidator ID="revMontoAutorizado"
                                            ControlToValidate="txtMontoAutorizado"
                                            ValidationGroup="AgregarAutorizante"
                                            runat="server"
                                            ValidationExpression="^[0-9]+(\.[0-9]{1,2})?$"
                                            ErrorMessage="Solo números positivos con hasta 2 decimales"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>

                                    <div class="mb-3">
                                        <label for="txtFecha" class="form-label">Fecha</label>
                                        <asp:TextBox ID="txtFecha" CssClass="form-control" runat="server" TextMode="Date" />
                                    </div>
                                </div>

                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtMes" class="form-label">Mes Base</label>
                                        <asp:TextBox ID="txtMes" CssClass="form-control" runat="server" TextMode="Date" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                </div>
                <div class="modal-footer d-flex justify-content-between px-4">
                    <asp:Button Text="Limpiar" ID="btnLimpiar" OnClick="btnLimpiar_Click" CssClass="btn btn-secondary" runat="server" />
                    <div class="d-flex gap-4">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <asp:Button Text="Agregar" ID="Button1" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" ValidationGroup="AgregarAutorizante" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- /Modal -->

    <div class="row mt-4 mb-3">
        <div class="col-12">
            <div class="d-flex  justify-content-end align-items-end flex-wrap gap-3">
                <!-- Contenedor de Filtros alineados a la izquierda -->
                <%--				<div class="d-flex flex-wrap gap-3">
					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="lblSubtotal">Subtotal:</label>
						<asp:Label ID="lblSubtotal" runat="server" CssClass="form-control-plaintext fw-bold" Text="Total: $0.00 (0 registros)" />
					</div>
				</div>--%>

                <!-- Contenedor de Botones alineados a la derecha -->
                <div class="d-flex gap-3">
                    <div class="form-group mb-2">
                        <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control" placeholder="Buscar..."></asp:TextBox>
                    </div>

                    <div class="form-group mb-2">
                        <button type="submit" class="btn btn-primary" runat="server" onserverclick="btnFiltrar_Click"
                            data-bs-toggle="tooltip" data-bs-placement="top" title="Filtrar">
                            <i class="bi bi-search"></i>
                        </button>
                    </div>

                    <div class="form-group mb-2">
                        <asp:LinkButton CssClass="btn btn-primary" ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClick="BtnClearFilters_Click"
                            data-bs-toggle="tooltip" data-bs-placement="top" title="Quita todos los filtros">
							<i class="bi bi-funnel"></i>
                        </asp:LinkButton>
                    </div>

                    <div class="form-group mb-2">
                        <asp:LinkButton ID="btnExportarExcel" runat="server" CssClass="btn btn-success" OnClick="btnExportarExcel_Click"
                            data-bs-toggle="tooltip" data-bs-placement="top" title="Exportar a Excel">
							<i class="bi bi-download"></i>
                        </asp:LinkButton>
                    </div>

                    <div class="form-group mb-2">
                        <button type="submit" class="btn btn-primary" runat="server" onserverclick="btnShowAddModal_Click">
                            <i class="bi bi-plus-lg"></i>Agregar
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <hr class="mb-3" />

    <div class="gridview-scroll-container">
        <asp:GridView ID="dgvAutorizante" DataKeyNames="CodigoAutorizante" CssClass="table1 table-bordered table-hover mb-4"
            OnSelectedIndexChanged="dgvAutorizante_SelectedIndexChanged"
            OnRowDeleting="dgvAutorizante_RowDeleting"
            OnDataBound="dgvAutorizante_DataBound"
            OnRowDataBound="dgvAutorizante_RowDataBound"
            OnPageIndexChanging="dgvAutorizante_PageIndexChanging"
            ShowHeaderWhenEmpty="true"
            AutoGenerateColumns="false"
            AllowPaging="false"
            AllowCustomPaging="false"
            runat="server">
            <Columns>

                <asp:TemplateField>
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblsHeaderArea" runat="server"
                            HeaderText="Area"
                            DataTextField="Nombre"
                            DataValueField="Id"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("AreaNombre") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField HeaderText="Obra" DataField="ObraId" Visible="false" />
                <asp:TemplateField>
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblsHeaderObra" runat="server"
                            HeaderText="Obra"
                            DataTextField="Descripcion"
                            DataValueField="Id"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("ObraDescripcion") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField HeaderText="Contrata" DataField="Contrata" />

                <asp:TemplateField>
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblsHeaderEmpresa" runat="server"
                            HeaderText="Empresa"
                            DataTextField="Nombre"
                            DataValueField="Id"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("EmpresaNombre") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <%--				<asp:BoundField HeaderText="Codigo Autorizante" DataField="CodigoAutorizante" />--%>
                <asp:TemplateField>
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblsHeaderCodigoAutorizante" runat="server"
                            HeaderText="Codigo Autorizante"
                            DataTextField="CodigoAutorizante"
                            DataValueField="CodigoAutorizante"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("CodigoAutorizante") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField>
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblsHeaderConcepto" runat="server"
                            HeaderText="Concepto"
                            DataTextField="Nombre"
                            DataValueField="Id"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("ConceptoNombre") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField HeaderText="Detalle" DataField="Detalle" />

                <asp:TemplateField HeaderText="Expediente">
                    <ItemTemplate>
                        <asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("Expediente") %>' AutoPostBack="true"
                            OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm">
                        </asp:TextBox>
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
                        <asp:DropDownList ID="ddlEstadoAutorizante" runat="server" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlEstadoAutorizante_SelectedIndexChanged"
                            CssClass="btn btn-sm dropdown-toggle"
                            Style="background-color: white !important; color: #34495e !important; font-weight: normal; padding: 8px 12px; font-size: 14px;">
                        </asp:DropDownList>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField HeaderText="Monto Autorizado" DataField="MontoAutorizado" DataFormatString="{0:C}" />
                <asp:BoundField HeaderText="Mes Aprobacion" DataField="MesAprobacion" DataFormatString="{0:dd-MM-yyyy}" />
                <asp:BoundField HeaderText="Mes Base" DataField="MesBase" DataFormatString="{0:dd-MM-yyyy}" />

                <asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade" HeaderStyle-ForeColor="White" />
                <asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" />

                <asp:TemplateField HeaderText="Acciones">
                    <ItemTemplate>
                        <div class="d-flex justify-content-center gap-2">
                            <asp:LinkButton ID="btnModificar" runat="server"
                                CommandName="Select"
                                CssClass="btn btn-sm btn-warning"
                                ToolTip="Modificar">
								<i class="bi bi-pencil-square"></i>
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnEliminar" runat="server"
                                CommandName="Delete"
                                CssClass="btn btn-sm btn-danger"
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

    <!-- Controles de paginación independientes -->
    <div class="pagination-controls-container bg-light p-3 border rounded">
        <div class="d-flex justify-content-between align-items-center">
            <!-- Controles de paginación a la izquierda -->
            <div class="d-flex align-items-center gap-1">
                <!-- Primera página -->
                <asp:LinkButton ID="lnkFirst" runat="server" OnClick="lnkFirst_Click"
                    CssClass="btn btn-sm btn-outline-primary" ToolTip="Primera página">
					<i class="bi bi-chevron-double-left"></i>
                </asp:LinkButton>

                <!-- Página anterior -->
                <asp:LinkButton ID="lnkPrev" runat="server" OnClick="lnkPrev_Click"
                    CssClass="btn btn-sm btn-outline-primary" ToolTip="Página anterior">
					<i class="bi bi-chevron-left"></i>
                </asp:LinkButton>

                <!-- Botones de páginas estáticos -->
                <asp:LinkButton ID="lnkPage1" runat="server" OnClick="lnkPage_Click" CommandArgument="0" CssClass="btn btn-sm btn-outline-primary mx-1" Text="1" ToolTip="Ir a página 1" />
                <asp:LinkButton ID="lnkPage2" runat="server" OnClick="lnkPage_Click" CommandArgument="1" CssClass="btn btn-sm btn-outline-primary mx-1" Text="2" ToolTip="Ir a página 2" />
                <asp:LinkButton ID="lnkPage3" runat="server" OnClick="lnkPage_Click" CommandArgument="2" CssClass="btn btn-sm btn-outline-primary mx-1" Text="3" ToolTip="Ir a página 3" />
                <asp:LinkButton ID="lnkPage4" runat="server" OnClick="lnkPage_Click" CommandArgument="3" CssClass="btn btn-sm btn-outline-primary mx-1" Text="4" ToolTip="Ir a página 4" />
                <asp:LinkButton ID="lnkPage5" runat="server" OnClick="lnkPage_Click" CommandArgument="4" CssClass="btn btn-sm btn-outline-primary mx-1" Text="5" ToolTip="Ir a página 5" />


                <!-- Página siguiente -->
                <asp:LinkButton ID="lnkNext" runat="server" OnClick="lnkNext_Click"
                    CssClass="btn btn-sm btn-outline-primary" ToolTip="Página siguiente">
					<i class="bi bi-chevron-right"></i>
                </asp:LinkButton>

                <!-- Última página -->
                <asp:LinkButton ID="lnkLast" runat="server" OnClick="lnkLast_Click"
                    CssClass="btn btn-sm btn-outline-primary" ToolTip="Última página">
					<i class="bi bi-chevron-double-right"></i>
                </asp:LinkButton>

                <!-- Info de página -->
                <span class="mx-2 small text-muted">
                    <asp:Label ID="lblPaginaInfo" runat="server" Text="Página 1 de 1" />
                </span>
            </div>

            <!-- Centro: Subtotal -->
            <div class="text-center">
                <asp:Label ID="lblSubtotalPaginacion" runat="server" Text="Total: $0.00 (0 registros)" CssClass="badge text-dark fs-6" />
            </div>

            <!-- Dropdown de registros por página a la derecha -->
            <div class="d-flex align-items-center gap-2">
                <label class="form-label mb-0 small">Registros por página:</label>
                <asp:DropDownList ID="ddlPageSizeExternal" runat="server" CssClass="form-select form-select-sm" AutoPostBack="true" OnSelectedIndexChanged="ddlPageSizeExternal_SelectedIndexChanged" Style="width: auto;">
                    <asp:ListItem Value="12" Text="12" Selected="true"></asp:ListItem>
                    <asp:ListItem Value="24" Text="24"></asp:ListItem>
                    <asp:ListItem Value="48" Text="48"></asp:ListItem>
                    <asp:ListItem Value="96" Text="96"></asp:ListItem>
                    <asp:ListItem Value="192" Text="192"></asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
    </div>

    <div class="text-center p-4">
        <asp:Label ID="lblMensaje" Text="" runat="server" />
    </div>

    <script type="text/javascript">
        function limpiarFormulario() {
            document.getElementById('<%= txtCodigoAutorizante.ClientID %>').value = '';
            document.getElementById('<%= txtExpediente.ClientID %>').value = '';
            document.getElementById('<%= txtDetalle.ClientID %>').value = '';
            document.getElementById('<%= txtMontoAutorizado.ClientID %>').value = '';
            document.getElementById('<%= txtFecha.ClientID %>').value = '';
            document.getElementById('<%= txtMes.ClientID %>').value = '';
            document.getElementById('<%= ddlObra.ClientID %>').selectedIndex = 0;
            document.getElementById('<%= ddlConcepto.ClientID %>').selectedIndex = 0;
            document.getElementById('<%= ddlEstado.ClientID %>').selectedIndex = 0;
        }
    </script>
</asp:Content>
