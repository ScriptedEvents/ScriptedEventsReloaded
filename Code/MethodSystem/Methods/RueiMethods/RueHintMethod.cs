using LabApi.Features.Wrappers;
using RueI.API;
using RueI.API.Elements;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.ArgumentSystem.Structures;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.MethodSystem.Structures;
using Tag = RueI.API.Elements.Tag;

namespace SER.Code.MethodSystem.Methods.RueiMethods;

public class RueHintMethod : SynchronousMethod, IDependOnFramework
{
    public override string Description => "Sends or removes hints (in Rue library) of players";
    public IDependOnFramework.Type DependsOn => IDependOnFramework.Type.Ruei;

    public const string Players = "players";
    public const string Option = "modifying option";
    public const string Id = "hint id";
    public const string Text = "hint text content";

    public const string Position = "y position";
    public const string Duration = "duration";

    public const string ZPosition = "z position";
    public const string Resolution = "y resolution based";
    public const string Vertical = "vertical alignment";
    public const string NoParse = "noparse";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument(Players)
        {
            Description = "The players that will have hint shown/removed"
        },

        new OptionsArgument(Option,
            new Option("Show", "Shows the player hint with certain tag text and position"),
            new Option("Remove", "Removes hint with certain id, doesn't require arguments after id.")
        )
        {
            Description = "Main option argument required."
        },

        new TextArgument(Id)
        {
            Description = "Id required for the hint (if same id will be shown again it will override the last hint)"
        },

        new TextArgument(Text)
        {
            DefaultValue = new("", "Empty"),
            Description = "The message of hint shown, optional in case Option is set to Remove."
        },

        new FloatArgument(Position, 0, 1000)
        {
            DefaultValue = new(300f, "300"),
            Description = "The position of hint (Y position), optional in case Option is set to Remove."
        },

        new DurationArgument(Duration)
        {
            DefaultValue = new(TimeSpan.MaxValue, $"MaxValue ({float.MaxValue})"),
            Description = "The duration of hint, optional in case Option is set to Remove."
        },

        new IntArgument(ZPosition)
        {
            DefaultValue = new(0, "0"),
            Description = "The Z position of hint, meaning what hint overlaps what (priority)"
        },

        new BoolArgument(Resolution)
        {
            DefaultValue = new(false, "false"),
            Description = "Resolution Based option serves to automatically move hints based on resolution (on the Y position)"
        },

        new EnumArgument<RueI.API.Elements.Enums.VerticalAlign>(Vertical)
        {
            DefaultValue = new(RueI.API.Elements.Enums.VerticalAlign.Center, "Center"),
            Description = "Sets the vertical alignment of the hint."
        },

        new EnumArgument<RueI.API.Elements.Enums.NoparseSettings>(NoParse)
        {
            DefaultValue = new(RueI.API.Elements.Enums.NoparseSettings.ParsesNone, "ParsesNone"),
            Description = "Sets the no parse option."
        },
    ];

    public override void Execute()
    {
        var players = Args.GetPlayers(Players);
        var option = Args.GetOption(Option);
        var id = Args.GetText(Id);
        var tag = new Tag(id);

        var message = Args.GetText(Text);
        var position = Args.GetFloat(Position);
        var duration = Args.GetDuration(Duration);

        var priority = Args.GetInt(ZPosition);
        var resolutionBased = Args.GetBool(Resolution);
        var vertical = Args.GetEnum<RueI.API.Elements.Enums.VerticalAlign>(Vertical);
        var noParse = Args.GetEnum<RueI.API.Elements.Enums.NoparseSettings>(NoParse);

        foreach (var plr in players)
        {
            var display = RueDisplay.Get(plr);

            if (option == "remove")
            {
                display.Remove(tag);
                continue;
            }

            if (option == "show")
            {
                display.Show(tag, new BasicElement(position, message)
                {
                    ResolutionBasedAlign = resolutionBased,
                    VerticalAlign = vertical,
                    NoparseSettings = noParse,
                    ZIndex = priority,
                }, duration);
            }
        }
    }
}
