﻿<%@ Page Title="" Language="C#" MasterPageFile="~/User.Master" AutoEventWireup="true" CodeBehind="Legitimos.aspx.cs" Inherits="WebForms.Legitimos" %>

<%@ Register Src="~/CustomControls/CheckBoxListSearch.ascx" TagPrefix="CustomControls" TagName="CheckBoxListSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
   
    <%--COMENTADO POR CIERRE PLANIFICACION--%>
    <%--<div id="section1" style="display: none;">
		<div class="row mt-4">
			<div class="col-md-12">
				<table class="table  table1">
					<thead class="thead-dark" >
						<tr>
							<th>Obra</th>
							<th>Código Autorizante</th>
							<th>Expediente</th>
							<th>Inicio Ejecución</th>
							<th>Fin Ejecución</th>
							<th>Monto Certificado</th>
							<th>Mes Aprobación</th>
							<th></th>
							<th></th>
						</tr>
					</thead>
					<tbody>
						<tr>
							<td>
								<asp:DropDownList ID="ddlObra" CssClass="form-control" runat="server"></asp:DropDownList>
							</td>
							<td>
								<asp:TextBox ID="txtAutorizante" CssClass="form-control" runat="server" />
							</td>
							<td>
								<asp:TextBox ID="txtExpediente" CssClass="form-control" runat="server" />
							</td>
							<td>
								<asp:TextBox ID="txtInicioEjecucion" CssClass="form-control" runat="server" TextMode="Date" />
							</td>
							<td>
								<asp:TextBox ID="txtFinEjecucion" CssClass="form-control" runat="server" TextMode="Date" />
							</td>
							<td>
								<asp:TextBox ID="txtCertificado" CssClass="form-control" runat="server"  />
							</td>
							<td>
								<asp:TextBox ID="txtMesAprobacion" CssClass="form-control" runat="server" TextMode="Date" />
							</td>
							<td class="text-right">
								<asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-outline-success" runat="server" />
							</td>
							<td class="text-right">
								<asp:Button Text="Limpiar" ID="btnLimpiar" OnClick="btnLimpiar_Click"
									CssClass="btn btn-outline-secondary ml-2" runat="server" /></td>
						</tr>
					</tbody>
				</table>
			</div>
		</div>
	</div>--%>
    
    <div class="row mt-4">
        <div class="col-md-12">
            <div class="text-end">

                <div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">
                    <div class="form-group">
                        <label class="form-label lbl-left" style="margin-left: 10PX;" for="cblEmpresa">Empresa:</label>
                        <div class="dropdown">
                            <%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownEmpresa" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownEmpresa" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblEmpresa" runat="server" CssClass="dropdown-item form-check" />
							</ul>--%>
                            <CustomControls:CheckBoxListSearch ID="cblEmpresa" runat="server" />
                        </div>
                    </div>

                    <div class="form-group">
                        <label class="form-label lbl-left" style="margin-left: 10PX;" for="cblAutorizante">Autorizante:</label>
                        <div class="dropdown">
                            <%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownAutorizante" data-bs-toggle="dropdown" aria-expanded="false">
								Todos
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownAutorizante" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblAutorizante" runat="server" CssClass="dropdown-item form-check" />
							</ul>--%>
                            <CustomControls:CheckBoxListSearch ID="cblAutorizante" runat="server" />
                        </div>
                    </div>


                    <div class="form-group">
                        <label class="form-label lbl-left" style="margin-left: 10PX;" for="cblFecha">Mes aprobación:</label>
                        <div class="dropdown">
                            <CustomControls:CheckBoxListSearch ID="cblFecha" runat="server" />
                        </div>
                    </div>

                    <div class="form-group">
                        <label class="form-label lbl-left" for="cblEstadoExpediente">Estado:</label>
                        <CustomControls:CheckBoxListSearch ID="cblEstadoExpediente" runat="server" />
                    </div>

                    <div class="form-group text-left" style="flex: 1; max-width: 300px;">
                        <label class="form-label lbl-left" for="txtBuscar">Buscar:</label>
                        <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control form-control-uniform"></asp:TextBox>
                    </div>


                    <div class="form-group text-left" style="flex: 1; max-width: 300px;">
                        <label class="form-label lbl-left" for="txtSubtotal">Subtotal:</label>
                        <asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control form-control-uniform" ReadOnly="true" />
                    </div>

                    <div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">

                        <%--COMENTADO POR CIERRE PLANIFICACION
                        <div class="form-group d-flex align-items-end">
							<button class="btn btn-sm btn-secondary" id="visibilityMessage">
								<span id="visibilityText">Agregar Legitimo</span>
							</button>
						</div>--%>


                        <div class="form-group  d-flex align-items-end">
                            <%--<asp:Button CssClass="btn btn-sm btn-primary " ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClientClick="limpiarFiltros();" />--%>
                            <asp:Button CssClass="btn btn-sm btn-primary " ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClick="BtnClearFilters_Click" />
                        </div>
                        <div class="form-group d-flex align-items-end">
                            <asp:Button CssClass="btn btn-sm btn-primary" ID="btnFiltrar" Text="Filtrar" runat="server" OnClick="btnFiltrar_Click" />
                        </div>


                    </div>
                </div>
            </div>

            <hr />
            <asp:GridView ID="dgvLegitimos" DataKeyNames="ID" CssClass="table1  table-bordered table-hover  "
                OnRowDeleting="dgvLegitimos_RowDeleting" AutoGenerateColumns="false" runat="server" OnSelectedIndexChanged="dgvLegitimos_SelectedIndexChanged" Style="display: block; overflow-x: auto;">
                <Columns>
                    <asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" />
                    <asp:BoundField HeaderText="Empresa" DataField="Empresa" />
                    <asp:BoundField HeaderText="Código Autorizante" DataField="CodigoAutorizante" />
                    <asp:TemplateField HeaderText="Expediente">
                        <ItemTemplate>
                            <asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("Expediente") %>' AutoPostBack="true"
                                OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField HeaderText="Inicio Ejecución" DataField="InicioEjecucion" DataFormatString="{0:dd-MM-yyyy}" />
                    <asp:BoundField HeaderText="Fin Ejecución" DataField="FinEjecucion" DataFormatString="{0:dd-MM-yyyy}" />
                    <asp:BoundField HeaderText="Certificado" DataField="Certificado" DataFormatString="{0:C}" />
                    <asp:BoundField HeaderText="Mes Aprobación" DataField="MesAprobacion" DataFormatString="{0:dd-MM-yyyy}" />
                    <asp:BoundField HeaderText="Estado" DataField="Estado" />
                    <asp:BoundField HeaderText="Sigaf" DataField="Sigaf" DataFormatString="{0:C}" />
                    <asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade" />
                    <asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" />
                    
                    <%--COMENTADO POR CIERRE PLANIFICACION
                    <asp:TemplateField HeaderText="Acciones">
                        <ItemTemplate>
                            <div class="d-flex justify-content-center gap-2">
                                <asp:LinkButton ID="btnModificar" runat="server"
                                    CommandName="Select"
                                    CssClass="btn btn-sm btn-warning text-dark"
                                    ToolTip="Modificar">
                                    <i class="bi bi-pencil-square"></i>
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnEliminar" runat="server"
                                    CommandName="Delete"
                                    CssClass="btn btn-sm btn-danger text-light"
                                    ToolTip="Eliminar"
                                    OnClientClick="return confirm('¿Está seguro que desea eliminar este registro?');">
                                    <i class="bi bi-trash"></i>
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>               
                    </asp:TemplateField>--%>

                </Columns>
            </asp:GridView>
            <div class="text-center p-4">
                <asp:Label ID="lblMensaje" Text="" CssClass="text-success" runat="server" />
            </div>
        </div>
    </div>
    <script type="text/javascript">

        $(document).ready(function () {
            // Inicializamos la visibilidad según el valor de localStorage
            var sectionVisible = localStorage.getItem("sectionVisible");

            // Si está marcado como 'true', mostramos la sección
            if (sectionVisible === "true") {
                $('#section1').show(); // Mostramos la sección
                $('#visibilityText').text("Ocultar sección"); // Texto cuando la sección es visible
            } else {
                $('#section1').hide(); // Ocultamos la sección
                $('#visibilityText').text("Agregar Legitimo"); // Texto cuando la sección está oculta
            }

            // Manejar clic en el mensaje para alternar el estado de visibilidad
            $(document).on('click', '#visibilityMessage', function () {
                // Cambiamos el valor de visibilidad
                var currentStatus = $('#visibilityText').text();

                if (currentStatus === "Agregar Legitimo") {
                    // Si está oculto, lo mostramos
                    localStorage.setItem("sectionVisible", "true");
                    $('#section1').show(); // Mostramos la sección
                    $('#visibilityText').text("Ocultar sección"); // Cambiar el texto
                } else {
                    // Si está visible, lo ocultamos
                    localStorage.setItem("sectionVisible", "false");
                    $('#section1').hide(); // Ocultamos la sección
                    $('#visibilityText').text("Agregar Legitimo"); // Cambiar el texto
                }
            });
        });

    </script>
    
</asp:Content>
