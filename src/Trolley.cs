using Elements;
using Elements.Geometry;
using System.Collections.Generic;

namespace Trolley
{
  public static class Trolley
  {
    /// <summary>
    /// The Trolley function.
    /// </summary>
    /// <param name="model">The input model.</param>
    /// <param name="input">The arguments to the execution.</param>
    /// <returns>A TrolleyOutputs instance containing computed results and the model with any new elements.</returns>
    public static TrolleyOutputs Execute(Dictionary<string, Model> inputModels, TrolleyInputs input)
    {
      // Your code here.
      var output = new TrolleyOutputs();

      var items = input.Overrides.Item.CreateElements(
        input.Overrides.Additions.Item,
        input.Overrides.Removals.Item,
        (add) => new Item(add),
        (item, identity) => item.Match(identity),
        (item, edit) => item.Update(edit, output.Warnings)
      );

      items.ForEach((item) =>
      {
        if (item.Updated)
        {
          ComplexFunction(input, item);
        }
        else
        {
          item.DeserializeFromSelf();
        }
      });

      output.Model.AddElements(items);

      return output;

    }

    public static void ComplexFunction(TrolleyInputs input, Item item)
    {
      var time = Convert.ToInt32(item.Difficulty) * 1000;
      Console.WriteLine($"Starting complex task (synchronous) for {item.AddId} that'll take {time}ms...");
      Thread.Sleep(time); // Sleep for difficulty seconds
      input.Overrides.Item.Apply(
        new List<Item>() { item },
        (item, identity) => item.AddId == identity.AddId,
        (item, edit) =>
        {
          item.Result = time;
          item.Updated = false;
          item.Locked = false;
          item.Self = item.Serialize();
          return item;
        }
      );
      Console.WriteLine("Complex task completed (synchronous).");
    }
  }
}