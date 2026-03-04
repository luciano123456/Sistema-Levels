/* =========================================================
   VentasNuevoModif.js — NUEVO / MODIFICAR (Pantalla PRO sin listas)
   - Cliente por Select2 (lista completa /Ventas/ListaClientes)
   - Tabs: Artistas / Personal / Cobros / Notas / Auditoría
   - Comisiones:
       * Artistas: % o total fijo (si editás total, recalcula %)
       * Personal: tipo_comision:
           - IdTipoComision=1 => % (calcula total)
           - IdTipoComision=2 => fijo (total fijo; % se calcula informativo)
   - Totales live:
       ImporteTotal - TotalComisiones - Cobrado(Conversion) = Saldo
   - Draft PRO (autosave + restore)
   - Compatible con tu Controller/VM/Repository
========================================================= */

(function () {
    "use strict";

    /* =========================
       STATE
    ========================= */
    const VN = {
        init: window.VN_INIT || { id: 0, idCliente: 0 },
        clientes: [],
        combos: {
            productoras: [],
            ubicaciones: [],
            monedas: [],
            estados: [],
            tiposContrato: [],
            opcionesBinarias: [],
            artistas: [],
            representantes: [],
            personal: [],
            cargos: [],
            tiposComision: [],
            cuentas: []
        },
        detalle: {
            artistas: [],
            personal: [],
            cobros: []
        },
        flags: {
            ready: false,
            restoringDraft: false,
            dirty: false,
            autosaveEnabled: true
        }
    };

    /* =========================
       ENDPOINTS (según tu proyecto)
    ========================= */
    const API = {
        // VentasController
        
        editarInfo: (id) => `/Ventas/EditarInfo?id=${id}`,
        insertar: "/Ventas/Insertar",
        actualizar: "/Ventas/Actualizar",

        // Combos
        productoras: "/Productoras/Lista",
        listaClientes: "/Ventas/ListaClientes",
        ubicaciones: "/Ubicaciones/Lista",
        monedas: "/PaisesMoneda/Lista",
        estadosVenta: "/VentasEstados/Lista",
        tiposContrato: "/TiposContratos/Lista",
        opcionesBinarias: "/OpcionesBinarias/Lista",
        artistas: "/Artistas/Lista",
        representantes: "/Personal/Lista",
        personal: "/Personal/Lista",
        cargos: "/PersonalRol/Lista",
        tiposComision: "/TiposComisiones/Lista",
        cuentas: "/MonedasCuentas/Lista",
        cuentasPorMoneda: (id) => `/MonedasCuenta/ListaMoneda?idMoneda=${id}`,
    };

    /* =========================
       AUTH HEADERS (token global)
    ========================= */
    const authHeaders = () => ({
        "Authorization": "Bearer " + (token || ""),
        "Content-Type": "application/json"
    });

    /* =========================
       DRAFT
    ========================= */
    const DRAFT_KEY = () => {
        const userId = localStorage.getItem("userId") || "default";
        return `VN_DRAFT_${userId}`;
    };

    /* =========================
       HELPERS
    ========================= */
    function $(sel) { return window.jQuery ? window.jQuery(sel) : null; }

    function vnToastOk(msg) {
        if (typeof window.exitoModal === "function") window.exitoModal(msg);
        else alert(msg);
    }
    function vnToastErr(msg) {
        if (typeof window.errorModal === "function") window.errorModal(msg);
        else alert(msg);
    }
    async function vnConfirm(msg) {
        if (typeof window.confirmarModal === "function") return await window.confirmarModal(msg);
        return confirm(msg);
    }

    function vnToNumber(v) {
        if (v == null) return 0;
        const s = String(v).trim();
        if (!s) return 0;
        const cleaned = s.replace(/[^\d,-]/g, "").replace(/\./g, "").replace(",", ".");
        const n = Number(cleaned);
        return Number.isFinite(n) ? n : 0;
    }

    function vnRound2(n) {
        const x = Number(n || 0);
        return Math.round(x * 100) / 100;
    }

    function vnFmtMoneyARS(n) {
        try {
            const v = vnRound2(Number(n || 0));
            return v.toLocaleString("es-AR", { style: "currency", currency: "ARS", minimumFractionDigits: 0, maximumFractionDigits: 0 });
        } catch { return "$ 0"; }
    }

    function vnIsoLocalFromDate(d) {
        try {
            const dt = (d instanceof Date) ? d : new Date(d);
            if (isNaN(dt.getTime())) return "";
            const pad = x => String(x).padStart(2, "0");
            return `${dt.getFullYear()}-${pad(dt.getMonth() + 1)}-${pad(dt.getDate())}T${pad(dt.getHours())}:${pad(dt.getMinutes())}`;
        } catch { return ""; }
    }
    function vnNowIsoLocal() {
        return vnIsoLocalFromDate(new Date());
    }

    async function vnFetchJson(url) {
        const r = await fetch(url, { method: "GET", headers: authHeaders() });
        if (!r.ok) throw new Error(r.statusText);
        return await r.json();
    }

    function vnSetSaving(state, text, mode) {
        // reuse ids from tu HTML si los agregás luego; acá usamos summary como feedback mínimo
        // (si querés barra, agregala al HTML: #spinSaving, #txtEstadoGuardado)
        const spin = document.getElementById("spinSaving");
        const lbl = document.getElementById("txtEstadoGuardado");
        const status = document.getElementById("lblEstadoGuardado");

        if (spin) spin.classList.toggle("d-none", !state);
        if (lbl) lbl.textContent = text || (state ? "Guardando..." : "Listo");
        if (status) {
            status.classList.remove("ok", "err");
            if (mode === "ok") status.classList.add("ok");
            if (mode === "err") status.classList.add("err");
        }
    }

    async function cargarCuentasPorMoneda(idx, idMoneda) {

        const sel = document.querySelector(`select.vn-c-cuenta[data-idx="${idx}"]`);
        if (!sel) return;

        if (!idMoneda) {
            vnFillSelectDom(sel, [], "Id", "Nombre", "Seleccionar");
            return;
        }

        try {

            const cuentas = await vnFetchJson(API.cuentasPorMoneda(idMoneda));

            vnFillSelectDom(sel, cuentas, "Id", "Nombre", "Seleccionar");

            $(sel)?.select2({
                width: "100%",
                allowClear: true,
                placeholder: "Cuenta"
            });

        }
        catch (e) {
            console.error("Error cargando cuentas", e);
        }
    }

    function vnMarkDirty() {
        if (VN.flags.restoringDraft) return;
        VN.flags.dirty = true;
    }

    function recalcularComisionesPorImporte() {

        const total = Number(document.getElementById("ImporteTotal")?.value || 0);

        // ARTISTAS
        VN.detalle.artistas.forEach((a, idx) => {

            if (a.PorcComision > 0) {

                a.TotalComision = vnRound2(total * (a.PorcComision / 100));

                const inp = document.querySelector(`input.vn-a-total[data-idx="${idx}"]`);
                if (inp) inp.value = a.TotalComision;

            } else {

                const val = Number(a.TotalComision || 0);

                a.PorcComision = total > 0
                    ? vnRound2((val / total) * 100)
                    : 0;

                const inp = document.querySelector(`input.vn-a-porc[data-idx="${idx}"]`);
                if (inp) inp.value = a.PorcComision;

            }

        });


        // PERSONAL
        VN.detalle.personal.forEach((p, idx) => {

            const tipo = Number(p.IdTipoComision || 0);

            if (tipo === 1) {

                // porcentaje
                p.TotalComision = vnRound2(total * (p.PorcComision / 100));

                const inp = document.querySelector(`input.vn-p-total[data-idx="${idx}"]`);
                if (inp) inp.value = p.TotalComision;

            }

            if (tipo === 2) {

                // fijo → recalcular %
                const val = Number(p.TotalComision || 0);

                p.PorcComision = total > 0
                    ? vnRound2((val / total) * 100)
                    : 0;

                const inp = document.querySelector(`input.vn-p-porc[data-idx="${idx}"]`);
                if (inp) inp.value = p.PorcComision;

            }

        });

    }

    function ensureSelect2(domSel, options) {
        const jq = $(domSel);
        if (!jq || !jq.length) return;
        if (jq.data("select2")) jq.select2("destroy");

        jq.select2(Object.assign({
            width: "100%",
            allowClear: true,
            placeholder: "Seleccionar"
        }, options || {}));
    }

    function vnFillSelectDom(el, data, idField, textField, placeholder) {
        if (!el) return;
        el.innerHTML = "";
        el.append(new Option(placeholder || "Seleccionar", "", true, true));
        (data || []).forEach(x => {
            const val = x[idField] ?? x.Id;
            const txt = x[textField] ?? x.Nombre ?? x.Descripcion ?? "";
            el.append(new Option(txt, val));
        });
    }

    function vnGetImporteTotal() {
        return vnToNumber(document.getElementById("ImporteTotal")?.value);
    }

    function vnGetClienteId() {
        const el = document.getElementById("IdCliente");
        return Number(el?.value || 0);
    }

    /* =========================
       INIT
    ========================= */
    document.addEventListener("DOMContentLoaded", async () => {
        try {
            initDuracionMask();
            initSecciones()
            bindUI();


            await cargarCombosBase();
            await cargarClientes();

            initSelectsBasicos();

            // defaults
            setDefaultsNueva();

            // si viene idCliente
            if (Number(VN.init.idCliente || 0) > 0) {
                setClienteSeleccionado(Number(VN.init.idCliente), true);
            }

            // si viene id (editar)
            const idVenta = Number(VN.init.id || document.getElementById("Venta_Id")?.value || 0);
            if (idVenta > 0) {
                await abrirVenta(idVenta);
            } else {
                // nueva
                document.getElementById("Venta_Id").value = "0";

                renderDetalle();        // 👈 ESTO
                recalcularTotales();    // 👈 (recomendado)

                vnSetSaving(false, "Listo", "ok");
            }

            // autosave draft
            setInterval(() => {
                if (!VN.flags.autosaveEnabled) return;
                if (!VN.flags.ready) return;
                if (!VN.flags.dirty) return;
                guardarDraft(false);
            }, 6000);

            VN.flags.ready = true;
        } catch (e) {
            console.error(e);
            vnToastErr("Error inicializando la pantalla de ventas.");
        }
    });

    function bindUI() {
        const btnGuardar = document.getElementById("btnGuardar");
        const btnReset = document.getElementById("btnReset");
        const btnDraftRestore = document.getElementById("btnDraftRestore");
        const btnDraftClear = document.getElementById("btnDraftClear");

        const btnDraftSave = document.getElementById("btnDraftSave");

        btnDraftSave?.addEventListener("click", () => {
            guardarDraft(true);
        });


        btnGuardar?.addEventListener("click", () => guardarVenta());
        btnReset?.addEventListener("click", async () => {
            const ok = await vnConfirm("¿Reiniciar la venta? (no guarda cambios)");
            if (!ok) return;
            resetAll();
        });

        btnDraftRestore?.addEventListener("click", () => restaurarDraft());
        btnDraftClear?.addEventListener("click", async () => {
            const ok = await vnConfirm("¿Eliminar el borrador guardado?");
            if (!ok) return;
            localStorage.removeItem(DRAFT_KEY());
            vnToastOk("Borrador eliminado.");
        });

        // adds
        document.getElementById("btnAddArtista")?.addEventListener("click", () => {
            VN.detalle.artistas.push({
                Id: 0,
                IdArtista: 0,
                IdRepresentante: 0,
                PorcComision: 0,
                TotalComision: 0
            });
            renderDetalle();
            vnMarkDirty();
        });

        document.getElementById("btnAddPersonal")?.addEventListener("click", () => {
            VN.detalle.personal.push({
                Id: 0,
                IdPersonal: 0,
                IdCargo: 0,
                IdTipoComision: 0, // 1=% 2=fijo
                PorcComision: 0,
                TotalComision: 0
            });
            renderDetalle();
            vnMarkDirty();
        });

        document.getElementById("btnAddCobro")?.addEventListener("click", () => {
            VN.detalle.cobros.push({
                Id: 0,
                Fecha: new Date(),
                IdMoneda: Number(document.getElementById("IdMoneda")?.value || 0) || 0,
                IdCuenta: 0,
                Importe: 0,
                Cotizacion: 1,
                Conversion: 0,
                ManualConversion: false,
                NotaCliente: "",
                NotaInterna: ""
            });
            renderDetalle();
            recalcularTotales();
            vnMarkDirty();
        });

        document
            .querySelectorAll("input, select, textarea")
            .forEach(el => {

                el.addEventListener("input", () => validarCampoIndividual(el))
                el.addEventListener("change", () => validarCampoIndividual(el))
                el.addEventListener("blur", () => validarCampoIndividual(el))

            })

        // mark dirty on main inputs
        const watchIds = [
            "IdCliente", "Fecha", "NombreEvento", "Duracion",
            "IdUbicacion", "IdProductora", "IdMoneda", "IdEstado",
            "IdTipoContrato", "IdOpExclusividad", "DiasPrevios",
            "FechaHasta", "ImporteTotal", "NotaInterna", "NotaCliente"
        ];

        $(document).on("select2:select select2:clear change", "select", function () {
            validarCampoIndividual(this);
        });

        const imp = document.getElementById("ImporteTotal");

        imp?.addEventListener("input", () => {

            recalcularComisionesPorImporte();
            recalcularTotales();
            vnMarkDirty();

        });

        watchIds.forEach(id => {
            const el = document.getElementById(id);
            if (!el) return;
            el.addEventListener("input", () => { vnMarkDirty(); recalcularTotales(); });
            el.addEventListener("change", () => { vnMarkDirty(); recalcularTotales(); });
        });
    }

    /* =========================
       CARGAS BASE
    ========================= */
    async function cargarCombosBase() {
        const [
            productoras, ubicaciones, monedas, estados, tiposContrato, opcionesBinarias,
            artistas, representantes, personal, cargos, tiposComision, cuentas
        ] = await Promise.all([
            vnFetchJson(API.productoras).catch(_ => []),
            vnFetchJson(API.ubicaciones).catch(_ => []),
            vnFetchJson(API.monedas).catch(_ => []),
            vnFetchJson(API.estadosVenta).catch(_ => []),
            vnFetchJson(API.tiposContrato).catch(_ => []),
            vnFetchJson(API.opcionesBinarias).catch(_ => []),

            vnFetchJson(API.artistas).catch(_ => []),
            vnFetchJson(API.representantes).catch(_ => []),

            vnFetchJson(API.personal).catch(_ => []),
            vnFetchJson(API.cargos).catch(_ => []),
            vnFetchJson(API.tiposComision).catch(_ => []),

            vnFetchJson(API.cuentas).catch(_ => [])
        ]);

        VN.combos.productoras = productoras || [];
        VN.combos.ubicaciones = ubicaciones || [];
        VN.combos.monedas = monedas || [];
        VN.combos.estados = estados || [];
        VN.combos.tiposContrato = tiposContrato || [];
        VN.combos.opcionesBinarias = opcionesBinarias || [];

        VN.combos.artistas = artistas || [];
        VN.combos.representantes = representantes || [];

        VN.combos.personal = personal || [];
        VN.combos.cargos = cargos || [];
        VN.combos.tiposComision = tiposComision || [];

        VN.combos.cuentas = cuentas || [];
    }

    async function cargarClientes() {
        const data = await vnFetchJson(API.listaClientes).catch(_ => []);
        VN.clientes = Array.isArray(data) ? data : [];
    }

    /* =========================
       INIT SELECTS
    ========================= */
    function initSelectsBasicos() {
        // Cliente (Select2 local con búsqueda)
        const selCli = document.getElementById("IdCliente");
        vnFillSelectDom(selCli, VN.clientes, "Id", "Nombre", "Seleccionar");

        ensureSelect2("#IdCliente", {
            placeholder: "Seleccioná cliente",
            matcher: function (params, data) {
                if (!params.term) return data;
                if (!data || !data.text) return null;
                const term = params.term.toLowerCase();
                return data.text.toLowerCase().includes(term) ? data : null;
            }
        });

        // Combos Datos
        vnFillSelectDom(document.getElementById("IdUbicacion"), VN.combos.ubicaciones, "Id", "Descripcion", "Seleccionar");
        vnFillSelectDom(document.getElementById("IdProductora"), VN.combos.productoras, "Id", "Nombre", "Seleccionar");
        vnFillSelectDom(document.getElementById("IdMoneda"), VN.combos.monedas, "Id", "Nombre", "Seleccionar");
        vnFillSelectDom(document.getElementById("IdEstado"), VN.combos.estados, "Id", "Nombre", "Seleccionar");
        vnFillSelectDom(document.getElementById("IdTipoContrato"), VN.combos.tiposContrato, "Id", "Nombre", "Seleccionar");

        // Exclusividad: 1=SI 2=NO (tu regla)
        // Endpoint también puede devolverlo, pero lo rellenamos robusto:
        const opBin = VN.combos.opcionesBinarias?.length ? VN.combos.opcionesBinarias : [
            { Id: 1, Nombre: "Sí" },
            { Id: 2, Nombre: "No" }
        ];
        vnFillSelectDom(document.getElementById("IdOpExclusividad"), opBin, "Id", "Nombre", "—");

        ensureSelect2("#IdUbicacion");
        ensureSelect2("#IdProductora");
        ensureSelect2("#IdMoneda");
        ensureSelect2("#IdEstado");
        ensureSelect2("#IdTipoContrato");
        ensureSelect2("#IdOpExclusividad");

        // change cliente writes hidden
        $("#IdCliente")?.on("change", function () {
            const idCliente = Number(this.value || 0);
            document.getElementById("Venta_IdCliente").value = String(idCliente || 0);
            vnMarkDirty();
        });
    }

    function setClienteSeleccionado(idCliente, triggerChange) {
        const el = document.getElementById("IdCliente");
        if (!el) return;
        el.value = String(idCliente || "");
        if (triggerChange) $("#IdCliente")?.trigger("change.select2");
        document.getElementById("Venta_IdCliente").value = String(idCliente || 0);
    }

    function setDefaultsNueva() {

        const id = Number(document.getElementById("Venta_Id")?.value || 0)
        if (id > 0) return

        const f = document.getElementById("Fecha")
        if (f && !f.value)
            f.value = vnTodayIso()

        const d = document.getElementById("Duracion")
        if (d && !d.value)
            d.value = "00:00"

    }

    function resetAll() {
        // reset form fields (sin recargar combos)
        document.getElementById("Venta_Id").value = "0";

        // cliente: mantener si venía desde query? por defecto lo limpiamos
        setClienteSeleccionado(0, true);

        document.getElementById("Fecha").value = vnNowIsoLocal();
        document.getElementById("NombreEvento").value = "";
        document.getElementById("Duracion").value = "00:00";

        document.getElementById("IdUbicacion").value = "";
        $("#IdUbicacion")?.trigger("change.select2");

        document.getElementById("IdProductora").value = "";
        $("#IdProductora")?.trigger("change.select2");

        document.getElementById("IdMoneda").value = "";
        $("#IdMoneda")?.trigger("change.select2");

        document.getElementById("IdEstado").value = "";
        $("#IdEstado")?.trigger("change.select2");

        document.getElementById("IdTipoContrato").value = "";
        $("#IdTipoContrato")?.trigger("change.select2");

        document.getElementById("IdOpExclusividad").value = "";
        $("#IdOpExclusividad")?.trigger("change.select2");

        document.getElementById("DiasPrevios").value = "";
        document.getElementById("FechaHasta").value = "";
        document.getElementById("ImporteTotal").value = "";

        document.getElementById("NotaInterna").value = "";
        document.getElementById("NotaCliente").value = "";

        // detalle
        VN.detalle.artistas = [];
        VN.detalle.personal = [];
        VN.detalle.cobros = [];

        renderDetalle();
        recalcularTotales();

        VN.flags.dirty = false;
        vnSetSaving(false, "Listo", "ok");
    }

    /* =========================
       ABRIR VENTA (editar)
    ========================= */
    async function abrirVenta(idVenta) {
        try {
            vnSetSaving(true, "Cargando venta...");

            const v = await vnFetchJson(API.editarInfo(idVenta));

            document.getElementById("Venta_Id").value = String(v.Id || 0);
            document.getElementById("Venta_IdCliente").value = String(v.IdCliente || 0);

            // Cliente
            setClienteSeleccionado(Number(v.IdCliente || 0), true);

            // Datos
            document.getElementById("Fecha").value = vnIsoDateTime(v.Fecha);
            document.getElementById("NombreEvento").value = v.NombreEvento || "";
            document.getElementById("Duracion").value = vnIsoLocalFromDate(v.Duracion);

            document.getElementById("IdUbicacion").value = String(v.IdUbicacion || "");
            $("#IdUbicacion")?.trigger("change.select2");

            document.getElementById("IdProductora").value = String(v.IdProductora || "");
            $("#IdProductora")?.trigger("change.select2");

            document.getElementById("IdMoneda").value = String(v.IdMoneda || "");
            $("#IdMoneda")?.trigger("change.select2");

            document.getElementById("IdEstado").value = String(v.IdEstado || "");
            $("#IdEstado")?.trigger("change.select2");

            document.getElementById("IdTipoContrato").value = String(v.IdTipoContrato || "");
            $("#IdTipoContrato")?.trigger("change.select2");

            document.getElementById("IdOpExclusividad").value = v.IdOpExclusividad != null ? String(v.IdOpExclusividad) : "";
            $("#IdOpExclusividad")?.trigger("change.select2");

            document.getElementById("DiasPrevios").value = v.DiasPrevios != null ? String(v.DiasPrevios) : "";
            document.getElementById("FechaHasta").value = v.FechaHasta ? vnIsoDateTime(v.FechaHasta) : "";

            document.getElementById("ImporteTotal").value = String(v.ImporteTotal ?? "");

            document.getElementById("NotaInterna").value = v.NotaInterna || "";
            document.getElementById("NotaCliente").value = v.NotaCliente || "";

            // Detalle
            VN.detalle.artistas = Array.isArray(v.Artistas) ? v.Artistas.map(x => ({
                Id: Number(x.Id || 0),
                IdArtista: Number(x.IdArtista || 0),
                IdRepresentante: Number(x.IdRepresentante || 0),
                PorcComision: Number(x.PorcComision || 0),
                TotalComision: Number(x.TotalComision || 0)
            })) : [];

            VN.detalle.personal = Array.isArray(v.Personal) ? v.Personal.map(x => ({
                Id: Number(x.Id || 0),
                IdPersonal: Number(x.IdPersonal || 0),
                IdCargo: Number(x.IdCargo || 0),
                IdTipoComision: Number(x.IdTipoComision || 0),
                PorcComision: Number(x.PorcComision || 0),
                TotalComision: Number(x.TotalComision || 0)
            })) : [];

            VN.detalle.cobros = Array.isArray(v.Cobros) ? v.Cobros.map(x => ({
                Id: Number(x.Id || 0),
                Fecha: x.Fecha ? new Date(x.Fecha) : new Date(),
                IdMoneda: Number(x.IdMoneda || 0),
                IdCuenta: Number(x.IdCuenta || 0),
                Importe: Number(x.Importe || 0),
                Cotizacion: Number(x.Cotizacion || 1) || 1,
                Conversion: Number(x.Conversion || 0),
                ManualConversion: true, // si viene del backend, respetamos
                NotaCliente: x.NotaCliente || "",
                NotaInterna: x.NotaInterna || ""
            })) : [];

            renderDetalle();
            recalcularTotales();

            // auditoría
            setAuditoria(v);

            VN.flags.dirty = false;
            vnSetSaving(false, "Listo", "ok");
        } catch (e) {
            console.error(e);
            vnToastErr("No se pudo abrir la venta.");
            vnSetSaving(false, "Error", "err");
        }
    }

    function setAuditoria(v) {
        const audReg = document.getElementById("audReg");
        const audMod = document.getElementById("audMod");

        if (audReg) {
            if (v.UsuarioRegistra && v.FechaRegistra) {
                audReg.innerHTML = `<div class="vn-chip"><i class="fa fa-user"></i> Registrado por <b>${v.UsuarioRegistra}</b> • <b>${humanDate(v.FechaRegistra)}</b></div>`;
            } else {
                audReg.innerHTML = `<div class="vn-chip"><i class="fa fa-user"></i> —</div>`;
            }
        }
        if (audMod) {
            if (v.UsuarioModifica && v.FechaModifica) {
                audMod.innerHTML = `<div class="vn-chip"><i class="fa fa-edit"></i> Última modificación por <b>${v.UsuarioModifica}</b> • <b>${humanDate(v.FechaModifica)}</b></div>`;
            } else {
                audMod.innerHTML = `<div class="vn-chip"><i class="fa fa-edit"></i> —</div>`;
            }
        }
    }

    function humanDate(fecha) {
        try {
            const d = new Date(fecha);
            if (isNaN(d.getTime())) return "";
            return d.toLocaleString("es-AR");
        } catch { return ""; }
    }

    /* =========================
       RENDER DETALLE
    ========================= */
    function renderDetalle() {
        renderArtistas();
        renderPersonal();
        renderCobros();

        const cntA = document.getElementById("cntArtistas");
        const cntP = document.getElementById("cntPersonal");
        const cntC = document.getElementById("cntCobros");

        if (cntA) cntA.textContent = `(${VN.detalle.artistas.length})`;
        if (cntP) cntP.textContent = `(${VN.detalle.personal.length})`;
        if (cntC) cntC.textContent = `(${VN.detalle.cobros.length})`;
    }

    function renderArtistas() {
        const tb = document.getElementById("tbArtistas");
        if (!tb) return;
        tb.innerHTML = "";

         if (VN.detalle.artistas.length === 0) {

        tb.innerHTML = `
            <tr class="vn-empty-row">
                <td colspan="5">
                    <div class="vn-empty">
                        <i class="fa fa-music"></i>
                        <div>No hay artistas cargados</div>
                        <small>Presioná <b>Agregar artista</b> para comenzar</small>
                    </div>
                </td>
            </tr>
        `;

        return;
    }

        VN.detalle.artistas.forEach((it, idx) => {
            tb.insertAdjacentHTML("beforeend", `
                <tr>
                    <td><select class="form-select vn-input vn-mini vn-a-artista" data-idx="${idx}"></select></td>
                    <td><select class="form-select vn-input vn-mini vn-a-rep" data-idx="${idx}"></select></td>
                    <td><input class="form-control vn-input vn-mini vn-a-porc" data-idx="${idx}" type="number" min="0" step="0.01" value="${Number(it.PorcComision || 0)}"></td>
                    <td><input class="form-control vn-input vn-mini vn-a-total" data-idx="${idx}" type="text" value="${Number(it.TotalComision || 0)}"></td>
                    <td class="text-end">
                        <button class="btn btn-outline-danger vn-btn vn-mini" type="button" onclick="window.vnDelArtista(${idx})">
                            <i class="fa fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `);
        });

        // fill + select2
        tb.querySelectorAll("select.vn-a-artista").forEach(sel => {
            const idx = Number(sel.dataset.idx);
            vnFillSelectDom(sel, VN.combos.artistas, "Id", "Nombre", "Seleccionar");
            sel.value = String(VN.detalle.artistas[idx].IdArtista || "");
            $(sel)?.select2({ width: "100%", allowClear: true, placeholder: "Artista" })
                .on("change", function () {
                    VN.detalle.artistas[idx].IdArtista = Number(this.value || 0);
                    vnMarkDirty();
                });
        });

        tb.querySelectorAll("select.vn-a-rep").forEach(sel => {
            const idx = Number(sel.dataset.idx);
            vnFillSelectDom(sel, VN.combos.representantes, "Id", "Nombre", "Seleccionar");
            sel.value = String(VN.detalle.artistas[idx].IdRepresentante || "");
            $(sel)?.select2({ width: "100%", allowClear: true, placeholder: "Representante" })
                .on("change", function () {
                    VN.detalle.artistas[idx].IdRepresentante = Number(this.value || 0);
                    vnMarkDirty();
                });
        });

        // % change => recalcular total
        tb.querySelectorAll("input.vn-a-porc").forEach(inp => {
            inp.addEventListener("input", function () {
                const idx = Number(this.dataset.idx);
                VN.detalle.artistas[idx].PorcComision = Number(this.value || 0);
                calcArtistaFromPercent(idx);
                recalcularTotales();
                vnMarkDirty();
            });
        });

        // total change => recalcular %
        tb.querySelectorAll("input.vn-a-total").forEach(inp => {
            inp.addEventListener("input", function () {
                const idx = Number(this.dataset.idx);
                VN.detalle.artistas[idx].TotalComision = vnToNumber(this.value);
                calcArtistaFromTotal(idx);
                recalcularTotales();
                vnMarkDirty();
            });
        });
    }

    function calcArtistaFromPercent(idx) {
        const it = VN.detalle.artistas[idx];
        const total = vnGetImporteTotal();
        const pct = Number(it.PorcComision || 0);
        it.TotalComision = vnRound2(total * (pct / 100));
        // refrescar celda total sin rerender completo
        const inp = document.querySelector(`input.vn-a-total[data-idx="${idx}"]`);
        if (inp) inp.value = String(it.TotalComision || 0);
    }

    function calcArtistaFromTotal(idx) {
        const it = VN.detalle.artistas[idx];
        const total = vnGetImporteTotal();
        const val = Number(it.TotalComision || 0);
        it.PorcComision = total > 0 ? vnRound2((val / total) * 100) : 0;
        const inp = document.querySelector(`input.vn-a-porc[data-idx="${idx}"]`);
        if (inp) inp.value = String(it.PorcComision || 0);
    }

    function renderPersonal() {
        const tb = document.getElementById("tbPersonal");
        if (!tb) return;
        tb.innerHTML = "";

        if (VN.detalle.personal.length === 0) {

            tb.innerHTML = `
            <tr class="vn-empty-row">
                <td colspan="6">
                    <div class="vn-empty">
                        <i class="fa fa-id-badge"></i>
                        <div>No hay personal asignado</div>
                        <small>Presioná <b>Agregar personal</b> para comenzar</small>
                    </div>
                </td>
            </tr>
        `;

            return;
        }


        VN.detalle.personal.forEach((it, idx) => {
            const isFixed = Number(it.IdTipoComision || 0) === 2;

            tb.insertAdjacentHTML("beforeend", `
                <tr>
                    <td><select class="form-select vn-input vn-mini vn-p-personal" data-idx="${idx}"></select></td>
                    <td><select class="form-select vn-input vn-mini vn-p-cargo" data-idx="${idx}"></select></td>
                    <td><select class="form-select vn-input vn-mini vn-p-tipo" data-idx="${idx}"></select></td>
                    <td>
                        <input class="form-control vn-input vn-mini vn-p-porc" data-idx="${idx}"
                               type="number" min="0" step="0.01" value="${Number(it.PorcComision || 0)}" ${isFixed ? "disabled" : ""}>
                    </td>
                    <td><input class="form-control vn-input vn-mini vn-p-total" data-idx="${idx}" type="text" value="${Number(it.TotalComision || 0)}"></td>
                    <td class="text-end">
                        <button class="btn btn-outline-danger vn-btn vn-mini" type="button" onclick="window.vnDelPersonal(${idx})">
                            <i class="fa fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `);
        });

        // select2: personal
        tb.querySelectorAll("select.vn-p-personal").forEach(sel => {
            const idx = Number(sel.dataset.idx);
            vnFillSelectDom(sel, VN.combos.personal, "Id", "Nombre", "Seleccionar");
            sel.value = String(VN.detalle.personal[idx].IdPersonal || "");
            $(sel)?.select2({ width: "100%", allowClear: true, placeholder: "Personal" })
                .on("change", function () {
                    VN.detalle.personal[idx].IdPersonal = Number(this.value || 0);
                    vnMarkDirty();
                });
        });

        // cargo
        tb.querySelectorAll("select.vn-p-cargo").forEach(sel => {
            const idx = Number(sel.dataset.idx);
            vnFillSelectDom(sel, VN.combos.cargos, "Id", "Nombre", "Seleccionar");
            sel.value = String(VN.detalle.personal[idx].IdCargo || "");
            $(sel)?.select2({ width: "100%", allowClear: true, placeholder: "Cargo" })
                .on("change", function () {
                    VN.detalle.personal[idx].IdCargo = Number(this.value || 0);
                    vnMarkDirty();
                });
        });

        // tipo comision
        tb.querySelectorAll("select.vn-p-tipo").forEach(sel => {
            const idx = Number(sel.dataset.idx);
            vnFillSelectDom(sel, VN.combos.tiposComision, "Id", "Nombre", "Seleccionar");
            sel.value = String(VN.detalle.personal[idx].IdTipoComision || "");
            $(sel)?.select2({ width: "100%", allowClear: true, placeholder: "Tipo comisión" })
                .on("change", function () {
                    VN.detalle.personal[idx].IdTipoComision = Number(this.value || 0);

                    // si es fijo (2), deshabilita % y recalcula % informativo
                    const isFixed = VN.detalle.personal[idx].IdTipoComision === 2;
                    const porcEl = document.querySelector(`input.vn-p-porc[data-idx="${idx}"]`);
                    if (porcEl) porcEl.disabled = isFixed;

                    // recalcular según tipo
                    if (isFixed) {
                        calcPersonalFixed(idx);
                    } else {
                        calcPersonalFromPercent(idx);
                    }
                    recalcularTotales();
                    vnMarkDirty();
                });
        });

        // % change => recalcular total (solo si tipo=%)
        tb.querySelectorAll("input.vn-p-porc").forEach(inp => {
            inp.addEventListener("input", function () {
                const idx = Number(this.dataset.idx);
                VN.detalle.personal[idx].PorcComision = Number(this.value || 0);

                const tipo = Number(VN.detalle.personal[idx].IdTipoComision || 0);
                if (tipo === 1) {
                    calcPersonalFromPercent(idx);
                }
                recalcularTotales();
                vnMarkDirty();
            });
        });

        // total change => comportamiento:
        // - si tipo=1 (%): recalcula %
        // - si tipo=2 (fijo): mantiene total, recalcula % informativo
        tb.querySelectorAll("input.vn-p-total").forEach(inp => {
            inp.addEventListener("input", function () {
                const idx = Number(this.dataset.idx);
                VN.detalle.personal[idx].TotalComision = vnToNumber(this.value);

                const tipo = Number(VN.detalle.personal[idx].IdTipoComision || 0);
                if (tipo === 1) {
                    calcPersonalFromTotal(idx);
                } else if (tipo === 2) {
                    calcPersonalFixed(idx);
                }
                recalcularTotales();
                vnMarkDirty();
            });
        });
    }

    function calcPersonalFromPercent(idx) {
        const it = VN.detalle.personal[idx];
        const total = vnGetImporteTotal();
        const pct = Number(it.PorcComision || 0);
        it.TotalComision = vnRound2(total * (pct / 100));
        const inp = document.querySelector(`input.vn-p-total[data-idx="${idx}"]`);
        if (inp) inp.value = String(it.TotalComision || 0);
    }

    function calcPersonalFromTotal(idx) {
        const it = VN.detalle.personal[idx];
        const total = vnGetImporteTotal();
        const val = Number(it.TotalComision || 0);
        it.PorcComision = total > 0 ? vnRound2((val / total) * 100) : 0;
        const inp = document.querySelector(`input.vn-p-porc[data-idx="${idx}"]`);
        if (inp) inp.value = String(it.PorcComision || 0);
    }

    function calcPersonalFixed(idx) {
        // fijo: total es el valor ingresado, porcentaje informativo
        const it = VN.detalle.personal[idx];
        const total = vnGetImporteTotal();
        const val = Number(it.TotalComision || 0);
        it.PorcComision = total > 0 ? vnRound2((val / total) * 100) : 0;
        const inp = document.querySelector(`input.vn-p-porc[data-idx="${idx}"]`);
        if (inp) inp.value = String(it.PorcComision || 0);
    }

    function renderCobros() {
        const tb = document.getElementById("tbCobros");
        if (!tb) return;
        tb.innerHTML = "";

        if (VN.detalle.cobros.length === 0) {

            tb.innerHTML = `
            <tr class="vn-empty-row">
                <td colspan="7">
                    <div class="vn-empty">
                        <i class="fa fa-money"></i>
                        <div>No hay cobros registrados</div>
                        <small>Podés agregar cobros cuando empiecen los pagos</small>
                    </div>
                </td>
            </tr>
        `;

            return;
        }

        VN.detalle.cobros.forEach((it, idx) => {
            const fechaIso = vnIsoLocalFromDate(it.Fecha || new Date());
            tb.insertAdjacentHTML("beforeend", `
                <tr>
                    <td><input class="form-control vn-input vn-mini vn-c-fecha" data-idx="${idx}" type="datetime-local" value="${fechaIso}"></td>
                    <td><select class="form-select vn-input vn-mini vn-c-mon" data-idx="${idx}"></select></td>
                    <td><select class="form-select vn-input vn-mini vn-c-cuenta" data-idx="${idx}"></select></td>
                    <td><input class="form-control vn-input vn-mini vn-c-imp" data-idx="${idx}" type="text" value="${Number(it.Importe || 0)}"></td>
                    <td><input class="form-control vn-input vn-mini vn-c-cot" data-idx="${idx}" type="number" step="0.0001" min="0" value="${Number(it.Cotizacion || 1)}"></td>
                    <td><input class="form-control vn-input vn-mini vn-c-conv" data-idx="${idx}" type="text" value="${Number(it.Conversion || 0)}"></td>
                    <td class="text-end">
                        <button class="btn btn-outline-danger vn-btn vn-mini" type="button" onclick="window.vnDelCobro(${idx})">
                            <i class="fa fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `);
        });

        // moneda
        tb.querySelectorAll("select.vn-c-mon").forEach(sel => {
            const idx = Number(sel.dataset.idx);
            vnFillSelectDom(sel, VN.combos.monedas, "Id", "Nombre", "Seleccionar");
            sel.value = String(VN.detalle.cobros[idx].IdMoneda || "");
            $(sel)?.select2({ width: "100%", allowClear: true, placeholder: "Moneda" })
                .on("change", function () {

                    VN.detalle.cobros[idx].IdMoneda = Number(this.value || 0);

                    // 🔹 cargar cuentas de esa moneda
                    cargarCuentasPorMoneda(idx, VN.detalle.cobros[idx].IdMoneda);

                    // reset cotización si no es manual
                    if (!VN.detalle.cobros[idx].ManualConversion) {
                        VN.detalle.cobros[idx].Cotizacion = 1;
                        const cotEl = document.querySelector(`input.vn-c-cot[data-idx="${idx}"]`);
                        if (cotEl) cotEl.value = "1";
                    }

                    recalcularCobro(idx);
                    vnMarkDirty();

                });
        });

        // cuenta
        tb.querySelectorAll("select.vn-c-cuenta").forEach(sel => {

            const idx = Number(sel.dataset.idx);
            const idMoneda = VN.detalle.cobros[idx].IdMoneda || 0;

            cargarCuentasPorMoneda(idx, idMoneda);

            sel.value = String(VN.detalle.cobros[idx].IdCuenta || "");

            $(sel)?.on("change", function () {

                VN.detalle.cobros[idx].IdCuenta = Number(this.value || 0);
                vnMarkDirty();

            });

        });

        // fecha
        tb.querySelectorAll("input.vn-c-fecha").forEach(inp => {
            inp.addEventListener("change", function () {
                const idx = Number(this.dataset.idx);
                VN.detalle.cobros[idx].Fecha = new Date(this.value);
                vnMarkDirty();
            });
        });

        // importe
        tb.querySelectorAll("input.vn-c-imp").forEach(inp => {
            inp.addEventListener("input", function () {
                const idx = Number(this.dataset.idx);
                VN.detalle.cobros[idx].Importe = vnToNumber(this.value);
                VN.detalle.cobros[idx].ManualConversion = false;
                recalcularCobro(idx);
                vnMarkDirty();
            });
        });

        // cotizacion
        tb.querySelectorAll("input.vn-c-cot").forEach(inp => {
            inp.addEventListener("input", function () {
                const idx = Number(this.dataset.idx);
                VN.detalle.cobros[idx].Cotizacion = Number(this.value || 1) || 1;
                VN.detalle.cobros[idx].ManualConversion = false;
                recalcularCobro(idx);
                vnMarkDirty();
            });
        });

        // conversion manual
        tb.querySelectorAll("input.vn-c-conv").forEach(inp => {
            inp.addEventListener("input", function () {
                const idx = Number(this.dataset.idx);
                VN.detalle.cobros[idx].Conversion = vnToNumber(this.value);
                VN.detalle.cobros[idx].ManualConversion = true;
                recalcularTotales();
                vnMarkDirty();
            });
        });
    }

    function recalcularCobro(idx) {
        const c = VN.detalle.cobros[idx];
        if (!c) return;

        const imp = Number(c.Importe || 0);
        const cot = Number(c.Cotizacion || 1) || 1;

        if (!c.ManualConversion) {
            c.Conversion = vnRound2(imp * cot);
            const convEl = document.querySelector(`input.vn-c-conv[data-idx="${idx}"]`);
            if (convEl) convEl.value = String(c.Conversion || 0);
        }
        recalcularTotales();
    }

    /* =========================
       DELETE handlers (global for onclick)
    ========================= */
    window.vnDelArtista = function (i) {
        VN.detalle.artistas.splice(i, 1);
        renderDetalle();
        recalcularTotales();
        vnMarkDirty();
    };
    window.vnDelPersonal = function (i) {
        VN.detalle.personal.splice(i, 1);
        renderDetalle();
        recalcularTotales();
        vnMarkDirty();
    };
    window.vnDelCobro = function (i) {
        VN.detalle.cobros.splice(i, 1);
        renderDetalle();
        recalcularTotales();
        vnMarkDirty();
    };

    /* =========================
       CÁLCULOS
    ========================= */
    function recalcularTotales() {
        const importeTotal = vnGetImporteTotal();

        // recalcular comisiones al vuelo según importeTotal (para %)
        VN.detalle.artistas.forEach((a, idx) => {
            // si el usuario cambió el % (o si total está en 0), mantenemos % como fuente
            // criterio: si existe % y no está NaN, recalculamos total desde %
            // (si quieren que total sea fuente, ya lo manejamos en input total change)
            if (typeof a.PorcComision === "number" && !isNaN(a.PorcComision)) {
                // no pisar si el user estaba editando total? esto solo corre por cambios generales; ok
                // respetamos el total si fue editado manualmente: no tenemos flag; usamos heurística:
                // si total == 0 y %>0 => recalcular
                if ((Number(a.TotalComision || 0) === 0 && Number(a.PorcComision || 0) > 0) || VN.flags.restoringDraft) {
                    a.TotalComision = vnRound2(importeTotal * (Number(a.PorcComision || 0) / 100));
                }
            }
        });

        VN.detalle.personal.forEach((p, idx) => {
            const tipo = Number(p.IdTipoComision || 0);
            if (tipo === 1) {
                // %
                if ((Number(p.TotalComision || 0) === 0 && Number(p.PorcComision || 0) > 0) || VN.flags.restoringDraft) {
                    p.TotalComision = vnRound2(importeTotal * (Number(p.PorcComision || 0) / 100));
                }
            }
            if (tipo === 2) {
                // fijo: % informativo
                p.PorcComision = importeTotal > 0 ? vnRound2((Number(p.TotalComision || 0) / importeTotal) * 100) : 0;
            }
        });

        const totalComisiones =
            (VN.detalle.artistas || []).reduce((a, x) => a + Number(x.TotalComision || 0), 0) +
            (VN.detalle.personal || []).reduce((a, x) => a + Number(x.TotalComision || 0), 0);

        // Cobrado: usamos Conversion (base currency)
        const cobrado = (VN.detalle.cobros || []).reduce((a, x) => a + Number(x.Conversion || 0), 0);

        const saldo = vnRound2(importeTotal - totalComisiones - cobrado);

        // Summary IDs (del HTML)
        const sumImporte = document.getElementById("sumImporte");
        const sumComisiones = document.getElementById("sumComisiones");
        const sumCobrado = document.getElementById("sumCobrado");
        const sumSaldo = document.getElementById("sumSaldo");

        if (sumImporte) sumImporte.textContent = vnFmtMoneyARS(importeTotal);
        if (sumComisiones) sumComisiones.textContent = vnFmtMoneyARS(totalComisiones);
        if (sumCobrado) sumCobrado.textContent = vnFmtMoneyARS(cobrado);
        if (sumSaldo) sumSaldo.textContent = vnFmtMoneyARS(saldo);
    }

    /* =========================
       VALIDACIÓN (mínima + robusta)
    ========================= */
    function validarCamposVenta() {

        let errores = []

        const campos = [
            { id: "IdCliente", nombre: "Cliente" },
            { id: "Fecha", nombre: "Fecha" },
            { id: "NombreEvento", nombre: "Nombre evento" },
            { id: "IdUbicacion", nombre: "Ubicación" },
            { id: "IdProductora", nombre: "Productora" },
            { id: "IdMoneda", nombre: "Moneda" },
            { id: "IdEstado", nombre: "Estado" },
            { id: "IdTipoContrato", nombre: "Tipo contrato" },
            { id: "ImporteTotal", nombre: "Importe total" }
        ]

        campos.forEach(c => {

            const el = document.getElementById(c.id)
            if (!el) return

            const val = (el.value ?? "").toString().trim()

            const esValido = val !== ""

            setEstadoCampo(el, esValido)

            if (!esValido)
                errores.push(c.nombre)

        })

        // duración HH:MM
        const dur = document.getElementById("Duracion")?.value || ""

        if (!/^\d{2}:\d{2}$/.test(dur)) {

            errores.push("Duración")

            setEstadoCampo(document.getElementById("Duracion"), false)
        }

        // cobros
        VN.detalle.cobros.forEach((c, i) => {

            if (!c.IdCuenta)
                errores.push(`Cobro #${i + 1} (cuenta)`)

            if (!c.IdMoneda)
                errores.push(`Cobro #${i + 1} (moneda)`)

            if (Number(c.Importe || 0) <= 0)
                errores.push(`Cobro #${i + 1} (importe)`)

        })

        if (errores.length > 0) {

            mostrarErrorCampos(
                `Debes completar los campos requeridos:<br>
            <strong>${errores.join(", ")}</strong>`,
                null,
                "validacion"
            )

            return false
        }

        cerrarErrorCampos()

        return true
    }

    /* =========================
       BUILD MODEL (VMVenta)
    ========================= */
    function buildModel() {
        const idVenta = Number(document.getElementById("Venta_Id")?.value || 0);
        const idCliente = vnGetClienteId();

        // antes de mapear, recalculamos para que quede consistente
        recalcularTotales();

        const importeTotal = vnGetImporteTotal();

        // Cobrado / saldo: (como tu VM tiene estos campos, y tu repo actual los pisa desde entity)
        // igual los mandamos calculados (no molesta)
        const totalComisiones =
            (VN.detalle.artistas || []).reduce((a, x) => a + Number(x.TotalComision || 0), 0) +
            (VN.detalle.personal || []).reduce((a, x) => a + Number(x.TotalComision || 0), 0);

        const cobrado = (VN.detalle.cobros || []).reduce((a, x) => a + Number(x.Conversion || 0), 0);
        const saldo = vnRound2(importeTotal - totalComisiones - cobrado);

        const model = {
            Id: idVenta,

            Fecha: new Date(document.getElementById("Fecha")?.value),
            IdUbicacion: Number(document.getElementById("IdUbicacion")?.value || 0),
            NombreEvento: document.getElementById("NombreEvento")?.value || "",
            Duracion: document.getElementById("Duracion")?.value
                ? new Date(document.getElementById("Duracion").value)
                : new Date(document.getElementById("Fecha").value),

            IdCliente: Number(idCliente || 0),
            IdProductora: Number(document.getElementById("IdProductora")?.value || 0),
            IdMoneda: Number(document.getElementById("IdMoneda")?.value || 0),
            IdEstado: Number(document.getElementById("IdEstado")?.value || 0),

            ImporteTotal: vnRound2(importeTotal),
            ImporteAbonado: vnRound2(cobrado),
            Saldo: vnRound2(saldo),

            NotaInterna: (document.getElementById("NotaInterna")?.value || "").trim() || null,
            NotaCliente: (document.getElementById("NotaCliente")?.value || "").trim() || null,

            IdTipoContrato: Number(document.getElementById("IdTipoContrato")?.value || 0),
            IdOpExclusividad: document.getElementById("IdOpExclusividad")?.value
                ? Number(document.getElementById("IdOpExclusividad").value)
                : null,
            DiasPrevios: document.getElementById("DiasPrevios")?.value
                ? Number(document.getElementById("DiasPrevios").value)
                : null,
            FechaHasta: document.getElementById("FechaHasta")?.value
                ? new Date(document.getElementById("FechaHasta").value)
                : null,

            Artistas: (VN.detalle.artistas || []).map(x => ({
                Id: Number(x.Id || 0),
                IdArtista: Number(x.IdArtista || 0),
                IdRepresentante: Number(x.IdRepresentante || 0),
                PorcComision: vnRound2(Number(x.PorcComision || 0)),
                TotalComision: vnRound2(Number(x.TotalComision || 0))
            })),

            Personal: (VN.detalle.personal || []).map(x => {
                const tipo = Number(x.IdTipoComision || 0);
                const total = vnRound2(Number(x.TotalComision || 0));
                const pct = (tipo === 2)
                    ? (importeTotal > 0 ? vnRound2((total / importeTotal) * 100) : 0)
                    : vnRound2(Number(x.PorcComision || 0));

                return ({
                    Id: Number(x.Id || 0),
                    IdPersonal: Number(x.IdPersonal || 0),
                    IdCargo: Number(x.IdCargo || 0),
                    IdTipoComision: Number(x.IdTipoComision || 0),
                    PorcComision: pct,
                    TotalComision: total
                });
            }),

            Cobros: (VN.detalle.cobros || []).map(x => ({
                Id: Number(x.Id || 0),
                Fecha: x.Fecha ? new Date(x.Fecha) : new Date(),
                IdMoneda: Number(x.IdMoneda || 0),
                IdCuenta: Number(x.IdCuenta || 0),
                Importe: vnRound2(Number(x.Importe || 0)),
                Cotizacion: vnRound2(Number(x.Cotizacion || 1) || 1),
                Conversion: vnRound2(Number(x.Conversion || x.Importe || 0)),
                NotaCliente: (x.NotaCliente || "").trim() || null,
                NotaInterna: (x.NotaInterna || "").trim() || null
            }))
        };

        return model;
    }

    /* =========================
       GUARDAR
    ========================= */
    async function guardarVenta() {
        if (!validarCamposVenta()) return;

        const model = buildModel();
        const isNew = Number(model.Id || 0) === 0;

        try {
            vnSetSaving(true, "Guardando...");
            const url = isNew ? API.insertar : API.actualizar;
            const method = isNew ? "POST" : "PUT";

            const r = await fetch(url, {
                method,
                headers: authHeaders(),
                body: JSON.stringify(model)
            });

            if (!r.ok) throw new Error(r.statusText);

            const data = await r.json();

            if (!data.valor) {
                vnToastErr(data.mensaje || "No se pudo guardar.");
                vnSetSaving(false, "Error", "err");
                return;
            }

            vnToastOk(data.mensaje || "Guardado OK");
            vnSetSaving(false, "Guardado", "ok");

            // limpiar borrador
            localStorage.removeItem(DRAFT_KEY());
            VN.flags.dirty = false;

            // si devuelve idReferencia, reabrimos para traer IDs de detalles (si el backend los asigna)
            const idRef = Number(data.idReferencia || 0);
            if (idRef > 0) {
                await abrirVenta(idRef);
            } else {
                // fallback: si era nueva y no devolvió id, dejamos como está
                // (pero tu service sí devuelve IdReferencia según tu código)
            }
        } catch (e) {
            console.error(e);
            vnToastErr("Error guardando la venta.");
            vnSetSaving(false, "Error", "err");
        }
    }

    /* =========================
       DRAFT PRO
    ========================= */
    function guardarDraft(showToast) {
        try {
            const idCliente = vnGetClienteId();
            if (!idCliente) return;

            const payload = {
                ts: new Date().toISOString(),
                idVenta: Number(document.getElementById("Venta_Id")?.value || 0),
                idCliente: idCliente,
                form: {
                    Fecha: document.getElementById("Fecha")?.value || "",
                    NombreEvento: document.getElementById("NombreEvento")?.value || "",
                    Duracion: document.getElementById("Duracion")?.value || "",
                    IdUbicacion: document.getElementById("IdUbicacion")?.value || "",
                    IdProductora: document.getElementById("IdProductora")?.value || "",
                    IdMoneda: document.getElementById("IdMoneda")?.value || "",
                    IdEstado: document.getElementById("IdEstado")?.value || "",
                    IdTipoContrato: document.getElementById("IdTipoContrato")?.value || "",
                    IdOpExclusividad: document.getElementById("IdOpExclusividad")?.value || "",
                    DiasPrevios: document.getElementById("DiasPrevios")?.value || "",
                    FechaHasta: document.getElementById("FechaHasta")?.value || "",
                    ImporteTotal: document.getElementById("ImporteTotal")?.value || "",
                    NotaInterna: document.getElementById("NotaInterna")?.value || "",
                    NotaCliente: document.getElementById("NotaCliente")?.value || ""
                },
                detalle: VN.detalle
            };

            localStorage.setItem(DRAFT_KEY(), JSON.stringify(payload));
            if (showToast) vnToastOk("Borrador guardado.");

            VN.flags.dirty = false;
            vnSetSaving(false, "Borrador guardado", "ok");
        } catch (e) {
            console.error(e);
        }
    }

    async function restaurarDraft() {
        try {
            const raw = localStorage.getItem(DRAFT_KEY());
            if (!raw) { vnToastErr("No hay borrador guardado."); return; }

            const d = JSON.parse(raw);
            if (!d || !d.form || !d.idCliente) { vnToastErr("Borrador inválido."); return; }

            VN.flags.restoringDraft = true;

            // set venta id (solo informativo; si era nueva, dejamos 0)
            document.getElementById("Venta_Id").value = String(Number(d.idVenta || 0));

            // cliente
            setClienteSeleccionado(Number(d.idCliente || 0), true);

            // form
            document.getElementById("Fecha").value = d.form.Fecha || vnNowIsoLocal();
            document.getElementById("NombreEvento").value = d.form.NombreEvento || "";
            document.getElementById("Duracion").value = d.form.Duracion || vnNowIsoLocal();

            document.getElementById("IdUbicacion").value = String(d.form.IdUbicacion || "");
            $("#IdUbicacion")?.trigger("change.select2");

            document.getElementById("IdProductora").value = String(d.form.IdProductora || "");
            $("#IdProductora")?.trigger("change.select2");

            document.getElementById("IdMoneda").value = String(d.form.IdMoneda || "");
            $("#IdMoneda")?.trigger("change.select2");

            document.getElementById("IdEstado").value = String(d.form.IdEstado || "");
            $("#IdEstado")?.trigger("change.select2");

            document.getElementById("IdTipoContrato").value = String(d.form.IdTipoContrato || "");
            $("#IdTipoContrato")?.trigger("change.select2");

            document.getElementById("IdOpExclusividad").value = String(d.form.IdOpExclusividad || "");
            $("#IdOpExclusividad")?.trigger("change.select2");

            document.getElementById("DiasPrevios").value = d.form.DiasPrevios || "";
            document.getElementById("FechaHasta").value = d.form.FechaHasta || "";
            document.getElementById("ImporteTotal").value = d.form.ImporteTotal || "";
            document.getElementById("NotaInterna").value = d.form.NotaInterna || "";
            document.getElementById("NotaCliente").value = d.form.NotaCliente || "";

            // detalle (clonar seguro)
            VN.detalle = d.detalle || { artistas: [], personal: [], cobros: [] };
            VN.detalle.artistas = Array.isArray(VN.detalle.artistas) ? VN.detalle.artistas : [];
            VN.detalle.personal = Array.isArray(VN.detalle.personal) ? VN.detalle.personal : [];
            VN.detalle.cobros = Array.isArray(VN.detalle.cobros) ? VN.detalle.cobros : [];

            // normalizar fechas de cobros
            VN.detalle.cobros = VN.detalle.cobros.map(c => ({
                ...c,
                Fecha: c.Fecha ? new Date(c.Fecha) : new Date(),
                ManualConversion: !!c.ManualConversion
            }));

            renderDetalle();
            recalcularTotales();

            VN.flags.dirty = false;
            VN.flags.restoringDraft = false;

            vnToastOk("Borrador restaurado.");
            vnSetSaving(false, "Borrador restaurado", "ok");
        } catch (e) {
            console.error(e);
            VN.flags.restoringDraft = false;
            vnToastErr("No se pudo restaurar el borrador.");
        }
    }

})();

/* =========================================================
SECCIONES DINÁMICAS (Artistas / Personal / Cobros / etc)
========================================================= */

function initSecciones() {

    const tabs = document.querySelectorAll(".vn-head-btn")
    const panels = document.querySelectorAll(".vn-section")

    tabs.forEach(tab => {

        tab.addEventListener("click", () => {

            const sec = tab.dataset.sec

            // activar botón
            tabs.forEach(t => t.classList.remove("active"))
            tab.classList.add("active")

            // ocultar paneles
            panels.forEach(p => p.classList.remove("active"))

            // mostrar panel correspondiente
            const panel = document.getElementById("sec-" + sec)

            if (panel)
                panel.classList.add("active")

        })

    })

}



function initDuracionMask() {

    const el = document.getElementById("Duracion");
    if (!el) return;

    el.addEventListener("input", function () {

        let v = this.value.replace(/\D/g, "");

        if (v.length > 4)
            v = v.slice(0, 4);

        if (v.length >= 3)
            v = v.slice(0, 2) + ":" + v.slice(2);

        this.value = v;

    });

    el.addEventListener("blur", function () {

        if (!this.value) return;

        const parts = this.value.split(":");

        if (parts.length !== 2) {
            this.value = "";
            return;
        }

        let h = parseInt(parts[0] || 0);
        let m = parseInt(parts[1] || 0);

        if (isNaN(h) || isNaN(m)) {
            this.value = "";
            return;
        }

        if (h > 23) h = 23;
        if (m > 59) m = 59;

        this.value =
            String(h).padStart(2, "0") +
            ":" +
            String(m).padStart(2, "0");

    });

}
function vnTodayIso() {

    const d = new Date()

    const pad = x => String(x).padStart(2, "0")

    return d.getFullYear() + "-" +
        pad(d.getMonth() + 1) + "-" +
        pad(d.getDate())

}

function validarCampoIndividual(el) {

    const obligatorios = [
        "IdCliente",
        "Fecha",
        "NombreEvento",
        "IdUbicacion",
        "IdProductora",
        "IdMoneda",
        "IdEstado",
        "ImporteTotal",
        "IdTipoContrato"
    ]

    if (!obligatorios.includes(el.id))
        return

    const valor = (el.value ?? "").toString().trim()

    const esValido =
        valor !== "" &&
        valor !== null &&
        valor !== "Seleccionar"

    setEstadoCampo(el, esValido)

}


