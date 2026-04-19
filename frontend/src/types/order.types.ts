import { PaginatedResponse } from './common.types';

export enum OrderStatus {
  Pending = 1,
  Processing = 2,
  Shipped = 3,
  Delivered = 4,
  Cancelled = 5,
  Returned = 6,
}

export const OrderStatusLabels: Record<number, string> = {
  [OrderStatus.Pending]: 'Pending',
  [OrderStatus.Processing]: 'Processing',
  [OrderStatus.Shipped]: 'Shipped',
  [OrderStatus.Delivered]: 'Delivered',
  [OrderStatus.Cancelled]: 'Cancelled',
  [OrderStatus.Returned]: 'Returned',
};

export enum OrderType {
  Purchase = 1,
  Rental = 2,
}

export const OrderTypeLabels: Record<number, string> = {
  [OrderType.Purchase]: 'Purchase',
  [OrderType.Rental]: 'Rental',
};

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
  userId: number;
  productId: number;
  quantity: number;
  totalPrice: number;
  orderType: OrderType;
  shippingAddress?: string;
}

export type PaginatedOrders = PaginatedResponse<Order>;
