using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class ArtistasRepository : IArtistasRepository<Artista>
    {
        private readonly SistemaLevelsContext _dbcontext;

        public ArtistasRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }

        public async Task<bool> Insertar(Artista model)
        {
            try
            {
                if (model.FechaNacimiento.HasValue &&
                    model.FechaNacimiento.Value < new DateTime(1753, 1, 1))
                {
                    model.FechaNacimiento = null;
                }

                _dbcontext.Artistas.Add(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Actualizar(Artista model)
        {
            try
            {
                var entity = await _dbcontext.Artistas
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null) return false;

                entity.Nombre = model.Nombre;
                entity.NombreArtistico = model.NombreArtistico;
                entity.Telefono = model.Telefono;
                entity.TelefonoAlternativo = model.TelefonoAlternativo;
                entity.Dni = model.Dni;
                entity.IdPais = model.IdPais;
                entity.IdTipoDocumento = model.IdTipoDocumento;
                entity.NumeroDocumento = model.NumeroDocumento;
                entity.Email = model.Email;
                entity.IdProductora = model.IdProductora;
                entity.IdProvincia = model.IdProvincia;
                entity.Localidad = model.Localidad;
                entity.EntreCalles = model.EntreCalles;
                entity.Direccion = model.Direccion;
                entity.CodigoPostal = model.CodigoPostal;
                entity.IdCondicionIva = model.IdCondicionIva;
                entity.IdRepresentante = model.IdRepresentante;

                if (model.FechaNacimiento.HasValue &&
                    model.FechaNacimiento.Value >= new DateTime(1753, 1, 1))
                    entity.FechaNacimiento = model.FechaNacimiento;
                else
                    entity.FechaNacimiento = null;

                entity.IdMoneda = model.IdMoneda;
                entity.PrecioUnitario = model.PrecioUnitario;
                entity.PrecioNegMax = model.PrecioNegMax;
                entity.PrecioNegMin = model.PrecioNegMin;

                entity.IdUsuarioModifica = model.IdUsuarioModifica;
                entity.FechaModifica = model.FechaModifica;

                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                var entity = await _dbcontext.Artistas.FindAsync(id);
                if (entity == null) return false;

                _dbcontext.Artistas.Remove(entity);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Artista?> Obtener(int id)
        {
            return await _dbcontext.Artistas
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IQueryable<Artista>> ObtenerTodos()
        {
            var query = _dbcontext.Artistas
                .Include(x => x.IdPaisNavigation)
                .Include(x => x.IdTipoDocumentoNavigation)
                .Include(x => x.IdCondicionIvaNavigation)
                .Include(x => x.IdProvinciaNavigation)
                .Include(x => x.IdRepresentanteNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation)
                .AsQueryable();

            return await Task.FromResult(query);
        }

        public async Task<Artista?> BuscarDuplicado(
    int? idExcluir,
    string? nombre,
    string? nombreArtistico,
    string? numeroDocumento,
    string? dni)
        {
            var query = _dbcontext.Artistas.AsQueryable();

            if (idExcluir.HasValue)
                query = query.Where(x => x.Id != idExcluir.Value);

            if (!string.IsNullOrWhiteSpace(dni))
            {
                var a = await query.FirstOrDefaultAsync(x => x.Dni == dni);
                if (a != null) return a;
            }

            if (!string.IsNullOrWhiteSpace(numeroDocumento))
            {
                var a = await query.FirstOrDefaultAsync(x => x.NumeroDocumento == numeroDocumento);
                if (a != null) return a;
            }

            if (!string.IsNullOrWhiteSpace(nombreArtistico))
            {
                var a = await query.FirstOrDefaultAsync(x => x.NombreArtistico == nombreArtistico);
                if (a != null) return a;
            }

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var a = await query.FirstOrDefaultAsync(x => x.Nombre == nombre);
                if (a != null) return a;
            }

            return null;
        }
    }
}
