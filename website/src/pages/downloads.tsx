import type {ReactNode} from 'react';
import Link from '@docusaurus/Link';
import Layout from '@theme/Layout';
import Heading from '@theme/Heading';

import DownloadOptions from '@site/src/components/Downloads';

import styles from './downloads.module.css';

export default function Downloads(): ReactNode {
  return (
    <Layout
      title="Download KubeUI"
      description="Download KubeUI for Windows, macOS, and Linux. Choose AMD64 or ARM64 packages.">
      <main className="container">
        <header className={styles.header}>
          <Heading as="h1">Download KubeUI</Heading>
          <p>Choose package for Windows, macOS, or Linux. AMD64 and ARM64 builds available.</p>
          <Link
            className="button button--primary"
            to="https://github.com/IvanJosipovic/KubeUI/releases/latest">
            View all GitHub releases
          </Link>
        </header>
        <DownloadOptions />
      </main>
    </Layout>
  );
}
