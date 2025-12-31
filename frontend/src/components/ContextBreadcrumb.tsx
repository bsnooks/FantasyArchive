import React, { useState } from "react";
import { Breadcrumb, Space, Dropdown, Button } from "antd";
import { useNavigate, useLocation } from "react-router-dom";
import {
  HomeOutlined,
  TeamOutlined,
  CalendarOutlined,
  DownOutlined,
} from "@ant-design/icons";
import type { MenuProps } from "antd";
import { useAppSelector, useAppDispatch } from "../store/hooks";
import {
  clearSelectedFranchise,
  clearSelectedSeason,
  setSelectedFranchise,
  setSelectedSeason,
} from "../store/contextSlice";
import { useFranchises, useSeasons } from "../hooks/useFantasyData";
import "./ContextBreadcrumb.css";

const ContextBreadcrumb: React.FC = () => {
  const { selectedFranchiseId, selectedFranchiseName, selectedSeason } =
    useAppSelector((state) => state.context);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const location = useLocation();
  const { data: franchises } = useFranchises();
  const { data: seasons } = useSeasons();
  const [seasonDropdownOpen, setSeasonDropdownOpen] = useState(false);

  // Don't show breadcrumb on homepage, trade detail pages, or wrapped page
  if (location.pathname === '/' || location.pathname.startsWith('/trade/') || location.pathname.endsWith('/wrapped') ||location.pathname === '/wrapped') {
    return null;
  }

  // Check if we're on the records page
  const isRecordsPage = location.pathname === '/records';

  // Handle franchise switching
  const handleFranchiseChange = (
    franchiseId: string,
    franchiseName: string
  ) => {
    dispatch(setSelectedFranchise({ 
      id: franchiseId, 
      name: franchiseName, 
      preserveSeasonContext: true 
    }));
    
    if (isRecordsPage) {
      // Stay on records page but update context
      navigate('/records');
    } else {
      // Navigate to franchise page, preserving season context if it exists
      if (selectedSeason) {
        navigate(`/franchise/${franchiseId}/season/${selectedSeason}`);
      } else {
        navigate(`/franchise/${franchiseId}`);
      }
    }
  };

  // Handle season switching
  const handleSeasonChange = (year: number) => {
    dispatch(setSelectedSeason(year));
    setSeasonDropdownOpen(false); // Close dropdown after selection
    
    if (isRecordsPage) {
      // Stay on records page but update context
      navigate('/records');
    } else {
      // Navigate to season page, preserving franchise context if it exists
      if (selectedFranchiseId) {
        navigate(`/franchise/${selectedFranchiseId}/season/${year}`);
      } else {
        navigate(`/season/${year}`);
      }
    }
  };

  // Handle clearing franchise context
  const handleClearFranchise = () => {
    dispatch(clearSelectedFranchise());
    if (isRecordsPage) {
      // Stay on records page but clear franchise context
      navigate('/records');
    } else {
      if (selectedSeason) {
        navigate(`/season/${selectedSeason}`);
      } else {
        navigate("/franchises");
      }
    }
  };

  // Handle clearing season context
  const handleClearSeason = () => {
    dispatch(clearSelectedSeason());
    setSeasonDropdownOpen(false); // Close dropdown after selection
    if (isRecordsPage) {
      // Stay on records page but clear season context
      navigate('/records');
    } else {
      if (selectedFranchiseId) {
        navigate(`/franchise/${selectedFranchiseId}`);
      } else {
        navigate("/seasons");
      }
    }
  };

  // Create franchise dropdown menu
  const franchiseMenuItems: MenuProps["items"] = [
    ...(franchises
      ?.sort((a, b) => a.Name.localeCompare(b.Name)) // Sort alphabetically
      .map((franchise) => ({
        key: franchise.Id,
        label: franchise.Name,
        onClick: () => handleFranchiseChange(franchise.Id, franchise.Name),
      })) || []),
    // Add separator and "All Franchises" option
    {
      type: "divider",
    },
    {
      key: "all-franchises",
      label: "All Franchises",
      onClick: handleClearFranchise,
    },
  ];

  // Create season dropdown menu with multiple columns for mobile
  const seasonDropdownOverlay = (
    <div style={{ 
      background: 'white', 
      borderRadius: '8px', 
      boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
      padding: '8px',
      minWidth: '320px',
      maxHeight: '400px',
      overflowY: 'auto'
    }}>
      <div style={{ 
        display: 'grid', 
        gridTemplateColumns: 'repeat(auto-fill, minmax(80px, 1fr))',
        gap: '4px'
      }}>
        {seasons
          ?.sort((a, b) => b.Year - a.Year)
          .map((season) => (
            <Button
              key={season.Year}
              type="text"
              size="small"
              onClick={() => handleSeasonChange(season.Year)}
              style={{
                height: '32px',
                padding: '4px 8px',
                fontSize: '13px',
                textAlign: 'center',
                border: selectedSeason === season.Year ? '1px solid #1890ff' : '1px solid transparent',
                backgroundColor: selectedSeason === season.Year ? '#e6f7ff' : 'transparent'
              }}
            >
              {season.Year}
            </Button>
          ))}
      </div>
      <div style={{ borderTop: '1px solid #f0f0f0', marginTop: '8px', paddingTop: '8px' }}>
        <Button
          type="text"
          size="small"
          onClick={handleClearSeason}
          style={{
            width: '100%',
            height: '32px',
            fontSize: '13px',
            border: !selectedSeason ? '1px solid #1890ff' : '1px solid transparent',
            backgroundColor: !selectedSeason ? '#e6f7ff' : 'transparent'
          }}
        >
          All Seasons
        </Button>
      </div>
    </div>
  );

  // Handle going home (clear all context)
  const handleGoHome = () => {
    dispatch(clearSelectedFranchise());
    dispatch(clearSelectedSeason());
    navigate("/");
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

  // Always add season context (show "All Time" if no season selected)
  items.push({
    title: (
      <Dropdown
        dropdownRender={() => seasonDropdownOverlay}
        trigger={["click"]}
        placement="bottomLeft"
        open={seasonDropdownOpen}
        onOpenChange={setSeasonDropdownOpen}
      >
        <Button type="text" className="breadcrumb-dropdown">
          <Space>
            <CalendarOutlined />
            <span>{selectedSeason ? `${selectedSeason} Season` : "All Time"}</span>
            <DownOutlined />
          </Space>
        </Button>
      </Dropdown>
    ),
  });

  // Always add franchise context (show "All Franchises" if no franchise selected)
  items.push({
    title: (
      <Dropdown
        menu={{ items: franchiseMenuItems }}
        trigger={["click"]}
        placement="bottomLeft"
      >
        <Button type="text" className="breadcrumb-dropdown">
          <Space>
            <TeamOutlined />
            <span>{selectedFranchiseName || "All Franchises"}</span>
            <DownOutlined />
          </Space>
        </Button>
      </Dropdown>
    ),
  });

  return (
    <div className="context-breadcrumb">
      <Breadcrumb items={items} />
    </div>
  );
};

export default ContextBreadcrumb;
