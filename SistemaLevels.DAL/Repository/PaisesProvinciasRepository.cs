



using Microsoft.EntityFrameworkCore;
using SistemaLevels.DAL.DataContext;
using SistemaLevels.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SistemaLevels.DAL.Repository
{
    public class PaisesProvinciaRepository : IPaisesProvinciaRepository<PaisesProvincia>
    {

        private readonly SistemaLevelsContext _dbcontext;

        public PaisesProvinciaRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }
        public async Task<bool> Actualizar(PaisesProvincia model)
        {
            try
            {
                _dbcontext.PaisesProvincias.Update(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                PaisesProvincia model = _dbcontext.PaisesProvincias.First(c => c.Id == id);
                _dbcontext.PaisesProvincias.Remove(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Insertar(PaisesProvincia model)
        {
            try
            {
                _dbcontext.PaisesProvincias.Add(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<PaisesProvincia> Obtener(int id)
        {
            try
            {
                PaisesProvincia model = await _dbcontext.PaisesProvincias
                    .Include(x => x.IdPaisNavigation)
                    .FirstOrDefaultAsync(x => x.Id == id);

                return model;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IQueryable<PaisesProvincia>> ObtenerPais(int idPais)
        {
            try
            {
                return _dbcontext.PaisesProvincias
                    .Include(x => x.IdPaisNavigation)
                    .Where(x => x.IdPais == idPais);
            }
            catch (Exception)
            {
                return Enumerable.Empty<PaisesProvincia>().AsQueryable();
            }
        }


        public async Task<IQueryable<PaisesProvincia>> ObtenerTodos()
        {
            try
            {
                IQueryable<PaisesProvincia> query = _dbcontext.PaisesProvincias;

                return await Task.FromResult(query);

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        
    }
}
