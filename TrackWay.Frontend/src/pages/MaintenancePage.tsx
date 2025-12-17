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
    Grid2 as Grid,
    MenuItem,
    Alert,
    Card,
    CardContent,
} from '@mui/material';
import { DataGrid, GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import {
    Add,
    Search,
    Edit,
    Visibility,
    Build,
    Schedule,
    AttachMoney,
} from '@mui/icons-material';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { getMaintenanceOrders, createMaintenanceOrder, getVehicles, MaintenanceOrder } from '../services/api';

const MaintenancePage: React.FC = () => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const [search, setSearch] = useState('');
    const [openCreate, setOpenCreate] = useState(false);
    const [paginationModel, setPaginationModel] = useState({ page: 0, pageSize: 10 });
    const [formError, setFormError] = useState<string | null>(null);

    // Form state
    const [formData, setFormData] = useState({
        vehicleId: 0,
        tipo: 0, // 0: Preventivo, 1: Correctivo
        descripcion: '',
        fechaProgramada: '',
        costoEstimado: 0,
    });

    // Queries
    const { data, isLoading, error } = useQuery({
        queryKey: ['maintenance', paginationModel.page + 1, paginationModel.pageSize],
        queryFn: () => getMaintenanceOrders(paginationModel.page + 1, paginationModel.pageSize),
    });

    const { data: vehiclesData } = useQuery({
        queryKey: ['vehicles-list'],
        queryFn: () => getVehicles(1, 100),
    });

    // Mutations
    const createMutation = useMutation({
        mutationFn: createMaintenanceOrder,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['maintenance'] });
            setOpenCreate(false);
            setFormError(null);
            resetForm();
        },
        onError: (error: any) => {
            console.error('Error creating maintenance order:', error);
            console.error('Response data:', error?.response?.data);
            // Extract error message from various possible locations
            const errorData = error?.response?.data;
            let errorMessage = 'Error al crear la orden de mantenimiento';
            if (typeof errorData === 'string') {
                errorMessage = errorData;
            } else if (errorData?.message) {
                errorMessage = errorData.message;
            } else if (errorData?.errors && errorData.errors.length > 0) {
                errorMessage = errorData.errors.join(', ');
            } else if (errorData?.title) {
                errorMessage = errorData.title + (errorData.detail ? ': ' + errorData.detail : '');
            } else if (error?.message) {
                errorMessage = error.message;
            }
            setFormError(errorMessage);
        },
    });

    const resetForm = () => {
        setFormData({
            vehicleId: 0,
            tipo: 0,
            descripcion: '',
            fechaProgramada: '',
            costoEstimado: 0,
        });
    };

    const handleCreate = () => {
        setFormError(null);
        // Validaciones
        if (!formData.vehicleId || formData.vehicleId === 0) {
            setFormError('Debe seleccionar un vehículo');
            return;
        }
        if (!formData.fechaProgramada) {
            setFormError('Debe seleccionar una fecha programada');
            return;
        }
        if (!formData.descripcion.trim()) {
            setFormError('Debe ingresar una descripción');
            return;
        }
        createMutation.mutate(formData);
    };

    const getEstadoColor = (estado: string) => {
        switch (estado) {
            case 'Completado': return 'success';
            case 'EnProgreso': return 'info';
            case 'Pendiente': return 'warning';
            case 'Cancelado': return 'error';
            default: return 'default';
        }
    };

    const getTipoColor = (tipo: string) => {
        return tipo === 'Preventivo' ? 'primary' : 'secondary';
    };

    const columns: GridColDef[] = [
        {
            field: 'id',
            headerName: 'ID',
            width: 70,
        },
        {
            field: 'vehiculoPlaca',
            headerName: t('maintenance.plate'),
            width: 120,
            renderCell: (params: GridRenderCellParams) => (
                <Chip label={params.value} size="small" variant="outlined" />
            ),
        },
        {
            field: 'tipo',
            headerName: t('maintenance.type'),
            width: 120,
            renderCell: (params: GridRenderCellParams) => (
                <Chip
                    label={params.value === 'Preventivo' ? t('maintenance.preventive') : t('maintenance.corrective')}
                    color={getTipoColor(params.value)}
                    size="small"
                />
            ),
        },
        {
            field: 'estado',
            headerName: t('maintenance.status'),
            width: 130,
            renderCell: (params: GridRenderCellParams) => (
                <Chip
                    label={params.value}
                    color={getEstadoColor(params.value) as any}
                    size="small"
                />
            ),
        },
        {
            field: 'descripcion',
            headerName: t('maintenance.description'),
            flex: 1,
            minWidth: 200,
        },
        {
            field: 'fechaProgramada',
            headerName: t('maintenance.scheduledDate'),
            width: 130,
            valueFormatter: (value: string) => {
                if (!value) return '-';
                return new Date(value).toLocaleDateString('es-PE');
            },
        },
        {
            field: 'costoEstimado',
            headerName: t('maintenance.estimatedCost'),
            width: 120,
            valueFormatter: (value: number) => `S/ ${value?.toFixed(2) || '0.00'}`,
        },
        {
            field: 'costoReal',
            headerName: t('maintenance.actualCost'),
            width: 120,
            valueFormatter: (value: number) => value ? `S/ ${value.toFixed(2)}` : '-',
        },
        {
            field: 'actions',
            headerName: 'Acciones',
            width: 100,
            sortable: false,
            renderCell: () => (
                <Box>
                    <IconButton size="small" color="primary">
                        <Visibility fontSize="small" />
                    </IconButton>
                    <IconButton size="small" color="secondary">
                        <Edit fontSize="small" />
                    </IconButton>
                </Box>
            ),
        },
    ];

    const orders = data?.data?.data?.items || [];
    const totalCount = data?.data?.data?.totalCount || 0;
    const vehicles = vehiclesData?.data?.data?.items || [];

    // Stats
    const pendientes = orders.filter((o: MaintenanceOrder) => o.estado === 'Pendiente').length;
    const enProgreso = orders.filter((o: MaintenanceOrder) => o.estado === 'EnProgreso').length;
    const costoTotal = orders.reduce((sum: number, o: MaintenanceOrder) => sum + (o.costoEstimado || 0), 0);

    return (
        <Box>
            {/* Header */}
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
                <Box>
                    <Typography variant="h4" fontWeight={700}>
                        {t('maintenance.title')}
                    </Typography>
                    <Typography color="text.secondary">
                        {t('maintenance.list')}
                    </Typography>
                </Box>
                <Button
                    variant="contained"
                    startIcon={<Add />}
                    onClick={() => setOpenCreate(true)}
                    sx={{
                        background: 'linear-gradient(135deg, #00d4aa 0%, #00b894 100%)',
                    }}
                >
                    {t('maintenance.addNew')}
                </Button>
            </Box>

            {/* Stats Cards */}
            <Grid container spacing={2} sx={{ mb: 3 }}>
                <Grid size={{ xs: 12, sm: 4 }}>
                    <Card>
                        <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                            <Schedule sx={{ fontSize: 40, color: 'warning.main' }} />
                            <Box>
                                <Typography variant="h4" fontWeight={700}>{pendientes}</Typography>
                                <Typography variant="body2" color="text.secondary">{t('maintenance.pending')}</Typography>
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>
                <Grid size={{ xs: 12, sm: 4 }}>
                    <Card>
                        <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                            <Build sx={{ fontSize: 40, color: 'info.main' }} />
                            <Box>
                                <Typography variant="h4" fontWeight={700}>{enProgreso}</Typography>
                                <Typography variant="body2" color="text.secondary">{t('maintenance.inProgress')}</Typography>
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>
                <Grid size={{ xs: 12, sm: 4 }}>
                    <Card>
                        <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                            <AttachMoney sx={{ fontSize: 40, color: 'success.main' }} />
                            <Box>
                                <Typography variant="h4" fontWeight={700}>S/ {costoTotal.toLocaleString()}</Typography>
                                <Typography variant="body2" color="text.secondary">{t('maintenance.estimatedCost')}</Typography>
                            </Box>
                        </CardContent>
                    </Card>
                </Grid>
            </Grid>

            {/* Search & Filters */}
            <Paper sx={{ p: 2, mb: 2 }}>
                <TextField
                    size="small"
                    placeholder={t('common.search')}
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    InputProps={{
                        startAdornment: (
                            <InputAdornment position="start">
                                <Search />
                            </InputAdornment>
                        ),
                    }}
                    sx={{ width: 300 }}
                />
            </Paper>

            {/* Error Alert */}
            {error && (
                <Alert severity="error" sx={{ mb: 2 }}>
                    Error al cargar las órdenes de mantenimiento
                </Alert>
            )}

            {/* DataGrid */}
            <Paper sx={{ height: 500 }}>
                <DataGrid
                    rows={orders}
                    columns={columns}
                    loading={isLoading}
                    paginationModel={paginationModel}
                    onPaginationModelChange={setPaginationModel}
                    pageSizeOptions={[5, 10, 25]}
                    rowCount={totalCount}
                    paginationMode="server"
                    disableRowSelectionOnClick
                    sx={{
                        border: 'none',
                        '& .MuiDataGrid-cell:focus': { outline: 'none' },
                    }}
                    localeText={{
                        noRowsLabel: t('maintenance.noMaintenance'),
                    }}
                />
            </Paper>

            {/* Create Dialog */}
            <Dialog open={openCreate} onClose={() => setOpenCreate(false)} maxWidth="sm" fullWidth>
                <DialogTitle>{t('maintenance.addNew')}</DialogTitle>
                <DialogContent>
                    {formError && (
                        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setFormError(null)}>
                            {formError}
                        </Alert>
                    )}
                    <Grid container spacing={2} sx={{ mt: 1 }}>
                        <Grid size={{ xs: 12 }}>
                            <TextField
                                select
                                fullWidth
                                label="Vehículo"
                                value={formData.vehicleId}
                                onChange={(e) => setFormData({ ...formData, vehicleId: Number(e.target.value) })}
                            >
                                {vehicles.map((v: any) => (
                                    <MenuItem key={v.id} value={v.id}>
                                        {v.placa} - {v.marca} {v.modelo}
                                    </MenuItem>
                                ))}
                            </TextField>
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField
                                select
                                fullWidth
                                label={t('maintenance.type')}
                                value={formData.tipo}
                                onChange={(e) => setFormData({ ...formData, tipo: Number(e.target.value) })}
                            >
                                <MenuItem value={0}>{t('maintenance.preventive')}</MenuItem>
                                <MenuItem value={1}>{t('maintenance.corrective')}</MenuItem>
                            </TextField>
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <TextField
                                fullWidth
                                type="date"
                                label={t('maintenance.scheduledDate')}
                                value={formData.fechaProgramada}
                                onChange={(e) => setFormData({ ...formData, fechaProgramada: e.target.value })}
                                InputLabelProps={{ shrink: true }}
                            />
                        </Grid>
                        <Grid size={{ xs: 12 }}>
                            <TextField
                                fullWidth
                                multiline
                                rows={3}
                                label={t('maintenance.description')}
                                value={formData.descripcion}
                                onChange={(e) => setFormData({ ...formData, descripcion: e.target.value })}
                            />
                        </Grid>
                        <Grid size={{ xs: 12 }}>
                            <TextField
                                fullWidth
                                type="number"
                                label={t('maintenance.estimatedCost')}
                                value={formData.costoEstimado}
                                onChange={(e) => setFormData({ ...formData, costoEstimado: Math.max(0, Number(e.target.value)) })}
                                inputProps={{ min: 0 }}
                                InputProps={{
                                    startAdornment: <InputAdornment position="start">S/</InputAdornment>,
                                }}
                            />
                        </Grid>
                    </Grid>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setOpenCreate(false)}>{t('common.cancel')}</Button>
                    <Button
                        variant="contained"
                        onClick={handleCreate}
                        disabled={createMutation.isPending}
                    >
                        {t('common.save')}
                    </Button>
                </DialogActions>
            </Dialog>
        </Box>
    );
};

export default MaintenancePage;
