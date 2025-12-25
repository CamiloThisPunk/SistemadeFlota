import { useState, useEffect, useCallback } from 'react';
import {
    Box, Paper, Typography, Grid2 as Grid, Card, CardContent, Chip, TextField, Select,
    MenuItem, FormControl, InputLabel, Button, Table, TableBody, TableCell,
    TableContainer, TableHead, TableRow, IconButton, LinearProgress, alpha,
    Pagination, InputAdornment, Tooltip, CircularProgress, Snackbar, Alert,
    Tabs, Tab, Dialog, DialogTitle, DialogContent, DialogActions
} from '@mui/material';
import {
    Business, People, DirectionsCar, Build, Refresh, Download, Logout,
    Search, CheckCircle, Cancel, Warning, TrendingUp, Email, Dashboard,
    CreditCard, Add, Edit, Visibility
} from '@mui/icons-material';
import api from '../services/api';

// ===================== Types =====================
interface DashboardKPIs {
    totalEmpresas: number;
    empresasActivas: number;
    empresasInactivas: number;
    totalUsuarios: number;
    promedioUsuariosPorEmpresa: number;
    totalVehiculos: number;
    totalMantenimientos: number;
}

interface SubscriptionPlan {
    id: number;
    nombre: string;
    descripcion: string;
    precioMensual: number;
    maxUsuarios: number;
    maxVehiculos: number;
    tier: string;
    colorHex: string;
}

interface Tenant {
    id: number;
    nombre: string;
    ruc: string;
    emailContacto: string;
    telefono: string | null;
    direccion: string | null;
    activo: boolean;
    fechaCreacion: string;
    totalUsuarios: number;
    totalVehiculos: number;
    totalMantenimientos: number;
    plan: SubscriptionPlan | null;
}

interface PlanStats {
    planId: number;
    nombre: string;
    precioMensual: number;
    colorHex: string;
    totalEmpresas: number;
    porcentajeDelTotal: number;
    maxVehiculos: number;
    promedioUsoVehiculos: number;
    empresasCercaDelLimite: number;
}

interface PagedTenantsResult {
    items: Tenant[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
}

interface TabPanelProps {
    children?: React.ReactNode;
    index: number;
    value: number;
}

// ===================== Tab Panel =====================
function TabPanel(props: TabPanelProps) {
    const { children, value, index, ...other } = props;
    return (
        <div role="tabpanel" hidden={value !== index} {...other}>
            {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
        </div>
    );
}

// ===================== Component =====================
const SuperAdminPage = () => {
    // State
    const [activeTab, setActiveTab] = useState(0);
    const [kpis, setKpis] = useState<DashboardKPIs | null>(null);
    const [tenants, setTenants] = useState<PagedTenantsResult | null>(null);
    const [planStats, setPlanStats] = useState<PlanStats[]>([]);
    const [plans, setPlans] = useState<SubscriptionPlan[]>([]);
    const [loading, setLoading] = useState(true);
    const [search, setSearch] = useState('');
    const [filterPlan, setFilterPlan] = useState<number | ''>('');
    const [filterStatus, setFilterStatus] = useState<boolean | ''>('');
    const [page, setPage] = useState(1);
    const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({ open: false, message: '', severity: 'success' });

    // Dialog states
    const [createDialog, setCreateDialog] = useState(false);
    const [editDialog, setEditDialog] = useState<{ open: boolean; tenant: Tenant | null }>({ open: false, tenant: null });
    const [viewDialog, setViewDialog] = useState<{ open: boolean; tenant: Tenant | null }>({ open: false, tenant: null });
    const [deleteDialog, setDeleteDialog] = useState<{ open: boolean; tenantId: number | null }>({ open: false, tenantId: null });

    // Form states
    const [formData, setFormData] = useState({
        nombre: '',
        ruc: '',
        emailContacto: '',
        telefono: '',
        direccion: '',
        planId: 1
    });

    // Fetch functions
    const fetchDashboard = useCallback(async () => {
        try {
            const response = await api.get('/api/superadmin/dashboard');
            setKpis(response.data);
        } catch (error) {
            console.error('Error fetching dashboard:', error);
        }
    }, []);

    const fetchTenants = useCallback(async () => {
        try {
            const params = new URLSearchParams({
                pageNumber: page.toString(),
                pageSize: '10',
            });
            if (search) params.append('search', search);
            if (filterPlan !== '') params.append('planId', filterPlan.toString());
            if (filterStatus !== '') params.append('activo', filterStatus.toString());

            const response = await api.get(`/api/superadmin/tenants?${params}`);
            setTenants(response.data);
        } catch (error) {
            console.error('Error fetching tenants:', error);
        }
    }, [page, search, filterPlan, filterStatus]);

    const fetchPlanStats = useCallback(async () => {
        try {
            const response = await api.get('/api/superadmin/plans/stats');
            setPlanStats(response.data);
        } catch (error) {
            console.error('Error fetching plan stats:', error);
        }
    }, []);

    const fetchPlans = useCallback(async () => {
        try {
            const response = await api.get('/api/superadmin/plans');
            setPlans(response.data);
        } catch (error) {
            console.error('Error fetching plans:', error);
        }
    }, []);

    const fetchAll = useCallback(async () => {
        setLoading(true);
        await Promise.all([fetchDashboard(), fetchTenants(), fetchPlanStats(), fetchPlans()]);
        setLoading(false);
    }, [fetchDashboard, fetchTenants, fetchPlanStats, fetchPlans]);

    useEffect(() => {
        fetchAll();
    }, [fetchAll]);

    useEffect(() => {
        fetchTenants();
    }, [fetchTenants]);

    // CRUD Actions
    const handleCreateTenant = async () => {
        try {
            const response = await api.post('/api/superadmin/tenants', formData);
            if (response.data.success) {
                setSnackbar({ open: true, message: 'Empresa creada exitosamente', severity: 'success' });
                setCreateDialog(false);
                resetForm();
                fetchTenants();
                fetchDashboard();
                fetchPlanStats();
            }
        } catch (error: any) {
            setSnackbar({ open: true, message: error.response?.data?.error || 'Error al crear empresa', severity: 'error' });
        }
    };

    const handleUpdateTenant = async () => {
        if (!editDialog.tenant) return;
        try {
            const response = await api.put(`/api/superadmin/tenants/${editDialog.tenant.id}`, formData);
            if (response.data.success) {
                setSnackbar({ open: true, message: 'Empresa actualizada exitosamente', severity: 'success' });
                setEditDialog({ open: false, tenant: null });
                resetForm();
                fetchTenants();
                fetchDashboard();
                fetchPlanStats();
            }
        } catch (error: any) {
            setSnackbar({ open: true, message: error.response?.data?.error || 'Error al actualizar empresa', severity: 'error' });
        }
    };

    const handleDeleteTenant = async () => {
        if (!deleteDialog.tenantId) return;
        try {
            // First deactivate, then we could add a real delete endpoint later
            await api.post(`/api/superadmin/tenants/${deleteDialog.tenantId}/toggle`);
            setSnackbar({ open: true, message: 'Empresa desactivada exitosamente', severity: 'success' });
            setDeleteDialog({ open: false, tenantId: null });
            fetchTenants();
            fetchDashboard();
        } catch (error) {
            setSnackbar({ open: true, message: 'Error al desactivar empresa', severity: 'error' });
        }
    };

    const handleToggleStatus = async (tenantId: number) => {
        try {
            const response = await api.post(`/api/superadmin/tenants/${tenantId}/toggle`);
            if (response.data.success) {
                setSnackbar({ open: true, message: `Empresa ${response.data.nuevoEstado ? 'activada' : 'desactivada'} exitosamente`, severity: 'success' });
                fetchTenants();
                fetchDashboard();
            }
        } catch (error) {
            setSnackbar({ open: true, message: 'Error al cambiar estado', severity: 'error' });
        }
    };

    const handleExport = async () => {
        try {
            const response = await api.get('/api/superadmin/export', { responseType: 'blob' });
            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `tenants_export_${new Date().toISOString().split('T')[0]}.csv`);
            document.body.appendChild(link);
            link.click();
            link.remove();
            setSnackbar({ open: true, message: 'Archivo exportado exitosamente', severity: 'success' });
        } catch (error) {
            setSnackbar({ open: true, message: 'Error al exportar', severity: 'error' });
        }
    };

    const handleLogout = () => {
        localStorage.removeItem('trackway_token');
        localStorage.removeItem('trackway_refresh_token');
        localStorage.removeItem('trackway_user');
        window.location.href = '/login';
    };

    const resetForm = () => {
        setFormData({ nombre: '', ruc: '', emailContacto: '', telefono: '', direccion: '', planId: 1 });
    };

    const openEditDialog = (tenant: Tenant) => {
        setFormData({
            nombre: tenant.nombre,
            ruc: tenant.ruc,
            emailContacto: tenant.emailContacto,
            telefono: tenant.telefono || '',
            direccion: tenant.direccion || '',
            planId: tenant.plan?.id || 1
        });
        setEditDialog({ open: true, tenant });
    };

    // Render
    if (loading) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh', bgcolor: 'background.default' }}>
                <CircularProgress size={60} />
            </Box>
        );
    }

    return (
        <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
            {/* Header con Gradiente */}
            <Box
                sx={{
                    background: 'linear-gradient(135deg, #6c5ce7 0%, #0984e3 50%, #00cec9 100%)',
                    color: 'white',
                    py: 3,
                    px: 3,
                    boxShadow: '0 4px 20px rgba(108, 92, 231, 0.3)',
                }}
            >
                <Box sx={{ maxWidth: 1400, mx: 'auto', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Box>
                        <Typography variant="h4" fontWeight={700} sx={{ mb: 0.5, display: 'flex', alignItems: 'center', gap: 1 }}>
                            🚀 Panel de SuperAdmin
                        </Typography>
                        <Typography variant="body2" sx={{ opacity: 0.9 }}>
                            Gestión global del SaaS TrackWay
                        </Typography>
                    </Box>
                    <Box sx={{ display: 'flex', gap: 1 }}>
                        <Button
                            variant="contained"
                            size="small"
                            startIcon={<Refresh />}
                            onClick={fetchAll}
                            sx={{ bgcolor: 'rgba(255,255,255,0.2)', '&:hover': { bgcolor: 'rgba(255,255,255,0.3)' } }}
                        >
                            Actualizar
                        </Button>
                        <Button
                            variant="contained"
                            size="small"
                            startIcon={<Logout />}
                            onClick={handleLogout}
                            sx={{ bgcolor: 'rgba(255,255,255,0.2)', '&:hover': { bgcolor: 'rgba(255,0,0,0.4)' } }}
                        >
                            Salir
                        </Button>
                    </Box>
                </Box>
            </Box>

            {/* Tabs Navigation */}
            <Box sx={{ maxWidth: 1400, mx: 'auto', px: 3, pt: 2 }}>
                <Paper sx={{ borderRadius: 2, mb: 3 }}>
                    <Tabs
                        value={activeTab}
                        onChange={(_, newValue) => setActiveTab(newValue)}
                        variant="fullWidth"
                        sx={{
                            '& .MuiTab-root': { py: 2, fontWeight: 600 },
                            '& .Mui-selected': { color: 'primary.main' },
                        }}
                    >
                        <Tab icon={<Dashboard />} label="Dashboard" iconPosition="start" />
                        <Tab icon={<CreditCard />} label="Planes de Suscripción" iconPosition="start" />
                        <Tab icon={<Business />} label="Gestión de Empresas" iconPosition="start" />
                    </Tabs>
                </Paper>

                {/* ==================== TAB 0: DASHBOARD ==================== */}
                <TabPanel value={activeTab} index={0}>
                    <Typography variant="h5" fontWeight={700} sx={{ mb: 3 }}>
                        📊 Resumen Ejecutivo
                    </Typography>
                    <Grid container spacing={3}>
                        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                            <Card sx={{ background: 'linear-gradient(135deg, #6c5ce7 0%, #a55eea 100%)', color: 'white', borderRadius: 3, height: '100%' }}>
                                <CardContent>
                                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                                        <Box>
                                            <Typography variant="overline" sx={{ opacity: 0.8 }}>Empresas Totales</Typography>
                                            <Typography variant="h3" fontWeight={700}>{kpis?.totalEmpresas ?? 0}</Typography>
                                            <Box sx={{ display: 'flex', gap: 1, mt: 1 }}>
                                                <Chip label={`${kpis?.empresasActivas ?? 0} Activas`} size="small" sx={{ bgcolor: 'rgba(0,255,0,0.3)', color: 'white' }} />
                                                <Chip label={`${kpis?.empresasInactivas ?? 0} Inactivas`} size="small" sx={{ bgcolor: 'rgba(255,0,0,0.3)', color: 'white' }} />
                                            </Box>
                                        </Box>
                                        <Business sx={{ fontSize: 48, opacity: 0.3 }} />
                                    </Box>
                                </CardContent>
                            </Card>
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                            <Card sx={{ background: 'linear-gradient(135deg, #0984e3 0%, #74b9ff 100%)', color: 'white', borderRadius: 3, height: '100%' }}>
                                <CardContent>
                                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                                        <Box>
                                            <Typography variant="overline" sx={{ opacity: 0.8 }}>Usuarios Totales</Typography>
                                            <Typography variant="h3" fontWeight={700}>{kpis?.totalUsuarios ?? 0}</Typography>
                                            <Typography variant="body2" sx={{ mt: 1, opacity: 0.8 }}>
                                                Promedio: {kpis?.promedioUsuariosPorEmpresa?.toFixed(1) ?? 0}/empresa
                                            </Typography>
                                        </Box>
                                        <People sx={{ fontSize: 48, opacity: 0.3 }} />
                                    </Box>
                                </CardContent>
                            </Card>
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                            <Card sx={{ background: 'linear-gradient(135deg, #00cec9 0%, #81ecec 100%)', color: 'white', borderRadius: 3, height: '100%' }}>
                                <CardContent>
                                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                                        <Box>
                                            <Typography variant="overline" sx={{ opacity: 0.8 }}>Vehículos Totales</Typography>
                                            <Typography variant="h3" fontWeight={700}>{kpis?.totalVehiculos ?? 0}</Typography>
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 1 }}>
                                                <TrendingUp fontSize="small" />
                                                <Typography variant="body2">Flota global</Typography>
                                            </Box>
                                        </Box>
                                        <DirectionsCar sx={{ fontSize: 48, opacity: 0.3 }} />
                                    </Box>
                                </CardContent>
                            </Card>
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                            <Card sx={{ background: 'linear-gradient(135deg, #fdcb6e 0%, #f39c12 100%)', color: 'white', borderRadius: 3, height: '100%' }}>
                                <CardContent>
                                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                                        <Box>
                                            <Typography variant="overline" sx={{ opacity: 0.8 }}>Mantenimientos</Typography>
                                            <Typography variant="h3" fontWeight={700}>{kpis?.totalMantenimientos ?? 0}</Typography>
                                            <Typography variant="body2" sx={{ mt: 1, opacity: 0.8 }}>Registros históricos</Typography>
                                        </Box>
                                        <Build sx={{ fontSize: 48, opacity: 0.3 }} />
                                    </Box>
                                </CardContent>
                            </Card>
                        </Grid>
                    </Grid>

                    {/* Quick Stats Table */}
                    <Paper sx={{ mt: 4, borderRadius: 3, overflow: 'hidden' }}>
                        <Box sx={{ p: 2, bgcolor: 'primary.main', color: 'white' }}>
                            <Typography variant="h6" fontWeight={600}>📈 Top 5 Empresas por Actividad</Typography>
                        </Box>
                        <TableContainer>
                            <Table>
                                <TableHead>
                                    <TableRow>
                                        <TableCell><strong>Empresa</strong></TableCell>
                                        <TableCell><strong>Plan</strong></TableCell>
                                        <TableCell align="center"><strong>Usuarios</strong></TableCell>
                                        <TableCell align="center"><strong>Vehículos</strong></TableCell>
                                        <TableCell align="center"><strong>Mantenimientos</strong></TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {tenants?.items.slice(0, 5).map((tenant) => (
                                        <TableRow key={tenant.id} hover>
                                            <TableCell>
                                                <Typography fontWeight={600}>{tenant.nombre}</Typography>
                                            </TableCell>
                                            <TableCell>
                                                {tenant.plan && (
                                                    <Chip label={tenant.plan.nombre} size="small" sx={{ bgcolor: tenant.plan.colorHex, color: 'white' }} />
                                                )}
                                            </TableCell>
                                            <TableCell align="center">{tenant.totalUsuarios}</TableCell>
                                            <TableCell align="center">{tenant.totalVehiculos}</TableCell>
                                            <TableCell align="center">{tenant.totalMantenimientos}</TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    </Paper>
                </TabPanel>

                {/* ==================== TAB 1: PLANES ==================== */}
                <TabPanel value={activeTab} index={1}>
                    <Typography variant="h5" fontWeight={700} sx={{ mb: 3 }}>
                        💳 Distribución por Planes de Suscripción
                    </Typography>
                    <Grid container spacing={3}>
                        {planStats.map((stat) => (
                            <Grid size={{ xs: 12, md: 4 }} key={stat.planId}>
                                <Card sx={{ borderRadius: 3, overflow: 'hidden', height: '100%' }}>
                                    <Box sx={{ bgcolor: stat.colorHex, py: 2, px: 2, color: 'white' }}>
                                        <Typography variant="h5" fontWeight={700}>
                                            {stat.nombre.toUpperCase()}
                                        </Typography>
                                        <Typography variant="h4" fontWeight={700}>${stat.precioMensual}/mes</Typography>
                                    </Box>
                                    <CardContent>
                                        <Box sx={{ mb: 3 }}>
                                            <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                                                <Typography variant="body1" color="text.secondary">Empresas en este plan</Typography>
                                                <Typography variant="h5" fontWeight={700} color={stat.colorHex}>{stat.totalEmpresas}</Typography>
                                            </Box>
                                            <Typography variant="caption" color="text.secondary">
                                                {stat.porcentajeDelTotal}% del total
                                            </Typography>
                                        </Box>

                                        <Box sx={{ mb: 2 }}>
                                            <Typography variant="body2" color="text.secondary" gutterBottom>
                                                Uso promedio de vehículos (máx {stat.maxVehiculos})
                                            </Typography>
                                            <LinearProgress
                                                variant="determinate"
                                                value={Math.min(stat.promedioUsoVehiculos, 100)}
                                                sx={{
                                                    height: 12,
                                                    borderRadius: 6,
                                                    bgcolor: alpha(stat.colorHex, 0.2),
                                                    '& .MuiLinearProgress-bar': { bgcolor: stat.colorHex, borderRadius: 6 },
                                                }}
                                            />
                                            <Typography variant="body1" fontWeight={600} sx={{ mt: 0.5 }}>
                                                {stat.promedioUsoVehiculos.toFixed(1)}%
                                            </Typography>
                                        </Box>

                                        {stat.empresasCercaDelLimite > 0 && (
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, p: 1.5, bgcolor: 'warning.light', borderRadius: 2 }}>
                                                <Warning color="warning" />
                                                <Typography variant="body2" fontWeight={600}>
                                                    {stat.empresasCercaDelLimite} empresas cerca del límite
                                                </Typography>
                                            </Box>
                                        )}
                                    </CardContent>
                                </Card>
                            </Grid>
                        ))}
                    </Grid>

                    {/* Plan Comparison Table */}
                    <Paper sx={{ mt: 4, borderRadius: 3, overflow: 'hidden' }}>
                        <Box sx={{ p: 2, bgcolor: 'secondary.main', color: 'white' }}>
                            <Typography variant="h6" fontWeight={600}>📋 Comparativa de Planes</Typography>
                        </Box>
                        <TableContainer>
                            <Table>
                                <TableHead>
                                    <TableRow>
                                        <TableCell><strong>Plan</strong></TableCell>
                                        <TableCell align="center"><strong>Precio</strong></TableCell>
                                        <TableCell align="center"><strong>Máx. Usuarios</strong></TableCell>
                                        <TableCell align="center"><strong>Máx. Vehículos</strong></TableCell>
                                        <TableCell><strong>Descripción</strong></TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {plans.map((plan) => (
                                        <TableRow key={plan.id} hover>
                                            <TableCell>
                                                <Chip label={plan.nombre} sx={{ bgcolor: plan.colorHex, color: 'white', fontWeight: 700 }} />
                                            </TableCell>
                                            <TableCell align="center">
                                                <Typography variant="h6" fontWeight={700}>${plan.precioMensual}</Typography>
                                            </TableCell>
                                            <TableCell align="center">{plan.maxUsuarios}</TableCell>
                                            <TableCell align="center">{plan.maxVehiculos}</TableCell>
                                            <TableCell>{plan.descripcion}</TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    </Paper>
                </TabPanel>

                {/* ==================== TAB 2: EMPRESAS ==================== */}
                <TabPanel value={activeTab} index={2}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                        <Typography variant="h5" fontWeight={700}>
                            🏢 Gestión de Empresas
                        </Typography>
                        <Box sx={{ display: 'flex', gap: 2 }}>
                            <Button
                                variant="contained"
                                startIcon={<Add />}
                                onClick={() => { resetForm(); setCreateDialog(true); }}
                            >
                                Nueva Empresa
                            </Button>
                            <Button
                                variant="outlined"
                                startIcon={<Download />}
                                onClick={handleExport}
                            >
                                Exportar CSV
                            </Button>
                        </Box>
                    </Box>

                    <Paper sx={{ borderRadius: 3, overflow: 'hidden' }}>
                        {/* Filters */}
                        <Box sx={{ p: 2, bgcolor: 'background.paper', borderBottom: 1, borderColor: 'divider' }}>
                            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                                <TextField
                                    size="small"
                                    placeholder="Buscar por nombre, RUC o email..."
                                    value={search}
                                    onChange={(e) => { setSearch(e.target.value); setPage(1); }}
                                    sx={{ minWidth: 300 }}
                                    InputProps={{
                                        startAdornment: (
                                            <InputAdornment position="start">
                                                <Search />
                                            </InputAdornment>
                                        ),
                                    }}
                                />
                                <FormControl size="small" sx={{ minWidth: 150 }}>
                                    <InputLabel>Plan</InputLabel>
                                    <Select
                                        value={filterPlan}
                                        label="Plan"
                                        onChange={(e) => { setFilterPlan(e.target.value as number | ''); setPage(1); }}
                                    >
                                        <MenuItem value="">Todos</MenuItem>
                                        {plans.map((plan) => (
                                            <MenuItem key={plan.id} value={plan.id}>{plan.nombre}</MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>
                                <FormControl size="small" sx={{ minWidth: 150 }}>
                                    <InputLabel>Estado</InputLabel>
                                    <Select
                                        value={filterStatus}
                                        label="Estado"
                                        onChange={(e) => { setFilterStatus(e.target.value as boolean | ''); setPage(1); }}
                                    >
                                        <MenuItem value="">Todos</MenuItem>
                                        <MenuItem value="true">Activas</MenuItem>
                                        <MenuItem value="false">Inactivas</MenuItem>
                                    </Select>
                                </FormControl>
                            </Box>
                        </Box>

                        {/* Table */}
                        <TableContainer>
                            <Table>
                                <TableHead>
                                    <TableRow sx={{ bgcolor: 'action.hover' }}>
                                        <TableCell><strong>Empresa</strong></TableCell>
                                        <TableCell><strong>Plan</strong></TableCell>
                                        <TableCell align="center"><strong>Usuarios</strong></TableCell>
                                        <TableCell align="center"><strong>Vehículos</strong></TableCell>
                                        <TableCell align="center"><strong>Mantenimientos</strong></TableCell>
                                        <TableCell align="center"><strong>Estado</strong></TableCell>
                                        <TableCell><strong>Creación</strong></TableCell>
                                        <TableCell align="center"><strong>Acciones</strong></TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {tenants?.items.map((tenant) => {
                                        const isAtUserLimit = tenant.plan && tenant.totalUsuarios >= tenant.plan.maxUsuarios;
                                        const isAtVehicleLimit = tenant.plan && tenant.totalVehiculos >= tenant.plan.maxVehiculos;

                                        return (
                                            <TableRow key={tenant.id} hover>
                                                <TableCell>
                                                    <Typography fontWeight={600}>{tenant.nombre}</Typography>
                                                    <Typography variant="caption" color="text.secondary">{tenant.ruc}</Typography>
                                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                                                        <Email fontSize="small" color="action" />
                                                        <Typography variant="caption">{tenant.emailContacto}</Typography>
                                                    </Box>
                                                </TableCell>
                                                <TableCell>
                                                    {tenant.plan ? (
                                                        <Chip
                                                            label={`${tenant.plan.nombre} - $${tenant.plan.precioMensual}`}
                                                            size="small"
                                                            sx={{ bgcolor: tenant.plan.colorHex, color: 'white', fontWeight: 600 }}
                                                        />
                                                    ) : (
                                                        <Chip label="Sin Plan" size="small" color="default" />
                                                    )}
                                                </TableCell>
                                                <TableCell align="center">
                                                    <Typography color={isAtUserLimit ? 'error' : 'text.primary'} fontWeight={isAtUserLimit ? 700 : 400}>
                                                        {tenant.totalUsuarios}/{tenant.plan?.maxUsuarios ?? '∞'}
                                                    </Typography>
                                                </TableCell>
                                                <TableCell align="center">
                                                    <Typography color={isAtVehicleLimit ? 'error' : 'text.primary'} fontWeight={isAtVehicleLimit ? 700 : 400}>
                                                        {tenant.totalVehiculos}/{tenant.plan?.maxVehiculos ?? '∞'}
                                                    </Typography>
                                                </TableCell>
                                                <TableCell align="center">{tenant.totalMantenimientos}</TableCell>
                                                <TableCell align="center">
                                                    <Chip
                                                        label={tenant.activo ? 'Activa' : 'Inactiva'}
                                                        color={tenant.activo ? 'success' : 'error'}
                                                        size="small"
                                                        icon={tenant.activo ? <CheckCircle /> : <Cancel />}
                                                    />
                                                </TableCell>
                                                <TableCell>{new Date(tenant.fechaCreacion).toLocaleDateString('es-PE')}</TableCell>
                                                <TableCell align="center">
                                                    <Box sx={{ display: 'flex', gap: 0.5 }}>
                                                        <Tooltip title="Ver detalles">
                                                            <IconButton size="small" onClick={() => setViewDialog({ open: true, tenant })}>
                                                                <Visibility color="info" />
                                                            </IconButton>
                                                        </Tooltip>
                                                        <Tooltip title="Editar">
                                                            <IconButton size="small" onClick={() => openEditDialog(tenant)}>
                                                                <Edit color="primary" />
                                                            </IconButton>
                                                        </Tooltip>
                                                        <Tooltip title={tenant.activo ? 'Desactivar' : 'Activar'}>
                                                            <IconButton size="small" onClick={() => handleToggleStatus(tenant.id)} color={tenant.activo ? 'error' : 'success'}>
                                                                {tenant.activo ? <Cancel /> : <CheckCircle />}
                                                            </IconButton>
                                                        </Tooltip>
                                                    </Box>
                                                </TableCell>
                                            </TableRow>
                                        );
                                    })}
                                </TableBody>
                            </Table>
                        </TableContainer>

                        {tenants && tenants.totalPages > 1 && (
                            <Box sx={{ display: 'flex', justifyContent: 'center', p: 2 }}>
                                <Pagination count={tenants.totalPages} page={page} onChange={(_, value) => setPage(value)} color="primary" />
                            </Box>
                        )}
                    </Paper>
                </TabPanel>
            </Box>

            {/* ==================== DIALOGS ==================== */}

            {/* Create Dialog */}
            <Dialog open={createDialog} onClose={() => setCreateDialog(false)} maxWidth="sm" fullWidth>
                <DialogTitle sx={{ bgcolor: 'primary.main', color: 'white' }}>
                    <Add sx={{ mr: 1, verticalAlign: 'middle' }} />
                    Nueva Empresa
                </DialogTitle>
                <DialogContent sx={{ mt: 2 }}>
                    <Grid container spacing={2} sx={{ mt: 1 }}>
                        <Grid size={{ xs: 12 }}>
                            <TextField fullWidth label="Nombre de la Empresa" value={formData.nombre} onChange={(e) => setFormData({ ...formData, nombre: e.target.value })} required />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField fullWidth label="RUC" value={formData.ruc} onChange={(e) => setFormData({ ...formData, ruc: e.target.value })} required />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField fullWidth label="Email de Contacto" type="email" value={formData.emailContacto} onChange={(e) => setFormData({ ...formData, emailContacto: e.target.value })} required />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField fullWidth label="Teléfono" value={formData.telefono} onChange={(e) => setFormData({ ...formData, telefono: e.target.value })} />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <FormControl fullWidth>
                                <InputLabel>Plan de Suscripción</InputLabel>
                                <Select value={formData.planId} label="Plan de Suscripción" onChange={(e) => setFormData({ ...formData, planId: e.target.value as number })}>
                                    {plans.map((plan) => (
                                        <MenuItem key={plan.id} value={plan.id}>
                                            {plan.nombre} - ${plan.precioMensual}/mes
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </Grid>
                        <Grid size={{ xs: 12 }}>
                            <TextField fullWidth label="Dirección" value={formData.direccion} onChange={(e) => setFormData({ ...formData, direccion: e.target.value })} multiline rows={2} />
                        </Grid>
                    </Grid>
                </DialogContent>
                <DialogActions sx={{ p: 2 }}>
                    <Button onClick={() => setCreateDialog(false)}>Cancelar</Button>
                    <Button variant="contained" onClick={handleCreateTenant}>Crear Empresa</Button>
                </DialogActions>
            </Dialog>

            {/* Edit Dialog */}
            <Dialog open={editDialog.open} onClose={() => setEditDialog({ open: false, tenant: null })} maxWidth="sm" fullWidth>
                <DialogTitle sx={{ bgcolor: 'info.main', color: 'white' }}>
                    <Edit sx={{ mr: 1, verticalAlign: 'middle' }} />
                    Editar Empresa
                </DialogTitle>
                <DialogContent sx={{ mt: 2 }}>
                    <Grid container spacing={2} sx={{ mt: 1 }}>
                        <Grid size={{ xs: 12 }}>
                            <TextField fullWidth label="Nombre de la Empresa" value={formData.nombre} onChange={(e) => setFormData({ ...formData, nombre: e.target.value })} required />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField fullWidth label="RUC" value={formData.ruc} onChange={(e) => setFormData({ ...formData, ruc: e.target.value })} required />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField fullWidth label="Email de Contacto" type="email" value={formData.emailContacto} onChange={(e) => setFormData({ ...formData, emailContacto: e.target.value })} required />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField fullWidth label="Teléfono" value={formData.telefono} onChange={(e) => setFormData({ ...formData, telefono: e.target.value })} />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <FormControl fullWidth>
                                <InputLabel>Plan de Suscripción</InputLabel>
                                <Select value={formData.planId} label="Plan de Suscripción" onChange={(e) => setFormData({ ...formData, planId: e.target.value as number })}>
                                    {plans.map((plan) => (
                                        <MenuItem key={plan.id} value={plan.id}>
                                            {plan.nombre} - ${plan.precioMensual}/mes
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </Grid>
                        <Grid size={{ xs: 12 }}>
                            <TextField fullWidth label="Dirección" value={formData.direccion} onChange={(e) => setFormData({ ...formData, direccion: e.target.value })} multiline rows={2} />
                        </Grid>
                    </Grid>
                </DialogContent>
                <DialogActions sx={{ p: 2 }}>
                    <Button onClick={() => setEditDialog({ open: false, tenant: null })}>Cancelar</Button>
                    <Button variant="contained" color="primary" onClick={handleUpdateTenant}>Guardar Cambios</Button>
                </DialogActions>
            </Dialog>

            {/* View Dialog */}
            <Dialog open={viewDialog.open} onClose={() => setViewDialog({ open: false, tenant: null })} maxWidth="sm" fullWidth>
                <DialogTitle sx={{ bgcolor: 'info.main', color: 'white' }}>
                    <Visibility sx={{ mr: 1, verticalAlign: 'middle' }} />
                    Detalles de Empresa
                </DialogTitle>
                <DialogContent sx={{ mt: 2 }}>
                    {viewDialog.tenant && (
                        <Box>
                            <Typography variant="h5" fontWeight={700} gutterBottom>{viewDialog.tenant.nombre}</Typography>
                            <Grid container spacing={2}>
                                <Grid size={{ xs: 6 }}><Typography color="text.secondary">RUC:</Typography><Typography fontWeight={600}>{viewDialog.tenant.ruc}</Typography></Grid>
                                <Grid size={{ xs: 6 }}><Typography color="text.secondary">Email:</Typography><Typography fontWeight={600}>{viewDialog.tenant.emailContacto}</Typography></Grid>
                                <Grid size={{ xs: 6 }}><Typography color="text.secondary">Teléfono:</Typography><Typography fontWeight={600}>{viewDialog.tenant.telefono || 'N/A'}</Typography></Grid>
                                <Grid size={{ xs: 6 }}><Typography color="text.secondary">Plan:</Typography><Chip label={viewDialog.tenant.plan?.nombre} sx={{ bgcolor: viewDialog.tenant.plan?.colorHex, color: 'white' }} /></Grid>
                                <Grid size={{ xs: 4 }}><Typography color="text.secondary">Usuarios:</Typography><Typography variant="h6" fontWeight={700}>{viewDialog.tenant.totalUsuarios}</Typography></Grid>
                                <Grid size={{ xs: 4 }}><Typography color="text.secondary">Vehículos:</Typography><Typography variant="h6" fontWeight={700}>{viewDialog.tenant.totalVehiculos}</Typography></Grid>
                                <Grid size={{ xs: 4 }}><Typography color="text.secondary">Mantenimientos:</Typography><Typography variant="h6" fontWeight={700}>{viewDialog.tenant.totalMantenimientos}</Typography></Grid>
                                <Grid size={{ xs: 6 }}><Typography color="text.secondary">Estado:</Typography><Chip label={viewDialog.tenant.activo ? 'Activa' : 'Inactiva'} color={viewDialog.tenant.activo ? 'success' : 'error'} /></Grid>
                                <Grid size={{ xs: 6 }}><Typography color="text.secondary">Creación:</Typography><Typography fontWeight={600}>{new Date(viewDialog.tenant.fechaCreacion).toLocaleDateString('es-PE')}</Typography></Grid>
                            </Grid>
                        </Box>
                    )}
                </DialogContent>
                <DialogActions sx={{ p: 2 }}>
                    <Button onClick={() => setViewDialog({ open: false, tenant: null })}>Cerrar</Button>
                </DialogActions>
            </Dialog>

            {/* Delete Confirmation Dialog */}
            <Dialog open={deleteDialog.open} onClose={() => setDeleteDialog({ open: false, tenantId: null })}>
                <DialogTitle>⚠️ Confirmar Desactivación</DialogTitle>
                <DialogContent>
                    <Typography>¿Estás seguro de que deseas desactivar esta empresa? Los usuarios no podrán acceder al sistema.</Typography>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setDeleteDialog({ open: false, tenantId: null })}>Cancelar</Button>
                    <Button variant="contained" color="error" onClick={handleDeleteTenant}>Desactivar</Button>
                </DialogActions>
            </Dialog>

            {/* Snackbar */}
            <Snackbar
                open={snackbar.open}
                autoHideDuration={4000}
                onClose={() => setSnackbar({ ...snackbar, open: false })}
                anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
            >
                <Alert severity={snackbar.severity} onClose={() => setSnackbar({ ...snackbar, open: false })}>
                    {snackbar.message}
                </Alert>
            </Snackbar>
        </Box>
    );
};

export default SuperAdminPage;
