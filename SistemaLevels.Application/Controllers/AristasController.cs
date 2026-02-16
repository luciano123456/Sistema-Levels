using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLevels.Application.Models.ViewModels;
using SistemaLevels.BLL.Service;
using SistemaLevels.Models;

namespace SistemaLevels.Application.Controllers
{
    [Authorize]
    public class ArtistasController : Controller
    {
        private readonly IArtistasService _service;

        public ArtistasController(IArtistasService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            try
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

                    // Documento
                    Dni = a.Dni,
                    NumeroDocumento = a.NumeroDocumento,

                    // Ubicación
                    Pais = a.IdPaisNavigation.Nombre,
                    Provincia = a.IdProvinciaNavigation.Nombre,
                    Localidad = a.Localidad,
                    Direccion = a.Direccion,

                    // Relaciones
                    TipoDocumento = a.IdTipoDocumentoNavigation != null
                        ? a.IdTipoDocumentoNavigation.Nombre
                        : "",
                    CondicionIva = a.IdCondicionIvaNavigation != null
                        ? a.IdCondicionIvaNavigation.Nombre
                        : "",
                    Representante = a.IdRepresentanteNavigation.Nombre,

                    Productora = a.IdProductoraNavigation.Nombre,

                    // Precios
                    PrecioUnitario = a.PrecioUnitario,
                    PrecioNegMin = a.PrecioNegMin,
                    PrecioNegMax = a.PrecioNegMax,

                    // Moneda (si tenés navegación)
                    Moneda = a.IdMoneda != null
                        ? a.IdMonedaNavigation.Nombre
                        : "",

                    // Fechas
                    FechaNacimiento = a.FechaNacimiento,

                    // Auditoría
                    IdUsuarioRegistra = a.IdUsuarioRegistra,
                    FechaRegistra = a.FechaRegistra,
                    UsuarioRegistra = a.IdUsuarioRegistraNavigation.Usuario,

                    IdUsuarioModifica = a.IdUsuarioModifica,
                    FechaModifica = a.FechaModifica,
                    UsuarioModifica = a.IdUsuarioModificaNavigation != null
                        ? a.IdUsuarioModificaNavigation.Usuario
                        : ""
                }).ToList();

                return Ok(lista);
            }
            catch
            {
                return Ok(new List<VMArtista>());
            }
        }


        [HttpGet]
        public async Task<IActionResult> EditarInfo(int id)
        {
            var a = await _service.Obtener(id);

            if (a == null) return Ok(null);

            var model = new VMArtista
            {
                Id = a.Id,
                Nombre = a.Nombre,
                NombreArtistico = a.NombreArtistico,
                Telefono = a.Telefono,
                TelefonoAlternativo = a.TelefonoAlternativo,
                Dni = a.Dni,
                IdPais = a.IdPais,
                IdTipoDocumento = a.IdTipoDocumento,
                NumeroDocumento = a.NumeroDocumento,
                Email = a.Email,
                IdProductora = a.IdProductora,
                IdProvincia = a.IdProvincia,
                Localidad = a.Localidad,
                EntreCalles = a.EntreCalles,
                Direccion = a.Direccion,
                CodigoPostal = a.CodigoPostal,
                IdCondicionIva = a.IdCondicionIva,
                IdRepresentante = a.IdRepresentante,
                FechaNacimiento = a.FechaNacimiento,
                IdMoneda = a.IdMoneda,
                PrecioUnitario = a.PrecioUnitario,
                PrecioNegMax = a.PrecioNegMax,
                PrecioNegMin = a.PrecioNegMin,

                IdUsuarioRegistra = a.IdUsuarioRegistra,
                FechaRegistra = a.FechaRegistra,
                UsuarioRegistra = a.IdUsuarioRegistraNavigation?.Usuario,

                IdUsuarioModifica = a.IdUsuarioModifica,
                FechaModifica = a.FechaModifica,
                UsuarioModifica = a.IdUsuarioModificaNavigation?.Usuario
            };

            return Ok(model);
        }

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
                IdRepresentante = model.IdRepresentante,
                FechaNacimiento = model.FechaNacimiento,
                IdMoneda = model.IdMoneda,
                PrecioUnitario = model.PrecioUnitario,
                PrecioNegMax = model.PrecioNegMax,
                PrecioNegMin = model.PrecioNegMin,

                IdUsuarioRegistra = idUsuario,
                FechaRegistra = DateTime.Now
            };

            bool respuesta = await _service.Insertar(artista);
            return Ok(new { valor = respuesta });
        }

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
                IdRepresentante = model.IdRepresentante,
                FechaNacimiento = model.FechaNacimiento,
                IdMoneda = model.IdMoneda,
                PrecioUnitario = model.PrecioUnitario,
                PrecioNegMax = model.PrecioNegMax,
                PrecioNegMin = model.PrecioNegMin,

                IdUsuarioModifica = idUsuario,
                FechaModifica = DateTime.Now
            };

            bool respuesta = await _service.Actualizar(artista);
            return Ok(new { valor = respuesta });
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            bool respuesta = await _service.Eliminar(id);
            return Ok(new { valor = respuesta });
        }
    }
}
