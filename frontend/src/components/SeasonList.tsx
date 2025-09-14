import React from 'react';
import { Season } from '../types/fantasy';
import SkeletonCard from './SkeletonCard';

interface SeasonListProps {
  seasons: Season[];
  isLoading?: boolean;
}

const SeasonList: React.FC<SeasonListProps> = ({ seasons, isLoading = false }) => {
  if (isLoading) {
    return (
      <div className="season-list">
        <h2>League Seasons</h2>
        <div className="season-grid skeleton-grid">
          <SkeletonCard count={8} height="400px" />
        </div>
      </div>
    );
  }

  if (seasons.length === 0) {
    return (
      <div className="season-list">
        <h2>League Seasons</h2>
        <div className="empty-state">
          <div className="empty-icon">📅</div>
          <h3>No seasons found</h3>
          <p>It looks like there aren't any seasons loaded yet.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="season-list">
      <h2>League Seasons</h2>
      <div className="season-grid">
        {seasons.map((season) => (
          <div key={season.Year} className="season-card">
            <div className="season-header">
              <h3>{season.Year} - {season.Name}</h3>
              <span className={`status ${season.IsActive ? 'active' : 'completed'}`}>
                {season.IsActive ? 'In Progress' : 'Completed'}
              </span>
            </div>
            <div className="season-details">
              <p><strong>Year:</strong> {season.Year}</p>
              <p><strong>Current Week:</strong> {season.CurrentWeek}</p>
              <p><strong>Teams:</strong> {season.Teams.length}</p>
            </div>
            {season.Teams.length > 0 && (
              <div className="season-standings">
                <h4>Team Standings</h4>
                <div className="standings-table">
                  {season.Teams
                    .sort((a, b) => {
                      if (a.Standing !== b.Standing) return a.Standing - b.Standing;
                      return b.Points - a.Points;
                    })
                    .map((team, index) => (
                      <div key={team.Id} className="team-row">
                        <span className="rank">{team.Standing}</span>
                        <span className="team-name">{team.TeamName}</span>
                        <span className="owner">({team.Owner})</span>
                        <span className="record">{team.Wins}-{team.Losses}{team.Ties ? `-${team.Ties}` : ''}</span>
                        <span className="points">{team.Points.toFixed(1)} PF</span>
                        {team.Champion && <span className="champion">🏆</span>}
                        {team.SecondPlace && <span className="second">🥈</span>}
                      </div>
                    ))}
                </div>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};

export default SeasonList;