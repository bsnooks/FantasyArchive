import React, { useState } from 'react';
import { 
  TrophyOutlined, 
  RiseOutlined, 
  FallOutlined, 
  DownOutlined, 
  UpOutlined,
  ClockCircleOutlined,
  CalendarOutlined,
  UserOutlined,
  TeamOutlined,
  CrownOutlined,
  FireOutlined,
  ThunderboltOutlined
} from '@ant-design/icons';
import { Button, Typography } from 'antd';
import { useRecords } from '../hooks/useRecords';
import { useAppSelector } from '../store/hooks';
import LoadingSkeleton from '../components/LoadingSkeleton';
import EmptyState from '../components/EmptyState';
import { FranchiseLink, SeasonLink } from '../components/links';
import FranchiseLogo from '../components/FranchiseLogo';
import './Records.css';

const { Text } = Typography;

const Records: React.FC = () => {
  const context = useAppSelector(state => state.context);
  const { data: records, isLoading, error } = useRecords(
    context.selectedFranchiseId || undefined, 
    context.selectedSeason || undefined
  );

  // State for filtering and expansion
  const [filters, setFilters] = useState({
    AllTime: false,
    Season: false,
    Weekly: false,
    Player: false,
    Franchise: false
  });
  const [expandedGroups, setExpandedGroups] = useState<Record<number, boolean>>({});

  // Helper to determine record category from title
  const getRecordCategory = (recordType: string, recordTitle: string) => {
    if (recordTitle.toLowerCase().includes('weekly')) return 'Weekly';
    return recordType;
  };

  // Filter records based on active filters
  const hasActiveFilters = Object.values(filters).some(filter => filter);
  const filteredRecords = records?.filter(recordGroup => {
    if (!hasActiveFilters) return true; // Show all when no filters are active
    const category = getRecordCategory(recordGroup.RecordType, recordGroup.RecordTitle);
    return filters[category as keyof typeof filters];
  }) || [];

  // Toggle expansion for a record group
  const toggleExpansion = (groupIndex: number) => {
    setExpandedGroups(prev => ({
      ...prev,
      [groupIndex]: !prev[groupIndex]
    }));
  };

  // Toggle filter
  const toggleFilter = (filterKey: keyof typeof filters) => {
    setFilters(prev => ({
      ...prev,
      [filterKey]: !prev[filterKey]
    }));
  };

  if (isLoading) {
    return (
      <div className="records-page">
        <LoadingSkeleton height="60px" />
        <LoadingSkeleton height="400px" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="records-page">
        <EmptyState 
          message="Failed to load records" 
          description="Please try refreshing the page."
        />
      </div>
    );
  }

  const getPageTitle = () => {
    if (context.selectedSeason) {
      return `${context.selectedSeason} Season Records`;
    } else if (context.selectedFranchiseId && context.selectedFranchiseName) {
      return `${context.selectedFranchiseName} Records`;
    } else {
      return 'All-Time League Records';
    }
  };

  const getRecordIcon = (recordType: string, recordTitle: string, isPositive: boolean) => {
    const title = recordTitle.toLowerCase();
    
    // Championship and franchise success records
    if (title.includes('championship') || title.includes('title')) {
      return <CrownOutlined className="record-icon trophy" />;
    }
    
    // All-time and franchise records get trophies
    if (recordType === 'AllTime' || recordType === 'Franchise') {
      return <TrophyOutlined className="record-icon trophy" />;
    }
    
    // Weekly records - use clock for time-based
    if (title.includes('weekly') || title.includes('week')) {
      return <ClockCircleOutlined className={`record-icon ${isPositive ? 'positive' : 'negative'}`} />;
    }
    
    // Season records - use calendar
    if (recordType === 'Season' || title.includes('season')) {
      return <CalendarOutlined className={`record-icon ${isPositive ? 'positive' : 'negative'}`} />;
    }
    
    // Player records - use user icon for individual achievements
    if (recordType === 'Player' || title.includes('player')) {
      // Special cases for exciting records
      if (title.includes('points') || title.includes('score')) {
        return <FireOutlined className={`record-icon ${isPositive ? 'positive' : 'negative'}`} />;
      }
      if (title.includes('performance') || title.includes('percent')) {
        return <ThunderboltOutlined className={`record-icon ${isPositive ? 'positive' : 'negative'}`} />;
      }
      return <UserOutlined className={`record-icon ${isPositive ? 'positive' : 'negative'}`} />;
    }
    
    // Team/franchise specific records
    if (title.includes('team') || title.includes('franchise')) {
      return <TeamOutlined className={`record-icon ${isPositive ? 'positive' : 'negative'}`} />;
    }
    
    // Default fallback
    return isPositive ? 
      <RiseOutlined className="record-icon positive" /> : 
      <FallOutlined className="record-icon negative" />;
  };

  const renderRecord = (record: any, index: number, isExpanded: boolean) => {
    const isRanked = record.Rank && record.Rank <= 3;
    const shouldShow = isExpanded || record.Rank <= 3;
    
    if (!shouldShow) return null;
    
    return (
      <div 
        key={index} 
        className={`record-item ${isRanked ? `rank-${record.Rank}` : ''} ${!isExpanded && record.Rank > 3 ? 'faded' : ''}`}
      >
        <div className="record-rank">
          {record.Rank}
        </div>
        
        <div className="record-content">
          {record.FranchiseId && (
            <div className="record-franchise">
              <FranchiseLogo
                franchiseId={record.FranchiseId}
                franchiseName={record.FranchiseName || 'Unknown'}
                size="small"
                className="record-franchise-logo"
              />
              <FranchiseLink
                franchiseId={record.FranchiseId}
                franchiseName={record.FranchiseName || 'Unknown'}
                to={`/franchise/${record.FranchiseId}`}
                className="record-franchise-link"
              >
                {record.FranchiseName || 'Unknown Franchise'}
              </FranchiseLink>
            </div>
          )}
          
          {record.PlayerName && (
            <div className="record-player">
              <span className="player-name">{record.PlayerName}</span>
              {record.PlayerPosition && (
                <span className="player-position">{record.PlayerPosition}</span>
              )}
            </div>
          )}
          
          <div className="record-value">
            {record.RecordValue}
          </div>
          
          {record.Year && (
            <div className="record-context">
              <SeasonLink 
                season={record.Year} 
                to={`/season/${record.Year}`}
                className="record-year-link"
              >
                {record.Year}
              </SeasonLink>
              {record.Week && (
                <span className="record-week">Week {record.Week}</span>
              )}
            </div>
          )}
        </div>
        
        {isRanked && (
          <div className="record-medal">
            {record.Rank === 1 && '🥇'}
            {record.Rank === 2 && '🥈'}
            {record.Rank === 3 && '🥉'}
          </div>
        )}
      </div>
    );
  };

  return (
    <div className="records-page">
      <header className="records-header">
        <h1>{getPageTitle()}</h1>
        {context.selectedSeason && (
          <p className="records-subtitle">
            Records and achievements from the {context.selectedSeason} season
          </p>
        )}
        {context.selectedFranchiseId && context.selectedFranchiseName && (
          <p className="records-subtitle">
            All-time records and achievements for {context.selectedFranchiseName}
          </p>
        )}
        {!context.selectedSeason && !context.selectedFranchiseId && (
          <p className="records-subtitle">
            League-wide records and achievements across all seasons
          </p>
        )}
        
        {/* Filter Controls */}
        <div className="records-filters">
          <Text strong style={{ marginRight: 16, display: 'block', marginBottom: 12 }}>
            Filter by type: {hasActiveFilters ? `(${Object.values(filters).filter(f => f).length} active)` : '(showing all)'}
          </Text>
          <div className="filter-button-groups">
            <Button.Group>
              <Button 
                type={filters.AllTime ? 'primary' : 'default'}
                size="small"
                onClick={() => toggleFilter('AllTime')}
              >
                All-Time
              </Button>
              <Button 
                type={filters.Season ? 'primary' : 'default'}
                size="small"
                onClick={() => toggleFilter('Season')}
              >
                Season
              </Button>
              <Button 
                type={filters.Weekly ? 'primary' : 'default'}
                size="small"
                onClick={() => toggleFilter('Weekly')}
              >
                Weekly
              </Button>
              <Button 
                type={filters.Player ? 'primary' : 'default'}
                size="small"
                onClick={() => toggleFilter('Player')}
              >
                Player Records
              </Button>
              <Button 
                type={filters.Franchise ? 'primary' : 'default'}
                size="small"
                onClick={() => toggleFilter('Franchise')}
              >
                Franchise
              </Button>
            </Button.Group>
          </div>
        </div>
      </header>

      <div className="records-content">
        {!filteredRecords || filteredRecords.length === 0 ? (
          <EmptyState 
            message="No records found" 
            description="No records match the current filters or context."
          />
        ) : (
          <div className="records-grid">
            {filteredRecords.map((recordGroup, groupIndex) => {
              const isExpanded = expandedGroups[groupIndex];
              const hasMoreThan3 = recordGroup.Records.length > 3;
              
              return (
                <div key={groupIndex} className="record-group">
                  <div className="record-group-header">
                    {getRecordIcon(recordGroup.RecordType, recordGroup.RecordTitle, recordGroup.PositiveRecord)}
                    <h2 className="record-group-title">{recordGroup.RecordTitle}</h2>
                    <span className={`record-type ${recordGroup.PositiveRecord ? 'positive' : 'negative'}`}>
                      {getRecordCategory(recordGroup.RecordType, recordGroup.RecordTitle)}
                    </span>
                  </div>
                  
                  <div className="record-list">
                    {recordGroup.Records.map((record, recordIndex) => 
                      renderRecord(record, recordIndex, isExpanded || !hasMoreThan3)
                    )}
                    
                    {hasMoreThan3 && (
                      <div className="record-expand-button">
                        <Button
                          type="text"
                          size="small"
                          icon={isExpanded ? <UpOutlined /> : <DownOutlined />}
                          onClick={() => toggleExpansion(groupIndex)}
                          style={{ 
                            color: '#6c757d',
                            fontSize: '12px',
                            height: '24px'
                          }}
                        >
                          {isExpanded ? 'Show less' : `Show all ${recordGroup.Records.length}`}
                        </Button>
                      </div>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
};

export default Records;