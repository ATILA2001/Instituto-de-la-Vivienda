<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="LegitimosAdmin.aspx.cs" Inherits="WebForms.LegitimosAdmin" %>

<%@ Register Src="~/CustomControls/CheckBoxListSearch.ascx" TagPrefix="CustomControls" TagName="CheckBoxListSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <style>
        .table, .table-3d, .table-3d thead, .table-3d tbody, .table-3d tr, .table-3d th, .table-3d td {
            color: #ecf0f1 !important;
        }

        .table-3d {
            border-radius: 10px;
            overflow: hidden;
            border-collapse: collapse;
        }

            .table-3d thead th {
                background-color: #153244;
                color: #ecf0f1;
                font-weight: bold;
                text-align: center;
                text-transform: uppercase;
                border: none;
            }
    </style>
    <div id="section1" style="display: none;">
        <div class="row mt-4">
            <div class="col-md-12">
                <table class="table  table-3d">
                    <thead class="thead-dark">
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
                                <asp:TextBox ID="txtCertificado" CssClass="form-control" runat="server" />
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
    </div>

    <div class="row mt-4">
        <div class="col-md-12">
            <div class="text-end">

                <div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">


                    <div class="form-group ">
                        <label class="form-label lbl-left" style="margin-left: 10PX;" for="cblArea">Area:</label>
                        <div class="dropdown">
                            <CustomControls:CheckBoxListSearch ID="cblArea" runat="server" />
                        </div>
                    </div>

                    <div class="form-group">
                        <label class="form-label lbl-left" style="margin-left: 10PX;" for="cblEmpresa">Empresa:</label>
                        <div class="dropdown">
                            <CustomControls:CheckBoxListSearch ID="cblEmpresa" runat="server" />
                        </div>
                    </div>

                    <div class="form-group">
                        <label class="form-label lbl-left" style="margin-left: 10PX;" for="cblAutorizante">Autorizante:</label>
                        <div class="dropdown">
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


                    <div class="form-group text-left" style="flex: 1; max-width: 300px;">
                        <label class="form-label lbl-left" for="txtSubtotal">Subtotal:</label>
                        <asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control form-control-uniform" ReadOnly="true" />
                    </div>

                    <div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">

                        <div class="form-group d-flex align-items-end">
                            <button class="btn btn-sm btn-outline-dark" id="visibilityMessage">
                                <strong id="visibilityText">Agregar Legitimo</strong>
                            </button>
                        </div>
                        <div class="form-group  d-flex align-items-end">
                            <%--<asp:Button CssClass="btn btn-sm btn-outline-dark " ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClientClick="limpiarFiltros();" />--%>
                            <asp:Button CssClass="btn btn-sm btn-outline-dark " ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClick="BtnClearFilters_Click" />
                        </div>
                        <div class="form-group d-flex align-items-end">
                            <asp:Button CssClass="btn btn-sm btn-outline-dark" ID="btnFiltrar" Text="Filtrar" runat="server" OnClick="btnFiltrar_Click" />
                        </div>


                    </div>
                </div>
            </div>
        </div>

    </div>
    <hr />
    <asp:GridView ID="dgvLegitimos" DataKeyNames="ID" CssClass="table1  table-bordered table-hover  "
        OnRowDeleting="dgvLegitimos_RowDeleting" OnSelectedIndexChanged="dgvLegitimos_SelectedIndexChanged" AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
        <Columns>
            <asp:BoundField HeaderText="Area" DataField="Obra.Area.Nombre" />
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
            <asp:BoundField HeaderText="Linea de gestion" DataField="Linea" DataFormatString="{0:dd-MM-yyyy}" />
            <asp:CommandField ShowSelectButton="true" SelectText="Modificar" ControlStyle-CssClass="btn  btn-sm btn-outline-warning" />
            <asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-sm btn-outline-danger" DeleteText="Eliminar" />
        </Columns>
    </asp:GridView>
    <div class="text-center p-4">
        <asp:Label ID="lblMensaje" Text="" CssClass="text-success" runat="server" />
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
    <style>
    	.form-group label {
    		font-size: 14px;
    		color: #212529;
    		font-weight: 600;
    	}

    	.form-group .dropdown-toggle {
    		background-color: #f8f9fa;
    		color: #212529;
    		border-radius: 0.375rem;
    		width: 100%;
    		text-align: left;
    		font-size: 14px;
    		font-weight: normal;
    	}

    		.form-group .dropdown-toggle:hover {
    			background-color: #e2e6ea;
    			border-color: #adb5bd;
    			color: #212529;
    		}

    	.form-group .dropdown-menu {
    		border: 1px solid;
    		border-radius: 0.375rem;
    		padding: 0.5rem;
    		background-color: #ffffff;
    		max-height: 200px;
    		overflow-y: auto;
    	}

    		.form-group .dropdown-menu .form-check:hover {
    			background-color: transparent;
    		}

    		.form-group .dropdown-menu .form-check input[type="checkbox"]:focus {
    			outline: none;
    			box-shadow: none;
    		}

    		.form-group .dropdown-menu .form-check {
    			margin-bottom: 0.5rem;
    		}

    			.form-group .dropdown-menu .form-check label {
    				font-size: 14px;
    				color: #495057;
    				background-color: transparent;
    			}

    			.form-group .dropdown-menu .form-check input[type="checkbox"] {
    				margin-right: 8px;
    			}

    				.form-group .dropdown-menu .form-check input[type="checkbox"]:focus + label,
    				.form-group .dropdown-menu .form-check input[type="checkbox"]:checked + label {
    					color: #495057;
    					background-color: transparent;
    					font-weight: normal;
    				}

    	.table-bordered th, .table-bordered td {
    		border: 1px solid #dddddd;
    		text-align: center;
    	}

    	.table-hover tbody tr:hover {
    		background-color: #f2f2f2;
    	}

    	.table1 th, .table1 td {
    		padding: 12px;
    		font-size: 14px;
    	}

    	.table1 {
    		border-radius: 10px;
    		box-shadow: 0 2px 5px rgba(0, 0, 0, 0.15);
    	}

    		.table1 th {
    			text-align: center;
    			font-weight: bold;
    			background: #153244;
    			color: white;
    			border: 1px solid #153244;
    		}

    	.form-label {
    		margin-bottom: 0;
    	}

    	.d-flex.align-items-end > .form-control {
    		margin-right: 8px;
    	}

    	.form-control-uniform {
    		display: inline-block;
    		font-size: 14px;
    		padding: 4px 12px;
    		border: 1px solid;
    	}

    	.btn {
    		background-color: #153244;
    		text-align: center;
    		transition: all 0.3s ease-in-out;
    		color: #ecf0f1;
    		border: none;
    		padding: 8px 12px;
    		font-size: 14px;
    		cursor: pointer;
    		border-radius: 4px;
    		display: inline-block;
    		font-weight: bold;
    	}

    		.btn:hover {
    			background-color: #8DE2D6;
    			color: #153244;
    		}

    	.lbl-left {
    		text-align: left;
    		display: block;
    		font-weight: bold;
    	}

    	#visibilityMessage {
    		background-color: #8DE2D6;
    		text-align: center;
    		transition: all 0.3s ease-in-out;
    		color: #153244;
    	}

    	#visibilityText {
    		cursor: pointer;
    		display: inline-block;
    		transition: color 0.3s ease-in-out;
    	}

    	#visibilityMessage:hover {
    		background-color: #153244;
    	}

    		#visibilityMessage:hover #visibilityText {
    			color: white;
    		}
    </style>
</asp:Content>

