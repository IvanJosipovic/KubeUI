# KubeUI documentation site

This Docusaurus site serves the KubeUI guides at [KubeUI.com](https://KubeUI.com).

## Run locally

```bash
npm ci
npm run start
```

## Build

```bash
npm run build
```

The static site is written to `build/`. GitHub Actions builds it with Node.js 22, then publishes it to GitHub Pages after each successful stable release from `main`.

GitHub Pages must use the GitHub Actions publishing source, and `KubeUI.com` DNS must point to GitHub Pages.
