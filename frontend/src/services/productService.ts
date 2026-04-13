import apiClient from './apiClient';
import {
  Product,
  ProductCreate,
  ProductUpdate,
  SearchParams,
  PaginatedProducts,
} from '../types/product.types';

// Helper to wrap array response as paginated
const wrapAsPaginated = (products: Product[], page = 0, size = 10): PaginatedProducts => ({
  content: products.slice(page * size, (page + 1) * size),
  totalElements: products.length,
  totalPages: Math.ceil(products.length / size),
  size,
  number: page,
  first: page === 0,
  last: (page + 1) * size >= products.length,
  empty: products.length === 0,
});

export const productService = {
  getProducts: async (params?: SearchParams): Promise<PaginatedProducts> => {
    const response = await apiClient.get<Product[]>('/api/products');
    return wrapAsPaginated(response.data, params?.page || 0, params?.size || 10);
  },

  getProduct: async (id: number): Promise<Product> => {
    const response = await apiClient.get<Product>(`/api/products/${id}`);
    return response.data;
  },

  createProduct: async (data: ProductCreate): Promise<Product> => {
    const response = await apiClient.post<Product>('/api/products', data);
    return response.data;
  },

  updateProduct: async (id: number, data: ProductUpdate): Promise<Product> => {
    const response = await apiClient.put<Product>(`/api/products/${id}`, data);
    return response.data;
  },

  deleteProduct: async (id: number): Promise<void> => {
    await apiClient.delete(`/api/products/${id}`);
  },

  searchProducts: async (params: SearchParams): Promise<PaginatedProducts> => {
    const response = await apiClient.get<Product[]>('/api/products');
    let products = response.data;
    if (params.query) {
      const q = params.query.toLowerCase();
      products = products.filter(
        (p) => p.name.toLowerCase().includes(q) || p.description?.toLowerCase().includes(q)
      );
    }
    if (params.category) {
      products = products.filter((p) => p.category === params.category);
    }
    return wrapAsPaginated(products, params.page || 0, params.size || 10);
  },
};
