<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="LegitimosEF.aspx.cs" Inherits="WebForms.LegitimosEF" %>

<%@ Register Src="~/CustomControls/TreeViewSearch/TreeViewSearch.ascx" TagPrefix="CustomControls" TagName="TreeViewSearch" %>
<%@ Register Src="~/CustomControls/PaginationControl/PaginationControl.ascx" TagPrefix="CustomControls" TagName="PaginationControl" %>

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
                    <h1 class="modal-title fs-5" id="staticBackdropLabel">Agregar Legítimo</h1>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                            <div class="modal-body">
                                <div class="form-group">
                                    <div class="container">
                                        <div class="row">
                                            <div class="col-12">
                                                <div class="mb-3">
                                                    <label for="ddlObra" class="form-label">Obra</label>
                                                    <asp:DropDownList ID="ddlObra" CssClass="form-select" runat="server" AppendDataBoundItems="true">
                                                        <asp:ListItem Value="" Text="Seleccione una obra" Selected="True"></asp:ListItem>
                                                    </asp:DropDownList>
                                                    <asp:RequiredFieldValidator ID="rfvObra"
                                                        ControlToValidate="ddlObra"
                                                        ValidationGroup="AgregarLegitimo"
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
                                                    <label for="txtAutorizante" class="form-label">Código Autorizante</label>
                                                    <asp:TextBox ID="txtAutorizante" CssClass="form-control" runat="server" />
                                                    <asp:RequiredFieldValidator ID="rfvAutorizante"
                                                        ControlToValidate="txtAutorizante"
                                                        ValidationGroup="AgregarLegitimo"
                                                        runat="server"
                                                        ErrorMessage="El código autorizante es requerido"
                                                        Display="Dynamic"
                                                        CssClass="text-danger"
                                                        EnableClientScript="true" />
                                                </div>
                                            </div>
                                            <div class="col-6">
                                                <div class="mb-3">
                                                    <label for="txtExpediente" class="form-label">Expediente</label>
                                                    <asp:TextBox ID="txtExpediente" CssClass="form-control" runat="server" placeHolder="xxxxxxxx/25" />
                                                    <asp:RequiredFieldValidator ID="rfvExpediente"
                                                        ControlToValidate="txtExpediente"
                                                        ValidationGroup="AgregarLegitimo"
                                                        runat="server"
                                                        ErrorMessage="El expediente es requerido"
                                                        Display="Dynamic"
                                                        CssClass="text-danger"
                                                        EnableClientScript="true" />
                                                </div>
                                            </div>
                                            <div class="col-6">
                                                <div class="mb-3">
                                                    <label for="txtInicioEjecucion" class="form-label">Inicio Ejecución</label>
                                                    <asp:TextBox ID="txtInicioEjecucion" CssClass="form-control" runat="server" TextMode="Date" />
                                                    <asp:RequiredFieldValidator ID="rfvInicioEjecucion"
                                                        ControlToValidate="txtInicioEjecucion"
                                                        ValidationGroup="AgregarLegitimo"
                                                        runat="server"
                                                        ErrorMessage="La fecha de inicio de ejecución es requerida"
                                                        Display="Dynamic"
                                                        CssClass="text-danger"
                                                        EnableClientScript="true" />
                                                </div>
                                            </div>
                                            <div class="col-6">
                                                <div class="mb-3">
                                                    <label for="txtFinEjecucion" class="form-label">Fin Ejecución</label>
                                                    <asp:TextBox ID="txtFinEjecucion" CssClass="form-control" runat="server" TextMode="Date" />
                                                    <asp:RequiredFieldValidator ID="rfvFinEjecucion"
                                                        ControlToValidate="txtFinEjecucion"
                                                        ValidationGroup="AgregarLegitimo"
                                                        runat="server"
                                                        ErrorMessage="La fecha de fin de ejecución es requerida"
                                                        Display="Dynamic"
                                                        CssClass="text-danger"
                                                        EnableClientScript="true" />
                                                    <asp:CompareValidator ID="cvFinEjecucion"
                                                        ControlToValidate="txtFinEjecucion"
                                                        ControlToCompare="txtInicioEjecucion"
                                                        ValidationGroup="AgregarLegitimo"
                                                        runat="server"
                                                        Operator="GreaterThanEqual"
                                                        Type="Date"
                                                        ErrorMessage="La fecha de fin debe ser mayor o igual a la fecha de inicio"
                                                        Display="Dynamic"
                                                        CssClass="text-danger"
                                                        EnableClientScript="true" />
                                                </div>
                                            </div>
                                            <div class="col-6">
                                                <div class="mb-3">
                                                    <label for="txtCertificado" class="form-label">Monto Certificado</label>
                                                    <asp:TextBox ID="txtCertificado" CssClass="form-control" runat="server" placeHolder="0,00" />
                                                    <asp:RequiredFieldValidator ID="rfvCertificado"
                                                        ControlToValidate="txtCertificado"
                                                        ValidationGroup="AgregarLegitimo"
                                                        runat="server"
                                                        ErrorMessage="El monto es requerido"
                                                        Display="Dynamic"
                                                        CssClass="text-danger"
                                                        EnableClientScript="true" />
                                                    <asp:RegularExpressionValidator ID="revCertificado"
                                                        ControlToValidate="txtCertificado"
                                                        ValidationGroup="AgregarLegitimo"
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
                                                    <label for="txtMesAprobacion" class="form-label">Mes Aprobación</label>
                                                    <asp:TextBox ID="txtMesAprobacion" CssClass="form-control" runat="server" TextMode="Date" />
                                                    <asp:RequiredFieldValidator ID="rfvMesAprobacion"
                                                        ControlToValidate="txtMesAprobacion"
                                                        ValidationGroup="AgregarLegitimo"
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
                    <button type="button" class="btn btn-outline-secondary" onclick="limpiarFormulario()"><i class="bi bi-trash3-fill"></i></button>
                    <div class="d-flex gap-4">
                        <button type="button" class="btn btn-danger" data-bs-dismiss="modal"><i class="bi bi-x-lg"></i></button>
                        <asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-success" runat="server" ValidationGroup="AgregarLegitimo" />
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- /Modal -->

    <div class="row mt-4 mb-3">
        <div class="col-12">
            <div class="d-flex justify-content-end align-items-end flex-wrap gap-3">


                <!-- botones a la derecha -->
                <div class="d-flex gap-3">
                    <div class="form-group mb-2">
                        <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control" placeholder="Buscar..."></asp:TextBox>
                        <asp:TextBox ID="txtAutorizanteFilter" runat="server" Style="display:none" />
                    </div>
                    <div class="form-group mb-2">
                        <asp:LinkButton ID="btnFiltrar" runat="server" CssClass="btn btn-primary" OnClick="btnFiltrar_Click"
                            data-bs-toggle="tooltip" data-bs-placement="top" title="Buscar">
                            <i class="bi bi-search"></i>
                        </asp:LinkButton>
                    </div>

                    <div class="form-group mb-2">
                        <asp:LinkButton CssClass="btn btn-primary" ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClick="BtnClearFilters_Click"
                            data-bs-toggle="tooltip" data-bs-placement="top" title="Quita todos los filtros">
                            <i class="bi bi-funnel"></i>
                        </asp:LinkButton>
                    </div>

                    <div class="form-group mb-2">
                        <asp:LinkButton ID="btnExportarExcel" runat="server" CssClass="btn btn-success" OnClick="btnExportarExcel_Click" data-bs-toggle="tooltip" data-bs-placement="top" title="Exportar a Excel">
                            <i class="bi bi-download"></i>
                        </asp:LinkButton>
                    </div>

                    <asp:Panel ID="panelShowAddButton" runat="server">
                        <div class="form-group mb-2">
                            <asp:LinkButton ID="btnShowAddModal" runat="server" CssClass="btn btn-primary" OnClick="btnShowAddModal_Click" CausesValidation="false">
                                <i class="bi bi-plus-lg"></i> Agregar
                            </asp:LinkButton>
                        </div>
                    </asp:Panel>

                </div>
            </div>
        </div>
    </div>
    <hr class="mb-3" />

    <div class="gridview-scroll-container">

        <asp:GridView ID="gridviewRegistros" DataKeyNames="Id" CssClass="table1 table-bordered table-hover mb-4"
                OnSelectedIndexChanged="gridviewRegistros_SelectedIndexChanged"        
                OnRowDeleting="gridviewRegistros_RowDeleting"
                OnRowDataBound="gridviewRegistros_RowDataBound"
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
                                                        OnAcceptChanges="OnAcceptChanges"
                                                        />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("ObraEF.Area.Nombre") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField HeaderText="Obra" DataField="ObraEF.Descripcion" />

                    <asp:TemplateField>
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderEmpresa" runat="server"
                                HeaderText="Empresa"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("ObraEF.Empresa.Nombre") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField>
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderAutorizante" runat="server"
                                HeaderText="Código Autorizante"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("CodigoAutorizante") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Expediente">
                        <ItemTemplate>
                            <asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("Expediente") %>' AutoPostBack="true"
                                OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm w-auto text-center"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField HeaderText="Inicio Ejecución" DataField="InicioEjecucion" DataFormatString="{0:dd-MM-yyyy}" />
                    <asp:BoundField HeaderText="Fin Ejecución" DataField="FinEjecucion" DataFormatString="{0:dd-MM-yyyy}" />
                    <asp:BoundField HeaderText="Certificado" DataField="Certificado" DataFormatString="{0:C}" />

                    <asp:TemplateField>
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderMesAprobacion" runat="server" 
                                HeaderText="Mes Aprobación"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("MesAprobacion", "{0:dd-MM-yyyy}") %>
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

                    <asp:BoundField HeaderText="Sigaf" DataField="Sigaf" DataFormatString="{0:C}" />
                    <asp:BoundField HeaderText="Buzón SADE" DataField="BuzonSade" />
                    <asp:BoundField HeaderText="Fecha SADE" DataField="FechaSade" DataFormatString="{0:yyyy-MM-dd}" />

                    <asp:TemplateField>
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderLineaGestion" runat="server"
                                HeaderText="Linea de Gestión"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("ObraEF.Proyecto.LineaGestionEF.Nombre") %>
                        </ItemTemplate>
                    </asp:TemplateField>

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

    <!-- Control de paginación personalizado -->
    <CustomControls:PaginationControl ID="paginationControl" runat="server"
        OnPageChanged="paginationControl_PageChanged"
        OnPageSizeChanged="paginationControl_PageSizeChanged" />


    <script type="text/javascript">
        function limpiarFormulario() {
            // campos del modal (si existen)
            try {
                document.getElementById('<%= txtAutorizante.ClientID %>').value = '';
            } catch (e) {}
        }
    </script>
</asp:Content>
