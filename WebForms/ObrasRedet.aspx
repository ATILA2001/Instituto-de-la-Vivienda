<%@ Page Title="" Language="C#" MasterPageFile="~/Redet.Master" AutoEventWireup="true" CodeBehind="ObrasRedet.aspx.cs" Inherits="WebForms.ObrasRedet" %>
<%@ Register Src="~/CustomControls/CheckBoxListSearch.ascx" TagPrefix="CustomControls" TagName="CheckBoxListSearch" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
	<div class="row mt-4 mb-3">
		<div class="col-12">
			<div class="d-flex justify-content-between align-items-end flex-wrap gap-3">
				<!-- Contenedor de Filtros alineados a la izquierda -->

				<div class="d-flex flex-wrap gap-3">


					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblArea">Area:</label>
						<div>

							<CustomControls:CheckBoxListSearch ID="cblArea" runat="server" />
						</div>
					</div>

					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblEmpresa">Empresa:</label>
						<div>

							<CustomControls:CheckBoxListSearch ID="cblEmpresa" runat="server" />
						</div>
					</div>


					<div class="form-group mb-2">
						<label class="form-label ms-2 mb-0" for="cblBarrio">Barrio:</label>
						<div>

							<CustomControls:CheckBoxListSearch ID="cblBarrio" runat="server" />
						</div>
					</div>
				</div>

				<!-- Contenedor de Botones alineados a la derecha -->
				<div class="d-flex gap-3">

					<div class="form-group mb-2">
						<asp:TextBox ID="txtBuscar" runat="server" CssClass="form-control" placeholder="Buscar..."></asp:TextBox>
					</div>
					<div class="form-group mb-2">
						<%--<asp:Button CssClass="btn btn-primary" ID="btnFiltrar" Text="Filtrar" runat="server" OnClick="btnFiltrar_Click" />--%>
						<asp:LinkButton ID="btnFiltrar" runat="server" CssClass="btn btn-primary" OnClick="btnFiltrar_Click"
							data-bs-toggle="tooltip" data-bs-placement="top" title="Filtrar">
					<i class="bi bi-search"></i>
						</asp:LinkButton>
					</div>


					<%-- logica que aparezca o desaparezca. copiar de david --%>
					<div class="form-group mb-2">
						<asp:LinkButton CssClass="btn btn-primary" ID="btnLimpiarFiltros" Text="Limpiar" runat="server" OnClick="BtnClearFilters_Click">
					<i class="bi bi-funnel"></i>
						</asp:LinkButton>
					</div>


					

				</div>
			</div>
		</div>
		<hr class="mb-3" />


		<asp:GridView ID="dgvObra" DataKeyNames="ID" CssClass="table1  table-bordered table-hover mb-4"
			
			AutoGenerateColumns="false" runat="server">

			<Columns>
<%--				<asp:BoundField HeaderText="ID" DataField="Id" />--%>
				<asp:BoundField HeaderText="Área" DataField="Area" />
				<asp:BoundField HeaderText="Empresa" DataField="Empresa" />
				<asp:TemplateField HeaderText="Contrata">
					<ItemTemplate>
						<%# Eval("Contrata") + " " + Eval("Numero") + "/" + Eval("Año") %>
					</ItemTemplate>
				</asp:TemplateField>
				<asp:BoundField HeaderText="Barrio" DataField="Barrio" />
				<asp:BoundField HeaderText="Nombre de Obra" DataField="Descripcion" />
				<asp:BoundField HeaderText="Linea de Gestion" DataField="Linea" />
				<asp:BoundField HeaderText="Proyecto" DataField="Proyecto" />


				<asp:BoundField HeaderText="Fecha Inicio" DataField="FechaInicio" DataFormatString="{0:dd-MM-yyyy}" />
				<asp:BoundField HeaderText="Fecha Fin" DataField="FechaFin" DataFormatString="{0:dd-MM-yyyy}" />
				</Columns>
		</asp:GridView>

		<div class="text-center p-4">
			<asp:Label ID="lblMensaje" Text="" runat="server" />
		</div>
	</div>


</asp:Content>



