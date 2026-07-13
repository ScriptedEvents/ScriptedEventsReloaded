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

function activate(context) {
    const hoverProvider = vscode.languages.registerHoverProvider('ser', {
        provideHover(document, position, token) {
            const wordRegex = /("(?:[^"\\]|\\.)*")|([@$&*][A-Za-z0-9_.]+)|([A-Za-z0-9_.]+)/g;
            let range = document.getWordRangeAtPosition(position, wordRegex);
            
            const lineText = document.lineAt(position.line).text;
            const charIdx = position.character;

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

            // If the cursor landed inside a quoted string but getWordRangeAtPosition
            // only returned the inner word, expand the range to the full string.
            if (lineText[charIdx] !== '"') {
                // Walk left to find the opening quote
                let openQuote = -1;
                let escaped = false;
                for (let i = charIdx; i >= 0; i--) {
                    if (lineText[i] === '\\') {
                        escaped = !escaped;
                        continue;
                    }
                    if (lineText[i] === '"' && !escaped) {
                        openQuote = i;
                        break;
                    }
                    escaped = false;
                }
                // Walk right to find the closing quote
                let closeQuote = -1;
                escaped = false;
                for (let i = charIdx; i < lineText.length; i++) {
                    if (lineText[i] === '\\') {
                        escaped = !escaped;
                        continue;
                    }
                    if (lineText[i] === '"' && !escaped) {
                        closeQuote = i;
                        break;
                    }
                    escaped = false;
                }
                if (openQuote !== -1 && closeQuote !== -1 && openQuote < closeQuote) {
                    range = new vscode.Range(
                        new vscode.Position(position.line, openQuote),
                        new vscode.Position(position.line, closeQuote + 1)
                    );
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
            const assignmentMatch = activeExpression.match(/^\s*[\$@\*&][a-z_][a-zA-Z0-9_]*\s*=\s*/);
            if (assignmentMatch) {
                relativeCursorChar -= assignmentMatch[0].length;
                activeExpression = activeExpression.substring(assignmentMatch[0].length);
            }

            // 2. Tokenize the current execution block to map out Method vs Argument positions
            // Treat balanced {…} and (…) expressions as single tokens
            const tokens = [];
            let i = 0;
            while (i < activeExpression.length) {
                if (activeExpression[i] === ' ' || activeExpression[i] === '\t') {
                    i++;
                    continue;
                }
                let tokenStart = i;
                if (activeExpression[i] === '"') {
                    // Quoted string
                    i++;
                    while (i < activeExpression.length && activeExpression[i] !== '"') {
                        if (activeExpression[i] === '\\') i++;
                        i++;
                    }
                    if (i < activeExpression.length) i++; // closing quote
                } else if (activeExpression[i] === '{') {
                    // Balanced {…} expression
                    let depth = 1;
                    i++;
                    while (i < activeExpression.length && depth > 0) {
                        if (activeExpression[i] === '{') depth++;
                        else if (activeExpression[i] === '}') depth--;
                        i++;
                    }
                } else if (activeExpression[i] === '(') {
                    // Balanced (…) expression
                    let depth = 1;
                    i++;
                    while (i < activeExpression.length && depth > 0) {
                        if (activeExpression[i] === '(') depth++;
                        else if (activeExpression[i] === ')') depth--;
                        i++;
                    }
                } else {
                    // Regular token
                    while (i < activeExpression.length && activeExpression[i] !== ' ' && activeExpression[i] !== '\t') {
                        i++;
                    }
                }
                tokens.push({ start: tokenStart, end: i, text: activeExpression.substring(tokenStart, i) });
            }

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

            const targetMethodName = SER_TRUTH_TABLE.methods.hasOwnProperty(hoveredWord) ? hoveredWord : detectedMethodName;

            if (!SER_TRUTH_TABLE.methods.hasOwnProperty(targetMethodName)) {
                return null;
            }

            const methodData = SER_TRUTH_TABLE.methods[targetMethodName];
            let md = "";

            // ==========================================
            // CASE A: HOVERING AN ARGUMENT
            // ==========================================
            if (activeArgIndex >= 0 && methodData.arguments && activeArgIndex < methodData.arguments.length) {
                const arg = methodData.arguments[activeArgIndex];

                md += `\`\`\`ser\n${escapeCodeBlock(arg.syntax)}\n\`\`\`\n\n`;
                if (arg.description) {
                    md += `<span style="color:#7F848E;">${escapeMarkdown(arg.description)}</span>\n\n`;
                }
                md += `**Type:** ${escapeInlineCode(arg.type)}\n\n`;
                
                if (arg.defaultValue) {
                    md += `**Default state:** ${escapeInlineCode(arg.defaultValue)}\n`;
                }
                
                if (arg.consumesRemainingValues) {
                    md += `<span style="color:#D19A66;">⚠️ Consumes remaining line values</span>\n`;
                }

                const markdown = new vscode.MarkdownString(md);
                markdown.supportHtml = true; // Enables the inline span styling colors
                markdown.isTrusted = true;
                return new vscode.Hover(markdown, range);
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

    context.subscriptions.push(hoverProvider);
}

function deactivate() {}

module.exports = { activate, deactivate };
