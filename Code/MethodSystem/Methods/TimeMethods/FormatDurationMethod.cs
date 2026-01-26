using JetBrains.Annotations;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Helpers;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.MethodDescriptors;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.TimeMethods;

[UsedImplicitly]
public class FormatDurationMethod : ReturningMethod<TextValue>, IAdditionalDescription
{
    public override string Description => "Formats a duration into a special format.";

    public string AdditionalDescription =>
        """
        Here are some examples: 
        
        #1) "HH:SS"
        Shows only minutes and seconds, perfect for round timers or cooldowns.
        Result: 01:30
        
        #2) "DD days, HH hours and MM minutes"
        Creates a readable sentence. The system automatically handles the spaces and commas.
        Result: 2 days, 04 hours and 15 minutes
        
        #3) "MMm SSs FFFms"
        Displays minutes, seconds, and milliseconds for pinpoint accuracy.
        Result: 01m 22s 450ms
        """;

    public override Argument[] ExpectedArguments { get; } =
    [
        new DurationArgument("duration to format"),
        new TextArgument("format")
        {
            Description = 
                """
                Use these tags to format your time: DD for days, HH for hours, MM for minutes, and SS for seconds. 
                You can separate them with colons (:) or dashes (-).
                """
        }
    ];

    public override void Execute()
    {
        var durationToFormat = Args.GetDuration("duration to format");
        var format = Args.GetText("format");
        ReturnValue = new DynamicTextValue(durationToFormat.ToString(CreateNativeFormat(format)), Script);
    }

    private static readonly Dictionary<string, string> TokenMap = new()
    {
        { "DD", @"d\ " }, 
        { "HH", "hh" },   
        { "MM", "mm" },   
        { "SS", "ss" },   
        { "FFF", "fff" }  
    };

    public static string CreateNativeFormat(string userFormat)
    {
        if (string.IsNullOrEmpty(userFormat)) return "c";

        // 1. Put a quote at the start and end of the string
        // 2. "Break" the quotes only when we find a token
        // Example: HH:MM -> 'hh':'mm'
    
        string result = "'" + userFormat + "'";

        // We replace the User Token with: ' + Native Token + '
        // This effectively closes the literal string, puts the command, and reopens the literal.
        result = result.Replace("DD", "'d'")
            .Replace("HH", "'hh'")
            .Replace("MM", "'mm'")
            .Replace("SS", "'ss'")
            .Replace("FFF", "'fff'");

        // Clean up empty quotes '' created if tokens are next to each other
        return result.Replace("''", "");
    }
}