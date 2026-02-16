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
    public class TareasEstadosRepository : ITareasEstadosRepository<TareasEstado>
    {

        private readonly SistemaLevelsContext _dbcontext;

        public TareasEstadosRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }
        public async Task<bool> Actualizar(TareasEstado model)
        {
            _dbcontext.TareasEstados.Update(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Eliminar(int id)
        {
            TareasEstado model = _dbcontext.TareasEstados.First(c => c.Id == id);
            _dbcontext.TareasEstados.Remove(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Insertar(TareasEstado model)
        {
            _dbcontext.TareasEstados.Add(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<TareasEstado> Obtener(int id)
        {
            TareasEstado model = await _dbcontext.TareasEstados.FindAsync(id);
            return model;
        }
        public async Task<IQueryable<TareasEstado>> ObtenerTodos()
        {
            IQueryable<TareasEstado> query = _dbcontext.TareasEstados;
            return await Task.FromResult(query);
        }




    }
}
