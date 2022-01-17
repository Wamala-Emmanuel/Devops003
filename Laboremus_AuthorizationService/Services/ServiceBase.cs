using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Laboremus_AuthorizationService.Repositories;

namespace Laboremus_AuthorizationService.Services
{
    /// <inheritdoc />
    public class ServiceBase<T, TV> : IServiceBase<T, TV> where T : class where TV:class
    {
        private readonly IGenericRepository<T> _repository;
        private readonly IMapper _mapper;

        public ServiceBase(IGenericRepository<T> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        
        public virtual async Task<TV> FirstAsync(Expression<Func<T, bool>> predicate)
        {
            var result = await _repository.FirstAsync(predicate);
            return _mapper.Map<TV>(result);
        }

        public virtual async Task<TV> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            var result = await _repository.FirstOrDefaultAsync(predicate);
            return _mapper.Map<TV>(result);
        }

        public virtual IQueryable<T> GetAll()
        {
            return _repository.GetAll();
        }

        public virtual IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            return _repository.FindBy(predicate);
        }

        public async Task<TV> FindAsync(params object[] keys)
        {
            var results = await _repository.FindAsync(keys);
            return _mapper.Map<TV>(results);
        }

        public virtual async Task AddAsync(TV model)
        {
            var entity = _mapper.Map<T>(model);
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(TV model)
        {
            var entity = _mapper.Map<T>(model);
            _repository.Delete(entity);
            await _repository.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(params object[] keys)
        {
            var entity = await _repository.FindAsync(keys);
            _repository.Delete(entity);
            await _repository.SaveChangesAsync();
        }
        
        public virtual async Task UpdateAsync(object id, TV model)
        {
            var entity = _mapper.Map<T>(model);
            await _repository.UpdateAsync(id, entity);
            await _repository.SaveChangesAsync();
        }

    }
}