import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  Card, 
  Button, 
  Statistic, 
  Row, 
  Col, 
  Typography, 
  Progress, 
  Avatar, 
  List, 
  Tag,
  Space,
  Alert
} from 'antd';
import { 
  ArrowLeftOutlined, 
  TrophyOutlined, 
  FireOutlined, 
  RiseOutlined, 
  FallOutlined,
  StarOutlined,
  ThunderboltOutlined
} from '@ant-design/icons';
import { useFranchises, useWrappedData } from '../hooks/useFantasyData';
import LoadingSkeleton from '../components/LoadingSkeleton';
import FranchiseLogo from '../components/FranchiseLogo';

const { Title, Text } = Typography;

const WrappedSeason: React.FC = () => {
  const { franchiseId, year } = useParams<{ franchiseId: string; year: string }>();
    WorstWeek: {
      Week: number;
      Points: number;
      Opponent: string;
      OpponentPoints: number;
      Won: boolean;
      Margin: number;
    };
    AverageWeeklyScore: number;
    HighestWeeklyScore: number;
    LowestWeeklyScore: number;
    WeeksAsHighScorer: number;
    WeeksAsLowScorer: number;
    PointsAgainst: number;
    AverageMarginOfVictory: number;
    AverageMarginOfDefeat: number;
    BlowoutWins: number;
    HeartbreakLosses: number;
  };
  Players: {
    MVP: PlayerHighlight;
    MostConsistent: PlayerHighlight;
    Breakout: PlayerHighlight;
    Disappointment: PlayerHighlight;
    BestDraftPick: PlayerHighlight;
    WorstDraftPick: PlayerHighlight;
    TopStarters: PlayerHighlight[];
  };
  LeagueComparisons: {
    PointsRank: number;
    PointsRankSuffix: string;
    PointsAboveAverage: number;
    WinsRank: number;
    WinsRankSuffix: string;
    WinPercentage: number;
    LeagueAverageWinPercentage: number;
    HighScoreWeeksRank: number;
    ConsistencyRank: number;
    StrongestPosition: string;
    WeakestPosition: string;
  };
  FunFacts: string[];
  Achievements: any[];
  HeadToHeadRecords: any[];
}

interface PlayerHighlight {
  PlayerID: number;
  PlayerName: string;
  Position: string;
  TotalPoints: number;
  AveragePoints: number;
  GamesStarted: number;
  GamesOwned: number;
  DraftRound?: number;
  DraftPosition?: number;
  ExpectedPoints?: number;
  PointsAboveExpected?: number;
  NflTeam?: string;
  HighlightReason: string;
}

const WrappedSeason: React.FC = () => {
  const { franchiseId, year } = useParams<{ franchiseId: string; year: string }>();
  const navigate = useNavigate();
  const { data: franchises, isLoading: franchisesLoading } = useFranchises();
  const { data: wrappedData, isLoading: wrappedLoading, error } = useWrappedData(franchiseId, year);

  if (franchisesLoading || wrappedLoading) {
    return (
      <div style={{ padding: '24px' }}>
        <LoadingSkeleton height="60px" />
        <LoadingSkeleton height="400px" />
        <LoadingSkeleton height="300px" />
      </div>
    );
  }

  if (error || !wrappedData) {
    return (
      <div style={{ padding: '24px' }}>
        <Button 
          icon={<ArrowLeftOutlined />} 
          onClick={() => navigate(`/franchise/${franchiseId}/season/${year}`)}
          style={{ marginBottom: '24px' }}
        >
          Back to Season
        </Button>
        <Alert
          message="Wrapped Data Not Available"
          description={error?.message || `No wrapped data found for the ${year} season.`}
          type="warning"
          showIcon
        />
      </div>
    );
  }

  const getPositionIcon = (position: string) => {
    switch (position) {
      case 'QB': return '🏈';
      case 'RB': return '🏃';
      case 'WR': return '🙌';
      case 'TE': return '🎯';
      case 'K': return '🦵';
      case 'DST': return '🛡️';
      default: return '⚡';
    }
  };

  return (
    <div style={{ padding: '24px', maxWidth: '1200px', margin: '0 auto' }}>
      {/* Header */}
      <div style={{ marginBottom: '32px' }}>
        <Button 
          icon={<ArrowLeftOutlined />} 
          onClick={() => navigate(`/franchise/${franchiseId}/season/${year}`)}
          style={{ marginBottom: '16px' }}
        >
          Back to Season
        </Button>
        
        <div style={{ textAlign: 'center', marginBottom: '32px' }}>
          <div style={{ marginBottom: '16px' }}>
            <FranchiseLogo 
              franchiseId={franchiseId!} 
              franchiseName={wrappedData.FranchiseName}
              size="large"
              className="rounded shadow"
            />
          </div>
          <Title level={1} style={{ margin: 0, background: 'linear-gradient(45deg, #1890ff, #52c41a)', backgroundClip: 'text', WebkitBackgroundClip: 'text', color: 'transparent' }}>
            {wrappedData.FranchiseName} Wrapped
          </Title>
          <Title level={3} style={{ margin: '8px 0', color: '#666' }}>
            {year} Season Highlights
          </Title>
        </div>
      </div>

      {/* Season Summary */}
      <Card style={{ marginBottom: '24px', background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', color: 'white' }}>
        <div style={{ textAlign: 'center' }}>
          <Title level={2} style={{ color: 'white', marginBottom: '24px' }}>
            Your {year} Season
          </Title>
          <Row gutter={[24, 24]}>
            <Col xs={24} sm={12} md={6}>
              <Statistic
                title={<span style={{ color: 'rgba(255,255,255,0.85)' }}>Record</span>}
                value={`${wrappedData.SeasonSummary.Wins}-${wrappedData.SeasonSummary.Losses}`}
                valueStyle={{ color: 'white', fontSize: '2em' }}
              />
            </Col>
            <Col xs={24} sm={12} md={6}>
              <Statistic
                title={<span style={{ color: 'rgba(255,255,255,0.85)' }}>Points</span>}
                value={wrappedData.SeasonSummary.Points.toFixed(1)}
                valueStyle={{ color: 'white', fontSize: '2em' }}
              />
            </Col>
            <Col xs={24} sm={12} md={6}>
              <Statistic
                title={<span style={{ color: 'rgba(255,255,255,0.85)' }}>Final Rank</span>}
                value={`#${wrappedData.SeasonSummary.Standing}`}
                valueStyle={{ color: 'white', fontSize: '2em' }}
                suffix={<span style={{ color: 'rgba(255,255,255,0.85)', fontSize: '0.5em' }}>of {wrappedData.SeasonSummary.TotalTeams}</span>}
              />
            </Col>
            <Col xs={24} sm={12} md={6}>
              <div style={{ textAlign: 'center' }}>
                <div style={{ fontSize: '3em', marginBottom: '8px' }}>
                  {wrappedData.SeasonSummary.Champion ? '🏆' : 
                   wrappedData.SeasonSummary.SecondPlace ? '🥈' : 
                   wrappedData.SeasonSummary.MadePlayoffs ? '🎯' : '📊'}
                </div>
                <Text style={{ color: 'white', fontSize: '16px' }}>
                  {wrappedData.SeasonSummary.SeasonOutcome}
                </Text>
              </div>
            </Col>
          </Row>
        </div>
      </Card>

      {/* Performance Highlights */}
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col xs={24} md={12}>
          <Card>
            <div style={{ display: 'flex', alignItems: 'center', marginBottom: '16px' }}>
              <RiseOutlined style={{ fontSize: '24px', color: '#52c41a', marginRight: '12px' }} />
              <Title level={4} style={{ margin: 0 }}>Best Week</Title>
            </div>
            <Statistic
              title={`Week ${wrappedData.Performance.BestWeek.Week}`}
              value={wrappedData.Performance.BestWeek.Points}
              precision={1}
              suffix="pts"
              valueStyle={{ color: '#52c41a' }}
            />
            <Text type="secondary">Your highest scoring week of the season!</Text>
          </Card>
        </Col>
        <Col xs={24} md={12}>
          <Card>
            <div style={{ display: 'flex', alignItems: 'center', marginBottom: '16px' }}>
              <FallOutlined style={{ fontSize: '24px', color: '#ff4d4f', marginRight: '12px' }} />
              <Title level={4} style={{ margin: 0 }}>Toughest Week</Title>
            </div>
            <Statistic
              title={`Week ${wrappedData.Performance.WorstWeek.Week}`}
              value={wrappedData.Performance.WorstWeek.Points}
              precision={1}
              suffix="pts"
              valueStyle={{ color: '#ff4d4f' }}
            />
            <Text type="secondary">We all have those weeks...</Text>
          </Card>
        </Col>
      </Row>

      {/* Player Highlights */}
      {wrappedData.Players.MVP.PlayerName && (
        <Card style={{ marginBottom: '24px' }}>
          <Title level={3}>
            <StarOutlined style={{ color: '#faad14', marginRight: '8px' }} />
            Your Season MVP
          </Title>
          <div style={{ display: 'flex', alignItems: 'center', padding: '16px', background: '#fafafa', borderRadius: '8px' }}>
            <Avatar size={64} style={{ backgroundColor: '#1890ff', fontSize: '24px' }}>
              {getPositionIcon(wrappedData.Players.MVP.Position)}
            </Avatar>
            <div style={{ marginLeft: '16px', flex: 1 }}>
              <Title level={4} style={{ margin: 0 }}>
                {wrappedData.Players.MVP.PlayerName}
              </Title>
              <Tag color="blue">{wrappedData.Players.MVP.Position}</Tag>
              <div style={{ marginTop: '8px' }}>
                <Statistic
                  value={wrappedData.Players.MVP.TotalPoints}
                  precision={1}
                  suffix="total points"
                  valueStyle={{ fontSize: '18px' }}
                />
              </div>
            </div>
          </div>
        </Card>
      )}

      {/* Top Starters */}
      {wrappedData.Players.TopStarters.length > 0 && (
        <Card style={{ marginBottom: '24px' }}>
          <Title level={3}>
            <ThunderboltOutlined style={{ color: '#722ed1', marginRight: '8px' }} />
            Your Top Performers
          </Title>
          <List
            dataSource={wrappedData.Players.TopStarters.slice(0, 5)}
            renderItem={(player: PlayerHighlight, index: number) => (
              <List.Item>
                <List.Item.Meta
                  avatar={
                    <Avatar style={{ backgroundColor: index === 0 ? '#faad14' : '#1890ff' }}>
                      {getPositionIcon(player.Position)}
                    </Avatar>
                  }
                  title={
                    <div>
                      <Text strong>{player.PlayerName}</Text>
                      <Tag color="blue" style={{ marginLeft: '8px' }}>{player.Position}</Tag>
                    </div>
                  }
                  description={`${player.TotalPoints.toFixed(1)} points • ${player.GamesStarted} starts`}
                />
                {index === 0 && <Tag color="gold">👑 MVP</Tag>}
              </List.Item>
            )}
          />
        </Card>
      )}

      {/* League Comparisons */}
      <Card style={{ marginBottom: '24px' }}>
        <Title level={3}>
          <TrophyOutlined style={{ color: '#fa8c16', marginRight: '8px' }} />
          How You Stacked Up
        </Title>
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} md={6}>
            <div style={{ textAlign: 'center', padding: '16px' }}>
              <Title level={1} style={{ margin: 0, color: '#1890ff' }}>
                #{wrappedData.LeagueComparisons.PointsRank}
              </Title>
              <Text>in total points</Text>
              {wrappedData.LeagueComparisons.PointsAboveAverage > 0 && (
                <div style={{ marginTop: '8px' }}>
                  <Text type="success">
                    +{wrappedData.LeagueComparisons.PointsAboveAverage.toFixed(1)} above average
                  </Text>
                </div>
              )}
            </div>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <div style={{ textAlign: 'center', padding: '16px' }}>
              <Title level={1} style={{ margin: 0, color: '#52c41a' }}>
                #{wrappedData.LeagueComparisons.WinsRank}
              </Title>
              <Text>in wins</Text>
              <div style={{ marginTop: '8px' }}>
                <Progress 
                  percent={(wrappedData.SeasonSummary.Wins / (wrappedData.SeasonSummary.Wins + wrappedData.SeasonSummary.Losses)) * 100} 
                  showInfo={false}
                  strokeColor="#52c41a"
                />
              </div>
            </div>
          </Col>
          {wrappedData.LeagueComparisons.BenchPointsRank && (
            <Col xs={24} sm={12} md={6}>
              <div style={{ textAlign: 'center', padding: '16px' }}>
                <Title level={1} style={{ margin: 0, color: '#722ed1' }}>
                  #{wrappedData.LeagueComparisons.BenchPointsRank}
                </Title>
                <Text>in bench points</Text>
                <div style={{ marginTop: '8px' }}>
                  <Text style={{ fontSize: '14px', color: '#8c8c8c' }}>
                    {wrappedData.LeagueComparisons.BenchPoints.toFixed(1)} pts from bench
                  </Text>
                </div>
              </div>
            </Col>
          )}
          {wrappedData.LeagueComparisons.TotalPlayersUsedRank && (
            <Col xs={24} sm={12} md={6}>
              <div style={{ textAlign: 'center', padding: '16px' }}>
                <Title level={1} style={{ margin: 0, color: '#fa541c' }}>
                  #{wrappedData.LeagueComparisons.TotalPlayersUsedRank}
                </Title>
                <Text>in roster diversity</Text>
                <div style={{ marginTop: '8px' }}>
                  <Text style={{ fontSize: '14px', color: '#8c8c8c' }}>
                    {wrappedData.LeagueComparisons.TotalPlayersUsed} different starters
                  </Text>
                </div>
              </div>
            </Col>
          )}
        </Row>
      </Card>

      {/* Bench Highlights */}
      {wrappedData.LeagueComparisons.TopBenchPlayer?.PlayerName && (
        <Card style={{ marginBottom: '24px' }}>
          <Title level={3}>
            <StarOutlined style={{ color: '#13c2c2', marginRight: '8px' }} />
            Your Bench MVP
          </Title>
          <div style={{ display: 'flex', alignItems: 'center', padding: '16px', background: '#f6ffed', borderRadius: '8px' }}>
            <Avatar size={64} style={{ backgroundColor: '#13c2c2', fontSize: '24px' }}>
              {getPositionIcon(wrappedData.LeagueComparisons.TopBenchPlayer.Position)}
            </Avatar>
            <div style={{ marginLeft: '16px', flex: 1 }}>
              <Title level={4} style={{ margin: 0 }}>
                {wrappedData.LeagueComparisons.TopBenchPlayer.PlayerName}
              </Title>
              <Tag color="cyan">{wrappedData.LeagueComparisons.TopBenchPlayer.Position}</Tag>
              <div style={{ marginTop: '8px' }}>
                <Statistic
                  value={wrappedData.LeagueComparisons.TopBenchPlayer.TotalPoints}
                  precision={1}
                  suffix="bench points"
                  valueStyle={{ fontSize: '18px' }}
                />
              </div>
              <Text type="secondary">Your best bench performer - imagine if they started more!</Text>
            </div>
          </div>
        </Card>
      )}

      {/* Roster Diversity */}
      {wrappedData.LeagueComparisons.TotalPlayersUsed > 0 && (
        <Card style={{ marginBottom: '24px' }}>
          <Title level={3}>
            <ThunderboltOutlined style={{ color: '#eb2f96', marginRight: '8px' }} />
            Your Roster Strategy
          </Title>
          <Row gutter={[16, 16]}>
            <Col xs={24} md={12}>
              <div style={{ padding: '16px', background: '#fafafa', borderRadius: '8px' }}>
                <Statistic
                  title="Different Starters Used"
                  value={wrappedData.LeagueComparisons.TotalPlayersUsed}
                  suffix={
                    <span style={{ fontSize: '14px', color: '#8c8c8c' }}>
                      (League avg: {wrappedData.LeagueComparisons.AvgPlayersUsedInLeague.toFixed(1)})
                    </span>
                  }
                  valueStyle={{ color: '#eb2f96' }}
                />
              </div>
            </Col>
            <Col xs={24} md={12}>
              <Title level={5} style={{ marginBottom: '12px' }}>Position Breakdown:</Title>
              <Space direction="vertical" style={{ width: '100%' }}>
                {[
                  { pos: 'QB', count: wrappedData.LeagueComparisons.QuarterbacksUsed, rank: wrappedData.LeagueComparisons.QBUsageRank },
                  { pos: 'RB', count: wrappedData.LeagueComparisons.RunningBacksUsed, rank: wrappedData.LeagueComparisons.RBUsageRank },
                  { pos: 'WR', count: wrappedData.LeagueComparisons.WideReceiversUsed, rank: wrappedData.LeagueComparisons.WRUsageRank },
                  { pos: 'TE', count: wrappedData.LeagueComparisons.TightEndsUsed, rank: wrappedData.LeagueComparisons.TEUsageRank },
                ].filter(p => p.count > 0).map(p => (
                  <div key={p.pos} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <span>
                      <Tag color="purple">{p.pos}</Tag>
                      {p.count} players
                    </span>
                    <Tag color={p.rank <= 3 ? 'green' : p.rank >= 8 ? 'red' : 'blue'}>
                      #{p.rank} in league
                    </Tag>
                  </div>
                ))}
              </Space>
            </Col>
          </Row>
        </Card>
      )}

      {/* Roster Strength */}
      {(wrappedData.LeagueComparisons.QuarterbackPoints > 0 || 
        wrappedData.LeagueComparisons.RunningBackPoints > 0 ||
        wrappedData.LeagueComparisons.WideReceiverPoints > 0 ||
        wrappedData.LeagueComparisons.TightEndPoints > 0) && (
        <Card style={{ marginBottom: '24px' }}>
          <Title level={3}>
            <FireOutlined style={{ color: '#f5222d', marginRight: '8px' }} />
            Your Roster Strength
          </Title>
          <Text type="secondary" style={{ marginBottom: '16px', display: 'block' }}>
            Total points scored by starters at each position compared to the league
          </Text>
          <Row gutter={[16, 16]}>
            {[
              { 
                pos: 'QB', 
                points: wrappedData.LeagueComparisons.QuarterbackPoints,
                rank: wrappedData.LeagueComparisons.QBPointsRank,
                avg: wrappedData.LeagueComparisons.AvgQBPointsInLeague,
                icon: '🎯'
              },
              { 
                pos: 'RB', 
                points: wrappedData.LeagueComparisons.RunningBackPoints,
                rank: wrappedData.LeagueComparisons.RBPointsRank,
                avg: wrappedData.LeagueComparisons.AvgRBPointsInLeague,
                icon: '🏃'
              },
              { 
                pos: 'WR', 
                points: wrappedData.LeagueComparisons.WideReceiverPoints,
                rank: wrappedData.LeagueComparisons.WRPointsRank,
                avg: wrappedData.LeagueComparisons.AvgWRPointsInLeague,
                icon: '🏈'
              },
              { 
                pos: 'TE', 
                points: wrappedData.LeagueComparisons.TightEndPoints,
                rank: wrappedData.LeagueComparisons.TEPointsRank,
                avg: wrappedData.LeagueComparisons.AvgTEPointsInLeague,
                icon: '🎪'
              }
            ].filter(p => p.points > 0).map(p => (
              <Col xs={24} sm={12} md={6} key={p.pos}>
                <div 
                  style={{ 
                    padding: '20px', 
                    background: '#f8fafc',
                    borderRadius: '12px',
                    textAlign: 'center',
                    border: p.rank <= 3 ? '2px solid #52c41a' : p.rank >= 8 ? '2px solid #ff4d4f' : '2px solid #1890ff'
                  }}
                >
                  <div style={{ fontSize: '32px', marginBottom: '8px' }}>
                    {p.icon}
                  </div>
                  <Tag 
                    color={p.rank <= 3 ? 'green' : p.rank >= 8 ? 'red' : 'blue'}
                    style={{ marginBottom: '8px', fontSize: '14px', fontWeight: 'bold' }}
                  >
                    #{p.rank} {p.pos}
                  </Tag>
                  <Statistic
                    value={p.points}
                    precision={1}
                    suffix="pts"
                    valueStyle={{ 
                      fontSize: '24px', 
                      color: p.rank <= 3 ? '#52c41a' : p.rank >= 8 ? '#ff4d4f' : '#1890ff',
                      fontWeight: 'bold'
                    }}
                  />
                  <div style={{ marginTop: '8px' }}>
                    <Text style={{ fontSize: '12px', color: '#8c8c8c' }}>
                      League avg: {p.avg.toFixed(1)}
                    </Text>
                  </div>
                  <div style={{ marginTop: '4px' }}>
                    <Text style={{ 
                      fontSize: '12px',
                      color: p.points > p.avg ? '#52c41a' : '#ff4d4f',
                      fontWeight: 'bold'
                    }}>
                      {p.points > p.avg ? '+' : ''}{(p.points - p.avg).toFixed(1)} vs avg
                    </Text>
                  </div>
                </div>
              </Col>
            ))}
          </Row>
        </Card>
      )}

      {/* Fun Facts */}
      {wrappedData.FunFacts.length > 0 && (
        <Card style={{ marginBottom: '24px' }}>
          <Title level={3}>
            <FireOutlined style={{ color: '#ff7875', marginRight: '8px' }} />
            Fun Facts
          </Title>
          <List
            dataSource={wrappedData.FunFacts}
            renderItem={(fact: string, index: number) => (
              <List.Item>
                <div style={{ display: 'flex', alignItems: 'center' }}>
                  <span style={{ fontSize: '24px', marginRight: '12px' }}>
                    {['🎯', '⚡', '🔥', '💯', '🚀'][index % 5]}
                  </span>
                  <Text style={{ fontSize: '16px' }}>{fact}</Text>
                </div>
              </List.Item>
            )}
          />
        </Card>
      )}

      <div style={{ textAlign: 'center', padding: '32px' }}>
        <Title level={4} style={{ color: '#8c8c8c' }}>
          Thanks for an amazing {year} season! 🎉
        </Title>
      </div>
    </div>
  );
};

export default WrappedSeason;