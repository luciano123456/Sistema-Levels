let monedas = [];
let paisesCache = [];

$(document).ready(() => {

    $(document).off("click.select2fix").on(
        "click.select2fix",
        ".select2-container--default .select2-selection--single",
        function () {
            const $select = $(this).closest(".select2-container").prev("select");
            if ($select.length) {
                if ($select.data("select2") && $select.data("select2").isOpen()) return;
                $select.select2("open");
            }
        }
    );

    document.querySelectorAll("#modalEdicion input, #modalEdicion select").forEach(el => {
        el.setAttribute("autocomplete", "off");
        el.addEventListener("input", () => validarCampoIndividual(el));
        el.addEventListener("change", () => validarCampoIndividual(el));
        el.addEventListener("blur", () => validarCampoIndividual(el));
    });

    $("#modalEdicion").on("select2:select select2:clear change", "select", function () {
        validarCampoIndividual(this);
    });

    cargarPaises().then(() => {
        inicializarSelect2Modal();
    });

    listarMonedas();
});

/* SELECT2 */

function ensureSelect2($el, options) {
    if (!$el || !$el.length) return;

    if ($el.data('select2')) {
        $el.select2('destroy');
    }

    $el.select2(Object.assign({
        width: '100%',
        allowClear: true,
        placeholder: "Seleccionar"
    }, options || {}));
}

function inicializarSelect2Modal() {
    const opts = {
        width: '100%',
        dropdownParent: $('#modalEdicion')
    };

    ensureSelect2($("#cmbPais"), opts);
}

/* LISTA */

async function listarMonedas() {
    const resp = await fetch("/PaisesMoneda/Lista", {
        method: "GET",
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        }
    });

    monedas = await resp.json();

    const cont = document.getElementById("contenedorMonedas");
    cont.innerHTML = "";

    (monedas || []).forEach(m => {
        const cot = Number(m.Cotizacion ?? 0);

        cont.innerHTML += `
            <div class="moneda-card">
                <div class="moneda-acciones">
                    <button class="btn-card" onclick="editarMoneda(${m.Id})">
                        <i class="fa fa-pencil"></i>
                    </button>
                    <button class="btn-card" onclick="eliminarMoneda(${m.Id})">
                        <i class="fa fa-trash"></i>
                    </button>
                </div>

                <div>
                    <div class="moneda-icon"><i class="fa fa-money"></i></div>
                    <div class="moneda-nombre">${m.Nombre}</div>
                    <div class="moneda-pais">${m.Pais}</div>
                </div>

                <div class="moneda-cotizacion">
                    ${cot.toLocaleString("es-AR", { minimumFractionDigits: 2 })}
                </div>
            </div>
        `;
    });
}

/* COMBOS */

async function cargarPaises() {
    const resp = await fetch("/Paises/Lista", {
        headers: {
            'Authorization': 'Bearer ' + token
        }
    });

    paisesCache = await resp.json();

    const cmb = document.getElementById("cmbPais");
    cmb.innerHTML = "";
    cmb.append(new Option("Seleccionar", "", true, true));

    (paisesCache || []).forEach(p => {
        cmb.append(new Option(p.Nombre, p.Id));
    });
}

/* CRUD */
function nuevaMoneda() {

    const form = document.querySelector("#modalEdicion");

    // limpiar valores
    $("#txtId").val("");
    $("#txtNombre").val("");
    $("#txtCotizacion").val("");
    $("#cmbPais").val("").trigger("change");

    limpiarValidaciones();

    // 🔴 limpiar mensaje general
    $("#errorCampos").addClass("d-none");

    $("#tituloModal").text("Nueva moneda");
    $("#btnGuardar").html(`<i class="fa fa-check"></i> Registrar`);

    $('#modalEdicion').modal('show');
}


function limpiarValidaciones() {
    const form = document.querySelector("#modalEdicion");

    form.querySelectorAll("input, select").forEach(el => {
        el.classList.remove("is-valid", "is-invalid");

        const wrap = el.closest(".mb-3");
        if (wrap) {
            const msg = wrap.querySelector(".invalid-feedback");
            if (msg) msg.classList.add("d-none");
        }
    });
}



function editarMoneda(id) {
    const m = monedas.find(x => x.Id === id);
    if (!m) return;

    limpiarModal();

    $("#txtId").val(m.Id);
    $("#txtNombre").val(m.Nombre);
    $("#txtCotizacion").val(m.Cotizacion);
    $("#cmbPais").val(m.IdPais).trigger("change.select2");

    $("#modalEdicionLabel").text("Editar moneda");
    $("#btnGuardar").html(`<i class="fa fa-check"></i> Guardar`);

    $("#modalEdicion").modal("show");
}

async function guardarMoneda() {
    if (!validarCampos()) return;

    const id = $("#txtId").val();

    const modelo = {
        Id: id ? parseInt(id) : 0,
        IdPais: parseInt($("#cmbPais").val()),
        Nombre: $("#txtNombre").val(),
        Cotizacion: toDecimal($("#txtCotizacion").val())
    };

    const url = id ? "/PaisesMoneda/Actualizar" : "/PaisesMoneda/Insertar";
    const method = id ? "PUT" : "POST";

    const resp = await fetch(url, {
        method,
        headers: {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(modelo)
    });

    const json = await resp.json();

    if (json.valor) {
        $("#modalEdicion").modal("hide");
        listarMonedas();
        exitoModal("Operación correcta");
    } else {
        errorModal("No se pudo guardar");
    }
}

async function eliminarMoneda(id) {
    const ok = await confirmarModal("¿Desea eliminar esta moneda?");
    if (!ok) return;

    const resp = await fetch("/PaisesMoneda/Eliminar?id=" + id, {
        method: "DELETE",
        headers: { 'Authorization': 'Bearer ' + token }
    });

    const json = await resp.json();

    if (json.valor) {
        listarMonedas();
        exitoModal("Moneda eliminada");
    }
}

/* VALIDACIONES */

function validarCampoIndividual(el) {
    const obligatorios = ["cmbPais", "txtNombre", "txtCotizacion"];
    if (!obligatorios.includes(el.id)) return;

    const valor = el.value.trim();
    setEstadoCampo(el, valor !== "");
}

function validarCampos() {
    let valido = true;

    ["#cmbPais", "#txtNombre", "#txtCotizacion"].forEach(sel => {
        const el = document.querySelector(sel);
        const ok = el.value.trim() !== "";
        setEstadoCampo(el, ok);
        if (!ok) valido = false;
    });

    $("#errorCampos").toggleClass("d-none", valido);
    return valido;
}

function setEstadoCampo(el, esValido) {
    el.classList.toggle("is-invalid", !esValido);
    el.classList.toggle("is-valid", esValido);
}

function limpiarModal() {
    $("#txtId").val("");
    $("#txtNombre").val("");
    $("#txtCotizacion").val("");
    $("#cmbPais").val("").trigger("change.select2");

    $("#errorCampos").addClass("d-none");
}

/* HELPERS */

function toDecimal(val) {
    if (!val) return 0;
    const n = Number(val.replace(",", "."));
    return isNaN(n) ? 0 : n;
}
