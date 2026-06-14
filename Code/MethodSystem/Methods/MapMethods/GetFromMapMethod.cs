using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.MethodSystem.BaseMethods.Synchronous;

namespace SER.Code.MethodSystem.Methods.MapMethods;

[UsedImplicitly]
public class GetFromMapMethod : ReturningMethod<CollectionValue<ReferenceValue>>
{
    public override string Description => "Gets a collection of objects from a map.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new OptionsArgument("object",
            Option.ReferenceCollection<Door>("doors"),
            Option.ReferenceCollection<Elevator>("elevators"),
            Option.ReferenceCollection<Generator>("generators"),
            Option.ReferenceCollection<Camera>("cameras"),
            Option.ReferenceCollection<Tesla>("teslas"),
            Option.ReferenceCollection<Room>("rooms"),
            Option.ReferenceCollection<LightsController>("roomLights"),
            Option.ReferenceCollection<Ragdoll>("ragdolls"))
    ];
    
    public override void Execute()
    {
        ReturnValue = Args.GetOption("object") switch
        {
            "doors" => new(Map.Doors.Select(d => new ReferenceValue<Door>(d))),
            "elevators" => new(Map.Elevators.Select(e => new ReferenceValue<Elevator>(e))),
            "generators" => new(Map.Generators.Select(g => new ReferenceValue<Generator>(g))),
            "cameras" => new(Map.Cameras.Select(c => new ReferenceValue<Camera>(c))),
            "teslas" => new(Map.Teslas.Select(t => new ReferenceValue<Tesla>(t))),
            "rooms" => new(Map.Rooms.Select(r => new ReferenceValue<Room>(r))),
            "roomlights" => new(Map.RoomLights.Select(rl => new ReferenceValue<LightsController>(rl))),
            "ragdolls" => new(Map.Ragdolls.Select(rd => new ReferenceValue<Ragdoll>(rd))),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}