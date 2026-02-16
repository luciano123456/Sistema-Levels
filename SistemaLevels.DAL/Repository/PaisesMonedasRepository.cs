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
    public class PaisesMonedaRepository : IPaisesMonedaRepository<PaisesMoneda>
    {

        private readonly SistemaLevelsContext _dbcontext;

        public PaisesMonedaRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }
        public async Task<bool> Actualizar(PaisesMoneda model)
        {
            try
            {
                _dbcontext.PaisesMonedas.Update(model);
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
                PaisesMoneda model = _dbcontext.PaisesMonedas.First(c => c.Id == id);
                _dbcontext.PaisesMonedas.Remove(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Insertar(PaisesMoneda model)
        {
            try
            {
                _dbcontext.PaisesMonedas.Add(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

     

        public async Task<PaisesMoneda> Obtener(int id)
        {
            try
            {
                PaisesMoneda model = await _dbcontext.PaisesMonedas
                    .Include(x => x.IdPaisNavigation)
                    .FirstOrDefaultAsync(x => x.Id == id);

                return model;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public async Task<IQueryable<PaisesMoneda>> ObtenerTodos()
        {
            try
            {
                IQueryable<PaisesMoneda> query = _dbcontext.PaisesMonedas;

                return await Task.FromResult(query);

            }
            catch (Exception ex)
            {
                return null;
            }
        }




    }
}
