using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public class PersonalRepository : IPersonalRepository
    {
        private readonly SistemaLevelsContext _dbcontext;

        public PersonalRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }

        /* ================= DUPLICADOS ================= */

        public async Task<Personal?> BuscarDuplicado(
            int? idExcluir,
            string? nombre,
            string? dni,
            string? numeroDocumento)
        {
            var query = _dbcontext.Personals.AsQueryable();

            if (idExcluir.HasValue)
                query = query.Where(x => x.Id != idExcluir.Value);

            if (!string.IsNullOrWhiteSpace(dni))
            {
                var p = await query.FirstOrDefaultAsync(x => x.Dni == dni);
                if (p != null) return p;
            }

            if (!string.IsNullOrWhiteSpace(numeroDocumento))
            {
                var p = await query.FirstOrDefaultAsync(x => x.NumeroDocumento == numeroDocumento);
                if (p != null) return p;
            }

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var p = await query.FirstOrDefaultAsync(x => x.Nombre == nombre);
                if (p != null) return p;
            }

            return null;
        }

        /* ================= INSERTAR ================= */

        public async Task<bool> Insertar(
            Personal model,
            List<int> rolesIds,
            List<int> artistasIds)
        {
            using var trx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                _dbcontext.Personals.Add(model);
                await _dbcontext.SaveChangesAsync();

                foreach (var idRol in rolesIds.Distinct())
                    _dbcontext.PersonalRolesAsignados.Add(
                        new PersonalRolesAsignado
                        {
                            IdPersonal = model.Id,
                            IdRol = idRol
                        });

                foreach (var idArtista in artistasIds.Distinct())
                    _dbcontext.PersonalArtistasAsignados.Add(
                        new PersonalArtistasAsignado
                        {
                            IdPersonal = model.Id,
                            IdArtista = idArtista
                        });

                await _dbcontext.SaveChangesAsync();
                await trx.CommitAsync();

                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                return false;
            }
        }

        /* ================= ACTUALIZAR ================= */

        public async Task<bool> Actualizar(
            Personal model,
            List<int> rolesIds,
            List<int> artistasIds)
        {
            using var trx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var entity = await _dbcontext.Personals
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

                await _dbcontext.SaveChangesAsync();

                var rolesActuales = await _dbcontext.PersonalRolesAsignados
                    .Where(x => x.IdPersonal == model.Id)
                    .ToListAsync();

                _dbcontext.PersonalRolesAsignados.RemoveRange(rolesActuales);

                foreach (var idRol in rolesIds.Distinct())
                    _dbcontext.PersonalRolesAsignados.Add(
                        new PersonalRolesAsignado
                        {
                            IdPersonal = model.Id,
                            IdRol = idRol
                        });

                var artistasActuales = await _dbcontext.PersonalArtistasAsignados
                    .Where(x => x.IdPersonal == model.Id)
                    .ToListAsync();

                _dbcontext.PersonalArtistasAsignados.RemoveRange(artistasActuales);

                foreach (var idArtista in artistasIds.Distinct())
                    _dbcontext.PersonalArtistasAsignados.Add(
                        new PersonalArtistasAsignado
                        {
                            IdPersonal = model.Id,
                            IdArtista = idArtista
                        });

                await _dbcontext.SaveChangesAsync();
                await trx.CommitAsync();

                return true;
            }
            catch
            {
                await trx.RollbackAsync();
                return false;
            }
        }

        /* ================= ELIMINAR ================= */

        public async Task<bool> Eliminar(int id)
        {
            var personal = await _dbcontext.Personals.FindAsync(id);
            if (personal == null) return false;

            _dbcontext.Personals.Remove(personal);
            await _dbcontext.SaveChangesAsync();

            return true;
        }

        /* ================= RESTO ================= */

        public Task<Personal?> Obtener(int id)
            => _dbcontext.Personals.FirstOrDefaultAsync(x => x.Id == id);

        public Task<IQueryable<Personal>> ObtenerTodos()
            => Task.FromResult(_dbcontext.Personals.AsQueryable());

        public Task<List<int>> ObtenerRolesIds(int idPersonal)
            => _dbcontext.PersonalRolesAsignados
                .Where(x => x.IdPersonal == idPersonal)
                .Select(x => x.IdRol)
                .ToListAsync();

        public Task<List<int>> ObtenerArtistasIds(int idPersonal)
            => _dbcontext.PersonalArtistasAsignados
                .Where(x => x.IdPersonal == idPersonal)
                .Select(x => x.IdArtista)
                .ToListAsync();

        public Task<IQueryable<Personal>> ListarFiltrado(
            string? nombre,
            int? idPais,
            int? idTipoDocumento,
            int? idCondicionIva,
            int? idRol,
            int? idArtista)
        {
            var query = _dbcontext.Personals.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(x => x.Nombre.Contains(nombre));

            if (idPais.HasValue)
                query = query.Where(x => x.IdPais == idPais);

            return Task.FromResult(query);
        }
    }
}