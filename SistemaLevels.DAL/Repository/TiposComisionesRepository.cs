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
    public class TiposComisionesRepository : ITiposComisionesRepository<TiposComision>
    {

        private readonly SistemaLevelsContext _dbcontext;

        public TiposComisionesRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }
        public async Task<bool> Actualizar(TiposComision model)
        {
            _dbcontext.TiposComisiones.Update(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Eliminar(int id)
        {
            TiposComision model = _dbcontext.TiposComisiones.First(c => c.Id == id);
            _dbcontext.TiposComisiones.Remove(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Insertar(TiposComision model)
        {
            _dbcontext.TiposComisiones.Add(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<TiposComision> Obtener(int id)
        {
            TiposComision model = await _dbcontext.TiposComisiones.FindAsync(id);
            return model;
        }
        public async Task<IQueryable<TiposComision>> ObtenerTodos()
        {
            IQueryable<TiposComision> query = _dbcontext.TiposComisiones;
            return await Task.FromResult(query);
        }




    }
}
