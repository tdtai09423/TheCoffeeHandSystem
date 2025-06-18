using MongoDB.Bson.Serialization.Attributes;


namespace Domain.Entities.CoffeeMachine {
    public class MachineParameter {
        [BsonElement("mode")]
        public string Mode { get; set; } = string.Empty;

        [BsonElement("parameters_required")]
        public List<string> ParametersRequired { get; set; } = new List<string>();
    }
}
