import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Typography, Button, Spin, Alert, Tag, Space } from 'antd';
import { CloseOutlined, CalendarOutlined, UserOutlined } from '@ant-design/icons';
import { TradeTreeSummary, TradeTreeIndex } from '../types/fantasy';
import MermaidRenderer from '../components/MermaidRenderer';
import { useMermaidDiagram } from '../hooks/useMermaidDiagram';
import './TradeDetail.css';

const { Title, Text } = Typography;

const TradeDetail: React.FC = () => {
  const { tradeId } = useParams<{ tradeId: string }>();
  const navigate = useNavigate();
  
  const [tradeSummary, setTradeSummary] = useState<TradeTreeSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Use the new hook to load Mermaid diagrams - no longer needs tradeTree
  const { 
    diagram: mermaidDiagram, 
    loading: diagramLoading, 
    error: diagramError
  } = useMermaidDiagram(tradeId || '');

  useEffect(() => {
    if (!tradeId) {
      navigate('/trade-trees');
      return;
    }
    
    const loadTradeData = async () => {
      try {
        setLoading(true);
        setError(null);

        // Load trade summary from the index
        const indexResponse = await fetch('/data/trade-trees/index.json');
        if (!indexResponse.ok) {
          throw new Error(`Trade index not found: ${indexResponse.status}`);
        }
        
        const indexData: TradeTreeIndex[] = await indexResponse.json();
        
        // Find the trade in the index
        let foundTrade: TradeTreeSummary | null = null;
        for (const yearData of indexData) {
          const trade = yearData.Trades.find(t => t.TransactionGroupId === tradeId);
          if (trade) {
            foundTrade = trade;
            break;
          }
        }
        
        if (!foundTrade) {
          throw new Error('Trade not found in index');
        }
        
        setTradeSummary(foundTrade);

      } catch (err) {
        console.error('Failed to load trade data:', err);
        setError(err instanceof Error ? err.message : 'Failed to load trade data');
      } finally {
        setLoading(false);
      }
    };
    
    loadTradeData();
  }, [tradeId, navigate]);

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

  if (loading || diagramLoading) {
    return (
      <div className="trade-detail-page">
        <div className="trade-detail-loading">
          <Spin size="large" />
          <Text style={{ marginTop: '16px', display: 'block' }}>
            {loading ? 'Loading trade details...' : 'Loading diagram...'}
          </Text>
        </div>
      </div>
    );
  }

  if (error || !tradeSummary || diagramError) {
    return (
      <div className="trade-detail-page">
        <div className="trade-detail-error">
          <Alert
            message="Failed to Load Trade"
            description={error || diagramError || 'Trade data not found'}
            type="error"
            showIcon
            action={
              <Space>
                <Button onClick={() => window.location.reload()}>Retry</Button>
                <Button type="primary" onClick={() => navigate('/trade-trees')}>
                  Back to Trade List
                </Button>
              </Space>
            }
          />
        </div>
      </div>
    );
  }

  const summary = tradeSummary;
  const complexity = summary ? getTradeComplexity(summary.PlayersInvolved.length) : null;

  return (
    <div className="trade-detail-page">
      {/* Trade details header - positioned below navigation */}
      <div className="trade-detail-header">
        <div className="trade-detail-title">
          <Title level={2} style={{ margin: 0 }}>
            {summary?.Description || 'Trade Details'}
          </Title>
          
          <div className="trade-detail-meta">
            <Space size="small" wrap>
              {summary && (
                <>
                  <Tag icon={<CalendarOutlined />} color="green">
                    {formatDate(summary.Date)}
                  </Tag>
                  <Tag icon={<UserOutlined />} color="blue">
                    {summary.PlayersInvolved.length} players
                  </Tag>
                  {complexity && (
                    <Tag color={complexity.color}>
                      {complexity.text} Trade
                    </Tag>
                  )}
                </>
              )}
            </Space>
          </div>
        </div>
        
        <div className="trade-detail-close">
          <Button
            icon={<CloseOutlined />}
            onClick={() => navigate('/trade-trees')}
            type="text"
            size="large"
            shape="circle"
          />
        </div>
      </div>

      {/* Diagram content */}
      <div className="trade-detail-content">
        {mermaidDiagram && (
          <MermaidRenderer
            diagram={mermaidDiagram}
            title="Trade Tree"
            height="100%"
            usePageScroll={false}
            enablePanZoom={true}
          />
        )}
      </div>
    </div>
  );
};

export default TradeDetail;