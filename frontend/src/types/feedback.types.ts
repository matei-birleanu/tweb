export enum FeedbackCategory {
  PRODUCT_QUALITY = 'PRODUCT_QUALITY',
  DELIVERY = 'DELIVERY',
  CUSTOMER_SERVICE = 'CUSTOMER_SERVICE',
  WEBSITE = 'WEBSITE',
}

export interface Feedback {
  id: number;
  userId: string;
  category: FeedbackCategory;
  rating: number;
  comment: string;
  agreedToTerms: boolean;
  createdAt: string;
}

export interface FeedbackCreate {
  category: FeedbackCategory;
  rating: number;
  comment: string;
  agreedToTerms: boolean;
}
