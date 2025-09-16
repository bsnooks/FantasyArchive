import React, { useEffect } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ConfigProvider, theme } from "antd";
import { Provider } from "react-redux";
import { store } from "./store";
import Navigation from "./components/Navigation";
import ContextBreadcrumb from "./components/ContextBreadcrumb";
import ContextInitializer from "./components/ContextInitializer";
import ScrollToTop from "./components/ScrollToTop";
import Landing from "./pages/Landing";
import FranchiseDetail from "./pages/FranchiseDetail";
import FranchiseSeasonDetail from "./pages/FranchiseSeasonDetail";
import SeasonDetail from "./pages/SeasonDetail";
import Seasons from "./pages/Seasons";
import Franchises from "./pages/Franchises";
import Drafts from "./pages/Drafts";
import TradeTrees from "./pages/TradeTrees";
import TradeDetail from "./pages/TradeDetail";
import Records from "./pages/Records";
import PlayerProfile from "./pages/PlayerProfile";
import "./App.css";

// Create a client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 minutes
      retry: 3,
    },
  },
});

const App: React.FC = () => {
  // Suppress ResizeObserver errors globally - comprehensive approach for dev
  useEffect(() => {
    // Only in development
    if (process.env.NODE_ENV === 'development') {
      // Suppress console errors
      const originalError = console.error;
      const originalWarn = console.warn;
      
      console.error = (...args) => {
        if (
          typeof args[0] === 'string' && 
          args[0].includes('ResizeObserver')
        ) {
          return;
        }
        originalError(...args);
      };

      console.warn = (...args) => {
        if (typeof args[0] === 'string' && args[0].includes('ResizeObserver')) {
          return;
        }
        originalWarn(...args);
      };

      // Suppress window errors
      const handleError = (event: ErrorEvent) => {
        if (event.message && (
          event.message.includes('ResizeObserver') ||
          event.message.includes('Non-Error promise rejection captured')
        )) {
          event.preventDefault();
          event.stopPropagation();
          return false;
        }
      };

      // Suppress unhandled promise rejections
      const handleUnhandledRejection = (event: PromiseRejectionEvent) => {
        if (event.reason && typeof event.reason === 'string' && 
            event.reason.includes('ResizeObserver')) {
          event.preventDefault();
          return false;
        }
      };

      window.addEventListener('error', handleError);
      window.addEventListener('unhandledrejection', handleUnhandledRejection);

      // Try to disable React error overlay
      if (process.env.REACT_APP_DISABLE_OVERLAY === 'true') {
        try {
          // Hide error overlay if it exists
          const errorOverlay = document.querySelector('iframe[data-react-error-overlay]');
          if (errorOverlay) {
            (errorOverlay as HTMLElement).style.display = 'none';
          }
        } catch (e) {
          // Ignore errors when trying to hide overlay
        }
      }

      return () => {
        console.error = originalError;
        console.warn = originalWarn;
        window.removeEventListener('error', handleError);
        window.removeEventListener('unhandledrejection', handleUnhandledRejection);
      };
    }
  }, []);

  return (
    <Provider store={store}>
      <ConfigProvider
        theme={{
          algorithm: theme.defaultAlgorithm,
          token: {
            colorPrimary: "#1a2b4c",
            colorInfo: "#1a2b4c",
            colorSuccess: "#52c41a",
            colorWarning: "#faad14",
            colorError: "#ff4d4f",
            borderRadius: 8,
            fontFamily:
              '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
          },
          components: {
            Table: {
              headerBg: "#f8f9fa",
              headerColor: "#495057",
              rowHoverBg: "#f8f9fa",
            },
          },
        }}
      >
        <QueryClientProvider client={queryClient}>
          <Router basename={process.env.NODE_ENV === 'production' ? '' : process.env.PUBLIC_URL}>
            <ScrollToTop />
            <ContextInitializer>
              <div className="app">
                <Navigation />
                <ContextBreadcrumb />
                <main className="main-content">
                  <Routes>
                    <Route path="/" element={<Landing />} />
                    <Route path="/seasons" element={<Seasons />} />
                    <Route path="/franchises" element={<Franchises />} />
                    <Route
                      path="/franchise/:franchiseId"
                      element={<FranchiseDetail />}
                    />
                    <Route
                      path="/franchise/:franchiseId/season/:year"
                      element={<FranchiseSeasonDetail />}
                    />
                    <Route path="/season/:year" element={<SeasonDetail />} />
                    <Route path="/drafts" element={<Drafts />} />
                    <Route path="/trade-trees" element={<TradeTrees />} />
                    <Route path="/trade/:tradeId" element={<TradeDetail />} />
                    <Route path="/records" element={<Records />} />
                    <Route
                      path="/player/:playerId"
                      element={<PlayerProfile />}
                    />
                  </Routes>
                </main>
              </div>
            </ContextInitializer>
          </Router>
        </QueryClientProvider>
      </ConfigProvider>
    </Provider>
  );
};

export default App;
