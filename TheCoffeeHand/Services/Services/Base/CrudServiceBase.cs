using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Base;
using Interfracture.Interfaces;
using Microsoft.EntityFrameworkCore;
using Services.ServiceInterfaces;

namespace Services.Services.Base
{
    public abstract class CrudServiceBase<T, TRequestDTO, TResponseDTO> : ICrudServiceBase<TRequestDTO, TResponseDTO> where T : class
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IMapper _mapper;
        protected readonly IRedisCacheServices _cacheService;
        private readonly string _cachePrefix;

        protected CrudServiceBase(IUnitOfWork unitOfWork, IMapper mapper, IRedisCacheServices cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
            _cachePrefix = typeof(T).Name.ToLower() + "_";
        }

        public async Task<TResponseDTO> CreateAsync(TRequestDTO dto)
        {
            var entity = _mapper.Map<T>(dto);
            await _unitOfWork.GetRepository<T>().InsertAsync(entity);
            await _unitOfWork.SaveAsync();

            await _cacheService.RemoveByPrefixAsync(_cachePrefix);

            return _mapper.Map<TResponseDTO>(entity);
        }

        public async Task<TResponseDTO> GetByIdAsync(Guid id)
        {
            string cacheKey = _cachePrefix + id;
            var cachedData = await _cacheService.GetAsync<TResponseDTO>(cacheKey);
            if (cachedData != null)
                return cachedData;

            var dto = await _unitOfWork.GetRepository<T>().Entities
                .Where(e => EF.Property<Guid>(e, "Id") == id)
                .ProjectTo<TResponseDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync() ?? throw new BaseException.NotFoundException("not_found","Not found");


            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));
            return dto;
        }

        public async Task<TResponseDTO> UpdateAsync(Guid id, TRequestDTO dto)
        {
            var entity = await _unitOfWork.GetRepository<T>().GetByIdAsync(id);
            if (entity == null)
                throw new Exception("Entity not found");

            _mapper.Map(dto, entity);
            await _unitOfWork.GetRepository<T>().UpdateAsync(entity);
            await _unitOfWork.SaveAsync();

            await _cacheService.RemoveAsync(_cachePrefix + id);
            await _cacheService.RemoveByPrefixAsync(_cachePrefix);

            return _mapper.Map<TResponseDTO>(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _unitOfWork.GetRepository<T>().GetByIdAsync(id);
            if (entity == null)
                throw new Exception("Entity not found");

            await _unitOfWork.GetRepository<T>().DeleteAsync(entity);
            await _unitOfWork.SaveAsync();

            await _cacheService.RemoveAsync(_cachePrefix + id);
            await _cacheService.RemoveByPrefixAsync(_cachePrefix);
        }
    }
}
