import { useState, useMemo } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider, createTheme, CssBaseline } from '@mui/material';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import MainLayout from './components/MainLayout';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import VehiclesPage from './pages/VehiclesPage';
import DriversPage from './pages/DriversPage';
import MaintenancePage from './pages/MaintenancePage';
import AlertsPage from './pages/AlertsPage';
import ReportsPage from './pages/ReportsPage';
import SuperAdminPage from './pages/SuperAdminPage';

// Create Query Client
const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            retry: 1,
            refetchOnWindowFocus: false,
            staleTime: 5 * 60 * 1000, // 5 minutos
        },
    },
});

// Theme configuration
const getTheme = (mode: 'light' | 'dark') => createTheme({
    palette: {
        mode,
        primary: {
            main: '#00d4aa',
            light: '#33debb',
            dark: '#00b894',
            contrastText: '#fff',
        },
        secondary: {
            main: '#6c5ce7',
            light: '#897ce9',
            dark: '#5b4bc4',
        },
        background: {
            default: mode === 'dark' ? '#0a0a0f' : '#f5f7fa',
            paper: mode === 'dark' ? '#12121a' : '#ffffff',
        },
        error: {
            main: '#ff6b6b',
        },
        warning: {
            main: '#ffc107',
        },
        success: {
            main: '#00d4aa',
        },
    },
    typography: {
        fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
        h1: { fontWeight: 700 },
        h2: { fontWeight: 700 },
        h3: { fontWeight: 600 },
        h4: { fontWeight: 600 },
        h5: { fontWeight: 600 },
        h6: { fontWeight: 600 },
    },
    shape: {
        borderRadius: 12,
    },
    components: {
        MuiButton: {
            styleOverrides: {
                root: {
                    textTransform: 'none',
                    fontWeight: 600,
                    borderRadius: 8,
                },
            },
        },
        MuiPaper: {
            styleOverrides: {
                root: {
                    backgroundImage: 'none',
                },
            },
        },
        MuiCard: {
            styleOverrides: {
                root: {
                    backgroundImage: 'none',
                },
            },
        },
        MuiTextField: {
            styleOverrides: {
                root: {
                    '& .MuiOutlinedInput-root': {
                        borderRadius: 8,
                    },
                },
            },
        },
        MuiChip: {
            styleOverrides: {
                root: {
                    fontWeight: 500,
                },
            },
        },
    },
});

// Unauthorized page
const UnauthorizedPage = () => (
    <div style={{ textAlign: 'center', padding: '50px' }}>
        <h1>403 - No Autorizado</h1>
        <p>No tienes permisos para acceder a esta página.</p>
    </div>
);

// Component to redirect SuperAdmin to their panel
import { useAuth } from './contexts/AuthContext';

const SuperAdminRedirect: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const { user } = useAuth();

    // Si es SuperAdmin, redirigir al panel de SuperAdmin
    if (user?.rol === 'SuperAdmin') {
        return <Navigate to="/superadmin" replace />;
    }

    return <>{children}</>;
};

function App() {
    const [mode, setMode] = useState<'light' | 'dark'>(() => {
        const saved = localStorage.getItem('trackway_theme');
        return (saved as 'light' | 'dark') || 'dark';
    });

    const theme = useMemo(() => getTheme(mode), [mode]);

    const toggleTheme = () => {
        const newMode = mode === 'dark' ? 'light' : 'dark';
        setMode(newMode);
        localStorage.setItem('trackway_theme', newMode);
    };

    return (
        <QueryClientProvider client={queryClient}>
            <ThemeProvider theme={theme}>
                <CssBaseline />
                <AuthProvider>
                    <BrowserRouter>
                        <Routes>
                            {/* Public routes */}
                            <Route path="/login" element={<LoginPage />} />
                            <Route path="/unauthorized" element={<UnauthorizedPage />} />

                            {/* Protected routes */}
                            <Route
                                element={
                                    <ProtectedRoute>
                                        <MainLayout toggleTheme={toggleTheme} isDarkMode={mode === 'dark'} />
                                    </ProtectedRoute>
                                }
                            >
                                <Route path="/" element={<SuperAdminRedirect><DashboardPage /></SuperAdminRedirect>} />
                                <Route path="/vehiculos" element={<VehiclesPage />} />
                                <Route path="/conductores" element={<DriversPage />} />
                                <Route path="/mantenimiento" element={<MaintenancePage />} />
                                <Route path="/alertas" element={<AlertsPage />} />
                                <Route
                                    path="/reportes"
                                    element={
                                        <ProtectedRoute requiredRoles={['Admin', 'Gerente']}>
                                            <ReportsPage />
                                        </ProtectedRoute>
                                    }
                                />
                            </Route>

                            {/* SuperAdmin Panel - rutas independientes sin MainLayout */}
                            <Route
                                path="/superadmin"
                                element={
                                    <ProtectedRoute requiredRoles={['SuperAdmin', 'Admin']}>
                                        <SuperAdminPage />
                                    </ProtectedRoute>
                                }
                            />

                            {/* Fallback */}
                            <Route path="*" element={<Navigate to="/" replace />} />
                        </Routes>
                    </BrowserRouter>
                </AuthProvider>
            </ThemeProvider>
        </QueryClientProvider>
    );
}

export default App;
