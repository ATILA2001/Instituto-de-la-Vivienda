<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AutorizantesEF.aspx.cs" Inherits="WebForms.AutorizantesEF" %>

<%@ Register Src="~/CustomControls/TreeViewSearch/TreeViewSearch.ascx" TagPrefix="CustomControls" TagName="TreeViewSearch" %>
<%@ Register Src="~/CustomControls/PaginationControl/PaginationControl.ascx" TagPrefix="CustomControls" TagName="PaginationControl" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Modal agregar autorizante -->
    <div class="modal fade" id="modalAgregarAutorizante" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h1 class="modal-title fs-5">Agregar Autorizante</h1>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">

                    <div class="form-group">
                        <div class="container">
                            <div class="row">
                                <div id="obraContainer" class="col-12">
                                    <div class="mb-3">
                                        <label for="ddlObraAgregar" class="form-label">Obra</label>
                                        <asp:DropDownList ID="ddlObraAgregar" CssClass="form-select" runat="server" AppendDataBoundItems="true">
                                            <asp:ListItem Value="" Text="Seleccione una obra" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvObraAgregar"
                                            ControlToValidate="ddlObraAgregar"
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
                                        <label for="ddlConceptoAgregar" class="form-label">Concepto</label>
                                        <asp:DropDownList ID="ddlConceptoAgregar" CssClass="form-select" runat="server" AppendDataBoundItems="true">
                                            <asp:ListItem Value="" Text="Seleccione un concepto" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvConceptoAgregar"
                                            ControlToValidate="ddlConceptoAgregar"
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
                                        <label for="txtExpedienteAgregar" class="form-label">Expediente</label>
                                        <asp:TextBox ID="txtExpedienteAgregar" CssClass="form-control" runat="server" placeHolder="xxxxxxxx/25" />
                                    </div>
                                </div>

                                <div class="col-12">
                                    <div class="mb-3">
                                        <label for="txtDetalleAgregar" class="form-label">Detalle</label>
                                        <asp:TextBox ID="txtDetalleAgregar" CssClass="form-control" runat="server" TextMode="MultiLine" Rows="3" />
                                        <asp:RequiredFieldValidator ID="rfvDetalleAgregar"
                                            ControlToValidate="txtDetalleAgregar"
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
                                        <label for="txtMontoAutorizadoAgregar" class="form-label">Monto Autorizado</label>
                                        <asp:TextBox ID="txtMontoAutorizadoAgregar" CssClass="form-control" runat="server" placeHolder="0,00" />
                                        <asp:RequiredFieldValidator ID="rfvMontoAutorizadoAgregar"
                                            ControlToValidate="txtMontoAutorizadoAgregar"
                                            ValidationGroup="AgregarAutorizante"
                                            runat="server"
                                            ErrorMessage="El monto es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                        <asp:RegularExpressionValidator ID="revMontoAutorizadoAgregar"
                                            ControlToValidate="txtMontoAutorizadoAgregar"
                                            ValidationGroup="AgregarAutorizante"
                                            runat="server"
                                            ValidationExpression="^[0-9]+([,][0-9]{1,2})?$"
                                            ErrorMessage="Solo números positivos con hasta 2 decimales"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>


                                </div>

                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtMesAprobacionAgregar" class="form-label">Mes Aprobación</label>
                                        <asp:TextBox ID="txtMesAprobacionAgregar" CssClass="form-control" runat="server" TextMode="Date" />
                                        <asp:RequiredFieldValidator ID="rfvMesAprobacionAgregar" runat="server" ControlToValidate="txtMesAprobacionAgregar" ErrorMessage="Mes Aprobación es obligatorio" CssClass="text-danger" Display="Dynamic" ValidationGroup="AgregarAutorizante" />
                                    </div>
                                </div>


                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtMesBaseAgregar" class="form-label">Mes Base</label>
                                        <asp:TextBox ID="txtMesBaseAgregar" CssClass="form-control" runat="server" TextMode="Date" />
                                    </div>
                                </div>

                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="ddlEstadoAgregar" class="form-label">Estado</label>
                                        <asp:DropDownList ID="ddlEstadoAgregar" CssClass="form-select" runat="server" AppendDataBoundItems="true">
                                            <asp:ListItem Value="" Text="Seleccione un estado" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvEstadoAgregar"
                                            ControlToValidate="ddlEstadoAgregar"
                                            ValidationGroup="AgregarAutorizante"
                                            runat="server"
                                            ErrorMessage="Seleccione un estado"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true"
                                            InitialValue="" />
                                    </div>
                                </div>



                            </div>
                        </div>
                    </div>

                </div>
                <div class="modal-footer d-flex justify-content-between px-4">
                    <button type="button" class="btn btn-secondary" id="btnLimpiarAgregar" onclick="limpiarFormularioAgregar()">Limpiar</button>
                    <div class="d-flex gap-4">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" ValidationGroup="AgregarAutorizante" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- /Modal agregar autorizante-->

    <!-- Modal editar autorizante -->
    <div class="modal fade" id="modalEditarAutorizante" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h1 class="modal-title fs-5">Editar Autorizante</h1>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">

                    <div class="form-group">
                        <div class="container">
                            <div class="row">

                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="ddlConceptoEditar" class="form-label">Concepto</label>
                                        <asp:DropDownList ID="ddlConceptoEditar" CssClass="form-select" runat="server" AppendDataBoundItems="true">
                                            <asp:ListItem Value="" Text="Seleccione un concepto" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvConceptoEditar"
                                            ControlToValidate="ddlConceptoEditar"
                                            ValidationGroup="EditarAutorizante"
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
                                        <asp:TextBox ID="txtExpedienteEditar" CssClass="form-control" runat="server" placeHolder="xxxxxxxx/25" />
                                    </div>
                                </div>

                                <div class="col-12">
                                    <div class="mb-3">
                                        <label for="txtDetalleEditar" class="form-label">Detalle</label>
                                        <asp:TextBox ID="txtDetalleEditar" CssClass="form-control" runat="server" TextMode="MultiLine" Rows="3" />
                                        <asp:RequiredFieldValidator ID="rfvDetalleEditar"
                                            ControlToValidate="txtDetalleEditar"
                                            ValidationGroup="EditarAutorizante"
                                            runat="server"
                                            ErrorMessage="El detalle es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>

                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtMontoAutorizadoEditar" class="form-label">Monto Autorizado</label>
                                        <asp:TextBox ID="txtMontoAutorizadoEditar" CssClass="form-control" runat="server" placeHolder="0,00" />
                                        <asp:RequiredFieldValidator ID="rfvMontoAutorizadoEditar"
                                            ControlToValidate="txtMontoAutorizadoEditar"
                                            ValidationGroup="EditarAutorizante"
                                            runat="server"
                                            ErrorMessage="El monto es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                        <asp:RegularExpressionValidator ID="revMontoAutorizadoEditar"
                                            ControlToValidate="txtMontoAutorizadoEditar"
                                            ValidationGroup="EditarAutorizante"
                                            runat="server"
                                            ValidationExpression="^[0-9]+([,][0-9]{1,2})?$"
                                            ErrorMessage="Solo números positivos con hasta 2 decimales"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>

                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtMesAprobacionEditar" class="form-label">Mes Aprobación</label>
                                        <asp:TextBox ID="txtMesAprobacionEditar" CssClass="form-control" runat="server" TextMode="Date" />
                                        <asp:RequiredFieldValidator ID="rfvMesAprobacionEditar" runat="server" ControlToValidate="txtMesAprobacionEditar" ErrorMessage="Mes Aprobación es obligatorio" CssClass="text-danger" Display="Dynamic" ValidationGroup="EditarAutorizante" />
                                    </div>
                                </div>

                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtMesBase" class="form-label">Mes Base</label>
                                        <asp:TextBox ID="txtMesBaseEditar" CssClass="form-control" runat="server" TextMode="Date" />
                                    </div>
                                </div>

                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="ddlEstadoEditar" class="form-label">Estado</label>
                                        <asp:DropDownList ID="ddlEstadoEditar" CssClass="form-select" runat="server" AppendDataBoundItems="true">
                                            <asp:ListItem Value="" Text="Seleccione un estado" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvEstadoEditar"
                                            ControlToValidate="ddlEstadoEditar"
                                            ValidationGroup="EditarAutorizante"
                                            runat="server"
                                            ErrorMessage="Seleccione un estado"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true"
                                            InitialValue="" />
                                    </div>
                                </div>


                            </div>
                        </div>
                    </div>

                </div>
                <div class="modal-footer d-flex justify-content-between px-4">
                    <button type="button" class="btn btn-secondary" id="btnLimpiarEditar" onclick="limpiarFormularioEditar()">Limpiar</button>
                    <div class="d-flex gap-4">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <asp:Button Text="Editar" ID="btnEditar" OnClick="btnEditar_Click" CssClass="btn btn-primary" runat="server" ValidationGroup="EditarAutorizante" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- /Modal editar autorizante-->

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

                    <asp:Panel ID="panelShowAddButton" runat="server">
                        <div class="form-group mb-2">
                            <button type="submit" class="btn btn-primary" runat="server" onserverclick="btnShowAddModal_Click">
                                <i class="bi bi-plus-lg"></i>Agregar
                            </button>
                        </div>
                    </asp:Panel>

                </div>
            </div>
        </div>
    </div>

    <hr class="mb-3" />

    <div class="gridview-scroll-container">
        <asp:GridView ID="gridviewRegistros" DataKeyNames="Id,IdRedeterminacion" CssClass="table1 table-bordered table-hover mb-4"
            OnSelectedIndexChanged="gridviewRegistros_SelectedIndexChanged"
            OnRowDeleting="gridviewRegistros_RowDeleting"
            OnDataBound="gridviewRegistros_DataBound"
            OnRowDataBound="gridviewRegistros_RowDataBound"
            OnPageIndexChanging="gridviewRegistros_PageIndexChanging"
            ShowHeaderWhenEmpty="true"
            AutoGenerateColumns="false"
            AllowPaging="false"
            AllowCustomPaging="false"
            runat="server">
            <Columns>
                <asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />

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


                <asp:TemplateField>
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblsHeaderContrata" runat="server"
                            HeaderText="Contrata"
                            DataTextField="Nombre"
                            DataValueField="Id"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("Contrata") %>
                    </ItemTemplate>
                </asp:TemplateField>



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
                            OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm w-auto text-center">
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
                            CssClass="form-select form-select-sm w-auto text-center">
                        </asp:DropDownList>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField HeaderText="Monto Autorizado" DataField="MontoAutorizado" DataFormatString="{0:C}" />
                <asp:BoundField HeaderText="Mes Aprobacion" DataField="MesAprobacion" DataFormatString="{0:dd-MM-yyyy}" />
                <asp:BoundField HeaderText="Mes Base" DataField="MesBase" DataFormatString="{0:dd-MM-yyyy}" />

                <asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade" HeaderStyle-ForeColor="White" />
                <asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" />
                <asp:BoundField HeaderText="Gedo" DataField="Acdir" />

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

    <!-- Control de paginación reutilizable -->
    <CustomControls:PaginationControl ID="paginationControl" runat="server"
        OnPageChanged="paginationControl_PageChanged"
        OnPageSizeChanged="paginationControl_PageSizeChanged" />



    <script type="text/javascript">
        function limpiarFormularioAgregar() {
            document.getElementById('<%= ddlObraAgregar.ClientID %>').selectedIndex = 0;
                document.getElementById('<%= txtExpedienteAgregar.ClientID %>').value = '';
                document.getElementById('<%= txtDetalleAgregar.ClientID %>').value = '';
                document.getElementById('<%= txtMontoAutorizadoAgregar.ClientID %>').value = '';
                document.getElementById('<%= txtMesAprobacionAgregar.ClientID %>').value = '';
                document.getElementById('<%= txtMesBaseAgregar.ClientID %>').value = '';
                document.getElementById('<%= ddlConceptoAgregar.ClientID %>').selectedIndex = 0;
                document.getElementById('<%= ddlEstadoAgregar.ClientID %>').selectedIndex = 0;
        }

        function limpiarFormularioEditar() {
            document.getElementById('<%= txtExpedienteEditar.ClientID %>').value = '';
                document.getElementById('<%= txtDetalleEditar.ClientID %>').value = '';
                document.getElementById('<%= txtMontoAutorizadoEditar.ClientID %>').value = '';
                document.getElementById('<%= txtMesAprobacionEditar.ClientID %>').value = '';
                document.getElementById('<%= txtMesBaseEditar.ClientID %>').value = '';
                document.getElementById('<%= ddlConceptoEditar.ClientID %>').selectedIndex = 0;
                document.getElementById('<%= ddlEstadoEditar.ClientID %>').selectedIndex = 0;
        }
    </script>
</asp:Content>
