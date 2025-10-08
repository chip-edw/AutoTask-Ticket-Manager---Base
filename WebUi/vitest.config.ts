// vitest.config.ts
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: { '@': path.resolve(__dirname, './src') },
  },
  test: {
    environment: 'jsdom',
    css: true,                       // <-- handles plain .css imports (e.g., MUI Data Grid CSS)
    setupFiles: './src/test/setup.ts',
    globals: true,
	// Sometimes helps when libraries import CSS from ESM entrypoints in node_modules
	server: { deps: { inline: ['@mui/x-data-grid', '@mui/material'] } },
  },
});
