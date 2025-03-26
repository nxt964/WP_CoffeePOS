using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeePOS.Core.Interfaces;

namespace CoffeePOS.Core.Repositories;
public class MockRepository<T> : IRepository<T> where T : class
{
    private List<T> _entities = new List<T>();

    public MockRepository(IEnumerable<T> entities)
    {
           _entities = entities.ToList();
    }

    public Task<IEnumerable<T>> GetAll()
    {
        return Task.FromResult(_entities.AsEnumerable());
    }

    public Task<T> Add(T entity)
    {

        var idProperty = entity.GetType().GetProperty("Id");
        if (idProperty != null && (int)idProperty.GetValue(entity) == 0)
        {
            int newId = _entities.Any() ? _entities.Max(e => (int)e.GetType().GetProperty("Id").GetValue(e)) + 1 : 1;
            idProperty.SetValue(entity, newId);
        }

        _entities.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<T> Delete(int id)
    {
        var entity = _entities.FirstOrDefault(e => (int)e.GetType().GetProperty("Id").GetValue(e) == id);
        if (entity != null)
        {
            _entities.Remove(entity);
            return Task.FromResult(entity);
        }
        return Task.FromResult<T>(null);
    }

    public Task<T> GetById(int id)
    {
        var entity = _entities.FirstOrDefault(e => (int)e.GetType().GetProperty("Id").GetValue(e) == id);
        return Task.FromResult(entity);
    }
    public Task<T> Update(T entity)
    {
        var entityToUpdate = _entities.FirstOrDefault(e => (int)e.GetType().GetProperty("Id").GetValue(e) == (int)entity.GetType().GetProperty("Id").GetValue(entity));
        if (entityToUpdate != null)
        {
            _entities.Remove(entityToUpdate);
            _entities.Add(entity);
            return Task.FromResult(entity);
        }
        return Task.FromResult<T>(null);
    }
}
