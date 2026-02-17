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

        public async Task<bool> Insertar(Personal model, List<int> rolesIds, List<int> artistasIds)
        {
            using var trx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                _dbcontext.Personals.Add(model);
                await _dbcontext.SaveChangesAsync();

                // roles
                rolesIds ??= new List<int>();
                foreach (var idRol in rolesIds.Distinct())
                {
                    _dbcontext.PersonalRolesAsignados.Add(new PersonalRolesAsignado
                    {
                        IdPersonal = model.Id,
                        IdRol = idRol
                    });
                }

                // artistas (solo si corresponde, el front lo manda si aplica)
                artistasIds ??= new List<int>();
                foreach (var idArtista in artistasIds.Distinct())
                {
                    _dbcontext.PersonalArtistasAsignados.Add(new PersonalArtistasAsignado
                    {
                        IdPersonal = model.Id,
                        IdArtista = idArtista
                    });
                }

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

        public async Task<bool> Actualizar(Personal model, List<int> rolesIds, List<int> artistasIds)
        {
            using var trx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var entity = await _dbcontext.Personals
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (entity == null) return false;

                entity.Nombre = model.Nombre;
                entity.Dni = model.Dni;
                entity.IdPais = model.IdPais;
                entity.IdTipoDocumento = model.IdTipoDocumento;
                entity.NumeroDocumento = model.NumeroDocumento;
                entity.Direccion = model.Direccion;
                entity.Telefono = model.Telefono;
                entity.Email = model.Email;
                entity.IdCondicionIva = model.IdCondicionIva;

                // ✅ FechaNacimiento safe
                if (model.FechaNacimiento.HasValue && model.FechaNacimiento.Value >= new DateTime(1753, 1, 1))
                    entity.FechaNacimiento = model.FechaNacimiento;
                else
                    entity.FechaNacimiento = null;

                entity.IdUsuarioModifica = model.IdUsuarioModifica;
                entity.FechaModifica = model.FechaModifica;

                await _dbcontext.SaveChangesAsync();

                // ====== Roles: reemplazo total (simple y sólido) ======
                rolesIds ??= new List<int>();
                rolesIds = rolesIds.Distinct().ToList();

                var rolesActuales = await _dbcontext.PersonalRolesAsignados
                    .Where(x => x.IdPersonal == model.Id)
                    .ToListAsync();

                _dbcontext.PersonalRolesAsignados.RemoveRange(rolesActuales);
                await _dbcontext.SaveChangesAsync();

                foreach (var idRol in rolesIds)
                {
                    _dbcontext.PersonalRolesAsignados.Add(new PersonalRolesAsignado
                    {
                        IdPersonal = model.Id,
                        IdRol = idRol
                    });
                }

                // ====== Artistas: reemplazo total ======
                artistasIds ??= new List<int>();
                artistasIds = artistasIds.Distinct().ToList();

                var artistasActuales = await _dbcontext.PersonalArtistasAsignados
                    .Where(x => x.IdPersonal == model.Id)
                    .ToListAsync();

                _dbcontext.PersonalArtistasAsignados.RemoveRange(artistasActuales);
                await _dbcontext.SaveChangesAsync();

                foreach (var idArtista in artistasIds)
                {
                    _dbcontext.PersonalArtistasAsignados.Add(new PersonalArtistasAsignado
                    {
                        IdPersonal = model.Id,
                        IdArtista = idArtista
                    });
                }

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

        public async Task<bool> Eliminar(int id)
        {
            using var trx = await _dbcontext.Database.BeginTransactionAsync();

            try
            {
                var personal = await _dbcontext.Personals.FindAsync(id);
                if (personal == null) return false;

                // borra joins primero (si no querés depender de cascade)
                var roles = await _dbcontext.PersonalRolesAsignados.Where(x => x.IdPersonal == id).ToListAsync();
                var artistas = await _dbcontext.PersonalArtistasAsignados.Where(x => x.IdPersonal == id).ToListAsync();

                _dbcontext.PersonalRolesAsignados.RemoveRange(roles);
                _dbcontext.PersonalArtistasAsignados.RemoveRange(artistas);
                await _dbcontext.SaveChangesAsync();

                _dbcontext.Personals.Remove(personal);
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

        public async Task<Personal?> Obtener(int id)
        {
            try
            {
                return await _dbcontext.Personals
                    .Include(x => x.IdPaisNavigation)
                    .Include(x => x.IdTipoDocumentoNavigation)
                    .Include(x => x.IdCondicionIvaNavigation)
                    .Include(x => x.IdUsuarioRegistraNavigation)
                    .Include(x => x.IdUsuarioModificaNavigation)
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public async Task<IQueryable<Personal>> ObtenerTodos()
        {
            var query = _dbcontext.Personals
                .Include(x => x.IdPaisNavigation)
                .Include(x => x.IdTipoDocumentoNavigation)
                .Include(x => x.IdCondicionIvaNavigation)
                .Include(x => x.IdUsuarioRegistraNavigation)
                .Include(x => x.IdUsuarioModificaNavigation)
                .AsQueryable();

            return await Task.FromResult(query);
        }

        public async Task<List<int>> ObtenerRolesIds(int idPersonal)
        {
            return await _dbcontext.PersonalRolesAsignados
                .Where(x => x.IdPersonal == idPersonal)
                .Select(x => x.IdRol)
                .ToListAsync();
        }

        public async Task<List<int>> ObtenerArtistasIds(int idPersonal)
        {
            return await _dbcontext.PersonalArtistasAsignados
                .Where(x => x.IdPersonal == idPersonal)
                .Select(x => x.IdArtista)
                .ToListAsync();
        }
    }
}
