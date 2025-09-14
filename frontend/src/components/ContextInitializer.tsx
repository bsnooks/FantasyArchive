import React, { useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../store/hooks';
import { setSelectedFranchise, setSelectedSeason, clearSelectedFranchise, clearSelectedSeason } from '../store/contextSlice';
import { useFranchises } from '../hooks/useFantasyData';

const ContextInitializer: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const location = useLocation();
  const dispatch = useAppDispatch();
  const { selectedFranchiseId, selectedSeason } = useAppSelector((state) => state.context);

  // Fetch franchises to get franchise name by ID
  const { data: franchises, isLoading: franchisesLoading } = useFranchises();

  useEffect(() => {
    // Don't process if franchises are still loading
    if (franchisesLoading || !franchises) {
      console.log('ContextInitializer: Waiting for franchises data...', { 
        franchisesLoading, 
        hasData: !!franchises
      });
      return;
    }

    const path = location.pathname;
    console.log('ContextInitializer: Processing path:', path);
    console.log('ContextInitializer: Available franchises:', franchises.length);
    console.log('ContextInitializer: Current context:', { selectedFranchiseId, selectedSeason });

    // Parse URL to extract parameters
    let franchiseId: string | null = null;
    let year: number | null = null;

    // Handle franchise season detail page: /franchise/:franchiseId/season/:year
    const franchiseSeasonMatch = path.match(/^\/franchise\/([^/]+)\/season\/(\d+)$/);
    if (franchiseSeasonMatch) {
      franchiseId = franchiseSeasonMatch[1];
      year = parseInt(franchiseSeasonMatch[2]);
      console.log('ContextInitializer: Franchise season route detected', { franchiseId, year });
      
      // Set both franchise and season context
      const franchise = franchises.find(f => f.Id === franchiseId);
      if (franchise) {
        console.log('ContextInitializer: Setting franchise context for:', franchise.Name);
        dispatch(setSelectedFranchise({
          id: franchiseId,
          name: franchise.Name,
          preserveSeasonContext: true
        }));
      } else {
        console.log('ContextInitializer: Franchise not found for ID:', franchiseId);
      }
      
      if (selectedSeason !== year) {
        console.log('ContextInitializer: Setting season context for:', year);
        dispatch(setSelectedSeason(year));
      }
    }
    // Handle franchise detail page: /franchise/:franchiseId
    else {
      const franchiseMatch = path.match(/^\/franchise\/([^/]+)$/);
      if (franchiseMatch) {
        const franchiseId = franchiseMatch[1];
        console.log('ContextInitializer: Franchise detail route detected', { franchiseId });
        
        // Only set context if not already set to this franchise
        if (selectedFranchiseId !== franchiseId) {
          const franchise = franchises.find(f => f.Id === franchiseId);
          if (franchise) {
            console.log('ContextInitializer: Setting franchise context for:', franchise.Name);
            dispatch(setSelectedFranchise({
              id: franchiseId,
              name: franchise.Name,
              preserveSeasonContext: true
            }));
          } else {
            console.log('ContextInitializer: Franchise not found for ID:', franchiseId);
          }
        } else {
          console.log('ContextInitializer: Franchise context already set');
        }
      }
      // Handle season detail page: /season/:year
      else {
        const seasonMatch = path.match(/^\/season\/(\d+)$/);
        if (seasonMatch) {
          const year = parseInt(seasonMatch[1]);
          console.log('ContextInitializer: Season route detected', { year });
          
          // Only set context if not already set to this season
          if (selectedSeason !== year) {
            console.log('ContextInitializer: Setting season context for:', year);
            dispatch(setSelectedSeason(year));
          } else {
            console.log('ContextInitializer: Season context already set');
          }
        }
      }
    }
    
    // Handle pages that should clear context (only home page clears all context)
    if (path === '/') {
      // Clear all context for home page
      if (selectedFranchiseId || selectedSeason) {
        console.log('ContextInitializer: Clearing all context for home route');
        dispatch(clearSelectedFranchise());
        dispatch(clearSelectedSeason());
      }
    }
    // For /franchises and /seasons routes, let the breadcrumb navigation handle context clearing
    // The ContextInitializer should only SET context from URLs, not clear it
    
  }, [location.pathname, selectedFranchiseId, selectedSeason, franchises, franchisesLoading, dispatch]);

  return <>{children}</>;
};

export default ContextInitializer;