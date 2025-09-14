import React from 'react';
import { Franchise } from '../types/fantasy';
import SkeletonCard from './SkeletonCard';

interface FranchiseListProps {
  franchises: Franchise[];
  isLoading?: boolean;
}

const FranchiseList: React.FC<FranchiseListProps> = ({ franchises, isLoading = false }) => {
  if (isLoading) {
    return (
      <div className="franchise-list">
        <h2>League Franchises</h2>
        <div className="franchise-grid skeleton-grid">
          <SkeletonCard count={6} height="300px" />
        </div>
      </div>
    );
  }

  if (franchises.length === 0) {
    return (
      <div className="franchise-list">
        <h2>League Franchises</h2>
        <div className="empty-state">
          <div className="empty-icon">🏈</div>
          <h3>No franchises found</h3>
          <p>It looks like there aren't any franchises loaded yet.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="franchise-list">
      <h2>League Franchises</h2>
      <div className="franchise-grid">
        {franchises.map((franchise) => (
          <div key={franchise.Id} className="franchise-card">
            <div className="franchise-header">
              <h3>{franchise.Name}</h3>
              <span className={`status ${franchise.IsActive ? 'active' : 'inactive'}`}>
                {franchise.IsActive ? 'Active' : 'Inactive'}
              </span>
            </div>
            <div className="franchise-details">
              <div className="owner-info">
                <p><strong>Current Owner:</strong> {franchise.Owner}</p>
                {franchise.Owners && franchise.Owners.length > 1 && (
                  <p><strong>Historical Owners:</strong> {franchise.Owners.join(', ')}</p>
                )}
              </div>
              {franchise.EstablishedDate && (
                <p><strong>Established:</strong> {new Date(franchise.EstablishedDate).getFullYear()}</p>
              )}
              <p><strong>Seasons Played:</strong> {franchise.Teams.length}</p>
            </div>
            {franchise.Teams.length > 0 && (
              <div className="franchise-stats">
                <h4>Recent Seasons</h4>
                <div className="recent-teams">
                  {franchise.Teams
                    .slice(-3)
                    .reverse()
                    .map((team) => (
                      <div key={team.Id} className="team-summary">
                        <span className="year">{team.Year}</span>
                        <span className="team-name">{team.TeamName}</span>
                        <span className="record">{team.Wins}-{team.Losses}{team.Ties ? `-${team.Ties}` : ''}</span>
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

export default FranchiseList;