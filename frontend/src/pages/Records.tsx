import React from 'react';
import { TrophyOutlined, RiseOutlined, FallOutlined } from '@ant-design/icons';
import { useRecords } from '../hooks/useRecords';
import { useAppSelector } from '../store/hooks';
import LoadingSkeleton from '../components/LoadingSkeleton';
import EmptyState from '../components/EmptyState';
import { FranchiseLink, SeasonLink } from '../components/links';
import FranchiseLogo from '../components/FranchiseLogo';
import './Records.css';

const Records: React.FC = () => {
  const context = useAppSelector(state => state.context);
  const { data: records, isLoading, error } = useRecords(
    context.selectedFranchiseId || undefined, 
    context.selectedSeason || undefined
  );

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

  const getRecordIcon = (recordType: string, isPositive: boolean) => {
    if (recordType === 'AllTime' || recordType === 'Franchise') {
      return <TrophyOutlined className="record-icon trophy" />;
    } else if (isPositive) {
      return <RiseOutlined className="record-icon positive" />;
    } else {
      return <FallOutlined className="record-icon negative" />;
    }
  };

  const renderRecord = (record: any, index: number) => {
    const isRanked = record.Rank && record.Rank <= 3;
    
    return (
      <div key={index} className={`record-item ${isRanked ? `rank-${record.Rank}` : ''}`}>
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
      </header>

      <div className="records-content">
        {!records || records.length === 0 ? (
          <EmptyState 
            message="No records found" 
            description="No records are available for the current context."
          />
        ) : (
          <div className="records-grid">
            {records.map((recordGroup, groupIndex) => (
              <div key={groupIndex} className="record-group">
                <div className="record-group-header">
                  {getRecordIcon(recordGroup.RecordType, recordGroup.PositiveRecord)}
                  <h2 className="record-group-title">{recordGroup.RecordTitle}</h2>
                  <span className={`record-type ${recordGroup.PositiveRecord ? 'positive' : 'negative'}`}>
                    {recordGroup.RecordType}
                  </span>
                </div>
                
                <div className="record-list">
                  {recordGroup.Records.map((record, recordIndex) => 
                    renderRecord(record, recordIndex)
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default Records;