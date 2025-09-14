import React, { useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Typography } from 'antd';
import { useFranchises, useSeasons } from '../hooks/useFantasyData';
import LoadingSkeleton from '../components/LoadingSkeleton';
import EmptyState from '../components/EmptyState';
import FranchiseTable from '../components/tables/FranchiseTable';
import { SeasonLink } from '../components/links';
import { useAppDispatch } from '../store/hooks';
import { clearSelectedFranchise, clearSelectedSeason } from '../store/contextSlice';
import './Landing.css';

const { Title, Paragraph } = Typography;

const Landing: React.FC = () => {
  const { data: franchises, isLoading: franchisesLoading, error: franchisesError } = useFranchises();
  const { data: seasons, isLoading: seasonsLoading, error: seasonsError } = useSeasons();
  const dispatch = useAppDispatch();

  const isLoading = franchisesLoading || seasonsLoading;
  const error = franchisesError || seasonsError;

  // Clear context when landing page loads
  useEffect(() => {
    dispatch(clearSelectedFranchise());
    dispatch(clearSelectedSeason());
  }, [dispatch]);

  if (isLoading) {
    return (
      <div className="landing-page">
        <h1>Fantasy Archive - Gibsons League</h1>
        <div className="franchise-grid">
          {[...Array(10)].map((_, index) => (
            <LoadingSkeleton key={index} height="200px" />
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="landing-page">
        <h1>Fantasy Archive - Gibsons League</h1>
        <EmptyState 
          message="Failed to load franchises" 
          description="Please try refreshing the page."
        />
      </div>
    );
  }

  if (!franchises || franchises.length === 0) {
    return (
      <div className="landing-page">
        <h1>Fantasy Archive - Gibsons League</h1>
        <EmptyState 
          message="No franchises found" 
          description="The league data appears to be empty."
        />
      </div>
    );
  }

  // Calculate all-time records for each franchise
  const franchisesWithStats = franchises.map(franchise => {
    const totalWins = franchise.Teams.reduce((sum, team) => sum + team.Wins, 0);
    const totalLosses = franchise.Teams.reduce((sum, team) => sum + team.Losses, 0);
    const totalTies = franchise.Teams.reduce((sum, team) => sum + team.Ties, 0);
    const championships = franchise.Teams.filter(team => team.Champion).length;
    const secondPlaces = franchise.Teams.filter(team => team.SecondPlace).length;
    const totalPoints = franchise.Teams.reduce((sum, team) => sum + team.Points, 0);
    
    return {
      ...franchise,
      totalWins,
      totalLosses,
      totalTies,
      championships,
      secondPlaces,
      totalPoints,
      winPercentage: totalWins / (totalWins + totalLosses + totalTies)
    };
  });

  // Sort by championships first, then by win percentage, then by total wins
  const sortedFranchises = franchisesWithStats.sort((a, b) => {
    if (a.championships !== b.championships) {
      return b.championships - a.championships;
    }
    if (a.winPercentage !== b.winPercentage) {
      return b.winPercentage - a.winPercentage;
    }
    return b.totalWins - a.totalWins;
  });

  return (
    <div className="landing-page">
      <header className="landing-header">
        <Title level={1} style={{ color: '#1a2b4c', fontFamily: 'Georgia, serif' }}>
          Fantasy Archive
        </Title>
        <Title level={2} style={{ color: '#495057', fontWeight: 600 }}>
          Gibsons League All-Time Standings
        </Title>
        <Paragraph style={{ color: '#6c757d', fontSize: '1.1rem' }}>
          Explore {franchises.length} franchises and decades of fantasy football history
        </Paragraph>
      </header>

      <FranchiseTable
        franchises={sortedFranchises}
        loading={isLoading}
        showRank={true}
        showActions={true}
      />

      <section className="recent-seasons">
        <Title level={2} style={{ color: '#1a2b4c', textAlign: 'center', marginBottom: '1.5rem' }}>
          Recent Seasons
        </Title>
        <div className="seasons-grid">
          {seasons?.sort((a, b) => b.Year - a.Year).slice(0, 6).map(season => {
            const champion = season.Teams.find(team => team.Champion);
            return (
              <SeasonLink 
                key={season.Year} 
                season={season.Year}
                to={`/season/${season.Year}`}
                clearFranchiseContext={true}
                className="season-card"
              >
                <div className="season-year">{season.Year}</div>
                <div className="season-name">{season.Name}</div>
                {champion && (
                  <div className="season-champion">
                    🏆 {champion.FranchiseName}
                  </div>
                )}
                <div className="season-status">
                  {season.IsActive ? `Week ${season.CurrentWeek}` : 'Complete'}
                </div>
              </SeasonLink>
            );
          })}
        </div>
        <div className="view-all-seasons">
          <Link 
            to="/seasons" 
            className="view-all-link"
          >
            View All {seasons?.length || 0} Seasons →
          </Link>
        </div>
      </section>
    </div>
  );
};

export default Landing;