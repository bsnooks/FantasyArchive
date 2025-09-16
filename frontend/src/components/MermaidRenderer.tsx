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

  // Update SVG transform for pan/zoom
  const updateSVGTransform = useCallback(() => {
    if (svgRef.current && enablePanZoom) {
      const g = svgRef.current.querySelector('g');
      if (g) {
        g.style.transform = `translate(${pan.x}px, ${pan.y}px) scale(${zoom})`;
        g.style.transformOrigin = '0 0';
      }
    }
  }, [pan, zoom, enablePanZoom]);

  const renderDiagram = useCallback(async () => {
    if (!diagram || !containerRef.current) {
      return;
    }

    try {
      setLoading(true);
      setError(null);
      
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
          
          // Apply initial transform
          updateSVGTransform();
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
  }, [diagram, usePageScroll, enablePanZoom, updateSVGTransform]);

  // Mouse and touch event handlers for pan/zoom
  const handleMouseDown = useCallback((e: React.MouseEvent) => {
    if (!enablePanZoom) return;
    setIsDragging(true);
    setDragStart({ x: e.clientX - pan.x, y: e.clientY - pan.y });
    if (svgRef.current) {
      svgRef.current.style.cursor = 'grabbing';
    }
  }, [enablePanZoom, pan]);

  const handleTouchStart = useCallback((e: React.TouchEvent) => {
    if (!enablePanZoom || e.touches.length !== 1) return;
    setIsDragging(true);
    const touch = e.touches[0];
    setDragStart({ x: touch.clientX - pan.x, y: touch.clientY - pan.y });
  }, [enablePanZoom, pan]);

  const handleMouseMove = useCallback((e: React.MouseEvent) => {
    if (!enablePanZoom || !isDragging) return;
    const newPan = {
      x: e.clientX - dragStart.x,
      y: e.clientY - dragStart.y
    };
    setPan(newPan);
  }, [enablePanZoom, isDragging, dragStart]);

  const handleTouchMove = useCallback((e: React.TouchEvent) => {
    if (!enablePanZoom || !isDragging || e.touches.length !== 1) return;
    e.preventDefault();
    const touch = e.touches[0];
    const newPan = {
      x: touch.clientX - dragStart.x,
      y: touch.clientY - dragStart.y
    };
    setPan(newPan);
  }, [enablePanZoom, isDragging, dragStart]);

  const handleMouseUp = useCallback(() => {
    if (!enablePanZoom) return;
    setIsDragging(false);
    if (svgRef.current) {
      svgRef.current.style.cursor = 'grab';
    }
  }, [enablePanZoom]);

  const handleTouchEnd = useCallback(() => {
    if (!enablePanZoom) return;
    setIsDragging(false);
  }, [enablePanZoom]);

  const handleWheel = useCallback((e: React.WheelEvent) => {
    if (!enablePanZoom) return;
    e.preventDefault();
    const delta = e.deltaY > 0 ? 0.9 : 1.1;
    const newZoom = Math.max(0.1, Math.min(3, zoom * delta));
    setZoom(newZoom);
  }, [enablePanZoom, zoom]);

  // Control functions
  const handleZoomIn = useCallback(() => {
    setZoom(prev => Math.min(3, prev * 1.2));
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

  useEffect(() => {
    if (diagram) {
      renderDiagram();
    }
  }, [diagram, renderDiagram]);

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
                position: 'relative' 
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
            userSelect: enablePanZoom ? 'none' : 'auto'
          }}
        />
      </div>
    </Card>
  );
};

export default MermaidRenderer;