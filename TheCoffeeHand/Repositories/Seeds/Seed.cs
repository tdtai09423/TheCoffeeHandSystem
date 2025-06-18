using Core.Constants.Enum;
using Domain.Entities;
using Interfracture.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Base;

namespace Repositories.Seeds
{
    public class Seed
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
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
        }

        private static async Task SeedRoles(RoleManager<ApplicationRole> roleManager)
        {
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                }
            }
        }

        private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@thecoffeehand.com",
                Email = "admin@thecoffeehand.com",
                FirstName = "Admin",
                LastName = "User",
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            string adminPassword = "Admin@123";
            var user = await userManager.FindByEmailAsync(adminUser.Email);

            if (user == null)
            {
                var createUser = await userManager.CreateAsync(adminUser, adminPassword);
                if (createUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private static async Task SeedCategories(ApplicationDbContext context)
        {
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Coffee" },
                    new Category { Name = "Tea" }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedDrinks(ApplicationDbContext context)
        {
            if (!context.Drinks.Any())
            {
                var coffeeCategory = context.Categories.First(c => c.Name == "Coffee");
                var teaCategory = context.Categories.First(c => c.Name == "Tea");

                context.Drinks.AddRange(
                    new Drink { Name = "Espresso", Price = 2.50, CategoryId = coffeeCategory.Id, isAvailable = true },
                    new Drink { Name = "Latte", Price = 3.50, CategoryId = coffeeCategory.Id, isAvailable = true },
                    new Drink { Name = "Green Tea", Price = 2.00, CategoryId = teaCategory.Id, isAvailable = true },
                    new Drink { Name = "Milk Coffee", Price = 2.00, CategoryId = coffeeCategory.Id, isAvailable = true }
                );
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedOrders(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (!context.Orders.Any())
            {
                var adminUser = await userManager.FindByEmailAsync("admin@thecoffeehand.com");
                var drinks = context.Drinks.ToList();

                var random = new Random();
                var orders = new List<Order>();

                for (int i = 0; i < 5; i++)
                {
                    var orderDate = new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month, random.Next(1, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) + 1),
                                                       random.Next(7, 20), random.Next(0, 60), 0, TimeSpan.Zero);

                    var selectedDrinks = drinks.OrderBy(x => random.Next()).Take(2).ToList();
                    double totalPrice = selectedDrinks.Sum(d => d.Price);

                    var orderDetails = selectedDrinks.Select(d => new OrderDetail
                    {
                        DrinkId = d.Id,
                        Total = 1,
                        Note = "Random order"
                    }).ToList();

                    orders.Add(new Order
                    {
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

        private static async Task SeedIngredients(ApplicationDbContext context)
        {
            if (!context.Ingredients.Any()) // Prevent duplicate seeding
            {
                context.Ingredients.AddRange(
                    new Ingredient { Name = "Coffee Beans", Quantity = 1000 },
                    new Ingredient { Name = "Milk", Quantity = 50 },
                    new Ingredient { Name = "Sugar", Quantity = 500 },
                    new Ingredient { Name = "Tea Leaves", Quantity = 300 },
                    new Ingredient { Name = "Cocoa Powder", Quantity = 200 },
                    new Ingredient { Name = "Vanilla Syrup", Quantity = 150 },
                    new Ingredient { Name = "Ice", Quantity = 150 },
                    new Ingredient { Name = "Water", Quantity = 150000 }
                );
                await context.SaveChangesAsync();
            }
        }

    }
}
