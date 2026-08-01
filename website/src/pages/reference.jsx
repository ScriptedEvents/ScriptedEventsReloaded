import React, {useMemo, useState} from 'react';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import constructs from '@site/src/data/constructs.json';
import styles from './reference.module.css';

const filters = ['All', 'Method', 'Event', 'PMER event', 'Flag', 'Keyword', 'Variable', 'Example'];

export default function ReferenceExplorer() {
  const {siteConfig} = useDocusaurusContext();
  const [query, setQuery] = useState('');
  const [filter, setFilter] = useState('All');
  const normalizedQuery = query.trim().toLocaleLowerCase();
  const results = useMemo(() => constructs.filter(item => {
    if (filter !== 'All' && item.type !== filter) return false;
    if (!normalizedQuery) return true;
    return `${item.name} ${item.description} ${item.syntax} ${item.group}`
      .toLocaleLowerCase()
      .includes(normalizedQuery);
  }).slice(0, 120), [filter, normalizedQuery]);

  return (
    <Layout title="Construct explorer" description="Search every SER method, event, flag, keyword, and predefined variable.">
      <main className={styles.page}>
        <section className={styles.hero}>
          <p className={styles.eyebrow}>Generated from SER itself</p>
          <h1>Find the exact construct you need.</h1>
          <p>
            Search {constructs.length.toLocaleString()} methods, events, flags, keywords, variables, and complete examples—without opening a game server.
          </p>
          <label className={styles.searchLabel}>
            <span className="sr-only">Search SER constructs</span>
            <input
              autoFocus
              type="search"
              value={query}
              onChange={event => setQuery(event.target.value)}
              placeholder="Try Broadcast, Death, player health…"
              className={styles.search}
            />
          </label>
          <div className={styles.filters} aria-label="Filter constructs">
            {filters.map(value => (
              <button
                key={value}
                type="button"
                aria-pressed={filter === value}
                className={filter === value ? styles.activeFilter : styles.filter}
                onClick={() => setFilter(value)}
              >
                {value}
              </button>
            ))}
          </div>
        </section>

        <section className={styles.results} aria-live="polite">
          <div className={styles.resultSummary}>
            <strong>{results.length}</strong>{results.length === 120 ? '+' : ''} result{results.length === 1 ? '' : 's'}
          </div>
          <div className={styles.grid}>
            {results.map(item => (
              <Link className={styles.card} to={`${siteConfig.baseUrl}${item.href.replace(/^\//, '')}`} key={`${item.type}:${item.name}`}>
                <span className={styles.type}>{item.type}</span>
                <h2>{item.name}</h2>
                {item.syntax && <code>{item.syntax}</code>}
                <p>{item.description || `Open the generated ${item.type.toLowerCase()} reference.`}</p>
                {item.group && <span className={styles.group}>{item.group}</span>}
              </Link>
            ))}
          </div>
          {!results.length && <p className={styles.empty}>No constructs match that search.</p>}
        </section>
      </main>
    </Layout>
  );
}
