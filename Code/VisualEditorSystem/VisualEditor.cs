using JetBrains.Annotations;
using Newtonsoft.Json;
using SER.Code.FlagSystem.Flags;
using SER.Code.MethodSystem;
using SER.Code.MethodSystem.BaseMethods.Interfaces;
using SER.Code.Plugin.Commands.HelpSystem;
using SER.Code.VariableSystem;
using SER.Code.VariableSystem.Variables;

namespace SER.Code.VisualEditorSystem;

[UsedImplicitly]
public static class VisualEditor
{
    [UsedImplicitly]
    public static void CreateFile()
    {
        if (MethodIndex.GetMethods().Length == 0) MethodIndex.Initialize();
        if (Flag.FlagInfos.Count == 0) Flag.RegisterFlags();
        if (VariableIndex.GlobalVariables.Count == 0) VariableIndex.Initialize();
        try
        {
            SER.Code.EventSystem.EventHandler.Initialize();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[DEBUG_LOG] Failed to initialize EventHandler: " + ex.Message);
        }

        var metadata = new
        {
            Events = SER.Code.EventSystem.EventHandler.AvailableEvents.Select(e => e.Name).Distinct().OrderBy(n => n).ToArray(),
            Methods = MethodIndex.GetMethods().Select(m => new
            {
                m.Name,
                Description = DocsProvider.GetMethodHelp(m),
                m.Subgroup,
                ReturnType = (m as IReturningMethod)?.Returns.ToString(),
                Arguments = m.ExpectedArguments.Select(a => new
                {
                    a.Name,
                    Type = a.GetType().Name,
                    a.IsRequired, 
                    HasDefault = a.DefaultValue != null, 
                    DefaultString = a.DefaultValue?.StringRep ?? (a.DefaultValue?.Value is Enum ? a.DefaultValue.Value.ToString().Replace(", ", "|") : a.DefaultValue?.Value is bool ? a.DefaultValue.Value.ToString().ToLower() : a.DefaultValue?.Value?.ToString()),
                    Options = (a as SER.Code.ArgumentSystem.Arguments.OptionsArgument)?.Options.Select(o => new { o.Value, o.Description }),
                    EffectTypes = (a as SER.Code.ArgumentSystem.Arguments.EffectTypeArgument) != null ? SER.Code.ArgumentSystem.Arguments.EffectTypeArgument.EffectNames.Keys.ToArray() : null,
                    EnumValues = a.GetType().IsGenericType && a.GetType().GetGenericTypeDefinition() == typeof(SER.Code.ArgumentSystem.Arguments.EnumArgument<>) 
                        ? Enum.GetNames(a.GetType().GetGenericArguments()[0]).Union([a.DefaultValue?.Value is Enum ? a.DefaultValue.Value.ToString().Replace(", ", "|") : null
                        ]).Where(v => v != null).Distinct().ToArray() : null
                })
            }),
            Variables = VariableIndex.GlobalVariables
                .Select(v => new
                {
                    v.Name,
                    Prefix = v.Prefix.ToString(),
                    Category = (v as PredefinedPlayerVariable)?.Category ?? "General",
                    Description = $"Prefix: {v.Prefix}\n\nName: {v.Name}\n\nType: {v.FriendlyName}"
                }),
            Flags = Flag.FlagInfos.Select(kvp => {
                var flag = (Flag)Activator.CreateInstance(kvp.Value);
                return new {
                    Name = kvp.Key,
                    Description = DocsProvider.GetFlagInfo(kvp.Key),
                    InlineArgument = flag.InlineArgument != null ? new { 
                        flag.InlineArgument.Value.Name, 
                        flag.InlineArgument.Value.Description,
                        flag.InlineArgument.Value.Example
                    } : null,
                    Arguments = flag.Arguments.Select(a => new {
                        a.Name,
                        a.Description,
                        a.Example
                    })
                };
            })
        };

        var metadataJson = JsonConvert.SerializeObject(metadata);

        var file = 
            $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <title>SER Block Editor</title>
                <script src="https://unpkg.com/blockly/blockly.min.js"></script>
                <style>
                    body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 0; background: #1e1e1e; color: #e0e0e0; display: flex; flex-direction: row; height: 100vh; overflow: hidden; }
                    
                    /* Right Sidebar: Code Output */
                    #sidebar { width: 250px; min-width: 50px; height: 100%; display: flex; flex-direction: column; background: #252526; border-left: 1px solid #333; box-sizing: border-box; position: relative; transition: width 0.1s; }
                    #sidebar.collapsed { width: 40px !important; min-width: 40px; }
                    #sidebar h2 { font-size: 14px; text-transform: uppercase; letter-spacing: 1px; color: #858585; padding: 10px 15px; margin: 0; border-bottom: 1px solid #333; white-space: nowrap; overflow: hidden; display: flex; justify-content: space-between; align-items: center; }
                    #copyBtn { cursor: pointer; color: #858585; font-size: 12px; font-weight: bold; background: #333; border: 1px solid #444; border-radius: 4px; padding: 2px 8px; transition: color 0.2s, background 0.2s; text-transform: none; letter-spacing: normal; }
                    #copyBtn:hover { color: #fff; background: #444; }
                    #codeOutput { flex: 1; width: 100%; background: #1e1e1e; color: #d4d4d4; padding: 15px; font-family: 'Consolas', 'Courier New', monospace; font-size: 14px; border: none; resize: none; box-sizing: border-box; outline: none; }
                    #sidebar.collapsed #codeOutput, #sidebar.collapsed h2 span, #sidebar.collapsed #copyBtn { display: none; }
                    
                    /* Resizer handle */
                    #resizer { width: 4px; cursor: col-resize; background: #333; height: 100%; z-index: 10; transition: background 0.2s; }
                    #resizer:hover { background: #007acc; }

                    /* Collapse button */
                    #collapseBtn { cursor: pointer; color: #858585; font-weight: bold; padding: 0 5px; }
                    #collapseBtn:hover { color: #fff; }
                    
                    /* Left Side: Blockly Editor */
                    #main { flex: 1; height: 100%; display: flex; flex-direction: column; position: relative; }
                    #blocklyDiv { flex: 1; width: 100%; }
                </style>
            </head>
            <body>

                <div id="main">
                    <div id="blocklyDiv"></div>
                </div>

                <div id="resizer"></div>
                <div id="sidebar">
                    <h2><div id="collapseBtn" title="Toggle Sidebar">▶</div> <span>SER Script Output</span> <button id="copyBtn">Copy</button></h2>
                    <textarea id="codeOutput" readonly># Your SER script will appear here...</textarea>
                </div>

                <script>
                    const metadata = {{metadataJson}};

                    // --- 1. DYNAMIC BLOCK DEFINITIONS ---
                    
                    const serGenerator = new Blockly.Generator('SER');
                    serGenerator.ORDER_ATOMIC = 0;
                    serGenerator.ORDER_NONE = 99;

                    // Map SER argument types to Blockly input types/checks
                    const typeMap = {
                        'PlayersArgument': 'Player',
                        'TextArgument': 'String',
                        'StringArgument': 'String',
                        'IntArgument': 'Number',
                        'FloatArgument': 'Number',
                        'DurationArgument': 'String',
                        'VariableNameArgument': 'String',
                        'VariableArgument': 'Variable',
                        'BoolArgument': 'Boolean',
                        'OptionsArgument': 'Option',
                        'EnumArgument': 'Enum',
                        'EffectTypeArgument': 'Enum',
                        'NumberValue': 'Number',
                        'TextValue': 'String',
                        'StaticTextValue': 'String',
                        'BoolValue': 'Boolean',
                        'PlayerValue': 'Player'
                    };

                    // Define Logic Blocks
                    Blockly.Blocks['ser_comparison'] = {
                        init: function() {
                            this.appendValueInput("A");
                            this.appendDummyInput()
                                .appendField(new Blockly.FieldDropdown([
                                    ["is", "is"], 
                                    ["isnt", "isnt"], 
                                    [">", ">"], 
                                    ["<", "<"], 
                                    [">=", ">="], 
                                    ["<=", "<="]
                                ]), "OP");
                            this.appendValueInput("B");
                            this.setInputsInline(true);
                            this.setOutput(true, "Boolean");
                            this.setColour(210);
                            this.setTooltip("Compares two values.");
                        }
                    };

                    serGenerator.forBlock['ser_comparison'] = function(block, generator) {
                        const a = generator.valueToCode(block, 'A', serGenerator.ORDER_ATOMIC) || '...';
                        const op = block.getFieldValue('OP');
                        const b = generator.valueToCode(block, 'B', serGenerator.ORDER_ATOMIC) || '...';
                        
                        let res = a + " " + op + " " + b;
                        // Wrap nested expressions in parentheses if they contain spaces (simple heuristic for nested logic)
                        if (block.getParent() && (block.getParent().type === 'ser_logic' || block.getParent().type === 'ser_comparison')) {
                            return ["(" + res + ")", serGenerator.ORDER_ATOMIC];
                        }
                        return [res, serGenerator.ORDER_ATOMIC];
                    };

                    Blockly.Blocks['ser_logic'] = {
                        init: function() {
                            this.appendValueInput("A").setCheck("Boolean");
                            this.appendDummyInput()
                                .appendField(new Blockly.FieldDropdown([["and", "and"], ["or", "or"]]), "OP");
                            this.appendValueInput("B").setCheck("Boolean");
                            this.setInputsInline(true);
                            this.setOutput(true, "Boolean");
                            this.setColour(210);
                            this.setTooltip("Logical AND/OR.");
                        }
                    };

                    serGenerator.forBlock['ser_logic'] = function(block, generator) {
                        const a = generator.valueToCode(block, 'A', serGenerator.ORDER_ATOMIC) || '...';
                        const op = block.getFieldValue('OP');
                        const b = generator.valueToCode(block, 'B', serGenerator.ORDER_ATOMIC) || '...';
                        
                        let res = a + " " + op + " " + b;
                        if (block.getParent() && (block.getParent().type === 'ser_logic' || block.getParent().type === 'ser_comparison')) {
                            return ["(" + res + ")", serGenerator.ORDER_ATOMIC];
                        }
                        return [res, serGenerator.ORDER_ATOMIC];
                    };

                    Blockly.Blocks['ser_not'] = {
                        init: function() {
                            this.appendValueInput("BOOL").setCheck("Boolean").appendField("not");
                            this.setOutput(true, "Boolean");
                            this.setColour(210);
                        }
                    };

                    serGenerator.forBlock['ser_not'] = function(block, generator) {
                        const bool = generator.valueToCode(block, 'BOOL', serGenerator.ORDER_ATOMIC) || '...';
                        return ["not " + bool, serGenerator.ORDER_ATOMIC];
                    };

                    // Define Control Keywords
                    Blockly.Blocks['ser_wait'] = {
                        init: function() {
                            this.appendDummyInput()
                                .appendField("wait")
                                .appendField(new Blockly.FieldTextInput("1s"), "DURATION");
                            this.setPreviousStatement(true, null);
                            this.setNextStatement(true, null);
                            this.setColour(120);
                            this.setTooltip("Halts execution for a specified duration.");
                        }
                    };

                    serGenerator.forBlock['ser_wait'] = function(block) {
                        return "wait " + block.getFieldValue('DURATION') + "\n";
                    };

                    Blockly.Blocks['ser_wait_until'] = {
                        init: function() {
                            this.appendValueInput("CONDITION")
                                .setCheck("Boolean")
                                .appendField("wait_until");
                            this.setPreviousStatement(true, null);
                            this.setNextStatement(true, null);
                            this.setColour(120);
                            this.setTooltip("Halts execution until a condition is met.");
                        }
                    };

                    serGenerator.forBlock['ser_wait_until'] = function(block, generator) {
                        const condition = generator.valueToCode(block, 'CONDITION', serGenerator.ORDER_ATOMIC) || '...';
                        return "wait_until {" + condition + "}\n";
                    };

                    // Define If-related blocks
                    serGenerator.forBlock['controls_if'] = function(block, generator) {
                      let n = 0;
                      let code = '';
                      
                      // 1. The initial "if"
                      const condition = generator.valueToCode(block, 'IF' + n, serGenerator.ORDER_ATOMIC) || '...';
                      const branch = generator.statementToCode(block, 'DO' + n);
                      code += `if ${condition}\n${branch}`;

                      // 2. The "elif" parts (Blockly calls these else-if)
                      for (n = 1; n <= block.elseifCount_; n++) {
                        const elifCondition = generator.valueToCode(block, 'IF' + n, serGenerator.ORDER_ATOMIC) || '...';
                        const elifBranch = generator.statementToCode(block, 'DO' + n);
                        code += `elif ${elifCondition}\n${elifBranch}`;
                      }

                      // 3. The "else" part
                      if (block.elseCount_) {
                        const elseBranch = generator.statementToCode(block, 'ELSE');
                        code += `else\n${elseBranch}`;
                      }

                      // 4. The SER "end" keyword
                      return code + 'end\n';
                    };

                    // --- LOOP BLOCKS WITH MUTATORS ---
                    
                    Blockly.Blocks['ser_repeat'] = {
                        init: function() {
                            this.appendValueInput("COUNT")
                                .setCheck("Number")
                                .appendField("repeat");
                            this.appendStatementInput("DO")
                                .setCheck(null);
                            this.setPreviousStatement(true, null);
                            this.setNextStatement(true, null);
                            this.setColour(120);
                            this.setTooltip("Repeats the enclosed statements a specified number of times.");
                            const mutator = (Blockly.icons && Blockly.icons.MutatorIcon) ? new Blockly.icons.MutatorIcon(['ser_loop_var'], this) : new Blockly.Mutator(['ser_loop_var']);
                            if (this.setMutator) this.setMutator(mutator); else this.addIcon(mutator);
                            this.hasVar_ = false;
                        },
                        updateShape_: function() {
                            const inputExists = this.getInput('VAR');
                            if (this.hasVar_) {
                                if (!inputExists) {
                                    this.appendValueInput('VAR')
                                        .setCheck('Variable')
                                        .appendField("with variable");
                                    this.moveInputBefore('VAR', 'DO');
                                }
                            } else if (inputExists) {
                                this.removeInput('VAR');
                            }
                        },
                        mutationToDom: function() {
                            const container = Blockly.utils.xml.createElement('mutation');
                            container.setAttribute('has_var', this.hasVar_);
                            return container;
                        },
                        domToMutation: function(xmlElement) {
                            this.hasVar_ = (xmlElement.getAttribute('has_var') === 'true');
                            this.updateShape_();
                        },
                        decompose: function(workspace) {
                            const containerBlock = workspace.newBlock('ser_loop_mutator');
                            containerBlock.initSvg();
                            if (this.hasVar_) {
                                const varBlock = workspace.newBlock('ser_loop_var');
                                varBlock.initSvg();
                                containerBlock.getInput('STACK').connection.connect(varBlock.previousConnection);
                            }
                            return containerBlock;
                        },
                        compose: function(containerBlock) {
                            let itemBlock = containerBlock.getInputTargetBlock('STACK');
                            this.hasVar_ = false;
                            while (itemBlock) {
                                if (itemBlock.type === 'ser_loop_var') {
                                    this.hasVar_ = true;
                                }
                                itemBlock = itemBlock.nextConnection && itemBlock.nextConnection.targetBlock();
                            }
                            this.updateShape_();
                        },
                        saveExtraState: function() {
                            return { 'hasVar': this.hasVar_ };
                        },
                        loadExtraState: function(state) {
                            this.hasVar_ = state['hasVar'];
                            this.updateShape_();
                        }
                    };

                    Blockly.Blocks['ser_over'] = {
                        init: function() {
                            this.appendValueInput("COLLECTION")
                                .setCheck(["Collection", "Player"])
                                .appendField("over");
                            this.appendStatementInput("DO")
                                .setCheck(null);
                            this.setPreviousStatement(true, null);
                            this.setNextStatement(true, null);
                            this.setColour(120);
                            this.setTooltip("Iterates over a collection or player array.");
                            const mutator = (Blockly.icons && Blockly.icons.MutatorIcon) ? new Blockly.icons.MutatorIcon(['ser_loop_var', 'ser_loop_iter'], this) : new Blockly.Mutator(['ser_loop_var', 'ser_loop_iter']);
                            if (this.setMutator) this.setMutator(mutator); else this.addIcon(mutator);
                            this.hasVar_ = false;
                            this.hasIter_ = false;
                        },
                        updateShape_: function() {
                            const varExists = this.getInput('VAR');
                            const iterExists = this.getInput('ITER');
                            
                            if (this.hasVar_) {
                                if (!varExists) {
                                    this.appendValueInput('VAR')
                                        .setCheck('Variable')
                                        .appendField("with item");
                                    this.moveInputBefore('VAR', 'DO');
                                }
                            } else if (varExists) {
                                this.removeInput('VAR');
                            }
                            
                            if (this.hasIter_) {
                                if (!iterExists) {
                                    this.appendValueInput('ITER')
                                        .setCheck('Variable')
                                        .appendField(this.hasVar_ ? "and index" : "with index");
                                    this.moveInputBefore('ITER', 'DO');
                                }
                            } else if (iterExists) {
                                this.removeInput('ITER');
                            }
                        },
                        mutationToDom: function() {
                            const container = Blockly.utils.xml.createElement('mutation');
                            container.setAttribute('has_var', this.hasVar_);
                            container.setAttribute('has_iter', this.hasIter_);
                            return container;
                        },
                        domToMutation: function(xmlElement) {
                            this.hasVar_ = (xmlElement.getAttribute('has_var') === 'true');
                            this.hasIter_ = (xmlElement.getAttribute('has_iter') === 'true');
                            this.updateShape_();
                        },
                        decompose: function(workspace) {
                            const containerBlock = workspace.newBlock('ser_loop_mutator');
                            containerBlock.initSvg();
                            let connection = containerBlock.getInput('STACK').connection;
                            if (this.hasVar_) {
                                const varBlock = workspace.newBlock('ser_loop_var');
                                varBlock.initSvg();
                                connection.connect(varBlock.previousConnection);
                                connection = varBlock.nextConnection;
                            }
                            if (this.hasIter_) {
                                const iterBlock = workspace.newBlock('ser_loop_iter');
                                iterBlock.initSvg();
                                connection.connect(iterBlock.previousConnection);
                            }
                            return containerBlock;
                        },
                        compose: function(containerBlock) {
                            let itemBlock = containerBlock.getInputTargetBlock('STACK');
                            this.hasVar_ = false;
                            this.hasIter_ = false;
                            while (itemBlock) {
                                if (itemBlock.type === 'ser_loop_var') this.hasVar_ = true;
                                if (itemBlock.type === 'ser_loop_iter') this.hasIter_ = true;
                                itemBlock = itemBlock.nextConnection && itemBlock.nextConnection.targetBlock();
                            }
                            this.updateShape_();
                        },
                        saveExtraState: function() {
                            return { 'hasVar': this.hasVar_, 'hasIter': this.hasIter_ };
                        },
                        loadExtraState: function(state) {
                            this.hasVar_ = state['hasVar'];
                            this.hasIter_ = state['hasIter'];
                            this.updateShape_();
                        }
                    };

                    Blockly.Blocks['ser_loop_mutator'] = {
                        init: function() {
                            this.appendDummyInput().appendField("loop options");
                            this.appendStatementInput("STACK");
                            this.setColour(120);
                            this.contextMenu = false;
                        }
                    };

                    Blockly.Blocks['ser_loop_var'] = {
                        init: function() {
                            this.appendDummyInput().appendField("item variable");
                            this.setPreviousStatement(true);
                            this.setNextStatement(true);
                            this.setColour(120);
                            this.contextMenu = false;
                        }
                    };

                    Blockly.Blocks['ser_loop_iter'] = {
                        init: function() {
                            this.appendDummyInput().appendField("iteration variable");
                            this.setPreviousStatement(true);
                            this.setNextStatement(true);
                            this.setColour(120);
                            this.contextMenu = false;
                        }
                    };

                    serGenerator.forBlock['ser_repeat'] = function(block, generator) {
                        const count = generator.valueToCode(block, 'COUNT', serGenerator.ORDER_ATOMIC) || '...';
                        const branch = generator.statementToCode(block, 'DO');
                        let code = `repeat ${count}`;
                        if (block.hasVar_) {
                            const varName = generator.valueToCode(block, 'VAR', serGenerator.ORDER_ATOMIC) || '$i';
                            code += ` with ${varName}`;
                        }
                        return code + `\n${branch}end\n`;
                    };

                    serGenerator.forBlock['ser_over'] = function(block, generator) {
                        const coll = generator.valueToCode(block, 'COLLECTION', serGenerator.ORDER_ATOMIC) || '...';
                        const branch = generator.statementToCode(block, 'DO');
                        let code = `over ${coll}`;
                        if (block.hasVar_ || block.hasIter_) {
                            code += ` with`;
                            if (block.hasVar_) {
                                const varName = generator.valueToCode(block, 'VAR', serGenerator.ORDER_ATOMIC) || (coll.includes('@') ? '@plr' : '$item');
                                code += ` ${varName}`;
                            }
                            if (block.hasIter_) {
                                const iterName = generator.valueToCode(block, 'ITER', serGenerator.ORDER_ATOMIC) || '$i';
                                code += ` ${iterName}`;
                            }
                        }
                        return code + `\n${branch}end\n`;
                    };

                    // Value Blocks (Literals)
                    Blockly.Blocks['ser_text_value'] = {
                        init: function() {
                            this.appendDummyInput()
                                .appendField(new Blockly.FieldTextInput("text"), "VALUE");
                            this.setOutput(true, "String");
                            this.setColour(160);
                        }
                    };

                    Blockly.Blocks['ser_number_value'] = {
                        init: function() {
                            this.appendDummyInput()
                                .appendField(new Blockly.FieldNumber(0), "VALUE");
                            this.setOutput(true, "Number");
                            this.setColour(160);
                        }
                    };

                    Blockly.Blocks['ser_bool_value'] = {
                        init: function() {
                            this.appendDummyInput()
                                .appendField(new Blockly.FieldDropdown([["true", "true"], ["false", "false"]]), "VALUE");
                            this.setOutput(true, "Boolean");
                            this.setColour(160);
                        }
                    };

                    serGenerator.forBlock['ser_text_value'] = function(block) {
                        let val = block.getFieldValue('VALUE');
                        if (!val.startsWith('"')) val = '"' + val + '"';
                        return [val, serGenerator.ORDER_ATOMIC];
                    };

                    serGenerator.forBlock['ser_number_value'] = function(block) {
                        return [block.getFieldValue('VALUE'), serGenerator.ORDER_ATOMIC];
                    };

                    serGenerator.forBlock['ser_bool_value'] = function(block) {
                        return [block.getFieldValue('VALUE'), serGenerator.ORDER_ATOMIC];
                    };

                    // Define Flags
                    metadata.Flags.forEach(flag => {
                        Blockly.Blocks['ser_flag_' + flag.Name] = {
                            init: function() {
                                let block = this;
                                block.appendDummyInput().appendField("Flag: " + flag.Name);
                                
                                if (flag.InlineArgument) {
                                    let input = block.appendDummyInput()
                                        .appendField(flag.InlineArgument.Name + ":");
                                    if (flag.Name === "OnEvent" && metadata.Events) {
                                        const eventOptions = metadata.Events.map(e => [e, e]);
                                        input.appendField(new Blockly.FieldDropdown(eventOptions), "INLINE");
                                    } else {
                                        input.appendField(new Blockly.FieldTextInput(""), "INLINE");
                                    }
                                }
                                
                                flag.Arguments.forEach(arg => {
                                    block.appendDummyInput()
                                        .appendField(arg.Name + ":")
                                        .appendField(new Blockly.FieldTextInput(""), arg.Name);
                                });

                                this.setPreviousStatement(false);
                                this.setNextStatement(true, null);
                                this.setColour(290);
                                this.setTooltip(flag.Description.replace(/\n(?!\n)/g, '\n\n'));
                            }
                        };

                        serGenerator.forBlock['ser_flag_' + flag.Name] = function(block) {
                            let line = `!-- ${flag.Name}`;
                            if (flag.InlineArgument) {
                                const val = block.getFieldValue('INLINE');
                                if (val) line += ` ${val}`;
                            }
                            line += "\n";
                            
                            flag.Arguments.forEach(arg => {
                                const val = block.getFieldValue(arg.Name);
                                if (val) {
                                    line += `-- ${arg.Name} ${val}\n`;
                                }
                            });
                            return line;
                        };
                    });
                    metadata.Methods.forEach(method => {
                        // 1. Standalone (Statement) Block
                        Blockly.Blocks['ser_method_' + method.Name] = {
                            init: function() {
                                let block = this;
                                block.appendDummyInput().appendField(method.Name + (method.ReturnType ? " [std]" : ""));
                                
                                method.Arguments.forEach(arg => {
                                    const blocklyType = typeMap[arg.Type] || null;
                                    if (blocklyType === 'Number' || arg.Type === 'IntArgument' || arg.Type === 'FloatArgument') {
                                        block.appendDummyInput()
                                            .appendField(arg.Name + ':')
                                            .appendField(new Blockly.FieldNumber(arg.DefaultString || 0), arg.Name);
                                    } else if (arg.Type === 'DurationArgument') {
                                        block.appendDummyInput()
                                            .appendField(arg.Name + ':')
                                            .appendField(new Blockly.FieldTextInput("5s"), arg.Name);
                                    } else if (arg.Type === 'BoolArgument') {
                                        block.appendDummyInput()
                                            .appendField(arg.Name + ':')
                                            .appendField(new Blockly.FieldDropdown([["true", "true"], ["false", "false"]]), arg.Name);
                                        if (arg.DefaultString) {
                                            block.setFieldValue(arg.DefaultString.toLowerCase(), arg.Name);
                                        }
                                    } else if (arg.Type === 'TextArgument' || arg.Type === 'StringArgument') {
                                        block.appendDummyInput()
                                            .appendField(arg.Name + ':')
                                            .appendField(new Blockly.FieldTextInput(arg.DefaultString || ""), arg.Name);
                                    } else if (arg.Type === 'OptionsArgument' && arg.Options) {
                                        const options = arg.Options.map(o => [o.Value, o.Value]);
                                        block.appendDummyInput()
                                            .appendField(arg.Name + ':')
                                            .appendField(new Blockly.FieldDropdown(options), arg.Name);
                                        if (arg.DefaultString) {
                                            block.setFieldValue(arg.DefaultString, arg.Name);
                                        }
                                    } else if (arg.Type === 'EffectTypeArgument' && arg.EffectTypes) {
                                        const options = arg.EffectTypes.map(v => [v, v]);
                                        block.appendDummyInput()
                                            .appendField(arg.Name + ':')
                                            .appendField(new Blockly.FieldDropdown(options), arg.Name);
                                        if (arg.DefaultString) {
                                            block.setFieldValue(arg.DefaultString, arg.Name);
                                        }
                                    } else if (arg.EnumValues) {
                                        const options = arg.EnumValues.map(v => [v, v]);
                                        block.appendDummyInput()
                                            .appendField(arg.Name + ':')
                                            .appendField(new Blockly.FieldDropdown(options), arg.Name);
                                        if (arg.DefaultString) {
                                            block.setFieldValue(arg.DefaultString, arg.Name);
                                        }
                                    } else {
                                        block.appendValueInput(arg.Name)
                                            .setCheck(blocklyType)
                                            .appendField(arg.Name + ':');
                                    }
                                });

                                this.setInputsInline(true);
                                this.setPreviousStatement(true, null);
                                this.setNextStatement(true, null);
                                this.setColour(230);
                                this.setTooltip(method.Description.replace(/\n(?!\n)/g, '\n\n'));
                            }
                        };

                        // 2. Returning (Expression) Block
                        if (method.ReturnType) {
                            Blockly.Blocks['ser_method_' + method.Name + '_ret'] = {
                                init: function() {
                                    let block = this;
                                    block.appendDummyInput().appendField(method.Name + " [ret]");
                                    
                                    method.Arguments.forEach(arg => {
                                        const blocklyType = typeMap[arg.Type] || null;
                                        if (blocklyType === 'Number' || arg.Type === 'IntArgument' || arg.Type === 'FloatArgument') {
                                            block.appendDummyInput()
                                                .appendField(arg.Name + ':')
                                                .appendField(new Blockly.FieldNumber(arg.DefaultString || 0), arg.Name);
                                        } else if (arg.Type === 'DurationArgument') {
                                            block.appendDummyInput()
                                                .appendField(arg.Name + ':')
                                                .appendField(new Blockly.FieldTextInput("5s"), arg.Name);
                                        } else if (arg.Type === 'BoolArgument') {
                                            block.appendDummyInput()
                                                .appendField(arg.Name + ':')
                                                .appendField(new Blockly.FieldDropdown([["true", "true"], ["false", "false"]]), arg.Name);
                                            if (arg.DefaultString) {
                                                block.setFieldValue(arg.DefaultString.toLowerCase(), arg.Name);
                                            }
                                        } else if (arg.Type === 'TextArgument' || arg.Type === 'StringArgument') {
                                            block.appendDummyInput()
                                                .appendField(arg.Name + ':')
                                                .appendField(new Blockly.FieldTextInput(arg.DefaultString || ""), arg.Name);
                                        } else if (arg.Type === 'OptionsArgument' && arg.Options) {
                                            const options = arg.Options.map(o => [o.Value, o.Value]);
                                            block.appendDummyInput()
                                                .appendField(arg.Name + ':')
                                                .appendField(new Blockly.FieldDropdown(options), arg.Name);
                                            if (arg.DefaultString) {
                                                block.setFieldValue(arg.DefaultString, arg.Name);
                                            }
                                        } else if (arg.Type === 'EffectTypeArgument' && arg.EffectTypes) {
                                            const options = arg.EffectTypes.map(v => [v, v]);
                                            block.appendDummyInput()
                                                .appendField(arg.Name + ':')
                                                .appendField(new Blockly.FieldDropdown(options), arg.Name);
                                            if (arg.DefaultString) {
                                                block.setFieldValue(arg.DefaultString, arg.Name);
                                            }
                                        } else if (arg.EnumValues) {
                                            const options = arg.EnumValues.map(v => [v, v]);
                                            block.appendDummyInput()
                                                .appendField(arg.Name + ':')
                                                .appendField(new Blockly.FieldDropdown(options), arg.Name);
                                            if (arg.DefaultString) {
                                                block.setFieldValue(arg.DefaultString, arg.Name);
                                            }
                                        } else {
                                            block.appendValueInput(arg.Name)
                                                .setCheck(blocklyType)
                                                .appendField(arg.Name + ':');
                                        }
                                    });

                                    this.setInputsInline(true);
                                    this.setOutput(true, typeMap[method.ReturnType] || null);
                                    this.setColour(260); // Use a purple/violet colour for returning methods
                                    this.setTooltip(method.Description.replace(/\n(?!\n)/g, '\n\n'));
                                }
                            };
                        }
                    });

                    // Define Predefined Variables (Dropdown)
                    Blockly.Blocks['ser_player_var'] = {
                        init: function() {
                            const options = metadata.Variables.map(v => [v.Prefix + v.Name, v.Prefix + v.Name]);
                            if (options.length === 0) options.push(["@all", "@all"]);
                            
                            this.appendDummyInput()
                                .appendField(new Blockly.FieldDropdown(options), "VAR");
                            this.setOutput(true, ["Player", "String", "Number"]);
                            this.setColour(160);
                            this.setTooltip(function() {
                                const varName = this.getFieldValue('VAR');
                                const variable = metadata.Variables.find(v => (v.Prefix + v.Name) === varName);
                                return variable ? variable.Description : "";
                            }.bind(this));
                        }
                    };

                    // Basic Variable Block
                    Blockly.Blocks['ser_custom_var'] = {
                        init: function() {
                            this.appendDummyInput()
                                .appendField(new Blockly.FieldDropdown([
                                    ["$", "$"], ["@", "@"], ["*", "*"], ["&", "&"]
                                ]), "PREFIX")
                                .appendField(new Blockly.FieldTextInput("myVar"), "VAR");
                            this.setOutput(true, ["String", "Number", "Player", "Variable"]);
                            this.setColour(160);
                        }
                    };

                    // Variable Assignment Block
                    Blockly.Blocks['ser_var_assign'] = {
                        init: function() {
                            this.appendDummyInput()
                                .appendField("Variable:")
                                .appendField(new Blockly.FieldDropdown([
                                    ["$", "$"], ["@", "@"], ["*", "*"], ["&", "&"]
                                ]), "PREFIX")
                                .appendField(new Blockly.FieldTextInput("myVar"), "VAR")
                                .appendField(new Blockly.FieldTextInput("Value"), "VALUE");
                            this.setInputsInline(true);
                            this.setPreviousStatement(true, null);
                            this.setNextStatement(true, null);
                            this.setColour(160);
                        }
                    };

                    // --- 2. DEFINE MORE OF THE SER GENERATOR ---

                    serGenerator.scrub_ = function(block, code, opt_thisOnly) {
                        const nextBlock = block.getNextBlock();
                        if (nextBlock && !opt_thisOnly) {
                            return code + serGenerator.blockToCode(nextBlock);
                        }
                        return code;
                    };

                    // Generator for dynamic methods
                    metadata.Methods.forEach(method => {
                        // 1. Generator for Standalone Block
                        serGenerator.forBlock['ser_method_' + method.Name] = function(block, generator) {
                            let args = [];
                            let lastProvidedIndex = -1;

                            // First pass: find the last argument that has a value provided by the user
                            method.Arguments.forEach((arg, index) => {
                                let val = null;
                                if (block.getField(arg.Name)) {
                                    val = block.getFieldValue(arg.Name); if (arg.HasDefault && val == arg.DefaultString) val = null;
                                } else {
                                    val = generator.valueToCode(block, arg.Name, serGenerator.ORDER_ATOMIC);
                                }
                                
                                if (val !== null && val !== '') {
                                    lastProvidedIndex = index;
                                }
                            });

                            // Second pass: build the argument list
                            method.Arguments.forEach((arg, index) => {
                                if (index > lastProvidedIndex && !arg.IsRequired) return;

                                let val;
                                if (block.getField(arg.Name)) {
                                    val = block.getFieldValue(arg.Name); 
                                    if (arg.HasDefault && val == arg.DefaultString) val = null;
                                    
                                    if (!arg.IsRequired && arg.HasDefault && index < lastProvidedIndex && (val === "" || val === null || val === arg.DefaultString)) {
                                         val = "_";
                                    } else {
                                        if (arg.Type === 'TextArgument' || arg.Type === 'StringArgument') {
                                            if (!val.startsWith('"')) val = '"' + val + '"';
                                        }
                                    }
                                } else {
                                    val = generator.valueToCode(block, arg.Name, serGenerator.ORDER_ATOMIC);
                                    if (val === null || val === '') {
                                        if (arg.IsRequired) val = "...";
                                        else if (index < lastProvidedIndex) val = "_";
                                        else return; 
                                    }
                                }
                                args.push(val);
                            });
                            
                            return `${method.Name} ${args.join(' ')}\n`;
                        };

                        // 2. Generator for Returning Block
                        if (method.ReturnType) {
                            serGenerator.forBlock['ser_method_' + method.Name + '_ret'] = function(block, generator) {
                                let args = [];
                                let lastProvidedIndex = -1;

                                method.Arguments.forEach((arg, index) => {
                                    let val = null;
                                    if (block.getField(arg.Name)) {
                                        val = block.getFieldValue(arg.Name); if (arg.HasDefault && val == arg.DefaultString) val = null;
                                    } else {
                                        val = generator.valueToCode(block, arg.Name, serGenerator.ORDER_ATOMIC);
                                    }
                                    if (val !== null && val !== '') lastProvidedIndex = index;
                                });

                                method.Arguments.forEach((arg, index) => {
                                    if (index > lastProvidedIndex && !arg.IsRequired) return;

                                    let val;
                                    if (block.getField(arg.Name)) {
                                        val = block.getFieldValue(arg.Name);
                                        if (arg.HasDefault && val == arg.DefaultString) val = null;
                                        if (!arg.IsRequired && arg.HasDefault && index < lastProvidedIndex && (val === "" || val === null || val === arg.DefaultString)) {
                                             val = "_";
                                        } else {
                                            if (arg.Type === 'TextArgument' || arg.Type === 'StringArgument') {
                                                if (!val.startsWith('"')) val = '"' + val + '"';
                                            }
                                        }
                                    } else {
                                        val = generator.valueToCode(block, arg.Name, serGenerator.ORDER_ATOMIC);
                                        if (val === null || val === '') {
                                            if (arg.IsRequired) val = "...";
                                            else if (index < lastProvidedIndex) val = "_";
                                            else return;
                                        }
                                    }
                                    args.push(val);
                                });
                                
                                return [`\{${method.Name} ${args.join(' ')}\}`, serGenerator.ORDER_ATOMIC];
                            };
                        }
                    });

                    serGenerator.forBlock['ser_player_var'] = function(block) {
                        return [block.getFieldValue('VAR'), serGenerator.ORDER_ATOMIC];
                    };

                    serGenerator.forBlock['ser_custom_var'] = function(block) {
                        const prefix = block.getFieldValue('PREFIX');
                        const name = block.getFieldValue('VAR');
                        return [prefix + name, serGenerator.ORDER_ATOMIC];
                    };

                    serGenerator.forBlock['ser_var_assign'] = function(block, generator) {
                        const prefix = block.getFieldValue('PREFIX');
                        const name = block.getFieldValue('VAR');
                        const value = block.getFieldValue('VALUE');
                        return `Variable: ${prefix}${name} ${value}\n`;
                    };

                    // --- 3. INITIALIZE WORKSPACE ---
                    
                    // Group methods by subgroup for toolbox
                    const methodCategories = [];
                    const categoryMap = {};
                    metadata.Methods.forEach(m => {
                        if (!categoryMap[m.Subgroup]) {
                            categoryMap[m.Subgroup] = [];
                            methodCategories.push({
                                kind: "category",
                                name: (m.Subgroup || "General") + " methods",
                                colour: "230",
                                contents: categoryMap[m.Subgroup]
                            });
                        }
                        categoryMap[m.Subgroup].push({ kind: "block", type: "ser_method_" + m.Name });
                        if (m.ReturnType) {
                            categoryMap[m.Subgroup].push({ kind: "block", type: "ser_method_" + m.Name + "_ret" });
                        }
                    });

                    // Sort method categories alphabetically
                    methodCategories.sort((a, b) => a.name.localeCompare(b.name));

                    const toolbox = {
                        "kind": "categoryToolbox",
                        "contents": [
                            {
                                "kind": "category", "name": "Flags", "colour": "290",
                                "contents": metadata.Flags.map(f => ({ kind: "block", type: "ser_flag_" + f.Name }))
                            },
                            {
                                "kind": "category", "name": "Methods", "colour": "230",
                                "contents": methodCategories
                            },
                            {
                                "kind": "category", "name": "Control", "colour": "120",
                                "contents": [
                                    { "kind": "block", "type": "ser_wait" },
                                    { "kind": "block", "type": "ser_wait_until" },
                                    { "kind": "block", "type": "controls_if" },
                                    { "kind": "block", "type": "ser_repeat" },
                                    { 
                                        "kind": "block", 
                                        "type": "ser_repeat",
                                        "extraState": { "hasVar": true }
                                    },
                                    { "kind": "block", "type": "ser_over" },
                                    {
                                        "kind": "block",
                                        "type": "ser_over",
                                        "extraState": { "hasVar": true }
                                    },
                                    { "kind": "block", "type": "ser_comparison" },
                                    { "kind": "block", "type": "ser_logic" },
                                    { "kind": "block", "type": "ser_not" }
                                ]
                            },
                            {
                                "kind": "category", "name": "Values", "colour": "160",
                                "contents": [
                                    { "kind": "block", "type": "ser_text_value" },
                                    { "kind": "block", "type": "ser_number_value" },
                                    { "kind": "block", "type": "ser_bool_value" }
                                ]
                            },
                            {
                                "kind": "category", "name": "Variables", "colour": "160",
                                "contents": [
                                    { "kind": "block", "type": "ser_var_assign" },
                                    { "kind": "block", "type": "ser_player_var" },
                                    { "kind": "block", "type": "ser_custom_var" }
                                ]
                            }
                        ]
                    };
                    
                    const SERDarkTheme = Blockly.Theme.defineTheme('ser_dark', {
                      'base': Blockly.Themes.Classic, // Inherit default block colors
                      'componentStyles': {
                        'workspaceBackgroundColour': '#1e1e1e', // Dark charcoal background
                        'toolboxBackgroundColour': '#333',      // Darker toolbox
                        'toolboxForegroundColour': '#fff',      // White text in toolbox
                        'flyoutBackgroundColour': '#252526',    // Dark flyout (the "drawer" for blocks)
                        'flyoutForegroundColour': '#ccc',
                        'scrollbarColour': '#797979',
                        'insertionMarkerColour': '#fff',
                        'insertionMarkerOpacity': 0.3,
                      }
                    });

                    const workspace = Blockly.inject('blocklyDiv', { 
                        toolbox: toolbox,
                        theme: SERDarkTheme,
                        sounds: false
                    });

                    // --- 4. SIDEBAR RESIZING & COLLAPSING ---
                    const sidebar = document.getElementById('sidebar');
                    const resizer = document.getElementById('resizer');
                    const collapseBtn = document.getElementById('collapseBtn');
                    let isResizing = false;

                    resizer.addEventListener('mousedown', (e) => {
                        isResizing = true;
                        document.addEventListener('mousemove', handleMouseMove);
                        document.addEventListener('mouseup', () => {
                            isResizing = false;
                            document.removeEventListener('mousemove', handleMouseMove);
                            Blockly.svgResize(workspace);
                        });
                    });

                    function handleMouseMove(e) {
                        if (!isResizing) return;
                        const newWidth = window.innerWidth - e.clientX;
                        if (newWidth > 150 && newWidth < 800) {
                            sidebar.style.width = newWidth + 'px';
                            sidebar.classList.remove('collapsed');
                            collapseBtn.textContent = '▶';
                            Blockly.svgResize(workspace);
                        }
                    }

                    collapseBtn.addEventListener('click', () => {
                        sidebar.classList.toggle('collapsed');
                        if (sidebar.classList.contains('collapsed')) {
                            collapseBtn.textContent = '◀';
                        } else {
                            collapseBtn.textContent = '▶';
                        }
                        // Use a small timeout to let the CSS transition finish if any, 
                        // though here we use a small transition time.
                        setTimeout(() => Blockly.svgResize(workspace), 110);
                    });

                    document.getElementById('copyBtn').addEventListener('click', () => {
                        const code = document.getElementById('codeOutput').value;
                        navigator.clipboard.writeText(code).then(() => {
                            const btn = document.getElementById('copyBtn');
                            btn.textContent = 'Copied!';
                            setTimeout(() => btn.textContent = 'Copy', 2000);
                        });
                    });

                    function updateCode() {
                        const code = serGenerator.workspaceToCode(workspace);
                        document.getElementById('codeOutput').value = code || "# Drag blocks to the field above to generate your SER script...";
                    }
                    workspace.addChangeListener(updateCode);
                    updateCode();
                </script>
            </body>
            </html>
            """;
        
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "SER Visual Editor.html"), file);
    }
}
