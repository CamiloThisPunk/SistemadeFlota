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
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Alert,
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
    Speed,
    DirectionsCar,
    LocalGasStation,
    Build,
    CheckCircle,
    Warning,
    Error as ErrorIcon,
    FilterList,
} from '@mui/icons-material';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { getVehicles, updateVehicleKm, deleteVehicle, Vehicle } from '../services/api';
import VehicleFormDialog from '../components/VehicleFormDialog';
import { useAuth } from '../contexts/AuthContext';
import { PermissionGate, useModuleAccess, AccessDeniedPage } from '../components/AccessControl';

const VehiclesPage: React.FC = () => {
    const { t } = useTranslation();
    const theme = useTheme();
    const queryClient = useQueryClient();
    const { canRead } = useAuth();
    const { canCreate, canEdit, canRemove } = useModuleAccess('vehicles');
    const [search, setSearch] = useState('');
    const [openForm, setOpenForm] = useState(false);
    const [openKm, setOpenKm] = useState(false);
    const [openDelete, setOpenDelete] = useState(false);
    const [selectedVehicle, setSelectedVehicle] = useState<Vehicle | null>(null);
    const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 10 });
    const [nuevoKm, setNuevoKm] = useState(0);

    // Verificar acceso de lectura
    if (!canRead('vehicles')) {
        return <AccessDeniedPage title="Acceso Denegado" message="No tienes permiso para ver los vehículos." />;
    }

    // Query
    const { data, isLoading, error } = useQuery({
        queryKey: ['vehicles', paginationModel.page + 1, paginationModel.pageSize],
        queryFn: () => getVehicles(paginationModel.page + 1, paginationModel.pageSize),
    });

    // Update Km Mutation
    const updateKmMutation = useMutation({
        mutationFn: ({ id, km }: { id: number; km: number }) => updateVehicleKm(id, km),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['vehicles'] });
            setOpenKm(false);
        },
    });

    // Delete Vehicle Mutation
    const deleteMutation = useMutation({
        mutationFn: (id: number) => deleteVehicle(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['vehicles'] });
            setOpenDelete(false);
            setSelectedVehicle(null);
        },
    });

    const handleUpdateKm = () => {
        if (selectedVehicle) {
            updateKmMutation.mutate({ id: selectedVehicle.id, km: nuevoKm });
        }
    };

    const handleOpenCreate = () => {
        setSelectedVehicle(null);
        setOpenForm(true);
    };

    const handleOpenEdit = (vehicle: Vehicle) => {
        setSelectedVehicle(vehicle);
        setOpenForm(true);
    };

    const handleOpenKm = (vehicle: Vehicle) => {
        setSelectedVehicle(vehicle);
        setNuevoKm(vehicle.kmActual);
        setOpenKm(true);
    };

    const handleOpenDelete = (vehicle: Vehicle) => {
        setSelectedVehicle(vehicle);
        setOpenDelete(true);
    };

    const handleConfirmDelete = () => {
        if (selectedVehicle) {
            deleteMutation.mutate(selectedVehicle.id);
        }
    };

    const getEstadoInfo = (estado: string) => {
        switch (estado) {
            case 'Operativo':
                return { color: 'success' as const, icon: <CheckCircle fontSize="small" />, label: 'Activo' };
            case 'EnMantenimiento':
                return { color: 'warning' as const, icon: <Build fontSize="small" />, label: 'En Mantención' };
            case 'FueraDeServicio':
                return { color: 'error' as const, icon: <ErrorIcon fontSize="small" />, label: 'Fuera de Servicio' };
            default:
                return { color: 'default' as const, icon: <CheckCircle fontSize="small" />, label: estado || 'Activo' };
        }
    };

    const getFuelIcon = (tipo: string) => {
        switch (tipo?.toLowerCase()) {
            case 'diesel':
                return <LocalGasStation sx={{ color: '#ff9800', fontSize: 18 }} />;
            case 'gasolina':
                return <LocalGasStation sx={{ color: '#4caf50', fontSize: 18 }} />;
            case 'electrico':
                return <LocalGasStation sx={{ color: '#2196f3', fontSize: 18 }} />;
            default:
                return <LocalGasStation sx={{ color: '#9e9e9e', fontSize: 18 }} />;
        }
    };

    const columns: GridColDef[] = [
        {
            field: 'placa',
            headerName: 'Placa',
            flex: 1,
            minWidth: 130,
            renderCell: (params: GridRenderCellParams) => (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                    <Box
                        sx={{
                            width: 40,
                            height: 40,
                            borderRadius: 2,
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            background: `linear-gradient(135deg, ${alpha(theme.palette.primary.main, 0.2)} 0%, ${alpha(theme.palette.primary.main, 0.1)} 100%)`,
                        }}
                    >
                        <DirectionsCar sx={{ fontSize: 22, color: 'primary.main' }} />
                    </Box>
                    <Typography fontWeight={700} sx={{ letterSpacing: 1 }}>
                        {params.value}
                    </Typography>
                </Box>
            ),
        },
        {
            field: 'marca',
            headerName: 'Marca',
            flex: 1,
            minWidth: 120,
            renderCell: (params: GridRenderCellParams) => (
                <Typography fontWeight={500} color="text.secondary">
                    {params.value}
                </Typography>
            ),
        },
        {
            field: 'modelo',
            headerName: 'Modelo',
            flex: 1,
            minWidth: 130,
            renderCell: (params: GridRenderCellParams) => (
                <Typography fontWeight={600}>
                    {params.value}
                </Typography>
            ),
        },
        {
            field: 'año',
            headerName: 'Año',
            width: 90,
            align: 'center',
            headerAlign: 'center',
            renderCell: (params: GridRenderCellParams) => (
                <Chip
                    label={params.value || '-'}
                    size="small"
                    variant="outlined"
                    sx={{ fontWeight: 600, minWidth: 60 }}
                />
            ),
        },
        {
            field: 'tipoCombustible',
            headerName: 'Combustible',
            flex: 1,
            minWidth: 130,
            renderCell: (params: GridRenderCellParams) => (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    {getFuelIcon(params.value)}
                    <Typography fontSize={14}>
                        {params.value || 'N/A'}
                    </Typography>
                </Box>
            ),
        },
        {
            field: 'kmActual',
            headerName: 'Km Actual',
            flex: 1,
            minWidth: 140,
            renderCell: (params: GridRenderCellParams) => (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Speed sx={{ fontSize: 18, color: 'text.secondary' }} />
                    <Typography fontWeight={600} sx={{ fontFamily: 'monospace' }}>
                        {(params.value || 0).toLocaleString('es-PE')} km
                    </Typography>
                </Box>
            ),
        },
        {
            field: 'estado',
            headerName: 'Estado',
            flex: 1,
            minWidth: 150,
            renderCell: (params: GridRenderCellParams) => {
                const info = getEstadoInfo(params.value);
                return (
                    <Chip
                        icon={info.icon}
                        label={info.label}
                        size="small"
                        color={info.color}
                        sx={{
                            fontWeight: 600,
                            '& .MuiChip-icon': { ml: 0.5 },
                        }}
                    />
                );
            },
        },
        {
            field: 'actions',
            headerName: 'Acciones',
            width: 180,
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
                        <Tooltip title="Actualizar Km" arrow>
                            <IconButton
                                size="small"
                                onClick={() => handleOpenKm(params.row)}
                                sx={{
                                    color: 'secondary.main',
                                    bgcolor: alpha(theme.palette.secondary.main, 0.1),
                                    '&:hover': { bgcolor: alpha(theme.palette.secondary.main, 0.2) },
                                }}
                            >
                                <Speed fontSize="small" />
                            </IconButton>
                        </Tooltip>
                    )}
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
                                onClick={() => handleOpenDelete(params.row)}
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

    // Extraer datos de la respuesta (maneja diferentes estructuras de API)
    const responseData = data?.data;
    const vehicles = responseData?.items || responseData?.data?.items || [];
    const totalCount = responseData?.totalCount || responseData?.data?.totalCount || vehicles.length;

    // Estadísticas rápidas
    const stats = {
        total: vehicles.length,
        operativos: vehicles.filter((v: Vehicle) => v.estado === 'Operativo' || !v.estado).length,
        enMantenimiento: vehicles.filter((v: Vehicle) => v.estado === 'EnMantenimiento').length,
        fueraDeServicio: vehicles.filter((v: Vehicle) => v.estado === 'FueraDeServicio').length,
    };

    return (
        <Box>
            {/* Header con gradiente */}
            <Paper
                elevation={0}
                sx={{
                    p: 3,
                    mb: 3,
                    background: `linear-gradient(135deg, ${alpha(theme.palette.primary.main, 0.1)} 0%, ${alpha(theme.palette.secondary.main, 0.05)} 100%)`,
                    borderRadius: 3,
                    border: `1px solid ${alpha(theme.palette.primary.main, 0.1)}`,
                }}
            >
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 2 }}>
                    <Box>
                        <Typography variant="h4" fontWeight={800} sx={{
                            background: `linear-gradient(135deg, ${theme.palette.primary.main} 0%, ${theme.palette.secondary.main} 100%)`,
                            backgroundClip: 'text',
                            WebkitBackgroundClip: 'text',
                            WebkitTextFillColor: 'transparent',
                        }}>
                            🚗 Gestión de Vehículos
                        </Typography>
                        <Typography color="text.secondary" sx={{ mt: 0.5 }}>
                            Administra y monitorea toda tu flota de vehículos
                        </Typography>
                    </Box>
                    {canCreate && (
                        <Button
                            variant="contained"
                            startIcon={<Add />}
                            onClick={handleOpenCreate}
                            size="large"
                            sx={{
                                background: 'linear-gradient(135deg, #00d4aa 0%, #00b894 100%)',
                                px: 4,
                                py: 1.5,
                                borderRadius: 2,
                                fontWeight: 700,
                                boxShadow: '0 4px 14px rgba(0, 212, 170, 0.4)',
                                '&:hover': {
                                    boxShadow: '0 6px 20px rgba(0, 212, 170, 0.5)',
                                    transform: 'translateY(-2px)',
                                },
                                transition: 'all 0.2s ease',
                            }}
                        >
                            Agregar Nuevo Vehículo
                        </Button>
                    )}
                </Box>
            </Paper>

            {/* Stats Cards */}
            <Grid container spacing={2} sx={{ mb: 3 }}>
                <Grid size={{ xs: 6, sm: 3 }}>
                    <Card sx={{
                        background: `linear-gradient(135deg, ${alpha('#2196f3', 0.15)} 0%, ${alpha('#2196f3', 0.05)} 100%)`,
                        border: `1px solid ${alpha('#2196f3', 0.2)}`,
                    }}>
                        <CardContent sx={{ py: 2 }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                                <Box>
                                    <Typography color="text.secondary" variant="body2">Total Vehículos</Typography>
                                    <Typography variant="h4" fontWeight={700} color="info.main">
                                        {isLoading ? <Skeleton width={40} /> : stats.total}
                                    </Typography>
                                </Box>
                                <DirectionsCar sx={{ fontSize: 40, color: alpha('#2196f3', 0.5) }} />
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>
                <Grid size={{ xs: 6, sm: 3 }}>
                    <Card sx={{
                        background: `linear-gradient(135deg, ${alpha('#4caf50', 0.15)} 0%, ${alpha('#4caf50', 0.05)} 100%)`,
                        border: `1px solid ${alpha('#4caf50', 0.2)}`,
                    }}>
                        <CardContent sx={{ py: 2 }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                                <Box>
                                    <Typography color="text.secondary" variant="body2">Operativos</Typography>
                                    <Typography variant="h4" fontWeight={700} color="success.main">
                                        {isLoading ? <Skeleton width={40} /> : stats.operativos}
                                    </Typography>
                                </Box>
                                <CheckCircle sx={{ fontSize: 40, color: alpha('#4caf50', 0.5) }} />
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>
                <Grid size={{ xs: 6, sm: 3 }}>
                    <Card sx={{
                        background: `linear-gradient(135deg, ${alpha('#ff9800', 0.15)} 0%, ${alpha('#ff9800', 0.05)} 100%)`,
                        border: `1px solid ${alpha('#ff9800', 0.2)}`,
                    }}>
                        <CardContent sx={{ py: 2 }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                                <Box>
                                    <Typography color="text.secondary" variant="body2">En Mantención</Typography>
                                    <Typography variant="h4" fontWeight={700} color="warning.main">
                                        {isLoading ? <Skeleton width={40} /> : stats.enMantenimiento}
                                    </Typography>
                                </Box>
                                <Build sx={{ fontSize: 40, color: alpha('#ff9800', 0.5) }} />
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>
                <Grid size={{ xs: 6, sm: 3 }}>
                    <Card sx={{
                        background: `linear-gradient(135deg, ${alpha('#f44336', 0.15)} 0%, ${alpha('#f44336', 0.05)} 100%)`,
                        border: `1px solid ${alpha('#f44336', 0.2)}`,
                    }}>
                        <CardContent sx={{ py: 2 }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                                <Box>
                                    <Typography color="text.secondary" variant="body2">Fuera de Servicio</Typography>
                                    <Typography variant="h4" fontWeight={700} color="error.main">
                                        {isLoading ? <Skeleton width={40} /> : stats.fueraDeServicio}
                                    </Typography>
                                </Box>
                                <Warning sx={{ fontSize: 40, color: alpha('#f44336', 0.5) }} />
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>
            </Grid>

            {error && (
                <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }}>
                    Error al cargar los vehículos. Por favor, intenta de nuevo.
                </Alert>
            )}

            {/* Search & Filters */}
            <Paper sx={{ p: 2, mb: 3, borderRadius: 2 }}>
                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', alignItems: 'center' }}>
                    <TextField
                        placeholder="Buscar por placa, marca, modelo..."
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        size="small"
                        sx={{
                            minWidth: 300,
                            '& .MuiOutlinedInput-root': {
                                borderRadius: 2,
                            },
                        }}
                        InputProps={{
                            startAdornment: (
                                <InputAdornment position="start">
                                    <Search color="action" />
                                </InputAdornment>
                            ),
                        }}
                    />
                    <Button
                        variant="outlined"
                        startIcon={<FilterList />}
                        sx={{ borderRadius: 2 }}
                    >
                        Filtros
                    </Button>
                </Box>
            </Paper>

            {/* DataGrid */}
            <Paper sx={{ borderRadius: 3, overflow: 'hidden' }}>
                <DataGrid
                    rows={vehicles}
                    columns={columns}
                    loading={isLoading}
                    pageSizeOptions={[10, 25, 50]}
                    paginationModel={paginationModel}
                    onPaginationModelChange={setPaginationModel}
                    paginationMode="server"
                    rowCount={totalCount}
                    disableRowSelectionOnClick
                    autoHeight
                    getRowId={(row) => row.id}
                    sx={{
                        border: 'none',
                        '& .MuiDataGrid-cell': {
                            borderColor: alpha(theme.palette.divider, 0.5),
                            py: 1.5,
                        },
                        '& .MuiDataGrid-columnHeaders': {
                            bgcolor: alpha(theme.palette.primary.main, 0.05),
                            borderBottom: `2px solid ${alpha(theme.palette.primary.main, 0.2)}`,
                        },
                        '& .MuiDataGrid-columnHeaderTitle': {
                            fontWeight: 700,
                            color: 'text.primary',
                        },
                        '& .MuiDataGrid-row:hover': {
                            bgcolor: alpha(theme.palette.primary.main, 0.04),
                        },
                        '& .MuiDataGrid-footerContainer': {
                            borderTop: `1px solid ${theme.palette.divider}`,
                        },
                    }}
                    localeText={{
                        noRowsLabel: 'No hay vehículos registrados',
                        MuiTablePagination: {
                            labelRowsPerPage: 'Filas por página:',
                        },
                    }}
                />
            </Paper>

            {/* Vehicle Form Dialog */}
            <VehicleFormDialog
                open={openForm}
                onClose={() => setOpenForm(false)}
                vehicle={selectedVehicle}
            />

            {/* Update Km Dialog */}
            <Dialog
                open={openKm}
                onClose={() => setOpenKm(false)}
                maxWidth="xs"
                fullWidth
                PaperProps={{
                    sx: { borderRadius: 3 }
                }}
            >
                <DialogTitle sx={{ pb: 1 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                        <Box
                            sx={{
                                width: 40,
                                height: 40,
                                borderRadius: 2,
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                                bgcolor: alpha(theme.palette.secondary.main, 0.1),
                            }}
                        >
                            <Speed sx={{ color: 'secondary.main' }} />
                        </Box>
                        <Typography variant="h6" fontWeight={700}>
                            Actualizar Kilometraje
                        </Typography>
                    </Box>
                </DialogTitle>
                <DialogContent>
                    <Paper
                        elevation={0}
                        sx={{
                            p: 2,
                            mb: 3,
                            mt: 1,
                            bgcolor: alpha(theme.palette.info.main, 0.05),
                            border: `1px solid ${alpha(theme.palette.info.main, 0.1)}`,
                            borderRadius: 2,
                        }}
                    >
                        <Typography color="text.secondary" variant="body2">
                            Vehículo seleccionado
                        </Typography>
                        <Typography variant="h6" fontWeight={700}>
                            {selectedVehicle?.placa} - {selectedVehicle?.marca} {selectedVehicle?.modelo}
                        </Typography>
                    </Paper>
                    <TextField
                        fullWidth
                        type="number"
                        label="Nuevo Kilometraje"
                        value={nuevoKm}
                        onChange={(e) => setNuevoKm(parseInt(e.target.value) || 0)}
                        InputProps={{
                            endAdornment: <InputAdornment position="end">km</InputAdornment>,
                            sx: { fontWeight: 600, fontSize: 18 },
                        }}
                        sx={{
                            '& .MuiOutlinedInput-root': {
                                borderRadius: 2,
                            },
                        }}
                    />
                </DialogContent>
                <DialogActions sx={{ px: 3, pb: 3 }}>
                    <Button
                        onClick={() => setOpenKm(false)}
                        sx={{ borderRadius: 2 }}
                    >
                        Cancelar
                    </Button>
                    <Button
                        variant="contained"
                        onClick={handleUpdateKm}
                        disabled={updateKmMutation.isPending}
                        sx={{
                            borderRadius: 2,
                            px: 3,
                            background: 'linear-gradient(135deg, #6c5ce7 0%, #a55eea 100%)',
                        }}
                    >
                        {updateKmMutation.isPending ? 'Guardando...' : 'Actualizar Km'}
                    </Button>
                </DialogActions>
            </Dialog>

            {/* Delete Confirmation Dialog */}
            <Dialog
                open={openDelete}
                onClose={() => setOpenDelete(false)}
                maxWidth="xs"
                fullWidth
                PaperProps={{
                    sx: { borderRadius: 3 }
                }}
            >
                <DialogTitle sx={{ pb: 1 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                        <Box
                            sx={{
                                width: 40,
                                height: 40,
                                borderRadius: 2,
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                                bgcolor: alpha(theme.palette.error.main, 0.1),
                            }}
                        >
                            <Delete sx={{ color: 'error.main' }} />
                        </Box>
                        <Typography variant="h6" fontWeight={700}>
                            Eliminar Vehículo
                        </Typography>
                    </Box>
                </DialogTitle>
                <DialogContent>
                    <Paper
                        elevation={0}
                        sx={{
                            p: 2,
                            mt: 1,
                            bgcolor: alpha(theme.palette.error.main, 0.05),
                            border: `1px solid ${alpha(theme.palette.error.main, 0.1)}`,
                            borderRadius: 2,
                        }}
                    >
                        <Typography color="text.secondary" variant="body2">
                            ¿Estás seguro que deseas eliminar el vehículo?
                        </Typography>
                        <Typography variant="h6" fontWeight={700} sx={{ mt: 1 }}>
                            {selectedVehicle?.placa} - {selectedVehicle?.marca} {selectedVehicle?.modelo}
                        </Typography>
                    </Paper>
                    <Alert severity="warning" sx={{ mt: 2, borderRadius: 2 }}>
                        Esta acción dará de baja el vehículo del sistema. Esta operación no se puede deshacer.
                    </Alert>
                </DialogContent>
                <DialogActions sx={{ px: 3, pb: 3 }}>
                    <Button
                        onClick={() => setOpenDelete(false)}
                        sx={{ borderRadius: 2 }}
                    >
                        Cancelar
                    </Button>
                    <Button
                        variant="contained"
                        color="error"
                        onClick={handleConfirmDelete}
                        disabled={deleteMutation.isPending}
                        sx={{
                            borderRadius: 2,
                            px: 3,
                        }}
                    >
                        {deleteMutation.isPending ? 'Eliminando...' : 'Eliminar'}
                    </Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
};

export default VehiclesPage;
