import React from 'react';
import { Container, Typography, Box, Button, Grid, Paper } from '@mui/material';
import { ShoppingCart, Inventory, Feedback as FeedbackIcon } from '@mui/icons-material';
import { Link } from 'react-router-dom';

const HomePage: React.FC = () => {
  return (
    <Container maxWidth="lg">
      <Box sx={{ textAlign: 'center', mt: 6, mb: 6 }}>
        <Typography variant="h3" component="h1" gutterBottom>
          Welcome to TWeb Shop
        </Typography>
        <Typography variant="h6" color="text.secondary" paragraph>
          Your one-stop e-commerce platform. Browse products, place orders, and share feedback.
        </Typography>
        <Button
          variant="contained"
          size="large"
          component={Link}
          to="/products"
          sx={{ mt: 2 }}
        >
          Browse Products
        </Button>
      </Box>

      <Grid container spacing={4} sx={{ mt: 2 }}>
        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 4, textAlign: 'center', height: '100%' }}>
            <Inventory sx={{ fontSize: 48, color: 'primary.main', mb: 2 }} />
            <Typography variant="h5" gutterBottom>
              Products
            </Typography>
            <Typography color="text.secondary" paragraph>
              Explore our catalog with search and filtering options.
            </Typography>
            <Button variant="outlined" component={Link} to="/products">
              View Products
            </Button>
          </Paper>
        </Grid>

        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 4, textAlign: 'center', height: '100%' }}>
            <ShoppingCart sx={{ fontSize: 48, color: 'primary.main', mb: 2 }} />
            <Typography variant="h5" gutterBottom>
              Orders
            </Typography>
            <Typography color="text.secondary" paragraph>
              Track your orders and view purchase history.
            </Typography>
            <Button variant="outlined" component={Link} to="/orders">
              My Orders
            </Button>
          </Paper>
        </Grid>

        <Grid item xs={12} md={4}>
          <Paper sx={{ p: 4, textAlign: 'center', height: '100%' }}>
            <FeedbackIcon sx={{ fontSize: 48, color: 'primary.main', mb: 2 }} />
            <Typography variant="h5" gutterBottom>
              Feedback
            </Typography>
            <Typography color="text.secondary" paragraph>
              Share your experience and help us improve.
            </Typography>
            <Button variant="outlined" component={Link} to="/feedback">
              Give Feedback
            </Button>
          </Paper>
        </Grid>
      </Grid>
    </Container>
  );
};

export default HomePage;
