using Domain.Entities;
using Domain.Entities.CoffeeMachine;
using Interfracture.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using Services.ServiceInterfaces;

namespace Services.Services {
    public class MachineInfoService: IMachineInfoService {
        private readonly IMongoDbUnitOfWork _mongoDbUnitOfWork;

        public MachineInfoService(IMongoDbUnitOfWork mongoDbUnitOfWork) {
            _mongoDbUnitOfWork = mongoDbUnitOfWork;
        }

        public async Task<List<Machine>> GetAllMachinesAsync() {
            var machineCollection = _mongoDbUnitOfWork.GetCollection<Machine>("machine-info");
            var machines = await machineCollection.Find(new BsonDocument()).ToListAsync();
            return machines;
        }

        public async Task<Machine?> GetMachineByIdAsync(string machineId) {
            var machineCollection = _mongoDbUnitOfWork.GetCollection<Machine>("machine-info");
            var filter = Builders<Machine>.Filter.Eq(m => m.Id, machineId);
            var machine = await machineCollection.Find(filter).FirstOrDefaultAsync();
            return machine;
        }

        public async Task<List<DrinkRecipe>> GetAllRecipesAsync() {
            var recipeCollection = _mongoDbUnitOfWork.GetCollection<DrinkRecipe>("recipe");
            var recipes = await recipeCollection.Find(new BsonDocument()).ToListAsync();
            return recipes;
        }

        public async Task<bool> CreateDrinkRecipeAsync(DrinkRecipe newRecipe, Guid drinkId) {
            try {
                var recipeCollection = _mongoDbUnitOfWork.GetCollection<DrinkRecipe>("recipe");

                // Tạo ID mới nếu chưa có
                //if (string.IsNullOrEmpty(newRecipe.Id)) {
                //    newRecipe.Id = $"recipe_{drinkId}";
                //}

                // Thêm công thức mới vào MongoDB
                await recipeCollection.InsertOneAsync(newRecipe);
                return true;
            } catch (Exception ex) {
                Console.WriteLine($"[ERROR] Failed to create recipe: {ex.Message}");
                return false;
            }
        }

        public Task<DrinkRecipe?> GetRecipeByIdAsync(string recipeId) {
            throw new NotImplementedException();
        }
    }
}
