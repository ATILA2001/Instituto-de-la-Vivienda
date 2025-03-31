<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="MovimientosGestion.aspx.cs" Inherits="WebForms.MovimientosGestion" %>

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
                            <th>Movimiento</th>
                            <th>Fecha</th>
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
                                <asp:TextBox ID="txtMovimiento" CssClass="form-control" runat="server" />
                            </td>
                            <td>
                                <asp:TextBox ID="txtFecha" CssClass="form-control" runat="server" TextMode="Date" />
                            </td>
                            <td class="text-right">
                                <asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click"
                                    CssClass="btn btn-outline-light" runat="server" />
                            </td>
                            <td class="text-right">
                                <asp:Button Text="Limpiar" ID="btnLimpiar" OnClick="btnLimpiar_Click"
                                    CssClass="btn btn-outline-light" runat="server" /></td>
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
                                <label class="form-label lbl-left" style="margin-left: 10PX;" for="cblObra">Obra:</label>
                                <div class="dropdown">
                                    <%--<button class="btn btn-sm dropdown-toggle" type="button" id="dropdownEmpresa" data-bs-toggle="dropdown" aria-expanded="false">
								Todas
                   
							</button>
							<ul class="dropdown-menu p-2" aria-labelledby="dropdownEmpresa" style="max-height: 200px; overflow-y: auto;">
								<!-- Rendimos la CheckBoxList aquí -->
								<asp:CheckBoxList ID="cblEmpresa" runat="server" CssClass="dropdown-item form-check" />
							</ul>
                                    --%>
                                    <CustomControls:CheckBoxListSearch ID="cblObra" runat="server" />
                                </div>
                            </div>

                            <div class="form-group">
                                <label class="form-label lbl-left" style="margin-left: 10PX;" for="cblFecha">Fecha:</label>
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

                            <div class="form-group text-left" style="flex: 1; max-width: 300px;">
                                <label class="form-label lbl-left" for="txtBuscar">Buscar:</label>
                                <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control form-control-uniform"></asp:TextBox>
                            </div>

                            <div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">





                                <div class="form-group d-flex align-items-end">
                                    <button class="btn btn-sm btn-outline-dark" id="visibilityMessage">
                                        <strong id="visibilityText">Cargar Movimiento</strong>
                                    </button>
                                </div>


                                <div class="form-group  d-flex align-items-end">
                                    <asp:Button CssClass="btn btn-sm btn-outline-dark " ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClick="BtnClearFilters_Click" />
                                </div>
                                <div class="form-group d-flex align-items-end">
                                    <asp:Button CssClass="btn btn-sm btn-outline-dark" ID="btnFiltrar" Text="Filtrar" runat="server" OnClick="btnFiltrar_Click" />
                                </div>
                            </div>
                        </div>
                    </div>

                    <hr />
                    <asp:GridView ID="dgvMovimiento" DataKeyNames="ID" CssClass="table1  table-bordered table-hover"
                        OnSelectedIndexChanged="dgvMovimiento_SelectedIndexChanged"
                        OnRowDeleting="dgvMovimiento_RowDeleting"
                        AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
                        <Columns>
                            <asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />
                            <asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" />
                                             <asp:BoundField HeaderText="Proyecto" DataField="Proyecto" />
                            <asp:BoundField HeaderText="SubProyecto" DataField="SubProyecto" />
                            <asp:BoundField HeaderText="Linea de Gestion" DataField="Linea" />


                            <asp:BoundField HeaderText="Movimiento" DataField="Monto" DataFormatString="{0:C}" />
                            <asp:BoundField HeaderText="Fecha" DataField="Fecha" DataFormatString="{0:dd-MM-yyyy}" />
                                    <asp:BoundField HeaderText="Autorizado Nuevo" DataField="AutorizadoNuevo" DataFormatString="{0:C}" />
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
        function soloNumeros(e) {
            var tecla = (document) ? e.keyCode : e.which;
            if (tecla == 8 || tecla == 46) {
                return true;
            }
            var patron = /^[0-9]$/;
            var te = String.fromCharCode(tecla);
            return patron.test(te);
        }

        $(document).ready(function () {
            // Textos constantes
            const mostrarTexto = "Cargar Movimiento";
            const ocultarTexto = "Ocultar sección";

            // Variables DOM
            const section = $('#section1');
            const visibilityText = $('#visibilityText');
            const visibilityMessage = $('#visibilityMessage');

            // Asegúrate de que los elementos existan
            if (!section.length || !visibilityText.length || !visibilityMessage.length) {
                console.error("Uno o más elementos no se encontraron en el DOM.");
                return;
            }

            // Función para inicializar el estado
            const initializeVisibility = () => {
                // Lee el estado del localStorage o usa "false" por defecto
                const sectionVisible = localStorage.getItem("sectionVisible") === "true";

                // Aplica visibilidad y actualiza el texto del botón
                section.toggle(sectionVisible);
                visibilityText.text(sectionVisible ? ocultarTexto : mostrarTexto);
            };

            // Función para alternar la visibilidad
            const toggleVisibility = () => {
                // Comprueba el estado actual de la sección
                const sectionVisible = section.is(':visible');

                // Guarda el nuevo estado en localStorage
                localStorage.setItem("sectionVisible", !sectionVisible);

                // Alterna la visibilidad de la sección
                section.toggle();

                // Actualiza el texto del botón
                visibilityText.text(sectionVisible ? mostrarTexto : ocultarTexto);
            };

            // Inicializa al cargar la página
            initializeVisibility();

            // Asigna el evento al botón
            visibilityMessage.off('click').on('click', toggleVisibility);
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
