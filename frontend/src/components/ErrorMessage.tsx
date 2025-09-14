import React from 'react';
import './ErrorMessage.css';

interface ErrorMessageProps {
  error: Error | null;
  onRetry?: () => void;
  message?: string;
}

const ErrorMessage: React.FC<ErrorMessageProps> = ({ 
  error, 
  onRetry, 
  message 
}) => {
  const errorMessage = message || error?.message || 'An unexpected error occurred';

  return (
    <div className="error-message">
      <div className="error-icon">⚠️</div>
      <h3>Oops! Something went wrong</h3>
      <p className="error-text">{errorMessage}</p>
      {onRetry && (
        <button onClick={onRetry} className="retry-button">
          Try Again
        </button>
      )}
    </div>
  );
};

export default ErrorMessage;