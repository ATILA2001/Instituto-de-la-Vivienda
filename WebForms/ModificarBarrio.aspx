<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="ModificarBarrio.aspx.cs" Inherits="WebForms.ModificarBarrio" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
 <h1 class="text-center p-2">Modificar Barrio</h1>

 <asp:ScriptManager ID="ScriptManager1" runat="server" />


 <div class="container-fluid" style="max-width: 50%;">
     <div class="card mb-2">
         <div class="card-body">


             <asp:UpdatePanel runat="server">
                 <ContentTemplate>
                     <div class="container overflow-hidden text-center">
                         <div class="row">
                             <div class="col-6">
                                 <div class="p-3">
                                     <asp:Label ID="lblNombre" CssClass="fs-5" Text="Nombre" runat="server" />
                                     <asp:TextBox ID="txtNombre"  CssClass="form-control"  runat="server" />
                                 </div>
                             </div>
                             <div class="mb-2 text-center p-2">
                                 <asp:Button Text="Volver" ID="btnVolver" CssClass="btn btn-outline-secondary" OnClick="btnVolver_Click" runat="server" />
                                 <asp:Button Text="Modificar" ID="btnModificar" OnClick="btnModificar_Click" CssClass="btn btn-outline-success" runat="server" />
                             </div>
                         </div>
                     </div>

                 </ContentTemplate>
             </asp:UpdatePanel>
         </div>
     </div>
 </div>
 <div class="text-end p-4">
     <asp:Label ID="lblMensaje" Text="" runat="server" />

 </div>

</asp:Content>
