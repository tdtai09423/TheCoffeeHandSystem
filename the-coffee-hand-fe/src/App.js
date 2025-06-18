// src/App.js
import React, { Suspense, useEffect } from "react";
import { useRoutes } from "react-router-dom";
import ThemeRoutes from "./routes/Router";
import ErrorBoundary from "./components/ErrorBoundary";

const LoadingSpinner = () => (
  <div className="d-flex justify-content-center align-items-center" style={{ height: '100vh' }}>
    <div className="spinner-border text-primary" role="status">
      <span className="visually-hidden">Loading...</span>
    </div>
  </div>
);

const AppRoutes = () => {
  const routing = useRoutes(ThemeRoutes);
  return routing;
};

const App = () => {
  useEffect(() => {
    // Preload components when app starts
    const preloadComponents = async () => {
      const components = [
        import("./views/Starter"),
        import("./views/ui/Orders"),
        import("./views/ui/Payments"),
        import("./views/ui/Drinks"),
        import("./views/ui/Ingredients"),
        import("./views/ui/Recipes"),
        import("./views/ui/Users"),
        import("./views/ui/Reports"),
        import("./views/ui/Settings"),
        import("./views/ui/login")
      ];

      await Promise.all(components.map(component => component.catch(() => {})));
    };

    preloadComponents();
  }, []);

  return (
    <ErrorBoundary>
      <Suspense fallback={<LoadingSpinner />}>
        <AppRoutes />
      </Suspense>
    </ErrorBoundary>
  );
};

export default App;
