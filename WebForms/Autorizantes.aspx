<%@ Page Title="" Language="C#" MasterPageFile="~/User.Master" AutoEventWireup="true" CodeBehind="Autorizantes.aspx.cs" Inherits="WebForms.Autorizantes" %>

<%@ Register Src="~/CustomControls/TreeViewSearch.ascx" TagPrefix="CustomControls" TagName="TreeViewSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
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
                                        <asp:RequiredFieldValidator ID="rfvExpediente"
                                            ControlToValidate="txtExpediente"
                                            ValidationGroup="AgregarAutorizante"
                                            runat="server"
                                            ErrorMessage="El expediente es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>
                                </div>

                                <div class="col-12">
                                    <div class="mb-3">
                                        <label for="txtDetalle" class="form-label">Detalle</label>
                                        <asp:TextBox ID="txtDetalle" CssClass="form-control" runat="server" />
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
                                        <asp:TextBox ID="txtMontoAutorizado" CssClass="form-control" runat="server" placeHolder="0,00" />
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
                                            ValidationExpression="^[0-9]+(\,[0-9]{1,2})?$"
                                            ErrorMessage="Solo números positivos con hasta 2 decimales"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>


                                    <div class="mb-3">
                                        <label for="txtFecha" class="form-label">Mes Base</label>
                                        <asp:TextBox ID="txtFecha" CssClass="form-control" runat="server" TextMode="Date" />
                                    </div>


                                </div>

                                <div class="col-6">
                                    <div class="mb-3">
                                        <label for="txtMes" class="form-label">Mes Aprobacion</label>
                                        <asp:TextBox ID="txtMes" CssClass="form-control" runat="server" TextMode="Date" />
                                        <asp:RequiredFieldValidator ID="rfvMes"
                                            ControlToValidate="txtMes"
                                            ValidationGroup="AgregarAutorizante"
                                            runat="server"
                                            ErrorMessage="El mes de aprobación es requerido"
                                            Display="Dynamic"
                                            CssClass="text-danger"
                                            EnableClientScript="true" />
                                    </div>

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
                            </div>
                        </div>

                    </div>


                </div>
                <div class="modal-footer d-flex justify-content-between px-4">

                    <button type="button" class="btn btn-secondary" onclick="limpiarFormulario()">Limpiar</button>
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
                        <asp:LinkButton ID="btnFiltrar" runat="server" CssClass="btn btn-primary" OnClick="btnFiltrar_Click"
                            data-bs-toggle="tooltip" data-bs-placement="top" title="Filtrar">
					<i class="bi bi-search"></i>
                        </asp:LinkButton>
                    </div>

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

        <asp:GridView ID="dgvAutorizante" DataKeyNames="CodigoAutorizante" CssClass="table1  table-bordered table-hover mb-4"
            OnSelectedIndexChanged="dgvAutorizante_SelectedIndexChanged"
            OnRowDeleting="dgvAutorizante_RowDeleting" OnRowDataBound="dgvAutorizante_RowDataBound"
            AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
            <Columns>
                <asp:BoundField HeaderText="Obra" DataField="Obra.Id" Visible="false" />

                <%--				<asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" />--%>
                <asp:TemplateField HeaderText="Obra">
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblsHeaderObra" runat="server"
                            HeaderText="Obra"
                            DataTextField="Descripcion"
                            DataValueField="Id"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("Obra.Descripcion") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField HeaderText="Contrata" DataField="Obra.Contrata.Nombre" />
                <%--				<asp:BoundField HeaderText="Empresa" DataField="Empresa" />--%>
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
                <%--				<asp:BoundField HeaderText="Concepto" DataField="Concepto.Nombre" />--%>
                <asp:TemplateField HeaderText="Concepto">
                    <HeaderTemplate>
                        <CustomControls:TreeViewSearch ID="cblsHeaderConcepto" runat="server"
                            HeaderText="Concepto"
                            DataTextField="Nombre"
                            DataValueField="Id"
                            OnAcceptChanges="OnAcceptChanges" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("Concepto.Nombre") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField HeaderText="Detalle" DataField="Detalle" />
                <asp:TemplateField HeaderText="Expediente">
                    <ItemTemplate>
                        <asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("Expediente") %>' AutoPostBack="true"
                            OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm" Style="background-color: white !important; color: #34495e !important; font-weight: normal; padding: 8px 12px; font-size: 14px;">
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
                            OnSelectedIndexChanged="ddlEstadoAutorizante_SelectedIndexChanged" class="btn btn-sm dropdown-toggle" Style="background-color: white !important; color: #34495e !important; font-weight: normal; padding: 8px 12px; font-size: 14px;">
                        </asp:DropDownList>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField HeaderText="Monto Autorizado" DataField="MontoAutorizado" DataFormatString="{0:C}" />
                <asp:BoundField HeaderText="Mes Aprobacion" DataField="Fecha" DataFormatString="{0:dd-MM-yyyy}" />
                <asp:BoundField HeaderText="Mes Base" DataField="MesBase" DataFormatString="{0:dd-MM-yyyy}" />

                <asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade" />
                <asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" />

               <%-- <asp:TemplateField HeaderText="Autorización GG">
                    <ItemTemplate>
                        <%# Eval("AutorizacionGG") != null && (bool)Eval("AutorizacionGG") ? "AUTORIZADO" : "A AUTORIZAR" %>
                    </ItemTemplate>
                </asp:TemplateField>--%>
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
                    </div>

        <div class="text-center p-4">
            <asp:Label ID="lblMensaje" Text="" runat="server" />
        </div>
    </div>


    <script type="text/javascript">
        function limpiarFormulario() {
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
