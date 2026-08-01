# SER documentation website

This Docusaurus site publishes the Markdown in `../docs` without duplicating it.
Its preparation step also generates searchable reference pages from
`data/ser-truth-table.json`, turns every file in `../Example Scripts` into a
cross-linked documentation page, and embeds the standalone SER Blocks editor.

## Local development

```powershell
cd website
npm install
npm start
```

The generated `.site-docs`, static assets, and construct-search index are build
artifacts and are intentionally ignored by Git.

## Keeping the reference synchronized

The normal SER tooling build writes the generated language manifest to both the
VS Code extension and `website/data/ser-truth-table.json`. After a C# change
which affects methods, events, flags, keywords, or predefined variables, build
SER and run `npm run verify` from `Tooling`.

If only the root `ser_method_info.js` needs to be copied into the website, run:

```powershell
cd website
npm run sync:data
```

## Production build

```powershell
cd website
npm run build
```

GitHub Actions builds pull requests and deploys `main` to GitHub Pages. The
workflow also rebuilds the standalone visual editor from the committed language
manifest snapshot before Docusaurus runs.
