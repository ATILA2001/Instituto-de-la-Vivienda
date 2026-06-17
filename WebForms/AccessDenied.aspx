<%@ Page Title="" Language="C#" MasterPageFile="~/LoginRegister.Master" AutoEventWireup="true" CodeBehind="AccessDenied.aspx.cs" Inherits="WebForms.AccessDenied" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="card p-4" style="max-width: 720px; width: 100%;">
        <h2 class="mb-3">Acceso denegado</h2>
        <p class="mb-4">No tenés permisos para visualizar esta página.</p>
        <div class="d-flex gap-2">
            <a class="btn btn-primary" href="javascript:history.back()">Volver atrás</a>
        </div>
    </div>
</asp:Content>
