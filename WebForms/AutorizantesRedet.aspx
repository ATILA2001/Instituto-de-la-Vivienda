<%@ Page Title="" Language="C#" MasterPageFile="~/Redet.Master" AutoEventWireup="true" CodeBehind="AutorizantesRedet.aspx.cs" Inherits="WebForms.AutorizantesRedet" %>
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
                      <label class="form-label ms-2 mb-0" for="cblObra">Obra:</label>
                      <div>
                          <CustomControls:CheckBoxListSearch ID="cblObra" runat="server" />
                      </div>
                  </div>

                  <div class="form-group mb-2">
                      <label class="form-label ms-2 mb-0" for="cblEmpresa">Empresa:</label>
                      <div>
                          <CustomControls:CheckBoxListSearch ID="cblEmpresa" runat="server" />
                      </div>
                  </div>

                  <div class="form-group mb-2">
                      <label class="form-label ms-2 mb-0" for="cblConcepto">Concepto:</label>
                      <div>
                          <CustomControls:CheckBoxListSearch ID="cblConcepto" runat="server" />
                      </div>
                  </div>

                  <div class="form-group mb-2">
                      <label class="form-label ms-2 mb-0" for="cblEstado">Estado:</label>
                      <div>

                          <CustomControls:CheckBoxListSearch ID="cblEstado" runat="server" />
                      </div>
                  </div>

                  <div class="form-group mb-2">
                      <label class="form-label ms-2 mb-0" for="txtSubtotal">Subtotal:</label>
                      <asp:TextBox ID="txtSubtotal" runat="server" CssClass="form-control form-control-uniform" ReadOnly="true" />
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

			<asp:GridView ID="dgvAutorizante" DataKeyNames="CodigoAutorizante" CssClass="table1  table-bordered table-hover mb-4 "
				AutoGenerateColumns="false" runat="server" >
				<Columns>
<%--					<asp:BoundField HeaderText="Obra" DataField="Obra.Id" />--%>

					<asp:BoundField HeaderText="Área" DataField="Obra.Area.Nombre" />

					<asp:BoundField HeaderText="Obra" DataField="Obra.Descripcion" />
					<asp:BoundField HeaderText="Contrata" DataField="Obra.Contrata.Nombre" />
					<asp:BoundField HeaderText="Empresa" DataField="Empresa" />

					<asp:BoundField HeaderText="Código Autorizante" DataField="CodigoAutorizante" />
					<asp:BoundField HeaderText="Concepto" DataField="Concepto.Nombre" />
					<asp:BoundField HeaderText="Detalle" DataField="Detalle" />

					<asp:BoundField HeaderText="Expediente" DataField="Expediente" />

					<asp:BoundField HeaderText="Estado" DataField="Estado" />

					<asp:BoundField HeaderText="Monto Autorizado" DataField="MontoAutorizado" DataFormatString="{0:C}" />
					<asp:BoundField HeaderText="Mes Aprobacion" DataField="Fecha" DataFormatString="{0:dd-MM-yyyy}" />

					<asp:BoundField HeaderText="Buzon sade" DataField="BuzonSade" />
					<asp:BoundField HeaderText="Fecha sade" DataField="FechaSade" DataFormatString="{0:dd-MM-yyyy}" />
				</Columns>
			</asp:GridView>


			<div class="text-center p-4">
				<asp:Label ID="lblMensaje" Text="" runat="server" />
			</div>
		</div>


</asp:Content>
