import React from 'react';
import { useParams } from 'react-router-dom';
import { Typography, Tag, Card, Row, Col, Statistic, Space, Divider } from 'antd';
import { CalendarOutlined, TeamOutlined, TrophyOutlined, StarOutlined } from '@ant-design/icons';
import { useSeasons } from '../hooks/useFantasyData';
import LoadingSkeleton from '../components/LoadingSkeleton';
import EmptyState from '../components/EmptyState';
import SeasonStandingsTable from '../components/tables/SeasonStandingsTable';
import { FranchiseLink } from '../components/links';
import './SeasonDetail.css';

const { Title, Paragraph } = Typography;

const SeasonDetail: React.FC = () => {
  const { year } = useParams<{ year: string }>();
  const { data: seasons, isLoading: seasonsLoading, error: seasonsError } = useSeasons();
  // const { data: franchises, isLoading: franchisesLoading, error: franchisesError } = useFranchises();

  const isLoading = seasonsLoading; // || franchisesLoading;
  const error = seasonsError; // || franchisesError;

  if (isLoading) {
    return (
      <div className="season-detail">
        <LoadingSkeleton height="80px" />
        <div className="season-content">
          <LoadingSkeleton height="300px" />
          <LoadingSkeleton height="400px" />
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="season-detail">
        <EmptyState 
          message="Failed to load season data" 
          description="Please try refreshing the page."
        />
      </div>
    );
  }

  const season = seasons?.find(s => s.Year.toString() === year);

  if (!season) {
    return (
      <div className="season-detail">
        <EmptyState 
          message="Season not found" 
          description={`The ${year} season could not be found.`}
        />
      </div>
    );
  }

  // Get franchise data for color mapping (if needed later)
  // const franchiseMap = new Map(franchises?.map(f => [f.Id, f]) || []);

  // Sort teams by standing (handled in table component)
  // const sortedTeams = [...season.Teams].sort((a, b) => a.Standing - b.Standing);

  // Calculate season statistics
  const totalPoints = season.Teams.reduce((sum, team) => sum + team.Points, 0);
  const averagePoints = totalPoints / season.Teams.length;
  const highestScore = Math.max(...season.Teams.map(team => team.Points));
  const lowestScore = Math.min(...season.Teams.map(team => team.Points));
  
  const champion = season.Teams.find(team => team.Champion);
  const runnerUp = season.Teams.find(team => team.SecondPlace);

  return (
    <div className="season-detail">
      <header className="season-header">
        <div className="season-title">
          <Title level={1} style={{ marginBottom: 0, color: '#1a2b4c' }}>
            {season.Name}
          </Title>
          <Space 
            size="middle" 
            wrap 
            style={{ 
              marginTop: '1rem',
              justifyContent: 'center',
              width: '100%'
            }}
          >
            <Tag 
              color={season.IsActive ? 'processing' : 'success'}
              icon={<CalendarOutlined />}
              style={{ 
                padding: '4px 8px', 
                fontSize: '0.8rem',
                display: 'flex',
                alignItems: 'center',
                gap: '4px'
              }}
            >
              <span className="tag-text-full">
                {season.IsActive ? `Week ${season.CurrentWeek} - In Progress` : 'Season Complete'}
              </span>
              <span className="tag-text-short">
                {season.IsActive ? `Week ${season.CurrentWeek}` : 'Complete'}
              </span>
            </Tag>
          </Space>
        </div>
        
        {(champion || runnerUp) && (
          <div className="season-champions">
            {champion && (
              <Card size="small" className="champion-card">
                <Space size="small">
                  <TrophyOutlined style={{ color: '#d4a615', fontSize: '20px' }} />
                  <div style={{ minWidth: 0, flex: 1 }}>
                    <div style={{ fontSize: '0.75rem', color: '#6c757d', textTransform: 'uppercase' }}>
                      Champion
                    </div>
                    <FranchiseLink 
                      franchiseId={champion.FranchiseId}
                      franchiseName={champion.FranchiseName}
                      to={`/franchise/${champion.FranchiseId}`}
                      preserveSeasonContext={true}
                      style={{ 
                        fontWeight: 600, 
                        color: '#1a2b4c',
                        textDecoration: 'none',
                        fontSize: '1rem',
                        display: 'block',
                        whiteSpace: 'nowrap',
                        overflow: 'hidden',
                        textOverflow: 'ellipsis'
                      }}
                    >
                      {champion.FranchiseName}
                    </FranchiseLink>
                    <div style={{ 
                      color: '#6c757d', 
                      fontSize: '0.8rem',
                      whiteSpace: 'nowrap',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis'
                    }}>
                      {champion.Wins}-{champion.Losses}
                      {champion.Ties > 0 && `-${champion.Ties}`} • {champion.Points.toFixed(1)} pts
                    </div>
                  </div>
                </Space>
              </Card>
            )}
            {runnerUp && (
              <Card size="small" className="runner-up-card">
                <Space size="small">
                  <StarOutlined style={{ color: '#c0c0c0', fontSize: '20px' }} />
                  <div style={{ minWidth: 0, flex: 1 }}>
                    <div style={{ fontSize: '0.75rem', color: '#6c757d', textTransform: 'uppercase' }}>
                      Runner-up
                    </div>
                    <FranchiseLink 
                      franchiseId={runnerUp.FranchiseId}
                      franchiseName={runnerUp.FranchiseName}
                      to={`/franchise/${runnerUp.FranchiseId}`}
                      preserveSeasonContext={true}
                      style={{ 
                        fontWeight: 600, 
                        color: '#1a2b4c',
                        textDecoration: 'none',
                        fontSize: '1rem',
                        display: 'block',
                        whiteSpace: 'nowrap',
                        overflow: 'hidden',
                        textOverflow: 'ellipsis'
                      }}
                    >
                      {runnerUp.FranchiseName}
                    </FranchiseLink>
                    <div style={{ 
                      color: '#6c757d', 
                      fontSize: '0.8rem',
                      whiteSpace: 'nowrap',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis'
                    }}>
                      {runnerUp.Wins}-{runnerUp.Losses}
                      {runnerUp.Ties > 0 && `-${runnerUp.Ties}`} • {runnerUp.Points.toFixed(1)} pts
                    </div>
                  </div>
                </Space>
              </Card>
            )}
          </div>
        )}
      </header>

      <div className="season-content">
        <section className="final-standings">
          <Title level={2} style={{ color: '#1a2b4c', marginBottom: '1.5rem' }}>
            Final Standings
          </Title>
          <SeasonStandingsTable
            teams={season.Teams}
            year={season.Year}
            loading={isLoading}
            showActions={true}
          />
        </section>

        <Divider />

        <section className="season-stats">
          <Title level={2} style={{ color: '#1a2b4c', marginBottom: '1.5rem' }}>
            Season Statistics
          </Title>
          <Row gutter={[16, 16]}>
            <Col xs={12} sm={6}>
              <Card>
                <Statistic
                  title="Total Points Scored"
                  value={totalPoints}
                  formatter={(value) => value?.toLocaleString()}
                  valueStyle={{ color: '#1a2b4c' }}
                />
              </Card>
            </Col>
            <Col xs={12} sm={6}>
              <Card>
                <Statistic
                  title="Average Team Score"
                  value={averagePoints}
                  precision={1}
                  valueStyle={{ color: '#1a2b4c' }}
                />
              </Card>
            </Col>
            <Col xs={12} sm={6}>
              <Card>
                <Statistic
                  title="Highest Team Score"
                  value={highestScore}
                  precision={1}
                  valueStyle={{ color: '#1a2b4c' }}
                />
              </Card>
            </Col>
            <Col xs={12} sm={6}>
              <Card>
                <Statistic
                  title="Lowest Team Score"
                  value={lowestScore}
                  precision={1}
                  valueStyle={{ color: '#1a2b4c' }}
                />
              </Card>
            </Col>
          </Row>
        </section>

        <Divider />

        <section className="coming-soon-features">
          <Title level={2} style={{ color: '#1a2b4c', marginBottom: '1.5rem' }}>
            🚧 Coming Soon
          </Title>
          <Row gutter={[16, 16]}>
            <Col xs={24} sm={12} lg={6}>
              <Card hoverable>
                <Title level={4} style={{ margin: 0, marginBottom: '0.5rem' }}>📊 Weekly Matchups</Title>
                <Paragraph style={{ margin: 0, color: '#6c757d' }}>
                  View all matchups and scores for each week of the season
                </Paragraph>
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card hoverable>
                <Title level={4} style={{ margin: 0, marginBottom: '0.5rem' }}>📋 Draft Results</Title>
                <Paragraph style={{ margin: 0, color: '#6c757d' }}>
                  See the complete draft board and analysis for this season
                </Paragraph>
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card hoverable>
                <Title level={4} style={{ margin: 0, marginBottom: '0.5rem' }}>🔄 Trade Activity</Title>
                <Paragraph style={{ margin: 0, color: '#6c757d' }}>
                  All trades that occurred during this season
                </Paragraph>
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card hoverable>
                <Title level={4} style={{ margin: 0, marginBottom: '0.5rem' }}>🏆 Awards & Records</Title>
                <Paragraph style={{ margin: 0, color: '#6c757d' }}>
                  Weekly high scores, records set, and other achievements
                </Paragraph>
              </Card>
            </Col>
          </Row>
        </section>
      </div>
    </div>
  );
};

export default SeasonDetail;