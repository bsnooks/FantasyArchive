import React from 'react';
import { useParams } from 'react-router-dom';
import './PlayerProfile.css';

const PlayerProfile: React.FC = () => {
  const { playerId } = useParams<{ playerId: string }>();

  return (
    <div className="player-profile-page">
      <header className="page-header">
        <h1>Player Profile</h1>
        <p>Individual player transaction history and performance</p>
      </header>

      <div className="coming-soon">
        <h2>🚧 Coming Soon</h2>
        <p>
          This page will show detailed player profiles including their complete 
          transaction history in the Gibsons League.
        </p>
        <ul>
          <li>Draft history and original team</li>
          <li>Trade timeline and all teams played for</li>
          <li>Waiver wire activity</li>
          <li>Performance statistics by team</li>
          <li>Fantasy points contributed to each franchise</li>
        </ul>
        
        {playerId && (
          <div className="player-id-display">
            <p><strong>Player ID:</strong> {playerId}</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default PlayerProfile;