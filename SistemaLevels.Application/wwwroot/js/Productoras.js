let gridProductoras;

let clientesCache = [];
let clientesSeleccionados = [];

let clientesAutomaticos = [];
let clientesDesdeCliente = [];


/**
 * Columnas:
 * 0 Acciones
 * 1 Nombre
 * 2 Representante (SELECT2 local desde la tabla)
 * 3 País (SELECT2 remoto)
 * 4 Tipo Doc (SELECT2 remoto)
 * 5 Nro Doc (TEXT)
 * 6 Condición IVA (SELECT2 remoto)
 * 7 Provincia (SELECT2 remoto)
 * 8 Teléfono (TEXT)
 * 9 Email (TEXT)
 */
const columnConfig = [
    { index: 1, filterType: 'text' },
    { index: 2, filterType: 'select', fetchDataFunc: listaPaisesFilter },
    { index: 3, filterType: 'select', fetchDataFunc: listaTiposDocumentoFilter },
    { index: 4, filterType: 'text' },
    { index: 5, filterType: 'select', fetchDataFunc: listaCondicionesIvaFilter },
    { index: 6, filterType: 'select', fetchDataFunc: listaProvinciasFilter },
    { index: 7, filterType: 'text' },
    { index: 8, filterType: 'text' },
];

$(document).ready(() => {

    listaProductoras();

    // Validación campo a campo (igual que Representantes)
    document.querySelectorAll("#modalEdicion input, #modalEdicion select, #modalEdicion textarea").forEach(el => {
        el.setAttribute("autocomplete", "off");
        el.addEventListener("input", () => validarCampoIndividual(el));
        el.addEventListener("change", () => validarCampoIndividual(el));
        el.addEventListener("blur", () => validarCampoIndividual(el));
    });

    // País -> depende tipo doc y condición IVA
    $("#cmbPais").on("change", async function () {
        const idPais = $(this).val();
        await listaTiposDocumento(idPais);
        await listaCondicionesIva(idPais);
        await listaProvincias(idPais);
    });

    // Select2: forzar validación cuando selecciona o limpia
    $("#modalEdicion").on("select2:select select2:clear change", "select", function () {
        validarCampoIndividual(this);
    });

});


$('#modalEdicion').on('shown.bs.modal', function () {
    inicializarSelect2Modal();
});


/* =========================
   SELECT2 HELPERS
========================= */

function ensureSelect2($el, options) {
    if (!$el || !$el.length) return;

    if ($el.data('select2')) return; // NO destruir

    $el.select2(Object.assign({
        width: '100%',
        allowClear: true,
        placeholder: "Todos"
    }, options || {}));
}


function inicializarSelect2Modal() {

    const $modal = $('#modalEdicion');

    $modal.find("select").each(function () {

        const $el = $(this);

        // si ya tiene select2, no lo vuelvas a crear
        if ($el.data('select2')) return;

        $el.select2({
            width: '100%',
            dropdownParent: $modal,
            allowClear: true,
            placeholder: "Seleccionar"
        });
    });
}

function inicializarSelect2Filtro($select) {
    ensureSelect2($select, {
        dropdownParent: $(document.body),
        minimumResultsForSearch: 0,
        allowClear: true,
        placeholder: "Todos"
    });
}

/* =========================
   CRUD
========================= */

function guardarProductora() {

    if (!validarCampos()) return false;

    const id = $("#txtId").val();

    const modelo = {
        Id: id !== "" ? parseInt(id) : 0,

        Nombre: $("#txtNombre").val(),

        Idpais: $("#cmbPais").val() || null,
        IdTipoDocumento: $("#cmbTipoDocumento").val() || null,
        NumeroDocumento: $("#txtNumeroDocumento").val(),

        IdCondicionIva: $("#cmbCondicionIva").val() || null,
        IdProvincia: $("#cmbProvincia").val() || null,

        Telefono: $("#txtTelefono").val(),
        TelefonoAlternativo: $("#txtTelefonoAlternativo").val(),

        Direccion: $("#txtDireccion").val(),
        Localidad: $("#txtLocalidad").val(),
        EntreCalles: $("#txtEntreCalles").val(),
        CodigoPostal: $("#txtCodigoPostal").val(),

        Email: $("#txtEmail").val(),

        AsociacionAutomatica:
            $("#chkAsociacionAutomatica").is(":checked"),

        // ⭐ SIEMPRE manuales
        ClientesIds: clientesSeleccionados
    };

    const url = id === "" ? "/Productoras/Insertar" : "/Productoras/Actualizar";
    const method = id === "" ? "POST" : "PUT";

    fetch(url, {
        method,
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify(modelo)
    })
        .then(r => r.json())
        .then(() => {

            $('#modalEdicion').modal('hide');

            exitoModal(
                id === ""
                    ? "Productora registrada correctamente"
                    : "Productora modificada correctamente"
            );

            listaProductoras();
        })
        .catch(() => errorModal("Ha ocurrido un error."));
}

async function nuevaProductora() {

    limpiarModal();
    setModalSoloLectura(false);

    clientesSeleccionados = [];

    await Promise.all([
        listaPaises(),
        cargarClientesChecklist()
    ]);

    resetSelect("cmbProvincia", "Seleccionar");
    resetSelect("cmbTipoDocumento", "Seleccionar");
    resetSelect("cmbCondicionIva", "Seleccionar");

    inicializarSelect2Modal();

    renderChecklistClientes();

    $('#modalEdicion').modal('show');

    $("#btnGuardar").html(`<i class="fa fa-check"></i> Registrar`);
    $("#modalEdicionLabel").text("Nueva Productora");

    $("#chkAsociacionAutomatica")
        .prop("checked", true)
        .prop("disabled", true);
}

async function mostrarModal(modelo) {

    limpiarModal();
   
    setModalSoloLectura(false);

    $("#txtId").val(modelo.Id || "");
    $("#txtNombre").val(modelo.Nombre || "");

    $("#txtNumeroDocumento").val(modelo.NumeroDocumento || "");
    $("#txtTelefono").val(modelo.Telefono || "");
    $("#txtTelefonoAlternativo").val(modelo.TelefonoAlternativo || "");
    $("#txtDireccion").val(modelo.Direccion || "");
    $("#txtLocalidad").val(modelo.Localidad || "");
    $("#txtEntreCalles").val(modelo.EntreCalles || "");
    $("#txtCodigoPostal").val(modelo.CodigoPostal || "");
    $("#txtEmail").val(modelo.Email || "");

    $("#chkAsociacionAutomatica")
        .prop("checked", true)
        .prop("disabled", true);

    // ⭐ cargar todo primero
    await listaPaises();
    await cargarClientesChecklist(true);

    if (modelo.Idpais) {
        await listaProvincias(modelo.Idpais);
        await listaTiposDocumento(modelo.Idpais);
        await listaCondicionesIva(modelo.Idpais);

        $("#cmbPais").val(modelo.Idpais).trigger("change.select2");
    }

    if (modelo.IdTipoDocumento)
        $("#cmbTipoDocumento").val(modelo.IdTipoDocumento).trigger("change.select2");

    if (modelo.IdCondicionIva)
        $("#cmbCondicionIva").val(modelo.IdCondicionIva).trigger("change.select2");

    if (modelo.IdProvincia)
        $("#cmbProvincia").val(modelo.IdProvincia).trigger("change.select2");


    clientesSeleccionados =
        modelo.clientesManualesIds || [];

    clientesAutomaticos =
        modelo.clientesAutomaticosIds || [];

    clientesDesdeCliente =
        modelo.clientesDesdeClienteIds || [];

    renderChecklistClientes();
    activarBuscadorClientes();

    abrirModalEdicion();

    $("#btnGuardar").html(`<i class="fa fa-check"></i> Guardar`);
    $("#modalEdicionLabel").text("Editar Productora");
}

const editarProductora = id => {
    

    fetch("/Productoras/EditarInfo?id=" + id, {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        }
    })
        .then(r => {
            if (!r.ok) throw new Error("Ha ocurrido un error.");
            return r.json();
        })
        .then(dataJson => {
            if (dataJson) mostrarModal(dataJson);
            else throw new Error("Ha ocurrido un error.");
        })
        .catch(_ => errorModal("Ha ocurrido un error."));
};

async function eliminarProductora(id) {
    

    const confirmado = await confirmarModal("¿Desea eliminar esta productora?");
    if (!confirmado) return;

    try {
        const response = await fetch("/Productoras/Eliminar?id=" + id, {
            method: "DELETE",
            headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) throw new Error("Error al eliminar la Productora.");

        const dataJson = await response.json();
        if (dataJson.valor) {
            listaProductoras();
            exitoModal("Productora eliminada correctamente");
        }
    } catch (e) {
        console.error("Ha ocurrido un error:", e);
        errorModal("Ha ocurrido un error.");
    }
}

/* =========================
   LISTA + DATATABLE
========================= */

async function listaProductoras() {
    let paginaActual = gridProductoras != null ? gridProductoras.page() : 0;

    const response = await fetch(`/Productoras/Lista`, {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        }
    });

    if (!response.ok) throw new Error(`Error en la solicitud: ${response.statusText}`);

    const data = await response.json();
    await configurarDataTable(data);

    if (paginaActual > 0) {
        gridProductoras.page(paginaActual).draw('page');
    }
}

async function configurarDataTable(data) {

    if (!gridProductoras) {

        const $thead = $('#grd_Productoras thead');
        if ($thead.find('tr.filters').length === 0) {
            $thead.find('tr').first().clone(true).addClass('filters').appendTo($thead);
        }

        gridProductoras = $('#grd_Productoras').DataTable({
            data: data,
            language: {
                sLengthMenu: "Mostrar MENU registros",
                url: "//cdn.datatables.net/plug-ins/2.0.7/i18n/es-MX.json"
            },
            scrollX: true,
            scrollCollapse: true,

            columns: [
                {
                    data: "Id",
                    title: '',
                    width: "1%",
                    render: function (data) {
                        return renderAccionesGrid(data, {
                            ver: "verProductora",
                            editar: "editarProductora",
                            eliminar: "eliminarProductora"
                        });
                    },
                    orderable: false,
                    searchable: false,
                },
                { data: 'Nombre' },
                { data: 'Pais' },
                { data: 'TipoDocumento' },
                { data: 'NumeroDocumento' },
                { data: 'CondicionIva' },
                { data: 'Provincia' },
                { data: 'Telefono' },
                { data: 'Email' },
            ],


            dom: 'Bfrtip',
            buttons: [
                {
                    extend: 'excelHtml5',
                    text: 'Excel',
                    className: 'rp-dt-btn',
                    exportOptions: { columns: ':visible:not(:first-child)' }
                },
                {
                    extend: 'pdfHtml5',
                    text: 'PDF',
                    className: 'rp-dt-btn',
                    exportOptions: { columns: ':visible:not(:first-child)' }
                },
                {
                    extend: 'print',
                    text: 'Imprimir',
                    className: 'rp-dt-btn',
                    exportOptions: { columns: ':visible:not(:first-child)' }
                },
                'pageLength'
            ],

            orderCellsTop: true,
            fixedHeader: true,

            initComplete: async function () {
                const api = this.api();

                for (const config of columnConfig) {

                    const cell = $('.filters th').eq(config.index);
                    if (!cell.length) continue;

                    // ✅ SIEMPRE vaciamos la celda del filtro
                    cell.empty();

                    if (config.filterType === 'select' || config.filterType === 'select_local') {

                        const $select = $(`
                            <select class="rp-filter-select" style="width:100%">
                                <option value="">Todos</option>
                            </select>
                        `).appendTo(cell);

                        if (config.filterType === 'select') {
                            const datos = await config.fetchDataFunc();
                            (datos || []).forEach(item => {
                                $select.append(`<option value="${item.Id}">${item.Nombre}</option>`);
                            });
                        } else {
                            // select_local: saca valores únicos del propio DataTable
                            const uniques = new Set();
                            api.column(config.index).data().each(v => {
                                const txt = (v ?? "").toString().trim();
                                if (txt) uniques.add(txt);
                            });

                            [...uniques].sort().forEach(txt => {
                                $select.append(`<option value="${txt}">${txt}</option>`);
                            });
                        }

                        // ✅ select2
                        inicializarSelect2Filtro($select);

                        $select.on('select2:clear', function () {
                            api.column(config.index).search('').draw();
                        });

                        $select.on('change', function () {

                            const value = $(this).val();

                            if (!value) {
                                api.column(config.index)
                                    .search('')
                                    .draw(false);
                                return;
                            }

                            const text = $(this).find('option:selected').text();

                            api.column(config.index)
                                .search('^' + escapeRegex(text) + '$', true, false)
                                .draw(false);
                        });


                    } else {
                        // TEXT
                        const $input = $(`<input class="rp-filter-input" type="text" placeholder="Buscar...">`)
                            .appendTo(cell)
                            .on('keyup change', function () {
                                api.column(config.index).search(this.value).draw();
                            });
                    }
                }

                // Col 0 sin filtro
                $('.filters th').eq(0).html('');

                configurarOpcionesColumnas();
                actualizarKpis(data);
            }
        });

    } else {
        gridProductoras.clear().rows.add(data).draw();
        actualizarKpis(data);
    }
}

/* =========================
   COMBOS MODAL
========================= */

function resetSelect(id, placeholder) {
    const el = document.getElementById(id);
    if (!el) return;
    el.innerHTML = "";
    el.append(new Option(placeholder || "Seleccionar", ""));
}

async function listaPaises() {
    const response = await fetch(`/Paises/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    const data = await response.json();

    resetSelect("cmbPais", "Seleccionar");
    const select = document.getElementById("cmbPais");
    (data || []).forEach(x => select.append(new Option(x.Nombre, x.Id)));
}

async function listaTiposDocumento(idPaisSeleccionado = null) {
    const response = await fetch(`/PaisesTiposDocumentos/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    const data = await response.json();

    resetSelect("cmbTipoDocumento", "Seleccionar");
    const select = document.getElementById("cmbTipoDocumento");

    (data || [])
        .filter(x => !idPaisSeleccionado || x.IdCombo == idPaisSeleccionado)
        .forEach(x => select.append(new Option(x.Nombre, x.Id)));
}

async function listaCondicionesIva(idPaisSeleccionado = null) {
    const response = await fetch(`/PaisesCondicionesIVA/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    const data = await response.json();

    resetSelect("cmbCondicionIva", "Seleccionar");
    const select = document.getElementById("cmbCondicionIva");

    (data || [])
        .filter(x => !idPaisSeleccionado || x.IdCombo == idPaisSeleccionado)
        .forEach(x => select.append(new Option(x.Nombre, x.Id)));

}

async function listaProvincias(idPaisSeleccionado = null) {

    resetSelect("cmbProvincia", "Seleccionar");

    // Si no hay país → no cargar provincias
    if (!idPaisSeleccionado) {
        return;
    }

    const response = await fetch(`/PaisesProvincia/ListaPais?idPais=${idPaisSeleccionado}`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    const data = await response.json();

    const select = document.getElementById("cmbProvincia");

    (data || []).forEach(x => {
        select.append(new Option(x.Nombre, x.Id));
    });

}


/* =========================
   FILTROS - DATOS (para selects)
========================= */

async function listaPaisesFilter() {
    const response = await fetch(`/Paises/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });
    return await response.json();
}

async function listaTiposDocumentoFilter() {
    const response = await fetch(`/PaisesTiposDocumentos/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });
    const data = await response.json();
    return (data || []).map(x => ({ Id: x.Id, Nombre: x.Nombre }));
}

async function listaCondicionesIvaFilter() {
    const response = await fetch(`/PaisesCondicionesIVA/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });
    const data = await response.json();
    return (data || []).map(x => ({ Id: x.Id, Nombre: x.Nombre }));
}

async function listaProvinciasFilter() {
    const response = await fetch(`/PaisesProvincia/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });
    const data = await response.json();
    return (data || []).map(x => ({ Id: x.Id, Nombre: x.Nombre, IdPais: x.IdCombo }));
}


/* =========================
   CONFIG COLUMNAS
========================= */

function configurarOpcionesColumnas() {
    const grid = $('#grd_Productoras').DataTable();
    const columnas = grid.settings().init().columns;
    const container = $('#configColumnasMenu');

    const storageKey = `Productoras_Columnas`;
    const savedConfig = JSON.parse(localStorage.getItem(storageKey)) || {};

    container.empty();

    columnas.forEach((col, index) => {
        if (col.data && col.data !== "Id") {

            const isChecked = savedConfig[`col_${index}`] !== undefined ? savedConfig[`col_${index}`] : true;
            grid.column(index).visible(isChecked);

            const name = $('#grd_Productoras thead tr').first().find('th').eq(index).text();

            container.append(`
                <li class="rp-dd-item">
                    <label class="rp-dd-label">
                        <input type="checkbox" class="toggle-column" data-column="${index}" ${isChecked ? 'checked' : ''}>
                        <span>${name}</span>
                    </label>
                </li>
            `);
        }
    });

    $('.toggle-column').on('change', function () {
        const columnIdx = parseInt($(this).data('column'), 10);
        const isChecked = $(this).is(':checked');

        savedConfig[`col_${columnIdx}`] = isChecked;
        localStorage.setItem(storageKey, JSON.stringify(savedConfig));

        grid.column(columnIdx).visible(isChecked);
    });
}


/* =========================
   VALIDACIONES
========================= */


function getSelect2Selection(el) {
    const $el = $(el);
    const s2 = $el.data("select2");
    if (s2 && s2.$selection && s2.$container) {
        return {
            $selection: s2.$selection,   // el borde visible
            $container: s2.$container    // wrapper select2
        };
    }

    // fallback por si cambia el DOM
    const $cont = $el.nextAll(".select2-container").first();
    return {
        $selection: $cont.find(".select2-selection").first(),
        $container: $cont
    };
}

function setEstadoCampo(el, esValido) {
    const $el = $(el);
    const esSelect = el.tagName === "SELECT";
    const valor = ($el.val() ?? "").toString().trim();

    // 1) clases en el elemento real
    el.classList.toggle("is-invalid", !esValido);
    el.classList.toggle("is-valid", esValido);

    // 2) clases en select2 (lo visible)
    if (esSelect && $el.data("select2")) {
        const { $selection, $container } = getSelect2Selection(el);
        $selection.toggleClass("is-invalid", !esValido);
        $selection.toggleClass("is-valid", esValido);

        // por si tu CSS apunta al container
        $container.toggleClass("is-invalid", !esValido);
        $container.toggleClass("is-valid", esValido);
    }

    // 3) mensaje "Campo obligatorio" (tu caso)
    // Busca feedback cerca del control (adaptable a tu HTML)
    const $wrap = $el.closest(".mb-3, .form-group, .col, .col-md-6, .rp-field, .rp-form-group");
    const $msg = $wrap.find(".invalid-feedback, .rp-invalid-msg, .campo-obligatorio, small.text-danger").first();

    if ($msg.length) {
        // Si es bootstrap invalid-feedback: lo controlamos con display
        // Si es tu <small class="text-danger">Campo obligatorio</small>, también.
        $msg.toggleClass("d-none", esValido);
    }
}


function limpiarModal() {
    const formulario = document.querySelector("#modalEdicion");
    if (!formulario) return;

    formulario.querySelectorAll("input, select, textarea").forEach(el => {
        if (el.type === "checkbox") {
            el.checked = false;
            el.classList.remove("is-invalid", "is-valid");
            return;
        }

        if (el.tagName === "SELECT") el.selectedIndex = 0;
        else el.value = "";

        el.classList.remove("is-invalid", "is-valid");
    });

    $("#errorCampos").addClass("d-none");
}
function validarCampoIndividual(el) {
    const id = el.id;
    const camposObligatorios = ["txtNombre", "cmbPais", "cmbProvincia"];

    if (!camposObligatorios.includes(id)) return;

    const valor = (el.value ?? "").toString().trim();
    const esValido = !(valor === "" || valor === "Seleccionar");

    setEstadoCampo(el, esValido);
    verificarErroresGenerales();
}



function verificarErroresGenerales() {
    const errorMsg = document.getElementById("errorCampos");
    const hayInvalidos = document.querySelectorAll("#modalEdicion .is-invalid").length > 0;
    if (!errorMsg) return;

    if (!hayInvalidos) errorMsg.classList.add("d-none");
}

function validarCampos() {

    const campos = ["#txtNombre", "#cmbPais", "#cmbProvincia"];
    let valido = true;

    campos.forEach(selector => {
        const el = document.querySelector(selector);
        const valor = (el?.value ?? "").toString().trim();
        const esValido = !!el && valor !== "" && valor !== "Seleccionar";

        if (!esValido) valido = false;
        if (el) setEstadoCampo(el, esValido);
    });

    document.getElementById("errorCampos")?.classList.toggle("d-none", valido);
    return valido;
}

/* =========================
   KPI + helpers
========================= */

function actualizarKpis(data) {
    const cant = Array.isArray(data) ? data.length : 0;
    $('#kpiCantProductoras').text(cant);
}

function escapeRegex(text) {
    return (text || "").replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

function formatearFecha(fecha) {
    try {
        const d = new Date(fecha);
        return d.toLocaleString("es-AR");
    } catch {
        return fecha;
    }
}


function activarBuscadorClientes() {

    const input = document.getElementById("txtBuscarCliente");
    if (!input) return;

    input.oninput = function () {

        const texto = this.value.toLowerCase();

        document.querySelectorAll("#listaClientes .rp-check-item")
            .forEach(el => {
                const nombre = el.innerText.toLowerCase();
                el.style.display = nombre.includes(texto) ? "" : "none";
            });
    };
}


const verProductora = id => {

    fetch("/Productoras/EditarInfo?id=" + id, {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        }
    })
        .then(r => {
            if (!r.ok) throw new Error("Ha ocurrido un error.");
            return r.json();
        })
        .then(async dataJson => {

            if (!dataJson) throw new Error("Ha ocurrido un error.");

            await mostrarModal(dataJson);

            // ⭐ GLOBAL (site.js)
            setModalSoloLectura(true);

            $("#modalEdicionLabel").text("Ver Productora");
        })
        .catch(_ => errorModal("Ha ocurrido un error."));
};

async function cargarClientesChecklist(force = false) {

    if (!force && clientesCache.length > 0) return;

    const res = await fetch("/Clientes/Lista", {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    clientesCache = await res.json() || [];
}

function renderChecklistClientes() {

    const cont = document.getElementById("listaClientes");
    cont.innerHTML = "";

    clientesCache.forEach(c => {

        const id = parseInt(c.Id);

        const esManual = clientesSeleccionados.includes(id);
        const esAuto = clientesAutomaticos.includes(id);
        const esDesdeCliente = clientesDesdeCliente.includes(id);

        let checked = "";
        let disabled = "";
        let badge = "";
        let claseExtra = "";

        if (esManual) {
            checked = "checked";
            badge = `<span class="rp-badge manual">Manual</span>`;
            claseExtra = "manual";
        }
        else if (esAuto) {
            checked = "checked";
            disabled = "disabled";
            badge = `<span class="rp-badge auto">Automático</span>`;
            claseExtra = "auto";
        }
        else if (esDesdeCliente) {
            checked = "checked";
            disabled = "disabled";
            badge = `<span class="rp-badge cli">Automático</span>`;
            claseExtra = "cli";
        }

        cont.insertAdjacentHTML("beforeend", `
            <label class="rp-check-item ${claseExtra}">
                <input type="checkbox"
                       value="${id}"
                       ${checked}
                       ${disabled}
                       onchange="toggleCliente(${id})">

                <span class="rp-check-text">
                    ${c.Nombre}
                    ${badge}
                </span>
            </label>
        `);
    });

    actualizarContadorClientes();
}

function toggleCliente(id) {

    id = parseInt(id);

    if (clientesAutomaticos.includes(id)) return;
    if (clientesDesdeCliente.includes(id)) return;

    if (clientesSeleccionados.includes(id))
        clientesSeleccionados =
            clientesSeleccionados.filter(x => x !== id);
    else
        clientesSeleccionados.push(id);

    actualizarContadorClientes();
}

function actualizarContadorClientes() {

    $("#cntClientes").text(
        `(${clientesSeleccionados.length +
        clientesAutomaticos.length +
        clientesDesdeCliente.length})`
    );
}

$("#chkAsociacionAutomatica").on("change", function () {

    const automatico = $(this).is(":checked");

    const cont = document.getElementById("listaClientes");
    if (!cont) return;

    cont.style.opacity = automatico ? "0.85" : "1";
});