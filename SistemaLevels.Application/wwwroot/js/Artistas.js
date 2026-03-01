let gridArtistas;

let exportTipo = null;

/**
 * Columnas:
 * 0 Acciones
 * 1 Nombre (TEXT)
 * 2 NombreArtistico (TEXT)
 * 3 Productora (SELECT2 remoto)
 * 4 Representante (SELECT2 remoto)
 * 5 País (SELECT2 remoto)
 * 6 Provincia (SELECT2 remoto)
 * 7 Tipo Doc (SELECT2 remoto)
 * 8 Nro Doc (TEXT)
 * 9 Condición IVA (SELECT2 remoto)
 * 10 Moneda (SELECT2 remoto)
 * 11 Precio (TEXT)
 * 12 Teléfono (TEXT)
 * 13 Email (TEXT)
 */
const columnConfig = [
    { index: 1, filterType: 'text' },
    { index: 2, filterType: 'text' },
    { index: 3, filterType: 'select', fetchDataFunc: listaProductorasFilter },
    { index: 4, filterType: 'select', fetchDataFunc: listaRepresentantesFilter },
    { index: 5, filterType: 'select', fetchDataFunc: listaPaisesFilter },
    { index: 6, filterType: 'select', fetchDataFunc: listaProvinciasFilter },
    { index: 7, filterType: 'select', fetchDataFunc: listaTiposDocumentoFilter },
    { index: 8, filterType: 'text' },
    { index: 9, filterType: 'select', fetchDataFunc: listaCondicionesIvaFilter },
    { index: 10, filterType: 'select', fetchDataFunc: listaMonedasFilter },
    { index: 11, filterType: 'text' },
    { index: 12, filterType: 'text' },
    { index: 13, filterType: 'text' },
];

$(document).ready(() => {

    listaArtistas();

    // Validación campo a campo (igual patrón)
    document.querySelectorAll("#modalEdicion input, #modalEdicion select, #modalEdicion textarea").forEach(el => {
        el.setAttribute("autocomplete", "off");
        el.addEventListener("input", () => validarCampoIndividual(el));
        el.addEventListener("change", () => validarCampoIndividual(el));
        el.addEventListener("blur", () => validarCampoIndividual(el));
    });

    // País -> depende tipo doc + condición IVA + provincias
    $("#cmbPais").on("change", async function () {
        const idPais = $(this).val();

        const tipoDocActual = $("#cmbTipoDocumento").val();
        const ivaActual = $("#cmbCondicionIva").val();
        const provinciaActual = $("#cmbProvincia").val();
        const monedaActual = $("#cmbMoneda").val();

        await listaTiposDocumento(idPais);
        await listaCondicionesIva(idPais);
        await listaProvincias(idPais);
     
        if (tipoDocActual) {
            $("#cmbTipoDocumento").val(tipoDocActual);
            refreshSelect2("#cmbTipoDocumento");
        }

        if (ivaActual) {
            $("#cmbCondicionIva").val(ivaActual);
            refreshSelect2("#cmbCondicionIva");
        }

        if (provinciaActual) {
            $("#cmbProvincia").val(provinciaActual);
            refreshSelect2("#cmbProvincia");
        }

    });



    // Inicializa select2 modal (dropdownParent modal)
    

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

    // si ya existe select2, no lo toques
    if ($el.data('select2')) return;

    $el.select2(Object.assign({
        width: '100%',
        allowClear: true,
        placeholder: "Seleccionar"
    }, options || {}));
}

function inicializarSelect2Modal() {

    const $modal = $('#modalEdicion');

    $modal.find("select").each(function () {

        const $el = $(this);

        if ($el.data('select2')) return;

        $el.select2({
            width: '100%',
            dropdownParent: $modal,
            allowClear: true,
            placeholder: "Seleccionar"
        });
    });
}



$('#modalEdicion').on('shown.bs.modal', function () {
    inicializarSelect2Modal();
});


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

function guardarArtista() {
    if (!validarCampos()) return false;

    const id = $("#txtId").val();

    const modelo = {
        Id: id !== "" ? parseInt(id, 10) : 0,

        Nombre: $("#txtNombre").val(),
        NombreArtistico: $("#txtNombreArtistico").val(),

        IdProductora: $("#cmbProductora").val() ? parseInt($("#cmbProductora").val(), 10) : null,
        IdRepresentante: $("#cmbRepresentante").val() ? parseInt($("#cmbRepresentante").val(), 10) : null,

        IdPais: $("#cmbPais").val() ? parseInt($("#cmbPais").val(), 10) : null,
        IdProvincia: $("#cmbProvincia").val() ? parseInt($("#cmbProvincia").val(), 10) : null,

        IdTipoDocumento: $("#cmbTipoDocumento").val() ? parseInt($("#cmbTipoDocumento").val(), 10) : null,
        NumeroDocumento: $("#txtNumeroDocumento").val(),

        IdCondicionIva: $("#cmbCondicionIva").val() ? parseInt($("#cmbCondicionIva").val(), 10) : null,

        Dni: $("#txtDni").val(),
        Telefono: $("#txtTelefono").val(),
        TelefonoAlternativo: $("#txtTelefonoAlternativo").val(),

        Email: $("#txtEmail").val(),

        Direccion: $("#txtDireccion").val(),
        Localidad: $("#txtLocalidad").val(),
        EntreCalles: $("#txtEntreCalles").val(),
        CodigoPostal: $("#txtCodigoPostal").val(),

        FechaNacimiento: $("#dtpFechaNacimiento").val() ? $("#dtpFechaNacimiento").val() : null,

        IdMoneda: $("#cmbMoneda").val() ? parseInt($("#cmbMoneda").val(), 10) : null,
        PrecioUnitario: toDecimal($("#txtPrecioUnitario").val()),
        PrecioNegMax: toDecimal($("#txtPrecioNegMax").val()),
        PrecioNegMin: toDecimal($("#txtPrecioNegMin").val()),
    };

    const url = id === "" ? "/Artistas/Insertar" : "/Artistas/Actualizar";
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
        .then(data => {

            if (!data.valor) {

                mostrarErrorCampos(
                    data.mensaje,
                    data.idReferencia,
                    data.tipo
                );

                return;
            }

            cerrarErrorCampos();

            $('#modalEdicion').modal('hide');

            exitoModal(data.mensaje);

            listaArtistas();
        })
        .catch(() => {
            mostrarErrorCampos("Error inesperado.");
        });
}

function nuevaArtista() {
    limpiarModal();

    setModalSoloLectura(false); 

    Promise.all([
        listaProductoras(),
        listaRepresentantes(),
        listaPaises(),
        listaMonedas()
    ])
        .then(() => {
            resetSelect("cmbProvincia", "Seleccionar");
            resetSelect("cmbTipoDocumento", "Seleccionar");
            resetSelect("cmbCondicionIva", "Seleccionar");
            
        });

    abrirModalEdicion();

    $("#btnGuardar").html(`<i class="fa fa-check"></i> Registrar`);
    $("#modalEdicionLabel").text("Nuevo Artista");

    $("#infoAuditoria").addClass("d-none");
    $("#infoRegistro").html("");
    $("#infoModificacion").html("");
}


async function mostrarModal(modelo) {
    limpiarModal();

    setModalSoloLectura(false);

    $("#txtId").val(modelo.Id || "");

    $("#txtNombre").val(modelo.Nombre || "");
    $("#txtNombreArtistico").val(modelo.NombreArtistico || "");

    $("#txtTelefono").val(modelo.Telefono || "");
    $("#txtTelefonoAlternativo").val(modelo.TelefonoAlternativo || "");
    $("#txtDni").val(modelo.Dni || "");

    $("#txtNumeroDocumento").val(modelo.NumeroDocumento || "");
    $("#txtEmail").val(modelo.Email || "");

    $("#txtDireccion").val(modelo.Direccion || "");
    $("#txtLocalidad").val(modelo.Localidad || "");
    $("#txtEntreCalles").val(modelo.EntreCalles || "");
    $("#txtCodigoPostal").val(modelo.CodigoPostal || "");

    $("#dtpFechaNacimiento").val(normalizarDateInput(modelo.FechaNacimiento));

    $("#txtPrecioUnitario").val(modelo.PrecioUnitario ?? "");
    $("#txtPrecioNegMax").val(modelo.PrecioNegMax ?? "");
    $("#txtPrecioNegMin").val(modelo.PrecioNegMin ?? "");

    await listaProductoras();
    await listaRepresentantes();
    await listaPaises();

    // primero setear país
    if (modelo.IdPais != null) {
        $("#cmbPais").val(modelo.IdPais).trigger("change.select2");

        await listaTiposDocumento(modelo.IdPais);
        await listaCondicionesIva(modelo.IdPais);
        await listaProvincias(modelo.IdPais);
        await listaMonedas();
    } else {
        resetSelect("cmbTipoDocumento", "Seleccionar");
        resetSelect("cmbCondicionIva", "Seleccionar");
        resetSelect("cmbProvincia", "Seleccionar");
        resetSelect("cmbMoneda", "Seleccionar");
    }


    // Set combos
    if (modelo.IdProductora != null) $("#cmbProductora").val(modelo.IdProductora).trigger("change.select2");
    if (modelo.IdRepresentante != null) $("#cmbRepresentante").val(modelo.IdRepresentante).trigger("change.select2");

    if (modelo.IdPais != null) {
        $("#cmbPais").val(modelo.IdPais).trigger("change.select2");
        await listaTiposDocumento(modelo.IdPais);
        await listaCondicionesIva(modelo.IdPais);
    } else {
        resetSelect("cmbTipoDocumento", "Seleccionar");
        resetSelect("cmbCondicionIva", "Seleccionar");
    }

    if (modelo.IdTipoDocumento != null) $("#cmbTipoDocumento").val(modelo.IdTipoDocumento).trigger("change.select2");
    if (modelo.IdCondicionIva != null) $("#cmbCondicionIva").val(modelo.IdCondicionIva).trigger("change.select2");
    if (modelo.IdProvincia != null) $("#cmbProvincia").val(modelo.IdProvincia).trigger("change.select2");
    if (modelo.IdMoneda != null) $("#cmbMoneda").val(modelo.IdMoneda).trigger("change.select2");

    

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

    abrirModalEdicion();


    $("#btnGuardar").html(`<i class="fa fa-check"></i> Guardar`);
    $("#modalEdicionLabel").text("Editar Artista");
}

const editarArtista = id => {

    fetch("/Artistas/EditarInfo?id=" + id, {
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

async function eliminarArtista(id) {

    const confirmado = await confirmarModal("¿Desea eliminar este artista?");
    if (!confirmado) return;

    try {
        const response = await fetch("/Artistas/Eliminar?id=" + id, {
            method: "DELETE",
            headers: {
                'Authorization': 'Bearer ' + token,
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) throw new Error("Error al eliminar el Artista.");

        const dataJson = await response.json();
        if (dataJson.valor) {
            listaArtistas();
            exitoModal("Artista eliminado correctamente");
        } else {
            errorModal("No se pudo eliminar.");
        }
    } catch (e) {
        console.error("Ha ocurrido un error:", e);
        errorModal("Ha ocurrido un error.");
    }
}

/* =========================
   LISTA + DATATABLE
========================= */

async function listaArtistas() {
    let paginaActual = gridArtistas != null ? gridArtistas.page() : 0;

    const response = await fetch(`/Artistas/Lista`, {
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
        gridArtistas.page(paginaActual).draw('page');
    }
}

async function configurarDataTable(data) {

    if (!gridArtistas) {

        const $thead = $('#grd_Artistas thead');
        if ($thead.find('tr.filters').length === 0) {
            $thead.find('tr').first().clone(true).addClass('filters').appendTo($thead);
        }

        gridArtistas = $('#grd_Artistas').DataTable({
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
                            ver: "verFicha",
                            editar: "editarArtista",
                            eliminar: "eliminarArtista"
                        });
                    },
                    orderable: false,
                    searchable: false,
                },
                { data: 'Nombre' },
                { data: 'NombreArtistico' },
                { data: 'Productora' },
                { data: 'Representante' },
                { data: 'Pais' },
                { data: 'Provincia' },
                { data: 'TipoDocumento' },
                { data: 'NumeroDocumento' },
                { data: 'CondicionIva' },
                { data: 'Moneda' },
                {
                    data: 'PrecioUnitario',
                    render: function (data) {
                        if (data == null || data === "") return "";
                        const n = Number(data);
                        if (Number.isNaN(n)) return data;
                        return n.toLocaleString("es-AR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                    }
                },
                { data: 'Telefono' },
                { data: 'Email' },
            ],

            dom: 'Bfrtip',
            buttons: [
                {
                    text: 'Excel',
                    action: () => abrirModalExportacion(gridArtistas, 'excel', 'Artistas')
                },
                {
                    text: 'PDF',
                    action: () => abrirModalExportacion(gridArtistas, 'pdf', 'Artistas')
                },
                {
                    text: 'Imprimir',
                    action: () => abrirModalExportacion(gridArtistas, 'print', 'Artistas')
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

                        // ✅ al limpiar, limpiar filtro SIEMPRE
                        $select.on('select2:clear', function () {
                            api.column(config.index).search('').draw(false);
                        });

                        // ✅ cambio normal (incluye value vacío)
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

             

            }
        });

    } else {
        gridArtistas.clear().rows.add(data).draw();
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



async function listaProductoras() {
    const response = await fetch(`/Productoras/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    const data = await response.json();

    resetSelect("cmbProductora", "Seleccionar");
    const select = document.getElementById("cmbProductora");
    (data || []).forEach(x => select.append(new Option(x.Nombre, x.Id)));

    
}

async function listaRepresentantes() {
    const response = await fetch(`/Representantes/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    const data = await response.json();

    resetSelect("cmbRepresentante", "Seleccionar");
    const select = document.getElementById("cmbRepresentante");
    (data || []).forEach(x => select.append(new Option(x.Nombre, x.Id)));

    
}

async function listaMonedas() {

    resetSelect("cmbMoneda", "Seleccionar");

    const response = await fetch(`/PaisesMoneda/Lista`, {
        headers: { 'Authorization': 'Bearer ' + token }
    });

    const data = await response.json();
    const select = document.getElementById("cmbMoneda");

    (data || []).forEach(x => {
        select.append(new Option(x.Nombre, x.Id));
    });
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

async function listaProductorasFilter() {
    const response = await fetch(`/Productoras/Lista`, { headers: { 'Authorization': 'Bearer ' + token } });
    const data = await response.json();
    return (data || []).map(x => ({ Id: x.Id, Nombre: x.Nombre }));
}

async function listaRepresentantesFilter() {
    const response = await fetch(`/Representantes/Lista`, { headers: { 'Authorization': 'Bearer ' + token } });
    const data = await response.json();
    return (data || []).map(x => ({ Id: x.Id, Nombre: x.Nombre }));
}

async function listaMonedasFilter() {
    const response = await fetch(`/PaisesMoneda/Lista`, { headers: { 'Authorization': 'Bearer ' + token } });
    const data = await response.json();
    return (data || []).map(x => ({ Id: x.Id, Nombre: x.Nombre }));
}

async function listaPaisesFilter() {
    const response = await fetch(`/Paises/Lista`, { headers: { 'Authorization': 'Bearer ' + token } });
    return await response.json();
}

async function listaTiposDocumentoFilter() {
    const response = await fetch(`/PaisesTiposDocumentos/Lista`, { headers: { 'Authorization': 'Bearer ' + token } });
    const data = await response.json();
    return (data || []).map(x => ({ Id: x.Id, Nombre: x.Nombre }));
}

async function listaCondicionesIvaFilter() {
    const response = await fetch(`/PaisesCondicionesIVA/Lista`, { headers: { 'Authorization': 'Bearer ' + token } });
    const data = await response.json();
    return (data || []).map(x => ({ Id: x.Id, Nombre: x.Nombre }));
}

async function listaProvinciasFilter() {
    const response = await fetch(`/PaisesProvincia/Lista`, { headers: { 'Authorization': 'Bearer ' + token } });
    const data = await response.json();
    return (data || []).map(x => ({ Id: x.Id, Nombre: x.Nombre, IdPais: x.IdCombo }));
}

/* =========================
   CONFIG COLUMNAS
========================= */

function configurarOpcionesColumnas() {
    const grid = $('#grd_Artistas').DataTable();
    const columnas = grid.settings().init().columns;
    const container = $('#configColumnasMenu');

    const storageKey = `Artistas_Columnas`;
    const savedConfig = JSON.parse(localStorage.getItem(storageKey)) || {};

    container.empty();

    columnas.forEach((col, index) => {
        if (col.data && col.data !== "Id") {

            const isChecked = savedConfig[`col_${index}`] !== undefined ? savedConfig[`col_${index}`] : true;
            grid.column(index).visible(isChecked);

            const name = $('#grd_Artistas thead tr').first().find('th').eq(index).text();

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

    // modo solo lectura
    if (document.querySelector("#modalEdicion")
        ?.getAttribute("data-sololectura") === "1")
        return true;

    const camposObligatorios = [
        "txtNombre",
        "txtNombreArtistico",
        "txtNumeroDocumento",
        "txtDni",
        "cmbPais"
    ];

    if (!camposObligatorios.includes(el.id))
        return;

    const valor = (el.value ?? "").toString().trim();

    const esValido =
        valor !== "" &&
        valor !== null &&
        valor !== "Seleccionar";

    setEstadoCampo(el, esValido);

    verificarErroresGenerales();
}
function verificarErroresGenerales() {
    const errorMsg = document.getElementById("errorCampos");
    if (!errorMsg) return;

    const hayInvalidos = document.querySelectorAll("#modalEdicion .is-invalid").length > 0;
    if (!hayInvalidos) errorMsg.classList.add("d-none");
}

function validarCampos() {

    // modo ver
    if (document.querySelector("#modalEdicion")
        ?.getAttribute("data-sololectura") === "1")
        return true;

    const campos = [
        { selector: "#txtNombre", nombre: "Nombre completo" },
        { selector: "#txtNombreArtistico", nombre: "Nombre artístico" },
        { selector: "#cmbPais", nombre: "Pais" },
        { selector: "#txtNumeroDocumento", nombre: "Número documento" },
        { selector: "#txtDni", nombre: "DNI" }
    ];

    let errores = [];

    campos.forEach(c => {

        const el = document.querySelector(c.selector);
        if (!el) return;

        const valor = (el.value ?? "").toString().trim();

        const esValido =
            valor !== "" &&
            valor !== null &&
            valor !== "Seleccionar";

        setEstadoCampo(el, esValido);

        if (!esValido)
            errores.push(c.nombre);
    });

    if (errores.length > 0) {

        mostrarErrorCampos(
            `Debes completar los campos requeridos:<br>
             <strong>${errores.join(", ")}</strong>`,
            null,
            "validacion"
        );

        return false;
    }

    cerrarErrorCampos();
    return true;
}
/* =========================
   KPI + helpers
========================= */

function actualizarKpis(data) {
    const cant = Array.isArray(data) ? data.length : 0;
    $('#kpiCantArtistas').text(cant);
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

function toDecimal(val) {
    if (val == null) return 0;
    const s = (val + "").trim();
    if (!s) return 0;
    const n = Number(s.replace(",", "."));
    return Number.isFinite(n) ? n : 0;
}

function normalizarDateInput(fecha) {
    if (!fecha) return "";
    try {
        const d = new Date(fecha);
        if (isNaN(d.getTime())) return "";
        // yyyy-MM-dd
        const yyyy = d.getFullYear();
        const mm = String(d.getMonth() + 1).padStart(2, '0');
        const dd = String(d.getDate()).padStart(2, '0');
        return `${yyyy}-${mm}-${dd}`;
    } catch {
        return "";
    }
}


function refreshSelect2(id) {
    const $el = $(id);
    if ($el.data('select2')) {
        $el.trigger('change.select2');
    }
}



const verFicha = id => {

    fetch("/Artistas/EditarInfo?id=" + id, {
        method: 'GET',
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        }
    })
        .then(r => r.json())
        .then(async dataJson => {

            if (!dataJson) throw new Error();

            await mostrarModal(dataJson);

            abrirModalEdicion();   // ✅ abrir UNA sola vez

            setModalSoloLectura(true);

            $("#modalEdicionLabel").text("Ver Artista");
        })
        .catch(_ => errorModal("Ha ocurrido un error."));
};



$('#modalEdicion').on('hidden.bs.modal', function () {

    // eliminar backdrops huérfanos
    $('.modal-backdrop').remove();

    // restaurar scroll
    $('body').removeClass('modal-open');
    $('body').css('padding-right', '');
});