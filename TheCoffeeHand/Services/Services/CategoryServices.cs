using AutoMapper;
using Interfracture.Interfaces;
using Microsoft.EntityFrameworkCore;
using Core.Utils;
using AutoMapper.QueryableExtensions;
using Services.DTOs;
using Services.ServiceInterfaces;
using Interfracture.PaggingItems;
using static Domain.Base.BaseException;
using Domain.Base;
using Domain.Entities;

namespace Services.Services
{
    public class CategoryServices : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRedisCacheServices _cacheService;

        public CategoryServices(IUnitOfWork unitOfWork, IMapper mapper, IRedisCacheServices cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<CategoryResponseDTO> CreateCategoryAsync(CategoryRequestDTO categoryDTO)
        {
            var existingCategory = await _unitOfWork.GetRepository<Category>().Entities.FirstOrDefaultAsync(c => c.Name != null && c.Name.ToLower() == categoryDTO.Name.ToLower());
            if (existingCategory != null)
            {
                throw new BadRequestException("bad_request", "Category with the same name already exists.");
            }
            var categoryRepo = _unitOfWork.GetRepository<Category>();

            var category = _mapper.Map<Category>(categoryDTO);
            category.CreatedTime = category.LastUpdatedTime = CoreHelper.SystemTimeNow;

            await categoryRepo.InsertAsync(category);
            await _unitOfWork.SaveAsync();
            await _cacheService.RemoveByPrefixAsync("categories_");

            return _mapper.Map<CategoryResponseDTO>(category);
        }

        public async Task<CategoryResponseDTO> GetCategoryByIdAsync(Guid id)
        {
            string cacheKey = $"category_{id}";

            // Try to get from cache first
            var category = await _cacheService.GetAsync<Category>(cacheKey);
            if (category != null)
                return _mapper.Map<CategoryResponseDTO>(category);

            var categoryRepo = _unitOfWork.GetRepository<Category>();
            category = await categoryRepo.Entities.FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                throw new BaseException.NotFoundException("category_not_found", $"Category with ID {id} not found.");

            await _cacheService.SetAsync(cacheKey, category, TimeSpan.FromMinutes(30));

            return _mapper.Map<CategoryResponseDTO>(category);
        }


        public async Task<PaginatedList<CategoryResponseDTO>> GetAllCategoriesAsync(int pageNumber, int pageSize)
        {
            string cacheKey = $"categories_{pageNumber}_{pageSize}";

            // Try to get data from cache first
            var cachedCategories = await _cacheService.GetAsync<PaginatedList<CategoryResponseDTO>>(cacheKey);
            if (cachedCategories != null)
            {
                return cachedCategories;
            }

            var categoryRepo = _unitOfWork.GetRepository<Category>();
            var query = categoryRepo.Entities.Where(c => c.DeletedTime == null);

            // Fetch and paginate data
            var paginatedCategories = await PaginatedList<CategoryResponseDTO>.CreateAsync(
                query.OrderBy(c => c.Name).ProjectTo<CategoryResponseDTO>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize
            );

            // Store in cache for 30 minutes
            await _cacheService.SetAsync(cacheKey, paginatedCategories, TimeSpan.FromMinutes(30));

            return paginatedCategories;
        }



        public async Task<CategoryResponseDTO> UpdateCategoryAsync(Guid id, CategoryRequestDTO categoryDTO)
        {
            var existingCategory = await _unitOfWork.GetRepository<Category>().Entities.FirstOrDefaultAsync(c => c.Id != id && c.Name != null && c.Name.ToLower() == categoryDTO.Name.ToLower());
            if (existingCategory != null)
            {
                throw new BadRequestException("bad_request", "Category with the same name already exists.");
            }
            string cacheKey = $"category_{id}";
            var categoryRepo = _unitOfWork.GetRepository<Category>();

            // Check cache first
            var category = await _cacheService.GetAsync<Category>(cacheKey);
            if (category == null)
            {
                category = await categoryRepo.Entities.FirstOrDefaultAsync(c => c.Id == id);
                if (category == null)
                    throw new BaseException.NotFoundException("category_not_found", "Category not found.");
            }

            if (category.DeletedTime != null)
                throw new BaseException.BadRequestException("category_deleted", "Cannot update a deleted category.");

            // Map only provided properties while keeping existing values
            _mapper.Map(categoryDTO, category);
            category.LastUpdatedTime = CoreHelper.SystemTimeNow;

            categoryRepo.Update(category);
            await _unitOfWork.SaveAsync();

            var updatedCategoryDTO = _mapper.Map<CategoryResponseDTO>(category);

            // Update cache with the new category data
            await _cacheService.SetAsync(cacheKey, category, TimeSpan.FromMinutes(30));

            await _cacheService.RemoveByPrefixAsync("categories_");

            return updatedCategoryDTO;
        }



        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            string cacheKey = $"category_{id}";
            var categoryRepo = _unitOfWork.GetRepository<Category>();

            // Check cache first
            var category = await _cacheService.GetAsync<Category>(cacheKey);
            if (category == null)
            {
                category = await categoryRepo.Entities.FirstOrDefaultAsync(c => c.Id == id);
                if (category == null)
                    throw new BaseException.NotFoundException("category_not_found", "Category not found.");
            }

            if (category.DeletedTime != null)
                throw new BaseException.BadRequestException("category_already_deleted", "Category has already been deleted.");

            category.DeletedTime = CoreHelper.SystemTimeNow;

            categoryRepo.Update(category);
            await _unitOfWork.SaveAsync();

            // Remove from cache since it's deleted
            await _cacheService.RemoveAsync(cacheKey);
            await _cacheService.RemoveByPrefixAsync("categories_");

            return true;
        }
    }
}
