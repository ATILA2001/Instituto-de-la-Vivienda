<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="LineasGestion.aspx.cs" Inherits="WebForms.LineasGestion" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
      <div class="container-fluid mt-4">
        <div class="row">
            <!-- Sección Listado (70%) -->
            <div class="col-md-8 border-end">
                <h2 class="text-center p-2">Listado de Líneas de Gestión</h2>
                <asp:GridView ID="dgvLineaGestion" DataKeyNames="ID" CssClass="table " 
                              OnSelectedIndexChanged="dgvLineaGestion_SelectedIndexChanged" 
                              OnRowDeleting="dgvLineaGestion_RowDeleting"
                              AutoGenerateColumns="false" runat="server" AllowPaging="true" 
                              PageSize="10" OnPageIndexChanging="dgvLineaGestion_PageIndexChanging">
                    <Columns>
                        <asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />
                        <asp:BoundField HeaderText="Nombre" DataField="Nombre"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
                        <asp:BoundField HeaderText="Tipo" DataField="Tipo" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center"  />
                        <asp:BoundField HeaderText="Grupo" DataField="Grupo"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
                        <asp:BoundField HeaderText="Repartición" DataField="Reparticion"  HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
                        <asp:CommandField ShowSelectButton="true" SelectText="Modificar" 
                                          ControlStyle-CssClass="btn btn-outline-warning" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center"  />
                        <asp:CommandField ShowDeleteButton="true" 
                                          ControlStyle-CssClass="btn btn-outline-danger" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center"  />
                    </Columns>
                </asp:GridView>
            </div>

            <!-- Sección Agregar Línea de Gestión (30%) con color de fondo -->
            <div class="col-md-4 bg-light rounded-3 p-3">
                <h2 class="text-center p-2">Agregar Línea de Gestión</h2>
                <div class="mx-auto p-2">
                    <div class="card">
                        <div class="card-body">
                            <div class="mb-2">
                                <label for="txtNombre" class="form-label">Nombre</label>
                                <asp:TextBox ID="txtNombre" CssClass="form-control" runat="server" />
                            </div>
                            <div class="mb-2">
                                <label for="txtTipo" class="form-label">Tipo</label>
                                <asp:TextBox ID="txtTipo" CssClass="form-control" runat="server" />
                            </div>
                            <div class="mb-2">
                                <label for="txtGrupo" class="form-label">Grupo</label>
                                <asp:TextBox ID="txtGrupo" CssClass="form-control" runat="server" />
                            </div>
                            <div class="mb-2">
                                <label for="txtReparticion" class="form-label">Repartición</label>
                                <asp:TextBox ID="txtReparticion" CssClass="form-control" runat="server" />
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
