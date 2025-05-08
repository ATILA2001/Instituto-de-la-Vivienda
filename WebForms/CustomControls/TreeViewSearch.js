/**
 * Cierra otros dropdowns abiertos y el actual si se hace clic fuera.
 */
document.addEventListener("click", function (event) {
    const dropdowns = document.querySelectorAll(".dropdown-content");
    const buttons = document.querySelectorAll(".dropdown-button");

    dropdowns.forEach((dropdown, index) => {
        const button = buttons[index];
        if (dropdown.style.display === 'block' && !dropdown.contains(event.target) && !button.contains(event.target)) {
            dropdown.style.display = "none";
        }
    });
});

/**
 * Inicializa componentes, gestiona estado de dropdowns y añade listeners al cargar.
 */
document.addEventListener("DOMContentLoaded", function () {
    // Inicializa los tooltips de Bootstrap.
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

    if (sessionStorage.getItem('shouldCloseDropdown') === 'true') {
        document.querySelectorAll(".date-tree-view").forEach(treeViewContainer => {
            const dropdown = document.getElementById(treeViewContainer.id + '_dropdown');
            if (dropdown) {
                dropdown.style.display = 'none';
                clearDropdownTimeout(treeViewContainer.id);
            }
        });
        sessionStorage.removeItem('shouldCloseDropdown');
    }

    // Itera sobre cada TreeView y configura sus listeners y estado inicial.
    document.querySelectorAll(".date-tree-view").forEach(treeViewContainer => {
        // Guarda el estado inicial para 'Cancelar'
        saveInitialState(treeViewContainer);

        // Reconstruye estados indeterminate después del postback
        // 1. Obtener todos los checkboxes
        const allCheckboxes = treeViewContainer.querySelectorAll('input[type="checkbox"]');
        // 2. Iterar sobre ellos para encontrar las hojas y iniciar la actualización de padres
        allCheckboxes.forEach(cb => {
            const parentTable = cb.closest('table');
            if (!parentTable) return;

            // Identificar si es un nodo hoja (no tiene un div '[id]Nodes' asociado)
            const nodeId = parentTable.querySelector('a[id*="chkListn"]')?.id;
            const childNodesDiv = nodeId ? document.getElementById(nodeId + 'Nodes') : null;
            const isLeafNode = !childNodesDiv;
            const isSelectAll = cb === treeViewContainer.querySelector(':scope > table input[type="checkbox"]'); // Evitar iniciar desde "Select All"

            // Si es un nodo hoja, llamar a updateParentState para recalcular sus ancestros
            if (isLeafNode && !isSelectAll) {
                // Iniciar la actualización desde este nodo hoja hacia arriba
                updateParentState(cb, treeViewContainer);
            }
        });
        // 3. Asegurarse de que el estado "Seleccionar todos" sea correcto después de actualizar los nodos intermedios.
        //    Aunque updateParentState debería llegar hasta la raíz, una llamada explícita aquí garantiza el estado correcto.
        updateSelectAllState(treeViewContainer);
        


        // Configura el listener delegado para clics dentro del TreeView
        treeViewContainer.addEventListener('click', function(event) {
            const target = event.target;
            let checkbox = null;
            let label = null;

            // Intenta encontrar el checkbox y la etiqueta asociados al clic
            // Esto asume la estructura renderizada por asp:TreeView donde el checkbox
            // y la etiqueta (<a>) están dentro del mismo contenedor de nodo (ej: <td> o <div>)
            const nodeElement = target.closest('table tr > td') || target.closest('div > div'); // Ajusta este selector si la estructura es diferente

            if (nodeElement) {
                checkbox = nodeElement.querySelector('input[type="checkbox"]');
                // La etiqueta suele ser un enlace <a> en TreeView
                label = nodeElement.querySelector('a');
            }

            // Si encontramos un checkbox válido y el clic NO fue directamente en él,
            // pero sí en su etiqueta o en el contenedor del nodo.
            if (checkbox && target !== checkbox && (target === label || nodeElement.contains(target))) {
                event.preventDefault(); // Previene la acción por defecto del clic (ej: navegación del <a>)
                checkbox.checked = !checkbox.checked; // Cambia el estado del checkbox manualmente

                // Dispara manualmente el evento 'change' para que se ejecute la lógica de actualización
                const changeEvent = new Event('change', { bubbles: true });
                checkbox.dispatchEvent(changeEvent);
            }
            // Si el clic fue directamente en el checkbox, no hacemos nada aquí,
            // dejaremos que el listener 'change' (definido abajo) maneje la lógica.
        });

        // Listener para el evento 'change' de los checkboxes (se dispara cuando cambia el estado)
        // Este manejará la lógica de actualización de padres/hijos y título.
        treeViewContainer.addEventListener('change', function(event) {
            const target = event.target;
            if (target.tagName === 'INPUT' && target.type === 'checkbox') {
                // Llama a la función que maneja la lógica de actualización de jerarquía y título
                handleTreeViewCheckboxChange(target, treeViewContainer);
            }
        });

        // Actualiza el título inicial basado en el estado cargado
        updateDropdownTitle(treeViewContainer);

        // Configura el listener para el campo de búsqueda (si existe)
        const searchInputId = treeViewContainer.id + '_txtSearch';
        const searchInput = document.getElementById(searchInputId);
        if (searchInput) {
            searchInput.addEventListener('keyup', () => filterCheckboxes(treeViewContainer.id, searchInputId));
        }
    });
});

/**
 * Limpia los timeouts asociados a un dropdown específico.
 * @param {string} listId - ID del elemento lista asociado al dropdown.
 */
function clearDropdownTimeout(listId) {
    const timeoutId = window['dropdownTimeout_' + listId];
    if (timeoutId) {
        clearTimeout(timeoutId);
        window['dropdownTimeout_' + listId] = null;
    }
}

/**
 * Alterna la visibilidad del dropdown y enfoca el campo de búsqueda.
 * Restaura el estado definitivo y guarda el estado actual como temporal para 'Cancelar'.
 * @param {string} dropdownId - ID del elemento dropdown (ej: MainContent_cblFecha_chkList_dropdown).
 */
function toggleDropdown(dropdownId) {
    const dropdown = document.getElementById(dropdownId);
    if (!dropdown) {
        return;
    }

    const isVisible = dropdown.style.display === 'block';

    const treeId = dropdownId.replace('_dropdown', '');
    const searchInputId = treeId + '_txtSearch';
    const titleElementId = treeId.substring(0, treeId.lastIndexOf('_chkList')) + '_litTitle';

    if (!isVisible) {
        document.querySelectorAll(".dropdown-content").forEach(otherDropdown => {
            if (otherDropdown.id !== dropdownId && otherDropdown.style.display === 'block') {
                otherDropdown.style.display = 'none';
            }
        });
    }

    dropdown.style.display = isVisible ? 'none' : 'block';

    if (!isVisible) {
        const searchInput = document.getElementById(searchInputId);
        if (searchInput) {
            setTimeout(() => {
                searchInput.focus();
            }, 0);
        }
    }
}

/**
 * Normaliza un texto: convierte a minúsculas y elimina diacríticos (acentos).
 * @param {string} text - El texto a normalizar.
 * @returns {string} El texto normalizado.
 */
function normalizeText(text) {
    if (!text) return '';
    return text
        .toLowerCase()
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, '');
}

/**
 * Filtra los nodos del TreeView según el texto de búsqueda (ignorando mayúsculas/minúsculas y acentos).
 * @param {string} treeId - ID del TreeView (contenedor .date-tree-view).
 * @param {string} searchInputId - ID del campo de búsqueda.
 */
function filterCheckboxes(treeId, searchInputId) {
    const input = document.getElementById(searchInputId);
    const treeViewContainer = document.getElementById(treeId);

    if (!input) {
        return;
    }
    if (!treeViewContainer) {
        return;
    }

    const filter = normalizeText(input.value.trim());

    const nodeTables = treeViewContainer.querySelectorAll('table');

    nodeTables.forEach(table => {
        const checkbox = table.querySelector('td input[type="checkbox"]');
        if (checkbox && checkbox.value !== 'select-all') {
            table.style.display = 'none';
            const nodeId = table.querySelector('a[id*="chkListn"]')?.id;
            const childNodesDiv = nodeId ? document.getElementById(nodeId + 'Nodes') : null;
            if (childNodesDiv) {
                childNodesDiv.style.display = 'none';
            }
        } else if (checkbox && checkbox.value === 'select-all') {
            table.style.display = '';
        }
    });

    nodeTables.forEach((table) => {
        const checkbox = table.querySelector('td input[type="checkbox"]');
        if (!checkbox || checkbox.value === 'select-all') {
            return;
        }

        const textCell = table.querySelector('tr td:last-child');
        const originalNodeText = textCell ? textCell.textContent.trim() : '';

        const nodeText = normalizeText(originalNodeText);

        const isVisible = nodeText.includes(filter);

        if (isVisible) {
            table.style.display = "";

            let currentTable = table;
            while (currentTable) {
                const containingDiv = currentTable.parentElement;
                if (!containingDiv || containingDiv === treeViewContainer || !containingDiv.id.endsWith('Nodes')) {
                    break;
                }

                const parentTable = containingDiv.previousElementSibling;
                if (parentTable && parentTable.tagName === 'TABLE') {
                    if (parentTable.style.display === 'none') {
                        parentTable.style.display = "";
                    }

                    const parentLink = parentTable.querySelector('a[id*="chkListn"]');
                    if (parentLink) {
                        const childNodesDivId = parentLink.id + 'Nodes';
                        const childNodesDiv = document.getElementById(childNodesDivId);
                        if (childNodesDiv && childNodesDiv.style.display === 'none') {
                            childNodesDiv.style.display = '';
                        }
                    }

                    currentTable = parentTable;
                } else {
                    break;
                }
            }
        }
    });
}

/**
 * Maneja el cambio de estado de cualquier checkbox en el TreeView.
 * Esta función ahora es llamada por el listener 'change'.
 * @param {HTMLInputElement} checkbox - El checkbox que cambió.
 * @param {HTMLElement} treeViewContainer - El contenedor principal del TreeView.
 */
function handleTreeViewCheckboxChange(checkbox, treeViewContainer) {
    // Determina si es el checkbox "Seleccionar todos" o un nodo hijo
    // La identificación de "Seleccionar todos" puede necesitar ajustarse según cómo se renderice.
    // Asumiremos que el primer checkbox dentro del contenedor es "Seleccionar todos" si existe.
    const isSelectAll = checkbox === treeViewContainer.querySelector('input[type="checkbox"]'); // Ajustar si es necesario

    if (isSelectAll) {
        handleSelectAllChange(checkbox, treeViewContainer);
    } else {
        handleChildCheckboxChange(checkbox, treeViewContainer);
    }
    // Actualiza el título después de cualquier cambio
    updateDropdownTitle(treeViewContainer);
}

/**
 * Gestiona la selección/deselección de todos los checkboxes cuando cambia "Seleccionar todos".
 * @param {HTMLInputElement} selectAllCheckbox - Checkbox "Seleccionar todos".
 * @param {HTMLElement} treeViewContainer - El contenedor principal del TreeView.
 */
function handleSelectAllChange(selectAllCheckbox, treeViewContainer) {
    const allCheckboxes = treeViewContainer.querySelectorAll('input[type="checkbox"]');
    const isChecked = selectAllCheckbox.checked;

    allCheckboxes.forEach(cb => {
        if (cb !== selectAllCheckbox) {
            if (cb.checked !== isChecked) {
                cb.checked = isChecked;
            }
            if (cb.indeterminate) {
                cb.indeterminate = false;
            }
        }
    });

    if (selectAllCheckbox.indeterminate) {
        selectAllCheckbox.indeterminate = false;
    }
}

/**
 * Maneja el cambio en un checkbox hijo (año, mes o día).
 * Actualiza el estado 'checked' de todos los descendientes y luego actualiza los ancestros.
 * @param {HTMLInputElement} checkbox - Checkbox que cambió (Año, Mes o Día).
 * @param {HTMLElement} treeViewContainer - El contenedor principal del TreeView.
 */
function handleChildCheckboxChange(checkbox, treeViewContainer) {
    const parentTable = checkbox.closest('table');
    if (!parentTable) return;

    const isChecked = checkbox.checked;
    const nodeId = parentTable.querySelector('a[id*="chkListn"]')?.id;

    const childNodesDiv = nodeId ? document.getElementById(nodeId + 'Nodes') : null;

    if (childNodesDiv) {
        const descendantCheckboxes = childNodesDiv.querySelectorAll('input[type="checkbox"]');

        descendantCheckboxes.forEach(descendantCb => {
            if (descendantCb.checked !== isChecked) {
                descendantCb.checked = isChecked;
            }
            if (descendantCb.indeterminate) {
                descendantCb.indeterminate = false;
            }
        });
    }

    updateParentState(checkbox, treeViewContainer);
}

/**
 * Actualiza el estado del nodo padre basado en el estado de sus hijos directos.
 * Establece el estado 'checked' e 'indeterminate'.
 * @param {HTMLInputElement} checkbox - Checkbox que inició la actualización.
 * @param {HTMLElement} treeViewContainer - El contenedor principal del TreeView.
 */
function updateParentState(checkbox, treeViewContainer) {
    const currentTable = checkbox.closest('table');
    if (!currentTable) {
        return;
    }

    const containingDiv = currentTable.parentElement;
    if (!containingDiv) {
        return;
    }

    if (containingDiv === treeViewContainer) {
        updateSelectAllState(treeViewContainer);
        return;
    }

    const parentTable = containingDiv.previousElementSibling;
    if (!parentTable || parentTable.tagName !== 'TABLE') {
        return;
    }

    const parentCheckbox = parentTable.querySelector('td input[type="checkbox"]');
    if (!parentCheckbox) {
        return;
    }

    const siblingCheckboxes = containingDiv.querySelectorAll(':scope > table td input[type="checkbox"]');

    const totalSiblings = siblingCheckboxes.length;
    let checkedSiblingsCount = 0;
    let indeterminateSiblingsCount = 0;
    if (totalSiblings > 0) {
        siblingCheckboxes.forEach(cb => {
            if (cb.checked) checkedSiblingsCount++;
            else if (cb.indeterminate) indeterminateSiblingsCount++;
        });
    }

    let newChecked = false;
    let newIndeterminate = false;

    if (indeterminateSiblingsCount > 0) { newIndeterminate = true; newChecked = false; }
    else if (checkedSiblingsCount === 0) { newChecked = false; newIndeterminate = false; }
    else if (checkedSiblingsCount === totalSiblings) { newChecked = true; newIndeterminate = false; }
    else { newChecked = false; newIndeterminate = true; }

    let stateChanged = false;
    if (parentCheckbox.checked !== newChecked) {
        parentCheckbox.checked = newChecked;
        stateChanged = true;
    }
    if (newChecked && parentCheckbox.indeterminate) {
         parentCheckbox.indeterminate = false;
    } else if (parentCheckbox.indeterminate !== newIndeterminate) {
        parentCheckbox.indeterminate = newIndeterminate;
        stateChanged = true;
    }

    if (stateChanged) {
        updateParentState(parentCheckbox, treeViewContainer);
    } else {
        const grandParentDiv = parentTable.parentElement;
        if (grandParentDiv === treeViewContainer) {
            updateSelectAllState(treeViewContainer);
        }
    }
}

/**
 * Actualiza el estado del checkbox "Seleccionar todos" basado en los nodos de año.
 * Incluye lógica para el estado 'indeterminate'.
 * @param {HTMLElement} treeViewContainer - El elemento contenedor del TreeView.
 */
function updateSelectAllState(treeViewContainer) {
    if (!treeViewContainer) return;

    const rootTable = treeViewContainer.querySelector(':scope > table');
    if (!rootTable) return;
    const selectAllCheckbox = rootTable.querySelector('td input[type="checkbox"]');
    if (!selectAllCheckbox) return;

    const yearNodesDiv = rootTable.nextElementSibling;
    if (!yearNodesDiv || !yearNodesDiv.id.endsWith('Nodes')) {
        if (selectAllCheckbox.checked || selectAllCheckbox.indeterminate) {
             selectAllCheckbox.checked = false;
             selectAllCheckbox.indeterminate = false;
        }
        return;
    }

    const yearCheckboxes = yearNodesDiv.querySelectorAll(':scope > table td input[type="checkbox"]');
    const totalYears = yearCheckboxes.length;
    let checkedYearsCount = 0;
    let indeterminateYearsCount = 0;

    if (totalYears > 0) {
        yearCheckboxes.forEach(yearCb => {
            if (yearCb.checked) {
                checkedYearsCount++;
            } else if (yearCb.indeterminate) {
                indeterminateYearsCount++;
            }
        });
    }

    let newChecked = false;
    let newIndeterminate = false;

    if (indeterminateYearsCount > 0) {
        newIndeterminate = true;
        newChecked = false;
    } else if (checkedYearsCount === 0) {
        newChecked = false;
        newIndeterminate = false;
    } else if (checkedYearsCount === totalYears) {
        newChecked = true;
        newIndeterminate = false;
    } else {
        newChecked = false;
        newIndeterminate = true;
    }

    if (selectAllCheckbox.checked !== newChecked) {
        selectAllCheckbox.checked = newChecked;
    }
    if (selectAllCheckbox.indeterminate !== newIndeterminate) {
        selectAllCheckbox.indeterminate = newIndeterminate;
    }
}

/**
 * Actualiza el título del dropdown mostrando la cantidad de nodos HOJA seleccionados.
 * @param {HTMLElement} containerElement - El elemento contenedor del TreeView (.date-tree-view).
 */
function updateDropdownTitle(containerElement) {
    // Verificar si el contenedor existe primero
    if (!containerElement) {
        console.error(`Error en updateDropdownTitle: Se recibió un containerElement nulo o inválido.`);
        return;
    }

    // --- Modificación: Búsqueda relativa del botón y el titleElement ---
    // 1. Encontrar el div.dropdown-content que contiene el containerElement
    const dropdownContent = containerElement.closest('.dropdown-content');
    // 2. Encontrar el div.dropdown padre
    const dropdownDiv = dropdownContent?.closest('.dropdown');
    // 3. Encontrar el botón dentro del div.dropdown
    const buttonElement = dropdownDiv?.querySelector('.dropdown-button');
    // 4. Encontrar el span (renderizado por asp:Literal) dentro del botón
    //    Asumimos que el Literal es el primer (o único) span dentro del botón.
    const titleElement = buttonElement?.querySelector('span');
    // --- Fin de la Modificación ---

    // Verificar si los elementos clave fueron encontrados
    if (!buttonElement || !titleElement) {
        console.error(`Error en updateDropdownTitle: No se encontró buttonElement o titleElement relativo a container (id=${containerElement.id}). Button:`, buttonElement, "Title:", titleElement);
        return;
    }

    // Obtener todos los checkboxes dentro del contenedor del TreeView.
    const allCheckboxes = containerElement.querySelectorAll(':scope input[type="checkbox"]');
    let selectedLeafCount = 0; // Inicializar contador para nodos hoja seleccionados.

    // Iterar sobre cada checkbox para contar los nodos hoja seleccionados.
    allCheckboxes.forEach(cb => {
        // Encontrar la tabla que contiene el checkbox.
        const parentTable = cb.closest('table');
        if (!parentTable) return; // Saltar si no está en una tabla (inesperado).

        // Identificar si es el checkbox "Seleccionar todos".
        const isSelectAllCheckbox = parentTable === containerElement.querySelector(':scope > table');

        // Identificar si es un nodo hoja.
        // Un nodo es hoja si el div '[id]Nodes' asociado NO existe.
        // Busca el enlace 'a' dentro de la tabla para obtener el ID base del nodo.
        const nodeId = parentTable.querySelector('a[id*="chkListn"]')?.id;
        // Busca el div de nodos hijos usando el ID del nodo + 'Nodes'.
        const childNodesDiv = nodeId ? document.getElementById(nodeId + 'Nodes') : null;
        // Es un nodo hoja si no se encontró el div de nodos hijos.
        const isLeafNode = !childNodesDiv;

        // Contar solo si es un nodo hoja, está marcado y no es "Seleccionar todos".
        if (!isSelectAllCheckbox && isLeafNode && cb.checked) {
            selectedLeafCount++;
        }
    });

    // Preparar el texto del título.
    let title = "";
    // Obtener el texto base del atributo data-default-text del botón, o usar "Todos" como fallback.
    const originalTitle = buttonElement.getAttribute('data-default-text') || "Todos";

    // Construir el HTML del título basado en el conteo.
    if (selectedLeafCount === 0) {
        // Si no hay seleccionados, usar el título original y el icono de flecha hacia abajo.
        title = `${originalTitle} <i class="bi bi-caret-down"></i>`;
    } else {
        // Si hay seleccionados, mostrar el conteo y el icono de flecha hacia abajo rellena.
        // Añadir 's' a "seleccionado" si el conteo es mayor que 1.
        title = `${selectedLeafCount} seleccionado${selectedLeafCount > 1 ? "s" : ""} <i class="bi bi-caret-down-fill"></i>`;
    }

    // Actualizar el contenido HTML del elemento Literal (dentro del botón).
    titleElement.innerHTML = title;
}

/**
 * Cancela la selección actual y restaura el estado guardado previamente.
 * @param {string} treeId - ID del TreeView.
 * @param {string} dropdownId - ID del dropdown.
 * @param {string} titleElementId - ID del elemento del título.
 */
function cancelSelection(treeId, dropdownId, titleElementId) {
    // --- Modificación: Obtener el elemento contenedor y pasarlo a updateDropdownTitle ---
    const containerElement = document.getElementById(treeId);
    if (containerElement) {
        updateDropdownTitle(containerElement);
    } else {
        console.error(`Error en cancelSelection: No se encontró el contenedor con id=${treeId}`);
    }

    // Cerrar el dropdown.
    const dropdown = document.getElementById(dropdownId);
    if (dropdown) {
        dropdown.style.display = 'none';
    }
}

/**
 * Guarda el estado inicial de los checkboxes en el TreeView.
 * @param {HTMLElement} treeViewContainer - El contenedor principal del TreeView.
 */
function saveInitialState(treeViewContainer) {
    const checkboxes = treeViewContainer.querySelectorAll('input[type="checkbox"]');
    const state = {};
    checkboxes.forEach(cb => {
        state[cb.id] = { checked: cb.checked, indeterminate: cb.indeterminate };
    });
    // Guarda el estado asociado al ID del contenedor
    sessionStorage.setItem(`initialState_${treeViewContainer.id}`, JSON.stringify(state));
}

/**
 * Restaura el estado inicial de los checkboxes en el TreeView.
 * @param {HTMLElement} treeViewContainer - El contenedor principal del TreeView.
 */
function restoreState(treeViewContainer) {
    const savedState = sessionStorage.getItem(`initialState_${treeViewContainer.id}`);
    if (savedState) {
        const state = JSON.parse(savedState);
        const checkboxes = treeViewContainer.querySelectorAll('input[type="checkbox"]');
        checkboxes.forEach(cb => {
            if (state[cb.id]) {
                cb.checked = state[cb.id].checked;
                cb.indeterminate = state[cb.id].indeterminate;
            }
        });
        // Vuelve a ejecutar la lógica de actualización visual/título después de restaurar
        updateDropdownTitle(treeViewContainer);
    }
}