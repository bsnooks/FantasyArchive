import React from 'react';
import './LoadingSkeleton.css';

interface LoadingSkeletonProps {
  height?: string;
  width?: string;
  borderRadius?: string;
  className?: string;
}

const LoadingSkeleton: React.FC<LoadingSkeletonProps> = ({ 
  height = "20px", 
  width = "100%", 
  borderRadius = "4px",
  className = ""
}) => {
  return (
    <div 
      className={`loading-skeleton ${className}`}
      style={{ 
        height, 
        width, 
        borderRadius 
      }}
    />
  );
};

export default LoadingSkeleton;