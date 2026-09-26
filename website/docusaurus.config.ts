import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

// This runs in Node.js - Don't use client-side code here (browser APIs, JSX...)

const config: Config = {
  title: 'KubeUI',
  tagline: 'A desktop Kubernetes client built for day to day cluster work.',
  favicon: 'img/KubeUI-Icon.png',

  // Future flags, see https://docusaurus.io/docs/api/docusaurus-config#future
  future: {
    v4: true, // Improve compatibility with the upcoming Docusaurus v4
  },

  // Set the production url of your site here
  url: 'https://KubeUI.com',
  // Set the /<baseUrl>/ pathname under which your site is served
  // For GitHub pages deployment, it is often '/<projectName>/'
  baseUrl: '/',

  // GitHub pages deployment config.
  // If you aren't using GitHub pages, you don't need these.
  organizationName: 'IvanJosipovic',
  projectName: 'KubeUI',

  onBrokenLinks: 'throw',

  // Even if you don't use internationalization, you can use this field to set
  // useful metadata like html lang. For example, if your site is Chinese, you
  // may want to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

    presets: [
        [
            'classic',
            {
            docs: {
                sidebarPath: './sidebars.ts',
                editUrl: 'https://github.com/IvanJosipovic/KubeUI/tree/main/website/',
            },
            blog: {
                routeBasePath: 'blog',
                showReadingTime: true,
                feedOptions: {
                    type: ['rss', 'atom'],
                    title: 'KubeUI Blog',
                    description: 'KubeUI releases, updates, and development news.',
                    copyright: `Copyright © ${new Date().getFullYear()} KubeUI.`,
                    limit: 20,
                }
            },
            theme: {
                customCss: './src/css/custom.css',
            },
            } satisfies Preset.Options,
        ],
    ],

  themeConfig: {
    // Replace with your project's social card
    image: 'img/KubeUI-Screenshot.png',
    colorMode: {
      respectPrefersColorScheme: true,
    },
    navbar: {
      title: 'KubeUI',
      logo: {
        alt: 'KubeUI logo',
        src: 'img/KubeUI-Icon.png',
      },
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'docsSidebar',
          position: 'left',
          label: 'Documentation',
        },
        {
          to: '/downloads',
          label: 'Download',
          position: 'right',
        },
        {
          href: 'https://github.com/IvanJosipovic/KubeUI',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {
              label: 'Get started',
              to: '/docs/getting-started',
            },
          ],
        },
        {
          title: 'Project',
          items: [
            {
              label: 'GitHub Issues',
              href: 'https://github.com/IvanJosipovic/KubeUI/issues',
            },
          ],
        },
        {
          title: 'More',
          items: [
            {
              label: 'GitHub',
              href: 'https://github.com/IvanJosipovic/KubeUI',
            },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} KubeUI.`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
