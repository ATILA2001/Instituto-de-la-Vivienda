<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="CertificadosAdmin.aspx.cs" Inherits="WebForms.CertificadosAdmin" %>

<%@ Register Src="~/CustomControls/CheckBoxListSearch.ascx" TagPrefix="CustomControls" TagName="CheckBoxListSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

	<div id="section1" style="display: none;">
		<div class="row mt-4">
			<div class="col-md-12">
				<table class="table  table-3d">
					<thead class="thead-dark">
						<tr>

							<th>Código Autorizante</th>
							<th>Expediente</th>
							<th>Tipo</th>
							<th>Monto Autorizado</th>
							<th>Mes Aprobacion</th>
							<th></th>
							<th></th>
						</tr>
					</thead>
					<tbody>
						<tr>


                            <td>
                                <asp:DropDownList ID="ddlAutorizante" CssClass="form-control" runat="server"></asp:DropDownList>
                            </td>

                            <td>
                                <asp:TextBox ID="txtExpediente" CssClass="form-control" runat="server" />
                            </td>

                            <td>
                                <asp:DropDownList ID="ddlTipo" CssClass="form-control" runat="server"></asp:DropDownList>
                            </td>

                            <td>
                                <asp:TextBox ID="txtMontoAutorizado" CssClass="form-control" runat="server" />
                            </td>
                            <td>
                                <asp:TextBox ID="txtFecha" CssClass="form-control" runat="server" TextMode="Date" />
                            </td>
                            <td class="text-right">
                                <asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" CssClass="btn btn-primary" runat="server" />
                            </td>
                            <td class="text-right">
                                <asp:Button Text="Limpiar" ID="btnLimpiar" OnClick="btnLimpiar_Click"
                                    CssClass="btn btn-primary" runat="server" /></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <div class="row mt-4">
        <div class="col-md-12">
            <div class="text-end">

                <div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">
                    <div class="form-group ">
                        <label class="form-label lbl-left" style="margin-left: 10PX;" for="cblArea">Area:</label>
                        <div class="dropdown">
                            <%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownArea" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
               
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownArea" style="max-height: 200px; overflow-y: auto;">
								<!-- Rendimos la CheckBoxList aquí -->
								<asp:CheckBoxList ID="cblArea" runat="server" CssClass="dropdown-item form-check" />
							</ul>--%>
							<CustomControls:CheckBoxListSearch ID="cblArea" runat="server" />
						</div>
					</div>

					<div class="form-group ">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblBarrio">Barrio:</label>
						<div class="dropdown">
							<%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownArea" data-bs-toggle="dropdown" aria-expanded="false">
						Todas
               
					</button>
					<ul class="dropdown-menu p-2" aria-labelledby="dropdownArea" style="max-height: 200px; overflow-y: auto;">
						<!-- Rendimos la CheckBoxList aquí -->
						<asp:CheckBoxList ID="cblArea" runat="server" CssClass="dropdown-item form-check" />
					</ul>--%>
							<CustomControls:CheckBoxListSearch ID="cblBarrio" runat="server" />
						</div>
					</div>


					<div class="form-group ">
						<label class="form-label lbl-left" style="margin-left: 10PX;" for="cblProyecto">Proyecto:</label>
						<div class="dropdown">
							<%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownArea" data-bs-toggle="dropdown" aria-expanded="false">
						Todas
               
					</button>
					<ul class="dropdown-menu p-2" aria-labelledby="dropdownArea" style="max-height: 200px; overflow-y: auto;">
						<!-- Rendimos la CheckBoxList aquí -->
						<asp:CheckBoxList ID="cblArea" runat="server" CssClass="dropdown-item form-check" />
					</ul>--%>
							<CustomControls:CheckBoxListSearch ID="cblProyecto" runat="server" />
						</div>
					</div>

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
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownAutorizante" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblAutorizante" runat="server" CssClass="dropdown-item form-check" />
							</ul>--%>
							<CustomControls:CheckBoxListSearch ID="cblAutorizante" runat="server" />
						</div>
					</div>

                    <div class="form-group">
                        <label class="form-label lbl-left" style="margin-left: 10PX;" for="cblTipo">Tipo:</label>
                        <div class="dropdown">
                            <%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownTipo" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownTipo" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblTipo" runat="server" CssClass="dropdown-item form-check" />
							</ul>--%>
							<CustomControls:CheckBoxListSearch ID="cblTipo" runat="server" />
						</div>
					</div>

                    <div class="form-group">
                        <label class="form-label lbl-left" style="margin-left: 10PX;" for="cblFecha">Mes Certificado:</label>
                        <div class="dropdown">
                            <%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownFecha" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownFecha" style="max-height: 200px; overflow-y: auto;">
								<asp:CheckBoxList ID="cblFecha" runat="server" CssClass="dropdown-item form-check" />
							</ul>--%>
							<CustomControls:CheckBoxListSearch ID="cblFecha" runat="server" />
						</div>
					</div>
					<div class="form-group">
						<label class="form-label lbl-left" for="cblEstadoExpediente">Estado:</label>
						<%--<asp:DropDownList ID="ddlExpediente" runat="server" CssClass="form-control form-control-sm">
							<asp:ListItem Text="Todos" Value="" />
							<asp:ListItem Text="Vacío" Value="vacio" />
							<asp:ListItem Text="No Vacío" Value="novacio" />
						</asp:DropDownList>--%>
						<CustomControls:CheckBoxListSearch ID="cblEstadoExpediente" runat="server" />
					</div>
                    <div class="form-group ">
                        <label class="form-label lbl-left" style="margin-left: 10PX;" for="cblLinea">Linea:</label>
                        <div class="dropdown">
                            <CustomControls:CheckBoxListSearch ID="cblLinea" runat="server" />
                        </div>
                    </div>

                    <div class="form-group text-left" style="flex: 1; max-width: 300px;">
                        <label class="form-label lbl-left" for="txtBuscar">Buscar:</label>
                        <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control form-control-uniform"></asp:TextBox>
                    </div>


                    <div class="form-group text-left" style="flex: 3; max-width: 300px;">
                        <label class="form-label lbl-left" for="txtSubtotal">Subtotal:</label>
                        <asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control form-control-uniform" ReadOnly="true" />
                    </div>




                    <div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">


                        <div class="form-group d-flex align-items-end">
                            <button class="btn btn-sm btn-primary" id="visibilityMessage">
                                <strong id="visibilityText">Agregar Certificado</strong>
                            </button>
                        </div>

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
			<asp:GridView ID="dgvCertificado" DataKeyNames="ID" CssClass="table1  table-bordered table-hover "
				OnSelectedIndexChanged="dgvCertificado_SelectedIndexChanged"
				OnRowDeleting="dgvCertificado_RowDeleting"
				AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto; overflow-y: auto; max-height: 80vh;">
				<Columns>
					<asp:BoundField HeaderText="ID" DataField="Autorizante.Id" Visible="false" />

					<asp:BoundField HeaderText="Area" DataField="Autorizante.Obra.Area.Nombre" />
					<asp:BoundField HeaderText="Contrata" DataField="Autorizante.Obra.Contrata.Nombre" />
					<asp:BoundField HeaderText="Obra" DataField="Autorizante.Obra.Descripcion" />
					<asp:BoundField HeaderText="Barrio" DataField="Autorizante.Obra.Barrio.Nombre" />
					<asp:BoundField HeaderText="Proyecto" DataField="Autorizante.Obra.Proyecto" />
					<asp:BoundField HeaderText="Empresa" DataField="Empresa" />
					<asp:BoundField HeaderText="Código Autorizante" DataField="Autorizante.CodigoAutorizante" />
					<asp:TemplateField HeaderText="Expediente">
						<ItemTemplate>
							<asp:TextBox ID="txtExpediente" runat="server" Text='<%# Bind("ExpedientePago") %>' AutoPostBack="true"
								OnTextChanged="txtExpediente_TextChanged" CssClass="form-control form-control-sm"></asp:TextBox>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField HeaderText="Estado" DataField="Estado" />

                    <asp:BoundField HeaderText="Tipo" DataField="Tipo.Nombre" />
                    <asp:BoundField HeaderText="Monto Certificado" DataField="MontoTotal" DataFormatString="{0:C}" />
                    <asp:BoundField HeaderText="Mes Certificado" DataField="MesAprobacion" DataFormatString="{0:dd-MM-yyyy}" />
                    <asp:BoundField HeaderText="Porcentaje" DataField="Porcentaje" DataFormatString="{0:N2}%" />
                    <asp:BoundField HeaderText="Sigaf" DataField="Sigaf" DataFormatString="{0:C}" />
                    <asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade" />
                    <asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" />

                    <asp:TemplateField HeaderText="Acciones">
                        <ItemTemplate>
                            <div class="d-flex justify-content-center gap-2">
                                <asp:LinkButton ID="btnModificar" runat="server"
                                    CommandName="Select"
                                    CssClass="btn btn-sm btn-outline-warning"
                                    ToolTip="Modificar">
                                    <i class="bi bi-pencil-square"></i>
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnEliminar" runat="server"
                                    CommandName="Delete"
                                    CssClass="btn btn-sm btn-outline-danger"
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
                $('#visibilityText').text("Agregar Certificado"); // Texto cuando la sección está oculta
            }

            // Manejar clic en el mensaje para alternar el estado de visibilidad
            $(document).on('click', '#visibilityMessage', function () {
                // Cambiamos el valor de visibilidad
                var currentStatus = $('#visibilityText').text();

                if (currentStatus === "Agregar Certificado") {
                    // Si está oculto, lo mostramos
                    localStorage.setItem("sectionVisible", "true");
                    $('#section1').show(); // Mostramos la sección
                    $('#visibilityText').text("Ocultar sección"); // Cambiar el texto
                } else {
                    // Si está visible, lo ocultamos
                    localStorage.setItem("sectionVisible", "false");
                    $('#section1').hide(); // Ocultamos la sección
                    $('#visibilityText').text("Agregar Certificado"); // Cambiar el texto
                }
            });
        });

    </script>
  
</asp:Content>
