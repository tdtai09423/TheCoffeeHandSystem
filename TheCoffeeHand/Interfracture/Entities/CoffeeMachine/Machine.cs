using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Domain.Entities.CoffeeMachine {
    public class Machine {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("machine_name")]
        public string MachineName { get; set; } = string.Empty;

        [BsonElement("ingredient")]
        public string Ingredient { get; set; } = string.Empty;

        [BsonElement("parameters")]
        public List<MachineParameter> Parameters { get; set; } = new List<MachineParameter>();
    }
}
