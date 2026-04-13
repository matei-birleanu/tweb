import * as Yup from 'yup';

export const loginSchema = Yup.object({
  username: Yup.string()
    .required('Username is required')
    .min(3, 'Username must be at least 3 characters'),
  password: Yup.string()
    .required('Password is required')
    .min(6, 'Password must be at least 6 characters'),
});

export const registerSchema = Yup.object({
  username: Yup.string()
    .required('Username is required')
    .min(3, 'Username must be at least 3 characters')
    .max(50, 'Username must be at most 50 characters'),
  email: Yup.string()
    .required('Email is required')
    .email('Invalid email address'),
  password: Yup.string()
    .required('Password is required')
    .min(8, 'Password must be at least 8 characters')
    .matches(
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/,
      'Password must contain at least one uppercase letter, one lowercase letter, and one number'
    ),
  confirmPassword: Yup.string()
    .required('Confirm password is required')
    .oneOf([Yup.ref('password')], 'Passwords must match'),
  firstName: Yup.string().max(50, 'First name must be at most 50 characters'),
  lastName: Yup.string().max(50, 'Last name must be at most 50 characters'),
});

export const productSchema = Yup.object({
  name: Yup.string()
    .required('Name is required')
    .min(3, 'Name must be at least 3 characters')
    .max(100, 'Name must be at most 100 characters'),
  description: Yup.string()
    .required('Description is required')
    .min(10, 'Description must be at least 10 characters')
    .max(1000, 'Description must be at most 1000 characters'),
  price: Yup.number()
    .required('Price is required')
    .positive('Price must be positive')
    .min(0.01, 'Price must be at least 0.01'),
  stock: Yup.number()
    .required('Stock is required')
    .integer('Stock must be an integer')
    .min(0, 'Stock cannot be negative'),
  category: Yup.string()
    .required('Category is required')
    .min(3, 'Category must be at least 3 characters')
    .max(50, 'Category must be at most 50 characters'),
});

export const orderSchema = Yup.object({
  productId: Yup.number()
    .required('Product is required')
    .positive('Invalid product'),
  quantity: Yup.number()
    .required('Quantity is required')
    .integer('Quantity must be an integer')
    .positive('Quantity must be positive')
    .min(1, 'Quantity must be at least 1'),
  orderType: Yup.string()
    .required('Order type is required')
    .oneOf(['ONLINE', 'IN_STORE'], 'Invalid order type'),
});

export const feedbackSchema = Yup.object({
  category: Yup.string()
    .required('Category is required')
    .oneOf(
      ['PRODUCT_QUALITY', 'DELIVERY', 'CUSTOMER_SERVICE', 'WEBSITE'],
      'Invalid category'
    ),
  rating: Yup.number()
    .required('Rating is required')
    .min(1, 'Rating must be between 1 and 5')
    .max(5, 'Rating must be between 1 and 5'),
  comment: Yup.string()
    .required('Comment is required')
    .min(10, 'Comment must be at least 10 characters')
    .max(1000, 'Comment must be at most 1000 characters'),
  agreedToTerms: Yup.boolean()
    .required('You must agree to terms')
    .oneOf([true], 'You must agree to terms'),
});
