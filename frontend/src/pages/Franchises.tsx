import React, { useEffect } from 'react';
import { Typography } from 'antd';
import { useFranchises } from '../hooks/useFantasyData';
import LoadingSkeleton from '../components/LoadingSkeleton';
import EmptyState from '../components/EmptyState';
import FranchiseTable from '../components/tables/FranchiseTable';
import { useAppDispatch } from '../store/hooks';
import { clearSelectedFranchise, setViewMode } from '../store/contextSlice';
import './Franchises.css';

const { Title, Paragraph } = Typography;

const Franchises: React.FC = () => {
  const { data: franchises, isLoading, error } = useFranchises();
  const dispatch = useAppDispatch();

  // Clear franchise context when viewing all franchises
  useEffect(() => {
    dispatch(clearSelectedFranchise());
    dispatch(setViewMode('global'));
  }, [dispatch]);

  if (isLoading) {
    return (
      <div className="franchises-page">
        <Title level={1} style={{ color: '#1a2b4c' }}>All Franchises</Title>
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
      <div className="franchises-page">
        <Title level={1} style={{ color: '#1a2b4c' }}>All Franchises</Title>
        <EmptyState 
          message="Failed to load franchises" 
          description="Please try refreshing the page."
        />
      </div>
    );
  }

  if (!franchises || franchises.length === 0) {
    return (
      <div className="franchises-page">
        <Title level={1} style={{ color: '#1a2b4c' }}>All Franchises</Title>
        <EmptyState 
          message="No franchises found" 
          description="The franchise data appears to be empty."
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
    <div className="franchises-page">
      <header className="franchises-header">
        <Title level={1} style={{ color: '#1a2b4c', marginBottom: 0 }}>
          All Franchises
        </Title>
        <Paragraph style={{ color: '#6c757d', fontSize: '1.1rem', marginBottom: '2rem' }}>
          Complete franchise rankings and historical performance
        </Paragraph>
      </header>

      <FranchiseTable
        franchises={sortedFranchises}
        loading={isLoading}
        showRank={true}
        showActions={true}
      />
    </div>
  );
};

export default Franchises;