import React from 'react';
import {
    Box,
    Grid2 as Grid,
    Paper,
    Typography,
    Card,
    CardContent,
    Chip,
    List,
    ListItem,
    ListItemIcon,
    ListItemText,
    Avatar,
    Skeleton,
} from '@mui/material';
import {
    DirectionsCar,
    People,
    Build,
    Warning,
    LocalGasStation,
    TrendingUp,
    TrendingDown,
} from '@mui/icons-material';
import {
    AreaChart,
    Area,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    ResponsiveContainer,
    BarChart,
    Bar,
    PieChart,
    Pie,
    Cell,
} from 'recharts';
import { useQuery } from '@tanstack/react-query';
import { getVehicles, getVehicleAlerts, getDriverAlerts, getMaintenanceOrders } from '../services/api';

// Mock data para charts (reemplazar con API real)
const consumoData = [
    { mes: 'Ene', galones: 450, costo: 4500 },
    { mes: 'Feb', galones: 520, costo: 5200 },
    { mes: 'Mar', galones: 480, costo: 4800 },
    { mes: 'Abr', galones: 390, costo: 3900 },
    { mes: 'May', galones: 510, costo: 5100 },
    { mes: 'Jun', galones: 470, costo: 4700 },
];

const mantenimientosData = [
    { mes: 'Ene', preventivo: 5, correctivo: 2 },
    { mes: 'Feb', preventivo: 3, correctivo: 4 },
    { mes: 'Mar', preventivo: 7, correctivo: 1 },
    { mes: 'Abr', preventivo: 4, correctivo: 3 },
    { mes: 'May', preventivo: 6, correctivo: 2 },
    { mes: 'Jun', preventivo: 5, correctivo: 1 },
];

const estadoFlotaData = [
    { name: 'Operativos', value: 12, color: '#00d4aa' },
    { name: 'En Mantenimiento', value: 3, color: '#ffc107' },
    { name: 'Fuera de Servicio', value: 1, color: '#f44336' },
];

// KPI Card Component
interface KPICardProps {
    title: string;
    value: string | number;
    subtitle?: string;
    icon: React.ReactNode;
    trend?: number;
    color?: string;
    loading?: boolean;
}

const KPICard: React.FC<KPICardProps> = ({ title, value, subtitle, icon, trend, color = '#00d4aa', loading }) => (
    <Card
        sx={{
            height: '100%',
            background: 'linear-gradient(135deg, rgba(255,255,255,0.05) 0%, rgba(255,255,255,0.02) 100%)',
            border: '1px solid',
            borderColor: 'divider',
            position: 'relative',
            overflow: 'hidden',
        }}
    >
        <CardContent sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <Box>
                    <Typography color="text.secondary" variant="body2" fontWeight={500}>
                        {title}
                    </Typography>
                    {loading ? (
                        <Skeleton width={80} height={40} />
                    ) : (
                        <Typography variant="h4" fontWeight={700} sx={{ mt: 0.5 }}>
                            {value}
                        </Typography>
                    )}
                    {subtitle && (
                        <Typography variant="caption" color="text.secondary">
                            {subtitle}
                        </Typography>
                    )}
                    {trend !== undefined && (
                        <Box sx={{ display: 'flex', alignItems: 'center', mt: 1 }}>
                            {trend >= 0 ? (
                                <TrendingUp sx={{ fontSize: 16, color: 'success.main', mr: 0.5 }} />
                            ) : (
                                <TrendingDown sx={{ fontSize: 16, color: 'error.main', mr: 0.5 }} />
                            )}
                            <Typography
                                variant="caption"
                                color={trend >= 0 ? 'success.main' : 'error.main'}
                                fontWeight={600}
                            >
                                {Math.abs(trend)}% vs mes anterior
                            </Typography>
                        </Box>
                    )}
                </Box>
                <Avatar
                    sx={{
                        width: 56,
                        height: 56,
                        bgcolor: `${color}20`,
                        color: color,
                    }}
                >
                    {icon}
                </Avatar>
            </Box>
        </CardContent>
        <Box
            sx={{
                position: 'absolute',
                bottom: 0,
                left: 0,
                right: 0,
                height: 4,
                bgcolor: color,
                opacity: 0.5,
            }}
        />
    </Card>
);

// Alert Item Component
interface AlertItemProps {
    type: 'warning' | 'error' | 'info';
    title: string;
    description: string;
}

const AlertItem: React.FC<AlertItemProps> = ({ type, title, description }) => (
    <ListItem
        sx={{
            bgcolor: type === 'error' ? 'error.dark' : type === 'warning' ? 'warning.dark' : 'info.dark',
            borderRadius: 2,
            mb: 1,
            opacity: 0.9,
        }}
    >
        <ListItemIcon>
            <Warning sx={{ color: 'white' }} />
        </ListItemIcon>
        <ListItemText
            primary={<Typography fontWeight={600} color="white">{title}</Typography>}
            secondary={<Typography variant="caption" color="grey.300">{description}</Typography>}
        />
    </ListItem>
);

const DashboardPage: React.FC = () => {
    // Queries
    const { data: vehiclesData, isLoading: loadingVehicles } = useQuery({
        queryKey: ['vehicles'],
        queryFn: () => getVehicles(1, 100),
    });

    const { data: alertsData, isLoading: loadingAlerts } = useQuery({
        queryKey: ['vehicleAlerts'],
        queryFn: () => getVehicleAlerts(30),
    });

    useQuery({
        queryKey: ['driverAlerts'],
        queryFn: () => getDriverAlerts(30),
    });

    useQuery({
        queryKey: ['maintenance'],
        queryFn: () => getMaintenanceOrders(1, 50),
    });

    const vehicles = vehiclesData?.data?.data?.items || [];
    const totalVehicles = vehicles.length || 16;
    const operativos = vehicles.filter((v: any) => v.estado === 'Operativo').length || 12;
    const enMantenimiento = vehicles.filter((v: any) => v.estado === 'EnMantenimiento').length || 3;
    const alertas = alertsData?.data?.data?.length || 5;

    return (
        <Box>
            {/* Header */}
            <Box sx={{ mb: 4 }}>
                <Typography variant="h4" fontWeight={700}>
                    Dashboard
                </Typography>
                <Typography color="text.secondary">
                    Bienvenido al panel de control de TrackWay
                </Typography>
            </Box>

            {/* KPIs */}
            <Grid container spacing={3} sx={{ mb: 4 }}>
                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <KPICard
                        title="Total Vehículos"
                        value={totalVehicles}
                        subtitle={`${operativos} operativos`}
                        icon={<DirectionsCar />}
                        color="#00d4aa"
                        loading={loadingVehicles}
                    />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <KPICard
                        title="Conductores Activos"
                        value={12}
                        subtitle="3 con alertas"
                        icon={<People />}
                        trend={5}
                        color="#2196f3"
                    />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <KPICard
                        title="Mantenimientos"
                        value={enMantenimiento}
                        subtitle="Este mes"
                        icon={<Build />}
                        trend={-12}
                        color="#ff9800"
                        loading={false}
                    />
                </Grid>
                <Grid size={{ xs: 12, sm: 6, lg: 3 }}>
                    <KPICard
                        title="Alertas Críticas"
                        value={alertas}
                        subtitle="Requieren atención"
                        icon={<Warning />}
                        color="#f44336"
                        loading={loadingAlerts}
                    />
                </Grid>
            </Grid>

            {/* Charts Row */}
            <Grid container spacing={3} sx={{ mb: 4 }}>
                {/* Consumo de Combustible */}
                <Grid size={{ xs: 12, lg: 8 }}>
                    <Paper sx={{ p: 3, height: 400 }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                            <Box>
                                <Typography variant="h6" fontWeight={600}>
                                    Consumo de Combustible
                                </Typography>
                                <Typography variant="caption" color="text.secondary">
                                    Últimos 6 meses
                                </Typography>
                            </Box>
                            <Chip
                                icon={<LocalGasStation sx={{ fontSize: 16 }} />}
                                label="2,820 gal totales"
                                size="small"
                                color="primary"
                            />
                        </Box>
                        <ResponsiveContainer width="100%" height={280}>
                            <AreaChart data={consumoData}>
                                <defs>
                                    <linearGradient id="colorGalones" x1="0" y1="0" x2="0" y2="1">
                                        <stop offset="5%" stopColor="#00d4aa" stopOpacity={0.3} />
                                        <stop offset="95%" stopColor="#00d4aa" stopOpacity={0} />
                                    </linearGradient>
                                </defs>
                                <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.1)" />
                                <XAxis dataKey="mes" stroke="#888" fontSize={12} />
                                <YAxis stroke="#888" fontSize={12} />
                                <Tooltip
                                    contentStyle={{
                                        background: 'rgba(0,0,0,0.8)',
                                        border: 'none',
                                        borderRadius: 8,
                                    }}
                                />
                                <Area
                                    type="monotone"
                                    dataKey="galones"
                                    stroke="#00d4aa"
                                    strokeWidth={2}
                                    fillOpacity={1}
                                    fill="url(#colorGalones)"
                                />
                            </AreaChart>
                        </ResponsiveContainer>
                    </Paper>
                </Grid>

                {/* Estado de Flota */}
                <Grid size={{ xs: 12, lg: 4 }}>
                    <Paper sx={{ p: 3, height: 400 }}>
                        <Typography variant="h6" fontWeight={600} sx={{ mb: 2 }}>
                            Estado de Flota
                        </Typography>
                        <ResponsiveContainer width="100%" height={200}>
                            <PieChart>
                                <Pie
                                    data={estadoFlotaData}
                                    cx="50%"
                                    cy="50%"
                                    innerRadius={50}
                                    outerRadius={80}
                                    paddingAngle={5}
                                    dataKey="value"
                                >
                                    {estadoFlotaData.map((entry, index) => (
                                        <Cell key={`cell-${index}`} fill={entry.color} />
                                    ))}
                                </Pie>
                                <Tooltip />
                            </PieChart>
                        </ResponsiveContainer>
                        <Box sx={{ mt: 2 }}>
                            {estadoFlotaData.map((item) => (
                                <Box
                                    key={item.name}
                                    sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}
                                >
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                        <Box sx={{ width: 12, height: 12, borderRadius: '50%', bgcolor: item.color }} />
                                        <Typography variant="body2">{item.name}</Typography>
                                    </Box>
                                    <Typography fontWeight={600}>{item.value}</Typography>
                                </Box>
                            ))}
                        </Box>
                    </Paper>
                </Grid>
            </Grid>

            {/* Second Charts Row */}
            <Grid container spacing={3}>
                {/* Mantenimientos por Mes */}
                <Grid size={{ xs: 12, lg: 8 }}>
                    <Paper sx={{ p: 3, height: 350 }}>
                        <Typography variant="h6" fontWeight={600} sx={{ mb: 3 }}>
                            Mantenimientos por Mes
                        </Typography>
                        <ResponsiveContainer width="100%" height={250}>
                            <BarChart data={mantenimientosData}>
                                <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.1)" />
                                <XAxis dataKey="mes" stroke="#888" fontSize={12} />
                                <YAxis stroke="#888" fontSize={12} />
                                <Tooltip
                                    contentStyle={{
                                        background: 'rgba(0,0,0,0.8)',
                                        border: 'none',
                                        borderRadius: 8,
                                    }}
                                />
                                <Bar dataKey="preventivo" name="Preventivo" fill="#00d4aa" radius={[4, 4, 0, 0]} />
                                <Bar dataKey="correctivo" name="Correctivo" fill="#ff9800" radius={[4, 4, 0, 0]} />
                            </BarChart>
                        </ResponsiveContainer>
                    </Paper>
                </Grid>

                {/* Alertas Recientes */}
                <Grid size={{ xs: 12, lg: 4 }}>
                    <Paper sx={{ p: 3, height: 350 }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                            <Typography variant="h6" fontWeight={600}>
                                Alertas Recientes
                            </Typography>
                            <Chip label="5 activas" size="small" color="error" />
                        </Box>
                        <List sx={{ p: 0 }}>
                            <AlertItem
                                type="error"
                                title="SOAT Vencido"
                                description="Vehículo ABC-123 - Vence hoy"
                            />
                            <AlertItem
                                type="warning"
                                title="Licencia por Vencer"
                                description="Carlos García - 5 días restantes"
                            />
                            <AlertItem
                                type="warning"
                                title="Mantenimiento Pendiente"
                                description="DEF-456 - Cambio de aceite"
                            />
                            <AlertItem
                                type="info"
                                title="Revisión Técnica"
                                description="GHI-789 - Próx. semana"
                            />
                        </List>
                    </Paper>
                </Grid>
            </Grid>
        </Box>
    );
};

export default DashboardPage;
