using Domain.Entities;
using Domain.Entities.CoffeeMachine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.ServiceInterfaces {
    public interface IMachineInfoService {
        Task<List<Machine>> GetAllMachinesAsync();
        Task<Machine?> GetMachineByIdAsync(string machineId);
        Task<List<DrinkRecipe>> GetAllRecipesAsync();
        Task<DrinkRecipe?> GetRecipeByIdAsync(string recipeId);
        Task<bool> CreateDrinkRecipeAsync(DrinkRecipe newRecipe, Guid drinkId);
    }
}
