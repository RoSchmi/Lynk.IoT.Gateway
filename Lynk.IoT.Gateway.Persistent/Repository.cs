using Lynk.IoT.Gateway.Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lynk.IoT.Gateway.Persistent
{
    public class Repository : IRepository
    {
        AppDbContext _dbContext;
        public Repository()
        {
            _dbContext = new AppDbContext();
            _dbContext.Database.Migrate();
            if (!_dbContext.Devices.Any())
            {
                _dbContext.Devices.Add(new Models.Device { Id = Guid.Parse("E5363BAF-2C1B-4C6C-A92B-41A8DAFFF870"), Name="Brain Pad", Key = "E5363BAF-2C1B-4C6C-A92B-41A8DAFFF870" });
                _dbContext.SaveChanges();
            }
        }
        public async Task AddEntityAsync<T>(T model) where T : class
        {
            await _dbContext.Set<T>().AddAsync(model);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteEntityAsync<T>(T model) where T : class
        {
            _dbContext.Entry(model).State = EntityState.Deleted;
           await  _dbContext.SaveChangesAsync();
        }

        public IQueryable<T> Get<T>() where T : class
        {
            return _dbContext.Set<T>().AsQueryable();
        }

        public Task<T> Get<T>(object id) where T : class
        {
            throw new NotImplementedException();
        }

        public async Task UpdateEntityAsync<T>(T model) where T : class
        {
            _dbContext.Entry(model).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
    }
}
