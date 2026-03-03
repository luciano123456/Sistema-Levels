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
    public class OpcionesBinariasRepository : IOpcionesBinariasRepository<OpcionesBinaria>
    {

        private readonly SistemaLevelsContext _dbcontext;

        public OpcionesBinariasRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }
        public async Task<bool> Actualizar(OpcionesBinaria model)
        {
            _dbcontext.OpcionesBinarias.Update(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Eliminar(int id)
        {
            OpcionesBinaria model = _dbcontext.OpcionesBinarias.First(c => c.Id == id);
            _dbcontext.OpcionesBinarias.Remove(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Insertar(OpcionesBinaria model)
        {
            _dbcontext.OpcionesBinarias.Add(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<OpcionesBinaria> Obtener(int id)
        {
            OpcionesBinaria model = await _dbcontext.OpcionesBinarias.FindAsync(id);
            return model;
        }
        public async Task<IQueryable<OpcionesBinaria>> ObtenerTodos()
        {
            IQueryable<OpcionesBinaria> query = _dbcontext.OpcionesBinarias;
            return await Task.FromResult(query);
        }




    }
}
