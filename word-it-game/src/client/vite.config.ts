import { defineConfig } from 'vite';
import { devvit } from '@devvit/start/vite';
import react from '@vitejs/plugin-react';
import tailwind from '@tailwindcss/vite';
import { fileURLToPath } from 'url';
import { dirname, resolve } from 'path';

const __dirname = dirname(fileURLToPath(import.meta.url));

export default defineConfig({
  plugins: [
    react(),     // remove if NOT using React
    tailwind(),  // remove if NOT using Tailwind
  ],

  logLevel: 'warn',

  build: {
    outDir: '../../dist/client',
    emptyOutDir: true,
    sourcemap: true,

    rollupOptions: {
      input: {
        default: resolve(__dirname, 'preview.html'),
        game: resolve(__dirname, 'index.html'),
        PostCreated: resolve(__dirname, 'PostCreated.html'),
        //leaderboard: resolve(__dirname, 'leaderboard.html'),
      },
      output: {
        entryFileNames: '[name].js',
        chunkFileNames: '[name].js',
        assetFileNames: '[name][extname]',
        sourcemapFileNames: '[name].js.map',
      },
    },
  },
});
