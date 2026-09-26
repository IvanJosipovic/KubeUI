import type {ReactNode} from 'react';

import styles from './index.module.css';

type Platform = 'windows' | 'macos' | 'linux';

function PlatformIcon({platform}: {platform: Platform}): ReactNode {
  if (platform === 'windows') {
    return (
      <svg className={styles.platformIcon} viewBox="0 0 24 24" aria-hidden="true" focusable="false">
        <path d="M3 5.5 10.3 4.4V11H3V5.5Zm9-1.3L21 2.8V11h-9V4.2ZM3 12.5h7.3v6.6L3 18v-5.5Zm9 0h9v8.7l-9-1.3v-7.4Z" />
      </svg>
    );
  }

  if (platform === 'macos') {
    return (
      <svg className={styles.platformIcon} viewBox="0 0 24 24" aria-hidden="true" focusable="false">
        <path d="M5 4.5h14A2.5 2.5 0 0 1 21.5 7v9A2.5 2.5 0 0 1 19 18.5h-5.2l.8 2H17a1 1 0 0 1 0 2H7a1 1 0 0 1 0-2h2.4l.8-2H5A2.5 2.5 0 0 1 2.5 16V7A2.5 2.5 0 0 1 5 4.5ZM4.5 7v9c0 .3.2.5.5.5h14c.3 0 .5-.2.5-.5V7c0-.3-.2-.5-.5-.5H5c-.3 0-.5.2-.5.5Z" />
      </svg>
    );
  }

  return (
    <svg className={styles.platformIcon} viewBox="0 0 24 24" aria-hidden="true" focusable="false">
      <rect x="3" y="4" width="18" height="16" rx="2" fill="none" stroke="currentColor" strokeWidth="2" />
      <path d="m7 9 3 3-3 3m6 0h4" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

function DownloadIcon(): ReactNode {
  return (
    <svg className={styles.downloadIcon} viewBox="0 0 16 16" aria-hidden="true" focusable="false">
      <path d="M8 1.5v8m-3-3 3 3 3-3M2 10.5v3h12v-3" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

export default function DownloadOptions(): ReactNode {
  return (
    <div className={styles.downloads}>
      <section className={styles.downloadGroup} aria-labelledby="windows-downloads">
        <h2 id="windows-downloads" className={styles.platformHeading}>
          <PlatformIcon platform="windows" />Windows
        </h2>
        <div className={styles.downloadOptions}>
          <a className={styles.downloadLink} href="https://github.com/IvanJosipovic/KubeUI/releases/latest/download/KubeUI-win-arm64-Setup.exe"><DownloadIcon />arm64 installer</a>
          <a className={styles.downloadLink} href="https://github.com/IvanJosipovic/KubeUI/releases/latest/download/KubeUI-win-arm64.msi"><DownloadIcon />arm64 MSI</a>
          <a className={styles.downloadLink} href="https://github.com/IvanJosipovic/KubeUI/releases/latest/download/KubeUI-win-arm64-Portable.zip"><DownloadIcon />arm64 portable</a>
          <a className={styles.downloadLink} href="https://github.com/IvanJosipovic/KubeUI/releases/latest/download/KubeUI-win-x64-Setup.exe"><DownloadIcon />x64 installer</a>
          <a className={styles.downloadLink} href="https://github.com/IvanJosipovic/KubeUI/releases/latest/download/KubeUI-win-x64.msi"><DownloadIcon />x64 MSI</a>
          <a className={styles.downloadLink} href="https://github.com/IvanJosipovic/KubeUI/releases/latest/download/KubeUI-win-x64-Portable.zip"><DownloadIcon />x64 portable</a>
        </div>
        <code className={styles.installCommand}>winget install KubeUI</code>
        <a
          className={styles.storeLink}
          href="https://apps.microsoft.com/detail/XP9MRWDV3N310N?mode=direct"
          aria-label="Download KubeUI from Microsoft Store">
          <img
            src="https://get.microsoft.com/images/en-us%20dark.svg"
            alt="Download from Microsoft Store"
          />
        </a>
      </section>

      <section className={styles.downloadGroup} aria-labelledby="macos-downloads">
        <h2 id="macos-downloads" className={styles.platformHeading}>
          <PlatformIcon platform="macos" />macOS
        </h2>
        <div className={styles.downloadOptions}>
          <a className={styles.downloadLink} href="https://github.com/IvanJosipovic/KubeUI/releases/latest/download/KubeUI-osx-arm64-Setup.pkg"><DownloadIcon />arm64 .pkg</a>
          <a className={styles.downloadLink} href="https://github.com/IvanJosipovic/KubeUI/releases/latest/download/KubeUI-osx-arm64-Portable.zip"><DownloadIcon />arm64 .app</a>
          <a className={styles.downloadLink} href="https://github.com/IvanJosipovic/KubeUI/releases/latest/download/KubeUI-osx-x64-Setup.pkg"><DownloadIcon />x64 .pkg</a>
          <a className={styles.downloadLink} href="https://github.com/IvanJosipovic/KubeUI/releases/latest/download/KubeUI-osx-x64-Portable.zip"><DownloadIcon />x64 .app</a>
        </div>
        <code className={styles.installCommand}>brew install --cask IvanJosipovic/homebrew-repo/kubeui</code>
      </section>

      <section className={styles.downloadGroup} aria-labelledby="linux-downloads">
        <h2 id="linux-downloads" className={styles.platformHeading}>
          <PlatformIcon platform="linux" />Linux
        </h2>
        <div className={styles.downloadOptions}>
          <a className={styles.downloadLink} href="https://github.com/IvanJosipovic/KubeUI/releases/latest/download/KubeUI-linux-arm64.AppImage"><DownloadIcon />arm64 AppImage</a>
          <a className={styles.downloadLink} href="https://github.com/IvanJosipovic/KubeUI/releases/latest/download/KubeUI-linux-x64.AppImage"><DownloadIcon />x64 AppImage</a>
        </div>
      </section>
    </div>
  );
}
