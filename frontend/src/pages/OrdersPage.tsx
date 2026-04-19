import React, { useState, useEffect } from 'react';
import {
  Container,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Paper,
  Chip,
  Box,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  Snackbar,
} from '@mui/material';
import { Payment as PaymentIcon } from '@mui/icons-material';
import { useQuery } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { orderService } from '../services/orderService';
import { OrderStatus, OrderType, OrderStatusLabels, OrderTypeLabels } from '../types/order.types';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorMessage from '../components/ErrorMessage';

const statusColors: Record<number, 'default' | 'warning' | 'info' | 'success' | 'error'> = {
  [OrderStatus.Pending]: 'warning',
  [OrderStatus.Processing]: 'info',
  [OrderStatus.Shipped]: 'info',
  [OrderStatus.Delivered]: 'success',
  [OrderStatus.Cancelled]: 'error',
  [OrderStatus.Returned]: 'default',
};

const OrdersPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const paymentResult = searchParams.get('payment');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [statusFilter, setStatusFilter] = useState<OrderStatus | ''>('');
  const [typeFilter, setTypeFilter] = useState<OrderType | ''>('');
  const [payingOrderId, setPayingOrderId] = useState<number | null>(null);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: paymentResult === 'success',
    message: paymentResult === 'success' ? 'Payment completed successfully!' : '',
    severity: 'success',
  });

  // Verify payment when returning from Stripe
  useEffect(() => {
    if (paymentResult === 'success') {
      const orderId = searchParams.get('orderId');
      if (orderId) {
        orderService.verifyPayment(parseInt(orderId, 10)).catch(console.error);
      }
    }
  }, [paymentResult, searchParams]);

  const { data, isLoading, error } = useQuery({
    queryKey: ['myOrders', page, rowsPerPage, statusFilter, typeFilter],
    queryFn: () =>
      orderService.getMyOrders(
        page,
        rowsPerPage,
        statusFilter || undefined,
        typeFilter || undefined
      ),
  });

  const handlePay = async (orderId: number) => {
    setPayingOrderId(orderId);
    try {
      const checkoutUrl = await orderService.payOrder(orderId);
      window.location.href = checkoutUrl;
    } catch (err: any) {
      setSnackbar({
        open: true,
        message: err?.message || 'Failed to start payment.',
        severity: 'error',
      });
      setPayingOrderId(null);
    }
  };

  const handleChangePage = (_: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  if (isLoading) return <LoadingSpinner message="Loading orders..." />;
  if (error) return <ErrorMessage message="Failed to load orders." />;

  const orders = data?.content || [];
  const totalElements = data?.totalElements || 0;

  return (
    <Container maxWidth="lg">
      <Typography variant="h4" component="h1" gutterBottom sx={{ mt: 2 }}>
        My Orders
      </Typography>

      {paymentResult === 'cancelled' && (
        <Alert severity="info" sx={{ mb: 2 }}>
          Payment was cancelled. You can try again anytime.
        </Alert>
      )}

      <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
        <FormControl size="small" sx={{ minWidth: 150 }}>
          <InputLabel>Status</InputLabel>
          <Select
            value={statusFilter}
            label="Status"
            onChange={(e) => {
              setStatusFilter(e.target.value as OrderStatus | '');
              setPage(0);
            }}
          >
            <MenuItem value="">All</MenuItem>
            {Object.values(OrderStatus).filter(v => typeof v === 'number').map((status) => (
              <MenuItem key={status} value={status}>
                {OrderStatusLabels[status as number]}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        <FormControl size="small" sx={{ minWidth: 150 }}>
          <InputLabel>Type</InputLabel>
          <Select
            value={typeFilter}
            label="Type"
            onChange={(e) => {
              setTypeFilter(e.target.value as OrderType | '');
              setPage(0);
            }}
          >
            <MenuItem value="">All</MenuItem>
            {Object.values(OrderType).filter(v => typeof v === 'number').map((type) => (
              <MenuItem key={type} value={type}>
                {OrderTypeLabels[type as number]}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Order ID</TableCell>
              <TableCell>Product</TableCell>
              <TableCell align="right">Quantity</TableCell>
              <TableCell align="right">Total Price</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Date</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {orders.length === 0 ? (
              <TableRow>
                <TableCell colSpan={8} align="center">
                  No orders found.
                </TableCell>
              </TableRow>
            ) : (
              orders.map((order) => (
                <TableRow key={order.id} hover>
                  <TableCell>#{order.id}</TableCell>
                  <TableCell>{order.productName}</TableCell>
                  <TableCell align="right">{order.quantity}</TableCell>
                  <TableCell align="right">${order.totalPrice.toFixed(2)}</TableCell>
                  <TableCell>
                    <Chip label={OrderTypeLabels[order.orderType] || order.orderType} size="small" variant="outlined" />
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={OrderStatusLabels[order.status] || order.status}
                      color={statusColors[order.status] || 'default'}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>
                    {new Date(order.createdAt).toLocaleDateString()}
                  </TableCell>
                  <TableCell>
                    {order.status === OrderStatus.Pending && (
                      <Button
                        size="small"
                        variant="contained"
                        color="primary"
                        startIcon={<PaymentIcon />}
                        disabled={payingOrderId === order.id}
                        onClick={() => handlePay(order.id)}
                      >
                        {payingOrderId === order.id ? 'Redirecting...' : 'Pay'}
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
        <TablePagination
          component="div"
          count={totalElements}
          page={page}
          onPageChange={handleChangePage}
          rowsPerPage={rowsPerPage}
          onRowsPerPageChange={handleChangeRowsPerPage}
          rowsPerPageOptions={[5, 10, 25]}
        />
      </TableContainer>

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

export default OrdersPage;
