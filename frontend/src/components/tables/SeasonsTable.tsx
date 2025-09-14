import React from 'react';
import { Table, Space } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import FranchiseLogo from '../FranchiseLogo';
import { SeasonLink, FranchiseLink } from '../links';
import './tables.css';

interface Team {
  FranchiseId: string;
  FranchiseName: string;
  Champion: boolean;
  SecondPlace: boolean;
  Wins: number;
  Losses: number;
  Ties: number;
  Points: number;
}

interface Season {
  Year: number;
  Name: string;
  IsActive: boolean;
  CurrentWeek: number;
  Teams: Team[];
}

interface SeasonsTableProps {
  seasons: Season[];
  loading?: boolean;
  showActions?: boolean;
}

const SeasonsTable: React.FC<SeasonsTableProps> = ({ 
  seasons, 
  loading = false, 
  showActions = true 
}) => {
  const columns: ColumnsType<Season> = [
    {
      title: 'Year',
      dataIndex: 'Year',
      key: 'year',
      width: 80,
      sorter: (a: Season, b: Season) => a.Year - b.Year,
      defaultSortOrder: 'descend',
      render: (year: number, record: Season) => (
        showActions ? (
          <SeasonLink 
            season={year}
            to={`/season/${year}`}
            clearFranchiseContext={true}
            style={{ 
              fontWeight: 600, 
              color: '#1a2b4c',
              textDecoration: 'none',
              fontSize: '1.1rem'
            }}
          >
            {year}
          </SeasonLink>
        ) : (
          <span style={{ fontWeight: 600, color: '#1a2b4c', fontSize: '1.1rem' }}>
            {year}
          </span>
        )
      ),
    },
    {
      title: 'Champion',
      key: 'champion',
      width: 200,
      render: (record: Season) => {
        const champion = record.Teams.find(team => team.Champion);
        return champion ? (
          <Space>
            <FranchiseLogo 
              franchiseId={champion.FranchiseId} 
              franchiseName={champion.FranchiseName}
              size="small"
            />
            <FranchiseLink
              franchiseId={champion.FranchiseId}
              franchiseName={champion.FranchiseName}
              to={`/franchise/${champion.FranchiseId}/season/${record.Year}`}
              preserveSeasonContext={true}
              style={{ fontWeight: 600, color: '#1a2b4c' }}
            >
              {champion.FranchiseName}
            </FranchiseLink>
            <span style={{ 
              fontFamily: 'Monaco, Consolas, monospace', 
              color: '#6c757d',
              fontSize: '0.9rem'
            }}>
              ({champion.Wins}-{champion.Losses}
              {champion.Ties > 0 && `-${champion.Ties}`})
            </span>
          </Space>
        ) : (
          <span style={{ color: '#adb5bd' }}>—</span>
        );
      },
    },
    {
      title: 'Runner-up',
      key: 'runnerUp',
      width: 180,
      render: (record: Season) => {
        const runnerUp = record.Teams.find(team => team.SecondPlace);
        return runnerUp ? (
          <Space>
            <FranchiseLogo 
              franchiseId={runnerUp.FranchiseId} 
              franchiseName={runnerUp.FranchiseName}
              size="small"
            />
            <FranchiseLink
              franchiseId={runnerUp.FranchiseId}
              franchiseName={runnerUp.FranchiseName}
              to={`/franchise/${runnerUp.FranchiseId}/season/${record.Year}`}
              preserveSeasonContext={true}
              style={{ fontWeight: 600, color: '#1a2b4c' }}
            >
              {runnerUp.FranchiseName}
            </FranchiseLink>
          </Space>
        ) : (
          <span style={{ color: '#adb5bd' }}>—</span>
        );
      },
    },
    {
      title: 'Total Points',
      key: 'totalPoints',
      width: 120,
      align: 'right',
      sorter: (a: Season, b: Season) => {
        const totalA = a.Teams.reduce((sum, team) => sum + team.Points, 0);
        const totalB = b.Teams.reduce((sum, team) => sum + team.Points, 0);
        return totalA - totalB;
      },
      render: (record: Season) => {
        const totalPoints = record.Teams.reduce((sum, team) => sum + team.Points, 0);
        return (
          <span style={{ 
            fontFamily: 'Monaco, Consolas, monospace', 
            fontWeight: 600, 
            color: '#495057' 
          }}>
            {totalPoints.toLocaleString('en-US', { 
              minimumFractionDigits: 2, 
              maximumFractionDigits: 2 
            })}
          </span>
        );
      },
    },
    {
      title: 'High Score',
      key: 'highScore',
      width: 100,
      align: 'right',
      sorter: (a: Season, b: Season) => {
        const highA = Math.max(...a.Teams.map(team => team.Points));
        const highB = Math.max(...b.Teams.map(team => team.Points));
        return highA - highB;
      },
      render: (record: Season) => {
        const highScore = Math.max(...record.Teams.map(team => team.Points));
        return (
          <span style={{ 
            fontFamily: 'Monaco, Consolas, monospace', 
            fontWeight: 600, 
            color: '#495057' 
          }}>
            {highScore.toLocaleString('en-US', { 
              minimumFractionDigits: 2, 
              maximumFractionDigits: 2 
            })}
          </span>
        );
      },
    },
  ];

  return (
    <Table
      columns={columns}
      dataSource={seasons}
      rowKey="Year"
      loading={loading}
      pagination={false}
      size="middle"
      style={{
        background: 'white',
        borderRadius: 12,
        overflow: 'hidden',
        boxShadow: '0 4px 6px rgba(0, 0, 0, 0.1)',
      }}
      rowClassName={() => 'seasons-table-row'}
      scroll={{ x: 800 }}
    />
  );
};

export default SeasonsTable;