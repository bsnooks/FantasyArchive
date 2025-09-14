import React from 'react';
import './SkeletonCard.css';

interface SkeletonCardProps {
  count?: number;
  height?: string;
}

const SkeletonCard: React.FC<SkeletonCardProps> = ({ count = 1, height = '200px' }) => {
  return (
    <>
      {Array.from({ length: count }).map((_, index) => (
        <div key={index} className="skeleton-card" style={{ height }}>
          <div className="skeleton-header">
            <div className="skeleton-title"></div>
            <div className="skeleton-badge"></div>
          </div>
          <div className="skeleton-content">
            <div className="skeleton-line short"></div>
            <div className="skeleton-line medium"></div>
            <div className="skeleton-line long"></div>
            <div className="skeleton-line medium"></div>
          </div>
        </div>
      ))}
    </>
  );
};

export default SkeletonCard;