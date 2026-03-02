using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Common;
using SistemaLevels.BLL.Service;
using SistemaLevels.Models;

namespace SistemaLevels.Controllers
{
    [Authorize]
    public class PersonalController : Controller
    {
        private readonly IPersonalService _service;

        public PersonalController(IPersonalService service)
        {
            _service = service;
        }

        /* =====================================================
           INDEX
        ===================================================== */

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        /* =====================================================
           LISTA
        ===================================================== */

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            var personals = await _service.ObtenerTodos();

            var lista = personals.Select(p => new VMPersonal
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Dni = p.Dni,
                Telefono = p.Telefono,
                Email = p.Email,
                Direccion = p.Direccion,
                NumeroDocumento = p.NumeroDocumento,
                FechaNacimiento = p.FechaNacimiento,

                Pais = p.IdPaisNavigation != null
                    ? p.IdPaisNavigation.Nombre : "",

                TipoDocumento = p.IdTipoDocumentoNavigation != null
                    ? p.IdTipoDocumentoNavigation.Nombre : "",

                CondicionIva = p.IdCondicionIvaNavigation != null
                    ? p.IdCondicionIvaNavigation.Nombre : "",

                // ⭐ MISMA LOGICA QUE CLIENTES / ARTISTAS
                Artista = (p.PersonalesArtista != null &&
                            p.PersonalesArtista.Count > 0)
                    ? (p.PersonalesArtista.Count == 1
                        ? (p.PersonalesArtista.First().IdArtistaNavigation != null
                            ? p.PersonalesArtista.First().IdArtistaNavigation.Nombre
                            : "1 asignado")
                        : $"{p.PersonalesArtista.Count} asignados")
                    : "",

                IdUsuarioRegistra = p.IdUsuarioRegistra,
                FechaRegistra = p.FechaRegistra,
                UsuarioRegistra = p.IdUsuarioRegistraNavigation.Usuario ?? "",

                IdUsuarioModifica = p.IdUsuarioModifica,
                FechaModifica = p.FechaModifica,
                UsuarioModifica = p.IdUsuarioModificaNavigation.Usuario ?? ""
            }).ToList();

            return Ok(lista);
        }

        /* =====================================================
           EDITAR INFO
        ===================================================== */

        [HttpGet]
        public async Task<IActionResult> EditarInfo(int id)
        {
            var p = await _service.Obtener(id);
            if (p == null) return NotFound();

            var manual = p.PersonalesArtista
                .Where(x => x.OrigenAsignacion == 1)
                .Select(x => x.IdArtista)
                .ToList();

            var automaticos = p.PersonalesArtista
                .Where(x => x.OrigenAsignacion == 3)
                .Select(x => x.IdArtista)
                .ToList();

            var desdeArtista = p.PersonalesArtista
                .Where(x => x.OrigenAsignacion == 2)
                .Select(x => x.IdArtista)
                .ToList();

            var rolesIds = await _service.ObtenerRolesIds(id);

            return Ok(new
            {
                p.Id,
                p.Nombre,
                p.Dni,
                p.Telefono,
                p.Email,
                p.Direccion,
                p.IdPais,
                p.IdTipoDocumento,
                p.NumeroDocumento,
                p.IdCondicionIva,
                p.FechaNacimiento,

                RolesIds = rolesIds,

                // ⭐ EXACTO ESPEJO ARTISTAS
                ArtistasIds = manual,
                ArtistasAutomaticosIds = automaticos,
                ArtistasDesdeArtistaIds = desdeArtista,

                p.FechaRegistra,
                UsuarioRegistra = p.IdUsuarioRegistraNavigation?.Usuario,
                p.FechaModifica,
                UsuarioModifica = p.IdUsuarioModificaNavigation?.Usuario
            });
        }
        /* =====================================================
           INSERTAR
        ===================================================== */

        [HttpPost]
        public async Task<IActionResult> Insertar([FromBody] VMPersonal model)
        {
            int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

            var personal = new Personal
            {
                Nombre = model.Nombre,
                Dni = model.Dni,
                Telefono = model.Telefono,
                Email = model.Email,
                Direccion = model.Direccion,
                IdPais = model.IdPais,
                IdTipoDocumento = model.IdTipoDocumento,
                NumeroDocumento = model.NumeroDocumento,
                IdCondicionIva = model.IdCondicionIva,
                FechaNacimiento = model.FechaNacimiento,
                IdUsuarioRegistra = idUsuario,
                FechaRegistra = DateTime.Now
            };

            var result = await _service.Insertar(
                personal,
                model.RolesIds ?? new(),
                model.ArtistasIds ?? new());

            return Ok(new
            {
                valor = result.Ok,
                mensaje = result.Mensaje,
                tipo = result.Tipo,
                idReferencia = result.IdReferencia
            });
        }

        /* =====================================================
           ACTUALIZAR
        ===================================================== */

        [HttpPut]
        public async Task<IActionResult> Actualizar([FromBody] VMPersonal model)
        {
            int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

            var personal = new Personal
            {
                Id = model.Id,
                Nombre = model.Nombre,
                Dni = model.Dni,
                Telefono = model.Telefono,
                Email = model.Email,
                Direccion = model.Direccion,
                IdPais = model.IdPais,
                IdTipoDocumento = model.IdTipoDocumento,
                NumeroDocumento = model.NumeroDocumento,
                IdCondicionIva = model.IdCondicionIva,
                FechaNacimiento = model.FechaNacimiento,
                IdUsuarioModifica = idUsuario,
                FechaModifica = DateTime.Now
            };

            var result = await _service.Actualizar(
                personal,
                model.RolesIds ?? new(),
                model.ArtistasIds ?? new());

            return Ok(new
            {
                valor = result.Ok,
                mensaje = result.Mensaje,
                tipo = result.Tipo,
                idReferencia = result.IdReferencia
            });
        }

        /* =====================================================
           ELIMINAR
        ===================================================== */

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            var result = await _service.Eliminar(id);

            return Ok(new
            {
                valor = result.Ok,
                mensaje = result.Mensaje,
                tipo = result.Tipo,
                idReferencia = result.IdReferencia
            });
        }
    }
}