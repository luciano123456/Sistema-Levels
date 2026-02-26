const token = localStorage.getItem('JwtToken');

async function MakeAjax(options) {
    return $.ajax({
        type: options.type,
        url: options.url,
        async: options.async,
        data: options.data,
        dataType: options.dataType,
        contentType: options.contentType
    });
}


async function MakeAjaxFormData(options) {
    return $.ajax({
        type: options.type,
        url: options.url,
        async: options.async,
        data: options.data,
        dataType: false,
        contentType: false,
        isFormData: true,
        processData: false
    });
}


// Formatear el número de manera correcta
function formatNumber(number) {
    if (typeof number !== 'number' || isNaN(number)) {
        return "$ 0,00"; // Si el número no es válido, retornar un valor por defecto
    }

    // Asegurarse de que el número tenga dos decimales
    const parts = number.toFixed(2).split("."); // Dividir en parte entera y decimal

    // Formatear la parte entera con puntos como separadores de miles
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, "."); // Usar punto para miles

    // Devolver el número con la coma como separador decimal
    return "$ " + parts.join(",");
}



function mostrarModalConContador(modal, texto, tiempo) {
    $(`#${modal}Text`).text(texto);
    $(`#${modal}`).modal('show');

    setTimeout(function () {
        $(`#${modal}`).modal('hide');
    }, tiempo);
}

function exitoModal(texto) {
    mostrarModalConContador('exitoModal', texto, 1000);
}

function errorModal(texto) {
    mostrarModalConContador('ErrorModal', texto, 3000);
}

function advertenciaModal(texto) {
    mostrarModalConContador('AdvertenciaModal', texto, 3000);
}

function confirmarModal(mensaje) {
    return new Promise((resolve) => {
        const modalEl = document.getElementById('modalConfirmar');
        const mensajeEl = document.getElementById('modalConfirmarMensaje');
        const btnAceptar = document.getElementById('btnModalConfirmarAceptar');

        mensajeEl.innerText = mensaje;

        const modal = new bootstrap.Modal(modalEl, {
            backdrop: 'static',
            keyboard: false
        });

        // Flag para que no resuelva dos veces
        let resuelto = false;

        // Limpia todos los listeners anteriores
        modalEl.replaceWith(modalEl.cloneNode(true));
        // Re-obtener referencias luego de clonar
        const nuevoModalEl = document.getElementById('modalConfirmar');
        const nuevoBtnAceptar = document.getElementById('btnModalConfirmarAceptar');

        const nuevoModal = new bootstrap.Modal(nuevoModalEl, {
            backdrop: 'static',
            keyboard: false
        });

        nuevoBtnAceptar.onclick = function () {
            if (resuelto) return;
            resuelto = true;
            resolve(true);
            nuevoModal.hide();
        };

        nuevoModalEl.addEventListener('hidden.bs.modal', () => {
            if (resuelto) return;
            resuelto = true;
            resolve(false);
        }, { once: true });

        nuevoModal.show();
    });
}


const formatoMoneda = new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: 'ARS', // Cambia "ARS" por el código de moneda que necesites
    minimumFractionDigits: 2
});

function convertirMonedaAFloat(moneda) {
    // Eliminar el símbolo de la moneda y otros caracteres no numéricos
    const soloNumeros = moneda.replace(/[^0-9,.-]/g, '');

    // Eliminar separadores de miles y convertir la coma en punto
    const numeroFormateado = soloNumeros.replace(/\./g, '').replace(',', '.');

    // Convertir a flotante
    const numero = parseFloat(numeroFormateado);

    // Devolver el número formateado como cadena, asegurando los decimales
    return numero.toFixed(2); // Asegura siempre dos decimales en la salida
}
function convertirAMonedaDecimal(valor) {
    // Reemplazar coma por punto
    if (typeof valor === 'string') {
        valor = valor.replace(',', '.'); // Cambiar la coma por el punto
    }
    // Convertir a número flotante
    return parseFloat(valor);
}

function formatoNumero(valor) {
    // Reemplaza la coma por punto y elimina otros caracteres no numéricos (como $)
    return parseFloat(valor.replace(/[^0-9,]+/g, '').replace(',', '.')) || 0;
}

function parseDecimal(value) {
    return parseFloat(value.replace(',', '.'));
}


function formatMoneda(valor) {
    // Convertir a string, cambiar el punto decimal a coma y agregar separadores de miles
    let formateado = valor
        .toString()
        .replace('.', ',') // Cambiar punto decimal a coma
        .replace(/\B(?=(\d{3})+(?!\d))/g, "."); // Agregar separadores de miles

    // Agregar el símbolo $ al inicio
    return `$ ${formateado}`;
}


function toggleAcciones(id) {
    const dropdown = document.querySelector(`.acciones-menu[data-id='${id}'] .acciones-dropdown`);
    const isVisible = dropdown.style.display === 'block';

    // Oculta todos los demás menús desplegables
    document.querySelectorAll('.acciones-dropdown').forEach(el => el.style.display = 'none');

    if (!isVisible) {
        // Muestra el menú
        dropdown.style.display = 'block';

        // Obtén las coordenadas del botón
        const menuButton = document.querySelector(`.acciones-menu[data-id='${id}']`);
        const rect = menuButton.getBoundingClientRect();

        // Mueve el menú al body y ajusta su posición
        const dropdownClone = dropdown.cloneNode(true);
        dropdownClone.style.position = 'fixed';
        dropdownClone.style.left = `${rect.left}px`;
        dropdownClone.style.top = `${rect.bottom}px`;
        dropdownClone.style.zIndex = '10000';
        dropdownClone.style.display = 'block';

        // Limpia menús previos si es necesario
        document.querySelectorAll('.acciones-dropdown-clone').forEach(clone => clone.remove());

        dropdownClone.classList.add('acciones-dropdown-clone');
        document.body.appendChild(dropdownClone);
    }
}




function formatearFechaParaInput(fecha) {
    const m = moment(fecha, [moment.ISO_8601, 'YYYY-MM-DD HH:mm:ss', 'YYYY-MM-DD']);
    return m.isValid() ? m.format('YYYY-MM-DD') : '';
}

function formatearFechaParaVista(fecha) {
    const m = moment(fecha, [moment.ISO_8601, 'YYYY-MM-DD HH:mm:ss', 'YYYY-MM-DD']);
    return m.isValid() ? m.format('DD/MM/YYYY') : '';
}

function formatearMiles(valor) {
    let num = String(valor).replace(/\D/g, '');
    return num.replace(/\B(?=(\d{3})+(?!\d))/g, ".");
}

function formatearSinMiles(valor) {
    if (!valor) return 0;

    // Si no tiene puntos, devolvés directamente el número original
    if (!valor.includes('.')) return parseFloat(valor) || 0;

    const limpio = valor.replace(/\./g, '').replace(',', '.');
    const num = parseFloat(limpio);
    return isNaN(num) ? 0 : num;
}


let audioContext = null;
let audioBuffer = null;


function llenarSelect(selectId, data, valueField = 'Id', textField = 'Nombre', conOpcionVacia = true) {
    const sel = document.getElementById(selectId);
    if (!sel) return;
    sel.innerHTML = conOpcionVacia ? '<option value="">Seleccione</option>' : '';
    (data || []).forEach(it => {
        const opt = document.createElement('option');
        opt.value = it[valueField];
        opt.textContent = it[textField];
        sel.appendChild(opt);
    });
}



function formatearFecha(fecha) {
    try {
        const d = new Date(fecha);
        return d.toLocaleString("es-AR");
    } catch {
        return fecha;
    }
}

function normalizarDateInput(fecha) {
    if (!fecha) return "";
    try {
        const d = new Date(fecha);
        if (isNaN(d.getTime())) return "";
        const yyyy = d.getFullYear();
        const mm = String(d.getMonth() + 1).padStart(2, '0');
        const dd = String(d.getDate()).padStart(2, '0');
        return `${yyyy}-${mm}-${dd}`;
    } catch {
        return "";
    }
}

function normalizarFechaTabla(fecha) {
    // Mostramos dd/MM/yyyy (si viene ISO)
    if (!fecha) return "";
    try {
        const d = new Date(fecha);
        if (isNaN(d.getTime())) return fecha;
        return d.toLocaleDateString("es-AR");
    } catch {
        return fecha;
    }
}


function abrirModalEdicion() {
    const modalEl = document.getElementById('modalEdicion');

    const modal = new bootstrap.Modal(modalEl, {
        backdrop: 'static',
        keyboard: false
    });

    modal.show();
}


function setModalSoloLectura(esSoloLectura) {
    const $modal = $("#modalEdicion");

    // Ocultar botón guardar/registrar
    $("#btnGuardar").toggleClass("d-none", esSoloLectura);

    // Opcional: por si tenés otro botón en el footer
    // $("#btnAlgoMas").toggleClass("d-none", esSoloLectura);

    // Deshabilitar inputs/textareas
    $modal.find("input, textarea").prop("disabled", esSoloLectura);

    // Deshabilitar selects normales + select2
    $modal.find("select").each(function () {
        const $el = $(this);
        $el.prop("disabled", esSoloLectura);

        if ($el.data("select2")) {
            $el.prop("disabled", esSoloLectura);
            $el.trigger("change.select2");
        }
    });

    // Evitar que se “pinten” validaciones mientras está solo lectura
    $modal.attr("data-sololectura", esSoloLectura ? "1" : "0");
}



/* =====================================
GS-UI — Render Acciones Grid GLOBAL
===================================== */

function renderAccionesGrid(id, acciones) {

    const btnVer = acciones.ver
        ? `
        <button type="button"
            class="btn btn-sm rp-act rp-act-view"
            title="Ver"
            onclick="${acciones.ver}(${id})">
            <i class="fa fa-file-text-o"></i>
        </button>`
        : "";

    const btnEditar = acciones.editar
        ? `
        <button type="button"
            class="btn btn-sm rp-act rp-act-edit"
            title="Editar"
            onclick="${acciones.editar}(${id})">
            <i class="fa fa-pencil-square-o"></i>
        </button>`
        : "";

    const btnEliminar = acciones.eliminar
        ? `
        <button type="button"
            class="btn btn-sm rp-act rp-act-del"
            title="Eliminar"
            onclick="${acciones.eliminar}(${id})">
            <i class="fa fa-trash-o"></i>
        </button>`
        : "";

    return `
        <div class="rp-row-actions" data-id="${id}">
            ${btnVer}
            ${btnEditar}
            ${btnEliminar}
        </div>
    `;
}

/* ======================================================
EXPORTADOR GLOBAL DATATABLES
(usar desde cualquier grid)
====================================================== */

window.ExportadorDT = {
    grid: null,
    tipo: null
};

/* =========================
   ABRIR MODAL
========================= */

window.abrirModalExportacion = function (grid, tipo, nombreListado) {

    if (!grid) return;

    ExportadorDT.grid = grid;
    ExportadorDT.tipo = tipo;
    ExportadorDT.nombreListado = nombreListado || "Datos";

    const container = $("#exportColumnsContainer");
    container.empty();

    const columns = grid.settings()[0].aoColumns;

    columns.forEach((col, index) => {

        if (index === 0) return;
        if (!grid.column(index).visible()) return;

        const nombre = col.sTitle || `Columna ${index}`;

        container.append(`
<label class="export-item">
    <input type="checkbox"
           class="export-col"
           value="${index}"
           checked>
    <span class="export-pill">${nombre}</span>
</label>
`);
    });

    $("#modalExportar").modal("show");
};

/* =========================
   CONFIRMAR EXPORT
========================= */

$(document).off("click.exportador")
    .on("click.exportador", "#btnConfirmarExport", function () {

        const columnas = [];

        $(".export-col:checked").each(function () {
            columnas.push(parseInt($(this).val()));
        });

        if (!columnas.length) {
            alert("Seleccione al menos una columna");
            return;
        }

        $("#modalExportar").modal("hide");

        ejecutarExportacionGlobal(columnas);
    });


/* =========================
   EXPORT REAL
========================= */

window.ejecutarExportacionGlobal = function (columnas) {

    const grid = ExportadorDT.grid;
    const tipo = ExportadorDT.tipo;

    if (!grid) return;

    const tituloExport = `Listado de ${ExportadorDT.nombreListado}`;

    const configBase = {
        title: tituloExport || "Exportación",
        exportOptions: {
            columns: columnas
        }
    };

    let buttonConfig;

    switch (tipo) {
        case "excel":
            buttonConfig = { extend: 'excelHtml5', ...configBase };
            break;

        case "pdf":
            buttonConfig = {
                extend: 'pdfHtml5',
                orientation: 'landscape',
                pageSize: 'A4',
                ...configBase
            };
            break;

        case "print":
            buttonConfig = { extend: 'print', ...configBase };
            break;

        default:
            return;
    }

    const temp = new $.fn.dataTable.Buttons(grid, {
        buttons: [buttonConfig]
    });

    // ✅ EJECUCIÓN REAL
    temp.container().find('button').trigger('click');
};

$(document).on("change", "#chkExportAll", function () {
    $(".export-col").prop("checked", this.checked);
});

window.ExportadorDT = {
    grid: null,
    tipo: null,
    nombreListado: null   // 👈 NUEVO
};