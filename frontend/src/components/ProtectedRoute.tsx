import React from 'react';
import { Navigate } from 'react-router-dom';
import { Role } from '../types/auth.types';
import { isAuthenticated } from '../utils/storage';

interface ProtectedRouteProps {
  children: React.ReactNode;
  roles: Role[];
  userRoles: Role[];
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ children, roles, userRoles }) => {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }

  const hasRequiredRole = roles.some((role) => userRoles.includes(role));

  if (!hasRequiredRole) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
};

export default ProtectedRoute;
