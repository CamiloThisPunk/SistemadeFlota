import React, { useState } from 'react';
import {
    Box,
    Paper,
    Typography,
    Button,
    TextField,
    InputAdornment,
    Chip,
    IconButton,
    Avatar,
    Alert,
    LinearProgress,
    Card,
    CardContent,
    Tooltip,
    Skeleton,
    useTheme,
    alpha,
} from '@mui/material';
import Grid from '@mui/material/Grid2';
import { DataGrid, GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import {
    Add,
    Search,
    Edit,
    Delete,
    Visibility,
    Badge,
    Warning,
    People,
    CheckCircle,
    AccessTime,
    FilterList,
    Person,
} from '@mui/icons-material';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { getDrivers, getDriverAlerts, DriverAlert, Driver } from '../services/api';
import DriverFormDialog from '../components/DriverFormDialog';
import { useAuth } from '../contexts/AuthContext';
import { useModuleAccess, AccessDeniedPage } from '../components/AccessControl';

const DriversPage: React.FC = () => {
    const { t } = useTranslation();
    const theme = useTheme();
    const { canRead } = useAuth();
    const { canCreate, canEdit, canRemove } = useModuleAccess('drivers');
    const [search, setSearch] = useState('');
    const [openForm, setOpenForm] = useState(false);
    const [selectedDriver, setSelectedDriver] = useState<Driver | null>(null);
    const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 10 });

    // Verificar acceso de lectura
    if (!canRead('drivers')) {
        return <AccessDeniedPage title="Acceso Denegado" message="No tienes permiso para ver los conductores." />;
    }

    // Queries
    const { data, isLoading, error } = useQuery({
        queryKey: ['drivers', paginationModel.page + 1, paginationModel.pageSize],
        queryFn: () => getDrivers(paginationModel.page + 1, paginationModel.pageSize),
    });

    const { data: alertsData } = useQuery({
        queryKey: ['driverAlerts'],
        queryFn: () => getDriverAlerts(30),
    });

    const handleOpenCreate = () => {
        setSelectedDriver(null);
        setOpenForm(true);
    };

    const handleOpenEdit = (driver: Driver) => {
        setSelectedDriver(driver);
        setOpenForm(true);
    };

    const getScoringColor = (scoring: number) => {
        if (scoring >= 80) return 'success.main';
        if (scoring >= 60) return 'warning.main';
        return 'error.main';
    };

    const getLicenseAlertDays = (driverId: number): number | null => {
        const alerts = alertsData?.data?.data || [];
        const alert = alerts.find((a: DriverAlert) => a.driverId === driverId);
        return alert?.diasParaVencimiento ?? null;
    };

    const columns: GridColDef[] = [
        {
            field: 'nombre',
            headerName: 'Conductor',
            flex: 1.5,
            minWidth: 250,
            renderCell: (params: GridRenderCellParams) => (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                    <Avatar 
                        sx={{ 
                            width: 42, 
                            height: 42, 
                            background: `linear-gradient(135deg, ${theme.palette.primary.main} 0%, ${theme.palette.secondary.main} 100%)`,
                            fontSize: 16,
                            fontWeight: 700,
                        }}
                    >
                        {params.row.nombres?.[0]}{params.row.apellidos?.[0]}
                    </Avatar>
                    <Box>
                        <Typography fontWeight={600}>
                            {params.row.nombres} {params.row.apellidos}
                        </Typography>
                        <Typography variant="caption" color="text.secondary" sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                            <Badge sx={{ fontSize: 12 }} /> {params.row.documento}
                        </Typography>
                    </Box>
                </Box>
            ),
        },
        {
            field: 'licencia',
            headerName: 'Licencia',
            flex: 1,
            minWidth: 130,
            renderCell: (params: GridRenderCellParams) => (
                <Typography fontWeight={600} sx={{ fontFamily: 'monospace', letterSpacing: 1 }}>
                    {params.value}
                </Typography>
            ),
        },
        {
            field: 'categoriaLicencia',
            headerName: 'Categoría',
            width: 110,
            align: 'center',
            headerAlign: 'center',
            renderCell: (params: GridRenderCellParams) => (
                <Chip 
                    label={params.value || 'N/A'} 
                    size="small" 
                    sx={{
                        fontWeight: 700,
                        bgcolor: alpha(theme.palette.primary.main, 0.1),
                        color: 'primary.main',
                        border: `1px solid ${alpha(theme.palette.primary.main, 0.3)}`,
                    }}
                />
            ),
        },
        {
            field: 'fechaVencimientoLicencia',
            headerName: 'Vence',
            flex: 1,
            minWidth: 160,
            renderCell: (params: GridRenderCellParams) => {
                const alertDays = getLicenseAlertDays(params.row.id);
                const fecha = params.value ? new Date(params.value).toLocaleDateString('es-PE') : '-';
                const isExpiringSoon = alertDays !== null && alertDays <= 30;
                const isExpired = alertDays !== null && alertDays <= 0;

                return (
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <AccessTime sx={{ 
                            fontSize: 18, 
                            color: isExpired ? 'error.main' : isExpiringSoon ? 'warning.main' : 'text.secondary' 
                        }} />
                        <Box>
                            <Typography fontSize={14}>{fecha}</Typography>
                            {isExpiringSoon && (
                                <Typography variant="caption" color={isExpired ? 'error.main' : 'warning.main'} fontWeight={600}>
                                    {isExpired ? '¡Expirada!' : `${alertDays} días`}
                                </Typography>
                            )}
                        </Box>
                    </Box>
                );
            },
        },
        {
            field: 'scoring',
            headerName: 'Scoring',
            flex: 1,
            minWidth: 150,
            renderCell: (params: GridRenderCellParams) => {
                const value = params.value || 85;
                return (
                    <Box sx={{ width: '100%', pr: 2 }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                            <Typography variant="body2" fontWeight={700} color={getScoringColor(value)}>
                                {value}%
                            </Typography>
                        </Box>
                        <LinearProgress
                            variant="determinate"
                            value={value}
                            sx={{
                                height: 8,
                                borderRadius: 4,
                                bgcolor: alpha(theme.palette.divider, 0.3),
                                '& .MuiLinearProgress-bar': {
                                    bgcolor: getScoringColor(value),
                                    borderRadius: 4,
                                },
                            }}
                        />
                    </Box>
                );
            },
        },
        {
            field: 'activo',
            headerName: 'Estado',
            width: 120,
            align: 'center',
            headerAlign: 'center',
            renderCell: (params: GridRenderCellParams) => (
                <Chip
                    icon={params.value ? <CheckCircle sx={{ fontSize: 16 }} /> : undefined}
                    label={params.value !== false ? 'Activo' : 'Inactivo'}
                    size="small"
                    color={params.value !== false ? 'success' : 'default'}
                    sx={{ fontWeight: 600 }}
                />
            ),
        },
        {
            field: 'actions',
            headerName: 'Acciones',
            width: 150,
            sortable: false,
            align: 'center',
            headerAlign: 'center',
            renderCell: (params: GridRenderCellParams) => (
                <Box sx={{ display: 'flex', gap: 0.5 }}>
                    <Tooltip title="Ver detalles" arrow>
                        <IconButton
                            size="small"
                            sx={{
                                color: 'info.main',
                                bgcolor: alpha(theme.palette.info.main, 0.1),
                                '&:hover': { bgcolor: alpha(theme.palette.info.main, 0.2) },
                            }}
                        >
                            <Visibility fontSize="small" />
                        </IconButton>
                    </Tooltip>
                    {canEdit && (
                        <Tooltip title="Editar" arrow>
                            <IconButton
                                size="small"
                                onClick={() => handleOpenEdit(params.row)}
                                sx={{
                                    color: 'primary.main',
                                    bgcolor: alpha(theme.palette.primary.main, 0.1),
                                    '&:hover': { bgcolor: alpha(theme.palette.primary.main, 0.2) },
                                }}
                            >
                                <Edit fontSize="small" />
                            </IconButton>
                        </Tooltip>
                    )}
                    {canRemove && (
                        <Tooltip title="Eliminar" arrow>
                            <IconButton
                                size="small"
                                sx={{
                                    color: 'error.main',
                                    bgcolor: alpha(theme.palette.error.main, 0.1),
                                    '&:hover': { bgcolor: alpha(theme.palette.error.main, 0.2) },
                                }}
                            >
                                <Delete fontSize="small" />
                            </IconButton>
                        </Tooltip>
                    )}
                </Box>
            ),
        },
    ];

    // Extraer datos
    const responseData = data?.data;
    const drivers = responseData?.items || responseData?.data?.items || [];
    const totalCount = responseData?.totalCount || responseData?.data?.totalCount || drivers.length;
    const alerts = alertsData?.data?.data || alertsData?.data || [];

    // Stats
    const stats = {
        total: drivers.length,
        activos: drivers.filter((d: Driver) => d.activo !== false).length,
        alertasLicencia: Array.isArray(alerts) ? alerts.filter((a: DriverAlert) => a.diasParaVencimiento <= 30).length : 0,
    };

    return (
        <Box>
            {/* Header con gradiente */}
            <Paper
                elevation={0}
                sx={{
                    p: 3,
                    mb: 3,
                    background: `linear-gradient(135deg, ${alpha(theme.palette.secondary.main, 0.1)} 0%, ${alpha(theme.palette.primary.main, 0.05)} 100%)`,
                    borderRadius: 3,
                    border: `1px solid ${alpha(theme.palette.secondary.main, 0.1)}`,
                }}
            >
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
                    <Box>
                        <Typography variant="h4" fontWeight={800} sx={{ 
                            background: `linear-gradient(135deg, ${theme.palette.secondary.main} 0%, ${theme.palette.primary.main} 100%)`,
                            backgroundClip: 'text',
                            WebkitBackgroundClip: 'text',
                            WebkitTextFillColor: 'transparent',
                        }}>
                            👨‍✈️ Gestión de Conductores
                        </Typography>
                        <Typography color="text.secondary" sx={{ mt: 0.5 }}>
                            Administra los conductores de tu flota
                        </Typography>
                    </Box>
                    {canCreate && (
                        <Button
                            variant="contained"
                            startIcon={<Add />}
                            onClick={handleOpenCreate}
                            size="large"
                            sx={{
                                background: 'linear-gradient(135deg, #6c5ce7 0%, #a55eea 100%)',
                                px: 4,
                                py: 1.5,
                                borderRadius: 2,
                                fontWeight: 700,
                                boxShadow: '0 4px 14px rgba(108, 92, 231, 0.4)',
                                '&:hover': {
                                    boxShadow: '0 6px 20px rgba(108, 92, 231, 0.5)',
                                    transform: 'translateY(-2px)',
                                },
                                transition: 'all 0.2s ease',
                            }}
                        >
                            Agregar Conductor
                        </Button>
                    )}
                </Box>
            </Paper>

            {/* Stats Cards */}
            <Grid container spacing={2} sx={{ mb: 3 }}>
                <Grid size={{ xs: 12, sm: 4 }}>
                    <Card sx={{ 
                        background: `linear-gradient(135deg, ${alpha('#6c5ce7', 0.15)} 0%, ${alpha('#6c5ce7', 0.05)} 100%)`,
                        border: `1px solid ${alpha('#6c5ce7', 0.2)}`,
                    }}>
                        <CardContent sx={{ py: 2 }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                                <Box>
                                    <Typography color="text.secondary" variant="body2">Total Conductores</Typography>
                                    <Typography variant="h4" fontWeight={700} color="secondary.main">
                                        {isLoading ? <Skeleton width={40} /> : stats.total}
                                    </Typography>
                                </Box>
                                <People sx={{ fontSize: 40, color: alpha('#6c5ce7', 0.5) }} />
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>
                <Grid size={{ xs: 12, sm: 4 }}>
                    <Card sx={{ 
                        background: `linear-gradient(135deg, ${alpha('#00d4aa', 0.15)} 0%, ${alpha('#00d4aa', 0.05)} 100%)`,
                        border: `1px solid ${alpha('#00d4aa', 0.2)}`,
                    }}>
                        <CardContent sx={{ py: 2 }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                                <Box>
                                    <Typography color="text.secondary" variant="body2">Activos</Typography>
                                    <Typography variant="h4" fontWeight={700} color="primary.main">
                                        {isLoading ? <Skeleton width={40} /> : stats.activos}
                                    </Typography>
                                </Box>
                                <CheckCircle sx={{ fontSize: 40, color: alpha('#00d4aa', 0.5) }} />
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>
                <Grid size={{ xs: 12, sm: 4 }}>
                    <Card sx={{ 
                        background: `linear-gradient(135deg, ${alpha('#ff9800', 0.15)} 0%, ${alpha('#ff9800', 0.05)} 100%)`,
                        border: `1px solid ${alpha('#ff9800', 0.2)}`,
                    }}>
                        <CardContent sx={{ py: 2 }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                                <Box>
                                    <Typography color="text.secondary" variant="body2">Licencias por Vencer</Typography>
                                    <Typography variant="h4" fontWeight={700} color="warning.main">
                                        {isLoading ? <Skeleton width={40} /> : stats.alertasLicencia}
                                    </Typography>
                                </Box>
                                <Warning sx={{ fontSize: 40, color: alpha('#ff9800', 0.5) }} />
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>
            </Grid>

            {error && (
                <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }}>
                    Error al cargar los conductores. Por favor, intenta de nuevo.
                </Alert>
            )}

            {/* Search & Filters */}
            <Paper sx={{ p: 2, mb: 3, borderRadius: 2 }}>
                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
                    <TextField
                        placeholder="Buscar por nombre, documento, licencia..."
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        size="small"
                        sx={{ 
                            minWidth: 350,
                            '& .MuiOutlinedInput-root': { borderRadius: 2 },
                        }}
                        InputProps={{
                            startAdornment: (
                                <InputAdornment position="start">
                                    <Search color="action" />
                                </InputAdornment>
                            ),
                        }}
                    />
                    <Button variant="outlined" startIcon={<FilterList />} sx={{ borderRadius: 2 }}>
                        Filtros
                    </Button>
                </Box>
            </Paper>

            {/* DataGrid */}
            <Paper sx={{ borderRadius: 3, overflow: 'hidden' }}>
                <DataGrid
                    rows={drivers}
                    columns={columns}
                    loading={isLoading}
                    pageSizeOptions={[10, 25, 50]}
                    paginationModel={paginationModel}
                    onPaginationModelChange={setPaginationModel}
                    paginationMode="server"
                    rowCount={totalCount}
                    disableRowSelectionOnClick
                    autoHeight
                    rowHeight={70}
                    getRowId={(row) => row.id}
                    sx={{
                        border: 'none',
                        '& .MuiDataGrid-cell': { 
                            borderColor: alpha(theme.palette.divider, 0.5),
                            py: 1,
                        },
                        '& .MuiDataGrid-columnHeaders': { 
                            bgcolor: alpha(theme.palette.secondary.main, 0.05),
                            borderBottom: `2px solid ${alpha(theme.palette.secondary.main, 0.2)}`,
                        },
                        '& .MuiDataGrid-columnHeaderTitle': {
                            fontWeight: 700,
                        },
                        '& .MuiDataGrid-row:hover': {
                            bgcolor: alpha(theme.palette.secondary.main, 0.04),
                        },
                    }}
                    localeText={{
                        noRowsLabel: 'No hay conductores registrados',
                        MuiTablePagination: {
                            labelRowsPerPage: 'Filas por página:',
                        },
                    }}
                />
            </Paper>

            {/* Driver Form Dialog */}
            <DriverFormDialog
                open={openForm}
                onClose={() => setOpenForm(false)}
                driver={selectedDriver}
            />
        </Box>
    );
};

export default DriversPage;
