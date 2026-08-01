import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';

const websiteDirectory = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const repositoryDirectory = path.resolve(websiteDirectory, '..');
const sourceDocsDirectory = path.join(repositoryDirectory, 'docs');
const generatedDocsDirectory = path.join(websiteDirectory, '.site-docs');
const staticDirectory = path.join(websiteDirectory, 'static');
const manifest = JSON.parse(
  fs.readFileSync(path.join(websiteDirectory, 'data', 'ser-truth-table.json'), 'utf8'),
);

function normalize(value) {
  return String(value ?? '').replaceAll('\r\n', '\n').trim();
}

function prose(value) {
  return normalize(value)
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('{', '&#123;')
    .replaceAll('}', '&#125;')
    .replaceAll('\n', '<br />');
}

function cell(value) {
  return prose(value).replaceAll('|', '\\|');
}

function inlineCode(value) {
  const text = normalize(value).replaceAll('`', '\\`');
  return `\`${text}\``;
}

function slug(value) {
  return normalize(value)
    .toLocaleLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-|-$/g, '');
}

function write(relativeFilename, content) {
  const filename = path.join(generatedDocsDirectory, relativeFilename);
  fs.mkdirSync(path.dirname(filename), {recursive: true});
  fs.writeFileSync(filename, `${content.trim()}\n`);
}

function frontMatter(title, extra = {}) {
  const fields = {title, ...extra};
  return `---\n${Object.entries(fields)
    .map(([key, value]) => `${key}: ${JSON.stringify(value)}`)
    .join('\n')}\n---\n`;
}

function relativeDocLink(fromRelativeFilename, toRelativeFilename) {
  let result = path.posix.relative(
    path.posix.dirname(fromRelativeFilename.replaceAll('\\', '/')),
    toRelativeFilename.replaceAll('\\', '/'),
  );
  if (!result.startsWith('.')) result = `./${result}`;
  return result;
}

function rewriteRepositoryLinks(content, relativeFilename) {
  let rewritten = content.replace(
    /(?:\.\.\/)+language_specification\.md/g,
    relativeDocLink(relativeFilename, 'language-specification.md'),
  );

  rewritten = rewritten.replace(
    /(?:\.\.\/)+Example%20Scripts(?:\/([^\s)#]+))?/g,
    (_match, exampleFilename) => {
      const target = exampleFilename
        ? `examples/${slug(decodeURIComponent(exampleFilename).replace(/\.(?:ser|txt)$/i, ''))}.md`
        : 'examples/index.md';
      return relativeDocLink(relativeFilename, target);
    },
  );

  return rewritten
    .split(/(```[\s\S]*?```)/g)
    .map((segment, index) => index % 2 === 0 ? segment.replaceAll('<br>', '<br />') : segment)
    .join('');
}

function copySourceDocs() {
  for (const entry of fs.readdirSync(sourceDocsDirectory, {recursive: true, withFileTypes: true})) {
    if (!entry.isFile() || !entry.name.endsWith('.md') || entry.name === 'SUMMARY.md') continue;
    const sourceFilename = path.join(entry.parentPath, entry.name);
    const relativeFilename = path.relative(sourceDocsDirectory, sourceFilename);
    let content = fs.readFileSync(sourceFilename, 'utf8');
    content = rewriteRepositoryLinks(content, relativeFilename);
    if (relativeFilename.replaceAll('\\', '/') === 'README.md') {
      content = `---\nslug: /\nsidebar_label: Start here\n---\n\n${content}`;
    }
    write(relativeFilename, content);
  }
}

function category(directory, label, position) {
  const target = path.join(generatedDocsDirectory, directory, '_category_.json');
  fs.mkdirSync(path.dirname(target), {recursive: true});
  fs.writeFileSync(target, `${JSON.stringify({label, position, collapsed: true}, null, 2)}\n`);
}

function methodPage(name, method, exampleNames) {
  const parts = [
    frontMatter(name, {sidebar_label: name}),
    method.requiredFramework
      ? `:::note Optional integration\nThis method requires **${prose(method.requiredFramework)}**.\n:::\n`
      : '',
    prose(method.description) || 'No description is available.',
    '',
    '## Syntax',
    '',
    `\`\`\`ser\n${normalize(method.syntax || name)}\n\`\`\``,
    '',
  ];

  if (method.returns) {
    parts.push('## Returns', '', prose(method.returns), '');
  }

  if (method.arguments?.length) {
    parts.push('## Arguments', '', '| Name | Required | Type | Description |', '|---|:---:|---|---|');
    for (const argument of method.arguments) {
      const fallbackDescription = argument.options?.length
        ? `Options: ${argument.options.map(option => inlineCode(option.value)).join(', ')}`
        : '';
      parts.push(
        `| ${inlineCode(argument.name)} | ${argument.mustBeProvided ? 'Yes' : 'No'} | ${cell(argument.type || argument.argumentKind)} | ${cell(argument.description || fallbackDescription || argument.defaultValue && `Default: ${argument.defaultValue}`)} |`,
      );
    }
    parts.push('');
  }

  if (method.additionalDescription) {
    parts.push('## Details', '', prose(method.additionalDescription), '');
  }

  if (method.errors?.length) {
    parts.push('## Possible errors', '', ...method.errors.map(error => `- ${prose(error)}`), '');
  }

  if (exampleNames.length) {
    parts.push(
      '## Validated examples using this method',
      '',
      ...exampleNames.map(example => `- [${example}](../../examples/${slug(example)}.md)`),
      '',
    );
  }

  parts.push(
    '## On a running server',
    '',
    `Use ${inlineCode(`serhelp ${name}`)} to inspect the reference generated by your installed SER version.`,
  );
  return parts.filter(part => part !== null && part !== undefined).join('\n');
}

function eventPage(name, details, pmer, exampleNames) {
  const flag = pmer ? 'OnPMER' : 'OnEvent';
  const parts = [
    frontMatter(name, {sidebar_label: name}),
    prose(details?.description) || `${pmer ? 'ProjectMER' : 'Game'} event exposed to SER scripts.`,
    '',
    '## Start a script section',
    '',
    `\`\`\`ser\n!-- ${flag} ${name}\n${details?.variables?.length ? `-- require ${details.variables.map(variable => variable.name).join(' ')}\n` : ''}\n# Add your instructions here\n\`\`\``,
    '',
    `- **Group:** ${prose(details?.group || 'Other')}`,
    `- **Cancellable:** ${details?.isCancellable ? 'Yes' : 'No'}`,
  ];

  if (details?.eventDataType) {
    parts.push(`- **Event data:** ${inlineCode(details.eventDataType)}`);
  }

  if (details?.variables?.length) {
    parts.push('', '## Injected variables', '', '| Variable | Type | Description |', '|---|---|---|');
    for (const variable of details.variables) {
      parts.push(`| ${inlineCode(variable.name)} | ${cell(variable.type)} | ${cell(variable.description)} |`);
    }
  } else {
    parts.push('', 'This event does not inject any variables.');
  }

  if (exampleNames.length) {
    parts.push(
      '',
      '## Validated examples using this event',
      '',
      ...exampleNames.map(example => `- [${example}](../../examples/${slug(example)}.md)`),
    );
  }

  return parts.join('\n');
}

function flagPage(name, flag) {
  const parts = [
    frontMatter(name, {sidebar_label: name}),
    prose(flag.description),
    '',
    '## Syntax',
    '',
    `\`\`\`ser\n${normalize(flag.syntax)}\n\`\`\``,
  ];

  if (flag.inlineArgument) {
    parts.push(
      '',
      '## Inline argument',
      '',
      `- **Name:** ${inlineCode(flag.inlineArgument.name)}`,
      `- **Required:** ${flag.inlineArgument.required ? 'Yes' : 'No'}`,
      `- **Description:** ${prose(flag.inlineArgument.description)}`,
    );
    if (flag.inlineArgument.example) {
      parts.push('', `\`\`\`ser\n${normalize(flag.inlineArgument.example)}\n\`\`\``);
    }
  }

  if (flag.arguments?.length) {
    parts.push('', '## Options', '');
    for (const argument of flag.arguments) {
      parts.push(
        `### ${inlineCode(`-- ${argument.name}`)}`,
        '',
        prose(argument.description),
        '',
        `**Required:** ${argument.required ? 'Yes' : 'No'}`,
      );
      if (argument.example) parts.push('', `\`\`\`ser\n${normalize(argument.example)}\n\`\`\``);
      parts.push('');
    }
  }

  return parts.join('\n');
}

function keywordPage(name, keyword) {
  const parts = [
    frontMatter(name, {sidebar_label: name}),
    prose(keyword.description),
    '',
    '## Syntax',
    '',
    `\`\`\`ser\n${normalize(keyword.syntax)}\n\`\`\``,
  ];
  if (keyword.example) {
    parts.push('', '## Example', '', `\`\`\`ser\n${normalize(keyword.example)}\n\`\`\``);
  }
  return parts.join('\n');
}

function variablePage(variable) {
  return [
    frontMatter(variable.fullName, {sidebar_label: variable.fullName}),
    `A predefined ${prose(variable.type)} available as ${inlineCode(variable.fullName)}.`,
    '',
    `- **Prefix:** ${inlineCode(variable.prefix)}`,
    `- **Category:** ${prose(variable.category || 'Other')}`,
    '',
    '## Example',
    '',
    `\`\`\`ser\nPrint ${variable.prefix === '@' ? `{AmountOf ${variable.fullName}}` : variable.fullName}\n\`\`\``,
  ].join('\n');
}

function exampleUsage(content) {
  const methods = Object.keys(manifest.methods).filter(name => {
    const escaped = name.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    return new RegExp(`(?:^|[^A-Za-z0-9_.])${escaped}(?:$|[^A-Za-z0-9_.])`, 'm').test(content);
  });
  const events = [...content.matchAll(/^\s*!--\s+OnEvent\s+(\S+)/gim)].map(match => match[1]);
  const pmerEvents = [...content.matchAll(/^\s*!--\s+OnPMER\s+(\S+)/gim)].map(match => match[1]);
  const flags = [...content.matchAll(/^\s*!--\s+(\S+)/gim)].map(match => match[1]);
  return {
    methods: [...new Set(methods)].sort(),
    events: [...new Set(events)].sort(),
    pmerEvents: [...new Set(pmerEvents)].sort(),
    flags: [...new Set(flags)].sort(),
  };
}

function buildExamples() {
  const sourceDirectory = path.join(repositoryDirectory, 'Example Scripts');
  const filenames = fs.readdirSync(sourceDirectory)
    .filter(filename => /\.(?:ser|txt)$/i.test(filename))
    .sort((a, b) => a.localeCompare(b));
  const examples = filenames.map(filename => {
    const content = fs.readFileSync(path.join(sourceDirectory, filename), 'utf8').trimEnd();
    return {filename, name: filename.replace(/\.(?:ser|txt)$/i, ''), content, usage: exampleUsage(content)};
  });

  const methodExamples = new Map();
  const eventExamples = new Map();
  const pmerEventExamples = new Map();
  for (const example of examples) {
    for (const name of example.usage.methods) {
      if (!methodExamples.has(name)) methodExamples.set(name, []);
      methodExamples.get(name).push(example.name);
    }
    for (const name of example.usage.events) {
      if (!eventExamples.has(name)) eventExamples.set(name, []);
      eventExamples.get(name).push(example.name);
    }
    for (const name of example.usage.pmerEvents) {
      if (!pmerEventExamples.has(name)) pmerEventExamples.set(name, []);
      pmerEventExamples.get(name).push(example.name);
    }
  }

  const indexRows = [];
  for (const example of examples) {
    const uses = [
      ...example.usage.methods.slice(0, 5),
      ...example.usage.events.slice(0, 2),
    ].map(inlineCode).join(', ');
    const links = [
      ...example.usage.methods.map(name => `[${name}](../reference/methods/${slug(name)}.md)`),
      ...example.usage.events.map(name => `[${name}](../reference/events/${slug(name)}.md)`),
      ...example.usage.pmerEvents.map(name => `[${name}](../reference/pmer-events/${slug(name)}.md)`),
      ...example.usage.flags.filter(name => manifest.flags[name]).map(name => `[${name}](../reference/flags/${slug(name)}.md)`),
    ];
    write(`examples/${slug(example.name)}.md`, [
      frontMatter(example.name, {sidebar_label: example.name}),
      `[Download the raw ${inlineCode(example.filename)} file](https://raw.githubusercontent.com/ScriptedEvents/ScriptedEventsReloaded/main/Example%20Scripts/${encodeURIComponent(example.filename)})`,
      '',
      links.length ? `**Uses:** ${[...new Set(links)].join(' · ')}` : '',
      '',
      '## Complete script',
      '',
      `\`\`\`ser title="${example.filename}"\n${example.content}\n\`\`\``,
      '',
      'This file is compiled during the SER build and is safe to use as a current-syntax learning example.',
    ].filter(Boolean).join('\n'));
    indexRows.push(`| [${example.name}](./${slug(example.name)}.md) | ${uses || 'Language fundamentals'} |`);
  }

  write('examples/index.md', [
    frontMatter('Build-validated example scripts', {slug: '/examples/', sidebar_label: 'All examples'}),
    'These complete scripts are generated from the repository’s `Example Scripts` directory. Every file is compiled during a normal SER build.',
    '',
    '[Open SER Blocks](pathname:///editor/) to assemble a beginner script visually, or copy an example below into a `.ser` file.',
    '',
    '| Example | Selected constructs |',
    '|---|---|',
    ...indexRows,
  ].join('\n'));

  return {examples, methodExamples, eventExamples, pmerEventExamples};
}

function buildReference(exampleMaps) {
  const constructs = [];
  const addConstruct = (type, name, description, syntax, group, href) => {
    constructs.push({type, name, description: normalize(description), syntax: normalize(syntax), group: normalize(group), href});
  };

  for (const [name, method] of Object.entries(manifest.methods).sort(([a], [b]) => a.localeCompare(b))) {
    const filename = `reference/methods/${slug(name)}.md`;
    write(filename, methodPage(name, method, exampleMaps.methodExamples.get(name) || []));
    addConstruct('Method', name, method.description, method.syntax, method.subgroup, `/${filename.replace(/\.md$/, '/')}`);
  }

  for (const [name, keyword] of Object.entries(manifest.keywords).sort(([a], [b]) => a.localeCompare(b))) {
    const filename = `reference/keywords/${slug(name)}.md`;
    write(filename, keywordPage(name, keyword));
    addConstruct('Keyword', name, keyword.description, keyword.syntax, keyword.isStatement ? 'Statement' : 'Keyword', `/${filename.replace(/\.md$/, '/')}`);
  }

  for (const [name, flag] of Object.entries(manifest.flags).sort(([a], [b]) => a.localeCompare(b))) {
    const filename = `reference/flags/${slug(name)}.md`;
    write(filename, flagPage(name, flag));
    addConstruct('Flag', name, flag.description, flag.syntax, 'Script entry point', `/${filename.replace(/\.md$/, '/')}`);
  }

  for (const variable of [...manifest.variables].sort((a, b) => a.fullName.localeCompare(b.fullName))) {
    const filename = `reference/variables/${slug(variable.fullName)}.md`;
    write(filename, variablePage(variable));
    addConstruct('Variable', variable.fullName, variable.type, variable.fullName, variable.category, `/${filename.replace(/\.md$/, '/')}`);
  }

  for (const example of exampleMaps.examples) {
    const firstComment = example.content
      .split(/\r?\n/)
      .map(line => line.trim())
      .find(line => line.startsWith('# '));
    addConstruct(
      'Example',
      example.name,
      firstComment?.slice(2) || 'A complete example compiled during every SER build.',
      example.filename,
      [...example.usage.methods.slice(0, 3), ...example.usage.events.slice(0, 2)].join(', ') || 'Language fundamentals',
      `/examples/${slug(example.name)}/`,
    );
  }

  for (const name of manifest.events) {
    const details = manifest.eventDetails[name] || {};
    const filename = `reference/events/${slug(name)}.md`;
    write(filename, eventPage(name, details, false, exampleMaps.eventExamples.get(name) || []));
    addConstruct('Event', name, details.description, `!-- OnEvent ${name}`, details.group, `/${filename.replace(/\.md$/, '/')}`);
  }

  for (const name of manifest.pmerEvents) {
    const details = manifest.pmerEventDetails[name] || {};
    const filename = `reference/pmer-events/${slug(name)}.md`;
    write(filename, eventPage(name, details, true, exampleMaps.pmerEventExamples.get(name) || []));
    addConstruct('PMER event', name, details.description, `!-- OnPMER ${name}`, details.group, `/${filename.replace(/\.md$/, '/')}`);
  }

  const counts = constructs.reduce((result, item) => {
    result[item.type] = (result[item.type] || 0) + 1;
    return result;
  }, {});
  write('reference/catalog.md', [
    frontMatter('SER language catalog', {slug: '/reference/catalog/', sidebar_label: 'Catalog overview'}),
    'This reference is generated from the same language manifest used by `serhelp`, SER Blocks, and the VS Code extension.',
    '',
    '[Open the construct explorer](/reference) to search the complete catalog in your browser.',
    '',
    '| Construct | Count |',
    '|---|---:|',
    ...Object.entries(counts).map(([type, count]) => `| ${type} | ${count} |`),
    '',
    'The generated pages describe the current repository build. A running server’s `serhelp` output remains authoritative for the exact plugin and optional integrations installed there.',
  ].join('\n'));

  fs.mkdirSync(path.join(websiteDirectory, 'src', 'data'), {recursive: true});
  fs.writeFileSync(
    path.join(websiteDirectory, 'src', 'data', 'constructs.json'),
    `${JSON.stringify(constructs.sort((a, b) => a.name.localeCompare(b.name)), null, 2)}\n`,
  );
  return constructs;
}

function copyStaticAssets() {
  const assetsDirectory = path.join(staticDirectory, 'assets', 'examples');
  const editorDirectory = path.join(staticDirectory, 'ser-blocks');
  const imageDirectory = path.join(staticDirectory, 'img');
  fs.mkdirSync(assetsDirectory, {recursive: true});
  fs.mkdirSync(editorDirectory, {recursive: true});
  fs.mkdirSync(imageDirectory, {recursive: true});

  for (const filename of fs.readdirSync(path.join(repositoryDirectory, 'Example Scripts'))) {
    if (/\.(?:ser|txt)$/i.test(filename)) {
      fs.copyFileSync(path.join(repositoryDirectory, 'Example Scripts', filename), path.join(assetsDirectory, filename));
    }
  }

  const editorSource = path.join(repositoryDirectory, 'SER Visual Editor.html');
  if (!fs.existsSync(editorSource)) {
    throw new Error('Missing SER Visual Editor.html. Build the SER tooling before building the documentation site.');
  }
  fs.copyFileSync(editorSource, path.join(editorDirectory, 'index.html'));
  fs.copyFileSync(path.join(repositoryDirectory, 'scriptedeventslogo.png'), path.join(imageDirectory, 'logo.png'));
}

fs.rmSync(generatedDocsDirectory, {recursive: true, force: true});
fs.rmSync(staticDirectory, {recursive: true, force: true});
copySourceDocs();

let languageSpecification = fs.readFileSync(path.join(repositoryDirectory, 'language_specification.md'), 'utf8');
languageSpecification = languageSpecification.replace(/^# .+$/m, '# Language specification');
languageSpecification = rewriteRepositoryLinks(languageSpecification, 'language-specification.md');
write('language-specification.md', `${frontMatter('Language specification', {sidebar_label: 'Language specification'})}\n${languageSpecification}`);
write('developer/project-guide.md', `${frontMatter('SER developer guide', {sidebar_label: 'Developer guide'})}\n${rewriteRepositoryLinks(fs.readFileSync(path.join(repositoryDirectory, 'PROJECT_GUIDE.md'), 'utf8'), 'developer/project-guide.md')}`);

const exampleMaps = buildExamples();
const constructs = buildReference(exampleMaps);
category('reference/methods', 'Methods', 1);
category('reference/events', 'Game events', 2);
category('reference/pmer-events', 'ProjectMER events', 3);
category('reference/flags', 'Flags', 4);
category('reference/keywords', 'Keywords', 5);
category('reference/variables', 'Predefined variables', 6);
copyStaticAssets();

console.log(`Prepared the SER documentation site: ${constructs.length} constructs and ${exampleMaps.examples.length} examples.`);
