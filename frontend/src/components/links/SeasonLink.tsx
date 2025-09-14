import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAppDispatch } from '../../store/hooks';
import { setSelectedSeason, clearSelectedFranchise } from '../../store/contextSlice';

interface SeasonLinkProps {
  season: number;
  to: string;
  children: React.ReactNode;
  className?: string;
  style?: React.CSSProperties;
  clearFranchiseContext?: boolean; // When true, clears franchise context for global season view
}

export const SeasonLink: React.FC<SeasonLinkProps> = ({
  season,
  to,
  children,
  className,
  style,
  clearFranchiseContext = true
}) => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const handleClick = (e: React.MouseEvent) => {
    e.preventDefault();
    dispatch(setSelectedSeason(season));
    
    // Clear franchise context for global season view unless explicitly preserving it
    if (clearFranchiseContext) {
      dispatch(clearSelectedFranchise());
    }
    
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