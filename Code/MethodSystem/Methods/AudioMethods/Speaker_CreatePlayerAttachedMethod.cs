using LabApi.Features.Wrappers;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Exceptions;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using UnityEngine;

namespace SER.Code.MethodSystem.Methods.AudioMethods;

[UsedImplicitly]
// ReSharper disable once InconsistentNaming
public class Speaker_CreatePlayerAttachedMethod : SynchronousMethod, ICanError
{
    public override string Description => "Creates a speaker attached to a player to play audio through.";

    public string[] ErrorReasons =>
    [
        "Player does not have a model to attach a speaker to."
    ];

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayerArgument("player to attach"),
        new TextArgument("speaker name"),
        new FloatArgument("volume", 0f)
        {
            DefaultValue = new(1f, "100%"),
            Description = "The volume of the audio."
        },
        new FloatArgument("min distance", 0f)
        {
            DefaultValue = new(5f, null),
            Description = "The minimum distance for full-volume audio."
        },
        new FloatArgument("max distance", 0f)
        {
            DefaultValue = new(15f, null),
            Description = "The maximum audible distance for the audio."
        },
        new BoolArgument("is stereo")
        {
            DefaultValue = new(true, null),
            Description = "Whether the audio will be 3D."
        },
        ..Argument.PlayersArgumentUpdating(
            "target players",
            playerDescription: "only these players will hear the audio.",
            playerDefault: new(null, "everyone"),
            updateDefault: new(false, "no update")
        )
    ];

    public override void Execute()
    {
        var player = Args.GetPlayer("player to attach");
        var speakerName = Args.GetText("speaker name");
        var volume = Args.GetFloat("volume");
        var minDistance = Args.GetFloat("min distance");
        var maxDistance = Args.GetFloat("max distance");
        var isStereo = Args.GetBool("is stereo");
        var targetPlayers = Args.GetPlayers("target players").MaybeNull();
        
        Func<ReferenceHub, bool>? condition;
        if (targetPlayers is not null && Args.GetBool("update target players"))
        {
            var func = Args.GetPlayersFunc("target players");
            condition = p => func().Contains(Player.Get(p));
        }
        else if (targetPlayers is not null)
        {
            condition = plr => targetPlayers.Contains(Player.Get(plr));
        }
        else
        {
            condition = null;
        }
        
        AudioPlayer.Create(
            speakerName, 
            condition: condition,
            onIntialCreation: p =>
            {        
                p.transform.parent = player.GameObject?.transform 
                                     ?? throw new ScriptRuntimeError(this, 
                                         $"Player '{player.Nickname}' does not have a model to attach a speaker to.");
                
                var speaker = p.AddSpeaker("Main", volume, isStereo, minDistance, maxDistance);
                
                speaker.transform.parent = player.GameObject.transform;
                speaker.transform.localPosition = Vector3.zero;
            }
        );
    }
}