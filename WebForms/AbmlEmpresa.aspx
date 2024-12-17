<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AbmlEmpresa.aspx.cs" Inherits="WebForms.AbmlEmpresa" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
     <style>
        .section-divider {
            margin: 2rem 0;
            border-top: 2px solid #ccc;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid mt-4">
        <div class="row">
            <!-- Sección Listado (70%) -->
            <div class="col-md-8 border-end">
                <h2 class="text-center p-2">Listado de Empresas</h2>
                <asp:GridView ID="dgvEmpresa" DataKeyNames="ID" CssClass="table table-dark table-bordered" 
                              OnSelectedIndexChanged="dgvEmpresa_SelectedIndexChanged" 
                              OnRowDeleting="dgvEmpresa_RowDeleting"
                              AutoGenerateColumns="false" runat="server" AllowPaging="true" 
              PageSize="10" OnPageIndexChanging="dgvEmpresa_PageIndexChanging">
                    <Columns>
                        <asp:BoundField HeaderText="ID" DataField="Id" />
                        <asp:BoundField HeaderText="Empresa" DataField="Nombre" />
                        <asp:CommandField ShowSelectButton="true" SelectText="Modificar" 
                                          ControlStyle-CssClass="btn btn-outline-warning" />
                        <asp:CommandField ShowDeleteButton="true" 
                                          ControlStyle-CssClass="btn btn-outline-danger" />
                    </Columns>
                </asp:GridView>
            </div>

            <!-- Sección Agregar Área (30%) con color de fondo -->
            <div class="col-md-4 bg-light rounded-3 p-3">
                <h2 class="text-center p-2">Agregar Empresa</h2>
                <div class="mx-auto p-2">
                    <div class="card">
                        <div class="card-body">
                            <div class="mb-2">
                                <label for="txtNombre" class="form-label">Nombre</label>
                                <asp:TextBox ID="txtNombre" CssClass="form-control" runat="server" />
                            </div>
                            <div class="mb-2 text-center p-2">
                                <asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" 
                                            CssClass="btn btn-outline-success" runat="server" />
                            </div>
                        </div>
                    </div>
                </div>
                <!-- Mensaje de éxito o error -->
                <div class="text-center p-4">
                    <asp:Label ID="lblMensaje" Text="" runat="server" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>