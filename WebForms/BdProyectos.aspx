<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="BdProyectos.aspx.cs" Inherits="WebForms.BdProyectos" %>

<%@ Register Src="~/CustomControls/TreeViewSearch.ascx" TagPrefix="CustomControls" TagName="TreeViewSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">


    <!-- Modal -->
    <div class="modal fade" id="modalAgregar" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h1 class="modal-title fs-5" id="staticBackdropLabel">Agregar Proyecto</h1>
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
                                    </div>
                                </div>
                                <div class="col-6">

                                    <div class="mb-3">
                                        <label for="txtProyecto" class="form-label">Proyecto</label>
                                        <asp:TextBox ID="txtProyecto" CssClass="form-control" runat="server" />
                                    </div>

                                    <div class="mb-3">
                                        <label for="ddlLineaGestion" class="form-label">Linea de Gestión</label>
                                        <asp:DropDownList ID="ddlLineaGestion" CssClass="form-select" runat="server" />
                                    </div>


                                </div>

                                <div class="col-6">

                                    <div class="mb-3">
                                        <label for="txtSubProyecto" class="form-label">SubProyecto</label>
                                        <asp:TextBox ID="txtSubProyecto" CssClass="form-control" runat="server" />
                                    </div>

                                    <div class="mb-3">
                                        <label for="txtMontoAutorizadoInicial" class="form-label">Monto Autorizado</label>
                                        <asp:TextBox ID="txtMontoAutorizadoInicial" CssClass="form-control" runat="server" placeHolder="0,00" />
                                        <asp:RequiredFieldValidator ID="rfvMontoAutorizado"
                                            ControlToValidate="txtMontoAutorizadoInicial"
                                            ValidationGroup="AgregarProyecto"
                                            runat="server"
                                            ErrorMessage="El monto es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                        <asp:RegularExpressionValidator ID="revMontoAutorizado"
                                            ControlToValidate="txtMontoAutorizadoInicial"
                                            ValidationGroup="AgregarProyecto"
                                            runat="server"
                                            ValidationExpression="^[0-9]+(\,[0-9]{1,2})?$"
                                            ErrorMessage="Solo números positivos con hasta 2 decimales"
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
                        <asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" ValidationGroup="AgregarProyecto" OnClientClick="if(!Page_ClientValidate('AgregarProyecto')) return false;" UseSubmitBehavior="false" />
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

        <asp:GridView ID="dgvBdProyecto" DataKeyNames="Id" CssClass="table1 table-bordered table-hover mb-4"
            OnSelectedIndexChanged="dgvBdProyecto_SelectedIndexChanged"
            OnRowDeleting="dgvBdProyecto_RowDeleting"
            OnRowDataBound="dgvBdProyecto_RowDataBound"
				AutoGenerateColumns="false" AllowPaging="true" PageSize="12" OnPageIndexChanging="dgvBdProyecto_PageIndexChanging" runat="server">
            <Columns>
                <%--<asp:BoundField HeaderText="Area " DataField="Obra.Area.Nombre" />--%>
                <asp:TemplateField HeaderText="Área">
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblHeaderArea" runat="server"
                                HeaderText="Área"
                                DataTextField="Nombre"
                                DataValueField="Id"
                                OnAcceptChanges="OnAcceptChanges"/>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("Obra.Area.Nombre") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField HeaderText="Contrata " DataField="Obra.Contrata.Nombre" />
                <asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" />
                <%--                <asp:BoundField HeaderText="Proyecto" DataField="Proyecto" />--%>
                <asp:TemplateField HeaderText="Proyecto">
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblHeaderProyecto" runat="server" 
                            HeaderText="Proyecto"
                            DataTextField="Nombre" 
                            DataValueField="Nombre" 
                            OnAcceptChanges="OnAcceptChanges"/>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("Proyecto") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField HeaderText="SubProyecto" DataField="SubProyecto" />
                <%--                <asp:BoundField HeaderText="Linea de Gestión" DataField="LineaGestion.Nombre" />--%>
                <asp:TemplateField HeaderText="Línea de Gestión">
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblHeaderLineaGestion" runat="server"
                            HeaderText="Línea de Gestión"
                            DataTextField="Nombre" 
                            DataValueField="Id" 
                            OnAcceptChanges="OnAcceptChanges"/>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("LineaGestion.Nombre") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField HeaderText="Monto Inicial" DataField="AutorizadoInicial" DataFormatString="{0:C}" />
                <asp:BoundField HeaderText="Monto Nuevo" DataField="AutorizadoNuevo" DataFormatString="{0:C}" />
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
                                CssClass="btn btn-sm btn-danger "
                                ToolTip="Eliminar"
                                OnClientClick="return confirm('¿Está seguro que desea eliminar este registro?');">
                                    <i class="bi bi-trash"></i>
                            </asp:LinkButton>
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
                    </div>

        <div class="text-center p-4">
            <asp:Label ID="lblMensaje" Text="" runat="server" />
        </div>
    </div>

    <script type="text/javascript">

        const myModal = document.getElementById('myModal')
        const myInput = document.getElementById('myInput')

        myModal.addEventListener('shown.bs.modal', () => {
            myInput.focus()
        })



        $(document).ready(function () {
            var modalAgregar = new bootstrap.Modal(document.getElementById('modalAgregar'));

            // Prevenir cierre del modal si hay errores de validación
            $('#modalAgregar').on('hide.bs.modal', function (e) {
                if (!Page_IsValid) {
                    e.preventDefault();
                }
            });

            // Validación en tiempo real
            $('#<%= txtMontoAutorizadoInicial.ClientID %>').on('change keyup', function () {
                if (typeof Page_ClientValidate === 'function') {
                    ValidatorValidate(document.getElementById('<%= rfvMontoAutorizado.ClientID %>'));
                    ValidatorValidate(document.getElementById('<%= revMontoAutorizado.ClientID %>'));
                }
            });

            // Si hay errores después de un postback, mostrar el modal
            if (!Page_IsValid) {
                modalAgregar.show();
            }
        });

        function limpiarFormulario() {
            document.getElementById('<%= txtProyecto.ClientID %>').value = '';
            document.getElementById('<%= txtSubProyecto.ClientID %>').value = '';
            document.getElementById('<%= txtMontoAutorizadoInicial.ClientID %>').value = '';
            document.getElementById('<%= ddlObra.ClientID %>').selectedIndex = 0;
            document.getElementById('<%= ddlLineaGestion.ClientID %>').selectedIndex = 0;
        }

    </script>
</asp:Content>
