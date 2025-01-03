<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ModificarCertificadoAdmin.aspx.cs" Inherits="WebForms.ModificarCertificadoAdmin" %>
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
                                    <th></th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <!-- Dropdowns y TextBoxes para agregar -->


                                    <td>
                                        <asp:TextBox ID="txtAutorizante" CssClass="form-control" runat="server" />
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
                                        <asp:Button Text="Modificar" ID="btnModificar" OnClick="btnModificar_Click" CssClass="btn btn-outline-success" runat="server" />
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
                            <div class="text-center p-4">
    <asp:Label ID="lblMensaje" Text="" runat="server" />
</div>
    </div>
</asp:Content>
