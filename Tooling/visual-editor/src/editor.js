(function createBeginnerEditor() {
  "use strict";

  const editorHost = typeof acquireVsCodeApi === "function" ? acquireVsCodeApi() : null;
  const AUTOSAVE_KEY = "ser.visualEditor.beginnerWorkspace.v2";
  const PROJECT_FORMAT = "ser-blockly-beginner-project";
  const PROJECT_VERSION = 2;
  const ENTRY_TYPES = new Set(["ser_when_event", "ser_when_command", "ser_when_manual"]);
  const ACTION_TYPES = new Set([
    "ser_broadcast",
    "ser_hint",
    "ser_heal",
    "ser_damage",
    "ser_kill",
    "ser_give_item",
    "ser_set_role",
    "ser_teleport",
    "ser_clear_inventory",
    "ser_reply",
    "ser_wait",
    "ser_stop",
    "ser_if",
    "ser_repeat"
  ]);

  const COLOURS = Object.freeze({
    start: 282,
    players: 203,
    messages: 326,
    actions: 25,
    decisions: 116,
    timing: 48,
    values: 188
  });

  const CURATED_EVENTS = [
    {
      value: "RoundStarted",
      label: "the round starts",
      fallback: "Runs after a new round has started."
    },
    {
      value: "WaitingForPlayers",
      label: "the server is waiting for players",
      fallback: "Runs when the server is ready and waiting for players."
    },
    {
      value: "Joined",
      label: "a player joins",
      fallback: "Runs when a player joins the server."
    },
    {
      value: "Death",
      label: "a player dies",
      fallback: "Runs when a player dies."
    },
    {
      value: "Hurt",
      label: "a player is hurt",
      fallback: "Runs after a player takes damage."
    },
    {
      value: "ChangedRole",
      label: "a player changes role",
      fallback: "Runs when a player's role changes."
    }
  ].filter(event => !SER_TRUTH_TABLE.events?.length || SER_TRUTH_TABLE.events.includes(event.value));

  const EVENT_OPTIONS = CURATED_EVENTS.map(event => [event.label, event.value]);
  const EVENT_LABELS = Object.fromEntries(CURATED_EVENTS.map(event => [event.value, event.label]));

  const TARGET_OPTIONS = [
    ["all players", "@all"],
    ["all living players", "@alivePlayers"],
    ["all SCP players", "@scpPlayers"],
    ["the player from this event", "@evPlayer"],
    ["the attacker from this event", "@evAttacker"],
    ["the player who used the command", "@sender"]
  ];

  const SINGLE_PLAYER_OPTIONS = [
    ["the player from this event", "@evPlayer"],
    ["the attacker from this event", "@evAttacker"],
    ["the player who used the command", "@sender"]
  ];

  const TARGET_LABELS = Object.fromEntries(TARGET_OPTIONS.map(([label, value]) => [value, label]));

  function methodEnumValues(methodName, argumentIndex) {
    return SERLanguageCore.enumArgumentValues(SER_TRUTH_TABLE, methodName, argumentIndex);
  }

  function curatedEnumOptions(methodName, argumentIndex, preferredValues) {
    const available = new Set(methodEnumValues(methodName, argumentIndex));
    const values = preferredValues.filter(value => available.size === 0 || available.has(value));
    return (values.length > 0 ? values : preferredValues).map(value => [
      value.replace(/([a-z])([A-Z])/g, "$1 $2"),
      value
    ]);
  }

  const ITEM_OPTIONS = curatedEnumOptions("GiveItem", 1, [
    "Medkit",
    "Painkillers",
    "Coin",
    "Flashlight",
    "KeycardJanitor",
    "KeycardScientist",
    "KeycardGuard",
    "GunCOM15",
    "GrenadeFlash",
    "GrenadeHE"
  ]);

  const ROLE_OPTIONS = curatedEnumOptions("SetRole", 1, [
    "ClassD",
    "Scientist",
    "FacilityGuard",
    "NtfPrivate",
    "ChaosConscript",
    "Scp173",
    "Scp049",
    "Scp939",
    "Spectator"
  ]);

  const serGenerator = new Blockly.Generator("SER");
  serGenerator.ORDER_ATOMIC = 0;
  serGenerator.ORDER_NONE = 99;
  serGenerator.INDENT = "    ";

  serGenerator.scrub_ = function scrubBlock(block, code, thisOnly) {
    const nextBlock = block.nextConnection?.targetBlock();
    const nextCode = thisOnly || !nextBlock ? "" : this.blockToCode(nextBlock);
    return code + nextCode;
  };

  function quoteSerText(value) {
    return `"${SERLanguageCore.escapeSerText(String(value ?? ""))}"`;
  }

  function unindentRootBody(code) {
    const indent = serGenerator.INDENT;
    return String(code || "")
      .split("\n")
      .map(line => line.startsWith(indent) ? line.slice(indent.length) : line)
      .join("\n");
  }

  function valueCode(generator, block, inputName, fallback = "...") {
    return generator.valueToCode(block, inputName, serGenerator.ORDER_ATOMIC) || fallback;
  }

  function targetFromBlock(block) {
    return ["ser_target", "ser_single_player"].includes(block?.type)
      ? block.getFieldValue("TARGET")
      : null;
  }

  function targetFromInput(block, inputName) {
    return targetFromBlock(block?.getInputTargetBlock(inputName));
  }

  function eventVariables(eventName) {
    return new Set(SERLanguageCore.eventVariableNames(SER_TRUTH_TABLE, eventName));
  }

  function eventTargetsUsed(rootBlock) {
    return [...new Set(
      rootBlock.getDescendants(false)
        .filter(block => ["ser_target", "ser_single_player"].includes(block.type))
        .map(block => block.getFieldValue("TARGET"))
        // Attackers are optional for environmental damage and suicides. Requiring
        // one would skip the section before the beginner's explicit exists-check.
        .filter(value => value?.startsWith("@ev") && value !== "@evAttacker")
    )].sort();
  }

  function statementBlock(block, colour, tooltip) {
    block.setPreviousStatement(true, "SER_ACTION");
    block.setNextStatement(true, "SER_ACTION");
    block.setColour(colour);
    block.setTooltip(tooltip);
  }

  function valueBlock(block, check, colour, tooltip) {
    block.setOutput(true, check);
    block.setColour(colour);
    block.setTooltip(tooltip);
  }

  Blockly.Blocks.ser_when_event = {
    init() {
      this.appendDummyInput()
        .appendField("when")
        .appendField(new Blockly.FieldDropdown(EVENT_OPTIONS), "EVENT");
      this.appendStatementInput("DO")
        .setCheck("SER_ACTION")
        .appendField("do");
      this.setColour(COLOURS.start);
      this.setTooltip("Starts a script when a common server or player event happens.");
    }
  };

  serGenerator.forBlock.ser_when_event = function generateEvent(block, generator) {
    const eventName = block.getFieldValue("EVENT");
    const required = eventTargetsUsed(block);
    const requireLine = required.length > 0 ? `-- require ${required.join(" ")}\n` : "";
    const body = unindentRootBody(generator.statementToCode(block, "DO"));
    return `!-- OnEvent ${eventName}\n${requireLine}${requireLine ? "\n" : ""}${body}`;
  };

  Blockly.Blocks.ser_when_command = {
    init() {
      this.appendDummyInput()
        .appendField("when a player uses")
        .appendField(new Blockly.FieldTextInput("heal"), "COMMAND")
        .appendField("from")
        .appendField(new Blockly.FieldDropdown([
          ["player console", "player"],
          ["Remote Admin", "remoteAdmin"],
          ["server console", "server"]
        ]), "AUDIENCE");
      this.appendStatementInput("DO")
        .setCheck("SER_ACTION")
        .appendField("do");
      this.setColour(COLOURS.start);
      this.setTooltip("Creates a small custom command and runs the attached actions.");
    }
  };

  serGenerator.forBlock.ser_when_command = function generateCommand(block, generator) {
    const command = block.getFieldValue("COMMAND").trim() || "...";
    const audience = block.getFieldValue("AUDIENCE");
    const body = unindentRootBody(generator.statementToCode(block, "DO"));
    return `!-- CustomCommand ${command}\n-- availableFor ${audience}\n\n${body}`;
  };

  Blockly.Blocks.ser_when_manual = {
    init() {
      this.appendDummyInput().appendField("when I run this script manually");
      this.appendStatementInput("DO")
        .setCheck("SER_ACTION")
        .appendField("do");
      this.setColour(COLOURS.start);
      this.setTooltip("Makes a script you start manually from the server or Remote Admin console.");
    }
  };

  serGenerator.forBlock.ser_when_manual = function generateManual(block, generator) {
    return unindentRootBody(generator.statementToCode(block, "DO"));
  };

  Blockly.Blocks.ser_target = {
    init() {
      this.appendDummyInput()
        .appendField("players:")
        .appendField(new Blockly.FieldDropdown(TARGET_OPTIONS), "TARGET");
      valueBlock(this, "SER_PLAYER", COLOURS.players, "Chooses which player or group an action affects.");
    }
  };

  serGenerator.forBlock.ser_target = function generateTarget(block) {
    return [block.getFieldValue("TARGET"), serGenerator.ORDER_ATOMIC];
  };

  Blockly.Blocks.ser_single_player = {
    init() {
      this.appendDummyInput()
        .appendField("one player:")
        .appendField(new Blockly.FieldDropdown(SINGLE_PLAYER_OPTIONS), "TARGET");
      valueBlock(
        this,
        "SER_SINGLE_PLAYER",
        COLOURS.players,
        "Chooses one event or command player as a teleport destination."
      );
    }
  };

  serGenerator.forBlock.ser_single_player = function generateSinglePlayer(block) {
    return [block.getFieldValue("TARGET"), serGenerator.ORDER_ATOMIC];
  };

  Blockly.Blocks.ser_text = {
    init() {
      this.appendDummyInput()
        .appendField("text")
        .appendField(new Blockly.FieldTextInput("Hello!"), "TEXT");
      valueBlock(this, "String", COLOURS.values, "Text shown to a player.");
    }
  };

  serGenerator.forBlock.ser_text = function generateText(block) {
    return [quoteSerText(block.getFieldValue("TEXT")), serGenerator.ORDER_ATOMIC];
  };

  Blockly.Blocks.ser_duration = {
    init() {
      this.appendDummyInput()
        .appendField(new Blockly.FieldNumber(5, 0.1, 3600, 0.1), "AMOUNT")
        .appendField(new Blockly.FieldDropdown([
          ["seconds", "s"],
          ["minutes", "m"]
        ]), "UNIT");
      valueBlock(this, "SER_DURATION", COLOURS.values, "How long something lasts.");
    }
  };

  serGenerator.forBlock.ser_duration = function generateDuration(block) {
    return [
      `${block.getFieldValue("AMOUNT")}${block.getFieldValue("UNIT")}`,
      serGenerator.ORDER_ATOMIC
    ];
  };

  Blockly.Blocks.ser_number = {
    init() {
      this.appendDummyInput()
        .appendField("amount")
        .appendField(new Blockly.FieldNumber(25, 0, 100000, 1), "NUMBER");
      valueBlock(this, "Number", COLOURS.values, "A number used by an action.");
    }
  };

  serGenerator.forBlock.ser_number = function generateNumber(block) {
    return [String(block.getFieldValue("NUMBER")), serGenerator.ORDER_ATOMIC];
  };

  Blockly.Blocks.ser_item = {
    init() {
      this.appendDummyInput()
        .appendField("item")
        .appendField(new Blockly.FieldDropdown(ITEM_OPTIONS), "ITEM");
      valueBlock(this, "SER_ITEM", COLOURS.values, "A common item to give to a player.");
    }
  };

  serGenerator.forBlock.ser_item = function generateItem(block) {
    return [block.getFieldValue("ITEM"), serGenerator.ORDER_ATOMIC];
  };

  Blockly.Blocks.ser_role = {
    init() {
      this.appendDummyInput()
        .appendField("role")
        .appendField(new Blockly.FieldDropdown(ROLE_OPTIONS), "ROLE");
      valueBlock(this, "SER_ROLE", COLOURS.values, "A common player role.");
    }
  };

  serGenerator.forBlock.ser_role = function generateRole(block) {
    return [block.getFieldValue("ROLE"), serGenerator.ORDER_ATOMIC];
  };

  Blockly.Blocks.ser_broadcast = {
    init() {
      this.appendValueInput("TARGET")
        .setCheck("SER_PLAYER")
        .appendField("show a large message to");
      this.appendValueInput("DURATION")
        .setCheck("SER_DURATION")
        .appendField("for");
      this.appendValueInput("MESSAGE")
        .setCheck("String")
        .appendField("saying");
      statementBlock(this, COLOURS.messages, "Shows a large broadcast message to the chosen players.");
    }
  };

  serGenerator.forBlock.ser_broadcast = function generateBroadcast(block, generator) {
    return `Broadcast ${valueCode(generator, block, "TARGET")} ` +
      `${valueCode(generator, block, "DURATION")} ${valueCode(generator, block, "MESSAGE")}\n`;
  };

  Blockly.Blocks.ser_hint = {
    init() {
      this.appendValueInput("TARGET")
        .setCheck("SER_PLAYER")
        .appendField("show a small hint to");
      this.appendValueInput("DURATION")
        .setCheck("SER_DURATION")
        .appendField("for");
      this.appendValueInput("MESSAGE")
        .setCheck("String")
        .appendField("saying");
      statementBlock(this, COLOURS.messages, "Shows a smaller hint message to the chosen players.");
    }
  };

  serGenerator.forBlock.ser_hint = function generateHint(block, generator) {
    return `Hint ${valueCode(generator, block, "TARGET")} ` +
      `${valueCode(generator, block, "DURATION")} ${valueCode(generator, block, "MESSAGE")}\n`;
  };

  Blockly.Blocks.ser_reply = {
    init() {
      this.appendValueInput("MESSAGE")
        .setCheck("String")
        .appendField("reply to the command with");
      statementBlock(this, COLOURS.messages, "Sends a reply back to the place that ran the command.");
    }
  };

  serGenerator.forBlock.ser_reply = function generateReply(block, generator) {
    return `Reply ${valueCode(generator, block, "MESSAGE")}\n`;
  };

  Blockly.Blocks.ser_heal = {
    init() {
      this.appendValueInput("TARGET")
        .setCheck("SER_PLAYER")
        .appendField("heal");
      this.appendValueInput("AMOUNT")
        .setCheck("Number")
        .appendField("by");
      statementBlock(this, COLOURS.actions, "Restores health without going over the player's maximum health.");
    }
  };

  serGenerator.forBlock.ser_heal = function generateHeal(block, generator) {
    return `Heal ${valueCode(generator, block, "TARGET")} ${valueCode(generator, block, "AMOUNT")}\n`;
  };

  Blockly.Blocks.ser_damage = {
    init() {
      this.appendValueInput("TARGET")
        .setCheck("SER_PLAYER")
        .appendField("hurt");
      this.appendValueInput("AMOUNT")
        .setCheck("Number")
        .appendField("by");
      statementBlock(this, COLOURS.actions, "Removes health from the chosen players.");
    }
  };

  serGenerator.forBlock.ser_damage = function generateDamage(block, generator) {
    return `Damage ${valueCode(generator, block, "TARGET")} ${valueCode(generator, block, "AMOUNT")}\n`;
  };

  Blockly.Blocks.ser_kill = {
    init() {
      this.appendValueInput("TARGET")
        .setCheck("SER_PLAYER")
        .appendField("kill");
      statementBlock(this, COLOURS.actions, "Kills the chosen players.");
    }
  };

  serGenerator.forBlock.ser_kill = function generateKill(block, generator) {
    return `Kill ${valueCode(generator, block, "TARGET")}\n`;
  };

  Blockly.Blocks.ser_give_item = {
    init() {
      this.appendValueInput("TARGET")
        .setCheck("SER_PLAYER")
        .appendField("give");
      this.appendValueInput("ITEM")
        .setCheck("SER_ITEM")
        .appendField("the item");
      statementBlock(this, COLOURS.actions, "Gives one common item to the chosen players.");
    }
  };

  serGenerator.forBlock.ser_give_item = function generateGiveItem(block, generator) {
    return `GiveItem ${valueCode(generator, block, "TARGET")} ${valueCode(generator, block, "ITEM")}\n`;
  };

  Blockly.Blocks.ser_set_role = {
    init() {
      this.appendValueInput("TARGET")
        .setCheck("SER_PLAYER")
        .appendField("change");
      this.appendValueInput("ROLE")
        .setCheck("SER_ROLE")
        .appendField("to");
      statementBlock(this, COLOURS.actions, "Changes the selected players to another role.");
    }
  };

  serGenerator.forBlock.ser_set_role = function generateSetRole(block, generator) {
    return `SetRole ${valueCode(generator, block, "TARGET")} ${valueCode(generator, block, "ROLE")}\n`;
  };

  Blockly.Blocks.ser_teleport = {
    init() {
      this.appendValueInput("TARGET")
        .setCheck("SER_PLAYER")
        .appendField("teleport");
      this.appendValueInput("DESTINATION")
        .setCheck("SER_SINGLE_PLAYER")
        .appendField("to");
      statementBlock(this, COLOURS.actions, "Teleports the first players to the second player.");
    }
  };

  serGenerator.forBlock.ser_teleport = function generateTeleport(block, generator) {
    return `TPPlayer ${valueCode(generator, block, "TARGET")} ` +
      `${valueCode(generator, block, "DESTINATION")}\n`;
  };

  Blockly.Blocks.ser_clear_inventory = {
    init() {
      this.appendValueInput("TARGET")
        .setCheck("SER_PLAYER")
        .appendField("clear the inventory of");
      statementBlock(this, COLOURS.actions, "Destroys every item in the chosen players' inventories.");
    }
  };

  serGenerator.forBlock.ser_clear_inventory = function generateClearInventory(block, generator) {
    return `ClearInventory ${valueCode(generator, block, "TARGET")}\n`;
  };

  Blockly.Blocks.ser_if = {
    init() {
      this.appendValueInput("CONDITION")
        .setCheck("Boolean")
        .appendField("if");
      this.appendStatementInput("DO")
        .setCheck("SER_ACTION")
        .appendField("then");
      statementBlock(this, COLOURS.decisions, "Runs the inside blocks only when the condition is true.");
    }
  };

  serGenerator.forBlock.ser_if = function generateIf(block, generator) {
    const condition = valueCode(generator, block, "CONDITION");
    const body = generator.statementToCode(block, "DO");
    return `if ${condition}\n${body}end\n`;
  };

  Blockly.Blocks.ser_player_exists = {
    init() {
      this.appendValueInput("TARGET")
        .setCheck("SER_SINGLE_PLAYER")
        .appendField("player exists");
      valueBlock(
        this,
        "Boolean",
        COLOURS.decisions,
        "Checks that an event player, such as an attacker, is available before using it."
      );
    }
  };

  serGenerator.forBlock.ser_player_exists = function generatePlayerExists(block, generator) {
    return [`{VarExists ${valueCode(generator, block, "TARGET")}}`, serGenerator.ORDER_ATOMIC];
  };

  Blockly.Blocks.ser_player_role = {
    init() {
      this.appendValueInput("TARGET")
        .setCheck("SER_SINGLE_PLAYER")
        .appendField("player");
      this.appendValueInput("ROLE")
        .setCheck("SER_ROLE")
        .appendField("has role");
      valueBlock(this, "Boolean", COLOURS.decisions, "Checks whether a player currently has a role.");
    }
  };

  serGenerator.forBlock.ser_player_role = function generatePlayerRole(block, generator) {
    const target = valueCode(generator, block, "TARGET");
    const role = valueCode(generator, block, "ROLE");
    return [`{${target} -> role} is ${quoteSerText(role)}`, serGenerator.ORDER_ATOMIC];
  };

  Blockly.Blocks.ser_random_chance = {
    init() {
      this.appendDummyInput()
        .appendField("random chance")
        .appendField(new Blockly.FieldNumber(50, 1, 100, 1), "PERCENT")
        .appendField("%");
      valueBlock(this, "Boolean", COLOURS.decisions, "Has the chosen chance to be true each time it is checked.");
    }
  };

  serGenerator.forBlock.ser_random_chance = function generateChance(block) {
    return [`{Chance ${block.getFieldValue("PERCENT")}%}`, serGenerator.ORDER_ATOMIC];
  };

  Blockly.Blocks.ser_repeat = {
    init() {
      this.appendDummyInput()
        .appendField("repeat")
        .appendField(new Blockly.FieldNumber(3, 1, 100, 1), "COUNT")
        .appendField("times");
      this.appendStatementInput("DO")
        .setCheck("SER_ACTION")
        .appendField("do");
      statementBlock(this, COLOURS.timing, "Repeats the inside blocks a small number of times.");
    }
  };

  serGenerator.forBlock.ser_repeat = function generateRepeat(block, generator) {
    const body = generator.statementToCode(block, "DO");
    return `repeat ${block.getFieldValue("COUNT")}\n${body}end\n`;
  };

  Blockly.Blocks.ser_wait = {
    init() {
      this.appendValueInput("DURATION")
        .setCheck("SER_DURATION")
        .appendField("wait");
      statementBlock(this, COLOURS.timing, "Pauses before continuing to the next block.");
    }
  };

  serGenerator.forBlock.ser_wait = function generateWait(block, generator) {
    return `wait ${valueCode(generator, block, "DURATION")}\n`;
  };

  Blockly.Blocks.ser_stop = {
    init() {
      this.appendDummyInput().appendField("stop this script");
      statementBlock(this, COLOURS.timing, "Stops this section immediately.");
    }
  };

  serGenerator.forBlock.ser_stop = function generateStop() {
    return "stop\n";
  };

  function shadow(type, fields = {}) {
    return { shadow: { type, fields } };
  }

  function targetShadow(target = "@all") {
    return shadow("ser_target", { TARGET: target });
  }

  function singlePlayerShadow(target = "@evPlayer") {
    return shadow("ser_single_player", { TARGET: target });
  }

  function durationShadow(amount = 5, unit = "s") {
    return shadow("ser_duration", { AMOUNT: amount, UNIT: unit });
  }

  function textShadow(text = "Hello!") {
    return shadow("ser_text", { TEXT: text });
  }

  function numberShadow(number = 25) {
    return shadow("ser_number", { NUMBER: number });
  }

  function actionToolboxBlock(type, inputs = {}) {
    return { kind: "block", type, inputs };
  }

  const toolbox = {
    kind: "categoryToolbox",
    contents: [
      {
        kind: "category",
        name: "1. When it runs",
        colour: String(COLOURS.start),
        contents: [
          { kind: "label", text: "Every script starts with one of these" },
          { kind: "block", type: "ser_when_event" },
          { kind: "block", type: "ser_when_command" },
          { kind: "block", type: "ser_when_manual" }
        ]
      },
      {
        kind: "category",
        name: "2. Players",
        colour: String(COLOURS.players),
        contents: [
          { kind: "label", text: "Plug a player into an action" },
          { kind: "block", type: "ser_target" }
        ]
      },
      {
        kind: "category",
        name: "3. Messages",
        colour: String(COLOURS.messages),
        contents: [
          actionToolboxBlock("ser_broadcast", {
            TARGET: targetShadow("@all"),
            DURATION: durationShadow(5),
            MESSAGE: textShadow("The round has started!")
          }),
          actionToolboxBlock("ser_hint", {
            TARGET: targetShadow("@evPlayer"),
            DURATION: durationShadow(4),
            MESSAGE: textShadow("Welcome!")
          }),
          actionToolboxBlock("ser_reply", {
            MESSAGE: textShadow("Done!")
          })
        ]
      },
      {
        kind: "category",
        name: "4. Player actions",
        colour: String(COLOURS.actions),
        contents: [
          actionToolboxBlock("ser_heal", {
            TARGET: targetShadow("@evPlayer"),
            AMOUNT: numberShadow(25)
          }),
          actionToolboxBlock("ser_damage", {
            TARGET: targetShadow("@evPlayer"),
            AMOUNT: numberShadow(10)
          }),
          actionToolboxBlock("ser_give_item", {
            TARGET: targetShadow("@evPlayer"),
            ITEM: shadow("ser_item", { ITEM: ITEM_OPTIONS[0][1] })
          }),
          actionToolboxBlock("ser_set_role", {
            TARGET: targetShadow("@evPlayer"),
            ROLE: shadow("ser_role", { ROLE: ROLE_OPTIONS[0][1] })
          }),
          actionToolboxBlock("ser_teleport", {
            TARGET: targetShadow("@evPlayer"),
            DESTINATION: shadow("ser_single_player", { TARGET: "@sender" })
          }),
          actionToolboxBlock("ser_clear_inventory", {
            TARGET: targetShadow("@evPlayer")
          }),
          actionToolboxBlock("ser_kill", {
            TARGET: targetShadow("@evPlayer")
          })
        ]
      },
      {
        kind: "category",
        name: "5. Decisions",
        colour: String(COLOURS.decisions),
        contents: [
          actionToolboxBlock("ser_if", {
            CONDITION: {
              block: {
                type: "ser_player_exists",
                inputs: { TARGET: singlePlayerShadow("@evAttacker") }
              }
            }
          }),
          actionToolboxBlock("ser_player_exists", {
            TARGET: singlePlayerShadow("@evAttacker")
          }),
          actionToolboxBlock("ser_player_role", {
            TARGET: singlePlayerShadow("@evPlayer"),
            ROLE: shadow("ser_role", { ROLE: ROLE_OPTIONS[0][1] })
          }),
          { kind: "block", type: "ser_random_chance" }
        ]
      },
      {
        kind: "category",
        name: "6. Timing",
        colour: String(COLOURS.timing),
        contents: [
          actionToolboxBlock("ser_wait", {
            DURATION: durationShadow(1)
          }),
          { kind: "block", type: "ser_repeat" },
          { kind: "block", type: "ser_stop" }
        ]
      }
    ]
  };

  const SERLearningTheme = Blockly.Theme.defineTheme("ser_learning", {
    base: Blockly.Themes.Classic,
    componentStyles: {
      workspaceBackgroundColour: "#111318",
      toolboxBackgroundColour: "#191c23",
      toolboxForegroundColour: "#f7f8fb",
      flyoutBackgroundColour: "#20242d",
      flyoutForegroundColour: "#f7f8fb",
      flyoutOpacity: 1,
      scrollbarColour: "#5b6272",
      insertionMarkerColour: "#ffffff",
      insertionMarkerOpacity: 0.32,
      cursorColour: "#a99eff",
      markerColour: "#a99eff"
    },
    fontStyle: {
      family: "Inter, Segoe UI, sans-serif",
      weight: "600",
      size: 12
    },
    startHats: true
  });

  const workspace = Blockly.inject("blocklyDiv", {
    toolbox,
    theme: SERLearningTheme,
    media: "__SER_BLOCKLY_MEDIA__",
    renderer: "zelos",
    sounds: false,
    trashcan: true,
    grid: {
      spacing: 24,
      length: 2,
      colour: "#242833",
      snap: false
    },
    move: {
      scrollbars: true,
      drag: true,
      wheel: true
    },
    zoom: {
      controls: true,
      wheel: true,
      startScale: 0.88,
      maxScale: 1.35,
      minScale: 0.52,
      scaleSpeed: 1.1
    }
  });

  const elements = {
    codeOutput: document.getElementById("codeOutput"),
    diagnostics: document.getElementById("diagnostics"),
    statusBadge: document.getElementById("statusBadge"),
    copy: document.getElementById("copyBtn"),
    download: document.getElementById("downloadBtn"),
    scriptName: document.getElementById("scriptName"),
    projectFile: document.getElementById("projectFileInput"),
    recipeDialog: document.getElementById("recipeDialog"),
    recipeGrid: document.getElementById("recipeGrid"),
    lessonTitle: document.getElementById("lessonTitle"),
    lessonText: document.getElementById("lessonText"),
    lessonTip: document.getElementById("lessonTip"),
    emptyButton: document.getElementById("emptyWorkspaceButton"),
    toast: document.getElementById("toast")
  };

  let autosaveTimer = null;
  let toastTimer = null;

  function showToast(message) {
    clearTimeout(toastTimer);
    elements.toast.textContent = message;
    elements.toast.classList.add("visible");
    toastTimer = setTimeout(() => elements.toast.classList.remove("visible"), 1800);
  }

  function safeFilename(value, extension) {
    const base = String(value || "my-first-script")
      .replace(/\.[^.]+$/, "")
      .replace(/[^a-zA-Z0-9_-]+/g, "-")
      .replace(/^-+|-+$/g, "") || "my-first-script";
    return `${base}.${extension}`;
  }

  function downloadText(filename, text, contentType) {
    if (editorHost) {
      editorHost.postMessage({ type: "saveFile", filename, text, contentType });
      return;
    }
    const blob = new Blob([text], { type: contentType });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = filename;
    anchor.click();
    setTimeout(() => URL.revokeObjectURL(url), 0);
  }

  function getWorkspaceState() {
    return {
      format: PROJECT_FORMAT,
      version: PROJECT_VERSION,
      scriptName: elements.scriptName.value,
      workspace: Blockly.serialization.workspaces.save(workspace)
    };
  }

  function loadWorkspaceState(project) {
    if (!project || project.format !== PROJECT_FORMAT || !project.workspace) {
      if (project?.format === "ser-blockly-project") {
        throw new Error(
          "This project was made with the older all-features editor. " +
          "SER Blocks now uses a smaller beginner block set, so that file cannot be opened here."
        );
      }
      throw new Error("This is not a valid SER Blocks project.");
    }
    workspace.clear();
    Blockly.serialization.workspaces.load(project.workspace, workspace);
    elements.scriptName.value = project.scriptName || "my-first-script";
    updateEditor();
  }

  function saveAutosave() {
    try {
      localStorage.setItem(AUTOSAVE_KEY, JSON.stringify(getWorkspaceState()));
    } catch {
      // Browser storage may be unavailable inside a hardened editor host.
    }
  }

  function scheduleAutosave(event) {
    if (event?.isUiEvent) return;
    clearTimeout(autosaveTimer);
    autosaveTimer = setTimeout(saveAutosave, 250);
  }

  function entryAncestor(block) {
    let current = block;
    while (current) {
      if (ENTRY_TYPES.has(current.type)) return current;
      current = current.getParent();
    }
    return null;
  }

  function isInsideExistsCondition(block, target) {
    let current = block;
    while (current) {
      if (current.type === "ser_player_exists" && targetFromInput(current, "TARGET") === target) {
        return true;
      }
      current = current.getParent();
    }
    return false;
  }

  function isProtectedByExists(block, target) {
    let current = block.getParent();
    while (current) {
      if (current.type === "ser_if") {
        const condition = current.getInputTargetBlock("CONDITION");
        if (condition?.type === "ser_player_exists" &&
            targetFromInput(condition, "TARGET") === target) {
          return true;
        }
      }
      current = current.getParent();
    }
    return false;
  }

  function workspaceDiagnostics(generatedCode) {
    const diagnostics = SERLanguageCore.validateGeneratedCode(generatedCode);
    const blocks = workspace.getAllBlocks(false);
    const entries = blocks.filter(block => ENTRY_TYPES.has(block.type));

    if (blocks.length > 0 && entries.length === 0) {
      diagnostics.push({
        severity: "error",
        code: "missing-entry",
        message: "Add a purple “When” block, then connect your actions inside it."
      });
    }

    for (const entry of entries) {
      if (!entry.getInputTargetBlock("DO")) {
        diagnostics.push({
          severity: "error",
          code: "empty-entry",
          message: "This “When” block has nothing to do yet. Add an action inside it."
        });
      }

      if (entry.type === "ser_when_command") {
        const command = entry.getFieldValue("COMMAND").trim();
        if (!/^[a-zA-Z0-9_-]+$/.test(command)) {
          diagnostics.push({
            severity: "error",
            code: "invalid-command-name",
            message: "Command names can use only letters, numbers, - and _."
          });
        }
      }
    }

    for (const block of blocks) {
      const entry = entryAncestor(block);
      if ((ACTION_TYPES.has(block.type) || block.outputConnection) &&
          !entry && !block.getParent()) {
        diagnostics.push({
          severity: "error",
          code: "disconnected-block",
          message: `Connect the loose “${block.toString(28, "…")}” block to a purple “When” block.`
        });
      }

      if (block.type === "ser_reply" && entry?.type !== "ser_when_command") {
        diagnostics.push({
          severity: "warning",
          code: "reply-outside-command",
          message: "“Reply” is clearest inside a command. Use a hint or broadcast for event scripts."
        });
      }

      if (!["ser_target", "ser_single_player"].includes(block.type)) continue;
      const target = block.getFieldValue("TARGET");
      if (!entry) continue;

      if (entry.type === "ser_when_event") {
        const eventName = entry.getFieldValue("EVENT");
        if (target === "@sender") {
          diagnostics.push({
            severity: "error",
            code: "sender-in-event",
            message: "“The player who used the command” only exists in a command script."
          });
        }
        if (target.startsWith("@ev") && !eventVariables(eventName).has(target)) {
          diagnostics.push({
            severity: "error",
            code: "event-player-unavailable",
            message: `${TARGET_LABELS[target]} is not provided when ${EVENT_LABELS[eventName]}.`
          });
        }
        if (target === "@evAttacker" &&
            !isInsideExistsCondition(block, target) &&
            !isProtectedByExists(block, target)) {
          diagnostics.push({
            severity: "warning",
            code: "unguarded-attacker",
            message: "An attacker is sometimes missing. Put attacker actions inside “if player exists”."
          });
        }
      } else if (target.startsWith("@ev")) {
        diagnostics.push({
          severity: "error",
          code: "event-player-outside-event",
          message: `${TARGET_LABELS[target]} only exists in an event script.`
        });
      } else if (target === "@sender" &&
          entry.type === "ser_when_command" &&
          entry.getFieldValue("AUDIENCE") === "server") {
        diagnostics.push({
          severity: "error",
          code: "sender-in-server-command",
          message: "A server-console command has no player sender. Choose a player target instead."
        });
      } else if (target === "@sender" && entry.type === "ser_when_manual") {
        diagnostics.push({
          severity: "warning",
          code: "sender-in-manual-script",
          message: "A manually run script has a player sender only when a player or admin starts it."
        });
      }
    }

    const unique = [];
    const seen = new Set();
    for (const diagnostic of diagnostics) {
      const key = `${diagnostic.code}:${diagnostic.line || ""}:${diagnostic.message}`;
      if (seen.has(key)) continue;
      seen.add(key);
      unique.push(diagnostic);
    }
    return unique;
  }

  function renderDiagnostics(diagnostics, hasCode) {
    elements.diagnostics.replaceChildren();
    const errors = diagnostics.filter(item => item.severity === "error");
    const warnings = diagnostics.filter(item => item.severity === "warning");
    const visibleDiagnostics = diagnostics.length === 0 && hasCode
      ? [{
          severity: "success",
          code: "ready",
          message: "Everything is connected. This script is ready to download."
        }]
      : diagnostics;

    for (const diagnostic of visibleDiagnostics) {
      const item = document.createElement("li");
      item.dataset.severity = diagnostic.severity;
      item.textContent = `${diagnostic.line ? `Line ${diagnostic.line}: ` : ""}${diagnostic.message}`;
      elements.diagnostics.appendChild(item);
    }

    const canExport = hasCode && errors.length === 0;
    elements.copy.disabled = !canExport;
    elements.download.disabled = !canExport;

    if (errors.length > 0) {
      elements.statusBadge.dataset.state = "error";
      elements.statusBadge.textContent = `${errors.length} to fix`;
    } else if (warnings.length > 0) {
      elements.statusBadge.dataset.state = "warning";
      elements.statusBadge.textContent = `${warnings.length} tip${warnings.length === 1 ? "" : "s"}`;
    } else if (hasCode) {
      elements.statusBadge.dataset.state = "valid";
      elements.statusBadge.textContent = "Ready";
    } else {
      elements.statusBadge.dataset.state = "";
      elements.statusBadge.textContent = "Start here";
    }
  }

  const LESSONS = {
    ser_when_event: {
      title: block => `When ${EVENT_LABELS[block.getFieldValue("EVENT")]}`,
      text: block => SER_TRUTH_TABLE.eventDetails?.[block.getFieldValue("EVENT")]?.description ||
        CURATED_EVENTS.find(event => event.value === block.getFieldValue("EVENT"))?.fallback,
      tip: "Only the actions connected inside this block run for the event."
    },
    ser_when_command: {
      title: "Make a player command",
      text: "The word in this block becomes a real custom SER command. The attached actions run when someone uses it.",
      tip: "Use “the player who used the command” to affect the person who typed it."
    },
    ser_when_manual: {
      title: "Run it when you choose",
      text: "This script has no automatic trigger. You run it manually from the server or Remote Admin console.",
      tip: "This is useful for announcements and one-off server actions."
    },
    ser_target: {
      title: block => TARGET_LABELS[block.getFieldValue("TARGET")],
      text: "A player target tells an action who should receive it or be changed by it.",
      tip: "Event players work only with compatible event starts; command users work with command starts."
    },
    ser_single_player: {
      title: block => TARGET_LABELS[block.getFieldValue("TARGET")],
      text: "Teleporting needs one destination player, so this block offers only event and command players.",
      tip: "Groups such as “all players” cannot be a teleport destination."
    },
    ser_broadcast: {
      title: "Show a large message",
      text: "This becomes SER’s Broadcast method. It shows a prominent message to the chosen players for a set time.",
      tip: "Use this for announcements. Use a hint for quieter, personal feedback."
    },
    ser_hint: {
      title: "Show a small hint",
      text: "This becomes SER’s Hint method. It shows a smaller message to the chosen players.",
      tip: "Hints work well for feedback that should not cover much of the screen."
    },
    ser_reply: {
      title: "Reply to a command",
      text: "This becomes SER’s Reply method and sends text back to the console that used a command.",
      tip: "Keep this inside a custom command script."
    },
    ser_heal: {
      title: "Restore health",
      text: "This becomes SER’s Heal method. It adds health without going over the player’s maximum.",
      tip: "Choose the command user for a self-heal command."
    },
    ser_damage: {
      title: "Deal damage",
      text: "This becomes SER’s Damage method and removes the chosen amount of health.",
      tip: "Start with a small amount while testing."
    },
    ser_kill: {
      title: "Kill a player",
      text: "This becomes SER’s Kill method and immediately kills every selected player.",
      tip: "Be careful when the target is “all players”."
    },
    ser_give_item: {
      title: "Give an item",
      text: "This becomes SER’s GiveItem method and adds one selected item to the player’s inventory.",
      tip: "The editor shows a short list of useful items, not every item SER supports."
    },
    ser_set_role: {
      title: "Change a role",
      text: "This becomes SER’s SetRole method and respawns the selected player as another role.",
      tip: "Role changes can also reset inventory and move the player."
    },
    ser_teleport: {
      title: "Teleport to another player",
      text: "This becomes SER’s TPPlayer method. The first player is moved to the second player.",
      tip: "The destination should usually be one specific event or command player."
    },
    ser_clear_inventory: {
      title: "Remove inventory items",
      text: "This becomes SER’s ClearInventory method and destroys the selected players’ held items.",
      tip: "This cannot be undone by the script."
    },
    ser_if: {
      title: "Make a decision",
      text: "The inside actions run only when the green condition is true.",
      tip: "Use this to check that an attacker exists or that a player has a certain role."
    },
    ser_player_exists: {
      title: "Check that a player exists",
      text: "Some events do not always have an attacker. This check prevents the script from using a missing player.",
      tip: "Put the action inside the matching “if” block."
    },
    ser_player_role: {
      title: "Check a player’s role",
      text: "This condition is true only while the selected player has the chosen role.",
      tip: "Plug it into the green space on an “if” block."
    },
    ser_random_chance: {
      title: "Add a random chance",
      text: "This condition is true only for the chosen percentage of attempts.",
      tip: "A 25% chance means roughly one out of four times over many runs."
    },
    ser_repeat: {
      title: "Repeat a few times",
      text: "The blocks inside run the selected number of times.",
      tip: "Add a wait inside when repeated messages should not appear all at once."
    },
    ser_wait: {
      title: "Pause before continuing",
      text: "SER waits for this duration, then moves to the next connected block.",
      tip: "Waits are useful between messages or repeated actions."
    },
    ser_stop: {
      title: "Stop this run",
      text: "SER stops this script section immediately and ignores blocks after this one.",
      tip: "Use it after a condition when the rest of the script should not run."
    },
    ser_text: {
      title: "The words players see",
      text: "Write the message exactly as you want it to appear.",
      tip: "SER’s generated quote and escape syntax is handled for you."
    },
    ser_duration: {
      title: "How long it lasts",
      text: "Choose a number and seconds or minutes.",
      tip: "Most player messages are comfortable at about 3–8 seconds."
    },
    ser_number: {
      title: "Choose an amount",
      text: "This number controls health restored, damage dealt, or another action amount.",
      tip: "Use small values first when testing gameplay changes."
    },
    ser_item: {
      title: "Choose a common item",
      text: "This short list covers useful beginner rewards and equipment.",
      tip: "SER supports more items when you move on to editing code."
    },
    ser_role: {
      title: "Choose a common role",
      text: "This short list covers the roles beginners most often use.",
      tip: "SER supports more roles when you move on to editing code."
    }
  };

  function showLesson(block) {
    if (!block) {
      const hasBlocks = workspace.getAllBlocks(false).length > 0;
      elements.lessonTitle.textContent = hasBlocks
        ? "Choose a block to learn what it does"
        : "Start with a “When” block";
      elements.lessonText.textContent = hasBlocks
        ? "Click any block in the workspace and its plain-language meaning will appear here."
        : "Every script needs a starting point: an event, a player command, or a manual run.";
      elements.lessonTip.textContent = hasBlocks
        ? "The panel below shows the real SER code your blocks create."
        : "Choose an idea for a complete example you can change.";
      return;
    }

    const lesson = LESSONS[block.type];
    if (!lesson) return;
    elements.lessonTitle.textContent = typeof lesson.title === "function"
      ? lesson.title(block)
      : lesson.title;
    elements.lessonText.textContent = typeof lesson.text === "function"
      ? lesson.text(block)
      : lesson.text;
    elements.lessonTip.textContent = typeof lesson.tip === "function"
      ? lesson.tip(block)
      : lesson.tip;
  }

  function updateEditor() {
    let generatedCode = "";
    try {
      generatedCode = serGenerator.workspaceToCode(workspace).trimEnd();
    } catch (error) {
      elements.codeOutput.value = "# The blocks are still being connected…";
      renderDiagnostics([{
        severity: "error",
        code: "generation-error",
        message: error.message || "The script could not be generated."
      }], false);
      return;
    }

    elements.codeOutput.value = generatedCode ||
      "# Choose an idea or drag in a purple “When” block to begin.";
    const diagnostics = workspaceDiagnostics(generatedCode);
    renderDiagnostics(diagnostics, Boolean(generatedCode.trim()));
    elements.emptyButton.hidden = workspace.getAllBlocks(false).length > 0;
  }

  function newBlock(type, fields = {}, isShadow = false) {
    const block = workspace.newBlock(type);
    for (const [name, value] of Object.entries(fields)) {
      block.setFieldValue(String(value), name);
    }
    block.initSvg();
    block.render();
    block.setShadow(isShadow);
    return block;
  }

  function plug(parent, inputName, child) {
    parent.getInput(inputName).connection.connect(child.outputConnection);
    return child;
  }

  function attach(parent, inputName, firstStatement) {
    parent.getInput(inputName).connection.connect(firstStatement.previousConnection);
    return firstStatement;
  }

  function connectNext(first, second) {
    first.nextConnection.connect(second.previousConnection);
    return second;
  }

  function targetValue(value) {
    return newBlock("ser_target", { TARGET: value }, true);
  }

  function singlePlayerValue(value) {
    return newBlock("ser_single_player", { TARGET: value }, true);
  }

  function textValue(value) {
    return newBlock("ser_text", { TEXT: value }, true);
  }

  function durationValue(amount, unit = "s") {
    return newBlock("ser_duration", { AMOUNT: amount, UNIT: unit }, true);
  }

  function numberValue(number) {
    return newBlock("ser_number", { NUMBER: number }, true);
  }

  function broadcast(target, seconds, message) {
    const block = newBlock("ser_broadcast");
    plug(block, "TARGET", targetValue(target));
    plug(block, "DURATION", durationValue(seconds));
    plug(block, "MESSAGE", textValue(message));
    return block;
  }

  function hint(target, seconds, message) {
    const block = newBlock("ser_hint");
    plug(block, "TARGET", targetValue(target));
    plug(block, "DURATION", durationValue(seconds));
    plug(block, "MESSAGE", textValue(message));
    return block;
  }

  function loadRecipe(recipe) {
    workspace.clear();
    recipe.build();
    elements.scriptName.value = recipe.filename;
    workspace.cleanUp();
    workspace.zoomToFit();
    elements.recipeDialog.close();
    saveAutosave();
    updateEditor();
    showLesson(null);
    showToast(`Loaded “${recipe.title}”. Change any block to make it yours.`);
  }

  const RECIPES = [
    {
      id: "welcome",
      icon: "👋",
      title: "Welcome a joining player",
      description: "Greet each new player with a large welcome and a small helpful hint.",
      filename: "welcome-players",
      build() {
        const root = newBlock("ser_when_event", { EVENT: "Joined" });
        const greeting = broadcast("@evPlayer", 8, "Welcome to the server!");
        const help = hint("@evPlayer", 5, "Have fun and stay safe.");
        attach(root, "DO", greeting);
        connectNext(greeting, help);
        root.moveBy(50, 45);
      }
    },
    {
      id: "round",
      icon: "📣",
      title: "Announce the round start",
      description: "Tell everyone when a new round begins, then show a second message.",
      filename: "round-announcement",
      build() {
        const root = newBlock("ser_when_event", { EVENT: "RoundStarted" });
        const first = broadcast("@all", 6, "The round has started. Good luck!");
        const wait = newBlock("ser_wait");
        plug(wait, "DURATION", durationValue(6));
        const second = hint("@all", 4, "Work with your team.");
        attach(root, "DO", first);
        connectNext(first, wait);
        connectNext(wait, second);
        root.moveBy(50, 45);
      }
    },
    {
      id: "heal-command",
      icon: "✚",
      title: "Make a self-heal command",
      description: "Let a player type a command to recover health and receive confirmation.",
      filename: "heal-command",
      build() {
        const root = newBlock("ser_when_command", {
          COMMAND: "heal",
          AUDIENCE: "player"
        });
        const heal = newBlock("ser_heal");
        plug(heal, "TARGET", targetValue("@sender"));
        plug(heal, "AMOUNT", numberValue(35));
        const feedback = hint("@sender", 4, "You recovered 35 health.");
        const reply = newBlock("ser_reply");
        plug(reply, "MESSAGE", textValue("You were healed."));
        attach(root, "DO", heal);
        connectNext(heal, feedback);
        connectNext(feedback, reply);
        root.moveBy(50, 45);
      }
    },
    {
      id: "reward",
      icon: "★",
      title: "Reward a player after a kill",
      description: "Safely check for an attacker, give them a coin, and show a message.",
      filename: "kill-reward",
      build() {
        const root = newBlock("ser_when_event", { EVENT: "Death" });
        const ifBlock = newBlock("ser_if");
        const exists = newBlock("ser_player_exists");
        plug(exists, "TARGET", singlePlayerValue("@evAttacker"));
        plug(ifBlock, "CONDITION", exists);
        const give = newBlock("ser_give_item");
        plug(give, "TARGET", targetValue("@evAttacker"));
        plug(give, "ITEM", newBlock("ser_item", { ITEM: "Coin" }, true));
        const feedback = hint("@evAttacker", 4, "You earned a coin!");
        attach(ifBlock, "DO", give);
        connectNext(give, feedback);
        attach(root, "DO", ifBlock);
        root.moveBy(50, 45);
      }
    }
  ];

  for (const recipe of RECIPES) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = "recipe-card";
    button.dataset.recipe = recipe.id;
    const icon = document.createElement("span");
    icon.className = "recipe-icon";
    icon.setAttribute("aria-hidden", "true");
    icon.textContent = recipe.icon;
    const title = document.createElement("strong");
    title.textContent = recipe.title;
    const description = document.createElement("small");
    description.textContent = recipe.description;
    button.append(icon, title, description);
    button.addEventListener("click", () => loadRecipe(recipe));
    elements.recipeGrid.appendChild(button);
  }

  function openRecipes() {
    if (typeof elements.recipeDialog.showModal === "function") {
      elements.recipeDialog.showModal();
    } else {
      elements.recipeDialog.setAttribute("open", "");
    }
  }

  function clearWorkspace(askFirst = true) {
    if (askFirst && workspace.getAllBlocks(false).length > 0 &&
        !window.confirm("Clear these blocks and start a new script?")) {
      return;
    }
    workspace.clear();
    elements.scriptName.value = "my-first-script";
    saveAutosave();
    updateEditor();
    showLesson(null);
  }

  document.getElementById("recipesBtn").addEventListener("click", openRecipes);
  elements.emptyButton.addEventListener("click", openRecipes);
  document.getElementById("closeRecipesBtn").addEventListener("click", () => {
    elements.recipeDialog.close();
  });
  document.getElementById("blankRecipeBtn").addEventListener("click", () => {
    clearWorkspace(false);
    elements.recipeDialog.close();
  });
  elements.recipeDialog.addEventListener("click", event => {
    if (event.target === elements.recipeDialog) elements.recipeDialog.close();
  });

  document.getElementById("newBtn").addEventListener("click", () => clearWorkspace(true));

  document.getElementById("saveProjectBtn").addEventListener("click", () => {
    downloadText(
      safeFilename(elements.scriptName.value, "ser.blocks.json"),
      JSON.stringify(getWorkspaceState(), null, 2),
      "application/json"
    );
    showToast("Saved an editable blocks project.");
  });

  document.getElementById("loadProjectBtn").addEventListener("click", () => {
    elements.projectFile.click();
  });

  elements.projectFile.addEventListener("change", async () => {
    const file = elements.projectFile.files?.[0];
    if (!file) return;
    try {
      loadWorkspaceState(JSON.parse(await file.text()));
      showToast("Opened your blocks project.");
    } catch (error) {
      window.alert(error.message || "Could not open this blocks project.");
    } finally {
      elements.projectFile.value = "";
    }
  });

  elements.copy.addEventListener("click", async () => {
    if (elements.copy.disabled) return;
    try {
      await navigator.clipboard.writeText(elements.codeOutput.value);
    } catch {
      elements.codeOutput.focus();
      elements.codeOutput.select();
      document.execCommand("copy");
    }
    showToast("Copied the SER code.");
  });

  elements.download.addEventListener("click", () => {
    if (elements.download.disabled) return;
    downloadText(
      safeFilename(elements.scriptName.value, "ser"),
      elements.codeOutput.value,
      "text/plain;charset=utf-8"
    );
    showToast("Downloaded a real .ser script.");
  });

  elements.scriptName.addEventListener("input", scheduleAutosave);
  window.addEventListener("resize", () => Blockly.svgResize(workspace));

  workspace.addChangeListener(event => {
    updateEditor();
    scheduleAutosave(event);
    if (event.type === Blockly.Events.SELECTED) {
      showLesson(event.newElementId ? workspace.getBlockById(event.newElementId) : null);
    } else if (!event.isUiEvent) {
      const selected = Blockly.getSelected?.();
      if (selected) showLesson(selected);
    }
  });

  let restored = false;
  try {
    const saved = JSON.parse(localStorage.getItem(AUTOSAVE_KEY));
    if (saved?.workspace) {
      loadWorkspaceState(saved);
      restored = true;
    }
  } catch {
    // Ignore corrupt or blocked browser storage and begin with the recipe chooser.
  }

  updateEditor();
  showLesson(null);
  if (!restored || workspace.getAllBlocks(false).length === 0) {
    setTimeout(openRecipes, 0);
  }
})();
