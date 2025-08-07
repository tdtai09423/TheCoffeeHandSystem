using Core.Constants.Enum;
using Domain.Entities;
using Interfracture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Base;

namespace Repositories.Seeds {
    public class Seed {
        public static async Task Initialize(IServiceProvider serviceProvider) {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Seed roles
            await SeedRoles(roleManager);

            // Seed admin user
            await SeedAdminUser(userManager);

            // Seed categories
            await SeedCategories(context);

            // Seed drinks
            await SeedDrinks(context);

            // Seed orders
            await SeedOrders(context, userManager);

            // Seed ingredients
            await SeedIngredients(context);

            // Seed recipes
            await SeedRecipes(context);
        }

        private static async Task SeedRoles(RoleManager<ApplicationRole> roleManager) {
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames) {
                if (!await roleManager.RoleExistsAsync(roleName)) {
                    await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                }
            }
        }

        private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager) {
            var adminUser = new ApplicationUser {
                UserName = "admin@thecoffeehand.com",
                Email = "admin@thecoffeehand.com",
                FirstName = "Admin",
                LastName = "User",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            string adminPassword = "Admin@123";
            var user = await userManager.FindByEmailAsync(adminUser.Email);

            if (user == null) {
                var createUser = await userManager.CreateAsync(adminUser, adminPassword);
                if (createUser.Succeeded) {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }


        private static async Task SeedCategories(ApplicationDbContext context) {
            if (!context.Categories.Any()) {
                context.Categories.AddRange(
                    new Category { Name = "Coffee" },
                    new Category { Name = "Tea" },
                    new Category { Name = "Latte" },
                    new Category { Name = "Smoothie" },
                    new Category { Name = "Milk" },
                    new Category { Name = "Juice" }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedDrinks(ApplicationDbContext context) {
            if (!context.Drinks.Any()) {
                var coffeeCategory = context.Categories.First(c => c.Name == "Coffee");
                var teaCategory = context.Categories.First(c => c.Name == "Tea");
                var latteCategory = context.Categories.First(c => c.Name == "Latte");
                var smoothieCategory = context.Categories.First(c => c.Name == "Smoothie");
                var milkCategory = context.Categories.First(c => c.Name == "Milk");
                var juiceCategory = context.Categories.First(c => c.Name == "Juice");

                context.Drinks.AddRange(
                    new Drink { Name = "Espresso", Price = 2.50, CategoryId = coffeeCategory.Id, isAvailable = true },
                    new Drink { Name = "Dark Coffee", Price = 3.50, CategoryId = coffeeCategory.Id, isAvailable = true },
                    new Drink { Name = "Green Tea", Price = 2.00, CategoryId = teaCategory.Id, isAvailable = true },
                    new Drink { Name = "Black Tea", Price = 2.50, CategoryId = teaCategory.Id, isAvailable = true },
                    new Drink { Name = "Milk Coffee", Price = 2.00, CategoryId = coffeeCategory.Id, isAvailable = true },
                    new Drink { Name = "Matcha Latte", Price = 3.00, CategoryId = latteCategory.Id, isAvailable = true },
                    new Drink { Name = "Cookie Latte", Price = 3.00, CategoryId = latteCategory.Id, isAvailable = true },
                    new Drink { Name = "Cookie Matcha Latte", Price = 3.70, CategoryId = latteCategory.Id, isAvailable = true },
                    new Drink { Name = "Strawberry Smoothie", Price = 4.00, CategoryId = smoothieCategory.Id, isAvailable = true },
                    new Drink { Name = "Banana Smoothie", Price = 4.00, CategoryId = smoothieCategory.Id, isAvailable = true },
                    new Drink { Name = "Chocolate Milk", Price = 2.50, CategoryId = milkCategory.Id, isAvailable = true },
                    new Drink { Name = "Vanilla Milk", Price = 2.50, CategoryId = milkCategory.Id, isAvailable = true },
                    new Drink { Name = "Orange Juice", Price = 3.00, CategoryId = juiceCategory.Id, isAvailable = true },
                    new Drink { Name = "Lemonade", Price = 2.50, CategoryId = juiceCategory.Id, isAvailable = true }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedOrders(ApplicationDbContext context, UserManager<ApplicationUser> userManager) {
            if (!context.Orders.Any()) {
                var adminUser = await userManager.FindByEmailAsync("admin@thecoffeehand.com");
                var drinks = context.Drinks.ToList();

                var random = new Random();
                var orders = new List<Order>();

                for (int i = 0; i < 5; i++) {
                    var orderDate = new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month, random.Next(1, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) + 1),
                                                       random.Next(7, 20), random.Next(0, 60), 0, TimeSpan.Zero);

                    var selectedDrinks = drinks.OrderBy(x => random.Next()).Take(2).ToList();
                    double totalPrice = selectedDrinks.Sum(d => d.Price);

                    var orderDetails = selectedDrinks.Select(d => new OrderDetail {
                        DrinkId = d.Id,
                        Total = 1,
                        Note = "Random order"
                    }).ToList();

                    orders.Add(new Order {
                        Date = orderDate,
                        Status = EnumOrderStatus.Done,
                        TotalPrice = totalPrice,
                        UserId = adminUser.Id,
                        OrderDetails = orderDetails
                    });
                }

                context.Orders.AddRange(orders);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedIngredients(ApplicationDbContext context) {
            if (!context.Ingredients.Any()) // Prevent duplicate seeding
            {
                context.Ingredients.AddRange(
                    new Ingredient { Name = "Coffee Beans", Quantity = 1000000 },
                    new Ingredient { Name = "Milk", Quantity = 5000000 },
                    new Ingredient { Name = "Sugar", Quantity = 5000000 },
                    new Ingredient { Name = "Tea Leaves", Quantity = 3000000 },
                    new Ingredient { Name = "Cocoa Powder", Quantity = 2000000 },
                    new Ingredient { Name = "Vanilla Syrup", Quantity = 1500000 },
                    new Ingredient { Name = "Ice", Quantity = 1500000 },
                    new Ingredient { Name = "Water", Quantity = 150000 },
                    new Ingredient { Name = "Lemon Juice", Quantity = 1000000 },
                    new Ingredient { Name = "Strawberry Puree", Quantity = 20000000 },
                    new Ingredient { Name = "Banana", Quantity = 1000000000 },
                    new Ingredient { Name = "Orange Juice", Quantity = 200000000 },
                    new Ingredient { Name = "Coffee flour", Quantity = 100000000 }
                );
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedRecipes(ApplicationDbContext context) {
            //if (!context.Recipes.Any()) // Prevent duplicate seeding
            //{
            //    var ingredients = context.Ingredients.ToList();
            //    var drinks = context.Drinks.ToList();
            //    context.Recipes.AddRange(
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Espresso").Id, IngredientId = ingredients.First(i => i.Name == "Coffee Beans").Id, Quantity = 10 },
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Dark Coffee").Id, IngredientId = ingredients.First(i => i.Name == "Coffee Beans").Id, Quantity = 15 },
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Green Tea").Id, IngredientId = ingredients.First(i => i.Name == "Tea Leaves").Id, Quantity = 5 },
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Black Tea").Id, IngredientId = ingredients.First(i => i.Name == "Tea Leaves").Id, Quantity = 5 },
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Milk Coffee").Id, IngredientId = ingredients.First(i => i.Name == "Coffee Beans").Id, Quantity = 10 },
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Matcha Latte").Id, IngredientId = ingredients.First(i => i.Name == "Tea Leaves").Id, Quantity = 5 },
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Cookie Latte").Id, IngredientId = ingredients.First(i => i.Name == "Coffee Beans").Id, Quantity = 10 },
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Cookie Matcha Latte").Id, IngredientId = ingredients.First(i => i.Name == "Tea Leaves").Id, Quantity = 5 },
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Strawberry Smoothie").Id, IngredientId = ingredients.First(i => i.Name == "Strawberry Puree").Id, Quantity = 20 },
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Banana Smoothie").Id, IngredientId = ingredients.First(i => i.Name == "Banana").Id, Quantity = 1 },
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Chocolate Milk").Id, IngredientId = ingredients.First(i => i.Name == "Cocoa Powder").Id, Quantity = 5 },
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Vanilla Milk").Id, IngredientId = ingredients.First(i => i.Name == "Vanilla Syrup").Id, Quantity = 10 },
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Orange Juice").Id, IngredientId = ingredients.First(i => i.Name == "Orange Juice").Id, Quantity = 50 },
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Apple Juice").Id, IngredientId = ingredients.First(i => i.Name == "Orange Juice").Id, Quantity = 50 },
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Lemonade").Id, IngredientId = ingredients.First(i => i.Name == "Lemon Juice").Id, Quantity = 20 },
            //        new Recipe { DrinkId = drinks.First(d => d.Name == "Coffee flour").Id, IngredientId = ingredients.First(i => i.Name == "Coffee flour").Id, Quantity = 10 }
            //        );
            //    await context.SaveChangesAsync();
            //}
            if (!context.Recipes.Any()) {
                var drinks = context.Drinks.ToList();
                var ingredients = context.Ingredients.ToList();
                var random = new Random();

                var recipes = new List<Recipe>();

                foreach (var drink in drinks) {

                    var selectedIngredients = ingredients.OrderBy(x => random.Next()).Take(random.Next(2, 4)).ToList();

                    foreach (var ingredient in selectedIngredients) {
                        recipes.Add(new Recipe {
                            DrinkId = drink.Id,
                            IngredientId = ingredient.Id,
                            Quantity = random.Next(1, 6)
                        });
                    }
                }

                context.Recipes.AddRange(recipes);
                await context.SaveChangesAsync();
            }
        }

    }
}
