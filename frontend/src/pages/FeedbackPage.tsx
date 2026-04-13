import React, { useState } from 'react';
import {
  Container,
  Typography,
  Paper,
  Box,
  Button,
  TextField,
  FormControl,
  FormLabel,
  RadioGroup,
  FormControlLabel,
  Radio,
  Checkbox,
  Select,
  MenuItem,
  InputLabel,
  Alert,
} from '@mui/material';
import { useFormik } from 'formik';
import { useMutation } from '@tanstack/react-query';
import { feedbackService } from '../services/feedbackService';
import { FeedbackCategory } from '../types/feedback.types';
import { feedbackSchema } from '../utils/validation';

const categoryLabels: Record<FeedbackCategory, string> = {
  [FeedbackCategory.PRODUCT_QUALITY]: 'Product Quality',
  [FeedbackCategory.DELIVERY]: 'Delivery',
  [FeedbackCategory.CUSTOMER_SERVICE]: 'Customer Service',
  [FeedbackCategory.WEBSITE]: 'Website',
};

const FeedbackPage: React.FC = () => {
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const submitMutation = useMutation({
    mutationFn: feedbackService.submitFeedback,
    onSuccess: () => {
      setSuccess(true);
      setError(null);
      formik.resetForm();
    },
    onError: (err: any) => {
      setError(err?.response?.data?.message || 'Failed to submit feedback.');
      setSuccess(false);
    },
  });

  const formik = useFormik({
    initialValues: {
      category: '' as FeedbackCategory | '',
      rating: 0,
      comment: '',
      agreedToTerms: false,
    },
    validationSchema: feedbackSchema,
    onSubmit: (values) => {
      submitMutation.mutate({
        category: values.category as FeedbackCategory,
        rating: values.rating,
        comment: values.comment,
        agreedToTerms: values.agreedToTerms,
      });
    },
  });

  return (
    <Container maxWidth="sm">
      <Typography variant="h4" component="h1" gutterBottom sx={{ mt: 2 }}>
        Feedback
      </Typography>

      <Paper sx={{ p: 4 }}>
        {success && (
          <Alert severity="success" sx={{ mb: 2 }} onClose={() => setSuccess(false)}>
            Thank you for your feedback!
          </Alert>
        )}

        {error && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
            {error}
          </Alert>
        )}

        <Box component="form" onSubmit={formik.handleSubmit} noValidate>
          {/* Select - Category */}
          <FormControl
            fullWidth
            margin="normal"
            error={formik.touched.category && Boolean(formik.errors.category)}
          >
            <InputLabel id="category-label">Category</InputLabel>
            <Select
              labelId="category-label"
              id="category"
              name="category"
              label="Category"
              value={formik.values.category}
              onChange={formik.handleChange}
              onBlur={formik.handleBlur}
            >
              {Object.values(FeedbackCategory).map((cat) => (
                <MenuItem key={cat} value={cat}>
                  {categoryLabels[cat]}
                </MenuItem>
              ))}
            </Select>
            {formik.touched.category && formik.errors.category && (
              <Typography variant="caption" color="error" sx={{ mt: 0.5, ml: 2 }}>
                {formik.errors.category}
              </Typography>
            )}
          </FormControl>

          {/* Radio Buttons - Rating */}
          <FormControl
            component="fieldset"
            margin="normal"
            error={formik.touched.rating && Boolean(formik.errors.rating)}
          >
            <FormLabel component="legend">Rating</FormLabel>
            <RadioGroup
              row
              name="rating"
              value={String(formik.values.rating)}
              onChange={(e) => formik.setFieldValue('rating', Number(e.target.value))}
            >
              {[1, 2, 3, 4, 5].map((value) => (
                <FormControlLabel
                  key={value}
                  value={String(value)}
                  control={<Radio />}
                  label={String(value)}
                />
              ))}
            </RadioGroup>
            {formik.touched.rating && formik.errors.rating && (
              <Typography variant="caption" color="error">
                {formik.errors.rating}
              </Typography>
            )}
          </FormControl>

          {/* Textarea - Comment */}
          <TextField
            fullWidth
            id="comment"
            name="comment"
            label="Comment"
            multiline
            rows={4}
            margin="normal"
            value={formik.values.comment}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            error={formik.touched.comment && Boolean(formik.errors.comment)}
            helperText={formik.touched.comment && formik.errors.comment}
          />

          {/* Checkbox - Agree to terms */}
          <FormControlLabel
            control={
              <Checkbox
                id="agreedToTerms"
                name="agreedToTerms"
                checked={formik.values.agreedToTerms}
                onChange={formik.handleChange}
              />
            }
            label="I agree to the terms and conditions"
          />
          {formik.touched.agreedToTerms && formik.errors.agreedToTerms && (
            <Typography variant="caption" color="error" display="block">
              {formik.errors.agreedToTerms}
            </Typography>
          )}

          <Button
            type="submit"
            fullWidth
            variant="contained"
            size="large"
            disabled={submitMutation.isPending}
            sx={{ mt: 3 }}
          >
            {submitMutation.isPending ? 'Submitting...' : 'Submit Feedback'}
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};

export default FeedbackPage;
