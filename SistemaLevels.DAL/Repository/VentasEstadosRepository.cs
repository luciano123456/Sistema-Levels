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
    public class VentasEstadosRepository : IVentasEstadosRepository<VentasEstado>
    {

        private readonly SistemaLevelsContext _dbcontext;

        public VentasEstadosRepository(SistemaLevelsContext context)
        {
            _dbcontext = context;
        }
        public async Task<bool> Actualizar(VentasEstado model)
        {
            _dbcontext.VentasEstados.Update(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Eliminar(int id)
        {
            VentasEstado model = _dbcontext.VentasEstados.First(c => c.Id == id);
            _dbcontext.VentasEstados.Remove(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Insertar(VentasEstado model)
        {
            _dbcontext.VentasEstados.Add(model);
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task<VentasEstado> Obtener(int id)
        {
            VentasEstado model = await _dbcontext.VentasEstados.FindAsync(id);
            return model;
        }
        public async Task<IQueryable<VentasEstado>> ObtenerTodos()
        {
            IQueryable<VentasEstado> query = _dbcontext.VentasEstados;
            return await Task.FromResult(query);
        }




    }
}
