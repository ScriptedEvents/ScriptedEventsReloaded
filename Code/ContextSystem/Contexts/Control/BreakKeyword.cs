using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Helpers.OldResultSystem;
using SER.Code.TokenSystem.Tokens;

namespace SER.Code.ContextSystem.Contexts.Control;

[UsedImplicitly]
public class BreakKeyword : StandardContext, IKeywordContext
{
    public override string FriendlyName => "'break' keyword";
    public string KeywordName => "break";
    public string Description =>
        "Makes a given loop or function (that the 'break' keyword is inside) act as it has completely ended its execution " +
        "(\"breaks\" free from the loop/function)";

    public string[] Arguments => [];

    public string Example =>
          """
          # for example:
          forever
              wait 1s
              
              Print "attempting to leave forever loop"
              if {Chance 20%}
                  break
              end
          end

          func Test
              if {Chance 20%}
                  break
              end
              
              Print "this will not run because the 'break' keyword was used"
          end

          run Test
          """;

    public override TryAddTokenRes TryAddToken(BaseToken token)
    {
        return TryAddTokenRes.Error("The 'break' keyword does not expect arguments after it.");
    }

    public override OldResult VerifyCurrentState()
    {
        return true;
    }

    protected override void Execute()
    {
        ParentContext?.SendControlMessage(new Break());
    }
}