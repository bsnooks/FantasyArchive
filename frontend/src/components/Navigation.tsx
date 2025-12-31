import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { Layout, Menu } from 'antd';
import { 
  HomeOutlined, 
  CalendarOutlined, 
  TeamOutlined, 
  TrophyOutlined,
  PartitionOutlined,
  GiftOutlined
} from '@ant-design/icons';
import type { MenuProps } from 'antd';
import './Navigation.css';

const { Header } = Layout;

const Navigation: React.FC = () => {
  const location = useLocation();

  const navItems = [
    { path: '/', label: 'Home', icon: <HomeOutlined /> },
    { path: '/records', label: 'Records', icon: <TrophyOutlined /> },
    { path: '/wrapped', label: 'Wrapped', icon: <GiftOutlined /> },
    { path: '/seasons', label: 'Seasons', icon: <CalendarOutlined /> },
    { path: '/franchises', label: 'Franchises', icon: <TeamOutlined /> },
    { path: '/trade-trees', label: 'Trade Trees', icon: <PartitionOutlined /> },
  ];

  const menuItems: MenuProps['items'] = navItems.map(item => ({
    key: item.path,
    icon: item.icon,
    label: (
      <Link to={item.path} style={{ textDecoration: 'none' }}>
        {item.label}
      </Link>
    ),
  }));

  return (
    <Header className="main-navigation">
      <div className="nav-brand">
        <Link to="/" style={{ display: 'flex', alignItems: 'center', textDecoration: 'none' }}>
          <img 
            src="/logo_white.png" 
            alt="Gibsons League" 
            className="nav-brand-logo"
          />
          <span className="nav-brand-text">Gibsons League</span>
        </Link>
      </div>
      
      {/* Desktop Menu */}
      <Menu
        theme="dark"
        mode="horizontal"
        selectedKeys={[location.pathname]}
        items={menuItems}
        className="nav-menu desktop-menu"
      />
      
      {/* Mobile Menu */}
      <div className="mobile-menu">
        {navItems.map(item => (
          <Link
            key={item.path}
            to={item.path}
            className={`mobile-nav-item ${location.pathname === item.path ? 'active' : ''}`}
          >
            {item.icon}
            <span className="mobile-nav-label">{item.label}</span>
          </Link>
        ))}
      </div>
    </Header>
  );
};

export default Navigation;