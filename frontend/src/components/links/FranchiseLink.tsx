import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAppDispatch } from '../../store/hooks';
import { setSelectedFranchise } from '../../store/contextSlice';

interface FranchiseLinkProps {
  franchiseId: string;
  franchiseName: string;
  to: string;
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
  preserveSeasonContext?: boolean; // When true, maintains current season context
}

export const FranchiseLink: React.FC<FranchiseLinkProps> = ({
  franchiseId,
  franchiseName,
  to,
  children,
  className,
  style,
  preserveSeasonContext = true
}) => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const handleClick = (e: React.MouseEvent) => {
    e.preventDefault();
    dispatch(setSelectedFranchise({
      id: franchiseId,
      name: franchiseName,
      preserveSeasonContext
    }));
    // Navigate using React Router
    navigate(to);
  };

  return (
    <Link 
      to={to} 
      onClick={handleClick}
      className={className}
      style={style}
    >
      {children}
    </Link>
  );
};