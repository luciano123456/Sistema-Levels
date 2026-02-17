



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
    public class MonedasCuentaRepository : IMonedasCuentaRepository<MonedasCuenta>
    {

        private readonly SistemaLevelsContext _dbcontext;

        public MonedasCuentaRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }
        public async Task<bool> Actualizar(MonedasCuenta model)
        {
            try
            {
                _dbcontext.MonedasCuentas.Update(model);
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
                MonedasCuenta model = _dbcontext.MonedasCuentas.First(c => c.Id == id);
                _dbcontext.MonedasCuentas.Remove(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Insertar(MonedasCuenta model)
        {
            try
            {
                _dbcontext.MonedasCuentas.Add(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<MonedasCuenta> Obtener(int id)
        {
            try
            {
                MonedasCuenta model = await _dbcontext.MonedasCuentas
                    .Include(x => x.IdMonedaNavigation)
                    .FirstOrDefaultAsync(x => x.Id == id);

                return model;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IQueryable<MonedasCuenta>> ObtenerMoneda(int idMoneda)
        {
            try
            {
                return _dbcontext.MonedasCuentas
                    .Include(x => x.IdMonedaNavigation)
                    .Where(x => x.IdMoneda == idMoneda);
            }
            catch (Exception)
            {
                return Enumerable.Empty<MonedasCuenta>().AsQueryable();
            }
        }


        public async Task<IQueryable<MonedasCuenta>> ObtenerTodos()
        {
            try
            {
                IQueryable<MonedasCuenta> query = _dbcontext.MonedasCuentas;

                return await Task.FromResult(query);

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        
    }
}
