using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Domain.Entities {
    public class DrinkRecipe {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("drink_name")]
        public string DrinkName { get; set; } = string.Empty;

        [BsonElement("recipe")]
        public List<RecipeStep> RecipeSteps { get; set; } = new();
    }

    public class RecipeStep {
        [BsonElement("ingredient")]
        public string Ingredient { get; set; } = string.Empty;

        [BsonElement("machine_name")]
        public string MachineName { get; set; } = string.Empty;

        [BsonElement("action")]
        public string Action { get; set; } = string.Empty;

        [BsonElement("parameters_required")]
        public List<Parameter> ParametersRequired { get; set; } = new();
    }

    public class Parameter {
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("value")]
        public double Value { get; set; }
    }
}
