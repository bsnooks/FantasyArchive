import React from 'react';
import { Typography } from 'antd';
import { useSeasons } from '../hooks/useFantasyData';
import LoadingSkeleton from '../components/LoadingSkeleton';
import EmptyState from '../components/EmptyState';
import SeasonsTable from '../components/tables/SeasonsTable';
import './Seasons.css';

const { Title, Paragraph } = Typography;

const Seasons: React.FC = () => {
  const { data: seasons, isLoading, error } = useSeasons();

  if (isLoading) {
    return (
      <div className="seasons-page">
        <Title level={1} style={{ color: '#1a2b4c' }}>All Seasons</Title>
        <div className="seasons-grid">
          {[...Array(12)].map((_, index) => (
            <LoadingSkeleton key={index} height="200px" />
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="seasons-page">
        <Title level={1} style={{ color: '#1a2b4c' }}>All Seasons</Title>
        <EmptyState 
          message="Failed to load seasons" 
          description="Please try refreshing the page."
        />
      </div>
    );
  }

  if (!seasons || seasons.length === 0) {
    return (
      <div className="seasons-page">
        <Title level={1} style={{ color: '#1a2b4c' }}>All Seasons</Title>
        <EmptyState 
          message="No seasons found" 
          description="The season data appears to be empty."
        />
      </div>
    );
  }

  // Sort seasons by year (newest first)
  const sortedSeasons = [...seasons].sort((a, b) => b.Year - a.Year);

  return (
    <div className="seasons-page">
      <header className="seasons-header">
        <Title level={1} style={{ color: '#1a2b4c', marginBottom: 0 }}>
          All Seasons
        </Title>
        <Paragraph style={{ color: '#6c757d', fontSize: '1.1rem', marginBottom: '2rem' }}>
          Complete history of the Gibsons League - {seasons.length} seasons and counting
        </Paragraph>
      </header>

      <SeasonsTable
        seasons={sortedSeasons}
        loading={isLoading}
        showActions={true}
      />
    </div>
  );
};

export default Seasons;