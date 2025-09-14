import React from "react";
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
import Trades from "./pages/Trades";
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
          <Router basename={process.env.PUBLIC_URL}>
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
                    <Route path="/trades" element={<Trades />} />
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
