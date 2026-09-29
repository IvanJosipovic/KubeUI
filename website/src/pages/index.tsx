import type {ReactNode} from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import Heading from '@theme/Heading';
import DownloadOptions from '@site/src/components/Downloads';

import styles from './index.module.css';

function HomepageHeader() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <header className={clsx('hero hero--primary', styles.heroBanner)}>
      <div className="container">
        <Heading as="h1" className="hero__title">
          {siteConfig.title}
        </Heading>
        <p className="hero__subtitle">{siteConfig.tagline}</p>
        <div className={styles.buttons}>
          <Link
            className="button button--primary button--lg"
            to="/docs/getting-started">
            Read the documentation
          </Link>
          <Link
            className={clsx('button button--outline button--primary button--lg', styles.downloadButton)}
            to="/downloads">
            Download KubeUI
          </Link>
        </div>
      </div>
    </header>
  );
}

export default function Home(): ReactNode {
  return (
    <Layout
      title="Kubernetes, clearly"
      description="Explore clusters, inspect resources, edit YAML, and operate workloads with KubeUI.">
      <HomepageHeader />
      <main className="container">
        <img
          className={styles.screenshot}
          src="/img/KubeUI-Screenshot.png"
          alt="KubeUI showing a Kubernetes resource workspace"
        />
        <section className={styles.featureSection} aria-labelledby="features-heading">
          <Heading as="h2" id="features-heading" className={styles.sectionHeading}>
            Explore Kubernetes with confidence
          </Heading>
          <div className={styles.featureGrid}>
            <article className={styles.featureCard}>
              <Heading as="h3">Browse resources</Heading>
              <p>Search, sort, and filter built-in Kubernetes resources and custom resources. Narrow lists by namespace and inspect each object's details.</p>
              <Link className={styles.featureLink} to="/docs/resource-browsing">Explore resource browsing</Link>
            </article>
            <article className={styles.featureCard}>
              <Heading as="h3">Understand relationships</Heading>
              <p>Visualize how Pods, controllers, services, and other resources connect within a namespace.</p>
              <Link className={styles.featureLink} to="/docs/resource-visualization">Explore resource visualization</Link>
            </article>
            <article className={styles.featureCard}>
              <Heading as="h3">Edit YAML safely</Heading>
              <p>Use Kubernetes-aware completion, validation feedback, and server-side dry runs before saving changes.</p>
              <Link className={styles.featureLink} to="/docs/yaml-editor">Explore the YAML editor</Link>
            </article>
            <article className={styles.featureCard}>
              <Heading as="h3">Operate workloads</Heading>
              <p>Follow live Pod logs, open consoles, forward ports, and manage supported workload and node actions.</p>
              <Link className={styles.featureLink} to="/docs/workload-tools">Explore workload tools</Link>
            </article>
          </div>
        </section>
        <section className={styles.downloadSection} id="downloads" aria-labelledby="downloads-heading">
          <Heading as="h2" id="downloads-heading" className={styles.sectionHeading}>
            Downloads
          </Heading>
          <DownloadOptions />
        </section>
      </main>
    </Layout>
  );
}
