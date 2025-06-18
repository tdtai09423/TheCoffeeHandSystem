import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import authAPI from '../api/authApi';

const ProtectedRoute = ({ children }) => {
  const location = useLocation();
  
  if (!authAPI.isAuthenticated()) {
    // Redirect to login page but save the attempted url
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return children;
};

export default ProtectedRoute; 