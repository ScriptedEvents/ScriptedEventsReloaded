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
            if (current === '~') {
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
                    if (expression[index] === '~') {
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

function getMethodCallContext(lineText, cursorCharacter, methods) {
    const bracketStack = [];
    let inString = false;
    let escaped = false;

    for (let index = 0; index < cursorCharacter; index++) {
        const character = lineText[index];
        if (inString) {
            if (character === '~' && !escaped) {
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
    const method = methods?.[methodName];
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

module.exports = {
    findQuotedStringAtPosition,
    getMethodCallContext,
    tokenizeSerExpression
};
