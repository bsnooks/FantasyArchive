import React from 'react';
import { Table, Space, Statistic } from 'antd';
import { TrophyOutlined, StarOutlined, UserOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import FranchiseLogo from '../FranchiseLogo';
import { FranchiseLink } from '../links';
import './tables.css';

interface Team {
  Id: string;
  FranchiseId: string;
  FranchiseName: string;
  TeamName: string;
  Owner: string;
  Standing: number;
  Wins: number;
  Losses: number;
  Ties: number;
  Points: number;
  Champion: boolean;
  SecondPlace: boolean;
}

interface SeasonStandingsTableProps {
  teams: Team[];
  year?: number; // The season year for building team season URLs
  loading?: boolean;
  showActions?: boolean;
}

const SeasonStandingsTable: React.FC<SeasonStandingsTableProps> = ({ 
  teams, 
  year,
  loading = false, 
  showActions = true 
}) => {

  // Sort teams by standing
  const sortedTeams = [...teams].sort((a, b) => a.Standing - b.Standing);

  const columns: ColumnsType<Team> = [
    {
      title: 'Pos',
      key: 'standing',
      width: 70,
      align: 'center',
      render: (record: Team) => (
        <span style={{ 
          fontWeight: 600, 
          color: '#495057',
          fontSize: '1.1rem'
        }}>
          #{record.Standing}
        </span>
      ),
    },
    {
      title: 'Team',
      key: 'team',
      width: 320,
      render: (record: Team) => (
        <Space size="middle">
          <FranchiseLogo 
            franchiseId={record.FranchiseId} 
            franchiseName={record.FranchiseName}
            size="medium"
          />
          <div style={{ lineHeight: 1.2 }}>
            {showActions ? (
              <FranchiseLink 
                franchiseId={record.FranchiseId}
                franchiseName={record.FranchiseName}
                to={year ? `/franchise/${record.FranchiseId}/season/${year}` : `/franchise/${record.FranchiseId}`}
                preserveSeasonContext={true}
                style={{ 
                  fontWeight: 600, 
                  color: '#1a2b4c',
                  textDecoration: 'none',
                  fontSize: '1.05rem'
                }}
              >
                {record.FranchiseName}
              </FranchiseLink>
            ) : (
              <span style={{ 
                fontWeight: 600, 
                color: '#1a2b4c',
                fontSize: '1.05rem'
              }}>
                {record.FranchiseName}
              </span>
            )}
            <div style={{ 
              color: '#6c757d', 
              fontSize: '0.9rem',
              marginTop: '2px'
            }}>
              {record.TeamName}
            </div>
            <div style={{ 
              color: '#868e96', 
              fontSize: '0.85rem',
              marginTop: '1px'
            }}>
              <UserOutlined style={{ marginRight: '4px' }} />
              {record.Owner}
            </div>
          </div>
        </Space>
      ),
    },
    {
      title: 'Record',
      key: 'record',
      width: 120,
      align: 'center',
      sorter: (a: Team, b: Team) => {
        const winPctA = a.Wins / (a.Wins + a.Losses + a.Ties);
        const winPctB = b.Wins / (b.Wins + b.Losses + b.Ties);
        return winPctA - winPctB;
      },
      render: (record: Team) => (
        <div style={{ textAlign: 'center' }}>
          <div style={{ 
            fontFamily: 'Monaco, Consolas, monospace', 
            fontWeight: 600, 
            color: '#495057',
            fontSize: '1rem'
          }}>
            {record.Wins}-{record.Losses}
            {record.Ties > 0 && `-${record.Ties}`}
          </div>
          <div style={{ 
            fontSize: '0.8rem', 
            color: '#6c757d',
            marginTop: '2px'
          }}>
            {((record.Wins / (record.Wins + record.Losses + record.Ties)) * 100).toFixed(1)}%
          </div>
        </div>
      ),
    },
    {
      title: 'Points',
      key: 'points',
      width: 120,
      align: 'right',
      sorter: (a: Team, b: Team) => a.Points - b.Points,
      render: (record: Team) => (
        <Statistic
          value={record.Points}
          precision={1}
          valueStyle={{ 
            fontFamily: 'Monaco, Consolas, monospace',
            fontSize: '1rem',
            color: '#495057'
          }}
        />
      ),
    },
    {
      title: 'Awards',
      key: 'awards',
      width: 90,
      align: 'center',
      render: (record: Team) => (
        <Space>
          {record.Champion && (
            <TrophyOutlined 
              style={{ color: '#d4a615', fontSize: '18px' }} 
              title="Champion"
            />
          )}
          {record.SecondPlace && (
            <StarOutlined 
              style={{ color: '#c0c0c0', fontSize: '18px' }} 
              title="Runner-up"
            />
          )}
        </Space>
      ),
    },
  ];

  return (
    <Table
      columns={columns}
      dataSource={sortedTeams}
      rowKey="Id"
      loading={loading}
      pagination={false}
      size="middle"
      style={{
        background: 'white',
        borderRadius: 12,
        overflow: 'hidden',
        boxShadow: '0 4px 6px rgba(0, 0, 0, 0.1)',
      }}
      rowClassName={(record) => {
        if (record.Champion) return 'champion-row';
        if (record.SecondPlace) return 'runner-up-row';
        return 'season-standings-row';
      }}
      scroll={{ x: 720 }}
    />
  );
};

export default SeasonStandingsTable;