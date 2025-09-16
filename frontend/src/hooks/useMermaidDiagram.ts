import { useState, useEffect } from 'react';

interface UseMermaidDiagramResult {
  diagram: string | null;
  loading: boolean;
  error: string | null;
  source: 'pregenerated' | null;
}

/**
 * Hook to load pre-generated Mermaid diagrams
 */
export const useMermaidDiagram = (tradeId: string): UseMermaidDiagramResult => {
  const [diagram, setDiagram] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [source, setSource] = useState<'pregenerated' | null>(null);

  useEffect(() => {
    if (!tradeId) {
      setDiagram(null);
      setLoading(false);
      setError(null);
      setSource(null);
      return;
    }

    const loadDiagram = async () => {
      setLoading(true);
      setError(null);

      try {
        // Try to load the pre-generated Mermaid diagram
        const mermaidResponse = await fetch(`/data/trade-trees/mermaid/${tradeId}.mmd`);
        
        if (mermaidResponse.ok) {
          const mermaidText = await mermaidResponse.text();
          if (mermaidText.trim()) {
            console.log(`Loaded pre-generated Mermaid diagram for trade ${tradeId}`);
            setDiagram(mermaidText);
            setSource('pregenerated');
            setLoading(false);
            return;
          }
        }

        // No fallback needed - if no pre-generated diagram exists, show error
        console.error(`Pre-generated diagram not found for trade ${tradeId}`);
        setError('Diagram not available');
        
      } catch (err) {
        console.error(`Error loading diagram for trade ${tradeId}:`, err);
        setError('Failed to load trade diagram');
      } finally {
        setLoading(false);
      }
    };

    loadDiagram();
  }, [tradeId]);

  return { diagram, loading, error, source };
};