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
    public class PaisRepository : IPaisRepository<Pais>
    {

        private readonly SistemaLevelsContext _dbcontext;

        public PaisRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }
        public async Task<bool> Actualizar(Pais model)
        {
            try
            {
                _dbcontext.Paises.Update(model);
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
                Pais model = _dbcontext.Paises.First(c => c.Id == id);
                _dbcontext.Paises.Remove(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Insertar(Pais model)
        {
            try
            {
                _dbcontext.Paises.Add(model);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<Pais> Obtener(int id)
        {
            try
            {
                Pais model = await _dbcontext.Paises.FindAsync(id);
                return model;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IQueryable<Pais>> ObtenerTodos()
        {
            try
            {
                IQueryable<Pais> query = _dbcontext.Paises;

                return await Task.FromResult(query);

            }
            catch (Exception ex)
            {
                return null;
            }
        }




    }
}
