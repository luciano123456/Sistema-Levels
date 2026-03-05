using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLevels.BLL.Common;
using System.IO;

namespace SistemaLevels.Controllers
{
    [Authorize]
    public class ContratosController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public ContratosController(IWebHostEnvironment env)
        {
            _env = env;
            EnsureFolder();
        }

        private string ContratosFolderPath()
        {
            // carpeta al lado del proyecto (más seguro que wwwroot si no querés exposición directa)
            // si preferís webroot: _env.WebRootPath
            return Path.Combine(_env.ContentRootPath, "Contratos");
        }

        private void EnsureFolder()
        {
            var dir = ContratosFolderPath();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        private string TemplatePath(int idTipoContrato)
            => Path.Combine(ContratosFolderPath(), $"{idTipoContrato}.docx");

        /* =====================================================
           LISTA (estado por tipo)
           Devuelve: [{ IdTipoContrato, Existe, Archivo, FechaModifica }]
        ===================================================== */

        [HttpGet]
        public IActionResult Lista()
        {
            EnsureFolder();
            var dir = ContratosFolderPath();

            var archivos = Directory.GetFiles(dir, "*.docx", SearchOption.TopDirectoryOnly)
                .Select(p => new FileInfo(p))
                .Select(fi =>
                {
                    // nombre = "12.docx"
                    var name = Path.GetFileNameWithoutExtension(fi.Name);
                    int.TryParse(name, out var idTipoContrato);

                    return new
                    {
                        IdTipoContrato = idTipoContrato,
                        Existe = true,
                        Archivo = fi.Name,
                        FechaModifica = fi.LastWriteTime
                    };
                })
                .Where(x => x.IdTipoContrato > 0)
                .OrderBy(x => x.IdTipoContrato)
                .ToList();

            return Ok(archivos);
        }

        /* =====================================================
           DESCARGAR PLANTILLA
        ===================================================== */

        [HttpGet]
        public IActionResult Descargar(int idTipoContrato, string? nombre = null)
        {
            EnsureFolder();

            var path = TemplatePath(idTipoContrato);
            if (!System.IO.File.Exists(path))
                return NotFound(new { valor = false, mensaje = "No existe plantilla para ese tipo de contrato." });

            var bytes = System.IO.File.ReadAllBytes(path);

            var safeName = string.IsNullOrWhiteSpace(nombre)
                ? $"Contrato_{idTipoContrato}.docx"
                : $"{SanitizeFileName(nombre)}.docx";

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                safeName);
        }

        /* =====================================================
           SUBIR/REEMPLAZAR PLANTILLA
           POST multipart/form-data: file
        ===================================================== */

        [HttpPost]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> Subir(int idTipoContrato, IFormFile file)
        {
            EnsureFolder();

            if (idTipoContrato <= 0)
                return Ok(new { valor = false, mensaje = "IdTipoContrato inválido.", tipo = "validacion" });

            if (file == null || file.Length == 0)
                return Ok(new { valor = false, mensaje = "Debe seleccionar un archivo.", tipo = "validacion" });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".docx")
                return Ok(new { valor = false, mensaje = "Solo se permite .docx", tipo = "validacion" });

            var path = TemplatePath(idTipoContrato);

            try
            {
                using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                await file.CopyToAsync(fs);

                return Ok(new
                {
                    valor = true,
                    mensaje = "Plantilla guardada correctamente.",
                    tipo = "ok",
                    idReferencia = idTipoContrato
                });
            }
            catch
            {
                return Ok(new { valor = false, mensaje = "No se pudo guardar la plantilla.", tipo = "error" });
            }
        }

        /* =====================================================
           ELIMINAR PLANTILLA (opcional)
        ===================================================== */

        [HttpDelete]
        public IActionResult Eliminar(int idTipoContrato)
        {
            EnsureFolder();
            var path = TemplatePath(idTipoContrato);

            try
            {
                if (!System.IO.File.Exists(path))
                    return Ok(new { valor = false, mensaje = "No existe plantilla.", tipo = "validacion" });

                System.IO.File.Delete(path);

                return Ok(new { valor = true, mensaje = "Plantilla eliminada.", tipo = "ok", idReferencia = idTipoContrato });
            }
            catch
            {
                return Ok(new { valor = false, mensaje = "No se pudo eliminar.", tipo = "error" });
            }
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Trim();
        }
    }
}