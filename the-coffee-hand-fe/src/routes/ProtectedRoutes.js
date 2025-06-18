import React from 'react';
import { Routes, Route } from 'react-router-dom';
import ProtectedRoute from '../components/ProtectedRoute';

// Import các components cần bảo vệ
import Starter from '../views/Starter';
import Orders from '../views/ui/Orders';
import Users from '../views/ui/Users';
import Drinks from '../views/ui/Drinks';
import Ingredients from '../views/ui/Ingredients';
import Reports from '../views/ui/Reports';
import Settings from '../views/ui/Settings';

const ProtectedRoutes = () => {
  return (
    <Routes>
      {/* <Route
        path="/starter"
        element={
          <ProtectedRoute>
            <Starter />
          </ProtectedRoute>
        }
      /> */}
      <Route
        path="/orders"
        element={
          <ProtectedRoute>
            <Orders />
          </ProtectedRoute>
        }
      />
      <Route
        path="/users"
        element={
          <ProtectedRoute>
            <Users />
          </ProtectedRoute>
        }
      />
      <Route
        path="/drinks"
        element={
          <ProtectedRoute>
            <Drinks />
          </ProtectedRoute>
        }
      />
      <Route
        path="/ingredients"
        element={
          <ProtectedRoute>
            <Ingredients />
          </ProtectedRoute>
        }
      />
      <Route
        path="/reports"
        element={
          <ProtectedRoute>
            <Reports />
          </ProtectedRoute>
        }
      />
      <Route
        path="/settings"
        element={
          <ProtectedRoute>
            <Settings />
          </ProtectedRoute>
        }
      />
    </Routes>
  );
};

export default ProtectedRoutes; 