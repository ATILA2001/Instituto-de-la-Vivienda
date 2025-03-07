<%@ Page Title="" Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="AbmlEmpresa.aspx.cs" Inherits="WebForms.AbmlEmpresa" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
     <style>
        .section-divider {
            margin: 2rem 0;
            border-top: 2px solid #ccc;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid mt-4">
        <div class="row">
            <div class="col-12 mb-4">
                <div class="card shadow-sm border-0" >
                    <div class="card-body d-flex flex-wrap justify-content-between gap-3">
                        <div class="form-group" style="max-width: 300px; flex-grow: 1;">
                            <label class="form-label" for="txtBuscar" >Buscar:</label>
                            <asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control form-control-uniform"  />
                        </div>
                        <div class="form-group d-flex align-items-end">
                            <asp:Button CssClass="btn" ID="btnFiltrar" Text="Filtrar" runat="server" OnClick="btnFiltrar_Click" 
                                         />
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-md-8 mb-4">
                  <div class="card-body">
                        <asp:GridView ID="dgvEmpresa" DataKeyNames="ID" CssClass="table1 table-hover" 
                                      OnSelectedIndexChanged="dgvEmpresa_SelectedIndexChanged" 
                                      OnRowDeleting="dgvEmpresa_RowDeleting"
                                      AutoGenerateColumns="false" runat="server">
                            <Columns>
                                <asp:BoundField HeaderText="ID" DataField="Id" Visible="false" />
                                <asp:BoundField HeaderText="Empresa" DataField="Nombre"  />
                                <asp:CommandField ShowSelectButton="true" SelectText="Modificar" 
                                                  ControlStyle-CssClass="btn btn-outline-warning" 
                                                  />
                                <asp:CommandField ShowDeleteButton="true" 
                                                  ControlStyle-CssClass="btn btn-outline-danger"  
                                                  />
                            </Columns>
                        </asp:GridView>
                    </div>
                
            </div>

            <div class="col-md-4">
                <div class="card shadow-sm border-0" style="border-radius: 12px;">
                    <div class="card-header text-center" style="background-color: #153244; color: #ecf0f1;  border-radius: 12px 12px 0 0;">
                        Agregar Empresa
                    </div>
                    <div class="card-body">
                        <div class="mb-2">
                            <label for="txtNombre" class="form-label" style="color: #153244; ">Nombre</label>
                            <asp:TextBox ID="txtNombre" CssClass="form-control" runat="server" 
                                         />
                        </div>
                        <div class="mb-2 text-center p-2">
                            <asp:Button Text="Agregar" ID="btnAgregar" OnClick="btnAgregar_Click" 
                                        CssClass="btn" runat="server" 
                                        />
                        </div>
                    </div>
                    <div class="text-center p-4">
                        <asp:Label ID="lblMensaje" Text="" runat="server"  />
                    </div>
                </div>
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
