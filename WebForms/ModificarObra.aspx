<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ModificarObra.aspx.cs" Inherits="WebForms.ModificarObra" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
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
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<div class="container-fluid mt-4">
    <div class="row">
        <div class="col-md-12 bg-light rounded-3 p-3">
            <h2 class="text-center p-2">OBRAS</h2>
            <div class="mx-auto p-2">
                <div class="card-body">
                    <table class="table table-3d">
                        <thead class="thead-dark">
                            <tr>
                                <th>Empresa</th>
                                <th>Contrata</th>
                                <th>Número</th>
                                <th>Año</th>
                                <th>Etapa</th>
                                <th>Obra</th>
                                <th>Barrio</th>
                                <th>Descripción</th>
                                <th></th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>
                                    <asp:DropDownList ID="ddlEmpresa" CssClass="form-control" runat="server"></asp:DropDownList>
                                </td>
                                <td>
                                    <asp:DropDownList ID="ddlContrata" CssClass="form-control" runat="server"></asp:DropDownList>
                                </td>
                                <td>
                                    <asp:TextBox ID="txtNumero" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
                                </td>

                                <td>
                                    <asp:TextBox ID="txtAño" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
                                </td>
                                <td>
                                    <asp:TextBox ID="txtEtapa" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
                                </td>
                                <td>
                                    <asp:TextBox ID="txtObra" CssClass="form-control" runat="server" onkeypress="return soloNumeros(event)" />
                                </td>
                                <td>
                                    <asp:DropDownList ID="ddlBarrio" CssClass="form-control" runat="server"></asp:DropDownList>
                                </td>
                                <td>
                                    <asp:TextBox ID="txtDescripcion" CssClass="form-control" runat="server" TextMode="Multiline" />
                                </td>
                                <td class="text-right">
                                    <asp:Button Text="Modificar" ID="btnModificar" OnClick="btnModificar_Click"
                                        CssClass="btn btn-outline-success" runat="server" />


                            </tr>
                        </tbody>
                    </table>
                    <div class="text-center p-4">
    <asp:Label ID="lblMensaje" Text="" runat="server" />
</div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
