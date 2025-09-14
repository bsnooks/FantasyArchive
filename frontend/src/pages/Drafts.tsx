import React from 'react';
import './Drafts.css';

const Drafts: React.FC = () => {
  return (
    <div className="drafts-page">
      <header className="page-header">
        <h1>Draft History</h1>
        <p>Explore every draft in league history</p>
      </header>

      <div className="coming-soon">
        <h2>🚧 Coming Soon</h2>
        <p>
          This page will show the complete draft history of the Gibsons League, 
          including draft boards, pick values, and draft analysis.
        </p>
        <ul>
          <li>Year-by-year draft results</li>
          <li>Draft position analysis</li>
          <li>Best/worst picks by round</li>
          <li>Draft day trades</li>
        </ul>
      </div>
    </div>
  );
};

export default Drafts;