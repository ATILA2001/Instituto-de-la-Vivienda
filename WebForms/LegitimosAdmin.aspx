<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="LegitimosAdmin.aspx.cs" Inherits="WebForms.LegitimosAdmin" %>

<%@ Register Src="~/CustomControls/CheckBoxListSearch.ascx" TagPrefix="CustomControls" TagName="CheckBoxListSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- Modal -->
    <div class="modal fade" id="modalAgregar" data-bs-backdrop="static" data-bs-keyboard="false" tabindex="-1" aria-labelledby="staticBackdropLabel" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content">
                <div class="modal-header">
                    <h1 class="modal-title fs-5" id="staticBackdropLabel">Agregar Legitimo</h1>
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
                    <button type="button" class="btn btn-secondary" onclick="limpiarFormulario()">Limpiar</button>
                    <div class="d-flex gap-4">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                        <asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" ValidationGroup="AgregarLegitimo" />
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
                        <asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control" ReadOnly="true" />
                    </div>
                </div>

                <!-- Contenedor de Botones alineados a la derecha -->
                <div class="d-flex gap-3">
                    <div class="form-group mb-2">
                        <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control" placeholder="Buscar..."></asp:TextBox>
                    </div>
                    <div class="form-group mb-2">
                        <asp:LinkButton ID="btnFiltrar" runat="server" CssClass="btn btn-primary" OnClick="btnFiltrar_Click"
                            data-bs-toggle="tooltip" data-bs-placement="top" title="Filtrar">
							<i class="bi bi-search"></i>
                        </asp:LinkButton>
                    </div>

                    <div class="form-group mb-2">
                        <asp:LinkButton CssClass="btn btn-primary" ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClick="BtnClearFilters_Click">
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

        <asp:GridView ID="dgvLegitimos" DataKeyNames="ID" CssClass="table1 table-bordered table-hover mb-4"
            OnRowDeleting="dgvLegitimos_RowDeleting" OnSelectedIndexChanged="dgvLegitimos_SelectedIndexChanged"
            OnRowDataBound="dgvLegitimos_RowDataBound"
            AutoGenerateColumns="false" runat="server">
            <Columns>
                <%--<asp:BoundField HeaderText="Area" DataField="Obra.Area.Nombre" />--%>
                <asp:TemplateField HeaderText="Area">
                    <HeaderTemplate>
                        <CustomControls:CheckBoxListSearch ID="cblsHeaderArea" runat="server"
                            HeaderText="Área"
                            DataTextField="Nombre"
                            DataValueField="Nombre"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("Obra.Area.Nombre") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" />
                <%--<asp:BoundField HeaderText="Empresa" DataField="Empresa" />--%>
                <asp:TemplateField HeaderText="Empresa">
                    <HeaderTemplate>
                        <CustomControls:CheckBoxListSearch ID="cblsHeaderEmpresa" runat="server"
                            HeaderText="Empresa"
                            DataTextField="Nombre"
                            DataValueField="Nombre" 
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("Empresa") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <%--<asp:BoundField HeaderText="Código Autorizante" DataField="CodigoAutorizante" />--%>
                <asp:TemplateField HeaderText="Código Autorizante">
                    <HeaderTemplate>
                        <CustomControls:CheckBoxListSearch ID="cblsHeaderCodigoAutorizante" runat="server"
                            HeaderText="Código Autorizante"
                            DataTextField="Nombre"
                            DataValueField="Nombre" 
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("CodigoAutorizante") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Expediente">
                    <ItemTemplate>
                        <asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("Expediente") %>' AutoPostBack="true"
                            OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm"></asp:TextBox>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField HeaderText="Inicio Ejecución" DataField="InicioEjecucion" DataFormatString="{0:dd-MM-yyyy}" />
                <asp:BoundField HeaderText="Fin Ejecución" DataField="FinEjecucion" DataFormatString="{0:dd-MM-yyyy}" />
                <asp:BoundField HeaderText="Certificado" DataField="Certificado" DataFormatString="{0:C}" />
                <%--				<asp:BoundField HeaderText="Mes Aprobación" DataField="MesAprobacion" DataFormatString="{0:dd-MM-yyyy}" />--%>
                <asp:TemplateField HeaderText="Mes Aprobación">
                    <HeaderTemplate>
                        <CustomControls:CheckBoxListSearch ID="cblsHeaderMesAprobacion" runat="server"
                            HeaderText="Mes Aprobación"
                            DataTextField="Nombre" 
                            DataValueField="Valor" 
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("MesAprobacion", "{0:dd-MM-yyyy}") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <%--				<asp:BoundField HeaderText="Estado" DataField="Estado" />--%>
                <asp:TemplateField HeaderText="Estado">
                    <HeaderTemplate>
                        <CustomControls:CheckBoxListSearch ID="cblsHeaderEstado" runat="server"
                            HeaderText="Estado"
                            DataTextField="Nombre"
                            DataValueField="Nombre"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("Estado") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField HeaderText="Sigaf" DataField="Sigaf" DataFormatString="{0:C}" />
                <asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade" />
                <asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" />
                <%--				<asp:BoundField HeaderText="Linea de gestion" DataField="Linea" />--%>
                <asp:TemplateField HeaderText="Linea de gestion">
                    <HeaderTemplate>
                        <CustomControls:CheckBoxListSearch ID="cblsHeaderLinea" runat="server"
                            HeaderText="Linea de gestion"
                            DataTextField="Nombre"
                            DataValueField="Nombre"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("Linea") %>
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
        </asp:GridView>

        <div class="text-center p-4">
            <asp:Label ID="lblMensaje" Text="" runat="server" />
        </div>
    </div>

    <script type="text/javascript">
        function limpiarFormulario() {
            document.getElementById('<%= txtAutorizante.ClientID %>').value = '';
            document.getElementById('<%= txtExpediente.ClientID %>').value = '';
            document.getElementById('<%= txtInicioEjecucion.ClientID %>').value = '';
            document.getElementById('<%= txtFinEjecucion.ClientID %>').value = '';
            document.getElementById('<%= txtCertificado.ClientID %>').value = '';
            document.getElementById('<%= txtMesAprobacion.ClientID %>').value = '';
            document.getElementById('<%= ddlObra.ClientID %>').selectedIndex = 0;
        }
    </script>
</asp:Content>
