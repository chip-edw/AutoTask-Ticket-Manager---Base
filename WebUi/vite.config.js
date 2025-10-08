import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  build: {
    // Split big libs so your app code ships smaller initial chunks
    rollupOptions: {
      output: {
        manualChunks: {
          react: ['react', 'react-dom', 'react-router-dom'],
          mui: ['@mui/material', '@mui/icons-material', '@emotion/react', '@emotion/styled'],
          grid: ['@mui/x-data-grid'],
        },
      },
    },
    // optional: silence the warning without being excessive
    chunkSizeWarningLimit: 1500,
    // reasonable baseline that still supports modern browsers
    target: 'es2019',
  },
    test: {
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
    css: true,              // allow CSS imports in components under test
    globals: true,          // lets you use `describe`, `it`, etc. without imports
    reporters: ['default'], // you can add 'junit' later if CI needs it
  },
});
