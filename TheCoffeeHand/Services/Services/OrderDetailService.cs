using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Base;
using Domain.Entities;
using Interfracture.Interfaces;
using Interfracture.PaggingItems;
using Microsoft.EntityFrameworkCore;
using Services.DTOs;
using Services.ServiceInterfaces;

namespace Services.Services
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRedisCacheServices _cacheService;

        public OrderDetailService(IUnitOfWork unitOfWork, IMapper mapper, IRedisCacheServices cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        //public async Task<OrderDetailResponselDTO> CreateOrderDetailAsync(OrderDetailRequestDTO dto)
        //{
        //    var orderDetail = _mapper.Map<OrderDetail>(dto);

        //    var cart = await _unitOfWork.GetRepository<Order>()
        //        .Entities
        //        .Include(o => o.OrderDetails!)
        //            .ThenInclude(od => od.Drink)
        //        .FirstOrDefaultAsync(c => c.Id == orderDetail.OrderId && c.Status == 0);

        //    if (cart == null)
        //        throw new BaseException.NotFoundException("not_found" ,"Cart not found");

        //    // Validate ingredient stock
        //    await ValidateIngredientStockAsync(orderDetail.DrinkId ?? throw new Exception("DrinkId cannot be null"), orderDetail.Total);


        //    using (var transaction = await _unitOfWork.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(orderDetail);
        //            await _unitOfWork.SaveAsync();

        //            cart.TotalPrice += orderDetail.Total * (orderDetail.Drink?.Price ?? 0);
        //            _unitOfWork.GetRepository<Order>().Update(cart);
        //            await _unitOfWork.SaveAsync();

        //            await transaction.CommitAsync();
        //        }
        //        catch
        //        {
        //            await transaction.RollbackAsync();
        //            throw;
        //        }
        //    }

        //    await _cacheService.RemoveByPrefixAsync("order_details_");
        //    await _cacheService.RemoveByPrefixAsync("orders_");

        //    return _mapper.Map<OrderDetailResponselDTO>(orderDetail);
        //}
        public async Task<OrderDetailResponselDTO> CreateOrderDetailAsync(OrderDetailRequestDTO dto) {
            // Tìm cart kèm OrderDetails
            var cart = await _unitOfWork.GetRepository<Order>()
                .Entities
                .Include(o => o.OrderDetails!)
                    .ThenInclude(od => od.Drink)
                .FirstOrDefaultAsync(c => c.Id == dto.OrderId && c.Status == 0);

            if (cart == null)
                throw new BaseException.NotFoundException("not_found", "Cart not found");

            // Kiểm tra xem drink đã tồn tại chưa
            var existingOrderDetail = cart.OrderDetails.FirstOrDefault(od => od.DrinkId == dto.DrinkId);

            using (var transaction = await _unitOfWork.BeginTransactionAsync()) {
                try {
                    if (existingOrderDetail != null) {
                        // Validate stock cho phần tăng thêm
                        await ValidateIngredientStockAsync(dto.DrinkId, dto.Total);

                        // Tăng total
                        existingOrderDetail.Total += dto.Total;

                        _unitOfWork.GetRepository<OrderDetail>().Update(existingOrderDetail);
                        await _unitOfWork.SaveAsync();
                    } else {
                        // Validate stock toàn bộ số lượng
                        await ValidateIngredientStockAsync(dto.DrinkId, dto.Total);

                        var newOrderDetail = _mapper.Map<OrderDetail>(dto);

                        await _unitOfWork.GetRepository<OrderDetail>().InsertAsync(newOrderDetail);
                        await _unitOfWork.SaveAsync();
                    }

                    // Cập nhật lại tổng tiền
                    cart.TotalPrice = cart.OrderDetails
                        .Where(od => od.Drink != null)
                        .Sum(od => od.Drink.Price * od.Total);

                    _unitOfWork.GetRepository<Order>().Update(cart);
                    await _unitOfWork.SaveAsync();

                    await transaction.CommitAsync();
                } catch {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            await _cacheService.RemoveByPrefixAsync("order_details_");
            await _cacheService.RemoveByPrefixAsync("orders_");

            // Trả về bản ghi mới hoặc cũ
            var orderDetailToReturn = existingOrderDetail ?? await _unitOfWork.GetRepository<OrderDetail>()
                .Entities
                .Include(od => od.Drink)
                .FirstOrDefaultAsync(od => od.OrderId == dto.OrderId && od.DrinkId == dto.DrinkId);

            return _mapper.Map<OrderDetailResponselDTO>(orderDetailToReturn);
        }


        private async Task ValidateIngredientStockAsync(Guid drinkId, int quantity)
        {
            var recipes = await _unitOfWork.GetRepository<Recipe>()
                .Entities
                .Where(r => r.DrinkId == drinkId)
                .Include(r => r.Ingredient)
                .ToListAsync();

            if (!recipes.Any())
                throw new BaseException.NotFoundException("not_found", "No recipes found for this drink");

            var requiredIngredients = recipes
                .Where(r => r.Ingredient != null)
                .ToLookup(r => r.Ingredient!.Id, r => r.Quantity * quantity)
                .ToDictionary(g => g.Key, g => g.Sum());

            var ingredients = await _unitOfWork.GetRepository<Ingredient>()
                .Entities
                .Where(i => requiredIngredients.Keys.Contains(i.Id))
                .ToListAsync();

            if (ingredients.Count != requiredIngredients.Count)
                throw new Exception("Some required ingredients are missing in the database");

            foreach (var ingredient in ingredients)
            {
                if (ingredient.Quantity < requiredIngredients[ingredient.Id])
                    throw new Exception($"Not enough stock for ingredient: {ingredient.Name}");
            }
        }


        public async Task<bool> RemoveFromCartAsync(Guid orderDetailId)
        {
            // Fetch the order detail with related drink and order
            var orderDetailRepo = _unitOfWork.GetRepository<OrderDetail>();
            var orderRepo = _unitOfWork.GetRepository<Order>();

            var orderDetail = await orderDetailRepo
                .Entities
                .Include(od => od.Drink)
                .FirstOrDefaultAsync(od => od.Id == orderDetailId);

            if (orderDetail == null)
                throw new Exception("Order detail not found");

            // Fetch the cart (order) associated with this order detail
            var cart = await orderRepo
                .Entities
                .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Drink)
                .FirstOrDefaultAsync(o => o.Id == orderDetail.OrderId && o.Status == 0);

            if (cart == null)
                throw new Exception("Cart not found");

            // Start a database transaction
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    // Remove order detail from cart
                    orderDetailRepo.Delete(orderDetail);
                    await _unitOfWork.SaveAsync();

                    // Recalculate cart total price
                    cart.TotalPrice = cart.OrderDetails!
                        .Where(od => od.Drink != null)
                        .Sum(od => od.Drink!.Price * od.Total);

                    // Save updated cart price
                    orderRepo.Update(cart);
                    await _unitOfWork.SaveAsync();

                    // Commit transaction
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            // Clear cache when data changes
            await _cacheService.RemoveByPrefixAsync("order_details_");
            await _cacheService.RemoveByPrefixAsync("orders_");

            return true;
        }



        public async Task<OrderDetailResponselDTO> GetOrderDetailByIdAsync(Guid id)
        {
            string cacheKey = $"order_detail_{id}";

            // Try to get from cache
            var cachedOrderDetail = await _cacheService.GetAsync<OrderDetailResponselDTO>(cacheKey);
            if (cachedOrderDetail != null)
            {
                return cachedOrderDetail;
            }

            var orderDetail = await _unitOfWork.GetRepository<OrderDetail>().GetByIdAsync(id);
            var dto = _mapper.Map<OrderDetailResponselDTO>(orderDetail);

            // Store in cache
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));

            return dto;
        }

        public async Task<List<OrderDetailResponselDTO>> GetOrderDetailsAsync()
        {
            string cacheKey = "order_details_all";

            // Try to get from cache
            var cachedOrderDetails = await _cacheService.GetAsync<List<OrderDetailResponselDTO>>(cacheKey);
            if (cachedOrderDetails != null)
            {
                return cachedOrderDetails;
            }

            var orderDetails = await _unitOfWork.GetRepository<OrderDetail>().Entities.ToListAsync();
            var dtoList = _mapper.Map<List<OrderDetailResponselDTO>>(orderDetails);

            // Store in cache
            await _cacheService.SetAsync(cacheKey, dtoList, TimeSpan.FromMinutes(30));

            return dtoList;
        }

        public async Task<PaginatedList<OrderDetailResponselDTO>> GetOrderDetailsAsync(int pageNumber, int pageSize)
        {
            string cacheKey = $"order_details_{pageNumber}_{pageSize}";

            // Try to get from cache
            var cachedOrderDetails = await _cacheService.GetAsync<PaginatedList<OrderDetailResponselDTO>>(cacheKey);
            if (cachedOrderDetails != null)
            {
                return cachedOrderDetails;
            }

            var orderDetailRepo = _unitOfWork.GetRepository<OrderDetail>();
            var query = orderDetailRepo.Entities;

            var paginatedOrderDetails = await PaginatedList<OrderDetailResponselDTO>.CreateAsync(
                query.OrderBy(o => o.Id).ProjectTo<OrderDetailResponselDTO>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize
            );

            // Store in cache for 30 minutes
            await _cacheService.SetAsync(cacheKey, paginatedOrderDetails, TimeSpan.FromMinutes(30));

            return paginatedOrderDetails;
        }

        public async Task<OrderDetailResponselDTO> UpdateOrderDetailAsync(Guid id, OrderDetailRequestDTO dto)
        {
            var orderDetail = _mapper.Map<OrderDetail>(dto);
            orderDetail.Id = id;

            await _unitOfWork.GetRepository<OrderDetail>().UpdateAsync(orderDetail);
            await _unitOfWork.SaveAsync();

            // Clear related caches
            await _cacheService.RemoveAsync($"order_detail_{id}");
            await _cacheService.RemoveByPrefixAsync("order_details_");

            return _mapper.Map<OrderDetailResponselDTO>(orderDetail);
        }

        public async Task DeleteOrderDetailAsync(Guid id)
        {
            var orderDetail = await _unitOfWork.GetRepository<OrderDetail>().GetByIdAsync(id);
            if (orderDetail == null)
                throw new Exception("Order detail not found");

            await _unitOfWork.GetRepository<OrderDetail>().DeleteAsync(orderDetail);
            await _unitOfWork.SaveAsync();

            // Clear related caches
            await _cacheService.RemoveAsync($"order_detail_{id}");
            await _cacheService.RemoveByPrefixAsync("order_details_");
        }
    }
}
