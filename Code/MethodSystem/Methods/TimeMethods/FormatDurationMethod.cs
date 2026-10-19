using System.Globalization;
using System.Text;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.ValueSystem;

namespace SER.Code.MethodSystem.Methods.TimeMethods;

[UsedImplicitly]
public class FormatDurationMethod : ReturningMethod<TextValue>, IAdditionalDescription
{
    public override string Description => "Formats a duration into a special format.";

    public string AdditionalDescription =>
        """
        Here are some examples: 
        
        #1) "MM:SS"
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
                FFF shows milliseconds. Any other text, including apostrophes, is shown exactly as you wrote it.
                """
        }
    ];

    public override void Execute()
    {
        var durationToFormat = Args.GetDuration("duration to format");
        var format = Args.GetText("format");
        ReturnValue = new StaticTextValue(Format(durationToFormat, format));
    }

    // Longest tags first, so "FFF" is matched before anything shorter.
    private static readonly string[] Tags = ["FFF", "DD", "HH", "MM", "SS"];

    /// <summary>
    /// Replaces the DD/HH/MM/SS/FFF tags with the matching part of the duration.
    /// Everything else in the format is kept exactly as written, so apostrophes,
    /// backslashes and other symbols never break the result.
    /// </summary>
    public static string Format(TimeSpan duration, string userFormat)
    {
        duration = duration.Duration();
        if (string.IsNullOrEmpty(userFormat))
            return duration.ToString("c", CultureInfo.InvariantCulture);

        StringBuilder sb = new();
        var i = 0;
        while (i < userFormat.Length)
        {
            var tag = Tags.FirstOrDefault(t => string.CompareOrdinal(userFormat, i, t, 0, t.Length) == 0);
            if (tag is null)
            {
                sb.Append(userFormat[i]);
                i++;
                continue;
            }

            sb.Append(tag switch
            {
                "DD" => duration.Days.ToString(CultureInfo.InvariantCulture),
                "HH" => duration.Hours.ToString("00", CultureInfo.InvariantCulture),
                "MM" => duration.Minutes.ToString("00", CultureInfo.InvariantCulture),
                "SS" => duration.Seconds.ToString("00", CultureInfo.InvariantCulture),
                _ => duration.Milliseconds.ToString("000", CultureInfo.InvariantCulture)
            });
            i += tag.Length;
        }

        return sb.ToString();
    }
}