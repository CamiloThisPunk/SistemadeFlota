import React, { useState } from 'react';
import {
    Box,
    Paper,
    Typography,
    Card,
    CardContent,
    Grid2 as Grid,
    TextField,
    Button,
    MenuItem,
    Divider,
} from '@mui/material';
import {
    LocalGasStation,
    Build,
    Speed,
    Assessment,
    PictureAsPdf,
    TableChart,
    DateRange,
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
    Legend,
    PieChart,
    Pie,
    Cell,
} from 'recharts';
import { useTranslation } from 'react-i18next';

// Mock data for reports
const fuelConsumptionData = [
    { mes: 'Ene', consumo: 4500, costo: 15750 },
    { mes: 'Feb', consumo: 5200, costo: 18200 },
    { mes: 'Mar', consumo: 4800, costo: 16800 },
    { mes: 'Abr', consumo: 3900, costo: 13650 },
    { mes: 'May', consumo: 5100, costo: 17850 },
    { mes: 'Jun', consumo: 4700, costo: 16450 },
    { mes: 'Jul', consumo: 5300, costo: 18550 },
    { mes: 'Ago', consumo: 4600, costo: 16100 },
    { mes: 'Sep', consumo: 4200, costo: 14700 },
    { mes: 'Oct', consumo: 4900, costo: 17150 },
    { mes: 'Nov', consumo: 5000, costo: 17500 },
    { mes: 'Dic', consumo: 5400, costo: 18900 },
];

const maintenanceData = [
    { mes: 'Ene', preventivo: 5, correctivo: 2 },
    { mes: 'Feb', preventivo: 3, correctivo: 4 },
    { mes: 'Mar', preventivo: 7, correctivo: 1 },
    { mes: 'Abr', preventivo: 4, correctivo: 3 },
    { mes: 'May', preventivo: 6, correctivo: 2 },
    { mes: 'Jun', preventivo: 5, correctivo: 1 },
];

const fleetUtilizationData = [
    { name: 'En Operación', value: 65, color: '#00d4aa' },
    { name: 'En Mantenimiento', value: 15, color: '#ff9800' },
    { name: 'Disponible', value: 15, color: '#2196f3' },
    { name: 'Fuera de Servicio', value: 5, color: '#f44336' },
];

const driverPerformanceData = [
    { nombre: 'Carlos Mendoza', km: 12500, eficiencia: 92, incidentes: 0 },
    { nombre: 'Juan Pérez', km: 11200, eficiencia: 88, incidentes: 1 },
    { nombre: 'María García', km: 10800, eficiencia: 95, incidentes: 0 },
    { nombre: 'Luis Torres', km: 9500, eficiencia: 82, incidentes: 2 },
    { nombre: 'Ana Rodríguez', km: 8900, eficiencia: 90, incidentes: 0 },
];

interface ReportCardProps {
    title: string;
    icon: React.ReactNode;
    children: React.ReactNode;
}

const ReportCard: React.FC<ReportCardProps> = ({ title, icon, children }) => (
    <Card sx={{ height: '100%' }}>
        <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                {icon}
                <Typography variant="h6" fontWeight={600}>
                    {title}
                </Typography>
            </Box>
            {children}
        </CardContent>
    </Card>
);

const ReportsPage: React.FC = () => {
    const { t } = useTranslation();
    const [dateFrom, setDateFrom] = useState('2025-01-01');
    const [dateTo, setDateTo] = useState('2025-12-31');
    const [reportType, setReportType] = useState('all');

    const handleExportPDF = () => {
        // TODO: Implement PDF export
        alert('Exportar a PDF - Función por implementar');
    };

    const handleExportExcel = () => {
        // TODO: Implement Excel export
        alert('Exportar a Excel - Función por implementar');
    };

    return (
        <Box>
            {/* Header */}
            <Box sx={{ mb: 4 }}>
                <Typography variant="h4" fontWeight={700}>
                    {t('reports.title')}
                </Typography>
                <Typography color="text.secondary">
                    Análisis y estadísticas de tu flota
                </Typography>
            </Box>

            {/* Filters */}
            <Paper sx={{ p: 3, mb: 4 }}>
                <Grid container spacing={2} alignItems="center">
                    <Grid size={{ xs: 12, sm: 3 }}>
                        <TextField
                            fullWidth
                            select
                            size="small"
                            label="Tipo de Reporte"
                            value={reportType}
                            onChange={(e) => setReportType(e.target.value)}
                        >
                            <MenuItem value="all">Todos los reportes</MenuItem>
                            <MenuItem value="fuel">{t('reports.fuelConsumption')}</MenuItem>
                            <MenuItem value="maintenance">{t('reports.maintenance')}</MenuItem>
                            <MenuItem value="drivers">{t('reports.driverPerformance')}</MenuItem>
                            <MenuItem value="fleet">{t('reports.fleetUtilization')}</MenuItem>
                        </TextField>
                    </Grid>
                    <Grid size={{ xs: 12, sm: 3 }}>
                        <TextField
                            fullWidth
                            type="date"
                            size="small"
                            label={t('reports.from')}
                            value={dateFrom}
                            onChange={(e) => setDateFrom(e.target.value)}
                            InputLabelProps={{ shrink: true }}
                            InputProps={{
                                startAdornment: <DateRange sx={{ mr: 1, color: 'action.active' }} />,
                            }}
                        />
                    </Grid>
                    <Grid size={{ xs: 12, sm: 3 }}>
                        <TextField
                            fullWidth
                            type="date"
                            size="small"
                            label={t('reports.to')}
                            value={dateTo}
                            onChange={(e) => setDateTo(e.target.value)}
                            InputLabelProps={{ shrink: true }}
                            InputProps={{
                                startAdornment: <DateRange sx={{ mr: 1, color: 'action.active' }} />,
                            }}
                        />
                    </Grid>
                    <Grid size={{ xs: 12, sm: 3 }}>
                        <Box sx={{ display: 'flex', gap: 1 }}>
                            <Button
                                variant="outlined"
                                startIcon={<PictureAsPdf />}
                                onClick={handleExportPDF}
                                fullWidth
                            >
                                PDF
                            </Button>
                            <Button
                                variant="outlined"
                                startIcon={<TableChart />}
                                onClick={handleExportExcel}
                                fullWidth
                            >
                                Excel
                            </Button>
                        </Box>
                    </Grid>
                </Grid>
            </Paper>

            {/* Charts Grid */}
            <Grid container spacing={3}>
                {/* Fuel Consumption Chart */}
                {(reportType === 'all' || reportType === 'fuel') && (
                    <Grid size={{ xs: 12, lg: 8 }}>
                        <ReportCard
                            title={t('reports.fuelConsumption')}
                            icon={<LocalGasStation color="primary" />}
                        >
                            <ResponsiveContainer width="100%" height={300}>
                                <AreaChart data={fuelConsumptionData}>
                                    <CartesianGrid strokeDasharray="3 3" opacity={0.3} />
                                    <XAxis dataKey="mes" />
                                    <YAxis yAxisId="left" orientation="left" />
                                    <YAxis yAxisId="right" orientation="right" />
                                    <Tooltip
                                        contentStyle={{
                                            backgroundColor: 'rgba(0,0,0,0.8)',
                                            border: 'none',
                                            borderRadius: 8,
                                        }}
                                    />
                                    <Legend />
                                    <Area
                                        yAxisId="left"
                                        type="monotone"
                                        dataKey="consumo"
                                        name="Galones"
                                        stroke="#00d4aa"
                                        fill="rgba(0,212,170,0.3)"
                                    />
                                    <Area
                                        yAxisId="right"
                                        type="monotone"
                                        dataKey="costo"
                                        name="Costo (S/)"
                                        stroke="#6c5ce7"
                                        fill="rgba(108,92,231,0.3)"
                                    />
                                </AreaChart>
                            </ResponsiveContainer>
                            <Divider sx={{ my: 2 }} />
                            <Box sx={{ display: 'flex', justifyContent: 'space-around' }}>
                                <Box sx={{ textAlign: 'center' }}>
                                    <Typography variant="h5" fontWeight={700} color="primary.main">
                                        57,600
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        Galones Totales
                                    </Typography>
                                </Box>
                                <Box sx={{ textAlign: 'center' }}>
                                    <Typography variant="h5" fontWeight={700} color="secondary.main">
                                        S/ 201,600
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        Costo Total
                                    </Typography>
                                </Box>
                                <Box sx={{ textAlign: 'center' }}>
                                    <Typography variant="h5" fontWeight={700} color="success.main">
                                        8.5
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        Km/Galón Promedio
                                    </Typography>
                                </Box>
                            </Box>
                        </ReportCard>
                    </Grid>
                )}

                {/* Fleet Utilization */}
                {(reportType === 'all' || reportType === 'fleet') && (
                    <Grid size={{ xs: 12, lg: 4 }}>
                        <ReportCard
                            title={t('reports.fleetUtilization')}
                            icon={<Speed color="info" />}
                        >
                            <ResponsiveContainer width="100%" height={250}>
                                <PieChart>
                                    <Pie
                                        data={fleetUtilizationData}
                                        cx="50%"
                                        cy="50%"
                                        innerRadius={60}
                                        outerRadius={80}
                                        paddingAngle={5}
                                        dataKey="value"
                                    >
                                        {fleetUtilizationData.map((entry, index) => (
                                            <Cell key={`cell-${index}`} fill={entry.color} />
                                        ))}
                                    </Pie>
                                    <Tooltip />
                                </PieChart>
                            </ResponsiveContainer>
                            <Box sx={{ mt: 2 }}>
                                {fleetUtilizationData.map((item) => (
                                    <Box
                                        key={item.name}
                                        sx={{
                                            display: 'flex',
                                            justifyContent: 'space-between',
                                            alignItems: 'center',
                                            mb: 1,
                                        }}
                                    >
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                            <Box
                                                sx={{
                                                    width: 12,
                                                    height: 12,
                                                    borderRadius: '50%',
                                                    bgcolor: item.color,
                                                }}
                                            />
                                            <Typography variant="body2">{item.name}</Typography>
                                        </Box>
                                        <Typography variant="body2" fontWeight={600}>
                                            {item.value}%
                                        </Typography>
                                    </Box>
                                ))}
                            </Box>
                        </ReportCard>
                    </Grid>
                )}

                {/* Maintenance Chart */}
                {(reportType === 'all' || reportType === 'maintenance') && (
                    <Grid size={{ xs: 12, lg: 6 }}>
                        <ReportCard
                            title={t('reports.maintenance')}
                            icon={<Build color="warning" />}
                        >
                            <ResponsiveContainer width="100%" height={300}>
                                <BarChart data={maintenanceData}>
                                    <CartesianGrid strokeDasharray="3 3" opacity={0.3} />
                                    <XAxis dataKey="mes" />
                                    <YAxis />
                                    <Tooltip
                                        contentStyle={{
                                            backgroundColor: 'rgba(0,0,0,0.8)',
                                            border: 'none',
                                            borderRadius: 8,
                                        }}
                                    />
                                    <Legend />
                                    <Bar
                                        dataKey="preventivo"
                                        name="Preventivo"
                                        fill="#00d4aa"
                                        radius={[4, 4, 0, 0]}
                                    />
                                    <Bar
                                        dataKey="correctivo"
                                        name="Correctivo"
                                        fill="#ff6b6b"
                                        radius={[4, 4, 0, 0]}
                                    />
                                </BarChart>
                            </ResponsiveContainer>
                            <Divider sx={{ my: 2 }} />
                            <Box sx={{ display: 'flex', justifyContent: 'space-around' }}>
                                <Box sx={{ textAlign: 'center' }}>
                                    <Typography variant="h5" fontWeight={700} color="success.main">
                                        30
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        Preventivos
                                    </Typography>
                                </Box>
                                <Box sx={{ textAlign: 'center' }}>
                                    <Typography variant="h5" fontWeight={700} color="error.main">
                                        13
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        Correctivos
                                    </Typography>
                                </Box>
                                <Box sx={{ textAlign: 'center' }}>
                                    <Typography variant="h5" fontWeight={700} color="primary.main">
                                        70%
                                    </Typography>
                                    <Typography variant="body2" color="text.secondary">
                                        Ratio Preventivo
                                    </Typography>
                                </Box>
                            </Box>
                        </ReportCard>
                    </Grid>
                )}

                {/* Driver Performance */}
                {(reportType === 'all' || reportType === 'drivers') && (
                    <Grid size={{ xs: 12, lg: 6 }}>
                        <ReportCard
                            title={t('reports.driverPerformance')}
                            icon={<Assessment color="secondary" />}
                        >
                            <Box sx={{ overflowX: 'auto' }}>
                                <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                                    <thead>
                                        <tr style={{ borderBottom: '1px solid rgba(255,255,255,0.1)' }}>
                                            <th style={{ textAlign: 'left', padding: '12px 8px' }}>
                                                <Typography variant="body2" fontWeight={600}>Conductor</Typography>
                                            </th>
                                            <th style={{ textAlign: 'right', padding: '12px 8px' }}>
                                                <Typography variant="body2" fontWeight={600}>Km</Typography>
                                            </th>
                                            <th style={{ textAlign: 'right', padding: '12px 8px' }}>
                                                <Typography variant="body2" fontWeight={600}>Eficiencia</Typography>
                                            </th>
                                            <th style={{ textAlign: 'right', padding: '12px 8px' }}>
                                                <Typography variant="body2" fontWeight={600}>Incidentes</Typography>
                                            </th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {driverPerformanceData.map((driver) => (
                                            <tr key={driver.nombre} style={{ borderBottom: '1px solid rgba(255,255,255,0.05)' }}>
                                                <td style={{ padding: '12px 8px' }}>
                                                    <Typography variant="body2">{driver.nombre}</Typography>
                                                </td>
                                                <td style={{ textAlign: 'right', padding: '12px 8px' }}>
                                                    <Typography variant="body2">{driver.km.toLocaleString()}</Typography>
                                                </td>
                                                <td style={{ textAlign: 'right', padding: '12px 8px' }}>
                                                    <Typography
                                                        variant="body2"
                                                        fontWeight={600}
                                                        color={
                                                            driver.eficiencia >= 90
                                                                ? 'success.main'
                                                                : driver.eficiencia >= 80
                                                                    ? 'warning.main'
                                                                    : 'error.main'
                                                        }
                                                    >
                                                        {driver.eficiencia}%
                                                    </Typography>
                                                </td>
                                                <td style={{ textAlign: 'right', padding: '12px 8px' }}>
                                                    <Typography
                                                        variant="body2"
                                                        fontWeight={600}
                                                        color={driver.incidentes === 0 ? 'success.main' : 'error.main'}
                                                    >
                                                        {driver.incidentes}
                                                    </Typography>
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </Box>
                        </ReportCard>
                    </Grid>
                )}
            </Grid>
        </Box>
    );
};

export default ReportsPage;
