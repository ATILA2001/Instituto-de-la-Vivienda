<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PaginationControl.ascx.cs" Inherits="WebForms.CustomControls.PaginationControl" %>

<div class="d-flex justify-content-between align-items-center p-3">
    <!-- Controles de paginación a la izquierda -->
    <div class="d-flex align-items-center gap-1">
        <!-- Primera página -->
        <asp:LinkButton ID="lnkFirst" runat="server" OnClick="lnkFirst_Click"
            CssClass="btn btn-sm btn-outline-primary" ToolTip="Primera página">
            <i class="bi bi-chevron-double-left"></i>
        </asp:LinkButton>

        <!-- Página anterior -->
        <asp:LinkButton ID="lnkPrev" runat="server" OnClick="lnkPrev_Click"
            CssClass="btn btn-sm btn-outline-primary" ToolTip="Página anterior">
            <i class="bi bi-chevron-left"></i>
        </asp:LinkButton>

        <!-- Botones de páginas numeradas -->
        <asp:LinkButton ID="lnkPage1" runat="server" OnClick="lnkPage_Click" CommandArgument="0" 
            CssClass="btn btn-sm btn-outline-primary mx-1" Text="1" ToolTip="Ir a página 1" />
        <asp:LinkButton ID="lnkPage2" runat="server" OnClick="lnkPage_Click" CommandArgument="1" 
            CssClass="btn btn-sm btn-outline-primary mx-1" Text="2" ToolTip="Ir a página 2" />
        <asp:LinkButton ID="lnkPage3" runat="server" OnClick="lnkPage_Click" CommandArgument="2" 
            CssClass="btn btn-sm btn-outline-primary mx-1" Text="3" ToolTip="Ir a página 3" />
        <asp:LinkButton ID="lnkPage4" runat="server" OnClick="lnkPage_Click" CommandArgument="3" 
            CssClass="btn btn-sm btn-outline-primary mx-1" Text="4" ToolTip="Ir a página 4" />
        <asp:LinkButton ID="lnkPage5" runat="server" OnClick="lnkPage_Click" CommandArgument="4" 
            CssClass="btn btn-sm btn-outline-primary mx-1" Text="5" ToolTip="Ir a página 5" />

        <!-- Página siguiente -->
        <asp:LinkButton ID="lnkNext" runat="server" OnClick="lnkNext_Click"
            CssClass="btn btn-sm btn-outline-primary" ToolTip="Página siguiente">
            <i class="bi bi-chevron-right"></i>
        </asp:LinkButton>

        <!-- Última página -->
        <asp:LinkButton ID="lnkLast" runat="server" OnClick="lnkLast_Click"
            CssClass="btn btn-sm btn-outline-primary" ToolTip="Última página">
            <i class="bi bi-chevron-double-right"></i>
        </asp:LinkButton>

        <!-- Info de página -->
        <span class="mx-2 small text-muted">
            <asp:Label ID="lblPaginaInfo" runat="server" Text="Página 1 de 1" />
        </span>
    </div>

    <!-- Centro: Subtotal -->
    <div class="text-center">
        <asp:Label ID="lblSubtotalPaginacion" runat="server" Text="Total: $0.00 (0 registros)" CssClass="badge text-dark fs-6" />
    </div>

    <!-- Dropdown de registros por página a la derecha -->
    <div class="d-flex align-items-center gap-2">
        <label class="form-label mb-0 small">Registros por página:</label>
        <asp:DropDownList ID="ddlPageSizeExternal" runat="server" CssClass="form-select form-select-sm" 
            AutoPostBack="true" OnSelectedIndexChanged="ddlPageSizeExternal_SelectedIndexChanged" Style="width: auto;">
            <asp:ListItem Value="12" Text="12" Selected="true"></asp:ListItem>
            <asp:ListItem Value="24" Text="24"></asp:ListItem>
            <asp:ListItem Value="48" Text="48"></asp:ListItem>
            <asp:ListItem Value="96" Text="96"></asp:ListItem>
            <asp:ListItem Value="192" Text="192"></asp:ListItem>
        </asp:DropDownList>
    </div>
</div>
