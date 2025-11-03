<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ObrasEF.aspx.cs" Inherits="WebForms.ObrasEF" %>

<%@ Register Src="~/CustomControls/TreeViewSearch/TreeViewSearch.ascx" TagPrefix="CustomControls" TagName="TreeViewSearch" %>
<%@ Register Src="~/CustomControls/PaginationControl/PaginationControl.ascx" TagPrefix="CustomControls" TagName="PaginationControl" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Modal -->
    <div class="modal fade" id="modalAgregar" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h1 class="modal-title fs-5" id="staticBackdropLabel">Agregar Obra</h1>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <div class="container">
                            <div class="row">
                                <div class="col-12">
                                    <div class="mb-3">
                                        <label for="txtDescripcion" class="form-label">Nombre de Obra</label>
                                        <asp:TextBox ID="txtDescripcion" CssClass="form-control" runat="server" />
                                        <asp:RequiredFieldValidator ID="rfvDescripcion"
                                            ControlToValidate="txtDescripcion"
                                            ValidationGroup="AgregarObra"
                                            runat="server"
                                            ErrorMessage="Ingrese el nombre de la obra"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>

                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="ddlBarrio" class="form-label">Barrio</label>
                                        <asp:DropDownList ID="ddlBarrio" CssClass="form-select" runat="server" AppendDataBoundItems="true">
                                            <asp:ListItem Value="" Text="Seleccione un barrio" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvBarrio"
                                            ControlToValidate="ddlBarrio"
                                            ValidationGroup="AgregarObra"
                                            runat="server"
                                            ErrorMessage="Seleccione un barrio"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true"
                                            InitialValue="" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="ddlEmpresa" class="form-label">Empresa</label>
                                        <asp:DropDownList ID="ddlEmpresa" CssClass="form-select" runat="server" AppendDataBoundItems="true">
                                            <asp:ListItem Value="" Text="Seleccione una empresa" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvEmpresa"
                                            ControlToValidate="ddlEmpresa"
                                            ValidationGroup="AgregarObra"
                                            runat="server"
                                            ErrorMessage="Seleccione una empresa"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true"
                                            InitialValue="" />
                                    </div>
                                </div>
                                <div id="areaContainer" class="col-6">
                                    <div class="mb-3">
                                        <label for="ddlArea" class="form-label">Area</label>
                                        <asp:DropDownList ID="ddlArea" CssClass="form-select" runat="server" AppendDataBoundItems="true">
                                            <asp:ListItem Value="" Text="Seleccione un área" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvArea"
                                            ControlToValidate="ddlArea"
                                            ValidationGroup="AgregarObra"
                                            runat="server"
                                            ErrorMessage="Seleccione un área"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true"
                                            InitialValue="" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="ddlContrata" class="form-label">Contrata</label>
                                        <asp:DropDownList ID="ddlContrata" CssClass="form-select" runat="server" AppendDataBoundItems="true">
                                            <asp:ListItem Value="" Text="Seleccione una contrata" Selected="True"></asp:ListItem>
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvContrata"
                                            ControlToValidate="ddlContrata"
                                            ValidationGroup="AgregarObra"
                                            runat="server"
                                            ErrorMessage="Seleccione una contrata"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true"
                                            InitialValue="" />
                                    </div>
                                </div>

                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtNumero" class="form-label">Número</label>
                                        <asp:TextBox ID="txtNumero" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
                                        <asp:RequiredFieldValidator ID="rfvNumero"
                                            ControlToValidate="txtNumero"
                                            ValidationGroup="AgregarObra"
                                            runat="server"
                                            ErrorMessage="Ingrese un número"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtAnio" class="form-label">Año</label>
                                        <asp:TextBox ID="txtAnio" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
                                        <asp:RequiredFieldValidator ID="rfvAnio"
                                            ControlToValidate="txtAnio"
                                            ValidationGroup="AgregarObra"
                                            runat="server"
                                            ErrorMessage="Ingrese un año"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtEtapa" class="form-label">Etapa</label>
                                        <asp:TextBox ID="txtEtapa" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
                                        <asp:RequiredFieldValidator ID="rfvEtapa"
                                            ControlToValidate="txtEtapa"
                                            ValidationGroup="AgregarObra"
                                            runat="server"
                                            ErrorMessage="Ingrese una etapa"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>
                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtObra" class="form-label">Obra N°</label>
                                        <asp:TextBox ID="txtObraNumero" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
                                        <asp:RequiredFieldValidator ID="rfvObraNumero"
                                            ControlToValidate="txtObraNumero"
                                            ValidationGroup="AgregarObra"
                                            runat="server"
                                            ErrorMessage="Ingrese el número de obra"
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
                        <asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" ValidationGroup="AgregarObra" />
                    <button type="button" class="btn btn-outline-secondary" onclick="limpiarFormulario()" data-bs-toggle="tooltip" title="Limpiar formulario"><i class="bi bi-trash3-fill"></i></button>
                    <div class="d-flex gap-2">
                        <button type="button" class="btn btn-danger" data-bs-dismiss="modal" data-bs-toggle="tooltip" title="Cancelar"><i class="bi bi-x-lg"></i></button>
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
                    <div class="form-group mb-2">
                        <asp:LinkButton ID="btnExportarExcel" runat="server" CssClass="btn btn-success" OnClick="btnExportarExcel_Click"
                            data-bs-toggle="tooltip" data-bs-placement="top" title="Exportar a Excel">
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


            <asp:GridView ID="dgvObra" DataKeyNames="Id" CssClass="table1  table-bordered table-hover mb-4"
                OnSelectedIndexChanged="dgvObra_SelectedIndexChanged"
                OnRowDeleting="dgvObra_RowDeleting"
                OnRowDataBound="dgvObra_RowDataBound"
                ShowHeaderWhenEmpty="true"
                AutoGenerateColumns="false"
                AllowPaging="false"
                AllowCustomPaging="false"
                runat="server">

                <Columns>
                    <asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />
                    <asp:TemplateField HeaderText="Área">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderArea" runat="server"
                                HeaderText="Área"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Area") %>
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

                    <asp:TemplateField HeaderText="Contrata">
                        <ItemTemplate>
                            <%# Eval("Contrata") + " " + Eval("Numero") + "/" + Eval("Anio") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Barrio">
                        <HeaderTemplate>
                            <CustomControls:TreeViewSearch ID="cblsHeaderBarrio" runat="server"
                                HeaderText="Barrio"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <%# Eval("Barrio") %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:BoundField HeaderText="Nombre de Obra" DataField="Descripcion" />
                    <asp:BoundField HeaderText="Linea de Gestion" DataField="LineaGestionNombre" />
                    <asp:BoundField HeaderText="Proyecto" DataField="ProyectoNombre" />

                    <asp:TemplateField HeaderText="Disponible Actual">
                        <HeaderTemplate>
                            <asp:PlaceHolder runat="server">Disponible Actual</asp:PlaceHolder>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:PlaceHolder runat="server">
                                <%# Eval("AutorizadoNuevo", "{0:C}") %>
                            </asp:PlaceHolder>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Planificacion 2025">
                        <HeaderTemplate>
                            <asp:PlaceHolder runat="server">Planificacion 2025</asp:PlaceHolder>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:PlaceHolder runat="server">
                                <%# Eval("MontoCertificado", "{0:C}") %>
                            </asp:PlaceHolder>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Ejecucion Presupuesto 2025">
                        <HeaderTemplate>
                            <asp:PlaceHolder runat="server">Ejecucion Presupuesto 2025</asp:PlaceHolder>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:PlaceHolder runat="server">
                                <%# Eval("Porcentaje", "{0:N2}%") %>
                            </asp:PlaceHolder>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Monto de Obra inicial">
                        <HeaderTemplate>
                            <asp:PlaceHolder runat="server">Monto de Obra inicial</asp:PlaceHolder>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:PlaceHolder runat="server">
                                <%# Eval("MontoInicial", "{0:C}") %>
                            </asp:PlaceHolder>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Monto de Obra actual">
                        <HeaderTemplate>
                            <asp:PlaceHolder runat="server">Monto de Obra actual</asp:PlaceHolder>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:PlaceHolder runat="server">
                                <%# Eval("MontoActual", "{0:C}") %>
                            </asp:PlaceHolder>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Faltante de Obra">
                        <HeaderTemplate>
                            <asp:PlaceHolder runat="server">Faltante de Obra</asp:PlaceHolder>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:PlaceHolder runat="server">
                                <%# Eval("MontoFaltante", "{0:C}") %>
                            </asp:PlaceHolder>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField HeaderText="Fecha Inicio" DataField="FechaInicio" DataFormatString="{0:dd-MM-yyyy}" />
                    <asp:BoundField HeaderText="Fecha Fin" DataField="FechaFin" DataFormatString="{0:dd-MM-yyyy}" />

                    <asp:TemplateField HeaderText="Acciones">
                        <HeaderTemplate>
                            <asp:PlaceHolder runat="server">Acciones</asp:PlaceHolder>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:PlaceHolder runat="server">
                                <div class="d-flex justify-content-center gap-2">
                                    <asp:LinkButton ID="btnModificar" runat="server"
                                        CommandName="Select"
                                        CausesValidation="false"
                                        CssClass="btn btn-sm btn-warning "
                                        ToolTip="Modificar">
                                        <i class="bi bi-pencil-square"></i>
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnEliminar" runat="server"
                                        CommandName="Delete"
                                        CausesValidation="false"
                                        CssClass="btn btn-sm btn-danger "
                                        ToolTip="Eliminar"
                                        OnClientClick="return confirm('¿Está seguro que desea eliminar este registro?');">
                                        <i class="bi bi-trash"></i>
                                    </asp:LinkButton>
                                </div>
                            </asp:PlaceHolder>
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
        function soloNumeros(e) {
            var tecla = (document) ? e.keyCode : e.which;
            if (tecla == 8 || tecla == 46) {
                return true;
            }
            var patron = /^[0-9]$/;
            var te = String.fromCharCode(tecla);
            return patron.test(te);
        }

        function limpiarFormulario() {
            document.getElementById('<%= txtNumero.ClientID %>').value = '';
            document.getElementById('<%= txtAnio.ClientID %>').value = '';
            document.getElementById('<%= txtEtapa.ClientID %>').value = '';
            document.getElementById('<%= txtObraNumero.ClientID %>').value = '';
            document.getElementById('<%= txtDescripcion.ClientID %>').value = '';
            document.getElementById('<%= ddlArea.ClientID %>').selectedIndex = 0;
            document.getElementById('<%= ddlEmpresa.ClientID %>').selectedIndex = 0;
            document.getElementById('<%= ddlContrata.ClientID %>').selectedIndex = 0;
            document.getElementById('<%= ddlBarrio.ClientID %>').selectedIndex = 0;
        }

    </script>
</asp:Content>
