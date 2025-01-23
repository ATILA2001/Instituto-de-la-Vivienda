<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="CertificadosAdmin.aspx.cs" Inherits="WebForms.CertificadosAdmin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

	<div class="container-fluid mt-4">
		<div class="row mt-4">

			<div class="col-md-12">
				<div class="text-end">
					<asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control form-control-uniform" ReadOnly="true" />

					<asp:DropDownList ID="ddlEmpresa" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlEmpresa_SelectedIndexChanged"  Width="300px" CssClass="btn btn-sm dropdown-toggle" BackColor="White">
					</asp:DropDownList>
					<asp:DropDownList ID="ddlAutorizanteFiltro" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlAutorizanteFiltro_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White">
					</asp:DropDownList>
					<asp:DropDownList ID="ddlTipoFiltro" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlTipoFiltro_SelectedIndexChanged" CssClass="btn btn-sm dropdown-toggle" BackColor="White">
					</asp:DropDownList>
					<asp:TextBox ID="txtMesAprobacionFiltro" runat="server" CssClass="form-control form-control-uniform" TextMode="Date" />
					<asp:Button ID="btnFiltrarMes" runat="server" CssClass="btn btn-sm btn-outline-dark ms-3" Text="Filtrar por Mes" OnClick="btnFiltrarMes_Click" />


					<br />
					<br />
				</div>

				<asp:GridView ID="dgvCertificado" DataKeyNames="ID" CssClass="table1  table-bordered table-hover "
					OnSelectedIndexChanged="dgvCertificado_SelectedIndexChanged"
					OnRowDeleting="dgvCertificado_RowDeleting"
					AutoGenerateColumns="false" runat="server" Style="display: block; overflow-x: auto;">
					<Columns>
							<asp:BoundField HeaderText="ID" DataField="Autorizante.Id" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" Visible="false" />

						<asp:BoundField HeaderText="Obra" DataField="Autorizante.Obra.Descripcion" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Contrata" DataField="Autorizante.Obra.Contrata.Nombre" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Empresa" DataField="Empresa" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Código Autorizante" DataField="Autorizante.CodigoAutorizante" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Expediente" DataField="ExpedientePago" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Tipo" DataField="Tipo.Nombre" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Monto Autorizado" DataField="MontoTotal" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Mes Aprobacion" DataField="MesAprobacion" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Porcentaje" DataField="Porcentaje" DataFormatString="{0:N2}%" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Sigaf" DataField="Sigaf" DataFormatString="{0:C}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />

						<asp:CommandField ShowSelectButton="true" SelectText="Modificar" ControlStyle-CssClass="btn btn-outline-warning" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
						<asp:CommandField ShowDeleteButton="true" ControlStyle-CssClass="btn btn-outline-danger" HeaderStyle-ForeColor="White" HeaderStyle-BackColor="#f1c40f" HeaderStyle-HorizontalAlign="Center" />
					</Columns>
				</asp:GridView>

				<div class="text-center p-4">
					<asp:Label ID="lblMensaje" Text="" runat="server" />
				</div>
			</div>
		</div>
	</div>

	<script type="text/javascript">
		function soloNumeros(e) {
			var tecla = (document.all) ? e.keyCode : e.which;
			if (tecla == 8 || tecla == 46) {
				return true;
			}
			var patron = /^[0-9]$/;
			var te = String.fromCharCode(tecla);
			return patron.test(te);
		}
	</script>
	<style>
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
			background: #f1c40f;
			color: white;
			border: 1px solid #f39c11;
		}

		.form-control-uniform {
			display: inline-block;
			width: 135px;
			height: 33px; /* Altura similar a los DropDownList */
			font-size: 14px; /* Tamaño de texto uniforme */
			padding: 6px 12px;
			margin-top: 5px;
		}

		.btn {
			margin-top: -4px;
		}
	</style>

</asp:Content>
