using SER.Code.ContextSystem.BaseContexts;
using SER.Code.ContextSystem.Contexts.Control.Loops;
using SER.Code.ContextSystem.Interfaces;
using SER.Code.ContextSystem.Structures;
using SER.Code.Exceptions;
using SER.Code.FlagSystem.Flags;
using SER.Code.Helpers.ResultSystem;
using SER.Code.ScriptSystem;
using SER.Code.ScriptSystem.Structures;
using SER.Code.TokenSystem;
using SER.Code.TokenSystem.Tokens;
using SER.Code.TokenSystem.Tokens.VariableTokens;
using SER.Code.ValueSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.Helpers;

internal static class AuditRegressionChecks
{
    internal static string? Verify()
    {
        var collection = new CollectionValue(new Value[] { new NumberValue(1), new NumberValue(2), new NumberValue(1) });
        foreach (var (amount, expected) in new[] { (-1, "2"), (0, "1,2,1"), (1, "2,1"), (2, "2"), (5, "2") })
        {
            var result = CollectionValue.Remove(collection, new NumberValue(1), amount);
            if (string.Join(",", result.CastedValues.Cast<NumberValue>().Select(value => value.Value)) != expected)
                return $"Collection removal failed for amount {amount}.";
        }
        if (collection.CastedValues.Length != 3
            || CollectionValue.Remove(collection, new NumberValue(9)).CastedValues.Length != 3)
            return "Collection removal changed its source or removed a nonmatching value.";
        try
        {
            CollectionValue.Remove(collection, new StaticTextValue("1"));
            return "Collection removal accepted a mismatched type.";
        }
        catch (CustomScriptRuntimeError) { }

        // These calls fail before accessing the catalog or starting a script.
        var command = new CustomCommandFlag.CustomCommand
        {
            Command = "audit_regression",
            Usage = ["value"],
            GlobalCooldown = TimeSpan.FromMinutes(1),
            GlobalMaxUses = 1
        };
        CustomCommandFlag.ScriptCommands.Add(command, new CustomCommandFlag { Command = command });
        try
        {
            foreach (var args in new[] { Array.Empty<string>(), new[] { "1", "2" }, new[] { "\"unterminated" } })
            {
                if (!CustomCommandFlag.RunAttachedScript(command, ServerConsoleExecutor.Instance, args).HasErrored()
                    || command.NextEligibleDateForGlobal is not null || command.GlobalUses != 0)
                    return "A rejected custom command consumed usage or started its cooldown.";
            }
        }
        finally
        {
            CustomCommandFlag.ScriptCommands.Remove(command);
        }

        foreach (var kind in new[] { "repeat", "while", "forever", "over" })
        foreach (var disposeEarly in new[] { false, true })
        {
            var script = Script.CreateAnonymous("loop_cleanup_regression", "");
            script.AddLocalVariable(new CollectionVariable("auditItems", collection));
            LoopContext loop = kind switch
            {
                "repeat" => new RepeatLoop { Script = script, LineNum = null },
                "while" => new WhileLoop { Script = script, LineNum = null },
                "forever" => new ForeverLoop { Script = script, LineNum = null },
                _ => new OverLoop { Script = script, LineNum = null }
            };
            BaseToken Token(string text) => Tokenizer.GetTokenFromString(text, script, null).Value
                ?? throw new InvalidOperationException($"Cannot tokenize regression input {text}.");
            if (kind != "forever")
                loop.TryAddToken(Token(kind == "repeat" ? "1" : kind == "while" ? "true" : "&auditItems"));
            var bindings = kind == "over" ? new[] { "$auditItem", "$auditIndex" } : new[] { "$auditIndex" };
            if (((IAcceptOptionalVariableDefinitionsContext)loop)
                .SetOptionalVariables(bindings.Select(name => (VariableToken)Token(name)).ToArray()).HasErrored(out var error)
                || loop.VerifyCurrentState().HasErrored(out error))
                return error;
            loop.Children.Add(new FailingBody { Script = script, LineNum = null });
            using (var run = loop.Run())
            {
                if (!run.MoveNext() || !script.LocalVariables.Any(variable => variable.Name == "auditIndex"))
                    return $"The {kind} regression did not enter its body.";
                if (!disposeEarly)
                {
                    try
                    {
                        run.MoveNext();
                        return $"The {kind} regression did not raise its expected error.";
                    }
                    catch (CustomScriptRuntimeError) { }
                }
            }
            if (script.LocalVariables.Any(variable => variable.Name is "auditIndex" or "auditItem"))
                return $"The {kind} loop left its bindings behind after {(disposeEarly ? "disposal" : "an error")}.";
        }
        return null;
    }

    private sealed class FailingBody : YieldingContext
    {
        public override string FriendlyName => "regression body";
        public override TryAddTokenRes TryAddToken(BaseToken token) => TryAddTokenRes.End();
        public override Result VerifyCurrentState() => true;
        protected override IEnumerator<float> Execute()
        {
            yield return 0;
            throw new CustomScriptRuntimeError("Expected regression error.");
        }
    }
}
