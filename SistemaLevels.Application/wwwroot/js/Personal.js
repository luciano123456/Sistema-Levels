let gridPersonal;
let rolesCatalogo = [];

let filtrosPersonalActivos = false;


/**
 * Columnas:
 * 0 Acciones
 * 1 Nombre (TEXT)
 * 2 País (SELECT2 remoto)
 * 3 Tipo Doc (SELECT2 remoto)
 * 4 Nro Doc (TEXT)
 * 5 Condición IVA (SELECT2 remoto)
 * 6 Teléfono (TEXT)
 * 7 Email (TEXT)
 */
const columnConfig = [
    { index: 1, filterType: 'text' }, // Nombre
    { index: 2, filterType: 'select', fetchDataFunc: listaPaisesFilter }, // País
    { index: 3, filterType: 'select', fetchDataFunc: listaTiposDocumentoFilter }, // Tipo Doc
    { index: 4, filterType: 'text' }, // Nro Doc
    { index: 5, filterType: 'select', fetchDataFunc: listaCondicionesIvaFilter }, // Condición IVA
    { index: 6, filterType: 'text' }, // Teléfono
    { index: 7, filterType: 'text' }, // Email
];

$(document).ready(() => {



    listaPersonal();

    // Validación campo a campo
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
    });

    // Inicializa select2 modal (dropdownParent modal)
    inicializarSelect2Modal();

    // Select2: forzar validación cuando selecciona o limpia
    $("#modalEdicion").on("select2:select select2:clear change", "select", function () {
        validarCampoIndividual(this);
    });

    inicializarFiltrosPersonal();
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
    const opts = {
        width: '100%',
        dropdownParent: $('#modalEdicion')
    };

    ensureSelect2($("#cmbPais"), opts);
    ensureSelect2($("#cmbTipoDocumento"), opts);
    ensureSelect2($("#cmbCondicionIva"), opts);
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

function guardarPersonal() {
    if (!validarCampos()) return false;

    const id = $("#txtId").val();

    const fnRaw = $("#txtFechaNacimiento").val();
    const fechaNacimiento = fnRaw && fnRaw.trim() !== "" ? fnRaw : null;

    const rolesIds = getRolesSeleccionadosIds();
    const artistasIds = getArtistasSeleccionadosIds();


    const modelo = {
        Id: id !== "" ? id : 0,

        Nombre: $("#txtNombre").val(),
        Dni: $("#txtDni").val(),

        IdPais: $("#cmbPais").val() || null,
        IdTipoDocumento: $("#cmbTipoDocumento").val() || null,
        NumeroDocumento: $("#txtNumeroDocumento").val(),

        Direccion: $("#txtDireccion").val(),
        Telefono: $("#txtTelefono").val(),
        Email: $("#txtEmail").val(),

        FechaNacimiento: fechaNacimiento,

        IdCondicionIva: $("#cmbCondicionIva").val() || null,

        // NUEVO
        RolesIds: rolesIds,
        ArtistasIds: artistasIds
    };

    const url = id === "" ? "/Personal/Insertar" : "/Personal/Actualizar";
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
            const mensaje = id === "" ? "Personal registrado correctamente" : "Personal modificado correctamente";
            $('#modalEdicion').modal('hide');
            exitoModal(mensaje);
            listaPersonal();
        })
        .catch(err => {
            console.error('Error:', err);
            errorModal("Ha ocurrido un error.");
        });
}

function nuevoPersonal() {
    limpiarModal();

    setModalSoloLectura(false);

    Promise.all([
        listaPaises(),
        listaRoles(),
        listaArtistas()
    ])
        .then(() => {
            evaluarPestaniaArtistas();
            resetSelect("cmbTipoDocumento", "Seleccionar");
            resetSelect("cmbCondicionIva", "Seleccionar");

            inicializarSelect2Modal();
        });

    $('#modalEdicion').modal('show');

    $("#btnGuardar")
        .attr("onclick", "guardarPersonal()")
        .html(`<i class="fa fa-check"></i> Registrar`);

    $("#modalEdicionLabel").text("Nuevo Personal");

    $("#infoAuditoria").addClass("d-none");
    $("#infoRegistro").html("");
    $("#infoModificacion").html("");
}

async function mostrarModal(modelo) {
    limpiarModal();

    setModalSoloLectura(false);

    // 🔹 SIEMPRE abrir en la pestaña de datos
    const tabDatos = document.querySelector('#personalTabs button[data-bs-target="#tabDatos"]');
    if (tabDatos) {
        const tab = new bootstrap.Tab(tabDatos);
        tab.show();
    }

    $("#txtId").val(modelo.Id || "");
    $("#txtNombre").val(modelo.Nombre || "");
    $("#txtDni").val(modelo.Dni || "");

    $("#txtNumeroDocumento").val(modelo.NumeroDocumento || "");
    $("#txtDireccion").val(modelo.Direccion || "");
    $("#txtTelefono").val(modelo.Telefono || "");
    $("#txtEmail").val(modelo.Email || "");

    if (modelo.FechaNacimiento) {
        try {
            const d = new Date(modelo.FechaNacimiento);
            const yyyy = d.getFullYear();
            const mm = String(d.getMonth() + 1).padStart(2, '0');
            const dd = String(d.getDate()).padStart(2, '0');
            $("#txtFechaNacimiento").val(`${yyyy}-${mm}-${dd}`);
        } catch {
            $("#txtFechaNacimiento").val("");
        }
    } else {
        $("#txtFechaNacimiento").val("");
    }

    await Promise.all([
        listaPaises(),
        listaRoles(),
        listaArtistas()
    ]);

    if (modelo.IdPais != null) {
        $("#cmbPais").val(modelo.IdPais).trigger("change.select2");
        await listaTiposDocumento(modelo.IdPais);
        await listaCondicionesIva(modelo.IdPais);
    }

    if (modelo.IdTipoDocumento != null) {
        $("#cmbTipoDocumento").val(modelo.IdTipoDocumento).trigger("change.select2");
    }

    if (modelo.IdCondicionIva != null) {
        $("#cmbCondicionIva").val(modelo.IdCondicionIva).trigger("change.select2");
    }

    // roles
    if (modelo.RolesIds) {
        $("#listaRoles input[type=checkbox]").each(function () {
            const id = parseInt($(this).val());
            if (modelo.RolesIds.includes(id)) {
                $(this).prop("checked", true);
            }
        });
    }

    evaluarBloqueArtistas();

    // artistas
    if (modelo.ArtistasIds) {
        $("#listaArtistas input[type=checkbox]").each(function () {
            const id = parseInt($(this).val());
            if (modelo.ArtistasIds.includes(id)) {
                $(this).prop("checked", true);
            }
        });
    }

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

    $("#btnGuardar")
        .attr("onclick", "guardarPersonal()")
        .html(`<i class="fa fa-check"></i> Guardar`);

    $("#modalEdicionLabel").text("Editar Personal");

    actualizarContadoresTabs();

}

const editarPersonal = id => {
    

    fetch("/Personal/EditarInfo?id=" + id, {
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

async function eliminarPersonal(id) {
    

    const confirmado = await confirmarModal("¿Desea eliminar este registro de personal?");
    if (!confirmado) return;

    try {
        const response = await fetch("/Personal/Eliminar?id=" + id, {
            method: "DELETE",
            headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) throw new Error("Error al eliminar el Personal.");

        const dataJson = await response.json();
        if (dataJson.valor) {
            listaPersonal();
            exitoModal("Personal eliminado correctamente");
        }
    } catch (e) {
        console.error("Ha ocurrido un error:", e);
        errorModal("Ha ocurrido un error.");
    }
}

/* =========================
   LISTA + DATATABLE
========================= */

async function listaPersonal() {
    let paginaActual = gridPersonal != null ? gridPersonal.page() : 0;

    const response = await fetch(`/Personal/Lista`, {
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
        gridPersonal.page(paginaActual).draw('page');
    }
}

async function configurarDataTable(data) {

    if (!gridPersonal) {

        const $thead = $('#grd_Personal thead');
        if ($thead.find('tr.filters').length === 0) {
            $thead.find('tr').first().clone(true).addClass('filters').appendTo($thead);
        }

        gridPersonal = $('#grd_Personal').DataTable({
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
                            ver: "verPersonal",
                            editar: "editarPersonal",
                            eliminar: "eliminarPersonal"
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
                { data: 'Telefono' },
                { data: 'Email' },
            ],

            dom: 'Bfrtip',
            buttons: [
                {
                    text: 'Excel',
                    action: () => abrirModalExportacion(gridPersonal, 'excel', 'Personal')
                },
                {
                    text: 'PDF',
                    action: () => abrirModalExportacion(gridPersonal, 'pdf', 'Personal')
                },
                {
                    text: 'Imprimir',
                    action: () => abrirModalExportacion(gridPersonal, 'print', 'Personal')
                }
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

                    if (config.filterType === 'select') {

                        const $select = $(`
                            <select class="rp-filter-select" style="width:100%">
                                <option value="">Todos</option>
                            </select>
                        `).appendTo(cell);

                        const datos = await config.fetchDataFunc();
                        (datos || []).forEach(item => {
                            $select.append(`<option value="${item.Id}">${item.Nombre}</option>`);
                        });

                        // ✅ select2
                        inicializarSelect2Filtro($select);

                        // ✅ al limpiar, borra filtro y redibuja bien
                        $select.on('select2:clear', function () {
                            api.column(config.index).search('').draw(false);
                        });

                        // ✅ change: aplica exact match por texto visible
                        $select.on('change', function () {

                            const value = $(this).val();

                            if (!value) {
                                api.column(config.index).search('').draw(false);
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
                                api.column(config.index).search(this.value).draw(false);
                            });
                    }
                }

                // Col 0 sin filtro
                $('.filters th').eq(0).html('');

                configurarOpcionesColumnas();
                actualizarKpis(data);

                // Fuerza que click en la selección abra el dropdown
                $(document).on("click", ".select2-container--default .select2-selection--single", function (e) {
                    const $select = $(this).closest(".select2-container").prev("select");
                    if ($select.length) {
                        if ($select.data("select2") && $select.data("select2").isOpen()) return;
                        $select.select2("open");
                    }
                });

            }
        });

    } else {
        gridPersonal.clear().rows.add(data).draw();
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

/* =========================
   CONFIG COLUMNAS
========================= */

function configurarOpcionesColumnas() {
    const grid = $('#grd_Personal').DataTable();
    const columnas = grid.settings().init().columns;
    const container = $('#configColumnasMenu');

    const storageKey = `Personal_Columnas`;
    const savedConfig = JSON.parse(localStorage.getItem(storageKey)) || {};

    container.empty();

    columnas.forEach((col, index) => {
        if (col.data && col.data !== "Id") {

            const isChecked = savedConfig[`col_${index}`] !== undefined ? savedConfig[`col_${index}`] : true;
            grid.column(index).visible(isChecked);

            const name = $('#grd_Personal thead tr').first().find('th').eq(index).text();

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

/* =========================
   VALIDACIONES
========================= */

function getSelect2Selection(el) {
    const $el = $(el);
    const s2 = $el.data("select2");
    if (s2 && s2.$selection && s2.$container) {
        return {
            $selection: s2.$selection,
            $container: s2.$container
        };
    }

    const $cont = $el.nextAll(".select2-container").first();
    return {
        $selection: $cont.find(".select2-selection").first(),
        $container: $cont
    };
}

function setEstadoCampo(el, esValido) {
    const $el = $(el);
    const esSelect = el.tagName === "SELECT";

    el.classList.toggle("is-invalid", !esValido);
    el.classList.toggle("is-valid", esValido);

    if (esSelect && $el.data("select2")) {
        const { $selection, $container } = getSelect2Selection(el);
        $selection.toggleClass("is-invalid", !esValido);
        $selection.toggleClass("is-valid", esValido);
        $container.toggleClass("is-invalid", !esValido);
        $container.toggleClass("is-valid", esValido);
    }

    const $wrap = $el.closest(".mb-3, .form-group, .col, .col-md-6, .rp-field, .rp-form-group");
    const $msg = $wrap.find(".invalid-feedback, .rp-invalid-msg, .campo-obligatorio, small.text-danger").first();

    if ($msg.length) {
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

        // limpiar select2 visible
        if (el.tagName === "SELECT" && $(el).data("select2")) {
            const { $selection, $container } = getSelect2Selection(el);
            $selection.removeClass("is-invalid is-valid");
            $container.removeClass("is-invalid is-valid");
        }
    });

    $("#errorCampos").addClass("d-none");
}

function validarCampoIndividual(el) {
    const id = el.id;
    const camposObligatorios = ["txtNombre", "cmbPais"];

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

    const campos = ["#txtNombre", "#cmbPais"];
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
    $('#kpiCantPersonal').text(cant);
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


/* =========================
ROLES Y ARTISTAS
========================= */


async function listaArtistas() {
    const response = await fetch(`/Artistas/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    const data = await response.json();

    const container = $("#listaArtistas");
    container.empty();

    // buscador
    container.append(`
        <div class="rp-search-box mb-2">
            <input type="text" 
                   id="buscarArtista"
                   class="form-control"
                   placeholder="Buscar artista...">
        </div>
    `);

    const lista = $('<div class="rp-checklist-items"></div>');
    container.append(lista);

    (data || []).forEach(x => {
        lista.append(`
            <label class="rp-check-item" data-nombre="${x.NombreArtistico.toLowerCase()}">
                <input type="checkbox" value="${x.Id}">
                <span>${x.NombreArtistico}</span>
            </label>
        `);
    });

    // filtro
    $("#buscarArtista").on("keyup", function () {
        const texto = $(this).val().toLowerCase();

        $("#listaArtistas .rp-check-item").each(function () {
            const nombre = $(this).data("nombre");
            $(this).toggle(nombre.includes(texto));
        });
    });

    // contador
    $("#listaArtistas input").on("change", actualizarContadoresTabs);

    actualizarContadoresTabs();
}

async function listaRoles() {
    const response = await fetch(`/PersonalRol/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    const data = await response.json();
    rolesCatalogo = data || [];

    const container = $("#listaRoles");
    container.empty();

    rolesCatalogo.forEach(x => {
        container.append(`
            <label class="rp-check-item">
                <input type="checkbox" value="${x.Id}">
                <span>${x.Nombre}</span>
            </label>
        `);
    });

    // eventos
    $("#listaRoles input").on("change", function () {
        evaluarBloqueArtistas();
        actualizarContadoresTabs();
    });

    actualizarContadoresTabs();
}


function evaluarPestaniaArtistas() {

    const rolesIds = getRolesSeleccionadosIds();

    const rolRepresentante = rolesCatalogo.find(r =>
        r.Nombre.toLowerCase() === "representante"
    );

    if (!rolRepresentante) return;

    const tieneRol = rolesIds.includes(rolRepresentante.Id);

    if (tieneRol) {
        $("#tabArtistasBtn").removeClass("d-none");
    } else {
        $("#tabArtistasBtn").addClass("d-none");

        // desmarcar artistas
        document
            .querySelectorAll("#listaArtistas input[type=checkbox]")
            .forEach(chk => chk.checked = false);
    }
}

function getRolesSeleccionadosIds() {
    const ids = [];
    document
        .querySelectorAll("#listaRoles input[type=checkbox]:checked")
        .forEach(chk => ids.push(parseInt(chk.value)));

    return ids;
}


function getArtistasSeleccionadosIds() {
    const ids = [];
    document
        .querySelectorAll("#listaArtistas input[type=checkbox]:checked")
        .forEach(chk => ids.push(parseInt(chk.value)));

    return ids;
}


function evaluarBloqueArtistas() {

    const rolesSeleccionados = [];

    $("#listaRoles input:checked").each(function () {
        rolesSeleccionados.push(parseInt($(this).val()));
    });

    const rolRepresentante = rolesCatalogo.find(r => r.Nombre.toLowerCase() === "representante");
    if (!rolRepresentante) return;

    const tieneRolRepresentante = rolesSeleccionados.includes(rolRepresentante.Id);

    if (tieneRolRepresentante) {
        $("#tabArtistasBtn").removeClass("d-none");
    } else {
        $("#tabArtistasBtn").addClass("d-none");
        $("#listaArtistas input").prop("checked", false);
    }
}


function actualizarContadoresTabs() {

    const cantRoles = $("#listaRoles input:checked").length;
    const cantArtistas = $("#listaArtistas input:checked").length;

    $("#contadorRoles").text(`(${cantRoles})`);
    $("#contadorArtistas").text(`(${cantArtistas})`);
}


const verPersonal = id => {

    fetch("/Personal/EditarInfo?id=" + id, {
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

            // ⭐ reutiliza función global del site.js
            setModalSoloLectura(true);

            $("#modalEdicionLabel").text("Ver Personal");
        })
        .catch(_ => errorModal("Ha ocurrido un error."));
};

async function inicializarFiltrosPersonal() {

    await Promise.all([
        cargarFiltroPaises(),
        cargarFiltroTiposDocumento(),
        cargarFiltroCondicionesIva(),
        cargarFiltroRoles(),
        cargarFiltroArtistas()
    ]);

    inicializarSelect2Filtro($("#fPais"));
    inicializarSelect2Filtro($("#fTipoDocumento"));
    inicializarSelect2Filtro($("#fCondicionIva"));
    inicializarSelect2Filtro($("#fRol"));
    inicializarSelect2Filtro($("#fArtista"));
}

async function cargarFiltroPaises() {

    const r = await fetch(`/Paises/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    const data = await r.json();

    const $sel = $("#fPais");
    $sel.empty().append(`<option value="">Todos</option>`);

    (data || []).forEach(x =>
        $sel.append(`<option value="${x.Id}">${x.Nombre}</option>`)
    );
}

async function cargarFiltroTiposDocumento() {

    const r = await fetch(`/PaisesTiposDocumentos/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    const data = await r.json();

    const $sel = $("#fTipoDocumento");
    $sel.empty().append(`<option value="">Todos</option>`);

    (data || []).forEach(x =>
        $sel.append(`<option value="${x.Id}">${x.Nombre}</option>`)
    );
}

async function cargarFiltroCondicionesIva() {

    const r = await fetch(`/PaisesCondicionesIVA/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    const data = await r.json();

    const $sel = $("#fCondicionIva");
    $sel.empty().append(`<option value="">Todos</option>`);

    (data || []).forEach(x =>
        $sel.append(`<option value="${x.Id}">${x.Nombre}</option>`)
    );
}

async function cargarFiltroRoles() {

    const r = await fetch(`/PersonalRol/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    const data = await r.json();

    const $sel = $("#fRol");
    $sel.empty().append(`<option value="">Todos</option>`);

    (data || []).forEach(x =>
        $sel.append(`<option value="${x.Id}">${x.Nombre}</option>`)
    );
}

async function cargarFiltroArtistas() {

    const r = await fetch(`/Artistas/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    const data = await r.json();

    const $sel = $("#fArtista");
    $sel.empty().append(`<option value="">Todos</option>`);

    (data || []).forEach(x =>
        $sel.append(`<option value="${x.Id}">${x.NombreArtistico}</option>`)
    );
}

/* =========================
APLICAR FILTROS
========================= */

async function aplicarFiltrosPersonal() {

    const filtros = {
        Nombre: $("#fNombre").val() || null,
        IdPais: $("#fPais").val() || null,
        IdTipoDocumento: $("#fTipoDocumento").val() || null,
        IdCondicionIva: $("#fCondicionIva").val() || null,
        IdRol: $("#fRol").val() || null,
        IdArtista: $("#fArtista").val() || null
    };

    filtrosPersonalActivos =
        Object.values(filtros).some(x => x !== null && x !== "");

    actualizarEstadoFiltrosPersonal();

    const response = await fetch(`/Personal/ListaFiltrada`, {
        method: "POST",
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(filtros)
    });

    if (!response.ok)
        return errorModal("Error aplicando filtros");

    const data = await response.json();

    gridPersonal.clear().rows.add(data).draw();
    actualizarKpis(data);
}

function limpiarFiltrosPersonal() {

    $("#fNombre").val("");

    $("#fPais").val(null).trigger("change");
    $("#fTipoDocumento").val(null).trigger("change");
    $("#fCondicionIva").val(null).trigger("change");
    $("#fRol").val(null).trigger("change");
    $("#fArtista").val(null).trigger("change");

    filtrosPersonalActivos = false;
    actualizarEstadoFiltrosPersonal();

    listaPersonal();
}

function actualizarEstadoFiltrosPersonal() {

    const txt = filtrosPersonalActivos
        ? "Filtros activos"
        : "";

    $("#txtFiltrosEstadoPersonal").text(txt);
}