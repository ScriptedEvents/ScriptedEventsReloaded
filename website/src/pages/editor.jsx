import React from 'react';
import Layout from '@theme/Layout';
import useBaseUrl from '@docusaurus/useBaseUrl';
import styles from './editor.module.css';

export default function BlocksEditor() {
  const editorUrl = useBaseUrl('/ser-blocks/');
  return (
    <Layout
      title="SER Blocks"
      description="Build a beginner SER script visually in your browser."
      noFooter
    >
      <main className={styles.page}>
        <div className={styles.bar}>
          <div>
            <strong>SER Blocks</strong>
            <span>Runs entirely in your browser</span>
          </div>
          <a href={editorUrl} target="_blank" rel="noreferrer">Open full screen</a>
        </div>
        <iframe
          className={styles.frame}
          src={editorUrl}
          title="SER Blocks visual script editor"
          allow="clipboard-write"
        />
      </main>
    </Layout>
  );
}
