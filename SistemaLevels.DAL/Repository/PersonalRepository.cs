using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class PersonalRepository : IPersonalRepository
    {
        private readonly SistemaLevelsContext _db;

        public PersonalRepository(SistemaLevelsContext context)
        {
            _db = context;
        }

        /* =====================================================
           SINCRONIZAR ARTISTAS
        ===================================================== */

        private async Task SincronizarArtistas(
            int idPersonal,
            List<int> artistasIds,
            byte origen = 1)
        {
            artistasIds ??= new();

            artistasIds = artistasIds
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            var actuales = await _db.PersonalesArtistas
                .Where(x =>
                    x.IdPersonal == idPersonal &&
                    x.OrigenAsignacion == origen)
                .ToListAsync();

            _db.PersonalesArtistas.RemoveRange(actuales);

            foreach (var idArtista in artistasIds)
            {
                _db.PersonalesArtistas.Add(new PersonalesArtista
                {
                    IdPersonal = idPersonal,
                    IdArtista = idArtista,
                    OrigenAsignacion = origen,
                    FechaRegistro = DateTime.Now
                });
            }
        }

        /* =====================================================
           INSERTAR
        ===================================================== */

        public async Task<bool> Insertar(
            Personal model,
            List<int> rolesIds,
            List<int> artistasIds)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                _db.Personals.Add(model);
                await _db.SaveChangesAsync();

                foreach (var idRol in rolesIds.Distinct())
                {
                    _db.PersonalRolesAsignados.Add(new PersonalRolesAsignado
                    {
                        IdPersonal = model.Id,
                        IdRol = idRol
                    });
                }

                await SincronizarArtistas(model.Id, artistasIds);

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
           ACTUALIZAR
        ===================================================== */

        public async Task<bool> Actualizar(
            Personal model,
            List<int> rolesIds,
            List<int> artistasIds)
        {
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                var entity = await _db.Personals
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null) return false;

                entity.Nombre = model.Nombre;
                entity.Dni = model.Dni;
                entity.NumeroDocumento = model.NumeroDocumento;
                entity.Telefono = model.Telefono;
                entity.Email = model.Email;
                entity.Direccion = model.Direccion;
                entity.IdPais = model.IdPais;
                entity.IdTipoDocumento = model.IdTipoDocumento;
                entity.IdCondicionIva = model.IdCondicionIva;
                entity.FechaNacimiento = model.FechaNacimiento;
                entity.IdUsuarioModifica = model.IdUsuarioModifica;
                entity.FechaModifica = model.FechaModifica;

                await _db.SaveChangesAsync();

                var rolesActuales = await _db.PersonalRolesAsignados
                    .Where(x => x.IdPersonal == model.Id)
                    .ToListAsync();

                _db.PersonalRolesAsignados.RemoveRange(rolesActuales);

                foreach (var idRol in rolesIds.Distinct())
                {
                    _db.PersonalRolesAsignados.Add(new PersonalRolesAsignado
                    {
                        IdPersonal = model.Id,
                        IdRol = idRol
                    });
                }

                await SincronizarArtistas(model.Id, artistasIds);

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
           ELIMINAR
        ===================================================== */

        public async Task<bool> Eliminar(int id)
        {
            var entity = await _db.Personals.FindAsync(id);
            if (entity == null) return false;

            var relaciones = await _db.PersonalesArtistas
                .Where(x => x.IdPersonal == id)
                .ToListAsync();

            _db.PersonalesArtistas.RemoveRange(relaciones);

            _db.Personals.Remove(entity);

            await _db.SaveChangesAsync();
            return true;
        }

        /* =====================================================
           OBTENER
        ===================================================== */

        public async Task<Personal?> Obtener(int id)
        {
            return await _db.Personals
                .AsNoTracking()
                .Include(x => x.PersonalesArtista)
                    .ThenInclude(pa => pa.IdArtistaNavigation)
                .Include(x => x.IdPaisNavigation)
                .Include(x => x.IdTipoDocumentoNavigation)
                .Include(x => x.IdCondicionIvaNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IQueryable<Personal>> ObtenerTodos()
        {
            var query = _db.Personals
                .AsNoTracking()
                .Include(x => x.PersonalesArtista)
                    .ThenInclude(pa => pa.IdArtistaNavigation)
                .Include(x => x.IdPaisNavigation)
                .Include(x => x.IdTipoDocumentoNavigation)
                .Include(x => x.IdCondicionIvaNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation);

            return await Task.FromResult(query);
        }

        /* =====================================================
           ⭐ LISTAR FILTRADO (FALTANTE)
        ===================================================== */

        public Task<IQueryable<Personal>> ListarFiltrado(
            string? nombre,
            int? idPais,
            int? idTipoDocumento,
            int? idCondicionIva,
            int? idRol,
            int? idArtista)
        {
            var query = _db.Personals
                .Include(x => x.PersonalRolesAsignados)
                .Include(x => x.PersonalesArtista)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(x => x.Nombre.Contains(nombre));

            if (idPais.HasValue)
                query = query.Where(x => x.IdPais == idPais);

            if (idTipoDocumento.HasValue)
                query = query.Where(x => x.IdTipoDocumento == idTipoDocumento);

            if (idCondicionIva.HasValue)
                query = query.Where(x => x.IdCondicionIva == idCondicionIva);

            if (idRol.HasValue)
                query = query.Where(x =>
                    x.PersonalRolesAsignados.Any(r => r.IdRol == idRol));

            if (idArtista.HasValue)
                query = query.Where(x =>
                    x.PersonalesArtista.Any(a => a.IdArtista == idArtista));

            return Task.FromResult(query);
        }

        /* =====================================================
           DUPLICADOS
        ===================================================== */

        public Task<Personal?> BuscarDuplicado(
            int? idExcluir,
            string? nombre,
            string? dni,
            string? numeroDocumento)
        {
            var query = _db.Personals.AsQueryable();

            if (idExcluir.HasValue)
                query = query.Where(x => x.Id != idExcluir);

            return query.FirstOrDefaultAsync(x =>
                x.Nombre == nombre ||
                x.Dni == dni ||
                x.NumeroDocumento == numeroDocumento);
        }

        public Task<List<int>> ObtenerRolesIds(int idPersonal)
            => _db.PersonalRolesAsignados
                .Where(x => x.IdPersonal == idPersonal)
                .Select(x => x.IdRol)
                .ToListAsync();

        public Task<List<int>> ObtenerArtistasIds(int idPersonal)
            => _db.PersonalesArtistas
                .Where(x => x.IdPersonal == idPersonal)
                .Select(x => x.IdArtista)
                .ToListAsync();
    }
}