let gridProductoras;

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
    { index: 2, filterType: 'select_local' }, // Representante (sale del dataset)
    { index: 3, filterType: 'select', fetchDataFunc: listaPaisesFilter },
    { index: 4, filterType: 'select', fetchDataFunc: listaTiposDocumentoFilter },
    { index: 5, filterType: 'text' }, // ✅ Nro Doc arreglado
    { index: 6, filterType: 'select', fetchDataFunc: listaCondicionesIvaFilter },
    { index: 7, filterType: 'select', fetchDataFunc: listaProvinciasFilter },
    { index: 8, filterType: 'text' },
    { index: 9, filterType: 'text' },
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


    // Inicializa select2 modal (dropdownParent modal)
    inicializarSelect2Modal();

    // Select2: forzar validación cuando selecciona o limpia
    $("#modalEdicion").on("select2:select select2:clear change", "select", function () {
        validarCampoIndividual(this);
    });

});

/* =========================
   SELECT2 HELPERS
========================= */

function ensureSelect2($el, options) {

    if (!$el || !$el.length) return;

    if ($el.data('select2')) {
        $el.select2('destroy');
    }

    $el.select2(Object.assign({
        width: '100%',
        allowClear: true,
        placeholder: "Todos"
    }, options || {}));
}

function inicializarSelect2Modal() {
    // los combos del modal deben abrir dentro del modal
    const opts = {
        width: '100%',
        dropdownParent: $('#modalEdicion')
    };

    ensureSelect2($("#cmbPais"), opts);
    ensureSelect2($("#cmbTipoDocumento"), opts);
    ensureSelect2($("#cmbCondicionIva"), opts);
    ensureSelect2($("#cmbProvincia"), opts);
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
        Id: id !== "" ? id : 0,

        Nombre: $("#txtNombre").val(),
        NombreRepresentante: $("#txtNombreRepresentante").val(),

        Idpais: $("#cmbPais").val() || null,
        IdTipoDocumento: $("#cmbTipoDocumento").val() || null,
        NumeroDocumento: $("#txtNumeroDocumento").val(),

        IdCondicionIva: $("#cmbCondicionIva").val() || null,

        IdProvincia: $("#cmbProvincia").val() || null,

        Telefono: $("#txtTelefono").val(),
        TelefonoAlternativo: $("#txtTelefonoAlternativo").val(),

        Direccion: $("#txtDireccion").val(),
        Localidad: $("#txtLocalidad").val(),
        EntreCalles: $("#txtEntreCalles").val ? $("#txtEntreCalles").val() : null, // si existe en tu modal
        CodigoPostal: $("#txtCodigoPostal").val(),

        Email: $("#txtEmail").val()
    };

    const url = id === "" ? "/Productoras/Insertar" : "/Productoras/Actualizar";
    const method = id === "" ? "POST" : "PUT";

    fetch(url, {
        method: method,
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify(modelo)
    })
        .then(r => {
            if (!r.ok) throw new Error(r.statusText);
            return r.json();
        })
        .then(_ => {
            const mensaje = id === "" ? "Productora registrada correctamente" : "Productora modificada correctamente";
            $('#modalEdicion').modal('hide');
            exitoModal(mensaje);
            listaProductoras();
        })
        .catch(err => {
            console.error('Error:', err);
            errorModal("Ha ocurrido un error.");
        });
}

function nuevaProductora() {
    limpiarModal();

    // cargar combos
    listaPaises()
        .then(() => listaProvincias(null)) // sin país → vacío

        .then(() => {
            // deja tipo doc e iva vacíos hasta elegir país
            resetSelect("cmbTipoDocumento", "Seleccionar");
            resetSelect("cmbCondicionIva", "Seleccionar");

            // reinit select2 por si se destruyó
            inicializarSelect2Modal();
        });

    $('#modalEdicion').modal('show');

    $("#btnGuardar").html(`<i class="fa fa-check"></i> Registrar`);
    $("#modalEdicionLabel").text("Nueva Productora");

    $("#infoAuditoria").addClass("d-none");
    $("#infoRegistro").html("");
    $("#infoModificacion").html("");
}

async function mostrarModal(modelo) {
    limpiarModal();

    $("#txtId").val(modelo.Id || "");
    $("#txtNombre").val(modelo.Nombre || "");
    $("#txtNombreRepresentante").val(modelo.NombreRepresentante || "");
    $("#txtNumeroDocumento").val(modelo.NumeroDocumento || "");

    $("#txtTelefono").val(modelo.Telefono || "");
    $("#txtTelefonoAlternativo").val(modelo.TelefonoAlternativo || "");

    $("#txtDireccion").val(modelo.Direccion || "");
    $("#txtLocalidad").val(modelo.Localidad || "");
    if ($("#txtEntreCalles").length) $("#txtEntreCalles").val(modelo.EntreCalles || "");
    $("#txtCodigoPostal").val(modelo.CodigoPostal || "");

    $("#txtEmail").val(modelo.Email || "");

    await listaPaises();

    if (modelo.Idpais != null) {
        await listaProvincias(modelo.Idpais);
    } else {
        await listaProvincias();
    }

    // Set País y cargar dependientes
    if (modelo.Idpais != null) {
        $("#cmbPais").val(modelo.Idpais).trigger("change.select2");
        await listaTiposDocumento(modelo.Idpais);
        await listaCondicionesIva(modelo.Idpais);
    }

    if (modelo.IdTipoDocumento != null) {
        $("#cmbTipoDocumento").val(modelo.IdTipoDocumento).trigger("change.select2");
    }

    if (modelo.IdCondicionIva != null) {
        $("#cmbCondicionIva").val(modelo.IdCondicionIva).trigger("change.select2");
    }

    if (modelo.IdProvincia != null) {
        $("#cmbProvincia").val(modelo.IdProvincia).trigger("change.select2");
    }

    inicializarSelect2Modal();

    // Auditoría
    let textoAuditoria = "";

    if (modelo.UsuarioModifica && modelo.FechaModifica) {
        textoAuditoria = `
            <i class="fa fa-edit"></i>
            Última modificación por 
            <strong>${modelo.UsuarioModifica}</strong> 
            el <strong>${formatearFecha(modelo.FechaModifica)}</strong>
        `;
        $("#infoModificacion").html(textoAuditoria);
        $("#infoRegistro").html("");
    } else if (modelo.UsuarioRegistra && modelo.FechaRegistra) {
        textoAuditoria = `
            <i class="fa fa-user"></i>
            Registrado por 
            <strong>${modelo.UsuarioRegistra}</strong> 
            el <strong>${formatearFecha(modelo.FechaRegistra)}</strong>
        `;
        $("#infoRegistro").html(textoAuditoria);
        $("#infoModificacion").html("");
    }

    if (textoAuditoria !== "") $("#infoAuditoria").removeClass("d-none");
    else $("#infoAuditoria").addClass("d-none");

    $('#modalEdicion').modal('show');

    $("#btnGuardar").html(`<i class="fa fa-check"></i> Guardar`);
    $("#modalEdicionLabel").text("Editar Productora");
}

const editarProductora = id => {
    $('.acciones-dropdown').hide();

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
    $('.acciones-dropdown').hide();

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
                        return `
                            <div class="acciones-menu" data-id="${data}">
                                <button class='btn btn-sm btnacciones' type='button' onclick='toggleAcciones(${data})'>
                                    <i class='fa fa-ellipsis-v fa-lg text-white'></i>
                                </button>
                                <div class="acciones-dropdown" style="display: none;">
                                    <button class='btn btn-sm btneditar' onclick='editarProductora(${data})'>
                                        <i class='fa fa-pencil-square-o text-success'></i> Editar
                                    </button>
                                    <button class='btn btn-sm btneliminar' onclick='eliminarProductora(${data})'>
                                        <i class='fa fa-trash-o text-danger'></i> Eliminar
                                    </button>
                                </div>
                            </div>`;
                    },
                    orderable: false,
                    searchable: false,
                },
                { data: 'Nombre' },
                { data: 'NombreRepresentante' },
                { data: 'Pais' },
                { data: 'TipoDocumento' },
                { data: 'NumeroDocumento' },   // ✅ Nro doc OK
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

                // Fuerza que click en la selección abra el dropdown (si algo del layout molesta)
                $(document).on("click", ".select2-container--default .select2-selection--single", function (e) {
                    const $select = $(this).closest(".select2-container").prev("select");
                    if ($select.length) {
                        // si está abierto, no hagas nada
                        if ($select.data("select2") && $select.data("select2").isOpen()) return;
                        $select.select2("open");
                    }
                });

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

    // select2
    inicializarSelect2Modal();
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

    inicializarSelect2Modal();
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

    inicializarSelect2Modal();
}

async function listaProvincias(idPaisSeleccionado = null) {

    resetSelect("cmbProvincia", "Seleccionar");

    // Si no hay país → no cargar provincias
    if (!idPaisSeleccionado) {
        inicializarSelect2Modal();
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

    inicializarSelect2Modal();
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
   ACCIONES DROPDOWN
========================= */

function toggleAcciones(id) {
    var $dropdown = $(`.acciones-menu[data-id="${id}"] .acciones-dropdown`);

    if ($dropdown.is(":visible")) $dropdown.hide();
    else {
        $('.acciones-dropdown').hide();
        $dropdown.show();
    }
}

$(document).on('click', function (e) {
    if (!$(e.target).closest('.acciones-menu').length) {
        $('.acciones-dropdown').hide();
    }
});

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
