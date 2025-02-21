<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CheckBoxListSearch.ascx.cs" Inherits="WebForms.CustomControls.CheckBoxListSearch" %>

<div class="dropdown">
    <button type="button" class="dropdown-button" onclick="toggleDropdown('<%= chkList.ClientID %>_dropdown')">
        Seleccionar filtros ▼
    </button>
    
    <div id="<%= chkList.ClientID %>_dropdown" class="dropdown-content">
        <input type="text" id="<%= chkList.ClientID %>_txtSearch" placeholder="Buscar..." onkeyup="filterCheckboxes('<%= chkList.ClientID %>')"/>
        
        <div class="checkbox-container">
            <asp:CheckBoxList ID="chkList" runat="server" CssClass="checkbox-list"></asp:CheckBoxList>
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


    function toggleDropdown(dropdownId) {
        var dropdown = document.getElementById(dropdownId);
        dropdown.style.display = (dropdown.style.display === 'block') ? 'none' : 'block';
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
/* Contenedor del dropdown */
.dropdown {
    position: relative;
    display: inline-block;
}

/* Botón de apertura */
.dropdown-button {
    background-color: #f1f1f1;
    border: 1px solid #ccc;
    padding: 5px 10px;
    cursor: pointer;
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
    padding: 5px;
    display: none;
    text-align: left;
}


/* Cuadro de búsqueda */
.dropdown-content input[type='text'] {
    width: 100%;
    padding: 5px;
    box-sizing: border-box;
    margin-bottom: 5px;
    position: sticky;
    top: 0;
    background: white;
    z-index: 10;
}


.dropdown-content input[type='text'] {
    width: 100%;
    padding: 5px;
    box-sizing: border-box;
}

/* Contenedor con scroll solo para los checkboxes */
.checkbox-container {
    flex-grow: 1;
    overflow-y: auto;
/*    padding: 5px;*/
}

.checkbox-list {
    margin: 0;
    padding: 0;
}

.checkbox-list span {
    display: block;   /* Cada ítem en su propia línea */
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
    white-space: nowrap;     /* Evita que el texto salte de línea */
}

</style>
