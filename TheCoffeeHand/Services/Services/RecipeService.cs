using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Interfracture.Interfaces;
using Interfracture.PaggingItems;
using Microsoft.EntityFrameworkCore;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace Services.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRedisCacheServices _cacheService;

        public RecipeService(IUnitOfWork unitOfWork, IMapper mapper, IRedisCacheServices cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<RecipeResponseDTO> CreateRecipeAsync(RecipeRequestDTO recipeDTO)
        {
            var recipe = _mapper.Map<Recipe>(recipeDTO);
            await _unitOfWork.GetRepository<Recipe>().InsertAsync(recipe);
            await _unitOfWork.SaveAsync();

            // Clear cache when data changes
            await _cacheService.RemoveByPrefixAsync("recipes_");
            await _cacheService.ClearAllCacheAsync();
            return _mapper.Map<RecipeResponseDTO>(recipe);
        }

        public async Task<RecipeResponseDTO> GetRecipeByIdAsync(Guid id)
        {
            string cacheKey = $"recipe_{id}";

            // Try to get from cache
            var cachedRecipe = await _cacheService.GetAsync<RecipeResponseDTO>(cacheKey);
            if (cachedRecipe != null)
            {
                return cachedRecipe;
            }

            var recipe = await _unitOfWork.GetRepository<Recipe>().GetByIdAsync(id);
            var recipeDTO = _mapper.Map<RecipeResponseDTO>(recipe);

            // Store in cache
            await _cacheService.SetAsync(cacheKey, recipeDTO, TimeSpan.FromMinutes(30));

            return recipeDTO;
        }

        public async Task<List<RecipeResponseDTO>> GetRecipesAsync()
        {
            string cacheKey = "recipes_all";

            // Try to get from cache
            var cachedRecipes = await _cacheService.GetAsync<List<RecipeResponseDTO>>(cacheKey);
            if (cachedRecipes != null)
            {
                return cachedRecipes;
            }

            var recipes = await _unitOfWork.GetRepository<Recipe>().Entities.ToListAsync();
            var recipeDTOs = _mapper.Map<List<RecipeResponseDTO>>(recipes);

            // Store in cache
            await _cacheService.SetAsync(cacheKey, recipeDTOs, TimeSpan.FromMinutes(30));

            return recipeDTOs;
        }

        public async Task<PaginatedList<RecipeResponseDTO>> GetRecipesAsync(int pageNumber, int pageSize)
        {
            string cacheKey = $"recipes_{pageNumber}_{pageSize}";

            // Try to get from cache
            var cachedRecipes = await _cacheService.GetAsync<PaginatedList<RecipeResponseDTO>>(cacheKey);
            if (cachedRecipes != null)
            {
                return cachedRecipes;
            }

            var recipeRepo = _unitOfWork.GetRepository<Recipe>();
            var query = recipeRepo.Entities;

            var paginatedRecipes = await PaginatedList<RecipeResponseDTO>.CreateAsync(
                query.OrderBy(r => r.Id).ProjectTo<RecipeResponseDTO>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize
            );

            // Store in cache for 30 minutes
            await _cacheService.SetAsync(cacheKey, paginatedRecipes, TimeSpan.FromMinutes(30));

            return paginatedRecipes;
        }

        public async Task<RecipeResponseDTO> UpdateRecipeAsync(Guid id, RecipeRequestDTO recipeDTO)
        {
            var recipe = _mapper.Map<Recipe>(recipeDTO);
            recipe.Id = id;

            await _unitOfWork.GetRepository<Recipe>().UpdateAsync(recipe);
            await _unitOfWork.SaveAsync();

            // Clear related caches
            await _cacheService.RemoveAsync($"recipe_{id}");
            await _cacheService.RemoveByPrefixAsync("recipes_");
            await _cacheService.ClearAllCacheAsync();

            return _mapper.Map<RecipeResponseDTO>(recipe);
        }

        public async Task DeleteRecipeAsync(Guid id)
        {
            var recipe = await _unitOfWork.GetRepository<Recipe>().GetByIdAsync(id);
            if (recipe == null)
                throw new Exception("Recipe not found");

            await _unitOfWork.GetRepository<Recipe>().DeleteAsync(recipe);
            await _unitOfWork.SaveAsync();

            // Clear related caches
            await _cacheService.RemoveAsync($"recipe_{id}");
            await _cacheService.RemoveByPrefixAsync("recipes_");
            await _cacheService.ClearAllCacheAsync();
        }
    }
}
