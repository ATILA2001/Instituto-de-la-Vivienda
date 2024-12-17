<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="HomeAdmin.aspx.cs" Inherits="WebForms.HomeAdmin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

 <br /><h2 style="text-align:center">Inicio</h2>
    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" rel="stylesheet">
  <style>
    .card {
        height: 155px;
        width: 520px;
        border-radius: 15px; 
        border: 2px solid #343a40; 
    }
    .card-body {
        display: flex;
        flex-direction: column;
        justify-content: space-between;
        height: 100%;
    }
    .card-title {
        margin-bottom: 10px;
    }
    .card-text {
        flex-grow: 1;
    }
    .card-badge {
        position: absolute;
        top: 10px;
        right: 10px;
    }
    .fixed-width-btn {
        width: 150px;
        text-align: center;
    }
    .button-container {
      display: flex;
      justify-content: flex-end;
    }
  </style>
   <div class="container mt-5">
    <div class="row justify-content-center mb-5">
      <div class="col-sm-6">
        <div class="card">
          <span class="badge badge-danger card-badge">New</span>
          <div class="card-body">
            <h5 class="card-title">Pedidos Activos</h5>
            <p class="card-text">Nuevos pedidos realizados, actualizacion de estado.</p>
              <div class="button-container">
                  <a href="ListadoVentas.aspx" class="btn btn-primary fixed-width-btn">Ver más</a>
              </div>
          </div>
        </div>
      </div>
      <div class="col-sm-6">
        <div class="card">
          <div class="card-body">
            <h5 class="card-title">Clientes</h5>
            <p class="card-text">Listado historico de clientes.</p>
              <div class="button-container">
                  <a href="Clientes.aspx" class="btn btn-primary fixed-width-btn">Ver más</a>
              </div>
          </div>
        </div>
      </div>
    </div>

    <div class="row justify-content-center mb-5">
      <div class="col-sm-6">
        <div class="card">
          <div class="card-body">
            <h5 class="card-title">Ventas</h5>
            <p class="card-text">Historial de ventas realizadas.</p>
              <div class="button-container">
                  <a href="ListadoDetalleVentas.aspx" class="btn btn-primary fixed-width-btn">Ver más</a>
              </div>
          </div>
        </div>
      </div>
      <div class="col-sm-6">
        <div class="card">
          <div class="card-body">
            <h5 class="card-title"> Control de Stock</h5>
            <p class="card-text">Productos con un stock menor diez.</p>
              <div class="button-container">
                  <a href="Stock.aspx" class="btn btn-primary fixed-width-btn">Ver más</a>
              </div>
          </div>
        </div>
      </div>
    </div>
        </div>

</asp:Content>
