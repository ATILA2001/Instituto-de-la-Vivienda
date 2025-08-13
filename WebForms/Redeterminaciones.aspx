<%@ Page Title="" Language="C#" MasterPageFile="~/Redet.Master" AutoEventWireup="true" CodeBehind="Redeterminaciones.aspx.cs" Inherits="WebForms.Redeterminaciones" %>

<%@ Register Src="~/CustomControls/TreeViewSearch/TreeViewSearch.ascx" TagPrefix="CustomControls" TagName="TreeViewSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Modal -->
    <div class="modal fade" id="modalAgregar" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h1 class="modal-title fs-5" id="staticBackdropLabel">Agregar Redeterminación</h1>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <div class="container">
                            <div class="row">
                                <div id="autorizanteContainer" class="col-12">
                                    <div class="mb-3">
                                        <label for="ddlAutorizante" class="form-label">Autorizante</label>
                                        <asp:DropDownList ID="ddlAutorizante" CssClass="form-select" runat="server"></asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvAutorizante"
                                            ControlToValidate="ddlAutorizante"
                                            ValidationGroup="AgregarRedeterminacion"
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
                                        <asp:TextBox ID="txtExpediente" CssClass="form-control" runat="server" />
                                        <asp:RequiredFieldValidator ID="rfvExpediente"
                                            ControlToValidate="txtExpediente"
                                            ValidationGroup="AgregarRedeterminacion"
                                            runat="server"
                                            ErrorMessage="El expediente es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtSalto" class="form-label">Salto</label>
                                        <asp:TextBox ID="txtSalto" CssClass="form-control" runat="server" TextMode="Date" />
                                        <asp:RequiredFieldValidator ID="rfvSalto"
                                            ControlToValidate="txtSalto"
                                            ValidationGroup="AgregarRedeterminacion"
                                            runat="server"
                                            ErrorMessage="El salto es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtNro" class="form-label">Nro</label>
                                        <asp:TextBox ID="txtNro" CssClass="form-control" runat="server" />
                                        <asp:RequiredFieldValidator ID="rfvNro"
                                            ControlToValidate="txtNro"
                                            ValidationGroup="AgregarRedeterminacion"
                                            runat="server"
                                            ErrorMessage="El número es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtTipo" class="form-label">Tipo</label>
                                        <asp:TextBox ID="txtTipo" CssClass="form-control" runat="server" />
                                        <asp:RequiredFieldValidator ID="rfvTipo"
                                            ControlToValidate="txtTipo"
                                            ValidationGroup="AgregarRedeterminacion"
                                            runat="server"
                                            ErrorMessage="El tipo es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="ddlEtapa" class="form-label">Etapa</label>
                                        <asp:DropDownList ID="ddlEtapa" CssClass="form-select" runat="server"></asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvEtapa"
                                            ControlToValidate="ddlEtapa"
                                            ValidationGroup="AgregarRedeterminacion"
                                            runat="server"
                                            ErrorMessage="Seleccione una etapa"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true"
                                            InitialValue="" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtPorcentaje" class="form-label">Porcentaje</label>
                                        <asp:TextBox ID="txtPorcentaje" CssClass="form-control" runat="server" />
                                        <asp:RequiredFieldValidator ID="rfvPorcentaje"
                                            ControlToValidate="txtPorcentaje"
                                            ValidationGroup="AgregarRedeterminacion"
                                            runat="server"
                                            ErrorMessage="El porcentaje es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>
                                <div class="col-12">
                                    <div class="mb-3">
                                        <label for="txtObservacion" class="form-label">Observaciones</label>
                                        <asp:TextBox ID="txtObservacion" CssClass="form-control" runat="server" TextMode="MultiLine" Rows="3" />
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
                        <asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" ValidationGroup="AgregarRedeterminacion" />
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

                <%--<div class="form-group">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblFecha">Fecha:</label>
						<div class="dropdown">
							<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownFecha" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownFecha" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblFecha" runat="server" CssClass="dropdown-item form-check" />
							</ul>
						</div>
					</div>--%>
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


            <asp:GridView ID="dgvRedeterminacion" DataKeyNames="ID" CssClass="table1 table-bordered table-hover  mb-4"
                OnSelectedIndexChanged="dgvRedeterminacion_SelectedIndexChanged"
                OnRowDeleting="dgvRedeterminacion_RowDeleting"
                OnRowDataBound="dgvRedeterminacion_RowDataBound"
                ShowHeaderWhenEmpty="true"
                AutoGenerateColumns="false" AllowPaging="true" PageSize="12" OnPageIndexChanging="dgvRedeterminacion_PageIndexChanging" runat="server">
                <Columns>
                    <asp:BoundField HeaderText="ID" DataField="ID" Visible="false" />
                    <asp:TemplateField HeaderText="Obra">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderObra" runat="server"
                                HeaderText="Obra" DataTextField="Nombre" DataValueField="Id" OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Autorizante.Obra.Descripcion") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Código Autorizante">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderAutorizante" runat="server"
                                HeaderText="Autorizante" DataTextField="Nombre" DataValueField="Id" OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("CodigoRedet") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Etapa">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderEstado" runat="server"
                                HeaderText="Estado" DataTextField="Nombre" DataValueField="Id" OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:DropDownList ID="ddlEtapas" runat="server" AutoPostBack="true"
                                OnSelectedIndexChanged="ddlEtapas_SelectedIndexChanged" class="btn btn-sm dropdown-toggle" Style="background-color: white !important; color: #34495e !important; font-weight: normal; padding: 8px 12px; font-size: 14px;">
                            </asp:DropDownList>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Expediente">
                        <ItemTemplate>
                            <asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("Expediente") %>' AutoPostBack="true"
                                OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField HeaderText="Tipo" DataField="Tipo" />
                    <asp:BoundField HeaderText="Salto" DataField="Salto" DataFormatString="{0:dd-MM-yyyy}" />
                    <asp:BoundField HeaderText="Porcentaje" DataField="Porcentaje" DataFormatString="{0:N2}%" />
                    <asp:BoundField HeaderText="Observaciones" DataField="Observaciones" />
                    <asp:BoundField HeaderText="Empresa" DataField="Empresa" />
                    <asp:BoundField HeaderText="Área" DataField="Area" />
                    <asp:BoundField HeaderText="Buzon SADE" DataField="BuzonSade" />
                    <asp:BoundField HeaderText="Fecha SADE" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" />

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
            document.getElementById('<%= txtSalto.ClientID %>').value = '';
            document.getElementById('<%= txtNro.ClientID %>').value = '';
            document.getElementById('<%= txtTipo.ClientID %>').value = '';
            document.getElementById('<%= txtPorcentaje.ClientID %>').value = '';
            document.getElementById('<%= txtObservacion.ClientID %>').value = '';
            document.getElementById('<%= ddlAutorizante.ClientID %>').selectedIndex = 0;
            document.getElementById('<%= ddlEtapa.ClientID %>').selectedIndex = 0;
        }
    </script>


</asp:Content>
