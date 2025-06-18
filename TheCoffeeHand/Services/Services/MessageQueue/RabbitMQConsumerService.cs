using Domain.Entities;
using Domain.Entities.CoffeeMachine;
using Interfracture.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Services.ServiceInterfaces;
using Services.Services.MessageQueue;
using System.Diagnostics;
using System.Text;

namespace Services.Services {
    public class RabbitMQConsumerService: BackgroundService, IRabbitMQConsumerService {
        private readonly ILogger<RabbitMQConsumerService> _logger;
        private readonly ConnectionFactory _factory;
        private IChannel _channel;
        private IConnection _connection;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRabbitMQService _rabbitMQService;
        private const string QueueName = "test_queue";

        public RabbitMQConsumerService(ConnectionFactory factory, ILogger<RabbitMQConsumerService> logger, IServiceProvider serviceProvider, IRabbitMQService rabbitMQService) {
            _factory = factory;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _rabbitMQService = rabbitMQService;
        }

        public async Task StartConsumingAsync(string queueName) {
            try {
                _connection = await _factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.QueueDeclareAsync(queue: queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) => {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    //_logger.LogInformation($" [x] Received: {message}");

                    try {
                        await ProcessMessageAsync(message);
                    } catch (Exception ex) {
                        _logger.LogError($"Error processing message: {ex.Message}");
                    }

                    // Xác nhận tin nhắn đã xử lý
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                };

                await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);

                _logger.LogInformation($"[*] Listening on queue: {queueName}");
            } catch (Exception ex) {
                _logger.LogError($"Error consuming messages from {queueName}: {ex.Message}");
            }
        }

        public async Task ProcessMessageAsync(string message) {
            var orderMessage = JsonConvert.DeserializeObject<OrderMessage>(message);

            try {
                using (var scope = _serviceProvider.CreateScope()) {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var mongoDbUnitOfWork = scope.ServiceProvider.GetRequiredService<IMongoDbUnitOfWork>();

                    foreach (var drink in orderMessage.Drinks) {
                        var ingredientDetails = await GetDrinkDetailFromDatabaseAsync(drink.DrinkId, unitOfWork);
                        var recipeList = await GetDrinkRecipeFromDatabaseAsync(drink.DrinkId, mongoDbUnitOfWork);

                        if (!recipeList.Any()) {
                            _logger.LogWarning($"No recipe found for DrinkId: {drink.DrinkId}");
                            continue;
                        }

                        var recipe = recipeList.First();

                        for (int i = 0; i < drink.Quantity; i++) {
                            var actions = new List<dynamic>();
                            int sequence = 1;

                            foreach (var step in recipe.RecipeSteps) {

                                var machineCollection = mongoDbUnitOfWork.GetCollection<Machine>("machine-info");
                                Machine machine = await machineCollection.Find(x => x.MachineName == step.MachineName).FirstAsync();


                                if (machine == null) {
                                    _logger.LogWarning($"No machine found");
                                    continue;
                                }

                                var parameters = new Dictionary<string, double>();

                                //foreach (var parameter in machine.Parameters) {
                                //    if (parameter.Mode.ToLower() == step.Action.ToLower()) {
                                //        foreach (var paramSet in step.ParametersRequired) {
                                //            foreach (var paramKey in paramSet.Keys) {
                                //                var paramList = paramSet[paramKey];
                                //                if (paramList.Any()) {
                                //                    parameters[paramKey] = paramList.First().Value;
                                //                    actionAdded = true;
                                //                }
                                //            }
                                //        }
                                //    }
                                //}

                                foreach (var param in step.ParametersRequired) {
                                    parameters.Add(param.Name, param.Value);
                                }

                                actions.Add(new {
                                    action_id = $"{step.Action}_{step.Ingredient.Replace(' ', '_')}",
                                    machine = step.MachineName,
                                    mode = step.Action,
                                    parameters = parameters,
                                    sequence = sequence++
                                });

                            }

                            Debug.WriteLine($" [x] Actions: {JsonConvert.SerializeObject(actions, Formatting.Indented)}");

                            var machineMessage = new {
                                activity_id = $"MK_{drink.DrinkId}_{orderMessage.OrderId}",
                                name = $"Make {drink.DrinkName}",
                                description = $"Make {drink.DrinkName} follow recipe.",
                                actions = actions,
                            };

                            string machineMessageJson = JsonConvert.SerializeObject(machineMessage, Formatting.Indented);

                            await _rabbitMQService.SendMessageAsync("machine_queue", machineMessageJson);
                        }
                    }
                }
            } catch (Exception ex) {
                _logger.LogError($"Error processing message: {ex.Message}");
            }
        }

        //private async Task<string> GetMachineByIngredientAndModeAsync(string ingredient, string mode, IMongoDbUnitOfWork mongoDbUnitOfWork) {
        //    var machineCollection = mongoDbUnitOfWork.GetCollection<Machine>("machines");


        //    var machine = await machineCollection.Find(m =>
        //        m.Ingredient.ToLower().Contains(ingredient.ToLower()) &&
        //        m.Parameters.Any(p => p.Mode.ToLower() == mode.ToLower())
        //    ).FirstOrDefaultAsync();

        //    if (machine == null) {
        //        _logger.LogWarning($"No machine found for Ingredient: {ingredient} with Mode: {mode}");
        //        return string.Empty;
        //    }

        //    _logger.LogInformation($"Found machine '{machine.MachineName}' for Ingredient: {ingredient} with Mode: {mode}");
        //    return machine.MachineName;

        //}



        private async Task<List<dynamic>> GetDrinkDetailFromDatabaseAsync(Guid drinkId, IUnitOfWork unitOfWork) {

            var recipes = await unitOfWork.GetRepository<Recipe>()
                .Entities
                .Where(r => r.DrinkId == drinkId)
                .Include(r => r.Ingredient)
                .ToListAsync();
            _logger.LogInformation($" [x] Ingredient: {recipes.Count}");

            var ingredientDetails = recipes
                .Where(r => r.Ingredient != null)
                .Select(r => new {
                    IngredientId = r.Ingredient!.Id,
                    IngredientName = r.Ingredient.Name,
                    RequiredQuantity = r.Quantity
                }).ToList<dynamic>();

            return ingredientDetails;
        }

        private async Task<List<DrinkRecipe>> GetDrinkRecipeFromDatabaseAsync(Guid drinkId, IMongoDbUnitOfWork mongoDbUnitOfWork) {

            var recipesCollection = mongoDbUnitOfWork.GetCollection<DrinkRecipe>("recipe");

            _logger.LogInformation($" [x] DRINK: {drinkId}");

            string recipeId = $"recipe_{drinkId}";

            var recipe = await recipesCollection.Find(x => x.Id == recipeId).ToListAsync();

            if (recipe == null || recipe.Count == 0) {
                _logger.LogWarning($"No recipe found for DrinkId: {drinkId}");
                return new List<DrinkRecipe>();
            }

            _logger.LogInformation($"Recipe found for DrinkId: {drinkId}, Name: {recipe.First().DrinkName}");

            return recipe;

        }

        private async Task<List<Machine>> GetMachineDetailListNeed(List<dynamic> ingredients, IMongoDbUnitOfWork mongoDbUnitOfWork) {
            var machines = new List<Machine>();
            var machineCollection = mongoDbUnitOfWork.GetCollection<Machine>("machine-info");

            foreach (var ingredient in ingredients) {
                string ingName = ingredient.IngredientName.ToString().ToLower();

                var machine = await machineCollection.Find(m => m.Ingredient.ToLower().Contains(ingName)).FirstOrDefaultAsync();

                if (machine != null) {
                    machines.Add(machine);
                }
            }

            _logger.LogInformation($" [x] Machines found: {machineCollection}");

            return machines;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            await StartConsumingAsync(QueueName);
        }

        public void Dispose() {
            _channel?.CloseAsync();
            _connection?.CloseAsync();
            base.Dispose();
        }
    }
}
