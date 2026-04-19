import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { getToken, removeToken, setToken } from '../utils/storage';
import { ErrorResponse } from '../types/common.types';

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:8080',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add JWT token
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = getToken();
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<ErrorResponse>) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & {
      _retry?: boolean;
    };

    // Handle 401 Unauthorized - skip for login/register endpoints
    const isAuthEndpoint = originalRequest.url?.includes('/api/users/login') || originalRequest.url?.includes('/api/users/register');
    if (error.response?.status === 401 && !originalRequest._retry && !isAuthEndpoint) {
      originalRequest._retry = true;

      // Try to refresh token
      const refreshToken = localStorage.getItem('refresh_token');
      if (refreshToken) {
        try {
          const response = await axios.post(
            `${import.meta.env.VITE_API_URL}/auth/refresh`,
            { refresh_token: refreshToken }
          );
          const { access_token } = response.data;
          setToken(access_token);

          // Retry original request with new token
          if (originalRequest.headers) {
            originalRequest.headers.Authorization = `Bearer ${access_token}`;
          }
          return apiClient(originalRequest);
        } catch (refreshError) {
          // Refresh failed, logout user
          removeToken();
          window.location.href = '/login';
          return Promise.reject(refreshError);
        }
      } else {
        // No refresh token, redirect to login
        removeToken();
        window.location.href = '/login';
      }
    }

    // For auth endpoints, pass through the original axios error so callers can read response.data
    if (isAuthEndpoint) {
      return Promise.reject(error);
    }

    // Handle other errors
    const data = error.response?.data as any;
    const errorResponse: ErrorResponse = {
      message: data?.message || data?.title || error.message || 'An error occurred',
      status: error.response?.status || 500,
      timestamp: new Date().toISOString(),
      path: error.config?.url,
      errors: data?.errors,
    };

    return Promise.reject(errorResponse);
  }
);

export default apiClient;
