using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class ArtistasRepository : IArtistasRepository<Artista>
    {
        private readonly SistemaLevelsContext _db;

        public ArtistasRepository(SistemaLevelsContext context)
        {
            _db = context;
        }

        /* =====================================================
           INSERTAR
        ===================================================== */

        public async Task<bool> Insertar(
            Artista model,
            List<int> personalIds)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                _db.Artistas.Add(model);
                await _db.SaveChangesAsync();

                await SincronizarPersonal(
                    model.Id,
                    personalIds,
                    2 // origen ARTISTA
                );

                await trx.CommitAsync();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                return false;
            }
        }

        /* =====================================================
           ACTUALIZAR
        ===================================================== */

        public async Task<bool> Actualizar(
            Artista model,
            List<int> personalIds)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var entity = await _db.Artistas
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null)
                    return false;

                entity.Nombre = model.Nombre;
                entity.NombreArtistico = model.NombreArtistico;
                entity.Telefono = model.Telefono;
                entity.TelefonoAlternativo = model.TelefonoAlternativo;
                entity.Email = model.Email;
                entity.Dni = model.Dni;

                entity.IdPais = model.IdPais;
                entity.IdProvincia = model.IdProvincia;
                entity.IdTipoDocumento = model.IdTipoDocumento;
                entity.NumeroDocumento = model.NumeroDocumento;
                entity.IdCondicionIva = model.IdCondicionIva;

                entity.IdProductora = model.IdProductora;
                entity.IdMoneda = model.IdMoneda;

                entity.Localidad = model.Localidad;
                entity.EntreCalles = model.EntreCalles;
                entity.Direccion = model.Direccion;
                entity.CodigoPostal = model.CodigoPostal;

                entity.FechaNacimiento = model.FechaNacimiento;

                entity.PrecioUnitario = model.PrecioUnitario;
                entity.PrecioNegMin = model.PrecioNegMin;
                entity.PrecioNegMax = model.PrecioNegMax;

                entity.IdUsuarioModifica = model.IdUsuarioModifica;
                entity.FechaModifica = DateTime.Now;

                await _db.SaveChangesAsync();

                await SincronizarPersonal(
                    entity.Id,
                    personalIds,
                    2
                );

                await trx.CommitAsync();
                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                return false;
            }
        }

        /* =====================================================
           BUSCAR DUPLICADO
        ===================================================== */

        public async Task<Artista?> BuscarDuplicado(
            int? idExcluir,
            string? nombre,
            string? numeroDocumento,
            string? dni)
        {
            var query = _db.Artistas.AsQueryable();

            if (idExcluir.HasValue)
                query = query.Where(x => x.Id != idExcluir.Value);

            if (!string.IsNullOrWhiteSpace(dni))
            {
                var dup = await query.FirstOrDefaultAsync(x => x.Dni == dni);
                if (dup != null) return dup;
            }

            if (!string.IsNullOrWhiteSpace(numeroDocumento))
            {
                var dup = await query.FirstOrDefaultAsync(x => x.NumeroDocumento == numeroDocumento);
                if (dup != null) return dup;
            }

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var dup = await query.FirstOrDefaultAsync(x => x.Nombre == nombre);
                if (dup != null) return dup;
            }

            return null;
        }

        /* =====================================================
           ⭐ SINCRONIZADOR PERSONAL (CLON CLIENTES)
        ===================================================== */

        private async Task SincronizarPersonal(
            int idArtista,
            List<int> nuevasIds,
            byte origenAsignacion)
        {
            nuevasIds ??= new List<int>();

            nuevasIds = nuevasIds
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            var actuales = await _db.PersonalesArtistas
                .Where(x =>
                    x.IdArtista == idArtista &&
                    x.OrigenAsignacion == origenAsignacion)
                .ToListAsync();

            _db.PersonalesArtistas.RemoveRange(actuales);

            foreach (var idPersonal in nuevasIds)
            {
                _db.PersonalesArtistas.Add(new PersonalesArtista
                {
                    IdArtista = idArtista,
                    IdPersonal = idPersonal,
                    OrigenAsignacion = origenAsignacion,
                    FechaRegistro = DateTime.Now
                });
            }

            await _db.SaveChangesAsync();
        }

        /* =====================================================
           ELIMINAR
        ===================================================== */

        public async Task<bool> Eliminar(int id)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var artista = await _db.Artistas
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (artista == null)
                    return false;

                var relaciones = await _db.PersonalesArtistas
                    .Where(x => x.IdArtista == id)
                    .ToListAsync();

                _db.PersonalesArtistas.RemoveRange(relaciones);
                _db.Artistas.Remove(artista);

                await _db.SaveChangesAsync();
                await trx.CommitAsync();

                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                return false;
            }
        }

        /* =====================================================
           OBTENER
        ===================================================== */

        public async Task<Artista?> Obtener(int id)
        {

            try
            {
                return await _db.Artistas
                    .AsNoTracking()

                    // =============================
                    // RELACION PERSONAL ⭐⭐⭐
                    // =============================
                    .Include(x => x.PersonalesArtista)
                        .ThenInclude(pa => pa.IdPersonalNavigation)

                    // =============================
                    // NAVEGACIONES NORMALES
                    // =============================
                    .Include(x => x.IdPaisNavigation)
                    .Include(x => x.IdProvinciaNavigation)
                    .Include(x => x.IdTipoDocumentoNavigation)
                    .Include(x => x.IdCondicionIvaNavigation)
                   
                    .Include(x => x.IdProductoraNavigation)
                    .Include(x => x.IdMonedaNavigation)

                    .Include(x => x.IdUsuarioRegistraNavigation)
                    .Include(x => x.IdUsuarioModificaNavigation)

                    .FirstOrDefaultAsync(x => x.Id == id);

            } catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IQueryable<Artista>> ObtenerTodos()
        {
            var query = _db.Artistas
                .AsNoTracking()

                // ⭐ RELACION PERSONAL
                .Include(x => x.PersonalesArtista)
                    .ThenInclude(pa => pa.IdPersonalNavigation)

                .Include(x => x.IdPaisNavigation)
                .Include(x => x.IdProvinciaNavigation)
                .Include(x => x.IdTipoDocumentoNavigation)
                .Include(x => x.IdCondicionIvaNavigation)
                .Include(x => x.IdProductoraNavigation)
                .Include(x => x.IdMonedaNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation);

            return await Task.FromResult(query);
        }
    }
}