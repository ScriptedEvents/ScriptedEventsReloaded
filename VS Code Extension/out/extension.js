const vscode = require('vscode');
const { SER_TRUTH_TABLE } = require('./ser_method_info.js');

/**
 * Escapes text for use inside markdown inline code spans (single backticks).
 * Only escapes backticks and collapses newlines to spaces.
 */
function escapeInlineCode(text) {
    if (!text) return text;
    return text
        .replace(/`/g, '\\`')
        .replace(/\r\n/g, ' ')
        .replace(/\n/g, ' ');
}

/**
 * Escapes text for use inside markdown fenced code blocks (triple backticks).
 * Only escapes backticks; newlines are preserved.
 */
function escapeCodeBlock(text) {
    if (!text) return text;
    return text.replace(/`/g, '\\`');
}

/**
 * Escapes markdown characters so truth table text renders as plain text.
 * For text OUTSIDE of code spans/blocks. Converts newlines to <br> tags
 * and escapes blockquote-inducing > at line starts.
 */
function escapeMarkdown(text) {
    return text;
    if (!text) return text;
    return text
        .replace(/\\/g, '\\\\')
        .replace(/\*/g, '\\*')
        .replace(/\[/g, '\\[')
        .replace(/\]/g, '\\]')
        .replace(/`/g, '\\`')
        .replace(/#/g, '\\#')
        .replace(/\r\n/g, '\n')
        .replace(/^\u003e/gm, '\\\u003e');
        //.replace(/\n/g, '<br>');
}

const VARIABLE_TYPE_NAMES = {
    '@': 'player variable',
    '$': 'literal variable',
    '&': 'collection variable',
    '*': 'reference variable'
};

function hasOwn(object, key) {
    return Boolean(object) && Object.prototype.hasOwnProperty.call(object, key);
}

function createMarkdownHover(content, range) {
    const markdown = new vscode.MarkdownString(content);
    markdown.supportHtml = true;
    markdown.isTrusted = true;
    return new vscode.Hover(markdown, range);
}

function methodArgumentMarkdown(argument) {
    let md = `\`\`\`ser\n${escapeCodeBlock(argument.syntax)}\n\`\`\`\n\n`;
    if (argument.description) {
        md += `${escapeMarkdown(argument.description)}\n\n`;
    }
    md += `**Type:** \`${escapeInlineCode(argument.type || 'SER value')}\`\n\n`;

    if (argument.enumValues?.length > 0) {
        if (argument.enumDescription) {
            md += `${escapeMarkdown(argument.enumDescription)}\n\n`;
        }
        md += `**Available values:**\n\n`;
        if (argument.enumDescriptions && Object.keys(argument.enumDescriptions).length > 0) {
            for (const value of argument.enumValues) {
                md += `- \`${escapeInlineCode(value)}\``;
                if (argument.enumDescriptions[value]) {
                    md += ` - ${escapeMarkdown(argument.enumDescriptions[value])}`;
                }
                md += '\n';
            }
            md += '\n';
        } else {
            md += `${argument.enumValues.map(value => `\`${escapeInlineCode(value)}\``).join(', ')}\n\n`;
        }
        if (argument.isEnumFlags) {
            md += `Multiple values can be joined with \`|\`.\n\n`;
        }
    }

    if (argument.options?.length > 0) {
        md += `**Available values:**\n\n`;
        for (const option of argument.options) {
            md += `- \`${escapeInlineCode(option.value)}\``;
            if (option.description) md += ` - ${escapeMarkdown(option.description)}`;
            md += `\n`;
        }
        md += `\n`;
    }

    if (argument.defaultValue) {
        md += `**Default:** \`${escapeInlineCode(argument.defaultValue)}\`\n\n`;
    }
    if (argument.consumesRemainingValues) {
        md += `**Note:** consumes the remaining values on the line.\n`;
    }

    return md;
}

function methodArgumentDocumentation(argument) {
    const markdown = new vscode.MarkdownString(methodArgumentMarkdown(argument));
    markdown.supportHtml = true;
    markdown.isTrusted = true;
    return markdown;
}

function argumentHasImmediateCompletions(argument) {
    if (!argument) return false;
    if (argument.enumValues?.length > 0 || argument.options?.length > 0) return true;
    return hasOwn(VARIABLE_TYPE_NAMES, argument.syntax?.[0]);
}

function isManualSignatureInvocation(context) {
    return context?.triggerKind === vscode.SignatureHelpTriggerKind?.Invoke;
}

function effectiveMethodArgumentIndex(method, activeArgument) {
    const argumentsList = method.arguments || [];
    if (argumentsList.length === 0) return -1;
    if (activeArgument < 0) return 0;
    if (activeArgument < argumentsList.length) return activeArgument;
    return argumentsList[argumentsList.length - 1].consumesRemainingValues
        ? argumentsList.length - 1
        : -1;
}

function createSignaturePresentation(label, parameterRanges, activeParameter, description = '') {
    const activeRange = parameterRanges[activeParameter];
    if (!activeRange) return null;

    const width = Math.max(activeRange[1] - activeRange[0], 1);
    const guide = `${' '.repeat(activeRange[0])}${'─'.repeat(width)}`;
    let content = `\`\`\`ser\n${escapeCodeBlock(label)}\n${guide}\n\`\`\``;
    if (description) content += `\n\n${escapeMarkdown(description)}`;

    const documentation = new vscode.MarkdownString(content);
    documentation.supportHtml = true;
    documentation.isTrusted = true;

    return {
        // VS Code renders newlines in SignatureInformation.label inline. Keep the
        // native active-parameter highlight on an invisible character instead.
        label: '\u200B',
        parameterRanges: parameterRanges.map(() => [0, 1]),
        documentation
    };
}

function findQuotedStringAtPosition(lineText, character) {
    let activeQuote = null;
    let quoteAtPosition = null;
    let escaped = false;
    const interpolations = [];

    for (let index = 0; index < lineText.length; index++) {
        if (index === character) quoteAtPosition = activeQuote;
        const current = lineText[index];

        if (activeQuote) {
            if (escaped) {
                escaped = false;
                continue;
            }
            if (current === '\\') {
                escaped = true;
                continue;
            }
            if (current === '"') {
                activeQuote.end = index;
                activeQuote = null;
                continue;
            }
            if (current === '{') {
                interpolations.push({ quote: activeQuote, depth: 1 });
                activeQuote = null;
            }
            continue;
        }

        if (interpolations.length > 0) {
            const interpolation = interpolations[interpolations.length - 1];
            if (current === '"') {
                activeQuote = { start: index, end: null };
                escaped = false;
            } else if (current === '{') {
                interpolation.depth++;
            } else if (current === '}') {
                interpolation.depth--;
                if (interpolation.depth === 0) {
                    interpolations.pop();
                    activeQuote = interpolation.quote;
                }
            }
            continue;
        }

        if (current === '"') {
            activeQuote = { start: index, end: null };
            escaped = false;
        }
    }

    if (character === lineText.length) quoteAtPosition = activeQuote;
    return quoteAtPosition?.end == null ? null : quoteAtPosition;
}

function findDocumentFlagName(document, fromLine = document.lineCount - 1) {
    for (let line = fromLine; line >= 0; line--) {
        const match = document.lineAt(line).text.match(/^\s*!--\s+([A-Za-z][A-Za-z0-9]*)\b/);
        if (match) return match[1];
    }
    return null;
}

function renderKeywordHover(keyword, range) {
    let md = `\`\`\`ser\n${escapeCodeBlock(keyword.syntax)}\n\`\`\`\n\n`;
    md += `${escapeMarkdown(keyword.description)}\n\n`;

    if (keyword.isStatement) {
        md += `**Statement:** requires an \`end\` keyword.\n\n`;
    }
    if (keyword.extendsSignal) {
        md += `**Extends signal:** \`${escapeInlineCode(keyword.extendsSignal)}\`\n\n`;
    }
    if (keyword.allowedSignals) {
        md += `**Allows extensions for:** \`${escapeInlineCode(keyword.allowedSignals)}\`\n\n`;
    }

    return createMarkdownHover(md, range);
}

function renderFlagArgumentHover(flagName, argument, range, isInline = false) {
    let md = `**${isInline ? 'Inline flag argument' : 'Flag argument'}:** \`${escapeInlineCode(argument.name)}\`\n\n`;
    md += `**Flag:** \`${escapeInlineCode(flagName)}\`  \n`;
    md += `**Required:** ${argument.required ? 'yes' : 'no'}\n\n`;
    md += `${escapeMarkdown(argument.description)}\n\n`;

    if (argument.example) {
        md += `\`\`\`ser\n${escapeCodeBlock(argument.example)}\n\`\`\``;
    }

    return createMarkdownHover(md, range);
}

function renderFlagHover(flagName, flag, range) {
    let md = `\`\`\`ser\n${escapeCodeBlock(flag.syntax)}\n\`\`\`\n\n`;
    md += `${escapeMarkdown(flag.description)}\n\n`;

    if (flag.inlineArgument) {
        md += `**Inline argument:** \`${escapeInlineCode(flag.inlineArgument.name)}\` `;
        md += `(${flag.inlineArgument.required ? 'required' : 'optional'})\n\n`;
    }

    if (flag.arguments && flag.arguments.length > 0) {
        md += `***\n\n**Flag arguments:**\n`;
        for (const argument of flag.arguments) {
            md += `- \`-- ${escapeInlineCode(argument.name)}\` (${argument.required ? 'required' : 'optional'})\n`;
        }
    }

    return createMarkdownHover(md, range);
}

function renderVariableHover(variableName, variable, range) {
    const prefix = variableName[0];
    const type = variable?.type || VARIABLE_TYPE_NAMES[prefix] || 'SER variable';
    let md = `\`\`\`ser\n${escapeCodeBlock(variableName)}\n\`\`\`\n\n`;
    md += `**Type:** ${escapeMarkdown(type)}\n\n`;
    if (variable?.category) {
        md += `**Category:** ${escapeMarkdown(variable.category)}\n`;
    }
    return createMarkdownHover(md, range);
}

function completionRange(position, typedText) {
    return new vscode.Range(
        new vscode.Position(position.line, position.character - typedText.length),
        position
    );
}

function completionDocumentation(description, example = null) {
    let content = escapeMarkdown(description || '');
    if (example) {
        content += `\n\n\`\`\`ser\n${escapeCodeBlock(example)}\n\`\`\``;
    }
    const markdown = new vscode.MarkdownString(content);
    markdown.supportHtml = true;
    return markdown;
}

function methodCompletionDocumentation(method) {
    let content = `\`\`\`ser\n${escapeCodeBlock(method.syntax)}\n\`\`\`\n\n`;
    content += escapeMarkdown(method.description || '');
    if (method.returns) {
        content += `\n\n**Returns:** ${escapeMarkdown(method.returns)}`;
    }
    const markdown = new vscode.MarkdownString(content);
    markdown.supportHtml = true;
    return markdown;
}

let workspaceGlobals = [];
let workspaceGlobalsDirty = true;
let workspaceGlobalsRefresh = null;

function parseGlobalVariables(text, source) {
    const variables = [];
    const definitionRegex = /^\s*global\s+([@$&*])([a-z_][a-zA-Z0-9_]*)\s*=/gm;
    let match;
    while ((match = definitionRegex.exec(text)) !== null) {
        variables.push({
            name: match[2],
            prefix: match[1],
            fullName: `${match[1]}${match[2]}`,
            type: VARIABLE_TYPE_NAMES[match[1]],
            category: 'Global',
            source: 'workspace',
            definedIn: source
        });
    }
    return variables;
}

async function refreshWorkspaceGlobals() {
    if (!vscode.workspace?.findFiles) {
        workspaceGlobals = [];
        workspaceGlobalsDirty = false;
        return workspaceGlobals;
    }

    const found = new Map();
    const openDocuments = new Map(
        (vscode.workspace.textDocuments || [])
            .filter(document => document.languageId === 'ser' && document.uri)
            .map(document => [document.uri.toString(), document])
    );

    for (const document of openDocuments.values()) {
        const source = vscode.workspace.asRelativePath?.(document.uri, false) ||
            document.uri.fsPath || document.uri.toString();
        for (const variable of parseGlobalVariables(document.getText(), source)) {
            if (!found.has(variable.fullName)) found.set(variable.fullName, variable);
        }
    }

    const uris = await vscode.workspace.findFiles('**/*.ser', '**/{bin,obj,node_modules}/**');

    for (const uri of uris) {
        try {
            const openDocument = openDocuments.get(uri.toString());
            const text = openDocument
                ? openDocument.getText()
                : new TextDecoder('utf-8').decode(await vscode.workspace.fs.readFile(uri));
            const source = vscode.workspace.asRelativePath?.(uri, false) || uri.fsPath || uri.toString();
            for (const variable of parseGlobalVariables(text, source)) {
                if (!found.has(variable.fullName)) found.set(variable.fullName, variable);
            }
        } catch {
            // A file can disappear between findFiles and readFile; keep indexing the rest.
        }
    }

    workspaceGlobals = [...found.values()];
    workspaceGlobalsDirty = false;
    return workspaceGlobals;
}

async function getWorkspaceGlobals() {
    if (!workspaceGlobalsDirty) return workspaceGlobals;
    if (!workspaceGlobalsRefresh) {
        workspaceGlobalsRefresh = refreshWorkspaceGlobals().finally(() => {
            workspaceGlobalsRefresh = null;
        });
    }
    return workspaceGlobalsRefresh;
}

function invalidateWorkspaceGlobals() {
    workspaceGlobalsDirty = true;
}

function collectVariables(document, prefix, indexedGlobals = []) {
    const variables = new Map();

    for (const variable of SER_TRUTH_TABLE.variables || []) {
        if (variable.prefix === prefix) {
            variables.set(variable.fullName, { ...variable, source: 'built-in' });
        }
    }

    for (const variable of indexedGlobals) {
        if (variable.prefix === prefix && !variables.has(variable.fullName)) {
            variables.set(variable.fullName, variable);
        }
    }

    const variableRegex = /([@$&*])([a-z_][a-zA-Z0-9_]*)/g;
    const documentText = document.getText();
    let match;
    while ((match = variableRegex.exec(documentText)) !== null) {
        if (match[1] !== prefix || variables.has(match[0])) continue;
        variables.set(match[0], {
            name: match[2],
            prefix,
            fullName: match[0],
            type: VARIABLE_TYPE_NAMES[prefix],
            category: null,
            source: 'document'
        });
    }

    return [...variables.values()];
}

async function variableCompletions(document, position, prefix, typedName, includePrefixInInsert = false) {
    const range = completionRange(position, typedName);
    const indexedGlobals = await getWorkspaceGlobals();
    return collectVariables(document, prefix, indexedGlobals).map(variable => {
        const item = new vscode.CompletionItem(variable.fullName, vscode.CompletionItemKind.Variable);
        item.insertText = includePrefixInInsert ? variable.fullName : variable.name;
        item.filterText = variable.name;
        item.range = range;
        item.detail = `${variable.type}${variable.category ? ` · ${variable.category}` : ''}`;
        item.documentation = new vscode.MarkdownString(
            `**${variable.source === 'document' ? 'Document' : variable.source === 'workspace' ? 'Workspace global' : 'Built-in'} ${escapeMarkdown(variable.type)}**` +
            (variable.category ? `\n\nCategory: ${escapeMarkdown(variable.category)}` : '') +
            (variable.definedIn ? `\n\nDefined in: \`${escapeInlineCode(variable.definedIn)}\`` : '')
        );
        const sourceRank = variable.source === 'document' ? '0' : variable.source === 'workspace' ? '1' : '2';
        item.sortText = `${sourceRank}_${variable.name}`;
        return item;
    });
}

function codeCompletions(position, typedName, returningOnly = false) {
    const range = completionRange(position, typedName);
    const items = [];

    for (const [name, method] of Object.entries(SER_TRUTH_TABLE.methods || {})) {
        if (returningOnly && !method.returns) continue;
        const item = new vscode.CompletionItem(name, vscode.CompletionItemKind.Method);
        item.insertText = name;
        item.filterText = name;
        item.range = range;
        item.detail = method.returns ? `SER method · returns ${method.returns}` : 'SER method';
        item.documentation = methodCompletionDocumentation(method);
        item.sortText = `0_${name}`;
        items.push(item);
    }

    if (!returningOnly) {
        for (const [name, keyword] of Object.entries(SER_TRUTH_TABLE.keywords || {})) {
            const item = new vscode.CompletionItem(name, vscode.CompletionItemKind.Keyword);
            item.insertText = name;
            item.filterText = name;
            item.range = range;
            item.detail = keyword.syntax;
            item.documentation = completionDocumentation(keyword.description, keyword.example);
            item.sortText = `1_${name}`;
            items.push(item);
        }
    }

    return items;
}

function parseFunctionArguments(text) {
    return [...text.matchAll(/[@$&*][a-z_][a-zA-Z0-9_]*/g)].map(match => match[0]);
}

function collectDocumentFunctions(document, beforeLine = Number.POSITIVE_INFINITY) {
    const lines = document.getText().split(/\r?\n/);
    const functions = new Map();
    const lastLine = Math.min(lines.length, beforeLine);

    for (let lineIndex = 0; lineIndex < lastLine; lineIndex++) {
        const declaration = lines[lineIndex].match(
            /^\s*func\s+([@$&*]?[A-Za-z_][A-Za-z0-9_]*)(?:\s+with\s+(.+?))?\s*(?:#.*)?$/i
        );
        if (!declaration) continue;

        let argumentsList = parseFunctionArguments(declaration[2] || '');
        if (argumentsList.length === 0) {
            for (let nextLine = lineIndex + 1; nextLine < lastLine; nextLine++) {
                const trimmed = lines[nextLine].trim();
                if (!trimmed || trimmed.startsWith('#')) continue;
                const withLine = trimmed.match(/^with\s+(.+?)(?:\s+#.*)?$/i);
                if (withLine) argumentsList = parseFunctionArguments(withLine[1]);
                break;
            }
        }

        const name = declaration[1];
        functions.set(name.toLowerCase(), {
            name,
            arguments: argumentsList,
            line: lineIndex
        });
    }

    return [...functions.values()];
}

function findDocumentFunction(document, name, beforeLine = Number.POSITIVE_INFINITY) {
    return collectDocumentFunctions(document, beforeLine)
        .find(func => func.name.toLowerCase() === name.toLowerCase());
}

function functionSyntax(func, prefix = 'run') {
    return `${prefix} ${func.name}${func.arguments.length > 0 ? ` ${func.arguments.join(' ')}` : ''}`;
}

function renderFunctionHover(func, range) {
    let md = `\`\`\`ser\n${escapeCodeBlock(functionSyntax(func))}\n\`\`\`\n\n`;
    md += `Function defined on line ${func.line + 1}.\n\n`;
    if (func.arguments.length > 0) {
        md += `**Arguments:** ${func.arguments.map(argument => `\`${escapeInlineCode(argument)}\``).join(', ')}\n`;
    } else {
        md += `This function takes no arguments.\n`;
    }
    return createMarkdownHover(md, range);
}

function functionCompletions(document, position, typedName) {
    const range = completionRange(position, typedName);
    return collectDocumentFunctions(document, position.line).map(func => {
        const item = new vscode.CompletionItem(func.name, vscode.CompletionItemKind.Function);
        if (func.arguments.length > 0) {
            const snippet = new vscode.SnippetString();
            snippet.appendText(func.name);
            for (const argument of func.arguments) {
                snippet.appendText(' ');
                snippet.appendPlaceholder(argument);
            }
            item.insertText = snippet;
        } else {
            item.insertText = func.name;
        }
        item.filterText = func.name;
        item.range = range;
        item.detail = `SER function${func.arguments.length > 0 ? ` · ${func.arguments.length} argument(s)` : ''}`;
        item.documentation = completionDocumentation(
            `Function defined on line ${func.line + 1}.`,
            functionSyntax(func)
        );
        item.sortText = `0_${func.name}`;
        return item;
    });
}

function flagCompletions(position, typedName) {
    const range = completionRange(position, typedName);
    return Object.entries(SER_TRUTH_TABLE.flags || {}).map(([name, flag]) => {
        const item = new vscode.CompletionItem(name, vscode.CompletionItemKind.Event);
        item.insertText = name;
        item.filterText = name;
        item.range = range;
        item.detail = flag.syntax;
        item.documentation = completionDocumentation(flag.description);
        return item;
    });
}

function flagArgumentCompletions(document, position, typedName) {
    const flagName = findDocumentFlagName(document, position.line);
    const flag = flagName ? SER_TRUTH_TABLE.flags?.[flagName] : null;
    if (!flag) return [];

    const range = completionRange(position, typedName);
    return (flag.arguments || []).map(argument => {
        const item = new vscode.CompletionItem(argument.name, vscode.CompletionItemKind.Property);
        item.insertText = argument.name;
        item.filterText = argument.name;
        item.range = range;
        item.detail = `${argument.required ? 'required' : 'optional'} flag argument`;
        item.documentation = completionDocumentation(argument.description, argument.example || `-- ${argument.name}`);
        return item;
    });
}

function eventCompletions(position, typedName) {
    const range = completionRange(position, typedName);
    return (SER_TRUTH_TABLE.events || []).map(eventName => {
        const details = SER_TRUTH_TABLE.eventDetails?.[eventName];
        const item = new vscode.CompletionItem(eventName, vscode.CompletionItemKind.Event);
        item.insertText = eventName;
        item.filterText = eventName;
        item.range = range;
        item.detail = details?.group ? `SER event · ${details.group}` : 'SER event';
        if (details) {
            let documentation = details.description ? `${escapeMarkdown(details.description)}\n\n` : '';
            if (details.eventDataType) {
                documentation += `**Event data:** \`${escapeInlineCode(details.eventDataType)}\`\n\n`;
                if (details.eventDataDescription) {
                    documentation += `${escapeMarkdown(details.eventDataDescription)}\n\n`;
                }
            }
            documentation += `**Cancellable:** ${details.isCancellable ? 'yes' : 'no'}\n\n`;
            if (details.variables?.length > 0) {
                documentation += '**Event variables:**\n\n';
                for (const variable of details.variables) {
                    documentation += `- \`${escapeInlineCode(variable.name)}\` (\`${escapeInlineCode(variable.type)}\`)`;
                    if (variable.description) documentation += ` - ${escapeMarkdown(variable.description)}`;
                    documentation += '\n';
                }
            }
            item.documentation = new vscode.MarkdownString(documentation);
        }
        return item;
    });
}

function tokenizeSerExpression(expression) {
    const tokens = [];
    let index = 0;

    while (index < expression.length) {
        if (/\s/.test(expression[index])) {
            index++;
            continue;
        }

        const start = index;
        const opening = expression[index];
        if (opening === '"') {
            const quotedString = findQuotedStringAtPosition(expression, start + 1);
            if (quotedString?.start === start) {
                index = quotedString.end + 1;
            } else {
                index++;
                while (index < expression.length) {
                    if (expression[index] === '\\') {
                        index += 2;
                        continue;
                    }
                    if (expression[index++] === '"') break;
                }
            }
        } else if (opening === '{' || opening === '(') {
            const closing = opening === '{' ? '}' : ')';
            let depth = 1;
            index++;
            while (index < expression.length && depth > 0) {
                if (expression[index] === opening) depth++;
                else if (expression[index] === closing) depth--;
                index++;
            }
        } else {
            while (index < expression.length && !/\s/.test(expression[index])) index++;
        }

        tokens.push({ start, end: index, text: expression.substring(start, index) });
    }

    return tokens;
}

function getMethodCallContext(lineText, cursorCharacter) {
    const bracketStack = [];
    let inString = false;
    let escaped = false;

    for (let index = 0; index < cursorCharacter; index++) {
        const character = lineText[index];
        if (inString) {
            if (character === '\\' && !escaped) {
                escaped = true;
                continue;
            }
            if (character === '"' && !escaped) inString = false;
            escaped = false;
            continue;
        }
        if (character === '"') {
            inString = true;
        } else if (character === '{' || character === '(') {
            bracketStack.push({ character, index });
        } else if (character === '}' || character === ')') {
            bracketStack.pop();
        }
    }

    const expressionStart = bracketStack.length > 0
        ? bracketStack[bracketStack.length - 1].index + 1
        : 0;
    let expression = lineText.substring(expressionStart);
    let relativeCursor = cursorCharacter - expressionStart;

    const assignmentPrefix = expression.match(
        /^\s*(?:(?:global|ephm)\s+)?[$@&*][a-z_][a-zA-Z0-9_]*\s*=\s*/
    );
    const returnPrefix = expression.match(/^\s*return\s+/i);
    const prefix = assignmentPrefix || returnPrefix;
    if (prefix) {
        expression = expression.substring(prefix[0].length);
        relativeCursor -= prefix[0].length;
    }

    const tokens = tokenizeSerExpression(expression);
    if (tokens.length === 0) return null;

    const methodName = tokens[0].text;
    const method = SER_TRUTH_TABLE.methods?.[methodName];
    if (!method) return null;

    let activeArgument = -1;
    if (relativeCursor > tokens[0].end) {
        activeArgument = tokens.length - 1;
        for (let index = 1; index < tokens.length; index++) {
            if (relativeCursor >= tokens[index].start && relativeCursor <= tokens[index].end) {
                activeArgument = index - 1;
                break;
            }
        }
    }

    return { methodName, method, activeArgument };
}

function provideSignatureHelp(document, position, token, signatureContext) {
    const lineText = document.lineAt(position.line).text;
    const context = getMethodCallContext(lineText, position.character);
    if (!context) {
        const linePrefix = lineText.substring(0, position.character);
        const runCall = linePrefix.match(
            /^\s*(?:(?:(?:(?:global|ephm)\s+)?[@$&*][a-z_][a-zA-Z0-9_]*\s*=\s*)|(?:return\s+))?run\s+([@$&*]?[A-Za-z_][A-Za-z0-9_]*)(?:\s+(.*))?$/i
        );
        if (!runCall) return null;

        const func = findDocumentFunction(document, runCall[1], position.line);
        if (!func || func.arguments.length === 0) return null;

        const argumentText = runCall[2] || '';
        const suppliedArguments = tokenizeSerExpression(argumentText).length;
        const requestedParameter = Math.max(
            suppliedArguments - (runCall[2]?.endsWith(' ') ? 0 : 1),
            0
        );
        if (requestedParameter >= func.arguments.length && !isManualSignatureInvocation(signatureContext)) {
            return null;
        }
        const activeParameter = Math.min(requestedParameter, func.arguments.length - 1);
        const expectedPrefix = func.arguments[activeParameter]?.[0];
        if (!isManualSignatureInvocation(signatureContext) && hasOwn(VARIABLE_TYPE_NAMES, expectedPrefix)) {
            return null;
        }

        let functionLabel = `run ${func.name}`;
        const functionParameterRanges = [];
        for (const argument of func.arguments) {
            functionLabel += ' ';
            const start = functionLabel.length;
            functionLabel += argument;
            functionParameterRanges.push([start, functionLabel.length]);
        }
        const functionPresentation = createSignaturePresentation(
            functionLabel,
            functionParameterRanges,
            activeParameter,
            `Function defined on line ${func.line + 1}.`
        );
        const signature = new vscode.SignatureInformation(
            functionPresentation.label,
            functionPresentation.documentation
        );
        signature.parameters = func.arguments.map((argument, index) => new vscode.ParameterInformation(
            functionPresentation.parameterRanges[index],
            VARIABLE_TYPE_NAMES[argument[0]] || 'SER function argument'
        ));

        const help = new vscode.SignatureHelp();
        help.signatures = [signature];
        help.activeSignature = 0;
        help.activeParameter = activeParameter;
        return help;
    }
    if (!context.method.arguments || context.method.arguments.length === 0) return null;

    const activeArgumentIndex = effectiveMethodArgumentIndex(context.method, context.activeArgument);
    if (activeArgumentIndex < 0 && !isManualSignatureInvocation(signatureContext)) return null;
    const displayedArgumentIndex = activeArgumentIndex < 0
        ? context.method.arguments.length - 1
        : activeArgumentIndex;
    const activeArgument = context.method.arguments[displayedArgumentIndex];
    if (!isManualSignatureInvocation(signatureContext) && argumentHasImmediateCompletions(activeArgument)) {
        return null;
    }

    let label = context.methodName;
    const parameterRanges = [];
    for (const argument of context.method.arguments) {
        label += ' ';
        const start = label.length;
        label += argument.syntax;
        const end = label.length;
        parameterRanges.push([start, end]);
    }
    const methodPresentation = createSignaturePresentation(
        label,
        parameterRanges,
        displayedArgumentIndex,
        context.method.description || ''
    );
    const parameters = context.method.arguments.map((argument, index) =>
        new vscode.ParameterInformation(
            methodPresentation.parameterRanges[index],
            methodArgumentDocumentation(argument)
        )
    );

    const signature = new vscode.SignatureInformation(
        methodPresentation.label,
        methodPresentation.documentation
    );
    signature.parameters = parameters;

    const help = new vscode.SignatureHelp();
    help.signatures = [signature];
    help.activeSignature = 0;
    help.activeParameter = displayedArgumentIndex;
    return help;
}

function enumValueCompletions(position, typedValue, argument) {
    const range = completionRange(position, typedValue);
    return (argument.enumValues || []).map(value => {
        const item = new vscode.CompletionItem(value, vscode.CompletionItemKind.EnumMember);
        item.insertText = value;
        item.filterText = value;
        item.range = range;
        item.detail = `${argument.name} enum value`;
        const valueDescription = argument.enumDescriptions?.[value];
        item.documentation = valueDescription
            ? new vscode.MarkdownString(escapeMarkdown(valueDescription))
            : methodArgumentDocumentation(argument);
        return item;
    });
}

function optionValueCompletions(position, typedValue, argument) {
    const range = completionRange(position, typedValue);
    return (argument.options || []).map(option => {
        const item = new vscode.CompletionItem(option.value, vscode.CompletionItemKind.Value);
        item.insertText = option.value;
        item.filterText = option.value;
        item.range = range;
        item.detail = `${argument.name} option`;
        item.documentation = methodArgumentDocumentation(argument);
        return item;
    });
}

async function provideCompletions(document, position) {
    const lineText = document.lineAt(position.line).text;
    const linePrefix = lineText.substring(0, position.character);
    if (/^\s*#/.test(linePrefix)) return [];

    const runArgumentMatch = linePrefix.match(
        /^\s*(?:(?:(?:(?:global|ephm)\s+)?[@$&*][a-z_][a-zA-Z0-9_]*\s*=\s*)|(?:return\s+))?run\s+([@$&*]?[A-Za-z_][A-Za-z0-9_]*)\s+(.*)$/i
    );
    if (runArgumentMatch && /\s$/.test(linePrefix)) {
        const func = findDocumentFunction(document, runArgumentMatch[1], position.line);
        const activeArgument = tokenizeSerExpression(runArgumentMatch[2]).length;
        const expectedPrefix = func?.arguments?.[activeArgument]?.[0];
        if (expectedPrefix && hasOwn(VARIABLE_TYPE_NAMES, expectedPrefix)) {
            return variableCompletions(document, position, expectedPrefix, '', true);
        }
    }

    const runMatch = linePrefix.match(
        /^\s*(?:(?:(?:(?:global|ephm)\s+)?[@$&*][a-z_][a-zA-Z0-9_]*\s*=\s*)|(?:return\s+))?run\s+([@$&*]?[A-Za-z_][A-Za-z0-9_]*|[@$&*])?$/i
    );
    if (runMatch) {
        return functionCompletions(document, position, runMatch[1] || '');
    }

    const variableMatch = linePrefix.match(/([@$&*])([a-z_][a-zA-Z0-9_]*)?$/);
    if (variableMatch) {
        return variableCompletions(document, position, variableMatch[1], variableMatch[2] || '');
    }

    const methodContext = getMethodCallContext(lineText, position.character);
    if (methodContext?.activeArgument >= 0) {
        const argument = methodContext.method.arguments?.[methodContext.activeArgument];
        const typedValue = linePrefix.match(/([A-Za-z0-9_]*)$/)?.[1] || '';
        if (argument?.enumValues?.length > 0) {
            return enumValueCompletions(position, typedValue, argument);
        }
        if (argument?.options?.length > 0) {
            return optionValueCompletions(position, typedValue, argument);
        }
        const expectedPrefix = argument?.syntax?.[0];
        if (typedValue.length === 0 && expectedPrefix && hasOwn(VARIABLE_TYPE_NAMES, expectedPrefix)) {
            return variableCompletions(document, position, expectedPrefix, '', true);
        }
    }

    const eventMatch = linePrefix.match(/^\s*!--\s+OnEvent\s+([A-Za-z0-9_]*)$/);
    if (eventMatch) {
        return eventCompletions(position, eventMatch[1]);
    }

    const flagMatch = linePrefix.match(/^\s*!--\s*([A-Za-z0-9]*)$/);
    if (flagMatch?.[1]) {
        return flagCompletions(position, flagMatch[1]);
    }

    const flagArgumentMatch = linePrefix.match(/^\s*--\s*([A-Za-z0-9]*)$/);
    if (flagArgumentMatch?.[1]) {
        return flagArgumentCompletions(document, position, flagArgumentMatch[1]);
    }

    const returnMatch = linePrefix.match(/^\s*return\s+([A-Za-z][A-Za-z0-9_.]*)?$/);
    if (returnMatch?.[1]) {
        return codeCompletions(position, returnMatch[1], true);
    }

    const expressionMatch = linePrefix.match(/[{(]\s*([A-Za-z][A-Za-z0-9_.]*)?$/);
    if (expressionMatch?.[1]) {
        return codeCompletions(position, expressionMatch[1], true);
    }

    const codeLineMatch = linePrefix.match(
        /^\s*(?:(?:global|ephm)\s+)?(?:[@$&*][a-z_][a-zA-Z0-9_]*\s*=\s*)?([A-Za-z][A-Za-z0-9_.]*)?$/
    );
    if (codeLineMatch?.[1]) {
        const returningOnly = /[@$&*][a-z_][a-zA-Z0-9_]*\s*=\s*/.test(linePrefix);
        return codeCompletions(position, codeLineMatch[1], returningOnly);
    }

    return [];
}

function activate(context) {
    const hoverProvider = vscode.languages.registerHoverProvider('ser', {
        provideHover(document, position, token) {
            const lineText = document.lineAt(position.line).text;
            const charIdx = position.character;
            const quotedString = findQuotedStringAtPosition(lineText, charIdx);
            const wordRegex = /([@$&*][A-Za-z0-9_.]+)|([A-Za-z0-9_.]+)/g;
            let range = quotedString
                ? new vscode.Range(
                    new vscode.Position(position.line, quotedString.start),
                    new vscode.Position(position.line, quotedString.end + 1)
                )
                : document.getWordRangeAtPosition(position, wordRegex);

            // Fallback for characters that don't match wordRegex (e.g. { } ( ))
            if (!range) {
                const ch = lineText[charIdx];
                if (ch === '{' || ch === '}' || ch === '(' || ch === ')') {
                    range = new vscode.Range(
                        new vscode.Position(position.line, charIdx),
                        new vscode.Position(position.line, charIdx + 1)
                    );
                } else {
                    return null;
                }
            }

            let hoveredWord = document.getText(range);

            // If cursor is on { } or ( ), expand range to the entire balanced expression
            if (hoveredWord === '{' || hoveredWord === '}' || hoveredWord === '(' || hoveredWord === ')') {
                let braceStart = -1;
                let braceEnd = -1;
                if (hoveredWord === '{' || hoveredWord === '(') {
                    braceStart = range.start.character;
                    const openCh = hoveredWord;
                    const closeCh = hoveredWord === '{' ? '}' : ')';
                    let depth = 1;
                    for (let i = braceStart + 1; i < lineText.length; i++) {
                        if (lineText[i] === openCh) depth++;
                        else if (lineText[i] === closeCh) {
                            depth--;
                            if (depth === 0) {
                                braceEnd = i;
                                break;
                            }
                        }
                    }
                } else {
                    braceEnd = range.start.character;
                    const closeCh = hoveredWord;
                    const openCh = hoveredWord === '}' ? '{' : '(';
                    let depth = 1;
                    for (let i = braceEnd - 1; i >= 0; i--) {
                        if (lineText[i] === closeCh) depth++;
                        else if (lineText[i] === openCh) {
                            depth--;
                            if (depth === 0) {
                                braceStart = i;
                                break;
                            }
                        }
                    }
                }
                if (braceStart !== -1 && braceEnd !== -1) {
                    range = new vscode.Range(
                        new vscode.Position(position.line, braceStart),
                        new vscode.Position(position.line, braceEnd + 1)
                    );
                    hoveredWord = document.getText(range);
                }
            }

            // Flag declaration: !-- FlagName inlineArgument
            const flagDeclaration = lineText.match(/^\s*!--\s+([A-Za-z][A-Za-z0-9]*)\b/);
            if (flagDeclaration) {
                const flagName = flagDeclaration[1];
                const flag = SER_TRUTH_TABLE.flags?.[flagName];
                if (!flag) return null;

                const flagNameStart = lineText.indexOf(flagName);
                const flagNameEnd = flagNameStart + flagName.length;
                if (position.character >= flagNameStart && position.character <= flagNameEnd) {
                    return renderFlagHover(flagName, flag, range);
                }
                if (flag.inlineArgument && position.character > flagNameEnd) {
                    return renderFlagArgumentHover(flagName, flag.inlineArgument, range, true);
                }
                return null;
            }

            // Named flag argument: -- argumentName value...
            const flagArgumentLine = lineText.match(/^\s*--\s+([A-Za-z][A-Za-z0-9]*)\b/);
            if (flagArgumentLine) {
                const flagName = findDocumentFlagName(document, position.line);
                const flag = flagName ? SER_TRUTH_TABLE.flags?.[flagName] : null;
                const argument = flag?.arguments?.find(arg => arg.name === flagArgumentLine[1]);
                return argument ? renderFlagArgumentHover(flagName, argument, range) : null;
            }

            if (hasOwn(SER_TRUTH_TABLE.keywords, hoveredWord)) {
                return renderKeywordHover(SER_TRUTH_TABLE.keywords[hoveredWord], range);
            }

            const runInvocation = lineText.match(
                /^\s*(?:(?:(?:(?:global|ephm)\s+)?[@$&*][a-z_][a-zA-Z0-9_]*\s*=\s*)|(?:return\s+))?run\s+([@$&*]?[A-Za-z_][A-Za-z0-9_]*)/i
            );
            if (runInvocation && hoveredWord === runInvocation[1]) {
                const func = findDocumentFunction(document, runInvocation[1], position.line);
                if (func) return renderFunctionHover(func, range);
            }

            // The assignment target describes a variable, not the method on the right-hand side.
            const assignmentDefinition = lineText.match(
                /^\s*(?:(global|ephm)\s+)?([@$&*][a-z_][a-zA-Z0-9_]*)\s*=/
            );
            if (assignmentDefinition && hoveredWord === assignmentDefinition[2]) {
                const variable = SER_TRUTH_TABLE.variables?.find(item => item.fullName === hoveredWord) || {
                    type: VARIABLE_TYPE_NAMES[hoveredWord[0]],
                    category: assignmentDefinition[1] === 'global'
                        ? 'Global'
                        : assignmentDefinition[1] === 'ephm' ? 'Ephemeral' : 'Local'
                };
                return renderVariableHover(hoveredWord, variable, range);
            }

            // 1. Context parsing for inner bracket expressions
            // Find the nearest opening bracket ('{' or '(') at depth 0 relative to cursor
            // (handles nested braces like {ToDuration {Random 1 $x} seconds})
            let openBracketIdx = -1;
            let openBracketCh = null;
            let depth = 0;
            for (let i = charIdx; i >= 0; i--) {
                if (lineText[i] === '}' || lineText[i] === ')') {
                    depth++;
                } else if (lineText[i] === '{' || lineText[i] === '(') {
                    if (depth === 0) {
                        openBracketIdx = i;
                        openBracketCh = lineText[i];
                        break;
                    }
                    depth--;
                }
            }

            // Find the matching closing bracket at depth 0 relative to cursor
            let closeBracketIdx = -1;
            depth = 0;
            const closeBracketMatch = openBracketCh === '{' ? '}' : ')';
            for (let i = charIdx; i < lineText.length; i++) {
                if (lineText[i] === openBracketCh) {
                    depth++;
                } else if (lineText[i] === closeBracketMatch) {
                    if (depth === 0) {
                        closeBracketIdx = i;
                        break;
                    }
                    depth--;
                }
            }

            const isInsideBrackets = openBracketIdx !== -1 && closeBracketIdx !== -1;

            let activeExpression = lineText;
            let relativeCursorChar = charIdx;

            if (isInsideBrackets) {
                activeExpression = lineText.substring(openBracketIdx + 1, closeBracketIdx);
                relativeCursorChar = charIdx - (openBracketIdx + 1);
            }

            // Strip indentation and assignment prefix (e.g. "    $var = ") so method is always token 0
            const assignmentMatch = activeExpression.match(
                /^\s*(?:(?:global|ephm)\s+)?[\$@\*&][a-z_][a-zA-Z0-9_]*\s*=\s*/
            );
            if (assignmentMatch) {
                relativeCursorChar -= assignmentMatch[0].length;
                activeExpression = activeExpression.substring(assignmentMatch[0].length);
            }

            // A returning method starts after the return keyword, not at token 0 of the full line.
            const returnPrefixMatch = activeExpression.match(/^\s*return\s+/i);
            if (returnPrefixMatch) {
                relativeCursorChar -= returnPrefixMatch[0].length;
                activeExpression = activeExpression.substring(returnPrefixMatch[0].length);
            }

            // Use the same interpolation-aware tokenizer as completion and signature help.
            const tokens = tokenizeSerExpression(activeExpression);

            let detectedMethodName = tokens.length > 0 ? tokens[0].text : null;
            let activeArgIndex = -1;
            for (let idx = 0; idx < tokens.length; idx++) {
                const t = tokens[idx];
                if (relativeCursorChar >= t.start && relativeCursorChar < t.end) {
                    if (idx === 0) {
                        activeArgIndex = -1;
                    } else {
                        activeArgIndex = idx - 1;
                    }
                    break;
                }
            }

            const targetMethodName = hasOwn(SER_TRUTH_TABLE.methods, hoveredWord) ? hoveredWord : detectedMethodName;

            if (!hasOwn(SER_TRUTH_TABLE.methods, targetMethodName)) {
                if (/^[@$&*][a-z_][a-zA-Z0-9_]*$/.test(hoveredWord)) {
                    const variable = SER_TRUTH_TABLE.variables?.find(item => item.fullName === hoveredWord);
                    return renderVariableHover(hoveredWord, variable, range);
                }
                return null;
            }

            const methodData = SER_TRUTH_TABLE.methods[targetMethodName];
            let md = "";

            // ==========================================
            // CASE A: HOVERING AN ARGUMENT
            // ==========================================
            if (activeArgIndex >= 0 && methodData.arguments && activeArgIndex < methodData.arguments.length) {
                const arg = methodData.arguments[activeArgIndex];
                return createMarkdownHover(methodArgumentMarkdown(arg), range);
            }

            // ==========================================
            // CASE B: HOVERING THE METHOD NAME
            // ==========================================
            md += `\`\`\`ser\n${escapeCodeBlock(methodData.syntax)}\n\`\`\`\n\n`;
            
            if (methodData.requiredFramework) {
                md += `<span style="color:#E5C07B;">⚠️ <strong>Requires Framework:</strong></span> \`${escapeInlineCode(methodData.requiredFramework)}\`\n\n`;
            }
            
            md += `${escapeMarkdown(methodData.description)}\n\n`;

            if (methodData.additionalDescription) {
                md += `<span style="color:#7F848E;">${escapeMarkdown(methodData.additionalDescription)}</span>\n\n`;
            }

            if (methodData.returns) {
                md += `**Returns:** <span style="color:#98C379;">${escapeMarkdown(methodData.returns)}</span>\n\n`;
            }

            // Errors rendered as a warning list block
            if (methodData.errors && methodData.errors.length > 0) {
                md += `\n***\n**Possible Exceptions:**\n`;
                methodData.errors.forEach(err => {
                    md += `- <span style="color:#E06C75;">❌ ${escapeMarkdown(err)}</span>\n`;
                });
            }

            const markdown = new vscode.MarkdownString(md);
            markdown.supportHtml = true; // Essential flag to authorize color translations
            markdown.isTrusted = true;
            return new vscode.Hover(markdown, range);
        }
    });

    const completionProvider = vscode.languages.registerCompletionItemProvider(
        'ser',
        { provideCompletionItems: provideCompletions },
        ' ', '@', '$', '&', '*'
    );

    const signatureProvider = vscode.languages.registerSignatureHelpProvider(
        'ser',
        { provideSignatureHelp },
        ' ', '\t', '@', '$', '&', '*', '{', '('
    );

    const workspaceSubscriptions = [];
    if (vscode.workspace?.onDidChangeTextDocument) {
        workspaceSubscriptions.push(vscode.workspace.onDidChangeTextDocument(event => {
            if (event.document?.languageId === 'ser') invalidateWorkspaceGlobals();
        }));
    }
    if (vscode.workspace?.createFileSystemWatcher) {
        const watcher = vscode.workspace.createFileSystemWatcher('**/*.ser');
        workspaceSubscriptions.push(
            watcher,
            watcher.onDidCreate(invalidateWorkspaceGlobals),
            watcher.onDidChange(invalidateWorkspaceGlobals),
            watcher.onDidDelete(invalidateWorkspaceGlobals)
        );
    }

    context.subscriptions.push(hoverProvider, completionProvider, signatureProvider, ...workspaceSubscriptions);
}

function deactivate() {}

module.exports = { activate, deactivate };
