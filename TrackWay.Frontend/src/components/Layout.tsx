import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import {
    Box,
    Drawer,
    AppBar,
    Toolbar,
    List,
    Typography,
    Divider,
    IconButton,
    ListItem,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    Avatar,
    Collapse,
    Chip,
    alpha,
} from '@mui/material';
import {
    Menu as MenuIcon,
    Dashboard as DashboardIcon,
    DirectionsCar as CarIcon,
    Person as PersonIcon,
    Route as RouteIcon,
    Restaurant as RestaurantIcon,
    TableBar as TableIcon,
    Fastfood as FoodIcon,
    Receipt as ReceiptIcon,
    ExpandLess,
    ExpandMore,
    LocalShipping as FleetIcon,
    Notifications as NotificationsIcon,
} from '@mui/icons-material';

const drawerWidth = 280;

interface LayoutProps {
    children: React.ReactNode;
}

export default function Layout({ children }: LayoutProps) {
    const [mobileOpen, setMobileOpen] = useState(false);
    const [fleetOpen, setFleetOpen] = useState(true);
    const [restauranteOpen, setRestauranteOpen] = useState(true);
    const location = useLocation();
    const navigate = useNavigate();

    const handleDrawerToggle = () => setMobileOpen(!mobileOpen);

    const isActive = (path: string) => location.pathname === path;

    const menuItemStyle = (path: string) => ({
        borderRadius: 2,
        mx: 1,
        mb: 0.5,
        backgroundColor: isActive(path) ? alpha('#6366f1', 0.2) : 'transparent',
        borderLeft: isActive(path) ? '3px solid #6366f1' : '3px solid transparent',
        '&:hover': {
            backgroundColor: alpha('#6366f1', 0.1),
        },
    });

    const drawer = (
        <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
            {/* Logo */}
            <Box sx={{ p: 3, display: 'flex', alignItems: 'center', gap: 2 }}>
                <Avatar
                    sx={{
                        width: 48,
                        height: 48,
                        background: 'linear-gradient(135deg, #6366f1 0%, #22d3ee 100%)',
                        fontWeight: 700,
                    }}
                >
                    TW
                </Avatar>
                <Box>
                    <Typography variant="h6" sx={{ fontWeight: 700, color: 'white' }}>
                        TrackWay
                    </Typography>
                    <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                        Sistema de Gestión
                    </Typography>
                </Box>
            </Box>

            <Divider sx={{ borderColor: alpha('#6366f1', 0.1) }} />

            {/* Navigation */}
            <List sx={{ flex: 1, pt: 2 }}>
                {/* Dashboard */}
                <ListItem disablePadding>
                    <ListItemButton onClick={() => navigate('/')} sx={menuItemStyle('/')}>
                        <ListItemIcon>
                            <DashboardIcon sx={{ color: isActive('/') ? '#6366f1' : 'text.secondary' }} />
                        </ListItemIcon>
                        <ListItemText primary="Dashboard" />
                    </ListItemButton>
                </ListItem>

                {/* Fleet Section */}
                <ListItem disablePadding sx={{ mt: 2 }}>
                    <ListItemButton onClick={() => setFleetOpen(!fleetOpen)} sx={{ mx: 1, borderRadius: 2 }}>
                        <ListItemIcon>
                            <FleetIcon sx={{ color: '#22d3ee' }} />
                        </ListItemIcon>
                        <ListItemText
                            primary="Fleet Management"
                            primaryTypographyProps={{ fontWeight: 600 }}
                        />
                        <Chip label="5" size="small" color="primary" sx={{ mr: 1 }} />
                        {fleetOpen ? <ExpandLess /> : <ExpandMore />}
                    </ListItemButton>
                </ListItem>
                <Collapse in={fleetOpen} timeout="auto" unmountOnExit>
                    <List component="div" disablePadding sx={{ pl: 2 }}>
                        <ListItemButton onClick={() => navigate('/fleet/vehiculos')} sx={menuItemStyle('/fleet/vehiculos')}>
                            <ListItemIcon>
                                <CarIcon sx={{ color: isActive('/fleet/vehiculos') ? '#6366f1' : 'text.secondary' }} />
                            </ListItemIcon>
                            <ListItemText primary="Vehículos" />
                        </ListItemButton>
                        <ListItemButton onClick={() => navigate('/fleet/conductores')} sx={menuItemStyle('/fleet/conductores')}>
                            <ListItemIcon>
                                <PersonIcon sx={{ color: isActive('/fleet/conductores') ? '#6366f1' : 'text.secondary' }} />
                            </ListItemIcon>
                            <ListItemText primary="Conductores" />
                        </ListItemButton>
                        <ListItemButton onClick={() => navigate('/fleet/rutas')} sx={menuItemStyle('/fleet/rutas')}>
                            <ListItemIcon>
                                <RouteIcon sx={{ color: isActive('/fleet/rutas') ? '#6366f1' : 'text.secondary' }} />
                            </ListItemIcon>
                            <ListItemText primary="Rutas" />
                        </ListItemButton>
                    </List>
                </Collapse>

                {/* Restaurante Section */}
                <ListItem disablePadding sx={{ mt: 1 }}>
                    <ListItemButton onClick={() => setRestauranteOpen(!restauranteOpen)} sx={{ mx: 1, borderRadius: 2 }}>
                        <ListItemIcon>
                            <RestaurantIcon sx={{ color: '#f59e0b' }} />
                        </ListItemIcon>
                        <ListItemText
                            primary="Restaurante"
                            primaryTypographyProps={{ fontWeight: 600 }}
                        />
                        <Chip label="6" size="small" color="warning" sx={{ mr: 1 }} />
                        {restauranteOpen ? <ExpandLess /> : <ExpandMore />}
                    </ListItemButton>
                </ListItem>
                <Collapse in={restauranteOpen} timeout="auto" unmountOnExit>
                    <List component="div" disablePadding sx={{ pl: 2 }}>
                        <ListItemButton onClick={() => navigate('/restaurante/mesas')} sx={menuItemStyle('/restaurante/mesas')}>
                            <ListItemIcon>
                                <TableIcon sx={{ color: isActive('/restaurante/mesas') ? '#6366f1' : 'text.secondary' }} />
                            </ListItemIcon>
                            <ListItemText primary="Mesas" />
                        </ListItemButton>
                        <ListItemButton onClick={() => navigate('/restaurante/productos')} sx={menuItemStyle('/restaurante/productos')}>
                            <ListItemIcon>
                                <FoodIcon sx={{ color: isActive('/restaurante/productos') ? '#6366f1' : 'text.secondary' }} />
                            </ListItemIcon>
                            <ListItemText primary="Productos" />
                        </ListItemButton>
                        <ListItemButton onClick={() => navigate('/restaurante/ordenes')} sx={menuItemStyle('/restaurante/ordenes')}>
                            <ListItemIcon>
                                <ReceiptIcon sx={{ color: isActive('/restaurante/ordenes') ? '#6366f1' : 'text.secondary' }} />
                            </ListItemIcon>
                            <ListItemText primary="Órdenes" />
                        </ListItemButton>
                    </List>
                </Collapse>
            </List>

            {/* Footer */}
            <Box sx={{ p: 2 }}>
                <Box
                    sx={{
                        p: 2,
                        borderRadius: 2,
                        background: 'linear-gradient(135deg, rgba(99, 102, 241, 0.2) 0%, rgba(34, 211, 238, 0.2) 100%)',
                        border: '1px solid',
                        borderColor: alpha('#6366f1', 0.2),
                    }}
                >
                    <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                        Versión 1.0.0
                    </Typography>
                </Box>
            </Box>
        </Box>
    );

    return (
        <Box sx={{ display: 'flex', minHeight: '100vh' }}>
            <AppBar
                position="fixed"
                sx={{
                    width: { sm: `calc(100% - ${drawerWidth}px)` },
                    ml: { sm: `${drawerWidth}px` },
                }}
            >
                <Toolbar>
                    <IconButton
                        color="inherit"
                        edge="start"
                        onClick={handleDrawerToggle}
                        sx={{ mr: 2, display: { sm: 'none' } }}
                    >
                        <MenuIcon />
                    </IconButton>
                    <Box sx={{ flexGrow: 1 }} />
                    <IconButton color="inherit">
                        <NotificationsIcon />
                    </IconButton>
                    <Avatar
                        sx={{
                            ml: 2,
                            width: 36,
                            height: 36,
                            background: 'linear-gradient(135deg, #6366f1 0%, #22d3ee 100%)',
                        }}
                    >
                        A
                    </Avatar>
                </Toolbar>
            </AppBar>

            <Box
                component="nav"
                sx={{ width: { sm: drawerWidth }, flexShrink: { sm: 0 } }}
            >
                <Drawer
                    variant="temporary"
                    open={mobileOpen}
                    onClose={handleDrawerToggle}
                    ModalProps={{ keepMounted: true }}
                    sx={{
                        display: { xs: 'block', sm: 'none' },
                        '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
                    }}
                >
                    {drawer}
                </Drawer>
                <Drawer
                    variant="permanent"
                    sx={{
                        display: { xs: 'none', sm: 'block' },
                        '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
                    }}
                    open
                >
                    {drawer}
                </Drawer>
            </Box>

            <Box
                component="main"
                sx={{
                    flexGrow: 1,
                    p: 3,
                    width: { sm: `calc(100% - ${drawerWidth}px)` },
                    mt: 8,
                    minHeight: 'calc(100vh - 64px)',
                }}
            >
                {children}
            </Box>
        </Box>
    );
}
