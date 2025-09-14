import React from 'react';
import './Trades.css';

const Trades: React.FC = () => {
  return (
    <div className="trades-page">
      <header className="page-header">
        <h1>Trade History</h1>
        <p>Every trade in league history</p>
      </header>

      <div className="coming-soon">
        <h2>🚧 Coming Soon</h2>
        <p>
          This page will show the complete trade history of the Gibsons League, 
          with analysis of trade winners and losers.
        </p>
        <ul>
          <li>Chronological trade timeline</li>
          <li>Trade analysis and outcomes</li>
          <li>Most active traders</li>
          <li>Best/worst trades in league history</li>
          <li>Trade deadline activity</li>
        </ul>
      </div>
    </div>
  );
};

export default Trades;