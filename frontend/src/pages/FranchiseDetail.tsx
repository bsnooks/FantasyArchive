import React from 'react';
import { useParams } from 'react-router-dom';
import { TrophyOutlined } from '@ant-design/icons';
import { useFranchises } from '../hooks/useFantasyData';
import LoadingSkeleton from '../components/LoadingSkeleton';
import EmptyState from '../components/EmptyState';
import FranchiseLogo from '../components/FranchiseLogo';
import { SeasonLink } from '../components/links';
import './FranchiseDetail.css';

const FranchiseDetail: React.FC = () => {
  const { franchiseId } = useParams<{ franchiseId: string }>();
  const { data: franchises, isLoading, error } = useFranchises();

  // Helper function to format seasons with trophies as tags
  const formatSeasonsAsTags = (seasons: number[], franchise: any) => {
    const championshipYears = franchise.Teams
      .filter((team: any) => team.Champion)
      .map((team: any) => team.Year);
    
    const secondPlaceYears = franchise.Teams
      .filter((team: any) => team.SecondPlace)
      .map((team: any) => team.Year);

    return seasons.map(year => {
      const isChampion = championshipYears.includes(year);
      const isSecondPlace = secondPlaceYears.includes(year);
      const shortYear = "'" + year.toString().slice(-2);
      
      let className = 'season-tag';
      if (isChampion) className += ' champion';
      else if (isSecondPlace) className += ' second-place';
      
      return (
        <span key={year} className={className}>
          {shortYear}
          {isChampion && <TrophyOutlined className="trophy-icon" />}
          {isSecondPlace && <TrophyOutlined className="trophy-icon silver" />}
        </span>
      );
    });
  };

  // Helper function to calculate total PPG for starters
  const calculateStartersPPG = (players: any[]) => {
    const starters = players.filter(player => !player.IsBench);
    const totalPPG = starters.reduce((sum, player) => sum + player.AveragePoints, 0);
    return totalPPG.toFixed(1);
  };

  if (isLoading) {
    return (
      <div className="franchise-detail">
        <LoadingSkeleton height="60px" />
        <LoadingSkeleton height="400px" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="franchise-detail">
        <EmptyState 
          message="Failed to load franchise data" 
          description="Please try refreshing the page."
        />
      </div>
    );
  }

  const franchise = franchises?.find(f => f.Id === franchiseId);

  if (!franchise) {
    return (
      <div className="franchise-detail">
        <EmptyState 
          message="Franchise not found" 
          description="The requested franchise could not be found."
        />
      </div>
    );
  }

  return (
    <div className="franchise-detail">
      <header className="franchise-header">
        <FranchiseLogo 
          franchiseId={franchise.Id} 
          franchiseName={franchise.Name}
          size="xlarge"
          className="franchise-logo-header rounded shadow"
        />
        <div className="franchise-info">
          <h1>{franchise.Name}</h1>
          <p className="current-owner">Current Owner: <strong>{franchise.Owner}</strong></p>
          {franchise.Owners.length > 1 && (
            <p className="historical-owners">
              Historical Owners: {franchise.Owners.join(', ')}
            </p>
          )}
          <p className="established">
            Established: {franchise.EstablishedDate ? new Date(franchise.EstablishedDate).getFullYear() : 'Unknown'}
          </p>
        </div>
      </header>

      <div className="franchise-content">
        <section className="all-time-roster">
          <h2>All-Time {franchise.Name} Roster</h2>
          <div className="roster-note">
            <p>
              <em>
                The top contributors at each position based on total points scored 
                while starting for this franchise during the regular season.
              </em>
            </p>
          </div>
          
          {franchise.AllTimeRoster ? (
            <div className="position-groups">
              <div className="position-group">
                <div className="position-header">
                  <h3>QBs</h3>
                  <div className="position-total-ppg">
                    Total PPG: {calculateStartersPPG(franchise.AllTimeRoster.Quarterbacks)}
                  </div>
                </div>
                <div className="player-slots">
                  {franchise.AllTimeRoster.Quarterbacks.map((player, index) => (
                    <div key={player.PlayerId} className={`player-slot filled ${player.IsBench ? 'bench' : ''}`}>
                      <div className="player-header">
                        <div className="player-position">
                          {player.IsBench ? 'BN' : `QB${index + 1}`}
                        </div>
                      </div>
                      <div className="player-name">{player.PlayerName}</div>
                      <div className="player-content">
                        <div className="player-stats-row">
                          <div className="player-stats">
                            <span className="stat-number">{player.TotalPoints}</span> pts in <span className="stat-number">{player.WeeksStarted}</span> starts
                          </div>
                          <div className="player-ppg">
                            {player.AveragePoints.toFixed(1)} ppg
                          </div>
                        </div>
                        <div className="player-seasons">
                          {formatSeasonsAsTags(player.SeasonsWithFranchise, franchise)}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
              
              <div className="position-group">
                <div className="position-header">
                  <h3>RBs</h3>
                  <div className="position-total-ppg">
                    Total PPG: {calculateStartersPPG(franchise.AllTimeRoster.RunningBacks)}
                  </div>
                </div>
                <div className="player-slots">
                  {franchise.AllTimeRoster.RunningBacks.map((player, index) => (
                    <div key={player.PlayerId} className={`player-slot filled ${player.IsBench ? 'bench' : ''}`}>
                      <div className="player-header">
                        <div className="player-position">
                          {player.IsBench ? 'BN' : `RB${index + 1}`}
                        </div>
                      </div>
                      <div className="player-name">{player.PlayerName}</div>
                      <div className="player-content">
                        <div className="player-stats-row">
                          <div className="player-stats">
                            <span className="stat-number">{player.TotalPoints}</span> pts in <span className="stat-number">{player.WeeksStarted}</span> starts
                          </div>
                          <div className="player-ppg">
                            {player.AveragePoints.toFixed(1)} ppg
                          </div>
                        </div>
                        <div className="player-seasons">
                          {formatSeasonsAsTags(player.SeasonsWithFranchise, franchise)}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
              
              <div className="position-group">
                <div className="position-header">
                  <h3>WRs</h3>
                  <div className="position-total-ppg">
                    Total PPG: {calculateStartersPPG(franchise.AllTimeRoster.WideReceivers)}
                  </div>
                </div>
                <div className="player-slots">
                  {franchise.AllTimeRoster.WideReceivers.map((player, index) => (
                    <div key={player.PlayerId} className={`player-slot filled ${player.IsBench ? 'bench' : ''}`}>
                      <div className="player-header">
                        <div className="player-position">
                          {player.IsBench ? 'BN' : `WR${index + 1}`}
                        </div>
                      </div>
                      <div className="player-name">{player.PlayerName}</div>
                      <div className="player-content">
                        <div className="player-stats-row">
                          <div className="player-stats">
                            <span className="stat-number">{player.TotalPoints}</span> pts in <span className="stat-number">{player.WeeksStarted}</span> starts
                          </div>
                          <div className="player-ppg">
                            {player.AveragePoints.toFixed(1)} ppg
                          </div>
                        </div>
                        <div className="player-seasons">
                          {formatSeasonsAsTags(player.SeasonsWithFranchise, franchise)}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
              
              <div className="position-group">
                <div className="position-header">
                  <h3>TEs</h3>
                  <div className="position-total-ppg">
                    Total PPG: {calculateStartersPPG(franchise.AllTimeRoster.TightEnds)}
                  </div>
                </div>
                <div className="player-slots">
                  {franchise.AllTimeRoster.TightEnds.map((player, index) => (
                    <div key={player.PlayerId} className={`player-slot filled ${player.IsBench ? 'bench' : ''}`}>
                      <div className="player-header">
                        <div className="player-position">
                          {player.IsBench ? 'BN' : `TE${index + 1}`}
                        </div>
                      </div>
                      <div className="player-name">{player.PlayerName}</div>
                      <div className="player-content">
                        <div className="player-stats-row">
                          <div className="player-stats">
                            <span className="stat-number">{player.TotalPoints}</span> pts in <span className="stat-number">{player.WeeksStarted}</span> starts
                          </div>
                          <div className="player-ppg">
                            {player.AveragePoints.toFixed(1)} ppg
                          </div>
                        </div>
                        <div className="player-seasons">
                          {formatSeasonsAsTags(player.SeasonsWithFranchise, franchise)}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          ) : (
            <div className="position-groups">
              <div className="position-group">
                <h3>Quarterbacks (2)</h3>
                <div className="player-slots">
                  <div className="player-slot">QB1 - Calculating...</div>
                  <div className="player-slot">QB2 - Calculating...</div>
                </div>
              </div>
              
              <div className="position-group">
                <h3>Running Backs (2)</h3>
                <div className="player-slots">
                  <div className="player-slot">RB1 - Calculating...</div>
                  <div className="player-slot">RB2 - Calculating...</div>
                </div>
              </div>
              
              <div className="position-group">
                <h3>Wide Receivers (3)</h3>
                <div className="player-slots">
                  <div className="player-slot">WR1 - Calculating...</div>
                  <div className="player-slot">WR2 - Calculating...</div>
                  <div className="player-slot">WR3 - Calculating...</div>
                </div>
              </div>
              
              <div className="position-group">
                <h3>Tight End (1)</h3>
                <div className="player-slots">
                  <div className="player-slot">TE1 - Calculating...</div>
                </div>
              </div>
            </div>
          )}
        </section>

        <section className="franchise-history">
          <h2>Season History</h2>
          <div className="seasons-grid">
            {franchise.Teams
              .sort((a, b) => b.Year - a.Year)
              .map(team => (
                <SeasonLink
                  key={team.Id}
                  season={team.Year}
                  to={`/franchise/${franchise.Id}/season/${team.Year}`}
                  clearFranchiseContext={false}
                  className="season-card-link"
                >
                  <div className="season-card">
                    <div className="season-year">{team.Year}</div>
                    <div className="team-name">{team.TeamName}</div>
                    <div className="season-record">
                      {team.Wins}-{team.Losses}
                      {team.Ties > 0 && `-${team.Ties}`}
                    </div>
                    <div className="season-standing">#{team.Standing}</div>
                    <div className="season-points">{team.Points.toFixed(1)} pts</div>
                    {team.Champion && <div className="achievement champion">🏆 Champion</div>}
                    {team.SecondPlace && <div className="achievement second">🥈 Runner-up</div>}
                  </div>
                </SeasonLink>
              ))}
          </div>
        </section>
      </div>
    </div>
  );
};

export default FranchiseDetail;