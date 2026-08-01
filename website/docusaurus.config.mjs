import {themes as prismThemes} from 'prism-react-renderer';

const config = {
  title: 'SER Documentation',
  tagline: 'Build SCP:SL server events without writing a plugin',
  favicon: 'img/logo.png',
  url: 'https://scriptedevents.github.io',
  baseUrl: '/ScriptedEventsReloaded/',
  organizationName: 'ScriptedEvents',
  projectName: 'ScriptedEventsReloaded',
  trailingSlash: true,
  onBrokenLinks: 'throw',
  markdown: {
    hooks: {
      onBrokenMarkdownLinks: 'throw',
    },
  },
  presets: [
    [
      'classic',
      {
        docs: {
          path: '.site-docs',
          routeBasePath: '/',
          sidebarPath: './sidebars.mjs',
          showLastUpdateTime: true,
        },
        blog: false,
        pages: {},
        sitemap: {
          changefreq: 'weekly',
          priority: 0.6,
        },
        theme: {
          customCss: './src/css/custom.css',
        },
      },
    ],
  ],
  themeConfig: {
    image: 'img/logo.png',
    metadata: [
      {name: 'keywords', content: 'SER, Scripted Events Reloaded, SCP Secret Laboratory, scripting, documentation'},
    ],
    colorMode: {
      defaultMode: 'dark',
      respectPrefersColorScheme: true,
    },
    navbar: {
      title: 'SER',
      logo: {
        alt: 'Scripted Events Reloaded',
        src: 'img/logo.png',
      },
      items: [
        {type: 'docSidebar', sidebarId: 'tutorialSidebar', label: 'Learn', position: 'left'},
        {to: '/reference', label: 'Constructs', position: 'left'},
        {to: '/examples/', label: 'Examples', position: 'left'},
        {to: '/editor/', label: 'SER Blocks', position: 'left'},
        {
          href: 'https://github.com/ScriptedEvents/ScriptedEventsReloaded',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Start',
          items: [
            {label: 'Install SER', to: '/getting-started/installation/'},
            {label: 'First script', to: '/getting-started/first-script/'},
            {label: 'SER Blocks', to: '/editor/'},
          ],
        },
        {
          title: 'Reference',
          items: [
            {label: 'Construct explorer', to: '/reference'},
            {label: 'Validated examples', to: '/examples/'},
            {label: 'Language specification', to: '/language-specification/'},
          ],
        },
        {
          title: 'Community',
          items: [
            {label: 'GitHub', href: 'https://github.com/ScriptedEvents/ScriptedEventsReloaded'},
            {label: 'Discord', href: 'https://discord.gg/3j54zBnbbD'},
          ],
        },
      ],
      copyright: `Scripted Events Reloaded · Documentation generated from the SER ${new Date().getFullYear()} language manifest`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
    },
  },
};

export default config;
