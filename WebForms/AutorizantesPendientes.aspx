<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AutorizantesPendientes.aspx.cs" Inherits="WebForms.AutorizantesPendientes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

	<div class="row mt-4">
		<div class="col-md-12">
			<div class="text-end">
				<!-- Contenedor para subtotal alineado a la izquierda -->
				<div class="d-flex flex-wrap justify-content-between p-3 gap-3">
					<!-- Subtotal alineado a la izquierda -->
					<div class="form-group text-left" style="flex: 1; max-width: 300px;">
						<label class="form-label lbl-left" for="txtSubtotal">Subtotal:</label>
						<asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control form-control-uniform" ReadOnly="true" />
					</div>

					<!-- Filtros alineados a la derecha -->
					<div class="d-flex flex-wrap justify-content-end gap-3" style="flex: 3;">
						<div class="form-group">
							<label class="form-label lbl-left" for="ddlEmpresa">Empresa:</label>
							<asp:DropDownList ID="ddlEmpresa" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlEmpresa_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White" Width="300px">
							</asp:DropDownList>
						</div>
						<div class="form-group">
							<label class="form-label lbl-left" for="ddlObraFiltro">Obra:</label>
							<asp:DropDownList ID="ddlObraFiltro" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlObraFiltro_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White" Width="500px">
							</asp:DropDownList>
						</div>
						<div class="form-group">
							<label class="form-label lbl-left" for="ddlEstadoFiltro">Estado:</label>
							<asp:DropDownList ID="ddlEstadoFiltro" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlEstadoFiltro_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White">
							</asp:DropDownList>
						</div>
						<div class="form-group">
							<label class="form-label lbl-left" for="ddlAreaFiltro">Area:</label>
							<asp:DropDownList ID="ddlAreaFiltro" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlAreaFiltro_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White">
							</asp:DropDownList>
						</div>
					</div>
				</div>
			</div>





			<asp:GridView ID="dgvAutorizante" DataKeyNames="CodigoAutorizante" CssClass="table1  table-bordered table-hover "
				OnSelectedIndexChanged="dgvAutorizante_SelectedIndexChanged"
				OnRowDeleting="dgvAutorizante_RowDeleting"
				AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
				<Columns>
										<asp:BoundField HeaderText="Obra" DataField="Obra.Id" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" Visible="false"/>

					<asp:BoundField HeaderText="Área" DataField="Obra.Area.Nombre" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

					<asp:BoundField HeaderText="Contrata" DataField="Obra.Contrata.Nombre" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Empresa" DataField="Empresa" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

					<asp:BoundField HeaderText="Código Autorizante" DataField="CodigoAutorizante" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

					<asp:BoundField HeaderText="Concepto" DataField="Concepto.Nombre" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:BoundField HeaderText="Detalle" DataField="Detalle" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />


					<asp:BoundField HeaderText="Expediente" DataField="Expediente" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

					<asp:BoundField HeaderText="Estado" DataField="Estado.Nombre" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

					<asp:BoundField HeaderText="Monto Autorizado" DataField="MontoAutorizado" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

					<asp:BoundField HeaderText="Mes Aprobacion" DataField="Fecha" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

					<asp:TemplateField HeaderText="Autorización GG" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center">
						<ItemTemplate>
							<%# Eval("AutorizacionGG") != null && (bool)Eval("AutorizacionGG") ? "AUTORIZADO" : "A AUTORIZAR" %>
						</ItemTemplate>
					</asp:TemplateField>



					<asp:CommandField ShowSelectButton="true" SelectText="Modificar" ControlStyle-CssClass="btn btn-outline-warning" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					<asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-outline-danger" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
				</Columns>
			</asp:GridView>

			<div class="text-center p-4">
				<asp:Label ID="lblMensaje" Text="" runat="server" />
			</div>
		</div>
	</div>
	<style>
	.form-group label {
		font-size: 14px;
		color: #212529; /* Gris oscuro para un aspecto formal */
		font-weight: 600; /* Peso semibold */
	}

	/* Estilo del botón dropdown */
	.form-group .dropdown-toggle {
		background-color: #f8f9fa; /* Fondo claro */
		color: #212529; /* Texto negro */
		border-radius: 0.375rem; /* Bordes redondeados suaves */
		width: 100%; /* Ocupa todo el ancho */
		text-align: left; /* Alineación del texto a la izquierda */
		font-size: 14px; /* Tamaño de texto claro y sobrio */
		font-weight: normal; /* Si necesitas enfatizar el texto */
	}

		.form-group .dropdown-toggle:hover {
			background-color: #e2e6ea; /* Color de fondo en hover */
			border-color: #adb5bd; /* Color del borde en hover */
			color: #212529;
		}

	/* Estilo del menú desplegable */
	.form-group .dropdown-menu {
		border: 1px solid; /* Mismo borde que el botón */
		border-radius: 0.375rem; /* Bordes redondeados */
		padding: 0.5rem; /* Espaciado interno */
		background-color: #ffffff; /* Fondo blanco */
		max-height: 200px;
		overflow-y: auto; /* Scroll para contenido largo */
	}

		.form-group .dropdown-menu .form-check:hover {
			background-color: transparent;
		}

		/* Opcional: elimina el contorno (outline) en estados de foco */
		.form-group .dropdown-menu .form-check input[type="checkbox"]:focus {
			outline: none;
			box-shadow: none;
		}
		/* Estilo de los items en el dropdown */
		.form-group .dropdown-menu .form-check {
			margin-bottom: 0.5rem;
		}

			.form-group .dropdown-menu .form-check label {
				font-size: 14px;
				color: #495057;
				background-color: transparent;
			}

			.form-group .dropdown-menu .form-check input[type="checkbox"] {
				margin-right: 8px;
			}

				.form-group .dropdown-menu .form-check input[type="checkbox"]:focus + label,
				.form-group .dropdown-menu .form-check input[type="checkbox"]:checked + label {
					color: #495057; /* Mantener el color gris */
					background-color: transparent; /* Sin fondo azul */
					font-weight: normal; /* Opcional, mantiene el estilo regular */
				}



	.table-bordered th, .table-bordered td {
		border: 1px solid #dddddd;
		text-align: center;
	}

	.table-hover tbody tr:hover {
		background-color: #f2f2f2;
	}

	.table1 th, .table1 td {
		padding: 12px;
		font-size: 14px;
	}

	/* Bordes redondeados y sombras en el GridView */
	.table1 {
		border-radius: 10px;
		box-shadow: 0 2px 5px rgba(0, 0, 0, 0.15);
	}

		/* Mejorar el estilo de las celdas del encabezado */
		.table1 th {
			text-align: center;
			font-weight: bold;
			background: #153244;
			color: white;
			border: 1px solid #153244;
		}

	.form-label {
		margin-bottom: 0;
	}

	.d-flex.align-items-end > .form-control {
		margin-right: 8px; /* Margen entre el campo y el botón */
	}

	.form-control-uniform {
		display: inline-block;
		font-size: 14px; /* Tamaño de texto uniforme */
		padding: 4px 12px;
		border: 1px solid;
	}

	.btn {
		background-color: #153244; /* Azul institucional */
		text-align: center;
		transition: all 0.3s ease-in-out;
		color: #ecf0f1; /* Blanco suave para el texto */
		border: none;
		padding: 8px 12px;
		font-size: 14px;
		cursor: pointer;
		border-radius: 4px; /* Bordes redondeados */
		display: inline-block;
		font-weight: bold; /* Si necesitas enfatizar el texto */
	}

		.btn:hover {
			background-color: #2c3e50; /* Gris oscuro al pasar el cursor */
			color: white;
		}




	.lbl-left {
		text-align: left;
		display: block; /* Asegura que el label ocupe toda la línea si es necesario */
		font-weight: bold; /* Si necesitas enfatizar el texto */
	}


	#visibilityMessage {
		background-color: #FFCC00; /* Fondo más neutro (gris claro) */
		text-align: center; /* Centrar el contenido */
		transition: all 0.3s ease-in-out; /* Suaviza las animaciones */
		color: #ecf0f1; /* Blanco suave para el texto */
	}

	#visibilityText {
		cursor: pointer;
		display: inline-block;
		transition: color 0.3s ease-in-out;
	}

	#visibilityMessage:hover {
		background-color: #FFCC00; /* Gris oscuro cuando se pasa el cursor */
	}


		#visibilityMessage:hover #visibilityText {
			color: white; /* Resaltar cuando se pasa el mouse */
		}
</style>
</asp:Content>
