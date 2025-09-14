import React from 'react';
import { Table, Space } from 'antd';
import { TrophyOutlined, StarOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import FranchiseLogo from '../FranchiseLogo';
import { FranchiseLink } from '../links';
import './tables.css';

interface Franchise {
  Id: string;
  Name: string;
  Owner: string;
  totalWins: number;
  totalLosses: number;
  totalTies: number;
  championships: number;
  secondPlaces: number;
  totalPoints: number;
  winPercentage: number;
}

interface FranchiseTableProps {
  franchises: Franchise[];
  loading?: boolean;
  showRank?: boolean;
  showActions?: boolean;
}

const FranchiseTable: React.FC<FranchiseTableProps> = ({ 
  franchises, 
  loading = false, 
  showRank = true,
  showActions = true 
}) => {
  const columns: ColumnsType<Franchise> = [
    ...(showRank ? [{
      title: 'Rank',
      key: 'rank',
      width: 60,
      align: 'center' as const,
      render: (_: any, __: Franchise, index: number) => (
        <span style={{ fontWeight: 600, color: '#495057' }}>
          #{index + 1}
        </span>
      ),
    }] : []),
    {
      title: 'Franchise',
      dataIndex: 'Name',
      key: 'franchise',
      width: 250,
      render: (name: string, record: Franchise) => (
        <Space size="middle">
          <FranchiseLogo 
            franchiseId={record.Id} 
            franchiseName={name}
            size="small"
          />
          {showActions ? (
            <FranchiseLink 
              franchiseId={record.Id}
              franchiseName={name}
              to={`/franchise/${record.Id}`}
              preserveSeasonContext={false}
              style={{ 
                fontWeight: 600, 
                color: '#1a2b4c',
                textDecoration: 'none'
              }}
            >
              {name}
            </FranchiseLink>
          ) : (
            <span style={{ fontWeight: 600, color: '#1a2b4c' }}>
              {name}
            </span>
          )}
        </Space>
      ),
    },
    {
      title: 'Current Owner',
      dataIndex: 'Owner',
      key: 'owner',
      width: 150,
      render: (owner: string) => (
        <span style={{ color: '#6c757d', fontWeight: 500 }}>
          {owner}
        </span>
      ),
    },
    {
      title: 'All-Time Record',
      key: 'record',
      width: 120,
      render: (record: Franchise) => (
        <span style={{ 
          fontFamily: 'Monaco, Consolas, monospace', 
          fontWeight: 600, 
          color: '#495057' 
        }}>
          {record.totalWins}-{record.totalLosses}
          {record.totalTies > 0 && `-${record.totalTies}`}
        </span>
      ),
    },
    {
      title: 'Win %',
      key: 'winPercentage',
      width: 80,
      sorter: (a: Franchise, b: Franchise) => a.winPercentage - b.winPercentage,
      render: (record: Franchise) => (
        <span style={{ 
          fontFamily: 'Monaco, Consolas, monospace', 
          fontWeight: 600, 
          color: '#495057' 
        }}>
          {(record.winPercentage * 100).toFixed(1)}%
        </span>
      ),
    },
    {
      title: 'Championships',
      key: 'championships',
      width: 120,
      align: 'center' as const,
      sorter: (a: Franchise, b: Franchise) => a.championships - b.championships,
      render: (record: Franchise) => (
        record.championships > 0 ? (
          <Space>
            <TrophyOutlined style={{ color: '#d4a615' }} />
            <span style={{ fontWeight: 600 }}>{record.championships}</span>
          </Space>
        ) : null
      ),
    },
    {
      title: 'Runner-ups',
      key: 'secondPlaces',
      width: 120,
      align: 'center' as const,
      sorter: (a: Franchise, b: Franchise) => a.secondPlaces - b.secondPlaces,
      render: (record: Franchise) => (
        record.secondPlaces > 0 ? (
          <Space>
            <StarOutlined style={{ color: '#c0c0c0' }} />
            <span style={{ fontWeight: 600 }}>{record.secondPlaces}</span>
          </Space>
        ) : null
      ),
    },
    {
      title: 'Total Points',
      key: 'totalPoints',
      width: 120,
      align: 'right' as const,
      sorter: (a: Franchise, b: Franchise) => a.totalPoints - b.totalPoints,
      render: (record: Franchise) => (
        <span style={{ 
          fontFamily: 'Monaco, Consolas, monospace', 
          fontWeight: 600, 
          color: '#495057' 
        }}>
          {record.totalPoints.toLocaleString('en-US', { 
            minimumFractionDigits: 2, 
            maximumFractionDigits: 2 
          })}
        </span>
      ),
    },
  ];

  return (
    <Table
      columns={columns}
      dataSource={franchises}
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
      rowClassName={() => 'franchise-table-row'}
      scroll={{ x: 800 }}
    />
  );
};

export default FranchiseTable;