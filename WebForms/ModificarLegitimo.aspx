<%@ Page Title="" Language="C#" MasterPageFile="~/User.Master" AutoEventWireup="true" CodeBehind="ModificarLegitimo.aspx.cs" Inherits="WebForms.ModificarLegitimo" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
 

    <div class="container-fluid mt-4">
        <div class="row">
            <div class="col-md-12 rounded-3 p-3">
                <div class="mx-auto p-2">
                    <div class="card-body">
                        <table class="table table1">
                            <thead class="thead-dark">
                                <tr>
                                    <th>Código Autorizante</th>
                                    <th>Obra</th>
                                    <th>Expediente</th>
                                    <th>Inicio Ejecución</th>
                                    <th>Fin Ejecución</th>
                                    <th>Certificado</th>
                                    <th>Mes Aprobación</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>
                                        <asp:TextBox ID="txtCodigoAutorizante" CssClass="form-control" runat="server" Enabled="false"></asp:TextBox>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtObra" CssClass="form-control" runat="server" Enabled="false"></asp:TextBox>
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
                                        <asp:TextBox ID="txtMesAprobacion" CssClass="form-control" runat="server" />
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