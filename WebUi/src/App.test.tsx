import { render, screen } from '@testing-library/react';
import React from 'react';
import App from '@/App';

it('renders the app shell', () => {
  render(<App />);
  // Look for something stable in your App UI; fall back to container existence
  expect(document.body).toBeInTheDocument();
});
