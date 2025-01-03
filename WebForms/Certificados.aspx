<%@ Page Title="" Language="C#" MasterPageFile="~/User.Master" AutoEventWireup="true" CodeBehind="Certificados.aspx.cs" Inherits="WebForms.Certificados" %>

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
            <!-- Sección de Agregar Autorizante -->
            <div class="col-md-12  rounded-3 p-3">
                <div class="mx-auto p-2">
                    <div class="card-body">
                        <table class="table table-bordered table-hover table-3d">
                            <thead class="thead-dark">
                                <tr>
                                    
                                    <th>Código Autorizante</th>
                                    <th>Expediente</th>
                                    <th>Tipo</th>
                                    <th>Monto Autorizado</th>
                                    <th>Mes Aprobacion</th>
                                    <th></th><th></th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <!-- Dropdowns y TextBoxes para agregar -->
                                 
                                   
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
                    <asp:GridView ID="dgvCertificado" DataKeyNames="ID" CssClass="table table-bordered table-hover table-3d"
                        OnSelectedIndexChanged="dgvCertificado_SelectedIndexChanged"
                        OnRowDeleting="dgvCertificado_RowDeleting"
                        AutoGenerateColumns="false" runat="server" AllowPaging="true"
                        PageSize="10" OnPageIndexChanging="dgvCertificado_PageIndexChanging">
                        <Columns>
                            <asp:BoundField HeaderText="Obra" DataField="Autorizante.Obra.Descripcion" />
                            <asp:BoundField HeaderText="Contrata" DataField="Autorizante.Obra.Contrata.Nombre" />
                            <asp:BoundField HeaderText="Código Autorizante" DataField="Autorizante.CodigoAutorizante" />
                            <asp:BoundField HeaderText="Expediente" DataField="ExpedientePago" />
                            <asp:BoundField HeaderText="Tipo" DataField="Tipo.Nombre" />
                            <asp:BoundField HeaderText="Monto Autorizado" DataField="MontoTotal" DataFormatString="{0:C}" />
                            <asp:BoundField HeaderText="Mes Aprobacion" DataField="MesAprobacion" DataFormatString="{0:dd-MM-yyyy}" />
                            <asp:BoundField HeaderText="Porcentaje" DataField="Porcentaje" DataFormatString="{0:N2}%" />

                            <asp:CommandField ShowSelectButton="true" SelectText="Modificar" ControlStyle-CssClass="btn btn-outline-warning" />
                            <asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-outline-danger" />
                        </Columns>
                    </asp:GridView>

                    <div class="text-center p-4">
                        <asp:Label ID="lblMensaje" Text="" runat="server" />
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
