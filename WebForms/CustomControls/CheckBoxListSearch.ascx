<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CheckBoxListSearch.ascx.cs" Inherits="WebForms.CustomControls.CheckBoxListSearch" %>

<link rel="stylesheet" href="<%= ResolveUrl("~/Content/bootstrap.min.css") %>" />
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bootstrap-icons/1.8.1/font/bootstrap-icons.min.css">
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Rounded:opsz,wght,FILL,GRAD@24,700,1,200"/>

<div class="dropdown">
    <button type="button" class="dropdown-button" onclick="toggleDropdown('<%= chkList.ClientID %>_dropdown')">
        <asp:Literal ID="litTitle" runat="server" Text="Todos ▼" />
    </button>

    <div id="<%= chkList.ClientID %>_dropdown" class="dropdown-content">
        <div class="search-container">
            <div class="search-row">
                <input type="text" id="<%= chkList.ClientID %>_txtSearch" placeholder="Buscar..." onkeyup="filterCheckboxes('<%= chkList.ClientID %>')" class="form-control" />
                <asp:Button ID="btnDeselectAll" runat="server" CssClass="material-symbols-rounded btn btn-secondary"
                    OnClick="BtnDeselectAll_Click"
                    OnClientClick="sessionStorage.setItem('shouldCloseDropdown', 'true'); return true;"
                    ToolTip="Deseleccionar todo"
                    Text="filter_alt_off">
            
                </asp:Button>
            </div>
        </div>

        <div class="checkbox-container">
            <asp:CheckBoxList ID="chkList" runat="server" CssClass="checkbox-list" AutoPostBack="true" OnSelectedIndexChanged="ChkList_SelectedIndexChanged"></asp:CheckBoxList>
        </div>
    </div>
</div>

<script>
    document.addEventListener("click", function (event) {
        var dropdowns = document.querySelectorAll(".dropdown-content");
        var buttons = document.querySelectorAll(".dropdown-button");

        dropdowns.forEach((dropdown, index) => {
            var button = buttons[index]; // Obtener el botón correspondiente

            if (!dropdown.contains(event.target) && !button.contains(event.target)) {
                dropdown.style.display = "none";
            }
        });
    });


    document.addEventListener("DOMContentLoaded", function () {
        // Verificamos si venimos de un deselectAll
        if (sessionStorage.getItem('shouldCloseDropdown') === 'true') {
            document.querySelectorAll(".checkbox-list").forEach(list => {
                var dropdown = document.getElementById(list.id + '_dropdown');
                if (dropdown) {
                    dropdown.style.display = 'none';
                    // Importante: limpiar cualquier script del servidor que intente abrir el dropdown
                    clearTimeout(window['dropdownTimeout_' + list.id]);
                }
            });
            sessionStorage.removeItem('shouldCloseDropdown');
        }

        // Actualizamos los títulos
        document.querySelectorAll(".checkbox-list").forEach(list => {
            updateDropdownTitle(list.id);
        });
    });

    function clearDropdownTimeout(listId) {
        if (window['dropdownTimeout_' + listId]) {
            clearTimeout(window['dropdownTimeout_' + listId]);
            window['dropdownTimeout_' + listId] = null;
        }
    }





    document.querySelectorAll(".checkbox-list input[type='checkbox']").forEach(cb => {
        cb.addEventListener('change', function () {
            updateDropdownTitle(cb.closest('.checkbox-list').id);
        });
    });

    function toggleDropdown(dropdownId) {
        var dropdown = document.getElementById(dropdownId);
        var isVisible = dropdown.style.display === 'block';
        dropdown.style.display = isVisible ? 'none' : 'block';

        if (!isVisible) {
            var listId = dropdownId.replace('_dropdown', '');
            var searchInput = document.getElementById(listId + "_txtSearch");
            if (searchInput) {
                searchInput.focus();
            }
        }
    }

    function filterCheckboxes(listId) {
        var input = document.getElementById(listId + "_txtSearch");
        var filter = input.value.toLowerCase();
        var checkboxes = document.querySelectorAll("#" + listId + " input[type='checkbox']");

        checkboxes.forEach(cb => {
            var label = cb.nextSibling.textContent.toLowerCase();
            cb.parentElement.style.display = label.includes(filter) ? "flex" : "none";
        });
    }

</script>

<style>
    .search-row {
        display: flex;
        align-items: center;
        gap: 5px;
        width: 100%;
    }

        .search-row input[type='text'] {
            flex: 1;
        }

    /* Contenedor del dropdown */
    .dropdown {
        position: relative;
        display: inline-block;
    }

    /* Botón de apertura */
    .dropdown-button {
        background-color: transparent;
        border: none;
        border-radius: 5px;
        padding: 5px 10px;
        cursor: pointer;
    }

        .dropdown-button:hover {
            background-color: #e2e6ea;
        }

    /* Contenedor fijo para el input */
    .search-container {
        padding: 5px;
        background: white;
        position: sticky;
        top: 0;
        z-index: 10;
        border-bottom: 1px solid #ccc;
    }

    .material-symbols-rounded.btn {
        height: 2.5em;
        width: 3em;
        padding: 0;
    }


    /* Contenido desplegable */
    .dropdown-content {
        position: absolute;
        background-color: #fff;
        border: 1px solid #ccc;
        width: auto;
        min-width: 250px;
        max-width: none;
        max-height: 500px;
        overflow-y: auto;
        box-shadow: 0px 4px 6px rgba(0,0,0,0.1);
        z-index: 1000;
        padding: 0px 5px;
        display: none;
        text-align: left;
    }


        /* Cuadro de búsqueda */
        .dropdown-content input[type='text'] {
            width: 100%;
            padding: 5px;
            box-sizing: border-box;
            margin: 5px 0px;
            position: sticky;
            top: 0;
            background: white;
            z-index: 10;
            border: 1px, solid black;
            border-radius: 5px;
        }

    /* Contenedor con scroll solo para los checkboxes */
    .checkbox-container {
        flex-grow: 1;
        overflow-y: auto;
        white-space: nowrap;
    }

    .checkbox-list {
        margin: 0;
        padding: 0;
    }

        .checkbox-list span {
            display: block; /* Cada ítem en su propia línea */
            white-space: nowrap;
            margin: 3px 0;
        }

        /* Alineamos el checkbox y su texto en la misma fila */
        .checkbox-list input[type='checkbox'] {
            vertical-align: middle;
            margin-right: 5px;
        }


        .checkbox-list label {
            vertical-align: middle;
            white-space: nowrap; /* Evita que el texto salte de línea */
        }
</style>
