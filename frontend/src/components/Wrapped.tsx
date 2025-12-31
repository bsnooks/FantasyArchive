import React from 'react';
import { Card, Row, Col, Typography, Space, Button, Divider } from 'antd';
import { CalendarOutlined, TrophyOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';
import { useFranchises, useSeasons } from '../hooks/useFantasyData';

const { Title, Text } = Typography;

const Wrapped: React.FC = () => {
  const { data: franchises } = useFranchises();
  const { data: seasons } = useSeasons();

  const sortedSeasons = seasons?.sort((a, b) => b.Year - a.Year) || [];
  const sortedFranchises = franchises?.sort((a, b) => a.Name.localeCompare(b.Name)) || [];

  return (
    <div style={{ padding: '24px', maxWidth: '1200px', margin: '0 auto' }}>
      {/* Header */}
      <div style={{ textAlign: 'center', marginBottom: '48px' }}>
        <Title level={1} style={{ 
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          WebkitBackgroundClip: 'text',
          WebkitTextFillColor: 'transparent',
          marginBottom: '16px'
        }}>
          🎁 Fantasy Archive Wrapped
        </Title>
        <Text type="secondary" style={{ fontSize: '18px' }}>
          Relive your fantasy football memories with personalized season summaries
        </Text>
      </div>

      {/* Seasons List */}
      <Row gutter={[24, 24]}>
        {sortedSeasons.map((season) => (
          <Col xs={24} sm={12} lg={8} xl={6} key={season.Year}>
            <Card
              hoverable
              style={{
                background: 'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)',
                border: '1px solid #e8e8e8',
                borderRadius: '12px',
                overflow: 'hidden'
              }}
            >
              <div style={{ textAlign: 'center', marginBottom: '20px' }}>
                <CalendarOutlined style={{ fontSize: '32px', color: '#1890ff', marginBottom: '12px' }} />
                <Title level={3} style={{ margin: '0 0 8px 0' }}>
                  {season.Year} Season
                </Title>
                <Text type="secondary">{season.Name}</Text>
              </div>

              <Divider style={{ margin: '20px 0' }} />

              <div>
                <Text strong style={{ display: 'block', marginBottom: '12px', color: '#595959' }}>
                  <TrophyOutlined style={{ marginRight: '8px' }} />
                  Franchise Wrapped:
                </Text>
                <Space direction="vertical" style={{ width: '100%' }} size="small">
                  {sortedFranchises.map((franchise) => (
                    <Link
                      key={franchise.Id}
                      to={`/franchise/${franchise.Id}/season/${season.Year}/wrapped`}
                      style={{ width: '100%' }}
                    >
                      <Button
                        type="text"
                        style={{
                          width: '100%',
                          textAlign: 'left',
                          height: 'auto',
                          padding: '12px 16px',
                          background: 'rgba(255, 255, 255, 0.7)',
                          border: '1px solid rgba(24, 144, 255, 0.2)',
                          borderRadius: '6px',
                          fontSize: '14px',
                          display: 'flex',
                          flexDirection: 'column',
                          alignItems: 'flex-start',
                          justifyContent: 'center',
                          minHeight: '60px'
                        }}
                        onMouseEnter={(e) => {
                          e.currentTarget.style.background = 'rgba(24, 144, 255, 0.1)';
                          e.currentTarget.style.borderColor = '#1890ff';
                        }}
                        onMouseLeave={(e) => {
                          e.currentTarget.style.background = 'rgba(255, 255, 255, 0.7)';
                          e.currentTarget.style.borderColor = 'rgba(24, 144, 255, 0.2)';
                        }}
                      >
                        <div style={{ width: '100%' }}>
                          <div style={{ 
                            fontWeight: 500, 
                            marginBottom: '4px',
                            lineHeight: '1.2'
                          }}>
                            {franchise.Name}
                          </div>
                          <Text type="secondary" style={{ 
                            fontSize: '12px',
                            lineHeight: '1.2'
                          }}>
                            {franchise.Owner}
                          </Text>
                        </div>
                      </Button>
                    </Link>
                  ))}
                </Space>
              </div>
            </Card>
          </Col>
        ))}
      </Row>

      {/* Footer */}
      <div style={{ textAlign: 'center', marginTop: '48px', padding: '24px' }}>
        <Text type="secondary">
          🎯 Click on any franchise to see their personalized season summary with stats, highlights, and fun insights!
        </Text>
      </div>
    </div>
  );
};

export default Wrapped;