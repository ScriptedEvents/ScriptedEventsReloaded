(function createSerLanguageCore(root, factory) {
  const api = factory();
  if (typeof module === "object" && module.exports) module.exports = api;
  if (root) root.SERLanguageCore = api;
})(typeof globalThis !== "undefined" ? globalThis : this, function serLanguageCoreFactory() {
  "use strict";

  const VARIABLE_TYPE_NAMES = Object.freeze({
    "@": "player variable",
    "$": "literal variable",
    "&": "collection variable",
    "*": "reference variable"
  });

  const BEGINNER_METHOD_FALLBACK = Object.freeze([
    "Broadcast",
    "Hint",
    "Heal",
    "Damage",
    "Kill",
    "GiveItem",
    "ClearInventory",
    "SetRole",
    "TPPlayer",
    "StartRound",
    "EndRound",
    "RestartRound",
    "Warhead",
    "RunScript",
    "CleanupPickups",
    "CleanupRagdolls"
  ]);

  function hasOwn(object, key) {
    return Boolean(object) && Object.prototype.hasOwnProperty.call(object, key);
  }

  function asArray(value) {
    if (Array.isArray(value)) return value;
    if (value == null) return [];
    return [...value];
  }

  function inferArgumentKind(argument) {
    if (argument.argumentKind) return argument.argumentKind;
    if (asArray(argument.options).length > 0) return "OptionsArgument";
    if (asArray(argument.enumValues).length > 0) return "EnumArgument`1";

    const syntax = String(argument.syntax || "");
    const description = `${argument.type || ""} ${argument.description || ""}`.toLowerCase();
    if (syntax.startsWith("@")) return "PlayersArgument";
    if (syntax.startsWith("&")) return "CollectionArgument";
    if (syntax.startsWith("*")) return "ReferenceArgument`1";
    if (syntax.startsWith("\"")) return "TextArgument";
    if (description.includes("duration")) return "DurationArgument";
    if (description.includes("true") && description.includes("false")) return "BoolArgument";
    if (description.includes("color") || description.includes("colour")) return "ColorArgument";
    if (description.includes("integer") || description.includes("whole number")) return "IntArgument";
    if (description.includes("number")) return "FloatArgument";
    if (description.includes("variable")) return "VariableArgument";
    return "AnyValueArgument";
  }

  function legacyReturnType(returnType) {
    const value = String(returnType || "").toLowerCase();
    if (!value) return null;
    if (value.includes("player")) return "PlayerValue";
    if (value.includes("collection")) return "CollectionValue";
    if (value.includes("bool")) return "BoolValue";
    if (value.includes("number")) return "NumberValue";
    if (value.includes("duration")) return "DurationValue";
    if (value.includes("text")) return "TextValue";
    if (value.includes("reference")) return "ReferenceValue";
    return "UnknownValue";
  }

  function methodEntries(manifest) {
    return Object.entries(manifest?.methods || {}).map(([name, method]) => ({
      name,
      ...method
    }));
  }

  function enumArgumentValues(manifest, methodName, argumentIndex) {
    return asArray(
      manifest?.methods?.[methodName]?.arguments?.[argumentIndex]?.enumValues
    );
  }

  function eventVariableNames(manifest, eventName) {
    return asArray(manifest?.eventDetails?.[eventName]?.variables)
      .map(variable => variable.name)
      .filter(Boolean);
  }

  function eventSupportsVariable(manifest, eventName, variableName) {
    return eventVariableNames(manifest, eventName).includes(variableName);
  }

  function beginnerMethodNames(manifest) {
    const markedEssential = methodEntries(manifest)
      .filter(method => method.essential)
      .map(method => method.name);
    return markedEssential.length > 0 ? markedEssential : [...BEGINNER_METHOD_FALLBACK];
  }

  function toEditorMetadata(manifest) {
    const methods = methodEntries(manifest).map(method => ({
      Name: method.name,
      Description: [
        method.description,
        method.additionalDescription,
        method.requiredFramework ? `Requires framework: ${method.requiredFramework}` : null
      ].filter(Boolean).join("\n\n"),
      Subgroup: method.subgroup || "General",
      ReturnType: legacyReturnType(method.returns),
      IsEssential: Boolean(method.essential),
      Arguments: asArray(method.arguments).map(argument => ({
        Name: argument.name,
        Type: inferArgumentKind(argument),
        IsRequired: Boolean(argument.mustBeProvided),
        HasDefault: argument.defaultValue != null,
        DefaultString: argument.defaultValue ?? null,
        Options: asArray(argument.options).map(option => ({
          Value: option.value,
          Description: option.description || ""
        })),
        EffectTypes: null,
        EnumValues: asArray(argument.enumValues)
      }))
    }));

    const flags = Object.entries(manifest?.flags || {}).map(([name, flag]) => ({
      Name: name,
      Description: flag.description || "",
      InlineArgument: flag.inlineArgument ? {
        Name: flag.inlineArgument.name,
        Description: flag.inlineArgument.description || "",
        Example: flag.inlineArgument.example || ""
      } : null,
      Arguments: asArray(flag.arguments).map(argument => ({
        Name: argument.name,
        Description: argument.description || "",
        Example: argument.example || ""
      }))
    }));

    return {
      Events: asArray(manifest?.events),
      Methods: methods,
      Variables: asArray(manifest?.variables).map(variable => ({
        Name: variable.name,
        Prefix: variable.prefix,
        Category: variable.category || "General",
        Description: `${variable.fullName || `${variable.prefix}${variable.name}`}\n\n${variable.type || "SER variable"}`
      })),
      Flags: flags,
      Keywords: manifest?.keywords || {},
      EventDetails: manifest?.eventDetails || {},
      BeginnerMethods: beginnerMethodNames(manifest)
    };
  }

  function searchCatalog(manifest, query, options = {}) {
    const normalizedQuery = String(query || "").trim().toLowerCase();
    const beginnerOnly = Boolean(options.beginnerOnly);
    const beginnerNames = new Set(beginnerMethodNames(manifest));

    return methodEntries(manifest)
      .filter(method => !beginnerOnly || beginnerNames.has(method.name))
      .filter(method => {
        if (!normalizedQuery) return true;
        return [
          method.name,
          method.subgroup,
          method.description,
          method.additionalDescription
        ].some(value => String(value || "").toLowerCase().includes(normalizedQuery));
      })
      .sort((left, right) => {
        const leftStarts = left.name.toLowerCase().startsWith(normalizedQuery) ? 0 : 1;
        const rightStarts = right.name.toLowerCase().startsWith(normalizedQuery) ? 0 : 1;
        return leftStarts - rightStarts || left.name.localeCompare(right.name);
      });
  }

  function escapeSerText(value) {
    return String(value ?? "")
      .replace(/~/g, "~~")
      .replace(/"/g, "~\"");
  }

  function withoutQuotedText(line) {
    let result = "";
    let quoted = false;
    let escaped = false;
    for (const character of String(line || "")) {
      if (escaped) {
        escaped = false;
        if (!quoted) result += character;
        continue;
      }
      if (character === "~") {
        escaped = true;
        if (!quoted) result += " ";
        continue;
      }
      if (character === "\"") {
        quoted = !quoted;
        result += " ";
        continue;
      }
      result += quoted ? " " : character;
    }
    return result;
  }

  function validateGeneratedCode(code) {
    const source = String(code || "");
    const diagnostics = [];
    const lines = source.split(/\r?\n/);

    if (!source.trim() || source.trimStart().startsWith("# Drag blocks")) {
      diagnostics.push({
        severity: "info",
        code: "empty-workspace",
        message: "Add a starter block to generate a script."
      });
      return diagnostics;
    }

    lines.forEach((line, index) => {
      const lineNumber = index + 1;
      const codeOnly = withoutQuotedText(line);
      if (/(^|\s)\.\.\.(?=\s|$)/.test(codeOnly)) {
        diagnostics.push({
          severity: "error",
          code: "missing-value",
          line: lineNumber,
          message: "A required block input is still empty."
        });
      }
      if (/^\s*!--\s+(CustomCommand|OnCustomTrigger|OnCRole|OnPMER)\s*$/.test(line)) {
        diagnostics.push({
          severity: "error",
          code: "missing-flag-argument",
          line: lineNumber,
          message: "This entry point needs a name or event."
        });
      }
      if (/^\s*run\s+\S+\s+with(?:\s|$)/.test(line)) {
        diagnostics.push({
          severity: "error",
          code: "invalid-function-call",
          line: lineNumber,
          message: "Function calls take arguments directly; remove the 'with' keyword."
        });
      }
    });

    const statementStack = [];
    lines.forEach((line, index) => {
      const keyword = line.trim().split(/\s+/)[0];
      if (["if", "repeat", "over", "while", "forever", "func", "attempt", "chance"].includes(keyword)) {
        statementStack.push({ keyword, line: index + 1, containsWait: false });
      } else if (keyword === "wait" || keyword === "wait_until") {
        for (const statement of statementStack) statement.containsWait = true;
      } else if (keyword === "end") {
        const statement = statementStack.pop();
        if (statement?.keyword === "forever" && !statement.containsWait) {
          diagnostics.push({
            severity: "warning",
            code: "forever-without-wait",
            line: statement.line,
            message: "A forever loop should contain wait or wait_until."
          });
        }
      }
    });

    for (const statement of statementStack) {
      diagnostics.push({
        severity: "error",
        code: "unclosed-statement",
        line: statement.line,
        message: `The '${statement.keyword}' block is missing its end.`
      });
    }

    return diagnostics;
  }

  return Object.freeze({
    VARIABLE_TYPE_NAMES,
    BEGINNER_METHOD_FALLBACK,
    hasOwn,
    inferArgumentKind,
    legacyReturnType,
    methodEntries,
    enumArgumentValues,
    eventVariableNames,
    eventSupportsVariable,
    beginnerMethodNames,
    toEditorMetadata,
    toLegacyEditorMetadata: toEditorMetadata,
    searchCatalog,
    escapeSerText,
    withoutQuotedText,
    validateGeneratedCode
  });
});
