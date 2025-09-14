import { useState, useEffect } from 'react';

export interface LeagueRecord {
  Rank: number;
  FranchiseId?: string;
  FranchiseName?: string;
  OtherFranchiseId?: string;
  OtherFranchiseName?: string;
  PlayerId?: number;
  PlayerName?: string;
  PlayerPosition?: string;
  RecordValue: string;
  RecordNumericValue: number;
  Year?: number;
  Week?: number;
}

export interface LeagueRecords {
  RecordTitle: string;
  PositiveRecord: boolean;
  RecordType: string;
  Records: LeagueRecord[];
}

export const useRecords = (franchiseId?: string, season?: number) => {
  const [data, setData] = useState<LeagueRecords[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchRecords = async () => {
      setIsLoading(true);
      setError(null);

      try {
        let path: string;
        
        if (season) {
          path = `/data/records/seasons/${season}.json`;
        } else if (franchiseId) {
          path = `/data/records/franchises/${franchiseId}.json`;
        } else {
          path = '/data/records/all-time/index.json';
        }

        console.log('Fetching records from:', path);
        const response = await fetch(path);
        
        if (!response.ok) {
          throw new Error(`Failed to fetch records: ${response.statusText}`);
        }

        const recordsData = await response.json();
        console.log('Records data loaded:', recordsData);
        setData(recordsData || []);
      } catch (err) {
        console.error('Error fetching records:', err);
        setError(err instanceof Error ? err.message : 'Unknown error occurred');
        setData([]);
      } finally {
        setIsLoading(false);
      }
    };

    fetchRecords();
  }, [franchiseId, season]);

  return { data, isLoading, error };
};