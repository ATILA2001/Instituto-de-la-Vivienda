<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CheckBoxListSearch.ascx.cs" Inherits="WebForms.CustomControls.CheckBoxListSearch" %>

<!-- <link rel="stylesheet" href="<%= ResolveUrl("~/Content/bootstrap.min.css") %>" /> -->
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bootstrap-icons/1.8.1/font/bootstrap-icons.min.css">
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Rounded:opsz,wght,FILL,GRAD@48,400,0,0" />
<link rel="stylesheet" href="<%= ResolveUrl("TreeViewSearch.css") %>" />

<script src="<%= ResolveUrl("TreeViewSearch.js") %>"></script>

<div class="dropdown">
    <button type="button" id="dropdownButton" runat="server" class="dropdown-button">
        <span id="litTitle" runat="server"></span>

    </button>

    <div id="<%= chkList.ClientID %>_dropdown" class="dropdown-content">
        <div class="search-container">

            <div class="search-row">
                <input type="text" id="<%= chkList.ClientID %>_txtSearch"
                    placeholder="Buscar..."
                    onkeyup='filterCheckboxes("<%= chkList.ClientID %>", "<%= chkList.ClientID %>_txtSearch")'
                    class="form-control" />
<%--                <asp:Button ID="btnDeselectAll" runat="server" CssClass="material-symbols-rounded btn btn-primary fs-5"
                    OnClick="BtnDeselectAll_Click"
                    OnClientClick="sessionStorage.setItem('shouldCloseDropdown', 'true'); return true;"
                    ToolTip="Deseleccionar todo"
                    Text="filter_alt_off"></asp:Button>--%>
            </div>
        </div>

        <div class="checkbox-container">
            <asp:TreeView ID="chkList" runat="server"
                ShowCheckBoxes="All"
                ShowLines="false"
                CssClass="date-tree-view"
                CollapseImageUrl="~/CustomControls/chevron-down.svg"
                ExpandImageUrl="~/CustomControls/chevron-right.svg">
            </asp:TreeView>

        </div>



        <div class="action-buttons d-flex justify-content-end align-items-center">


            <asp:linkbutton runat="server" id="chkList_btnDeselectAll"
                OnClick="BtnDeselectAll_Click"
                OnClientClick="sessionStorage.setItem('shouldCloseDropdown', 'true'); return true;"
                Visible="false"
                cssclass="btn btn-primary fw-bold"
                data-bs-toggle="tooltip"
                data-bs-title="Deseleccionar todo">
                <i class="material-symbols-rounded">filter_alt_off</i>
            </asp:linkbutton>




            <button type="button" runat="server" id="chkList_btnAccept"
                class="btn btn-success fw-bold"
                onserverclick="BtnAccept_Click"
                data-bs-toggle="tooltip"
                data-bs-title="Aceptar">
                <i class="bi bi-check-lg"></i>
            </button>




            <%--                <button type="button" runat="server" id="chkList_btnCancel"
                    class="btn btn-danger fw-bold"
                    onclick="cancelSelection('<%= chkList.ClientID %>', '<%= chkList.ClientID %>_dropdown', '<%= litTitle.ClientID %>'); return false;"
                    data-bs-toggle="tooltip"
                    data-bs-title="Cancelar">
                    <i class="bi bi-x-lg"></i>
                </button>--%>
        </div>



    </div>
</div>
