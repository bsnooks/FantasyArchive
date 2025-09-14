import React from 'react';
import './RecordBook.css';

const RecordBook: React.FC = () => {
  return (
    <div className="record-book-page">
      <header className="page-header">
        <h1>League Record Book</h1>
        <p>The greatest achievements in Gibsons League history</p>
      </header>

      <div className="record-sections">
        <section className="record-section">
          <h2>🏆 League Records</h2>
          <div className="coming-soon">
            <p>Multi-season achievements and career records</p>
            <ul>
              <li>Most championships</li>
              <li>Highest career win percentage</li>
              <li>Most playoff appearances</li>
              <li>Longest winning/losing streaks</li>
            </ul>
          </div>
        </section>

        <section className="record-section">
          <h2>📅 Season Records</h2>
          <div className="coming-soon">
            <p>Single season achievements and milestones</p>
            <ul>
              <li>Most points in a season</li>
              <li>Best regular season record</li>
              <li>Highest scoring weeks</li>
              <li>Most points against</li>
            </ul>
          </div>
        </section>

        <section className="record-section">
          <h2>📊 Weekly Records</h2>
          <div className="coming-soon">
            <p>Single week performances and highlights</p>
            <ul>
              <li>Highest single week score</li>
              <li>Lowest single week score</li>
              <li>Biggest blowouts</li>
              <li>Closest matchups</li>
            </ul>
          </div>
        </section>
      </div>

      <div className="note">
        <p>
          <strong>🚧 Coming Soon:</strong> All records will be pre-calculated 
          by the data exporter and displayed here.
        </p>
      </div>
    </div>
  );
};

export default RecordBook;