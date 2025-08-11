<%@ Page Title="Cerrar Sesión" Language="C#" MasterPageFile="~/LoginRegister.Master" AutoEventWireup="true" CodeBehind="Logout.aspx.cs" Inherits="WebForms.Logout" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="d-flex justify-content-center align-items-center" style="min-height: 80vh;">
        <div class="container-fluid" style="max-width: 28rem;">
            <div class="text-center">
                <div class="card shadow-lg border-0">
                    <div class="card-body p-5">
                        <div class="mb-4">
                            <i class="fas fa-check-circle fa-3x text-success mb-3"></i>
                            <h2 class="mb-3">Sesión Cerrada Exitosamente</h2>
                            <p class="text-muted">Has cerrado sesión de forma segura.</p>
                            <p class="text-muted">Gracias por usar nuestro sistema.</p>
                        </div>
                        
                        <div class="d-grid gap-2">
                            <a href="Login.aspx" class="btn btn-primary btn-lg">
                                <i class="fas fa-sign-in-alt me-2"></i>Iniciar Sesión Nuevamente
                            </a>
                        </div>
                        
                        <div class="mt-4">
                            <small class="text-muted">
                                Puedes cerrar esta ventana de forma segura.
                            </small>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
