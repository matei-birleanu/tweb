import { PaginatedResponse } from './common.types';

export enum OrderStatus {
  PENDING = 'PENDING',
  CONFIRMED = 'CONFIRMED',
  SHIPPED = 'SHIPPED',
  DELIVERED = 'DELIVERED',
  CANCELLED = 'CANCELLED',
}

export enum OrderType {
  ONLINE = 'ONLINE',
  IN_STORE = 'IN_STORE',
}

export interface Order {
  id: number;
  userId: string;
  productId: number;
  productName: string;
  quantity: number;
  totalPrice: number;
  status: OrderStatus;
  orderType: OrderType;
  createdAt: string;
  updatedAt: string;
}

export interface OrderCreate {
  productId: number;
  quantity: number;
  orderType: OrderType;
}

export type PaginatedOrders = PaginatedResponse<Order>;
