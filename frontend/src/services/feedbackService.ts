import apiClient from './apiClient';
import { Feedback, FeedbackCreate, FeedbackCategory } from '../types/feedback.types';

export const feedbackService = {
  submitFeedback: async (data: FeedbackCreate): Promise<Feedback> => {
    const response = await apiClient.post<Feedback>('/api/feedback', data);
    return response.data;
  },

  getFeedback: async (
    page = 0,
    size = 10,
    _category?: FeedbackCategory
  ): Promise<{ content: Feedback[]; totalElements: number; totalPages: number }> => {
    const response = await apiClient.get<Feedback[]>('/api/feedback');
    const data = response.data;
    return {
      content: data.slice(page * size, (page + 1) * size),
      totalElements: data.length,
      totalPages: Math.ceil(data.length / size),
    };
  },

  getMyFeedback: async (
    page = 0,
    size = 10
  ): Promise<{ content: Feedback[]; totalElements: number; totalPages: number }> => {
    const response = await apiClient.get<Feedback[]>('/api/feedback');
    const data = response.data;
    return {
      content: data.slice(page * size, (page + 1) * size),
      totalElements: data.length,
      totalPages: Math.ceil(data.length / size),
    };
  },
};
