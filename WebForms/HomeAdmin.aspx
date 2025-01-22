<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="HomeAdmin.aspx.cs" Inherits="WebForms.HomeAdmin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<div class="container mt-5">
		<!-- Primera fila -->
		<div class="row g-5">
			<!-- Tarjeta 1 -->
			<div class="col-md-6 col-lg-4">
				<div class="card">
					<div class="card-body">
						<h5 class="card-title">AUTORIZANTES</h5>
						<p class="card-text">Autorizantes pendientes de aprobación.</p>
						<a href="AutorizantesPendientes.aspx" class="btn btn-primary">Ver más</a>
					</div>
				</div>
			</div>

			<!-- Tarjeta 2 -->
			<div class="col-md-6 col-lg-4">
				<div class="card">
					<div class="card-body">
						<h5 class="card-title">USUARIOS</h5>
						<p class="card-text">Usuarios pendientes de habilitación.</p>
						<a href="UsuariosPendientes.aspx" class="btn btn-primary">Ver más</a>
					</div>
				</div>
			</div>
						<!-- Tarjeta 3 -->
			<div class="col-md-6 col-lg-4">
				<div class="card">
					<div class="card-body">
						<h5 class="card-title">DEVENGADOS</h5>
						<p class="card-text">Tabla devengados 2025.</p>
						<a href="Devengados.aspx" class="btn btn-primary">Ver más</a>
					</div>
				</div>
			</div>

		</div>
	</div>

	<br />	<br />	<br />	<br />	

	<style>
	    /* Estilo de tarjetas */
	    .card {
	        background: rgba(255, 255, 255, 0.9); /* Fondo blanco traslúcido */
	        border-radius: 20px;
	        box-shadow: 0 10px 20px rgba(0, 0, 0, 0.2);
	        transition: transform 0.3s, box-shadow 0.3s;
	        overflow: hidden;
	    }

	        .card:hover {
	            transform: translateY(-10px);
	            box-shadow: 0 20px 40px rgba(0, 0, 0, 0.3);
	        }

	    .card-body {
	        padding: 30px;
	        text-align: center;
	    }

	    .card-title {
	        font-size: 1.8rem;
	        font-family: var(--fuente_bold);
	        color: var(--color-principal);
	        margin-bottom: 20px;
	    }

	    .card-text {
	        font-size: 1rem;
	        font-family: var(--fuente_normal);
	        color: var(--color-secundario);
	        margin-bottom: 30px;
	    }

	    /* Botones llamativos */
	    .btn-primary {
	        font-family: var(--fuente_bold);
	        font-size: 1rem;
	        color: #fff;
	        background: linear-gradient(45deg, #f39c12, #e67e22);
	        padding: 10px 30px;
	        border-radius: 50px;
	        border: none;
	        transition: all 0.3s ease-in-out;
	    }

	        .btn-primary:hover {
	            background: linear-gradient(45deg, #e67e22, #d35400);
	            transform: scale(1.1);
	        }

	    /* Animaciones globales */
	    .card, .btn-primary {
	        animation: fadeInUp 0.5s ease-in-out;
	    }

	    @keyframes fadeInUp {
	        from {
	            opacity: 0;
	            transform: translateY(20px);
	        }

	        to {
	            opacity: 1;
	            transform: translateY(0);
	        }
	    }
	</style>
</asp:Content>
