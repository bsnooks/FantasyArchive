import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Typography, Card, Spin, Alert, Select, Space, Tag, List } from 'antd';
import { SwapOutlined, CalendarOutlined, UserOutlined, ArrowRightOutlined } from '@ant-design/icons';
import { TradeTreeIndex, TradeTreeSummary } from '../types/fantasy';
import FranchiseLogo from '../components/FranchiseLogo';
import './TradeTrees.css';

const { Title, Text } = Typography;
const { Option } = Select;

// Component for adversarial trade card layout
interface TradeMatchupCardProps {
  trade: TradeTreeSummary;
  complexity: { color: string; text: string };
  formatDate: (dateString: string) => string;
  selectedYear: number | null;
  getTradeParticipants: (transactionGroupId: string) => Promise<{franchiseId: string, franchiseName: string, color: string, players: string[]}[]>;
}

const TradeMatchupCard: React.FC<TradeMatchupCardProps> = ({ 
  trade, 
  complexity, 
  formatDate, 
  selectedYear,
  getTradeParticipants 
}) => {
  const [participants, setParticipants] = useState<{franchiseId: string, franchiseName: string, color: string, players: string[]}[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadParticipants = async () => {
      setLoading(true);
      const data = await getTradeParticipants(trade.TransactionGroupId);
      setParticipants(data);
      setLoading(false);
    };
    
    loadParticipants();
  }, [trade.TransactionGroupId, getTradeParticipants]);

  if (loading) {
    return (
      <Card className="trade-card" hoverable>
        <div style={{ textAlign: 'center', padding: '20px' }}>
          <Spin size="small" />
          <Text style={{ marginLeft: '8px' }}>Loading trade details...</Text>
        </div>
      </Card>
    );
  }

  // Split participants into two sides (typically there are 2 teams in most trades)
  const leftSide = participants[0];
  const rightSide = participants[1];
  const hasValidMatchup = participants.length >= 2;

  return (
    <Card className="trade-matchup-card" hoverable>
      <div className="trade-matchup-header">
        <Space size="small" wrap>
          <Tag icon={<CalendarOutlined />} color="green">
            {formatDate(trade.Date)}
          </Tag>
          <Tag icon={<UserOutlined />} color="blue">
            {trade.PlayersInvolved.length} players
          </Tag>
          <Tag color={complexity.color}>
            {complexity.text} Trade
          </Tag>
          {selectedYear && (
            <Tag color="blue">{selectedYear}</Tag>
          )}
        </Space>
      </div>
      
      <div className="trade-matchup-content">
        {hasValidMatchup ? (
          <div className="matchup-layout">
            <div className="team-side left-side" style={{ borderColor: leftSide.color }}>
              <div className="team-header" style={{ backgroundColor: leftSide.color }}>
                <div className="franchise-logo-wrapper" style={{ '--franchise-color': leftSide.color } as React.CSSProperties}>
                  <FranchiseLogo
                    franchiseId={leftSide.franchiseId}
                    franchiseName={leftSide.franchiseName}
                    size="medium"
                    className="rounded shadow"
                  />
                </div>
                <div className="team-name">
                  <Text strong style={{ color: 'white' }}>{leftSide.franchiseName}</Text>
                </div>
              </div>
              <div className="team-players">
                {leftSide.players.map(player => (
                  <Tag key={player} color="purple" style={{ margin: '2px' }}>
                    {player}
                  </Tag>
                ))}
              </div>
            </div>
            
            <div className="vs-divider">
              <ArrowRightOutlined style={{ fontSize: '24px', color: '#1890ff' }} />
              <Text type="secondary" style={{ display: 'block', marginTop: '4px' }}>TRADE</Text>
            </div>
            
            <div className="team-side right-side" style={{ borderColor: rightSide.color }}>
              <div className="team-header" style={{ backgroundColor: rightSide.color }}>
                <div className="franchise-logo-wrapper" style={{ '--franchise-color': rightSide.color } as React.CSSProperties}>
                  <FranchiseLogo
                    franchiseId={rightSide.franchiseId}
                    franchiseName={rightSide.franchiseName}
                    size="medium"
                    className="rounded shadow"
                  />
                </div>
                <div className="team-name">
                  <Text strong style={{ color: 'white' }}>{rightSide.franchiseName}</Text>
                </div>
              </div>
              <div className="team-players">
                {rightSide.players.map(player => (
                  <Tag key={player} color="orange" style={{ margin: '2px' }}>
                    {player}
                  </Tag>
                ))}
              </div>
            </div>
          </div>
        ) : (
          // Fallback for trades with more or fewer than 2 teams
          <div className="trade-fallback">
            <Title level={5} style={{ margin: 0, marginBottom: '12px' }}>
              {trade.Description}
            </Title>
            <div className="trade-participants">
              {participants.map((participant, index) => (
                <div key={participant.franchiseId} className="participant-section" style={{ marginBottom: '12px' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '6px' }}>
                    <FranchiseLogo
                      franchiseId={participant.franchiseId}
                      franchiseName={participant.franchiseName}
                      size="small"
                      className="rounded"
                    />
                    <Text strong style={{ color: participant.color }}>{participant.franchiseName}</Text>
                  </div>
                  <div>
                    {participant.players.map(player => (
                      <Tag key={player} color="purple" style={{ margin: '2px' }}>
                        {player}
                      </Tag>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </Card>
  );
};

const TradeTrees: React.FC = () => {
  const navigate = useNavigate();
  const [tradeTreeIndex, setTradeTreeIndex] = useState<TradeTreeIndex[]>([]);
  const [selectedYear, setSelectedYear] = useState<number | null>(null);
  const [selectedTrades, setSelectedTrades] = useState<TradeTreeSummary[]>([]);
  const [loadingIndex, setLoadingIndex] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Load trade tree index on component mount
  useEffect(() => {
    loadTradeTreeIndex();
  }, []);

  // Update selected trades when year changes
  useEffect(() => {
    if (selectedYear && tradeTreeIndex.length > 0) {
      const yearData = tradeTreeIndex.find(y => y.Year === selectedYear);
      setSelectedTrades(yearData?.Trades || []);
    } else {
      setSelectedTrades([]);
    }
  }, [selectedYear, tradeTreeIndex]);

  const loadTradeTreeIndex = async () => {
    try {
      setLoadingIndex(true);
      const response = await fetch('/data/trade-trees/index.json');
      if (!response.ok) {
        throw new Error('Failed to load trade tree index');
      }
      const data: TradeTreeIndex[] = await response.json();
      setTradeTreeIndex(data.sort((a, b) => b.Year - a.Year)); // Sort by year descending
      
      // Set the most recent year as default
      if (data.length > 0) {
        setSelectedYear(data[data.length - 1].Year);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load trade tree index');
    } finally {
      setLoadingIndex(false);
    }
  };

  // Get franchises involved in a trade from the enhanced trade data
  const getTradeParticipants = async (transactionGroupId: string): Promise<{franchiseId: string, franchiseName: string, color: string, players: string[]}[]> => {
    try {
      // Find the trade in our selected trades
      const trade = selectedTrades.find((t: TradeTreeSummary) => t.TransactionGroupId === transactionGroupId);
      if (!trade || !trade.TradeSides) {
        return [];
      }

      // Convert the detailed trade sides to the expected format
      return trade.TradeSides.map(side => ({
        franchiseId: side.FranchiseId,
        franchiseName: side.FranchiseName,
        color: side.FranchiseColor,
        players: side.Players.map(p => `${p.PlayerName} (${p.PrimaryPosition})`)
      }));
      
    } catch (error) {
      console.warn(`Failed to get trade participants for ${transactionGroupId}:`, error);
      return [];
    }
  };

  const formatDate = (dateString: string) => {
    try {
      return new Date(dateString).toLocaleDateString();
    } catch {
      return dateString;
    }
  };

  const getTradeComplexity = (playersCount: number): { color: string; text: string } => {
    if (playersCount <= 2) return { color: 'green', text: 'Simple' };
    if (playersCount <= 4) return { color: 'orange', text: 'Medium' };
    return { color: 'red', text: 'Complex' };
  };

  if (loadingIndex) {
    return (
      <div className="trade-trees-page">
        <div className="loading-container">
          <Spin size="large" />
          <Text>Loading trade trees...</Text>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="trade-trees-page">
        <Alert
          message="Error"
          description={error}
          type="error"
          showIcon
        />
      </div>
    );
  }

  return (
    <div className="trade-trees-page">
      <header className="page-header">
        <Title level={1}>
          <SwapOutlined /> Trade Trees
        </Title>
        <Text className="page-description">
          Explore the complete history of player trades and their subsequent movements
        </Text>
      </header>

      <div className="trade-trees-controls">
        <Space size="large" align="center" wrap>
          <div>
            <Text strong>Select Year: </Text>
            <Select
              value={selectedYear}
              onChange={setSelectedYear}
              style={{ width: 120 }}
              placeholder="Select year"
            >
              {tradeTreeIndex.map(yearData => (
                <Option key={yearData.Year} value={yearData.Year}>
                  {yearData.Year}
                </Option>
              ))}
            </Select>
          </div>
          
          {selectedYear && (
            <Tag icon={<CalendarOutlined />} color="blue">
              {selectedTrades.length} trades in {selectedYear}
            </Tag>
          )}
        </Space>
      </div>

      <div className="trade-trees-content">
        {selectedTrades.length > 0 ? (
          <List
            className="trades-list"
            dataSource={selectedTrades}
            renderItem={(trade) => {
              const complexity = getTradeComplexity(trade.PlayersInvolved.length);
              
              return (
                <List.Item
                  key={trade.TransactionGroupId}
                  className="trade-list-item"
                  onClick={() => navigate(`/trade/${trade.TransactionGroupId}`)}
                >
                  <TradeMatchupCard 
                    trade={trade}
                    complexity={complexity}
                    formatDate={formatDate}
                    selectedYear={selectedYear}
                    getTradeParticipants={getTradeParticipants}
                  />
                </List.Item>
              );
            }}
          />
        ) : selectedYear ? (
          <Card className="empty-state">
            <Text type="secondary">No trades found for {selectedYear}</Text>
          </Card>
        ) : (
          <Card className="empty-state">
            <Text type="secondary">Select a year to view trades</Text>
          </Card>
        )}
      </div>
    </div>
  );
};

export default TradeTrees;