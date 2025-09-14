import { useQuery } from '@tanstack/react-query';
import { Franchise, Season } from '../types/fantasy';

// API functions
const fetchFranchises = async (): Promise<Franchise[]> => {
  const response = await fetch('/data/franchises/index.json');
  if (!response.ok) {
    throw new Error('Failed to fetch franchises');
  }
  return response.json();
};

const fetchSeasons = async (): Promise<Season[]> => {
  const response = await fetch('/data/seasons/index.json');
  if (!response.ok) {
    throw new Error('Failed to fetch seasons');
  }
  return response.json();
};

// React Query hooks
export const useFranchises = () => {
  return useQuery({
    queryKey: ['franchises'],
    queryFn: fetchFranchises,
    staleTime: 1000 * 60 * 5, // 5 minutes
    retry: 3,
  });
};

export const useSeasons = () => {
  return useQuery({
    queryKey: ['seasons'],
    queryFn: fetchSeasons,
    staleTime: 1000 * 60 * 5, // 5 minutes
    retry: 3,
  });
};