import React from 'react';
import {
    Box,
    Paper,
    Typography,
    Card,
    CardContent,
    Chip,
    List,
    ListItem,
    ListItemIcon,
    ListItemText,
    ListItemSecondaryAction,
    Tabs,
    Tab,
    Skeleton,
    Alert as MuiAlert,
    Button,
    Divider,
} from '@mui/material';
import {
    Warning,
    Error as ErrorIcon,
    Info,
    DirectionsCar,
    Badge,
    CalendarToday,
    CheckCircle,
} from '@mui/icons-material';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { getVehicleAlerts, getDriverAlerts, VehicleAlert, DriverAlert } from '../services/api';

interface TabPanelProps {
    children?: React.ReactNode;
    index: number;
    value: number;
}

function TabPanel(props: TabPanelProps) {
    const { children, value, index, ...other } = props;
    return (
        <div
            role="tabpanel"
            hidden={value !== index}
            id={`alerts-tabpanel-${index}`}
            {...other}
        >
            {value === index && <Box sx={{ py: 2 }}>{children}</Box>}
        </div>
    );
}

const AlertsPage: React.FC = () => {
    const { t } = useTranslation();
    const [tabValue, setTabValue] = React.useState(0);

    const { data: vehicleAlertsData, isLoading: loadingVehicle } = useQuery({
        queryKey: ['vehicleAlerts'],
        queryFn: () => getVehicleAlerts(30),
    });

    const { data: driverAlertsData, isLoading: loadingDriver } = useQuery({
        queryKey: ['driverAlerts'],
        queryFn: () => getDriverAlerts(30),
    });

    const vehicleAlerts: VehicleAlert[] = vehicleAlertsData?.data?.data || [];
    const driverAlerts: DriverAlert[] = driverAlertsData?.data?.data || [];

    const totalAlerts = vehicleAlerts.length + driverAlerts.length;
    const criticalCount = vehicleAlerts.filter(a => a.diasParaVencimiento && a.diasParaVencimiento <= 7).length +
        driverAlerts.filter(a => a.diasParaVencimiento <= 7).length;
    const warningCount = totalAlerts - criticalCount;

    const getAlertSeverity = (days: number | undefined) => {
        if (!days || days <= 7) return 'error';
        if (days <= 15) return 'warning';
        return 'info';
    };

    const getSeverityIcon = (days: number | undefined) => {
        if (!days || days <= 7) return <ErrorIcon color="error" />;
        if (days <= 15) return <Warning color="warning" />;
        return <Info color="info" />;
    };

    return (
        <Box>
            {/* Header */}
            <Box sx={{ mb: 4 }}>
                <Typography variant="h4" fontWeight={700}>
                    {t('alerts.title')}
                </Typography>
                <Typography color="text.secondary">
                    {t('alerts.noAlerts', { defaultValue: 'Gestiona las alertas de tu flota' })}
                </Typography>
            </Box>

            {/* Summary Cards */}
            <Box sx={{ display: 'flex', gap: 2, mb: 4, flexWrap: 'wrap' }}>
                <Card sx={{ minWidth: 180, flex: 1 }}>
                    <CardContent>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                            <ErrorIcon sx={{ fontSize: 40, color: 'error.main' }} />
                            <Box>
                                <Typography variant="h4" fontWeight={700} color="error.main">
                                    {criticalCount}
                                </Typography>
                                <Typography variant="body2" color="text.secondary">
                                    {t('alerts.critical')}
                                </Typography>
                            </Box>
                        </Box>
                    </CardContent>
                </Card>
                <Card sx={{ minWidth: 180, flex: 1 }}>
                    <CardContent>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                            <Warning sx={{ fontSize: 40, color: 'warning.main' }} />
                            <Box>
                                <Typography variant="h4" fontWeight={700} color="warning.main">
                                    {warningCount}
                                </Typography>
                                <Typography variant="body2" color="text.secondary">
                                    {t('alerts.warning')}
                                </Typography>
                            </Box>
                        </Box>
                    </CardContent>
                </Card>
                <Card sx={{ minWidth: 180, flex: 1 }}>
                    <CardContent>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                            <CheckCircle sx={{ fontSize: 40, color: 'success.main' }} />
                            <Box>
                                <Typography variant="h4" fontWeight={700} color="success.main">
                                    {totalAlerts === 0 ? '✓' : totalAlerts}
                                </Typography>
                                <Typography variant="body2" color="text.secondary">
                                    Total
                                </Typography>
                            </Box>
                        </Box>
                    </CardContent>
                </Card>
            </Box>

            {/* Tabs */}
            <Paper sx={{ mb: 2 }}>
                <Tabs
                    value={tabValue}
                    onChange={(_, newValue) => setTabValue(newValue)}
                    sx={{ borderBottom: 1, borderColor: 'divider' }}
                >
                    <Tab
                        label={
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <DirectionsCar fontSize="small" />
                                {t('alerts.vehicle')}
                                <Chip label={vehicleAlerts.length} size="small" color="error" />
                            </Box>
                        }
                    />
                    <Tab
                        label={
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <Badge fontSize="small" />
                                {t('alerts.driver')}
                                <Chip label={driverAlerts.length} size="small" color="warning" />
                            </Box>
                        }
                    />
                </Tabs>

                {/* Vehicle Alerts */}
                <TabPanel value={tabValue} index={0}>
                    {loadingVehicle ? (
                        <Box sx={{ p: 2 }}>
                            {[1, 2, 3].map(i => <Skeleton key={i} height={80} sx={{ mb: 1 }} />)}
                        </Box>
                    ) : vehicleAlerts.length === 0 ? (
                        <MuiAlert severity="success" sx={{ m: 2 }}>
                            No hay alertas de vehículos
                        </MuiAlert>
                    ) : (
                        <List>
                            {vehicleAlerts.map((alert, index) => (
                                <React.Fragment key={alert.vehicleId}>
                                    <ListItem>
                                        <ListItemIcon>
                                            {getSeverityIcon(alert.diasParaVencimiento)}
                                        </ListItemIcon>
                                        <ListItemText
                                            primary={
                                                <Typography fontWeight={600}>
                                                    {alert.placa}
                                                </Typography>
                                            }
                                            secondary={
                                                <Box sx={{ mt: 0.5 }}>
                                                    {alert.alertas.map((a, i) => (
                                                        <Chip
                                                            key={i}
                                                            label={a}
                                                            size="small"
                                                            color={getAlertSeverity(alert.diasParaVencimiento) as any}
                                                            variant="outlined"
                                                            sx={{ mr: 0.5, mb: 0.5 }}
                                                        />
                                                    ))}
                                                </Box>
                                            }
                                        />
                                        <ListItemSecondaryAction>
                                            {alert.diasParaVencimiento && (
                                                <Chip
                                                    icon={<CalendarToday />}
                                                    label={`${alert.diasParaVencimiento} días`}
                                                    color={getAlertSeverity(alert.diasParaVencimiento) as any}
                                                    size="small"
                                                />
                                            )}
                                        </ListItemSecondaryAction>
                                    </ListItem>
                                    {index < vehicleAlerts.length - 1 && <Divider />}
                                </React.Fragment>
                            ))}
                        </List>
                    )}
                </TabPanel>

                {/* Driver Alerts */}
                <TabPanel value={tabValue} index={1}>
                    {loadingDriver ? (
                        <Box sx={{ p: 2 }}>
                            {[1, 2, 3].map(i => <Skeleton key={i} height={80} sx={{ mb: 1 }} />)}
                        </Box>
                    ) : driverAlerts.length === 0 ? (
                        <MuiAlert severity="success" sx={{ m: 2 }}>
                            No hay alertas de conductores
                        </MuiAlert>
                    ) : (
                        <List>
                            {driverAlerts.map((alert, index) => (
                                <React.Fragment key={alert.driverId}>
                                    <ListItem>
                                        <ListItemIcon>
                                            {getSeverityIcon(alert.diasParaVencimiento)}
                                        </ListItemIcon>
                                        <ListItemText
                                            primary={
                                                <Typography fontWeight={600}>
                                                    {alert.nombreCompleto}
                                                </Typography>
                                            }
                                            secondary={
                                                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.5 }}>
                                                    <Chip
                                                        label={`Licencia ${alert.categoria}`}
                                                        size="small"
                                                        variant="outlined"
                                                    />
                                                    <Typography variant="body2" color="text.secondary">
                                                        Vence en {alert.diasParaVencimiento} días
                                                    </Typography>
                                                </Box>
                                            }
                                        />
                                        <ListItemSecondaryAction>
                                            <Chip
                                                icon={<CalendarToday />}
                                                label={`${alert.diasParaVencimiento} días`}
                                                color={getAlertSeverity(alert.diasParaVencimiento) as any}
                                                size="small"
                                            />
                                        </ListItemSecondaryAction>
                                    </ListItem>
                                    {index < driverAlerts.length - 1 && <Divider />}
                                </React.Fragment>
                            ))}
                        </List>
                    )}
                </TabPanel>
            </Paper>

            {/* Actions */}
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
                <Button variant="outlined" startIcon={<CheckCircle />}>
                    Marcar todas como revisadas
                </Button>
            </Box>
        </Box>
    );
};

export default AlertsPage;
