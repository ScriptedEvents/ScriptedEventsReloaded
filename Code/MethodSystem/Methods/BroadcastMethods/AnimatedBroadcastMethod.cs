using System.Text.RegularExpressions;
using Cassie;
using SER.Code.ArgumentSystem.Arguments;
using SER.Code.ArgumentSystem.BaseArguments;
using SER.Code.Extensions;
using SER.Code.MethodSystem.BaseMethods.Synchronous;
using SER.Code.MethodSystem.Structures;
using SER.Code.TokenSystem.Tokens.ValueTokens;
using StringBuilder = System.Text.StringBuilder;

namespace SER.Code.MethodSystem.Methods.BroadcastMethods;

[UsedImplicitly]
public class AnimatedBroadcastMethod : SynchronousMethod, IAdditionalDescription
{
    public override string Description => "Sends an animated broadcast to specified players.";

    public override Argument[] ExpectedArguments { get; } =
    [
        new PlayersArgument("players"),
        new DurationArgument("duration"),
        new TextArgument("content")
        {
            Description = 
                "Use <charwait=x> and </charwait> tags to specify how much time each character will take to be printed, " +
                "or <wait=x> to specify a single wait action. " +
                "Example: \"<charwait=100ms>Slow print</charwait><br><wait=2s>waited 2 seconds for that!\""
        },
        new IntArgument("line break length")
        {
            Description = "The maximum amount of characters per line",
            DefaultValue = new(60, null)
        }
    ];
    
    public override void Execute()
    {
        var players = Args.GetPlayers("players");
        var content = Args.GetText("content");
        var duration = Args.GetDuration("duration").TotalSeconds;
        var lineBreakLength = Args.GetInt("line break length");
        
        foreach (var plr in players)
        {
            plr.Connection.Send(new CassieTtsPayload(string.Empty, string.Empty, false));
            plr.SendCassieMessage(
                $"$SLEEP_{duration-1} .", 
                Helper.FormatToCassieCentralScreenSubtitles(
                    content, 
                    lineBreakLength
                ), 
                false,
                0
            );
        }
    }

    public string AdditionalDescription =>
        "Uses CASSIE to make an animated broadcast - if there is CASSIE playing, it will be stopped. " +
        "Keep custom formatting to a minimum, this system is very limited.";
    
    public static class Helper
    {
        public static string FormatToRawCassieSubtitles(string text, int lineBreakLength)
        {
            var result = new StringBuilder();
            var index = 72;
            var timePerCharStack = new Stack<TimeSpan>();
        
            foreach (var line in text.Split('\n'))
            {
                // Skip empty lines
                if (line.Length == 0)
                {
                    index -= 1;
                    continue;
                }
        
                // Calculate actual length excluding HTML tags
                var len = CalculateTextLength(line);
                var parts = new List<string>();
        
                // Split long lines
                if (len > lineBreakLength)
                {
                    SplitLongLine(line, parts, lineBreakLength);
                }
                else
                {
                    parts.Add(line);
                }
        
                // Add all parts to result with proper formatting
                foreach (var part in parts)
                {
                    index -= 1;
                    result.Append(FormatLine(part, index, timePerCharStack));
                }
            }
        
            return result.ToString();
        }
        
        private static int CalculateTextLength(string line)
        {
            var len = 0;
            var isTag = false;
        
            foreach (var c in line)
            {
                switch (c)
                {
                    case '<':
                        isTag = true;
                        continue;
                    case '>':
                        isTag = false;
                        continue;
                }
        
                if (!isTag) len++;
            }
        
            return len;
        }
        
        private static void SplitLongLine(string line, List<string> parts, int lineBreakLength)
        {
            int? lastUnusedSpaceIndex = null;
            
            for (var i = 0; i < line.Length; i++)
            {
                if (!char.IsWhiteSpace(line[i])) continue;

                if (i <= lineBreakLength)
                {
                    lastUnusedSpaceIndex = i;
                    continue;
                }
                
                var lastAvailableSpaceIndex = lastUnusedSpaceIndex ?? i;
                var leftPart = line[..lastAvailableSpaceIndex].Trim();
                parts.Add(leftPart);
                
                var rightPart = line[(lastAvailableSpaceIndex + 1)..].Trim();
                if (CalculateTextLength(rightPart) > lineBreakLength)
                {
                    SplitLongLine(rightPart, parts, lineBreakLength);
                }
                else
                {
                    parts.Add(rightPart);
                }
                
                return;
            }
            
            parts.Add(line);
        }

        private static string GetDelayString(TimeSpan time)
        {
            var dots = new string('c', (int)Math.Round(time.TotalMilliseconds / 20, MidpointRounding.AwayFromZero));
            return $"<size=0>{dots}</size>";
        }
        
        private static string FormatLine(string text, int index, Stack<TimeSpan> activeDelayTags)
        {
            var openTags = new Regex(@"<charwait=((\d|\.)+(ms|s))>").Matches(text).Cast<Match>().ToArray();
            var closeTags = new Regex("</charwait>").Matches(text).Cast<Match>().ToArray();
            
            StringBuilder newText = new();
            var isTag = false;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (char.IsWhiteSpace(c))
                {
                    newText.Append(c);
                    continue;
                }

                if (openTags.FirstOrDefault(t => t.Index == i) is { } openTag)
                {
                    if (DurationToken.Parse(openTag.Groups[1].Value).HasErrored(out _, out var nullableTimeSpan)
                        || nullableTimeSpan is not { } timeSpan)
                    {
                        newText.Append(c);
                        continue;
                    }
                    
                    activeDelayTags.Push(timeSpan);
                    i = openTag.Index + openTag.Length - 1;
                    continue;
                }
                
                if (closeTags.FirstOrDefault(t => t.Index == i) is { } closeTag)
                {
                    if (activeDelayTags.Count == 0)
                    {
                        newText.Append(c);
                        continue;
                    }
                    
                    activeDelayTags.Pop();
                    i = closeTag.Index + closeTag.Length - 1;
                    continue;
                }
                
                isTag = c switch
                {
                    '<' => true,
                    '>' => false,
                    _ => isTag
                };

                if (!isTag && activeDelayTags.Count > 0)
                {
                    newText.Append($"{c}{GetDelayString(activeDelayTags.Peek())}");
                }
                else
                {
                    newText.Append(c);
                }
            }

            foreach (var match in new Regex(@"<wait=(\d+(ms|s))>").Matches(newText.ToString()).Cast<Match>().ToArray())
            {
                if (DurationToken.Parse(match.Groups[1].Value).HasErrored(out _, out var nullableTimeSpan)
                    || nullableTimeSpan is not { } timeSpan)
                {
                    continue;
                }
                
                newText.Replace(match.Value, GetDelayString(timeSpan));
            }
            
            return $"<voffset={index}em>{newText}</voffset>\\n";
        }

        public static string FormatToCassieCentralScreenSubtitles(string text, int lineBreakLength)
        {
            return
                @"<line-height=2700>\n</line-height></size><align=center><size=30><line-height=0%>\n"
                + GetDelayString(TimeSpan.FromMilliseconds(500))
                + FormatToRawCassieSubtitles(text, lineBreakLength);
        }

        // public static string FormatToCassieSpeechSubtitles(string text, bool addWaits)
        // {
        //     return @"<line-height=3500>\n</line-height></size><align=left><size=25><line-height=0%>\n"
        //            + FormatToRawCassieSubtitles(text);
        // }
    }
}

