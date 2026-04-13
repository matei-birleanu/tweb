import apiClient from './apiClient';
import { User, LoginResponse, RegisterRequest } from '../types/auth.types';
import { setToken, removeToken, getToken } from '../utils/storage';

export const authService = {
  login: async (username: string, password: string): Promise<LoginResponse> => {
    const response = await apiClient.post<{ token: string }>('/api/users/login', {
      username,
      password,
    });
    const loginResponse: LoginResponse = {
      access_token: response.data.token,
      refresh_token: '',
      expires_in: 3600,
      refresh_expires_in: 0,
      token_type: 'Bearer',
    };
    setToken(loginResponse.access_token);
    return loginResponse;
  },

  logout: async (): Promise<void> => {
    try {
      await apiClient.post('/auth/logout');
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      removeToken();
      localStorage.removeItem('refresh_token');
    }
  },

  register: async (data: RegisterRequest): Promise<User> => {
    const response = await apiClient.post<User>('/api/users/register', data);
    return response.data;
  },

  getCurrentUser: async (): Promise<User> => {
    // Decode user info from JWT token stored locally
    const token = getToken();
    if (!token) throw new Error('No token');
    const payload = JSON.parse(atob(token.split('.')[1]));
    return {
      id: payload.sub || payload.nameid || '',
      username: payload.unique_name || payload.email || '',
      email: payload.email || '',
      firstName: payload.given_name || '',
      lastName: payload.family_name || '',
      roles: payload.role ? (Array.isArray(payload.role) ? payload.role : [payload.role]) : [],
      enabled: true,
      emailVerified: true,
    };
  },
};
