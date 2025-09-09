/**
 * Cerrar dropdowns al hacer click fuera
 */
document.addEventListener('click', function (event) {
    const target = event.target;
    document.querySelectorAll('.dropdown-content').forEach(openDropdown => {
        if (window.getComputedStyle(openDropdown).display === 'none') return;
        const associatedButton = document.querySelector(`.dropdown-button[data-dropdown-id="${openDropdown.id}"]`);
        if (openDropdown.contains(target) || (associatedButton && associatedButton.contains(target))) return;
        openDropdown.style.display = 'none';
        removeDropdownDynamicHandlers(openDropdown);
        if (openDropdown.id && typeof clearDropdownTimeout === 'function') {
            clearDropdownTimeout(openDropdown.id);
        }
    });
}, false);

function getPageName() {
    try {
        const path = window.location.pathname;
        const pageName = path.split('/').pop().split('.')[0];
        return pageName && pageName.length > 0 ? pageName : 'Unknown';
    } catch (error) {
        console.warn('Error al obtener nombre de página:', error);
        return 'Unknown';
    }
}


/**
 * Inicializa componentes, gestiona estado de dropdowns y añade listeners al cargar.
 */
document.addEventListener("DOMContentLoaded", function () {
    // Inicializa los tooltips de Bootstrap.
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

    // Conectar botones con data-dropdown-id a la función toggleDropdown (respaldo si server no asignó onclick)
    document.querySelectorAll('.dropdown-button[data-dropdown-id]').forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            const dropdownId = btn.getAttribute('data-dropdown-id');
            if (dropdownId) toggleDropdown(dropdownId);
            return false;
        });
    });


    // Gestiona el cierre del dropdown si venimos de un postback que lo requiere.
    if (sessionStorage.getItem('shouldCloseDropdown') === 'true') {
        document.querySelectorAll(".date-tree-view").forEach(treeViewContainer => {
            if (treeViewContainer && treeViewContainer.id) {
                const dropdown = document.getElementById(treeViewContainer.id + '_dropdown');
                if (dropdown) {
                    dropdown.style.display = 'none';
                    if (typeof clearDropdownTimeout === 'function') {
                        clearDropdownTimeout(treeViewContainer.id);
                    }
                }
            }
        });
        sessionStorage.removeItem('shouldCloseDropdown');
    }

    // Manejo del flag filtersCleared
    const filtersWereClearedOnServer = sessionStorage.getItem('filtersCleared') === 'true';
    if (filtersWereClearedOnServer) {
        console.log("[DOMContentLoaded] Flag 'filtersCleared' detectado. Se procederá a reinicializar la UI de los TreeViews para reflejar el estado limpio del servidor.");
    }

    // Itera sobre cada TreeView y configura sus listeners y estado inicial.
    document.querySelectorAll(".date-tree-view").forEach(treeViewContainer => {
         if (filtersWereClearedOnServer) {
            clearLocalStatesForTreeView(treeViewContainer);
        }

        if (!treeViewContainer || !treeViewContainer.id) {
            return; // Saltar a la siguiente iteración
        }

        if (typeof initializeIndeterminateStatesForContainer === 'function') {
            initializeIndeterminateStatesForContainer(treeViewContainer);
        }

        // Llama a updateDropdownIcon solo si data-title-id existe y no es la cadena "null"
        const dataTitleId = treeViewContainer.getAttribute('data-title-id');
        if (dataTitleId !== null && dataTitleId !== 'null') {
            if (typeof updateDropdownIcon === 'function') {
                updateDropdownIcon(treeViewContainer);
            }
        }

        // saveInitialState podría ser para una funcionalidad de "Cancelar" que revierte cambios
        // hechos en el cliente *antes* de un postback. Se mantiene por ahora.
        if (typeof saveInitialState === 'function') {
            saveInitialState(treeViewContainer);
        }

        // Configura el listener delegado para clics en la fila dentro del TreeView
        // Implementación más robusta: buscar la <table> que representa el nodo y alternar su checkbox.
        if (!treeViewContainer.dataset.tvsInit) {
            treeViewContainer.addEventListener('click', function(event) {
                const target = event.target;

                // Si se hizo click directamente sobre el checkbox, dejamos que el evento 'change' normal lo maneje.
                if (target.tagName === 'INPUT' && target.type === 'checkbox') return;

                // Ascender el DOM buscando el ancestro que contenga un checkbox (más robusto que assuming table structure)
                let ancestor = target;
                let checkbox = null;
                let anchor = null;
                while (ancestor && ancestor !== treeViewContainer) {
                    if (ancestor.querySelector) {
                        checkbox = ancestor.querySelector('input[type="checkbox"]');
                        anchor = ancestor.querySelector('a[id*="chkListn"]');
                        if (checkbox) break;
                    }
                    ancestor = ancestor.parentElement;
                }

                if (!checkbox) return;

                // Si el click ocurrió dentro de un anchor del nodo, prevenir postback/link default
                if (target.closest('a[id*="chkListn"]')) {
                    event.preventDefault();
                    event.stopPropagation();
                }

                // Evitar doble-toggle: leer estado inicial y aplicar toggle condicional en next tick.
                const initialChecked = checkbox.checked;
                setTimeout(() => {
                    try {
                        // Si el estado no cambió por el comportamiento nativo, invertirlo
                        if (checkbox.checked === initialChecked) {
                            checkbox.checked = !initialChecked;
                        }
                        // Disparar evento change para propagar la actualización
                        const changeEvent = new Event('change', { bubbles: true });
                        checkbox.dispatchEvent(changeEvent);
                    } catch (err) {
                        console.debug('Error toggling checkbox in TreeViewSearch click handler', err);
                    }
                }, 0);
            });

            // Marcar como inicializado para evitar listeners duplicados (importante en re-renderes)
            treeViewContainer.dataset.tvsInit = 'true';
        }

        // Listener para el evento 'change' de los checkboxes
        treeViewContainer.addEventListener('change', function(event) {
            const target = event.target;
            if (target.tagName === 'INPUT' && target.type === 'checkbox') {
                if (typeof handleTreeViewCheckboxChange === 'function') {
                    handleTreeViewCheckboxChange(target, treeViewContainer);
                }
            }
        });

    const dropdownContent = treeViewContainer.closest('.dropdown-content');
        if (dropdownContent) {
            const renderedAcceptButtonId = treeViewContainer.id + '_btnAccept';
            const specificAcceptButton = document.getElementById(renderedAcceptButtonId);
            if (specificAcceptButton && dropdownContent.contains(specificAcceptButton)) {
                specificAcceptButton.addEventListener('click', function () {                    
                    // 1. Guardar el estado actual como el nuevo "estado inicial" para este filtro.
                    // Esto significa que "Cancelar" o una reapertura futura usarán este estado.
                    if (typeof saveInitialState === 'function') {
                        // Pasamos el elemento treeViewContainer directamente
                        saveInitialState(treeViewContainer); 
                    }

                    // 2. Cerrar el dropdown.
                    if (dropdownContent) { 
                        dropdownContent.style.display = 'none';
                        
                        // Limpiar timeouts si los hubiera (asumiendo que dropdownContent tiene un ID)
                        if (dropdownContent.id && typeof clearDropdownTimeout === 'function') {
                            clearDropdownTimeout(dropdownContent.id); 
                        }
                    }
                    // 3. Actualizar el ícono del dropdown para reflejar el estado aceptado.
                    if (typeof updateDropdownIcon === 'function') {
                        updateDropdownIcon(treeViewContainer);
                    }
                });
            }
        }

        const searchInputId = treeViewContainer.id + '_txtSearch';
        const searchInput = document.getElementById(searchInputId);
        if (searchInput) {
            searchInput.addEventListener('keyup', () => {
                if (typeof filterCheckboxes === 'function') {
                    filterCheckboxes(treeViewContainer.id, searchInputId);
                }
            });
        }
        
    });

    // Limpia el flag para los filtros después de usarlo
    if (filtersWereClearedOnServer) {
        sessionStorage.removeItem('filtersCleared');
    }
});

// Re-inicializar listeners después de partial postbacks de ASP.NET AJAX
if (window.Sys && Sys.WebForms && Sys.WebForms.PageRequestManager) {
    try {
        const prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function () {
            // Reutilizar la lógica de inicialización para nuevos fragmentos
            document.querySelectorAll('.date-tree-view').forEach(treeViewContainer => {
                // Limpiar flag de inicialización para forzar re-attach en caso de nuevo DOM
                if (treeViewContainer.dataset.tvsInit) {
                    delete treeViewContainer.dataset.tvsInit;
                }
            });
            // Ejecutar de nuevo la lógica principal
            // Llamamos manualmente a DOMContentLoaded handler simplificado
            try {
                const event = new Event('DOMContentLoaded');
                document.dispatchEvent(event);
            } catch (e) {
                
                document.querySelectorAll('.dropdown-button[data-dropdown-id]').forEach(btn => {
                    btn.removeEventListener('click', function () { });
                    btn.addEventListener('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                        const dropdownId = btn.getAttribute('data-dropdown-id');
                        if (dropdownId) toggleDropdown(dropdownId);
                        return false;
                    });
                });
            }
        });
    } catch (err) {
        console.warn('No se pudo registrar PageRequestManager.endRequest: ', err);
    }
}

// Función para limpiar estados locales de un TreeView específico
function clearLocalStatesForTreeView(treeViewContainer) {
    const pageName = (typeof getPageName === 'function') ? getPageName() : 'Unknown';
    
    // Limpiar estado inicial guardado
    sessionStorage.removeItem(`initialState_${pageName}_${treeViewContainer.id}`);
    
    // Desmarcar todos los checkboxes
    const checkboxes = treeViewContainer.querySelectorAll('input[type="checkbox"]');
    checkboxes.forEach(cb => {
        cb.checked = false;
        cb.indeterminate = false;
    });
    
    // Actualizar ícono
    if (typeof updateDropdownIcon === 'function') {
        updateDropdownIcon(treeViewContainer);
    }
}

/**
 * Actualiza el ícono del botón dropdown basado en los nodos HOJA seleccionados.
 * @param {HTMLElement} treeViewContainer - El elemento contenedor del TreeView (.date-tree-view).
 */
function updateDropdownIcon(treeViewContainer) {
    if (!treeViewContainer) {
        return;
    }

    const titleSpanId = treeViewContainer.getAttribute('data-title-id');
    const titleSpan = document.getElementById(titleSpanId); // Este es el <span> renderizado por litTitle

    if (!titleSpan) {
        return;
    }


    let baseText = titleSpan.getAttribute('data-default-text');
    if (baseText === null) { 
        // Si data-default-text no existe (primera vez), tomar el textContent actual,
        // limpiarle el ícono si existe, y guardarlo como data-default-text.
        const currentContent = titleSpan.textContent || ""; // Usar textContent para obtener solo el texto
        const iconTextRegex = / (bi-caret-down|bi-caret-down-fill)$/; // Asume que el ícono es texto al final
        baseText = currentContent.replace(iconTextRegex, '').trim();
        titleSpan.setAttribute('data-default-text', baseText);
    }

    const allCheckboxes = treeViewContainer.querySelectorAll(':scope input[type="checkbox"]');
    let selectedLeafCount = 0;

    allCheckboxes.forEach(cb => {
        const parentTable = cb.closest('table');
        if (!parentTable) return;

        const isSelectAllCheckbox = cb.value === 'select-all';
        const nodeId = parentTable.querySelector('a[id*="chkListn"]')?.id;
        const childNodesDiv = nodeId ? document.getElementById(nodeId + 'Nodes') : null;
        const isLeafNode = !childNodesDiv;

        if (cb.checked && isLeafNode && !isSelectAllCheckbox) {
            selectedLeafCount++;
        } 
        else if (isSelectAllCheckbox && cb.checked) {
            // Considerar "select-all" como hoja seleccionada si no tiene hijos visibles/reales
            const rootNodesDiv = treeViewContainer.querySelector(':scope > div[id$="Nodes"]');
            let hasVisibleChildren = false;
            if (rootNodesDiv) {
                const childCheckboxes = rootNodesDiv.querySelectorAll(':scope > table input[type="checkbox"]');
                if (childCheckboxes.length > 0) {
                    hasVisibleChildren = true;
                }
            }
            if (!hasVisibleChildren) { 
                selectedLeafCount++;
            }
        }
    });
    
    let iconClass = (selectedLeafCount === 0) ? 'bi-caret-down' : 'bi-caret-down-fill';
    
    // Reconstruir el innerHTML del titleSpan con el baseText y el nuevo ícono
    titleSpan.innerHTML = `${baseText} <i class="bi ${iconClass}"></i>`;
}

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

    // Usar getComputedStyle para decidir visibilidad
    const isVisible = window.getComputedStyle(dropdown).display !== 'none';

    const treeId = dropdownId.replace('_dropdown', '');
    const searchInputId = treeId + '_txtSearch';

    // Cerrar otros dropdowns visibles
    document.querySelectorAll('.dropdown-content').forEach(otherDropdown => {
        if (otherDropdown.id !== dropdownId && window.getComputedStyle(otherDropdown).display !== 'none') {
            otherDropdown.style.display = 'none';
        }
    });

    dropdown.style.display = isVisible ? 'none' : 'block';

    if (!isVisible) {
        const searchInput = document.getElementById(searchInputId);
        if (searchInput) {
            setTimeout(() => {
                searchInput.focus();
            }, 0);
        }
        // Limitar el ancho máximo del dropdown respecto a su contenedor offsetParent
        try {
            const offsetParent = dropdown.offsetParent || dropdown.parentElement || document.body;
            const parentWidth = (offsetParent && offsetParent.clientWidth) ? offsetParent.clientWidth : Math.max(document.documentElement.clientWidth || 0, window.innerWidth || 0);
            const margin = 16; // dejar un pequeño margen
            const maxW = Math.max(150, parentWidth - margin);
            dropdown.style.maxWidth = maxW + 'px';
            dropdown.style.boxSizing = 'border-box';
        } catch (err) {
            // no hacer nada si falla
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
    const isSelectAll = checkbox === treeViewContainer.querySelector('input[type="checkbox"]'); // Ajustar si es necesario

    if (isSelectAll) {
        handleSelectAllChange(checkbox, treeViewContainer);
    } else {
        handleChildCheckboxChange(checkbox, treeViewContainer);
    }

    if (typeof updateDropdownIcon === 'function') {
        const dataTitleId = treeViewContainer.getAttribute('data-title-id');
        if (dataTitleId !== null && dataTitleId !== 'null') {
            updateDropdownIcon(treeViewContainer);
        }
    }       
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
function updateParentState(checkbox, treeViewContainer) 
{
    const currentTable = checkbox.closest('table');
    if (!currentTable) {
        return;
    }

    const containingDiv = currentTable.parentElement;
    if (!containingDiv) {
        return;
    }

    if (containingDiv === treeViewContainer) {
        if (typeof updateSelectAllState === 'function') {
            updateSelectAllState(treeViewContainer);
        }
        return;
    }

    const parentTable = containingDiv.previousElementSibling;
    if (!parentTable || parentTable.tagName !== 'TABLE') {
        // Si el parentElement de containingDiv es treeViewContainer, entonces containingDiv
        // es el div de nodos hijos de "select-all".
        if (containingDiv.parentElement === treeViewContainer) {
            if (typeof updateSelectAllState === 'function') {
                updateSelectAllState(treeViewContainer);
            }
        }
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

    let newCheckedLogical = false; 
    let newIndeterminateBasedOnChildren = false;

    if (totalSiblings === 0) {
        newCheckedLogical = parentCheckbox.checked; 
        newIndeterminateBasedOnChildren = false;
    } else if (indeterminateSiblingsCount > 0) {
        newIndeterminateBasedOnChildren = true;
        newCheckedLogical = false; 
    } else if (checkedSiblingsCount === 0) {
        newCheckedLogical = false;
        newIndeterminateBasedOnChildren = false;
    } else if (checkedSiblingsCount === totalSiblings) {
        newCheckedLogical = true;
        newIndeterminateBasedOnChildren = false;
    } else { 
        newCheckedLogical = false;
        newIndeterminateBasedOnChildren = true;
    }

    let stateChanged = false;
    // Ahora siempre se intenta actualizar si el estado lógico es diferente.
    if (parentCheckbox.checked !== newCheckedLogical) {
        parentCheckbox.checked = newCheckedLogical;
        stateChanged = true; 
    }
    // El estado indeterminate final depende del estado 'checked' actual del DOM del padre
    // y del newIndeterminateBasedOnChildren.
    const currentParentCheckedStateInDOM = parentCheckbox.checked; // Leer el estado 'checked' actual del DOM
    let finalIndeterminateValue;

    if (newCheckedLogical) { // Usar el estado 'checked' que se acaba de determinar/aplicar
        finalIndeterminateValue = false; 
    } else {
        finalIndeterminateValue = newIndeterminateBasedOnChildren;
    }

    if (parentCheckbox.indeterminate !== finalIndeterminateValue) {
        parentCheckbox.indeterminate = finalIndeterminateValue;
        stateChanged = true;
    }

    if (stateChanged) {
        updateParentState(parentCheckbox, treeViewContainer);
    } else {
        const grandParentDiv = parentTable.parentElement;
        if (grandParentDiv === treeViewContainer) {
            if (typeof updateSelectAllState === 'function') {
                updateSelectAllState(treeViewContainer);
            }
        }
    }
}


/**
 * Actualiza el estado del checkbox "Seleccionar todos" basado en los nodos de primer nivel.
 * Incluye lógica para el estado 'indeterminate'.
 * @param {HTMLElement} treeViewContainer - El elemento contenedor del TreeView.
 */
function updateSelectAllState(treeViewContainer) {
    if (!treeViewContainer) return;

    const rootTable = treeViewContainer.querySelector(':scope > table'); // Asume que "select-all" es la primera tabla.
    if (!rootTable) return;
    const selectAllCheckbox = rootTable.querySelector('td input[type="checkbox"]');
    if (!selectAllCheckbox) return;

    // Contenedor de los nodos hijos directos de "select-all".
    const firstLevelNodesContainer = rootTable.nextElementSibling;

    if (!firstLevelNodesContainer || !firstLevelNodesContainer.id || !firstLevelNodesContainer.id.endsWith('Nodes')) {
        // Si no hay un contenedor de nodos hijos válido.
        // Si 'selectAllCheckbox' estaba 'checked', se desmarca porque no hay hijos.
        // Se elimina la condición de window.isInitializingTreeViewStates.
        if (selectAllCheckbox.checked) {
            selectAllCheckbox.checked = false;
        }
        if (selectAllCheckbox.indeterminate) {
            selectAllCheckbox.indeterminate = false;
        }
        return;
    }

    const firstLevelCheckboxes = firstLevelNodesContainer.querySelectorAll(':scope > table td input[type="checkbox"]');
    const totalFirstLevelNodes = firstLevelCheckboxes.length;
    let checkedFirstLevelCount = 0;
    let indeterminateFirstLevelCount = 0;

    if (totalFirstLevelNodes > 0) {
        firstLevelCheckboxes.forEach(cb => {
            if (cb.checked) {
                checkedFirstLevelCount++;
            } else if (cb.indeterminate) {
                indeterminateFirstLevelCount++;
            }
        });
    }

    // Variables para el estado lógico calculado basado en los hijos.
    let newCheckedLogical = false;
    let newIndeterminateBasedOnChildren = false;

    if (totalFirstLevelNodes === 0) {
        // Si no hay nodos hijos, el estado 'checked' lógico es el actual del checkbox.
        // Esto es importante durante la inicialización para respetar el estado del servidor.
        newCheckedLogical = selectAllCheckbox.checked;
        newIndeterminateBasedOnChildren = false; // No puede ser indeterminado.
    } else if (indeterminateFirstLevelCount > 0) {
        newIndeterminateBasedOnChildren = true;
        newCheckedLogical = false; // Si algún hijo es indeterminado, el padre no puede estar 'checked'.
    } else if (checkedFirstLevelCount === 0) {
        newCheckedLogical = false;
        newIndeterminateBasedOnChildren = false;
    } else if (checkedFirstLevelCount === totalFirstLevelNodes) {
        newCheckedLogical = true;
        newIndeterminateBasedOnChildren = false;
    } else { // Algunos marcados, ninguno indeterminado.
        newCheckedLogical = false;
        newIndeterminateBasedOnChildren = true;
    }

    // Solo se modifica el estado 'checked' si no estamos en la fase de inicialización
    // y si el estado lógico calculado es diferente al actual.
    if (selectAllCheckbox.checked !== newCheckedLogical) {
        selectAllCheckbox.checked = newCheckedLogical;
    }

    // El estado 'indeterminate' final depende del estado 'checked' actual del DOM del 'selectAllCheckbox'
    // y del 'newIndeterminateBasedOnChildren' calculado.
    const currentSelectAllCheckedStateInDOM = selectAllCheckbox.checked; // Leer el estado 'checked' actual del DOM.
    let finalIndeterminateValue;

    if (newCheckedLogical) { // Usar el estado 'checked' que se acaba de determinar/aplicar
        finalIndeterminateValue = false;
    } else {
        finalIndeterminateValue = newIndeterminateBasedOnChildren;
    }

    if (selectAllCheckbox.indeterminate !== finalIndeterminateValue) {
        selectAllCheckbox.indeterminate = finalIndeterminateValue;
    }

}

/**
 * Inicializa los estados 'indeterminate' de los checkboxes padres en un TreeView
 * basándose en el estado 'checked' de sus hijos (que ya viene del servidor).
 * NO MODIFICA el estado 'checked' de ningún checkbox.
 * @param {HTMLElement} treeViewContainer - El contenedor principal del TreeView.
 */
function initializeIndeterminateStatesForContainer(treeViewContainer) {
    const allCheckboxesInContainer = Array.from(treeViewContainer.querySelectorAll('input[type="checkbox"]'));

    const rootSelectAllCheckbox = treeViewContainer.querySelector(':scope > table td input[type="checkbox"]'); 

    // Iteramos al revés (de los nodos más profundos/hojas hacia la raíz)
    // para asegurar que el estado 'indeterminate' de un padre intermedio
    // ya esté calculado cuando se evalúe su propio padre.
    for (let i = allCheckboxesInContainer.length - 1; i >= 0; i--) {
        const currentCheckbox = allCheckboxesInContainer[i];
        const parentTable = currentCheckbox.closest('table');
        if (!parentTable) continue;

        let childNodesDiv;
        // Determinar el div de nodos hijos para el currentCheckbox
        if (currentCheckbox === rootSelectAllCheckbox) { 
            childNodesDiv = parentTable.nextElementSibling;
            // Validar que el nextElementSibling sea realmente un contenedor de nodos
            if (childNodesDiv && (!childNodesDiv.id || !childNodesDiv.id.endsWith('Nodes'))) {
                childNodesDiv = null; 
            }
        } else {
            const nodeIdAttr = parentTable.querySelector('a[id*="chkListn"]')?.id;
            childNodesDiv = nodeIdAttr ? document.getElementById(nodeIdAttr + 'Nodes') : null;
        }

        if (childNodesDiv) { // Es un nodo padre (tiene un div de hijos)
            const childCheckboxes = childNodesDiv.querySelectorAll(':scope > table td input[type="checkbox"]');
            const totalChildren = childCheckboxes.length;
            let checkedChildrenCount = 0;
            let indeterminateChildrenCount = 0; // Para propagar el estado indeterminate

            if (totalChildren > 0) {
                childCheckboxes.forEach(childCb => {
                    if (childCb.checked) checkedChildrenCount++;
                    // El 'indeterminate' del hijo ya debería estar establecido por esta misma función
                    // en una iteración anterior (debido al bucle inverso).
                    if (childCb.indeterminate) indeterminateChildrenCount++; 
                });

                // Establecer 'indeterminate' para currentCheckbox (el padre)
                if (currentCheckbox.checked) { 
                    currentCheckbox.indeterminate = false;
                } else { 
                    if (indeterminateChildrenCount > 0) {
                        currentCheckbox.indeterminate = true;
                    } else if (checkedChildrenCount > 0 && checkedChildrenCount < totalChildren) {
                        currentCheckbox.indeterminate = true;
                    } else {
                        // Si todos los hijos están marcados pero el padre no (según el servidor),
                        // o si ningún hijo está marcado, el padre no es indeterminado.
                        currentCheckbox.indeterminate = false;
                    }
                }
            } else { // No tiene nodos hijos (visibles en el DOM)
                currentCheckbox.indeterminate = false;
            }
        } else { // Es un nodo hoja (no tiene childNodesDiv)
            currentCheckbox.indeterminate = false;
        }
    }
}


/**
 * Cancela la selección actual y restaura el estado guardado previamente.
 * @param {string} treeId - ID del TreeView.
 * @param {string} dropdownId - ID del dropdown.
 * @param {string} titleElementId - ID del elemento del título.
 */
function cancelSelection(treeId, dropdownId, titleElementId) {
    const containerElement = document.getElementById(treeId);
    if (containerElement) {
        // updateDropdownTitle(containerElement);
    }

    // Cerrar el dropdown.
    const dropdown = document.getElementById(dropdownId);
    if (dropdown) {
        dropdown.style.display = 'none';
    }
}

// Guarda los valores seleccionados en el hidden asociado al TreeView
function saveCheckedToHidden(treeViewContainerId, hiddenFieldId) {
    const treeView = document.getElementById(treeViewContainerId);
    const hidden = document.getElementById(hiddenFieldId);

    if (!treeView || !hidden) {
        return;
    }
    
    const checkedValues = [];
    treeView.querySelectorAll('input[type="checkbox"]').forEach(cb => {
        if (cb.checked && cb.value !== 'select-all') {
            checkedValues.push(cb.value);    
        }
    });
    hidden.value = checkedValues.join(',');
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
    // sessionStorage.setItem(`initialState_${treeViewContainer.id}`, JSON.stringify(state));
    const pageName = getPageName();
    sessionStorage.setItem(`initialState_${pageName}_${treeViewContainer.id}`, JSON.stringify(state));
}

/**
 * Restaura el estado inicial de los checkboxes en el TreeView.
 * @param {HTMLElement} treeViewContainer - El contenedor principal del TreeView.
 */
function restoreState(treeViewContainer) {
    const pageName = getPageName();
    // const savedState = sessionStorage.getItem(`initialState_${treeViewContainer.id}`);
    const savedState = sessionStorage.getItem(`initialState_${pageName}_${treeViewContainer.id}`);
    if (savedState) {
        const state = JSON.parse(savedState);
        const checkboxes = treeViewContainer.querySelectorAll('input[type="checkbox"]');
        checkboxes.forEach(cb => {
            if (state[cb.id]) {
                cb.checked = state[cb.id].checked;
            }
        });

        // Esto asegura que los estados indeterminate sean consistentes con los checked restaurados.
        if (typeof initializeIndeterminateStatesForContainer === 'function') {
            initializeIndeterminateStatesForContainer(treeViewContainer);
        }

        // Actualizar el ícono del dropdown después de restaurar y recalcular estados.
        if (typeof updateDropdownIcon === 'function') {
            updateDropdownIcon(treeViewContainer);
        }
    }
}