<%@ Page Title="" Language="C#" MasterPageFile="~/User.Master" AutoEventWireup="true" CodeBehind="Legitimos.aspx.cs" Inherits="WebForms.Legitimos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <style>
        .table-3d {
            box-shadow: 0 10px 15px rgba(0, 0, 0, 0.2), 0 4px 6px rgba(0, 0, 0, 0.1);
            transform: translateY(-5px);
            transition: transform 0.3s ease, box-shadow 0.3s ease;
            border-radius: 10px; /* Borde redondeado */
            overflow: hidden; /* Para que el contenido no sobresalga de los bordes */
        }

            .table-3d:hover {
                box-shadow: 0 15px 25px rgba(0, 0, 0, 0.3), 0 8px 10px rgba(0, 0, 0, 0.2);
                transform: translateY(-10px);
            }
    </style>
    <div class="container-fluid mt-4">
        <div class="row">
            <!-- Sección de Agregar Legitimos Abonos -->
            <div class="col-md-12 rounded-3 p-3">
                <div class="card mx-auto p-2">
                    <div class="card-body">
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
                                    <!-- Controles para agregar -->
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
                                        <asp:TextBox ID="txtCertificado" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
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
        </div>
    </div>
    <hr />
    <div class="container-fluid mt-4">
        <div class="row mt-4">
            <div class="col-md-12">
                <asp:GridView ID="dgvLegitimos" DataKeyNames="CodigoAutorizante" CssClass="table  "
                    OnRowDeleting="dgvLegitimos_RowDeleting" AutoGenerateColumns="false" runat="server"
                    AllowPaging="true" PageSize="10" OnPageIndexChanging="dgvLegitimos_PageIndexChanging">
                    <Columns>
                        <asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
                        <asp:BoundField HeaderText="Código Autorizante" DataField="CodigoAutorizante" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
                        <asp:BoundField HeaderText="Expediente" DataField="Expediente" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
                        <asp:BoundField HeaderText="Inicio Ejecución" DataField="InicioEjecucion" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
                        <asp:BoundField HeaderText="Fin Ejecución" DataField="FinEjecucion" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
                        <asp:BoundField HeaderText="Certificado" DataField="Certificado" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
                        <asp:BoundField HeaderText="Mes Aprobación" DataField="MesAprobacion" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
                        <asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-outline-danger" DeleteText="Eliminar" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
                    </Columns>
                </asp:GridView>
                <div class="text-center p-4">
                    <asp:Label ID="lblMensaje" Text="" CssClass="text-success" runat="server" />
                </div>
            </div>
        </div>
    </div>
    <script type="text/javascript">
        function soloNumeros(e) {
            var tecla = (document.all) ? e.keyCode : e.which;
            if (tecla == 8 || tecla == 46) {
                return true;
            }
            var patron = /^[0-9]$/;
            var te = String.fromCharCode(tecla);
            return patron.test(te);
        }
    </script>
</asp:Content>
