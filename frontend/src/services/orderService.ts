import apiClient from './apiClient';
import { Order, OrderCreate, PaginatedOrders, OrderStatus, OrderType } from '../types/order.types';

const wrapAsPaginated = (orders: Order[], page = 0, size = 10): PaginatedOrders => ({
  content: orders.slice(page * size, (page + 1) * size),
  totalElements: orders.length,
  totalPages: Math.ceil(orders.length / size),
  size,
  number: page,
  first: page === 0,
  last: (page + 1) * size >= orders.length,
  empty: orders.length === 0,
});

export const orderService = {
  createOrder: async (data: OrderCreate): Promise<Order> => {
    const response = await apiClient.post<Order>('/api/orders', data);
    return response.data;
  },

  getOrders: async (
    page = 0,
    size = 10,
    _status?: OrderStatus,
    _orderType?: OrderType
  ): Promise<PaginatedOrders> => {
    const response = await apiClient.get<Order[]>('/api/orders');
    return wrapAsPaginated(response.data, page, size);
  },

  getOrder: async (id: number): Promise<Order> => {
    const response = await apiClient.get<Order>(`/api/orders/${id}`);
    return response.data;
  },

  getMyOrders: async (
    page = 0,
    size = 10,
    _status?: OrderStatus,
    _orderType?: OrderType
  ): Promise<PaginatedOrders> => {
    // Use the main orders endpoint (backend returns all for now)
    const response = await apiClient.get<Order[]>('/api/orders');
    return wrapAsPaginated(response.data, page, size);
  },
};
