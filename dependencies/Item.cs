using Elements.Geometry;
using Elements.Geometry.Solids;
using Trolley;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using Namotion.Reflection;

namespace Elements
{
    public partial class Item : GeometricElement
    {
        [JsonProperty("Add Id")]
        public string AddId { get; set; }

        public double Difficulty { get; set; }

        [JsonIgnore]
        public bool Updated { get; set; }

        public string Self { get; set; }

        public bool Locked { get; set; }

        // List of properties to exclude from serialization
        [JsonIgnore]
        private static readonly HashSet<string> ExcludedProperties = new HashSet<string>
        {
            nameof(Id),
            nameof(Self),
            nameof(Updated),
            nameof(RepresentationInstances),
        };

        public Item(ItemOverrideAddition add)
        {
            this.AddId = add.Id;
            this.Name = add.Value.Name;
            this.Transform = add.Value.Transform;
            this.Difficulty = add.Value.Difficulty;

            GenerateGeometry();
            this.Self = Serialize();
        }

        public bool Match(ItemIdentity identity)
        {
            return identity.AddId == this.AddId;
        }

        public Item Update(ItemOverride edit, List<string> warnings)
        {
            this.Name = edit.Value.Name;
            this.Transform = edit.Value.Transform;
            this.Difficulty = edit.Value.Difficulty;
            this.Self = edit.Value.Self;

            GenerateGeometry();

            var newSelf = Serialize();
            if (newSelf != this.Self)
            {
                Console.WriteLine($"Updated {this.AddId}...");
                LogDifferences(this.Self, newSelf, warnings);
                this.Updated = true;
                this.Locked = false;
                this.Self = newSelf;
            }
            else
            {
                var noDiffs = $"No Diffs for '{this.Name}'.";
                Console.WriteLine(noDiffs);
                warnings.Add(noDiffs);
                this.Locked = true;
            }
            return this;
        }

        public void GenerateGeometry()
        {
            this.RepresentationInstances = new List<RepresentationInstance>();

            this.RepresentationInstances.Add(
                new RepresentationInstance(
                    new SolidRepresentation(
                        new Extrude(Polygon.Rectangle(2, 2), 2, Vector3.ZAxis)
                    ),
                    BuiltInMaterials.Glass
                )
            );
        }

        public string Serialize()
        {
            // Create a dictionary to hold the properties excluding 'self'
            var dict = new Dictionary<string, object>();

            foreach (var prop in this.GetType().GetProperties())
            {
                if (!ExcludedProperties.Contains(prop.Name))
                {
                    dict[prop.Name] = prop.GetValue(this);
                }
            }

            // Serialize the dictionary to a JSON string
            return JsonConvert.SerializeObject(dict);
        }

        // Method to deserialize the 'self' property and apply the values to the current object
        public void DeserializeFromSelf()
        {
            if (!string.IsNullOrEmpty(Self))
            {
                // Deserialize the JSON string to a dictionary
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(Self);

                // Populate the current object with the values from the dictionary
                foreach (var kvp in dict)
                {
                    var prop = this.GetType().GetProperty(kvp.Key);
                    if (prop != null && prop.CanWrite)
                    {
                        var value = Convert.ChangeType(kvp.Value, prop.PropertyType);
                        prop.SetValue(this, value);
                    }
                }
            }
        }

        private void LogDifferences(string oldSelf, string newSelf, List<string> warnings)
        {
            var oldDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(oldSelf);
            var newDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(newSelf);

            var diffHeader = $"Property Diffs for '{this.Name}'.";
            Console.WriteLine(diffHeader);
            warnings.Add(diffHeader);
            LogDifferencesRecursive(oldDict, newDict, string.Empty, warnings);
        }

        // Recursive method to log differences between two dictionaries
        private void LogDifferencesRecursive(Dictionary<string, object> oldDict, Dictionary<string, object> newDict, string parent, List<string> warnings)
        {
            foreach (var kvp in newDict)
            {
                if (oldDict.TryGetValue(kvp.Key, out var oldValue))
                {
                    if (oldValue is JObject oldObj && kvp.Value is JObject newObj)
                    {
                        // Recurse for nested objects
                        var oldNestedDict = oldObj.ToObject<Dictionary<string, object>>();
                        var newNestedDict = newObj.ToObject<Dictionary<string, object>>();
                        LogDifferencesRecursive(oldNestedDict, newNestedDict, $"{parent}{kvp.Key}.", warnings);
                    }
                    else
                    {
                        var oldToken = oldValue != null ? JToken.FromObject(oldValue) : JValue.CreateNull();
                        var newToken = kvp.Value != null ? JToken.FromObject(kvp.Value) : JValue.CreateNull();

                        if (!JToken.DeepEquals(oldToken, newToken))
                        {
                            var diff = $"Property '{parent}{kvp.Key}' changed from '{oldToken}' to '{newToken}'";
                            Console.WriteLine(diff);
                            warnings.Add(diff);
                        }
                    }
                }
                else
                {
                    var diff = $"Property '{parent}{kvp.Key}' added with value '{kvp.Value}'";
                    Console.WriteLine(diff);
                    warnings.Add(diff);
                }
            }

            foreach (var kvp in oldDict)
            {
                if (!newDict.ContainsKey(kvp.Key))
                {

                    var diff = $"Property '{parent}{kvp.Key}' with value '{kvp.Value}' was removed";
                    Console.WriteLine(diff);
                    warnings.Add(diff);
                }
            }
        }
    }
}