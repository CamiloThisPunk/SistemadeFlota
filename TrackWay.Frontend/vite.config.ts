import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
    plugins: [react()],
    resolve: {
        alias: {
            '@': path.resolve(__dirname, './src'),
        },
    },
    server: {
        port: 3000,
        proxy: {
            '/api': {
                target: 'https://localhost:5001',
                changeOrigin: true,
                secure: false,
            },
        },
    },
    build: {
        rollupOptions: {
            output: {
                manualChunks: {
                    // Vendor chunks
                    'vendor-react': ['react', 'react-dom', 'react-router-dom'],
                    'vendor-mui': ['@mui/material', '@mui/icons-material', '@emotion/react', '@emotion/styled'],
                    'vendor-datagrid': ['@mui/x-data-grid'],
                    'vendor-charts': ['recharts'],
                    'vendor-query': ['@tanstack/react-query', 'axios'],
                    'vendor-forms': ['react-hook-form', '@hookform/resolvers', 'zod'],
                    'vendor-i18n': ['i18next', 'react-i18next', 'i18next-browser-languagedetector'],
                },
            },
        },
    },
})
