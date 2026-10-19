// Shared line scanner for quoted SER text and `{...}` / `(...)` expressions.
// Quoted text may contain `{...}` interpolations, which may in turn contain
// further quoted text. Inside text, `~` escapes the next character, so `~"`
// and `~{` stay literal.
function createSerLineScanner() {
    return { stack: [], escaped: false };
}

function stepSerLineScanner(state, current, index) {
    const top = state.stack[state.stack.length - 1];
    if (top?.kind === 'quote') {
        if (state.escaped) {
            state.escaped = false;
            return;
        }
        if (current === '~') {
            state.escaped = true;
        } else if (current === '"') {
            top.end = index;
            state.stack.pop();
        } else if (current === '{') {
            state.stack.push({ kind: 'bracket', character: '{', index, interpolation: true });
        }
        return;
    }

    if (current === '"') {
        state.stack.push({ kind: 'quote', start: index, end: null });
        state.escaped = false;
    } else if (current === '{' || current === '(') {
        state.stack.push({ kind: 'bracket', character: current, index, interpolation: false });
    } else if ((current === '}' || current === ')') && top?.kind === 'bracket') {
        state.stack.pop();
    }
}

function activeQuoteOf(state) {
    const top = state.stack[state.stack.length - 1];
    return top?.kind === 'quote' ? top : null;
}

function findQuotedStringAtPosition(lineText, character) {
    const state = createSerLineScanner();
    let quoteAtPosition = null;

    for (let index = 0; index < lineText.length; index++) {
        if (index === character) quoteAtPosition = activeQuoteOf(state);
        stepSerLineScanner(state, lineText[index], index);
    }

    if (character === lineText.length) quoteAtPosition = activeQuoteOf(state);
    return quoteAtPosition?.end == null
        ? null
        : { start: quoteAtPosition.start, end: quoteAtPosition.end };
}

// Returns the index just after the quoted text or bracket group opening at `start`.
function findGroupEnd(text, start) {
    const state = createSerLineScanner();
    for (let index = start; index < text.length; index++) {
        stepSerLineScanner(state, text[index], index);
        if (state.stack.length === 0) return index + 1;
    }
    return text.length;
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
        if (opening === '"' || opening === '{' || opening === '(') {
            index = findGroupEnd(expression, start);
        } else {
            while (index < expression.length && !/\s/.test(expression[index])) index++;
        }

        tokens.push({ start, end: index, text: expression.substring(start, index) });
    }

    return tokens;
}

function getMethodCallContext(lineText, cursorCharacter, methods) {
    // Find the innermost `{...}` or `(...)` around the cursor, including
    // interpolations inside quoted text such as "Delay: {ToDuration 3 Seconds}".
    const state = createSerLineScanner();
    const limit = Math.min(cursorCharacter, lineText.length);
    for (let index = 0; index < limit; index++) {
        stepSerLineScanner(state, lineText[index], index);
    }

    let frameDepth = -1;
    for (let depth = state.stack.length - 1; depth >= 0; depth--) {
        if (state.stack[depth].kind === 'bracket') {
            frameDepth = depth;
            break;
        }
    }

    let expressionStart = 0;
    let expressionEnd = lineText.length;
    if (frameDepth >= 0) {
        const frame = state.stack[frameDepth];
        expressionStart = frame.index + 1;
        for (let index = limit; index < lineText.length; index++) {
            stepSerLineScanner(state, lineText[index], index);
            if (state.stack[frameDepth] !== frame) {
                expressionEnd = index;
                break;
            }
        }
    }

    let expression = lineText.substring(expressionStart, expressionEnd);
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
        // In whitespace, the cursor is on the argument after every argument
        // that ends before it. Arguments to the right of the cursor do not count.
        activeArgument = tokens.slice(1).filter(token => token.end < relativeCursor).length;
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
