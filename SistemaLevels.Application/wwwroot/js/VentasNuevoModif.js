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
        cuentas: "/MonedasCuenta/Lista",
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

    function vnIsoDateTimeLocal(d) {

        if (!d) return "";

        const dt = new Date(d);
        if (isNaN(dt.getTime())) return "";

        const pad = n => String(n).padStart(2, "0");

        return dt.getFullYear() + "-" +
            pad(dt.getMonth() + 1) + "-" +
            pad(dt.getDate()) + "T" +
            pad(dt.getHours()) + ":" +
            pad(dt.getMinutes());

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


    /* =========================================================
    CONTRATOS (DOCX con plantilla servidor)
    - Depende de: VN, buildModel, validarCamposVenta, vnRound2, vnToNumber
    - Usa: errorModal / exitoModal
    ========================================================= */

    async function exportarContrato(tipo) {

        try {

            if (!validarCamposVenta())
                return;

            const model = buildModel();
            const idTipoContrato = Number(model.IdTipoContrato || 0);

            if (!idTipoContrato) {
                errorModal("Seleccioná el Tipo de contrato.");
                return;
            }

            // ===============================
            // 1️⃣ Obtener plantilla DOCX
            // ===============================

            const tplBuf = await fetchContratoTemplate(
                idTipoContrato,
                `Contrato_${idTipoContrato}.docx`
            );

            // ===============================
            // 2️⃣ Generar datos del contrato
            // ===============================

            const data = buildContratoData(model);

            // ===============================
            // 3️⃣ Generar DOCX con docxtemplater
            // ===============================

            const blobDocx = renderDocxFromTemplate(tplBuf, data);

            // ===============================
            // WORD
            // ===============================

            if (tipo === "word") {

                const url = URL.createObjectURL(blobDocx);

                const a = document.createElement("a");
                a.href = url;
                a.download = `${data.NombreArchivo}.docx`;
                a.click();

                URL.revokeObjectURL(url);

                exitoModal("Contrato generado en Word.");
                return;
            }

            // ===============================
            // PDF
            // ===============================

            const arrayBuffer = await blobDocx.arrayBuffer();

            // convertir DOCX a HTML
            const result = await mammoth.convertToHtml({ arrayBuffer });

            const html = result.value;

            // contenedor temporal
            const container = document.createElement("div");

            container.style.padding = "40px";
           
            container.style.left = "-9999px"
            container.style.top = "-9999px"
            container.style.background = "white";
            container.innerHTML = html;

            document.body.appendChild(container);

            const opt = {
                margin: 10,
                filename: `${data.NombreArchivo}.pdf`,
                image: { type: "jpeg", quality: 0.98 },
                html2canvas: { scale: 2 },
                jsPDF: {
                    unit: "mm",
                    format: "a4",
                    orientation: "portrait"
                }
            };

            await html2pdf()
                .set(opt)
                .from(container)
                .save();

            document.body.removeChild(container);

            exitoModal("Contrato generado en PDF.");

        }
        catch (e) {

            console.error(e);
            errorModal("No se pudo generar el contrato.");

        }

    }
    async function fetchContratoTemplate(idTipoContrato, fallbackName) {
        const r = await fetch(`/Contratos/Descargar?idTipoContrato=${idTipoContrato}&nombre=${encodeURIComponent(fallbackName)}`, {
            method: "GET",
            headers: { "Authorization": "Bearer " + (token || "") }
        });

        if (!r.ok) {
            // 🔥 este error tiene que ir con errorModal
            throw new Error("No existe plantilla para este tipo de contrato.");
        }

        return await r.arrayBuffer();
    }

    function buildContratoData(model) {

        // ===== selects (texto visible)
        const NombreCliente = ($("#IdCliente").find(":selected")?.text() || "").trim();
        const ProductoraCliente = ($("#IdProductora").find(":selected")?.text() || "").trim();
        const Ubicacion = ($("#IdUbicacion").find(":selected")?.text() || "").trim();
        const NombreMoneda = ($("#IdMoneda").find(":selected")?.text() || "").trim();
        const TipoContrato = ($("#IdTipoContrato").find(":selected")?.text() || "").trim();

        // ===== fecha (día/mes/año)
        const f = model?.Fecha ? new Date(model.Fecha) : null;
        const Dia = f ? String(f.getDate()) : "";
        const Mes = f ? f.toLocaleString("es-AR", { month: "long" }) : "";
        const Año = f ? String(f.getFullYear()) : "";

        const Fecha = f ? f.toLocaleDateString("es-AR") : "";

        // ===== duración HH:MM
        const dur = (document.getElementById("Duracion")?.value || "00:00").trim();
        const [hhRaw, mmRaw] = dur.split(":");
        const DuracionHora = (hhRaw ?? "0").replace(/^0+/, "") || "0";
        const DuracionMinuto = (mmRaw ?? "0").replace(/^0+/, "") || "0";

        // ===== exclusividad
        // En tu contrato @Exclusividad está pegado al final del punto 1.1,
        // por eso devolvemos un texto que ya trae el "extra".
        let Exclusividad = "";
        if (Number(model?.IdOpExclusividad || 0) === 1) Exclusividad = " con exclusividad.";
        else if (Number(model?.IdOpExclusividad || 0) === 2) Exclusividad = "";

        // ===== importes
        const ImporteTotal = vnRound2(model?.ImporteTotal || 0);
        const ImporteTotalMitad_1 = vnRound2(ImporteTotal / 2);
        const ImporteTotalMitad_2 = vnRound2(ImporteTotal - ImporteTotalMitad_1); // evita centavos raros

        // ===== artista/representante (si tenés varios, tomo el primero)
        const art0 = (VN.detalle.artistas || [])[0] || null;

        const NombreArtista = art0 ? textById(VN.combos.artistas, art0.IdArtista) : "";
        const NombreRepresentante = art0 ? textById(VN.combos.representantes, art0.IdRepresentante) : "";

        // ===== lugar/espacio (tu doc usa @Lugar y @Espacio)
        // @Lugar suele ser "Ciudad/Provincia", @Espacio es el predio/venue
        // Como en tu pantalla solo tenés Ubicación, lo mapeo ahí.
        const Lugar = Ubicacion;
        const Espacio = model?.Espacio || ""; // si no existe en tu VM, queda vacío

        // ===== nombre de archivo
        const safe = (s) => String(s || "")
            .trim()
            .replace(/[\\/:*?"<>|]/g, "")     // inválidos Windows
            .replace(/\s+/g, "_")
            .slice(0, 80);

        const NombreArchivo =
            `Contrato_${safe(NombreCliente || "Cliente")}_${(f ? `${Año}-${String(f.getMonth() + 1).padStart(2, "0")}-${String(f.getDate()).padStart(2, "0")}` : "sin_fecha")}`;

        // ===== devolvemos TODO lo que tu doc pide + extras útiles
        const data = {

            // --- util
            NombreArchivo,

            // --- contrato: cliente
            NombreCliente,
            DniCliente: model?.DniCliente || "",
            CuitCliente: model?.CuitCliente || "",
            DomicilioCliente: model?.DomicilioCliente || "",
            ProductoraCliente,

            // --- contrato: artista + productora/manager
            NombreArtista,
            DniArtista: model?.DniArtista || "",
            DomicilioArtista: model?.DomicilioArtista || "",
            CuitArtista: model?.CuitArtista || "",
            DatosArtista2: model?.DatosArtista2 || "",

            NombreRepresentante,
            DniRepresentante: model?.DniRepresentante || "",
            DomicilioRepresentante: model?.DomicilioRepresentante || "",
            CuitRepresentante: model?.CuitRepresentante || "",

            // --- fecha / show
            Dia,
            Mes,
            Año,
            Fecha,

            Lugar,
            Espacio,
            Ubicacion,
            NombreEvento: model?.NombreEvento || "",

            DuracionHora,
            DuracionMinuto,
            Exclusividad,

            // --- dinero
            ImporteTotal,
            ImporteTotalMitad_1,
            ImporteTotalMitad_2,

            NombreMoneda_1: NombreMoneda,
            NombreMoneda_2: NombreMoneda,
            NombreMoneda_3: NombreMoneda,

            // --- extras (por si querés mostrar/validar)
            TipoContrato,
            DiasPrevios: model?.DiasPrevios ?? "",
            FechaHasta: model?.FechaHasta ? new Date(model.FechaHasta).toLocaleString("es-AR") : ""
        };

        // ✅ opcional: espejo con @... por si después configurás delimiters raro
        // (Docxtemplater NO reemplaza @Campo por defecto)
        // data["@NombreCliente"] = data.NombreCliente; // etc...

        return data;
    }

    function textById(list, id) {
        id = Number(id || 0);
        if (!id) return "";
        const it = (list || []).find(x => Number(x.Id) === id);
        return it?.Nombre || it?.Descripcion || "";
    }

    function renderDocxFromTemplate(arrayBuffer, data) {

        const zip = new PizZip(arrayBuffer);

        // Archivos DOCX donde puede haber texto
        const xmlFiles = Object.keys(zip.files).filter(f =>
            f.startsWith("word/") && f.endsWith(".xml")
        );

        xmlFiles.forEach(file => {

            let xml = zip.file(file).asText();

            // 🔹 Word rompe texto en varios nodos
            xml = xml.replace(/<\/w:t>\s*<w:t[^>]*>/g, "");

            // 🔹 Convertir @Campo -> {Campo}
            xml = xml.replace(/@([A-Za-z0-9_]+)/g, "{$1}");

            zip.file(file, xml);

        });

        const doc = new docxtemplater(zip, {
            paragraphLoop: true,
            linebreaks: true,
            nullGetter: () => ""
        });

        doc.render(data);

        return doc.getZip().generate({
            type: "blob",
            mimeType:
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        });
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

                const titulo = document.getElementById("tituloVenta");
                if (titulo) titulo.textContent = " Modificar venta";

                await abrirVenta(idVenta);

            } else {

                const titulo = document.getElementById("tituloVenta");
                if (titulo) titulo.textContent = " Registrar venta";

                document.getElementById("Venta_Id").value = "0";

                renderDetalle();
                recalcularTotales();

                // auditoría vacía
                setAuditoria({});

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

    document.querySelectorAll(".Inputmiles").forEach(inp => {

        inp.addEventListener("input", function () {
            formatearMilesInput(this);
        });

    });

    function bindUI() {
        const btnGuardar = document.getElementById("btnGuardarVenta");
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

        // CONTRATO
        document.getElementById("btnContrato")?.addEventListener("click", async () => {

            const idTipoContrato = Number(document.getElementById("IdTipoContrato")?.value || 0);

            if (!idTipoContrato) {
                vnToastErr("Seleccioná el Tipo de contrato antes de generar.");
                return;
            }

            const formato = document.getElementById("Contrato_Formato")?.value || "pdf";

            await exportarContrato(formato);

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

            // =========================
            // Cliente
            // =========================
            setClienteSeleccionado(Number(v.IdCliente || 0), true);

            // =========================
            // Fecha (input type="date" => YYYY-MM-DD)
            // =========================
            const elFecha = document.getElementById("Fecha");
            if (elFecha) {
                elFecha.value = vnIsoDateOnly(v.Fecha); // ✅ date-only
            }

            // =========================
            // Nombre evento
            // =========================
            document.getElementById("NombreEvento").value = v.NombreEvento || "";

            // =========================
            // Duración (input "HH:mm")
            // VM trae DateTime => extraer HH:mm
            // =========================
            const elDur = document.getElementById("Duracion");
            if (elDur) {
                elDur.value = vnTimeHHmm(v.Duracion); // ✅ "HH:mm"
            }

            // =========================
            // Combos (Select2)
            // =========================
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

            document.getElementById("IdOpExclusividad").value =
                v.IdOpExclusividad != null ? String(v.IdOpExclusividad) : "";
            $("#IdOpExclusividad")?.trigger("change.select2");

            // =========================
            // Campos opcionales
            // =========================
            document.getElementById("DiasPrevios").value =
                v.DiasPrevios != null ? String(v.DiasPrevios) : "";

            const elFechaHasta = document.getElementById("FechaHasta");
            if (elFechaHasta) {
                // si tu input es datetime-local: usar vnIsoDateTimeLocal
                // si tu input es date: usar vnIsoDateOnly
                // (te dejo el más común: datetime-local)
                elFechaHasta.value = v.FechaHasta ? vnIsoDateTimeLocal(v.FechaHasta) : "";
            }

            // =========================
            // ImporteTotal (texto, y si tenés miles, lo formateás después)
            // =========================
            const elImp = document.getElementById("ImporteTotal");
            if (elImp) {
                // guardo como número simple, sin "NaN"
                elImp.value = String(Number(v.ImporteTotal ?? 0));
            }

            document.getElementById("NotaInterna").value = v.NotaInterna || "";
            document.getElementById("NotaCliente").value = v.NotaCliente || "";

            // =========================
            // Detalle (normalización)
            // =========================
            VN.detalle.artistas = Array.isArray(v.Artistas)
                ? v.Artistas.map(x => ({
                    Id: Number(x.Id || 0),
                    IdArtista: Number(x.IdArtista || 0),
                    IdRepresentante: Number(x.IdRepresentante || 0),
                    PorcComision: Number(x.PorcComision || 0),
                    TotalComision: Number(x.TotalComision || 0)
                }))
                : [];

            VN.detalle.personal = Array.isArray(v.Personal)
                ? v.Personal.map(x => ({
                    Id: Number(x.Id || 0),
                    IdPersonal: Number(x.IdPersonal || 0),
                    IdCargo: Number(x.IdCargo || 0),
                    IdTipoComision: Number(x.IdTipoComision || 0),
                    PorcComision: Number(x.PorcComision || 0),
                    TotalComision: Number(x.TotalComision || 0)
                }))
                : [];

            VN.detalle.cobros = Array.isArray(v.Cobros)
                ? v.Cobros.map(x => {
                    const importe = Number(x.Importe || 0);
                    const cot = Number(x.Cotizacion || 1) || 1;
                    const conv = Number(x.Conversion || 0);

                    return {
                        Id: Number(x.Id || 0),
                        Fecha: x.Fecha ? new Date(x.Fecha) : new Date(),
                        IdMoneda: Number(x.IdMoneda || 0),
                        IdCuenta: Number(x.IdCuenta || 0),
                        Importe: importe,
                        Cotizacion: cot,
                        Conversion: conv > 0 ? conv : vnRound2(importe * cot), // ✅ fallback consistente
                        ManualConversion: true, // viene de backend => respetamos
                        NotaCliente: x.NotaCliente || "",
                        NotaInterna: x.NotaInterna || ""
                    };
                })
                : [];

            // =========================
            // Render + totales
            // =========================
            renderDetalle();
            recalcularTotales();

            // =========================
            // Auditoría
            // =========================
            setAuditoria(v);

            VN.flags.dirty = false;
            vnSetSaving(false, "Listo", "ok");

            // Si usás input con miles, aplicalo al final
            if (typeof aplicarFormatoMiles === "function") aplicarFormatoMiles();

        } catch (e) {
            console.error(e);
            vnToastErr("No se pudo abrir la venta.");
            vnSetSaving(false, "Error", "err");
        }
    }

    function setAuditoria(v) {

        const audReg = document.getElementById("audReg");
        const audMod = document.getElementById("audMod");

        const emptyHtml = `
        <div class="vn-empty">
            <i class="fa fa-clock-o"></i>
            <div>Sin información de auditoría</div>
            <small>La auditoría se generará cuando se registre o modifique la venta</small>
        </div>
    `;

     

        if (audMod && audReg) {

            if (v?.UsuarioModifica && v?.FechaModifica) {

                audMod.innerHTML = `
                <div class="vn-chip">
                    <i class="fa fa-edit"></i>
                    Última modificación por <b>${v.UsuarioModifica}</b>
                    • <b>${humanDate(v.FechaModifica)}</b>
                </div>
            `;

            } else {

                audMod.innerHTML = emptyHtml;

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

        aplicarFormatoMiles()

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
                    <td><input class="form-control vn-input  vn-mini vn-a-total Inputmiles" data-idx="${idx}" type="text" value="${Number(it.TotalComision || 0)}"></td>
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
                VN.detalle.artistas[idx].PorcComision = formatearMiles(Number(this.value || 0));
                calcArtistaFromPercent(idx);
                recalcularTotales();
                vnMarkDirty();
            });
        });

        // total change => recalcular %
        tb.querySelectorAll("input.vn-a-total").forEach(inp => {
            inp.addEventListener("input", function () {
                const idx = Number(this.dataset.idx);
                VN.detalle.artistas[idx].TotalComision = formatearMiles(vnToNumber(this.value));
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
        if (inp) inp.value = formatearMiles(String(it.TotalComision || 0));
    }

    function calcArtistaFromTotal(idx) {
        const it = VN.detalle.artistas[idx];
        const total = vnGetImporteTotal();
        const val = Number(it.TotalComision || 0);
        it.PorcComision = total > 0 ? vnRound2((val / total) * 100) : 0;
        const inp = document.querySelector(`input.vn-a-porc[data-idx="${idx}"]`);
        if (inp) inp.value = formatearMiles(String(it.PorcComision || 0));
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
                        <input class="form-control vn-input vn-mini vn-p-porc Inputmiles" data-idx="${idx}"
                               type="number" min="0" step="0.01" value="${Number(it.PorcComision || 0)}" ${isFixed ? "disabled" : ""}>
                    </td>
                    <td><input class="form-control vn-input vn-mini vn-p-total Inputmiles" data-idx="${idx}" type="text" value="${Number(it.TotalComision || 0)}"></td>
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
        if (inp) inp.value = formatearMiles(String(it.TotalComision || 0));
    }

    function calcPersonalFromTotal(idx) {
        const it = VN.detalle.personal[idx];
        const total = vnGetImporteTotal();
        const val = Number(it.TotalComision || 0);
        it.PorcComision = total > 0 ? vnRound2((val / total) * 100) : 0;
        const inp = document.querySelector(`input.vn-p-porc[data-idx="${idx}"]`);
        if (inp) inp.value = formatearMiles(String(it.PorcComision || 0));
    }

    function calcPersonalFixed(idx) {
        // fijo: total es el valor ingresado, porcentaje informativo
        const it = VN.detalle.personal[idx];
        const total = vnGetImporteTotal();
        const val = Number(it.TotalComision || 0);
        it.PorcComision = total > 0 ? vnRound2((val / total) * 100) : 0;
        const inp = document.querySelector(`input.vn-p-porc[data-idx="${idx}"]`);
        if (inp) inp.value = formatearMiles(String(it.PorcComision || 0));
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
                    <td><input class="form-control vn-input vn-mini vn-c-imp Inputmiles" data-idx="${idx}" type="text" value="${Number(it.Importe || 0)}"></td>
                    <td><input class="form-control vn-input vn-mini vn-c-cot Inputmiles" data-idx="${idx}" type="number" step="0.0001" min="0" value="${Number(it.Cotizacion || 1)}"></td>
                    <td><input class="form-control vn-input vn-mini vn-c-conv Inputmiles" data-idx="${idx}" type="text" value="${Number(it.Conversion || 0)}"></td>
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

    $(sel)?.select2({
        width: "100%",
        allowClear: true,
        placeholder: "Moneda"
    })
    .on("change", function () {

        const idMoneda = Number(this.value || 0);

        VN.detalle.cobros[idx].IdMoneda = idMoneda;

        // 🔹 buscar moneda
        const moneda = VN.combos.monedas.find(m => Number(m.Id) === idMoneda);

        if (moneda) {

            const cot = Number(moneda.Cotizacion || 1);

            VN.detalle.cobros[idx].Cotizacion = formatearMiles(cot);

            const cotEl = document.querySelector(`input.vn-c-cot[data-idx="${idx}"]`);

            if (cotEl)
                cotEl.value = formatearMiles(cot);

        }

        // 🔹 cargar cuentas de esa moneda
        cargarCuentasPorMoneda(idx, idMoneda);

        recalcularCobro(idx);

        vnMarkDirty();

    });

});

        // cuenta
        tb.querySelectorAll("select.vn-c-cuenta").forEach(async sel => {

            const idx = Number(sel.dataset.idx);
            const idMoneda = VN.detalle.cobros[idx].IdMoneda || 0;

            await cargarCuentasPorMoneda(idx, idMoneda);

            sel.value = String(VN.detalle.cobros[idx].IdCuenta || "");

            $(sel)?.select2({
                width: "100%",
                allowClear: true,
                placeholder: "Cuenta"
            });

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
                VN.detalle.cobros[idx].Importe = formatearMiles(vnToNumber(this.value));
                VN.detalle.cobros[idx].ManualConversion = false;
                recalcularCobro(idx);
                vnMarkDirty();
            });
        });

        // cotizacion
        tb.querySelectorAll("input.vn-c-cot").forEach(inp => {
            inp.addEventListener("input", function () {
                const idx = Number(this.dataset.idx);
                VN.detalle.cobros[idx].Cotizacion = formatearMiles(Number(this.value || 1) || 1);
                VN.detalle.cobros[idx].ManualConversion = false;
                recalcularCobro(idx);
                vnMarkDirty();
            });
        });

        // conversion manual
        tb.querySelectorAll("input.vn-c-conv").forEach(inp => {
            inp.addEventListener("input", function () {
                const idx = Number(this.dataset.idx);
                VN.detalle.cobros[idx].Conversion = formatearMiles(vnToNumber(this.value));
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

        recalcularTotales();

        const importeTotal = vnGetImporteTotal();

        const totalComisiones =
            (VN.detalle.artistas || []).reduce((a, x) => a + Number(x.TotalComision || 0), 0) +
            (VN.detalle.personal || []).reduce((a, x) => a + Number(x.TotalComision || 0), 0);

        const cobrado =
            (VN.detalle.cobros || []).reduce((a, x) => a + Number(x.Conversion || 0), 0);

        const saldo = vnRound2(importeTotal - totalComisiones - cobrado);

        const dur = document.getElementById("Duracion")?.value || "00:00";
        const [h, m] = dur.split(":").map(Number);

        const durDate = new Date();
        durDate.setHours(h || 0);
        durDate.setMinutes(m || 0);
        durDate.setSeconds(0);
        durDate.setMilliseconds(0);

        const model = {

            Id: idVenta,

            Fecha: document.getElementById("Fecha")?.value
                ? new Date(document.getElementById("Fecha").value)
                : new Date(),

            Duracion: durDate,

            IdUbicacion: Number(document.getElementById("IdUbicacion")?.value || 0),

            NombreEvento:
                document.getElementById("NombreEvento")?.value || "",

            IdCliente: Number(idCliente || 0),

            IdProductora: Number(document.getElementById("IdProductora")?.value || 0),

            IdMoneda: Number(document.getElementById("IdMoneda")?.value || 0),

            IdEstado: Number(document.getElementById("IdEstado")?.value || 0),

            ImporteTotal: vnRound2(importeTotal),

            ImporteAbonado: vnRound2(cobrado),

            Saldo: vnRound2(saldo),

            NotaInterna:
                (document.getElementById("NotaInterna")?.value || "").trim() || null,

            NotaCliente:
                (document.getElementById("NotaCliente")?.value || "").trim() || null,

            IdTipoContrato:
                Number(document.getElementById("IdTipoContrato")?.value || 0),

            IdOpExclusividad:
                document.getElementById("IdOpExclusividad")?.value
                    ? Number(document.getElementById("IdOpExclusividad").value)
                    : null,

            DiasPrevios:
                document.getElementById("DiasPrevios")?.value
                    ? Number(document.getElementById("DiasPrevios").value)
                    : null,

            FechaHasta:
                document.getElementById("FechaHasta")?.value
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

                const pct =
                    tipo === 2
                        ? (importeTotal > 0
                            ? vnRound2((total / importeTotal) * 100)
                            : 0)
                        : vnRound2(Number(x.PorcComision || 0));

                return {
                    Id: Number(x.Id || 0),
                    IdPersonal: Number(x.IdPersonal || 0),
                    IdCargo: Number(x.IdCargo || 0),
                    IdTipoComision: tipo,
                    PorcComision: pct,
                    TotalComision: total
                };

            }),

            Cobros: (VN.detalle.cobros || [])
                .filter(x => Number(x.IdCuenta || 0) > 0)
                .map(x => ({

                    Id: Number(x.Id || 0),

                    Fecha: x.Fecha
                        ? new Date(x.Fecha)
                        : new Date(),

                    IdMoneda: Number(x.IdMoneda || 0),

                    IdCuenta: Number(x.IdCuenta || 0),

                    Importe: vnRound2(Number(x.Importe || 0)),

                    Cotizacion: vnRound2(Number(x.Cotizacion || 1)),

                    Conversion: vnRound2(
                        Number(
                            x.Conversion > 0
                                ? x.Conversion
                                : (Number(x.Importe || 0) *
                                    Number(x.Cotizacion || 1))
                        )
                    ),

                    NotaCliente:
                        (x.NotaCliente || "").trim() || null,

                    NotaInterna:
                        (x.NotaInterna || "").trim() || null
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

            // limpiar draft
            localStorage.removeItem(DRAFT_KEY());
            VN.flags.dirty = false;

            vnSetSaving(false, "Guardado", "ok");

            // siempre ir al index
            setTimeout(() => {
                window.location.href = "/Ventas";
            }, 600);

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
                    ImporteTotal: formatearSinMiles(document.getElementById("ImporteTotal")?.value) || "",
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



// Mini helper UI (sin lógica de negocio): marca tile seleccionado y setea #Contrato_Formato
(function () {
    function setFormato(fmt) {
        var inp = document.getElementById("Contrato_Formato");
        if (inp) inp.value = fmt;

        var tileWord = document.getElementById("tileWord");
        var tilePdf = document.getElementById("tilePdf");

        if (tileWord) tileWord.classList.toggle("is-selected", fmt === "word");
        if (tilePdf) tilePdf.classList.toggle("is-selected", fmt === "pdf");

        var rWord = document.querySelector('input[name="ContratoFormato"][value="word"]');
        var rPdf = document.querySelector('input[name="ContratoFormato"][value="pdf"]');
        if (rWord) rWord.checked = (fmt === "word");
        if (rPdf) rPdf.checked = (fmt === "pdf");
    }

    document.addEventListener("click", function (e) {
        var tile = e.target.closest && e.target.closest(".vn-contract-tile");
        if (tile && tile.dataset && tile.dataset.formato) {
            setFormato(tile.dataset.formato);
        }
    });

    // Modal: si eligen Word/PDF desde el modal, sincroniza el selector de la sección
    document.addEventListener("click", function (e) {
        var btn = e.target.closest && e.target.closest(".vn-export-option");
        if (!btn) return;

        var tipo = btn.getAttribute("data-tipo");
        if (tipo === "word" || tipo === "pdf") setFormato(tipo);
    });

    // default
    setFormato("pdf");
})();

document.addEventListener("input", function (e) {
    const input = e.target;
    if (!input.classList || !input.classList.contains("Inputmiles")) return;

    const cursorPos = input.selectionStart || 0;
    const originalLength = input.value.length;

    const soloNumeros = input.value.replace(/\D/g, "");
    if (!soloNumeros) { input.value = ""; return; }

    const formateado = formatearMiles(soloNumeros);
    input.value = formateado;

    const newLength = formateado.length;
    const delta = newLength - originalLength;
    const newPos = Math.max(0, cursorPos + delta);

    try { input.setSelectionRange(newPos, newPos); } catch { }
});

