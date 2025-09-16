import React, { useEffect, useRef, useState, useCallback } from 'react';
import { Card, Spin, Alert, Button, Space, Tooltip } from 'antd';
import { ZoomInOutlined, ZoomOutOutlined, ReloadOutlined, ExpandOutlined } from '@ant-design/icons';

interface MermaidRendererProps {
  diagram: string;
  title?: string;
  height?: string | number;
  usePageScroll?: boolean;
  enablePanZoom?: boolean;
}

const MermaidRenderer: React.FC<MermaidRendererProps> = ({
  diagram,
  title = "Trade Tree Diagram",
  height = "600px",
  usePageScroll = false,
  enablePanZoom = false
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const svgRef = useRef<SVGSVGElement | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [zoom, setZoom] = useState(1);
  const [pan, setPan] = useState({ x: 0, y: 0 });
  const [isDragging, setIsDragging] = useState(false);
  const [dragStart, setDragStart] = useState({ x: 0, y: 0 });
  const [lastTouchDistance, setLastTouchDistance] = useState(0);

  // Helper function to calculate distance between two touches
  const getTouchDistance = useCallback((touches: React.TouchList) => {
    if (touches.length < 2) return 0;
    const touch1 = touches[0];
    const touch2 = touches[1];
    return Math.sqrt(
      Math.pow(touch2.clientX - touch1.clientX, 2) + 
      Math.pow(touch2.clientY - touch1.clientY, 2)
    );
  }, []);

  // Update SVG transform for pan/zoom with performance optimization
  const updateSVGTransform = useCallback(() => {
    if (svgRef.current && enablePanZoom) {
      const g = svgRef.current.querySelector('g');
      if (g) {
        // Use requestAnimationFrame for smoother updates on mobile
        requestAnimationFrame(() => {
          g.style.transform = `translate(${pan.x}px, ${pan.y}px) scale(${zoom})`;
          g.style.transformOrigin = '0 0';
        });
      }
    }
  }, [pan, zoom, enablePanZoom]);

  // Mouse and touch event handlers for pan/zoom
  const handleMouseDown = useCallback((e: React.MouseEvent) => {
    if (!enablePanZoom) return;
    setIsDragging(true);
    // Store the starting mouse position relative to current pan, accounting for zoom sensitivity
    const sensitivity = Math.max(1, zoom * 0.5);
    setDragStart({ x: e.clientX - pan.x / sensitivity, y: e.clientY - pan.y / sensitivity });
    if (svgRef.current) {
      svgRef.current.style.cursor = 'grabbing';
    }
  }, [enablePanZoom, pan, zoom]);

  const handleTouchStart = useCallback((e: React.TouchEvent) => {
    if (!enablePanZoom) return;
    
    // Prevent default to avoid page zoom/scroll
    e.preventDefault();
    
    if (e.touches.length === 1) {
      // Single touch - start panning with zoom-sensitive positioning
      setIsDragging(true);
      const touch = e.touches[0];
      const sensitivity = Math.max(1, zoom * 0.5);
      setDragStart({ x: touch.clientX - pan.x / sensitivity, y: touch.clientY - pan.y / sensitivity });
      
      // Add visual feedback for touch start
      if (svgRef.current) {
        svgRef.current.style.cursor = 'grabbing';
      }
    } else if (e.touches.length === 2) {
      // Two touches - start pinch zoom
      setIsDragging(false);
      const distance = getTouchDistance(e.touches);
      setLastTouchDistance(distance);
      
      // Reset cursor for pinch zoom
      if (svgRef.current) {
        svgRef.current.style.cursor = 'grab';
      }
    }
  }, [enablePanZoom, pan, zoom, getTouchDistance]);

  const handleMouseMove = useCallback((e: React.MouseEvent) => {
    if (!enablePanZoom || !isDragging) return;
    // Apply zoom-based sensitivity - more movement at higher zoom levels
    const sensitivity = Math.max(1, zoom * 0.5);
    const deltaX = (e.clientX - dragStart.x) * sensitivity;
    const deltaY = (e.clientY - dragStart.y) * sensitivity;
    const newPan = {
      x: deltaX,
      y: deltaY
    };
    setPan(newPan);
  }, [enablePanZoom, isDragging, dragStart, zoom]);

  const handleTouchMove = useCallback((e: React.TouchEvent) => {
    if (!enablePanZoom) return;
    
    // Always prevent default to stop page zoom/scroll
    e.preventDefault();
    
    if (e.touches.length === 1 && isDragging) {
      // Single touch - panning with zoom-based sensitivity for better navigation at high zoom
      const touch = e.touches[0];
      const sensitivity = Math.max(1, zoom * 0.5); // Increase sensitivity at higher zoom levels
      const deltaX = (touch.clientX - dragStart.x) * sensitivity;
      const deltaY = (touch.clientY - dragStart.y) * sensitivity;
      const newPan = {
        x: deltaX,
        y: deltaY
      };
      setPan(newPan);
    } else if (e.touches.length === 2) {
      // Two touches - pinch zoom with higher max zoom for mobile
      const distance = getTouchDistance(e.touches);
      if (lastTouchDistance > 0) {
        const scale = distance / lastTouchDistance;
        const newZoom = Math.max(0.1, Math.min(10, zoom * scale)); // Increased max zoom to 1000%
        setZoom(newZoom);
      }
      setLastTouchDistance(distance);
    }
  }, [enablePanZoom, isDragging, dragStart, getTouchDistance, lastTouchDistance, zoom]);

  const handleMouseUp = useCallback(() => {
    if (!enablePanZoom) return;
    setIsDragging(false);
    if (svgRef.current) {
      svgRef.current.style.cursor = 'grab';
    }
  }, [enablePanZoom]);

  const handleTouchEnd = useCallback((e: React.TouchEvent) => {
    if (!enablePanZoom) return;
    
    // Prevent default to avoid page interactions
    e.preventDefault();
    
    if (e.touches.length === 0) {
      // All touches ended - restore grab cursor
      setIsDragging(false);
      setLastTouchDistance(0);
      if (svgRef.current) {
        svgRef.current.style.cursor = 'grab';
      }
    } else if (e.touches.length === 1) {
      // One touch remaining - switch back to pan mode with zoom-sensitive positioning
      setIsDragging(true);
      const touch = e.touches[0];
      const sensitivity = Math.max(1, zoom * 0.5);
      setDragStart({ x: touch.clientX - pan.x / sensitivity, y: touch.clientY - pan.y / sensitivity });
      setLastTouchDistance(0);
      if (svgRef.current) {
        svgRef.current.style.cursor = 'grabbing';
      }
    }
  }, [enablePanZoom, pan, zoom]);

  const handleWheel = useCallback((e: React.WheelEvent) => {
    if (!enablePanZoom) return;
    e.preventDefault();
    const delta = e.deltaY > 0 ? 0.9 : 1.1;
    const newZoom = Math.max(0.1, Math.min(10, zoom * delta)); // Increased max zoom to 1000%
    setZoom(newZoom);
  }, [enablePanZoom, zoom]);

  // Control functions
  const handleZoomIn = useCallback(() => {
    setZoom(prev => Math.min(10, prev * 1.2)); // Increased max zoom to 1000%
  }, []);

  const handleZoomOut = useCallback(() => {
    setZoom(prev => Math.max(0.1, prev / 1.2));
  }, []);

  const handleReset = useCallback(() => {
    setZoom(1);
    setPan({ x: 0, y: 0 });
  }, []);

  const handleFitToScreen = useCallback(() => {
    if (svgRef.current && containerRef.current) {
      const svgRect = svgRef.current.getBoundingClientRect();
      const containerRect = containerRef.current.getBoundingClientRect();
      
      const scaleX = containerRect.width / svgRect.width;
      const scaleY = containerRect.height / svgRect.height;
      const scale = Math.min(scaleX, scaleY) * 0.9; // 90% to add some padding
      
      setZoom(scale);
      setPan({ x: 0, y: 0 });
    }
  }, []);

  // Update transform when pan/zoom changes
  useEffect(() => {
    updateSVGTransform();
  }, [updateSVGTransform]);

  // Only re-render diagram when diagram content or basic props change
  useEffect(() => {
    const renderDiagramInternal = async () => {
      if (!diagram || !containerRef.current) {
        return;
      }

      try {
        setLoading(true);
        setError(null);
        
        // Reset pan/zoom state for new diagrams
        setZoom(1);
        setPan({ x: 0, y: 0 });
        
        // Clear container
        containerRef.current.innerHTML = '';
        
        // Import mermaid dynamically to avoid initialization issues
        const mermaid = await import('mermaid');
        
        // Initialize with safe settings
        mermaid.default.initialize({
          startOnLoad: false,
          theme: 'default',
          securityLevel: 'loose',
          fontFamily: 'Arial, sans-serif',
          flowchart: {
            useMaxWidth: !enablePanZoom && usePageScroll,
            htmlLabels: false,
            curve: 'linear',
            diagramPadding: usePageScroll ? 10 : 20,
            nodeSpacing: 50,
            rankSpacing: 60
          },
          layout: 'dagre'
        });

        // Create a unique ID
        const id = `diagram-${Date.now()}`;
        
        // Try to render using the parse + render approach
        const isValid = await mermaid.default.parse(diagram);
        if (!isValid) {
          throw new Error('Invalid Mermaid syntax');
        }

        const { svg } = await mermaid.default.render(id, diagram);
        
        // Insert the SVG
        containerRef.current.innerHTML = svg;
        
        // Get the SVG element and set up pan/zoom if enabled
        const svgElement = containerRef.current.querySelector('svg');
        if (svgElement) {
          svgRef.current = svgElement;
          
          if (enablePanZoom) {
            // Set up SVG for pan/zoom
            svgElement.style.cursor = 'grab';
            svgElement.style.width = '100%';
            svgElement.style.height = '100%';
            svgElement.style.display = 'block';
            
            // Apply current transform state (use current values, not state dependencies)
            const g = svgElement.querySelector('g');
            if (g) {
              g.style.transform = `translate(0px, 0px) scale(1)`;
              g.style.transformOrigin = '0 0';
            }
          } else if (usePageScroll) {
            // For page scroll mode, ensure SVG is properly sized for mobile
            svgElement.style.maxWidth = '100%';
            svgElement.style.height = 'auto';
            svgElement.style.display = 'block';
            svgElement.style.margin = '0 auto';
          }
        }
        
        console.log('MermaidRenderer: Render successful');
        setLoading(false);
        
      } catch (err) {
        console.error('MermaidRenderer: Render failed:', err);
        setError(err instanceof Error ? err.message : 'Failed to render diagram');
        setLoading(false);
      }
    };

    if (diagram) {
      renderDiagramInternal();
    }
  }, [diagram, usePageScroll, enablePanZoom]); // Only depend on diagram content and basic props

  return (
    <Card 
      title={title} 
      style={usePageScroll ? { minHeight: height } : { height }}
      bodyStyle={usePageScroll ? { padding: '24px' } : undefined}
      className={enablePanZoom ? 'interactive-diagram' : undefined}
      extra={enablePanZoom && (
        <Space>
          <Tooltip title="Zoom In">
            <Button 
              size="small" 
              icon={<ZoomInOutlined />} 
              onClick={handleZoomIn}
            />
          </Tooltip>
          <Tooltip title="Zoom Out">
            <Button 
              size="small" 
              icon={<ZoomOutOutlined />} 
              onClick={handleZoomOut}
            />
          </Tooltip>
          <Tooltip title="Fit to Screen">
            <Button 
              size="small" 
              icon={<ExpandOutlined />} 
              onClick={handleFitToScreen}
            />
          </Tooltip>
          <Tooltip title="Reset View">
            <Button 
              size="small" 
              icon={<ReloadOutlined />} 
              onClick={handleReset}
            />
          </Tooltip>
          <span style={{ 
            fontSize: '12px', 
            color: '#666', 
            marginLeft: '8px',
            minWidth: '50px',
            textAlign: 'center'
          }}>
            {Math.round(zoom * 100)}%
          </span>
        </Space>
      )}
    >
      <div 
        style={
          usePageScroll 
            ? { 
                position: 'relative', 
                minHeight: '400px',
                overflowX: 'auto',
                overflowY: 'visible',
                WebkitOverflowScrolling: 'touch'
              }
            : { 
                height: 'calc(100% - 60px)', 
                overflow: enablePanZoom ? 'hidden' : 'auto', 
                position: 'relative',
                touchAction: enablePanZoom ? 'none' : 'auto',
                WebkitUserSelect: enablePanZoom ? 'none' : 'auto',
                userSelect: enablePanZoom ? 'none' : 'auto',
                // Additional mobile optimizations
                WebkitTapHighlightColor: enablePanZoom ? 'transparent' : 'inherit',
                WebkitTouchCallout: enablePanZoom ? 'none' : 'default',
                transform: enablePanZoom ? 'translateZ(0)' : 'none' // Hardware acceleration
              }
        }
        onMouseDown={handleMouseDown}
        onMouseMove={handleMouseMove}
        onMouseUp={handleMouseUp}
        onMouseLeave={handleMouseUp}
        onTouchStart={handleTouchStart}
        onTouchMove={handleTouchMove}
        onTouchEnd={handleTouchEnd}
        onWheel={handleWheel}
      >

        {loading && (
          <div style={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            textAlign: 'center',
            zIndex: 10
          }}>
            <Spin size="large" />
            <div style={{ marginTop: '16px' }}>Rendering diagram...</div>
          </div>
        )}

        {error && (
          <div style={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            width: '90%',
            maxWidth: '500px',
            zIndex: 10
          }}>
            <Alert
              message="Diagram Rendering Error"
              description={error}
              type="error"
              showIcon
            />
          </div>
        )}

        <div 
          ref={containerRef}
          style={{
            width: '100%',
            minHeight: usePageScroll ? '400px' : '300px',
            height: enablePanZoom ? '100%' : 'auto',
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'flex-start',
            paddingTop: '20px',
            overflowX: usePageScroll && !enablePanZoom ? 'auto' : 'visible',
            overflowY: enablePanZoom ? 'hidden' : 'visible',
            WebkitOverflowScrolling: usePageScroll ? 'touch' : 'auto',
            userSelect: enablePanZoom ? 'none' : 'auto',
            touchAction: enablePanZoom ? 'none' : 'auto',
            WebkitTouchCallout: enablePanZoom ? 'none' : 'default',
            // Performance optimizations for mobile
            transform: enablePanZoom ? 'translateZ(0)' : 'none', // Enable hardware acceleration
            backfaceVisibility: enablePanZoom ? 'hidden' : 'visible',
            perspective: enablePanZoom ? '1000px' : 'none',
            willChange: enablePanZoom ? 'transform' : 'auto'
          }}
        />
      </div>
    </Card>
  );
};

export default MermaidRenderer;