using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Common;
using SistemaLevels.BLL.Service;
using SistemaLevels.Models;

namespace SistemaLevels.Controllers
{
    [Authorize]
    public class ArtistasController : Controller
    {
        private readonly IArtistasService _service;

        public ArtistasController(IArtistasService service)
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
            var artistas = await _service.ObtenerTodos();

            var lista = artistas.Select(a => new VMArtista
            {
                Id = a.Id,
                Nombre = a.Nombre,
                NombreArtistico = a.NombreArtistico,
                Telefono = a.Telefono,
                TelefonoAlternativo = a.TelefonoAlternativo,
                Email = a.Email,

                Dni = a.Dni,
                NumeroDocumento = a.NumeroDocumento,

                IdPais = a.IdPais,
                IdProvincia = a.IdProvincia,
                IdTipoDocumento = a.IdTipoDocumento,
                IdCondicionIva = a.IdCondicionIva,

                Pais = a.IdPaisNavigation != null
                    ? a.IdPaisNavigation.Nombre : "",

                Provincia = a.IdProvinciaNavigation != null
                    ? a.IdProvinciaNavigation.Nombre : "",

                TipoDocumento = a.IdTipoDocumentoNavigation != null
                    ? a.IdTipoDocumentoNavigation.Nombre : "",

                CondicionIva = a.IdCondicionIvaNavigation != null
                    ? a.IdCondicionIvaNavigation.Nombre : "",

                Productora = a.IdProductoraNavigation != null
                    ? a.IdProductoraNavigation.Nombre : "",

                Moneda = a.IdMonedaNavigation != null
                    ? a.IdMonedaNavigation.Nombre : "",

                PrecioUnitario = a.PrecioUnitario ?? 0,

                // ⭐⭐⭐ EXACTAMENTE IGUAL LOGICA CLIENTES
                Representante = (a.PersonalesArtista != null && a.PersonalesArtista.Count > 0)
                    ? (a.PersonalesArtista.Count == 1
                        ? (a.PersonalesArtista.First().IdPersonalNavigation != null
                            ? a.PersonalesArtista.First().IdPersonalNavigation.Nombre
                            : "1 asignado")
                        : $"{a.PersonalesArtista.Count} asignados")
                    : "",

                IdUsuarioRegistra = a.IdUsuarioRegistra,
                FechaRegistra = a.FechaRegistra,
                UsuarioRegistra = a.IdUsuarioRegistraNavigation != null
                    ? a.IdUsuarioRegistraNavigation.Usuario : "",

                IdUsuarioModifica = a.IdUsuarioModifica,
                FechaModifica = a.FechaModifica,
                UsuarioModifica = a.IdUsuarioModificaNavigation != null
                    ? a.IdUsuarioModificaNavigation.Usuario : ""
            }).ToList();

            return Ok(lista);
        }

        /* =====================================================
           EDITAR INFO (PERSONAL ASIGNADO)
        ===================================================== */

        [HttpGet]
        public async Task<IActionResult> EditarInfo(int id)
        {
            var a = await _service.Obtener(id);

            if (a == null)
                return NotFound();

            var manual = a.PersonalesArtista
                .Where(x => x.OrigenAsignacion == 2)
                .Select(x => x.IdPersonal)
                .ToList();

            var automaticos = a.PersonalesArtista
                .Where(x => x.OrigenAsignacion == 3)
                .Select(x => x.IdPersonal)
                .ToList();

            var desdePersonal = a.PersonalesArtista
                .Where(x => x.OrigenAsignacion == 1)
                .Select(x => x.IdPersonal)
                .ToList();

            return Ok(new
            {
                a.Id,
                a.Nombre,
                a.NombreArtistico,
                a.Telefono,
                a.TelefonoAlternativo,
                a.Email,

                a.Dni,
                a.NumeroDocumento,

                a.IdPais,
                a.IdProvincia,
                a.IdTipoDocumento,
                a.IdCondicionIva,
                a.IdProductora,
                a.IdMoneda,

                a.Direccion,
                a.Localidad,
                a.EntreCalles,
                a.CodigoPostal,

                a.FechaNacimiento,
                a.PrecioUnitario,
                a.PrecioNegMax,
                a.PrecioNegMin,

                PersonalIds = manual,
                PersonalAutomaticosIds = automaticos,
                PersonalDesdePersonalIds = desdePersonal,

                a.FechaRegistra,
                UsuarioRegistra = a.IdUsuarioRegistraNavigation?.Usuario,
                a.FechaModifica,
                UsuarioModifica = a.IdUsuarioModificaNavigation?.Usuario
            });
        }

        /* =====================================================
           INSERTAR
        ===================================================== */

        [HttpPost]
        public async Task<IActionResult> Insertar([FromBody] VMArtista model)
        {
            int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

            var artista = new Artista
            {
                Nombre = model.Nombre,
                NombreArtistico = model.NombreArtistico,
                Telefono = model.Telefono,
                TelefonoAlternativo = model.TelefonoAlternativo,
                Dni = model.Dni,
                IdPais = model.IdPais,
                IdTipoDocumento = model.IdTipoDocumento,
                NumeroDocumento = model.NumeroDocumento,
                Email = model.Email,
                IdProductora = model.IdProductora,
                IdProvincia = model.IdProvincia,
                Localidad = model.Localidad,
                EntreCalles = model.EntreCalles,
                Direccion = model.Direccion,
                CodigoPostal = model.CodigoPostal,
                IdCondicionIva = model.IdCondicionIva,
                FechaNacimiento = model.FechaNacimiento,
                IdMoneda = model.IdMoneda,
                PrecioUnitario = model.PrecioUnitario,
                PrecioNegMax = model.PrecioNegMax,
                PrecioNegMin = model.PrecioNegMin,
                IdUsuarioRegistra = idUsuario,
                FechaRegistra = DateTime.Now
            };

            ServiceResult result = await _service.Insertar(
                artista,
                model.PersonalIds ?? new List<int>()
            );

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
        public async Task<IActionResult> Actualizar([FromBody] VMArtista model)
        {
            int idUsuario = int.Parse(User.FindFirst("Id")!.Value);

            var artista = new Artista
            {
                Id = model.Id,
                Nombre = model.Nombre,
                NombreArtistico = model.NombreArtistico,
                Telefono = model.Telefono,
                TelefonoAlternativo = model.TelefonoAlternativo,
                Dni = model.Dni,
                IdPais = model.IdPais,
                IdTipoDocumento = model.IdTipoDocumento,
                NumeroDocumento = model.NumeroDocumento,
                Email = model.Email,
                IdProductora = model.IdProductora,
                IdProvincia = model.IdProvincia,
                Localidad = model.Localidad,
                EntreCalles = model.EntreCalles,
                Direccion = model.Direccion,
                CodigoPostal = model.CodigoPostal,
                IdCondicionIva = model.IdCondicionIva,
                FechaNacimiento = model.FechaNacimiento,
                IdMoneda = model.IdMoneda,
                PrecioUnitario = model.PrecioUnitario,
                PrecioNegMax = model.PrecioNegMax,
                PrecioNegMin = model.PrecioNegMin,
                IdUsuarioModifica = idUsuario,
                FechaModifica = DateTime.Now
            };

            ServiceResult result = await _service.Actualizar(
                artista,
                model.PersonalIds ?? new List<int>()
            );

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
            ServiceResult result = await _service.Eliminar(id);

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