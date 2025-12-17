import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
    Box,
    Paper,
    TextField,
    Button,
    Typography,
    Alert,
    CircularProgress,
    InputAdornment,
    IconButton,
} from '@mui/material';
import { Visibility, VisibilityOff, LocalShipping } from '@mui/icons-material';
import { useAuth } from '../contexts/AuthContext';

const LoginPage: React.FC = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const { login, isLoading } = useAuth();

    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [error, setError] = useState('');

    const from = (location.state as any)?.from?.pathname || '/';

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');

        if (!email || !password) {
            setError('Por favor ingresa email y contraseña');
            return;
        }

        try {
            await login(email, password);
            navigate(from, { replace: true });
        } catch (err: any) {
            setError(err.message || 'Error al iniciar sesión');
        }
    };

    return (
        <Box
            sx={{
                minHeight: '100vh',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                background: 'linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%)',
                p: 2,
            }}
        >
            <Paper
                elevation={24}
                sx={{
                    p: 4,
                    width: '100%',
                    maxWidth: 420,
                    borderRadius: 3,
                    background: 'rgba(255, 255, 255, 0.05)',
                    backdropFilter: 'blur(10px)',
                    border: '1px solid rgba(255, 255, 255, 0.1)',
                }}
            >
                {/* Logo */}
                <Box sx={{ textAlign: 'center', mb: 4 }}>
                    <Box
                        sx={{
                            display: 'inline-flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            width: 80,
                            height: 80,
                            borderRadius: '50%',
                            background: 'linear-gradient(135deg, #00d4aa 0%, #00b894 100%)',
                            mb: 2,
                        }}
                    >
                        <LocalShipping sx={{ fontSize: 40, color: 'white' }} />
                    </Box>
                    <Typography variant="h4" fontWeight={700} color="white">
                        TrackWay
                    </Typography>
                    <Typography color="grey.400" sx={{ mt: 1 }}>
                        Sistema de Gestión de Flota
                    </Typography>
                </Box>

                {/* Form */}
                <form onSubmit={handleSubmit}>
                    {error && (
                        <Alert severity="error" sx={{ mb: 3 }}>
                            {error}
                        </Alert>
                    )}

                    <TextField
                        fullWidth
                        label="Email"
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        sx={{ mb: 2 }}
                        InputProps={{
                            sx: {
                                bgcolor: 'rgba(255, 255, 255, 0.05)',
                                '& input': { color: 'white' }
                            }
                        }}
                        InputLabelProps={{ sx: { color: 'grey.400' } }}
                    />

                    <TextField
                        fullWidth
                        label="Contraseña"
                        type={showPassword ? 'text' : 'password'}
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        sx={{ mb: 3 }}
                        InputProps={{
                            sx: {
                                bgcolor: 'rgba(255, 255, 255, 0.05)',
                                '& input': { color: 'white' }
                            },
                            endAdornment: (
                                <InputAdornment position="end">
                                    <IconButton
                                        onClick={() => setShowPassword(!showPassword)}
                                        sx={{ color: 'grey.400' }}
                                    >
                                        {showPassword ? <VisibilityOff /> : <Visibility />}
                                    </IconButton>
                                </InputAdornment>
                            ),
                        }}
                        InputLabelProps={{ sx: { color: 'grey.400' } }}
                    />

                    <Button
                        type="submit"
                        fullWidth
                        variant="contained"
                        size="large"
                        disabled={isLoading}
                        sx={{
                            py: 1.5,
                            background: 'linear-gradient(135deg, #00d4aa 0%, #00b894 100%)',
                            fontWeight: 600,
                            fontSize: '1rem',
                            '&:hover': {
                                background: 'linear-gradient(135deg, #00b894 0%, #009d7f 100%)',
                            },
                        }}
                    >
                        {isLoading ? <CircularProgress size={24} color="inherit" /> : 'Iniciar Sesión'}
                    </Button>
                </form>

                {/* Demo credentials */}
                <Box sx={{ mt: 3, textAlign: 'center' }}>
                    <Typography color="grey.500" variant="caption">
                        Demo: admin@trackway.com / Admin123!
                    </Typography>
                </Box>
            </Paper>
        </Box>
    );
};

export default LoginPage;
