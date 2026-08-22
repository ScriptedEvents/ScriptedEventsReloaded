import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';

const websiteDirectory = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const repositoryDirectory = path.resolve(websiteDirectory, '..');
const generatedDocsDirectory = path.join(websiteDirectory, '.site-docs');
const staticDirectory = path.join(websiteDirectory, 'static');
const markdownDirectory = path.join(staticDirectory, 'llms');
const truthTableSource = path.join(websiteDirectory, 'data', 'ser-truth-table.json');
const publicBaseUrl = new URL('https://scriptedevents.github.io/ScriptedEventsReloaded/');

function collectMarkdownFiles(directory, result = []) {
  for (const entry of fs.readdirSync(directory, {withFileTypes: true})) {
    const filename = path.join(directory, entry.name);
    if (entry.isDirectory()) collectMarkdownFiles(filename, result);
    else if (entry.isFile() && entry.name.endsWith('.md')) result.push(filename);
  }
  return result;
}

function posixRelative(filename) {
  return path.relative(generatedDocsDirectory, filename).replaceAll('\\', '/');
}

function parseFrontMatter(markdown) {
  const normalized = markdown.replaceAll('\r\n', '\n');
  const match = normalized.match(/^---\n([\s\S]*?)\n---(?:\n|$)/);
  if (!match) return {attributes: {}, body: normalized};

  const attributes = {};
  for (const line of match[1].split('\n')) {
    const separator = line.indexOf(':');
    if (separator === -1) continue;
    const key = line.slice(0, separator).trim();
    const rawValue = line.slice(separator + 1).trim();
    try {
      attributes[key] = JSON.parse(rawValue);
    } catch {
      attributes[key] = rawValue;
    }
  }
  return {attributes, body: normalized.slice(match[0].length).trimStart()};
}

function titleFrom(relativeFilename, attributes, body) {
  if (attributes.title) return String(attributes.title);
  const heading = body.match(/^#\s+(.+)$/m)?.[1];
  if (heading) return heading.trim();
  return path.posix.basename(relativeFilename, '.md')
    .replaceAll('-', ' ')
    .replace(/\b\w/g, character => character.toUpperCase());
}

function canonicalRoute(relativeFilename, attributes) {
  if (attributes.slug !== undefined) return String(attributes.slug).replace(/^\/+/, '');
  return relativeFilename.replace(/\.md$/, '/');
}

function markdownUrl(relativeFilename) {
  return new URL(`llms/${relativeFilename}`, publicBaseUrl).href;
}

function canonicalUrl(relativeFilename, attributes) {
  return new URL(canonicalRoute(relativeFilename, attributes), publicBaseUrl).href;
}

function escapedLinkTitle(title) {
  return title.replaceAll('\\', '\\\\').replaceAll('[', '\\[').replaceAll(']', '\\]');
}

function slug(value) {
  return String(value)
    .toLocaleLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-|-$/g, '');
}

function sectionFor(relativeFilename) {
  if (
    relativeFilename === 'README.md'
    || relativeFilename === 'SUMMARY.md'
    || relativeFilename.startsWith('getting-started/')
    || relativeFilename.startsWith('guides/')
    || relativeFilename.startsWith('tutorial/')
  ) return 'Learn and guides';
  if (relativeFilename === 'language-specification.md' || relativeFilename.startsWith('language/')) {
    return 'Language reference';
  }
  if (relativeFilename.startsWith('reference/')) return 'Generated reference';
  if (relativeFilename.startsWith('examples/')) return 'Build-validated examples';
  if (relativeFilename.startsWith('developer/')) return 'Developer documentation';
  return 'Additional documentation';
}

if (!fs.existsSync(generatedDocsDirectory)) {
  throw new Error('Missing .site-docs. Run the website preparation step before building LLM artifacts.');
}

const pageSources = collectMarkdownFiles(generatedDocsDirectory)
  .map(filename => ({filename, relativeFilename: posixRelative(filename)}));
const summaryFilename = path.join(repositoryDirectory, 'docs', 'SUMMARY.md');
pageSources.push({
  filename: summaryFilename,
  relativeFilename: 'SUMMARY.md',
  canonicalUrlOverride: markdownUrl('SUMMARY.md'),
  markdownOverride: fs.readFileSync(summaryFilename, 'utf8')
    .replaceAll('../language_specification.md', 'language-specification.md'),
});

const existingPagePaths = new Set(pageSources.map(page => page.relativeFilename));
const exampleSourceDirectory = path.join(repositoryDirectory, 'Example Scripts');
for (const entry of fs.readdirSync(exampleSourceDirectory, {recursive: true, withFileTypes: true})) {
  if (!entry.isFile() || !/\.(?:ser|txt)$/i.test(entry.name)) continue;
  const filename = path.join(entry.parentPath, entry.name);
  const examplePath = path.relative(exampleSourceDirectory, filename).replaceAll('\\', '/');
  const exampleName = examplePath.replace(/\.(?:ser|txt)$/i, '');
  const relativeFilename = `examples/${slug(exampleName)}.md`;
  if (existingPagePaths.has(relativeFilename)) continue;

  const rawUrl = `https://raw.githubusercontent.com/ScriptedEvents/ScriptedEventsReloaded/main/Example%20Scripts/${examplePath.split('/').map(encodeURIComponent).join('/')}`;
  const script = fs.readFileSync(filename, 'utf8').trimEnd();
  pageSources.push({
    filename,
    relativeFilename,
    canonicalUrlOverride: markdownUrl(relativeFilename),
    markdownOverride: [
      `# ${exampleName}`,
      '',
      `[Download the raw \`${entry.name}\` file](${rawUrl})`,
      '',
      '## Complete script',
      '',
      `\`\`\`ser title="${entry.name}"\n${script}\n\`\`\``,
      '',
      'This file is compiled during the SER build and is safe to use as a current-syntax learning example.',
      '',
    ].join('\n'),
  });
  existingPagePaths.add(relativeFilename);
}

const pages = pageSources
  .map(({filename, relativeFilename, canonicalUrlOverride, markdownOverride}) => {
    const markdown = markdownOverride ?? fs.readFileSync(filename, 'utf8');
    const {attributes, body} = parseFrontMatter(markdown);
    return {
      filename,
      relativeFilename,
      markdown,
      body,
      title: titleFrom(relativeFilename, attributes, body),
      canonicalUrl: canonicalUrlOverride || canonicalUrl(relativeFilename, attributes),
      markdownUrl: markdownUrl(relativeFilename),
      section: sectionFor(relativeFilename),
    };
  })
  .sort((left, right) => left.relativeFilename.localeCompare(right.relativeFilename));

fs.rmSync(markdownDirectory, {recursive: true, force: true});
for (const page of pages) {
  const target = path.join(markdownDirectory, ...page.relativeFilename.split('/'));
  fs.mkdirSync(path.dirname(target), {recursive: true});
  fs.writeFileSync(target, page.markdown);
}

const sectionOrder = [
  'Learn and guides',
  'Language reference',
  'Generated reference',
  'Build-validated examples',
  'Developer documentation',
  'Additional documentation',
];
const indexParts = [
  '# ScriptedEventsReloaded documentation',
  '',
  '> Stable, machine-readable documentation generated from the same sources as the public SER documentation site.',
  '',
  '## Complete corpus and structured data',
  '',
  `- [Full documentation corpus](${new URL('llms-full.txt', publicBaseUrl).href})`,
  `- [SER truth table](${new URL('data/ser-truth-table.json', publicBaseUrl).href})`,
];

for (const section of sectionOrder) {
  const sectionPages = pages.filter(page => page.section === section);
  if (!sectionPages.length) continue;
  indexParts.push('', `## ${section}`, '');
  for (const page of sectionPages) {
    indexParts.push(`- [${escapedLinkTitle(page.title)}](${page.markdownUrl})`);
  }
}

const fullParts = [
  '# ScriptedEventsReloaded full documentation',
  '',
  '> Generated from the current SER tutorials, guides, language specification, reference catalog, and build-validated examples.',
];
for (const page of pages) {
  fullParts.push(
    '',
    '---',
    '',
    `<!-- BEGIN DOCUMENT: ${page.relativeFilename} -->`,
    '',
    `Canonical source URL: [${page.canonicalUrl}](${page.canonicalUrl})`,
    '',
    `Markdown source: [${page.markdownUrl}](${page.markdownUrl})`,
    '',
  );
  if (!/^#\s+/m.test(page.body)) fullParts.push(`# ${page.title}`, '');
  fullParts.push(page.body.trimEnd(), '', `<!-- END DOCUMENT: ${page.relativeFilename} -->`);
}

fs.writeFileSync(path.join(staticDirectory, 'llms.txt'), `${indexParts.join('\n')}\n`);
fs.writeFileSync(path.join(staticDirectory, 'llms-full.txt'), `${fullParts.join('\n')}\n`);

JSON.parse(fs.readFileSync(truthTableSource, 'utf8'));
const publicDataDirectory = path.join(staticDirectory, 'data');
fs.mkdirSync(publicDataDirectory, {recursive: true});
fs.copyFileSync(truthTableSource, path.join(publicDataDirectory, 'ser-truth-table.json'));

console.log(`Generated llms.txt, llms-full.txt, ${pages.length} Markdown assets, and the SER truth table.`);
