import React, { useState } from 'react';
import {
  Container,
  Typography,
  Paper,
  Box,
  Button,
  Chip,
  Grid,
  Divider,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  Snackbar,
} from '@mui/material';
import { ArrowBack, ShoppingCart } from '@mui/icons-material';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { productService } from '../services/productService';
import { orderService } from '../services/orderService';
import { OrderType } from '../types/order.types';
import { isAuthenticated } from '../utils/storage';
import { authService } from '../services/authService';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorMessage from '../components/ErrorMessage';

const ProductDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [orderDialogOpen, setOrderDialogOpen] = useState(false);
  const [quantity, setQuantity] = useState(1);
  const [orderType, setOrderType] = useState<OrderType>(OrderType.Purchase);
  const [submitting, setSubmitting] = useState(false);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success',
  });

  const { data: product, isLoading, error } = useQuery({
    queryKey: ['product', id],
    queryFn: () => productService.getProduct(Number(id)),
    enabled: !!id,
  });

  const handlePlaceOrder = async () => {
    if (!product) return;
    setSubmitting(true);
    try {
      const user = await authService.getCurrentUser();
      const orderData = {
        productId: product.id,
        quantity,
        orderType: Number(orderType),
        userId: parseInt(user.id, 10),
        totalPrice: product.price * quantity,
      };
      console.log('Sending order:', JSON.stringify(orderData));
      await orderService.createOrder(orderData);
      setOrderDialogOpen(false);
      setQuantity(1);
      setSnackbar({ open: true, message: 'Order placed successfully!', severity: 'success' });
    } catch (err: any) {
      console.error('Order error:', err);
      // err is transformed by apiClient interceptor into ErrorResponse {message, errors}
      const msg = err?.message || err?.response?.data?.title || 'Failed to place order.';
      const validationErrors = err?.errors;
      let fullMsg = msg;
      if (validationErrors) {
        const details = Object.values(validationErrors).flat().join('; ');
        if (details) fullMsg = details;
      }
      setSnackbar({ open: true, message: fullMsg, severity: 'error' });
    } finally {
      setSubmitting(false);
    }
  };

  if (isLoading) return <LoadingSpinner message="Loading product..." />;
  if (error || !product) return <ErrorMessage message="Product not found." />;

  const loggedIn = isAuthenticated();

  return (
    <Container maxWidth="md">
      <Button
        startIcon={<ArrowBack />}
        onClick={() => navigate('/products')}
        sx={{ mt: 2, mb: 2 }}
      >
        Back to Products
      </Button>

      <Paper sx={{ p: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          {product.name}
        </Typography>

        <Box sx={{ display: 'flex', gap: 1, mb: 2 }}>
          <Chip label={product.category} color="primary" />
          <Chip
            label={product.stock > 0 ? `In Stock (${product.stock})` : 'Out of Stock'}
            color={product.stock > 0 ? 'success' : 'error'}
          />
        </Box>

        <Divider sx={{ my: 2 }} />

        <Grid container spacing={3}>
          <Grid item xs={12}>
            <Typography variant="h5" color="primary" gutterBottom>
              ${product.price.toFixed(2)}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6" gutterBottom>
              Description
            </Typography>
            <Typography variant="body1" color="text.secondary">
              {product.description}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Box sx={{ display: 'flex', gap: 2, mt: 2 }}>
              {loggedIn ? (
                <Button
                  variant="contained"
                  color="primary"
                  size="large"
                  startIcon={<ShoppingCart />}
                  disabled={product.stock <= 0}
                  onClick={() => setOrderDialogOpen(true)}
                >
                  {product.stock > 0 ? 'Place Order' : 'Out of Stock'}
                </Button>
              ) : (
                <Button
                  variant="contained"
                  size="large"
                  onClick={() => navigate('/login')}
                >
                  Login to Order
                </Button>
              )}
            </Box>
          </Grid>

          <Grid item xs={6}>
            <Typography variant="body2" color="text.secondary">
              Created: {new Date(product.createdAt).toLocaleDateString()}
            </Typography>
          </Grid>
          <Grid item xs={6}>
            <Typography variant="body2" color="text.secondary">
              Updated: {new Date(product.updatedAt).toLocaleDateString()}
            </Typography>
          </Grid>
        </Grid>
      </Paper>

      {/* Order Dialog */}
      <Dialog open={orderDialogOpen} onClose={() => setOrderDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Place Order - {product.name}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 1 }}>
            <TextField
              label="Quantity"
              type="number"
              value={quantity}
              onChange={(e) => setQuantity(Math.max(1, Math.min(product.stock, parseInt(e.target.value) || 1)))}
              inputProps={{ min: 1, max: product.stock }}
              fullWidth
            />
            <FormControl fullWidth>
              <InputLabel>Order Type</InputLabel>
              <Select
                value={orderType}
                label="Order Type"
                onChange={(e) => setOrderType(e.target.value as OrderType)}
              >
                <MenuItem value={OrderType.Purchase}>Purchase</MenuItem>
                <MenuItem value={OrderType.Rental}>Rental</MenuItem>
              </Select>
            </FormControl>
            <Divider />
            <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
              <Typography variant="body1">Unit Price:</Typography>
              <Typography variant="body1">${product.price.toFixed(2)}</Typography>
            </Box>
            <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
              <Typography variant="body1">Quantity:</Typography>
              <Typography variant="body1">{quantity}</Typography>
            </Box>
            <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
              <Typography variant="h6">Total:</Typography>
              <Typography variant="h6" color="primary">
                ${(product.price * quantity).toFixed(2)}
              </Typography>
            </Box>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOrderDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handlePlaceOrder}
            disabled={submitting}
          >
            {submitting ? 'Placing Order...' : 'Confirm Order'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Snackbar */}
      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar((s) => ({ ...s, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert
          onClose={() => setSnackbar((s) => ({ ...s, open: false }))}
          severity={snackbar.severity}
          variant="filled"
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Container>
  );
};

export default ProductDetailPage;
