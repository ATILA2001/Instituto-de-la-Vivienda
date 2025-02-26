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
		color: #212529;
		font-weight: 600;
	}

	.form-group .dropdown-toggle {
		background-color: #f8f9fa;
		color: #212529;
		border-radius: 0.375rem;
		width: 100%;
		text-align: left;
		font-size: 14px;
		font-weight: normal;
	}

		.form-group .dropdown-toggle:hover {
			background-color: #e2e6ea;
			border-color: #adb5bd;
			color: #212529;
		}

	.form-group .dropdown-menu {
		border: 1px solid;
		border-radius: 0.375rem;
		padding: 0.5rem;
		background-color: #ffffff;
		max-height: 200px;
		overflow-y: auto;
	}

		.form-group .dropdown-menu .form-check:hover {
			background-color: transparent;
		}

		.form-group .dropdown-menu .form-check input[type="checkbox"]:focus {
			outline: none;
			box-shadow: none;
		}

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
					color: #495057;
					background-color: transparent;
					font-weight: normal;
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

	.table1 {
		border-radius: 10px;
		box-shadow: 0 2px 5px rgba(0, 0, 0, 0.15);
	}

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
		margin-right: 8px;
	}

	.form-control-uniform {
		display: inline-block;
		font-size: 14px;
		padding: 4px 12px;
		border: 1px solid;
	}

	.btn {
		background-color: #153244;
		text-align: center;
		transition: all 0.3s ease-in-out;
		color: #ecf0f1;
		border: none;
		padding: 8px 12px;
		font-size: 14px;
		cursor: pointer;
		border-radius: 4px;
		display: inline-block;
		font-weight: bold;
	}

		.btn:hover {
			background-color: #8DE2D6;
			color: #153244;
		}

	.lbl-left {
		text-align: left;
		display: block;
		font-weight: bold;
	}

	#visibilityMessage {
		background-color: #8DE2D6;
		text-align: center;
		transition: all 0.3s ease-in-out;
		color: #153244;
	}

	#visibilityText {
		cursor: pointer;
		display: inline-block;
		transition: color 0.3s ease-in-out;
	}

	#visibilityMessage:hover {
		background-color: #153244;
	}

		#visibilityMessage:hover #visibilityText {
			color: white;
		}
</style>
</asp:Content>
