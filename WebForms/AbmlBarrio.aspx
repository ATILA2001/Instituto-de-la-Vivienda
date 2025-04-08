<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AbmlBarrio.aspx.cs" Inherits="WebForms.AbmlBarrio" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid mt-4">
        <div class="row">
            <div class="col-12 mb-4">
                <div class="card shadow-sm border-0" >
                    <div class="card-body d-flex flex-wrap justify-content-between gap-3">
                        <div class="form-group" style="max-width: 300px; flex-grow: 1;">
                            <label class="form-label" for="txtBuscar" >Buscar:</label>
                            <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control form-control-uniform" 
                                        />
                        </div>
                        <div class="form-group d-flex align-items-end">
                            <asp:Button CssClass="btn btn-primary" ID="btnFiltrar" Text="Filtrar" runat="server" OnClick="btnFiltrar_Click" 
                                        />
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-md-8 mb-4">
                  <div class="card-body">
                        <asp:GridView ID="dgvBarrio" DataKeyNames="ID" CssClass="table1 table-hover" 
                                      OnSelectedIndexChanged="dgvBarrio_SelectedIndexChanged" 
                                      OnRowDeleting="dgvBarrio_RowDeleting"
                                      AutoGenerateColumns="false" runat="server">
                            <Columns>
                                <asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />
                                <asp:BoundField HeaderText="Barrio" DataField="Nombre"   />
                                <asp:CommandField ShowSelectButton="true" SelectText="Modificar" 
                                                  ControlStyle-CssClass="btn btn-warning" 
                                                  />
                                <asp:CommandField ShowDeleteButton="true" 
                                                  ControlStyle-CssClass="btn btn-danger"  
                                                   />
                            </Columns>
                        </asp:GridView>
                    </div>
                
            </div>

            <div class="col-md-4">
                <div class="card shadow-sm border-0" style="border-radius: 12px;">
                    <div class="card-header text-center" style="background-color: #153244; color: #ecf0f1;  border-radius: 12px 12px 0 0;">
                        Agregar Barrio
                    </div>
                    <div class="card-body">
                        <div class="mb-2">
                            <label for="txtNombre" class="form-label" style="color: #153244; ">Nombre</label>
                            <asp:TextBox ID="txtNombre" CssClass="form-control" runat="server" 
                                          />
                        </div>
                        <div class="mb-2 text-center p-2">
                            <asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" 
                                        CssClass="btn btn-primary" runat="server" />
                        </div>
                    </div>
                    <div class="text-center p-4">
                        <asp:Label ID="lblMensaje" Text="" runat="server"  />
                    </div>
                </div>
            </div>
        </div>
    </div>


</asp:Content>
