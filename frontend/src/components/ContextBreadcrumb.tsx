import React from 'react';
import { Breadcrumb, Space, Dropdown, Button } from 'antd';
import { useNavigate } from 'react-router-dom';
import { 
  HomeOutlined, 
  TeamOutlined, 
  CalendarOutlined, 
  DownOutlined 
} from '@ant-design/icons';
import type { MenuProps } from 'antd';
import { useAppSelector, useAppDispatch } from '../store/hooks';
import { setSelectedFranchise, setSelectedSeason, clearSelectedFranchise, clearSelectedSeason } from '../store/contextSlice';
import { useFranchises, useSeasons } from '../hooks/useFantasyData';
import './ContextBreadcrumb.css';

const ContextBreadcrumb: React.FC = () => {
  const { selectedFranchiseId, selectedFranchiseName, selectedSeason } = useAppSelector(
    (state) => state.context
  );
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { data: franchises } = useFranchises();
  const { data: seasons } = useSeasons();

  // Handle franchise switching
  const handleFranchiseChange = (franchiseId: string, franchiseName: string) => {
    dispatch(setSelectedFranchise({ id: franchiseId, name: franchiseName, preserveSeasonContext: true }));
    // Navigate to franchise page, preserving season context if it exists
    if (selectedSeason) {
      navigate(`/franchise/${franchiseId}/season/${selectedSeason}`);
    } else {
      navigate(`/franchise/${franchiseId}`);
    }
  };

  // Handle season switching
  const handleSeasonChange = (year: number) => {
    dispatch(setSelectedSeason(year));
    // Navigate to season page, preserving franchise context if it exists
    if (selectedFranchiseId) {
      navigate(`/franchise/${selectedFranchiseId}/season/${year}`);
    } else {
      navigate(`/season/${year}`);
    }
  };

  // Create franchise dropdown menu
  const franchiseMenuItems: MenuProps['items'] = franchises
    ?.sort((a, b) => a.Name.localeCompare(b.Name)) // Sort alphabetically
    .map(franchise => ({
      key: franchise.Id,
      label: franchise.Name,
      onClick: () => handleFranchiseChange(franchise.Id, franchise.Name),
    })) || [];

  // Create season dropdown menu
  const seasonMenuItems: MenuProps['items'] = seasons
    ?.sort((a, b) => b.Year - a.Year) // Sort by year descending (most recent first)
    .map(season => ({
      key: season.Year,
      label: `${season.Year} - ${season.Name}`,
      onClick: () => handleSeasonChange(season.Year),
    })) || [];

  // Show breadcrumb if we have any context (season or franchise)
  if (!selectedSeason && !selectedFranchiseId) {
    return null; // Don't show breadcrumb when no context is selected
  }

  // Handle going home (clear all context)
  const handleGoHome = () => {
    dispatch(clearSelectedFranchise());
    dispatch(clearSelectedSeason());
    navigate('/');
  };

  const items = [
    {
      title: (
        <Button 
          type="text" 
          className="breadcrumb-dropdown"
          onClick={handleGoHome}
        >
          <Space>
            <HomeOutlined />
            <span>Home</span>
          </Space>
        </Button>
      ),
    },
  ];

  // Add season context if selected (season has priority)
  if (selectedSeason) {
    items.push({
      title: (
        <Dropdown 
          menu={{ items: seasonMenuItems }}
          trigger={['click']}
          placement="bottomLeft"
        >
          <Button type="text" className="breadcrumb-dropdown">
            <Space>
              <CalendarOutlined />
              <span>{selectedSeason} Season</span>
              <DownOutlined />
            </Space>
          </Button>
        </Dropdown>
      ),
    });
  }

  // Add franchise context if selected
  if (selectedFranchiseId && selectedFranchiseName) {
    items.push({
      title: (
        <Dropdown 
          menu={{ items: franchiseMenuItems }}
          trigger={['click']}
          placement="bottomLeft"
        >
          <Button type="text" className="breadcrumb-dropdown">
            <Space>
              <TeamOutlined />
              <span>{selectedFranchiseName}</span>
              <DownOutlined />
            </Space>
          </Button>
        </Dropdown>
      ),
    });
  }

  return (
    <div className="context-breadcrumb">
      <Breadcrumb items={items} />
    </div>
  );
};

export default ContextBreadcrumb;