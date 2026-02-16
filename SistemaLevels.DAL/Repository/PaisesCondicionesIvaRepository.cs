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
    public class PaisesCondicionesIvaRepository : IPaisesCondicionesIvaRepository<PaisesCondicionesIva>
    {

        private readonly SistemaLevelsContext _dbcontext;

        public PaisesCondicionesIvaRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }
        public async Task<bool> Actualizar(PaisesCondicionesIva model)
        {
            try
            {
                _dbcontext.PaisesCondicionesIvas.Update(model);
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
                PaisesCondicionesIva model = _dbcontext.PaisesCondicionesIvas.First(c => c.Id == id);
                _dbcontext.PaisesCondicionesIvas.Remove(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Insertar(PaisesCondicionesIva model)
        {
            try
            {
                _dbcontext.PaisesCondicionesIvas.Add(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<PaisesCondicionesIva> Obtener(int id)
        {
            try
            {
                PaisesCondicionesIva model = await _dbcontext.PaisesCondicionesIvas
                    .Include(x => x.IdPaisNavigation)
                    .FirstOrDefaultAsync(x => x.Id == id);

                return model;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public async Task<IQueryable<PaisesCondicionesIva>> ObtenerTodos()
        {
            try
            {
                IQueryable<PaisesCondicionesIva> query = _dbcontext.PaisesCondicionesIvas;

                return await Task.FromResult(query);

            }
            catch (Exception ex)
            {
                return null;
            }
        }




    }
}
