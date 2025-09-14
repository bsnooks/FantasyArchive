import React from 'react';
import './FranchiseLogo.css';

interface FranchiseLogoProps {
  franchiseId: string;
  franchiseName: string;
  size?: 'small' | 'medium' | 'large' | 'xlarge';
  showFallback?: boolean;
  className?: string;
}

const FranchiseLogo: React.FC<FranchiseLogoProps> = ({
  franchiseId,
  franchiseName,
  size = 'medium',
  showFallback = true,
  className = ''
}) => {
  const logoPath = `/franchise/${franchiseId}.png`;

  const handleImageError = (e: React.SyntheticEvent<HTMLImageElement>) => {
    if (showFallback) {
      // Hide the image and show the fallback
      e.currentTarget.style.display = 'none';
      const fallback = e.currentTarget.nextElementSibling as HTMLElement;
      if (fallback) {
        fallback.style.display = 'flex';
      }
    }
  };

  return (
    <div className={`franchise-logo ${size} ${className}`}>
      <img
        src={logoPath}
        alt={`${franchiseName} logo`}
        className="logo-image"
        onError={handleImageError}
      />
      {showFallback && (
        <div className="logo-fallback" style={{ display: 'none' }}>
          <span className="logo-initials">
            {franchiseName
              .split(' ')
              .map(word => word.charAt(0))
              .join('')
              .substring(0, 2)
              .toUpperCase()}
          </span>
        </div>
      )}
    </div>
  );
};

export default FranchiseLogo;