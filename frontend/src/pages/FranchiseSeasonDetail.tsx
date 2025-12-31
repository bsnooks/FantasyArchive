import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useFranchises, useSeasons } from '../hooks/useFantasyData';
import LoadingSkeleton from '../components/LoadingSkeleton';
import EmptyState from '../components/EmptyState';
import FranchiseLogo from '../components/FranchiseLogo';
import { Result, Card, Statistic, Row, Col, Button, Space } from 'antd';
import { CalendarOutlined, TrophyOutlined, TeamOutlined, GiftOutlined } from '@ant-design/icons';

const FranchiseSeasonDetail: React.FC = () => {
  const { franchiseId, year } = useParams<{ franchiseId: string; year: string }>();
  const navigate = useNavigate();
  const { data: franchises, isLoading: franchisesLoading } = useFranchises();
  const { data: seasons, isLoading: seasonsLoading } = useSeasons();

  const isLoading = franchisesLoading || seasonsLoading;

  if (isLoading) {
    return (
      <div style={{ padding: '24px' }}>
        <LoadingSkeleton height="60px" />
        <LoadingSkeleton height="400px" />
      </div>
    );
  }

  const franchise = franchises?.find(f => f.Id === franchiseId);
  const season = seasons?.find(s => s.Year === parseInt(year || '0'));
  const team = franchise?.Teams.find(t => t.Year === parseInt(year || '0'));

  if (!franchise || !season || !team) {
    return (
      <div style={{ padding: '24px' }}>
        <EmptyState 
          message="Team season not found" 
          description="The requested franchise season could not be found."
        />
      </div>
    );
  }

  return (
    <div style={{ padding: '24px' }}>
      <div style={{ marginBottom: '24px', display: 'flex', alignItems: 'center', gap: '16px' }}>
        <FranchiseLogo 
          franchiseId={franchise.Id} 
          franchiseName={franchise.Name}
          size="large"
          className="rounded shadow"
        />
        <div style={{ flex: 1 }}>
          <h1 style={{ margin: 0 }}>{team.TeamName}</h1>
          <p style={{ margin: 0, fontSize: '16px', color: '#666' }}>
            {franchise.Name} • {year} Season
          </p>
        </div>
        <Button 
          type="primary"
          icon={<GiftOutlined />}
          size="large"
          onClick={() => navigate(`/franchise/${franchiseId}/season/${year}/wrapped`)}
          style={{ 
            background: 'linear-gradient(45deg, #1890ff, #52c41a)',
            border: 'none',
            boxShadow: '0 2px 8px rgba(0,0,0,0.15)'
          }}
        >
          View {year} Wrapped
        </Button>
      </div>

      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Regular Season Record"
              value={`${team.Wins}-${team.Losses}${team.Ties > 0 ? `-${team.Ties}` : ''}`}
              prefix={<TeamOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Final Standing"
              value={`#${team.Standing}`}
              prefix={<TrophyOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Total Points"
              value={team.Points.toFixed(1)}
              prefix={<CalendarOutlined />}
            />
          </Card>
        </Col>
      </Row>

      {(team.Champion || team.SecondPlace) && (
        <Card style={{ marginBottom: '24px', backgroundColor: team.Champion ? '#f6ffed' : '#fff7e6' }}>
          <div style={{ textAlign: 'center' }}>
            <div style={{ fontSize: '48px', marginBottom: '8px' }}>
              {team.Champion ? '🏆' : '🥈'}
            </div>
            <h2 style={{ margin: 0 }}>
              {team.Champion ? 'League Champion!' : 'Championship Runner-up'}
            </h2>
          </div>
        </Card>
      )}

      <Result
        icon={<CalendarOutlined style={{ fontSize: '64px' }} />}
        title="Team Season Details Coming Soon"
        subTitle={`Detailed ${year} season statistics for ${team.TeamName} will be available in a future update.`}
        extra={
          <div style={{ textAlign: 'left', maxWidth: '400px', margin: '0 auto' }}>
            <h4>Planned Features:</h4>
            <ul>
              <li>Weekly matchup results</li>
              <li>Roster and lineup history</li>
              <li>Player performance by week</li>
              <li>Trade and waiver activity</li>
              <li>Playoff bracket progression</li>
            </ul>
          </div>
        }
      />
    </div>
  );
};

export default FranchiseSeasonDetail;