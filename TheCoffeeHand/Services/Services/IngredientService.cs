using AutoMapper;
using AutoMapper.QueryableExtensions;
using Core.Utils;
using Domain.Entities;
using Interfracture.Interfaces;
using Interfracture.PaggingItems;
using Microsoft.EntityFrameworkCore;
using Services.DTOs;
using Services.ServiceInterfaces;
using static Domain.Base.BaseException;

namespace Services.Services
{
    public class IngredientService : IIngredientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRedisCacheServices _cacheService;

        public IngredientService(IUnitOfWork unitOfWork, IMapper mapper, IRedisCacheServices cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<IngredientResponseDTO> CreateIngredientAsync(IngredientRequestDTO ingredientDTO)
        {
            var existingIngredient = await _unitOfWork.GetRepository<Ingredient>().Entities
                .FirstOrDefaultAsync(i => i.Name.ToLower() == ingredientDTO.Name.ToLower());
            if (existingIngredient != null)
            {
                throw new BadRequestException("bad_request", "Ingredient with the same name already exists.");
            }
            var ingredient = _mapper.Map<Ingredient>(ingredientDTO);
            await _unitOfWork.GetRepository<Ingredient>().InsertAsync(ingredient);
            await _unitOfWork.SaveAsync();

            // Clear cache when data changes
            await _cacheService.RemoveByPrefixAsync("ingredients_");

            return _mapper.Map<IngredientResponseDTO>(ingredient);
        }

        public async Task<IngredientResponseDTO> GetIngredientByIdAsync(Guid id)
        {
            string cacheKey = $"ingredient_{id}";

            // Try to get from cache
            var cachedIngredient = await _cacheService.GetAsync<IngredientResponseDTO>(cacheKey);
            if (cachedIngredient != null)
            {
                return cachedIngredient;
            }

            var ingredient = await _unitOfWork.GetRepository<Ingredient>().GetByIdAsync(id);
            var ingredientDTO = _mapper.Map<IngredientResponseDTO>(ingredient);

            // Store in cache
            await _cacheService.SetAsync(cacheKey, ingredientDTO, TimeSpan.FromMinutes(30));

            return ingredientDTO;
        }

        public async Task<List<IngredientResponseDTO>> GetIngredientsAsync()
        {
            string cacheKey = "ingredients_all";

            // Try to get from cache
            var cachedIngredients = await _cacheService.GetAsync<List<IngredientResponseDTO>>(cacheKey);
            if (cachedIngredients != null)
            {
                return cachedIngredients;
            }

            var ingredients = await _unitOfWork.GetRepository<Ingredient>().Entities.ToListAsync();
            var ingredientDTOs = _mapper.Map<List<IngredientResponseDTO>>(ingredients);

            // Store in cache
            await _cacheService.SetAsync(cacheKey, ingredientDTOs, TimeSpan.FromMinutes(30));

            return ingredientDTOs;
        }

        public async Task<PaginatedList<IngredientResponseDTO>> GetIngredientsAsync(int pageNumber, int pageSize)
        {
            string cacheKey = $"ingredients_{pageNumber}_{pageSize}";

            // Try to get from cache
            var cachedIngredients = await _cacheService.GetAsync<PaginatedList<IngredientResponseDTO>>(cacheKey);
            if (cachedIngredients != null)
            {
                return cachedIngredients;
            }

            var ingredientRepo = _unitOfWork.GetRepository<Ingredient>();
            var query = ingredientRepo.Entities.Where(i => i.DeletedTime == null);

            var paginatedIngredients = await PaginatedList<IngredientResponseDTO>.CreateAsync(
                query.OrderBy(i => i.Name.ToLower()).ProjectTo<IngredientResponseDTO>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize
            );

            // Store in cache for 30 minutes
            await _cacheService.SetAsync(cacheKey, paginatedIngredients, TimeSpan.FromMinutes(30));

            return paginatedIngredients;
        }

        public async Task<IngredientResponseDTO> UpdateIngredientAsync(Guid id, IngredientResponseDTO ingredientDTO)
        {
            var existingIngredient = await _unitOfWork.GetRepository<Ingredient>().Entities
                .FirstOrDefaultAsync(i => i.Id != id && i.Name.ToLower() == ingredientDTO.Name.ToLower());
            if (existingIngredient != null)
            {
                throw new BadRequestException("bad_request", "Ingredient with the same name already exists.");
            }

            var ingredient = _mapper.Map<Ingredient>(ingredientDTO);
            ingredient.Id = id;

            await _unitOfWork.GetRepository<Ingredient>().UpdateAsync(ingredient);
            await _unitOfWork.SaveAsync();

            // Clear related caches
            await _cacheService.RemoveAsync($"ingredient_{id}");
            await _cacheService.RemoveByPrefixAsync("ingredients_");

            return _mapper.Map<IngredientResponseDTO>(ingredient);
        }

        public async Task DeleteIngredientAsync(Guid id)
        {
            var ingredient = await _unitOfWork.GetRepository<Ingredient>().GetByIdAsync(id);
            if (ingredient == null || ingredient.DeletedTime != null)
                throw new NotFoundException("not_found", "Ingredient not found");

            ingredient.DeletedTime = CoreHelper.SystemTimeNow;

            await _unitOfWork.GetRepository<Ingredient>().UpdateAsync(ingredient);
            await _unitOfWork.SaveAsync();

            // Clear related caches
            await _cacheService.RemoveAsync($"ingredient_{id}");
            await _cacheService.RemoveByPrefixAsync("ingredients_");
        }

        public async Task<IngredientResponseDTO> GetIngredientByNameAsync(string name) {
            string cacheKey = $"ingredient_name_{name.ToLower()}";

            // Try to get from cache
            var cachedIngredient = await _cacheService.GetAsync<IngredientResponseDTO>(cacheKey);
            if (cachedIngredient != null) {
                return cachedIngredient;
            }

            var ingredient = await _unitOfWork.GetRepository<Ingredient>().Entities
                .FirstOrDefaultAsync(i => i.Name.ToLower() == name.ToLower() && i.DeletedTime == null);

            if (ingredient == null) {
                throw new NotFoundException("not_found", "Ingredient not found");
            }

            var ingredientDTO = _mapper.Map<IngredientResponseDTO>(ingredient);

            // Store in cache
            await _cacheService.SetAsync(cacheKey, ingredientDTO, TimeSpan.FromMinutes(30));

            return ingredientDTO;
        }
    }
}
