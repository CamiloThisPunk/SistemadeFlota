import React, { useState } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
    Box,
    Drawer,
    AppBar,
    Toolbar,
    Typography,
    IconButton,
    List,
    ListItem,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    Divider,
    Avatar,
    Menu,
    MenuItem,
    useTheme,
    useMediaQuery,
    Tooltip,
    Badge,
    Chip,
} from '@mui/material';
import {
    Menu as MenuIcon,
    Dashboard,
    DirectionsCar,
    People,
    Build,
    Warning,
    Assessment,
    DarkMode,
    LightMode,
    Logout,
    LocalShipping,
    ChevronLeft,
    Settings,
    AdminPanelSettings,
} from '@mui/icons-material';
import { useAuth, Permission, UserRole } from '../contexts/AuthContext';
import { RoleBadge } from './AccessControl';
import LanguageSwitcher from './LanguageSwitcher';

const drawerWidth = 280;
const collapsedWidth = 72;

interface MainLayoutProps {
    toggleTheme: () => void;
    isDarkMode: boolean;
}

interface NavItem {
    labelKey: string;
    icon: React.ReactNode;
    path: string;
    permission?: Permission;
    roles?: UserRole[];
    badge?: number;
}

const navItems: NavItem[] = [
    { labelKey: 'navigation.dashboard', icon: <Dashboard />, path: '/', permission: 'dashboard:read' },
    { labelKey: 'navigation.vehicles', icon: <DirectionsCar />, path: '/vehiculos', permission: 'vehicles:read' },
    { labelKey: 'navigation.drivers', icon: <People />, path: '/conductores', permission: 'drivers:read' },
    { labelKey: 'navigation.maintenance', icon: <Build />, path: '/mantenimiento', permission: 'maintenance:read' },
    { labelKey: 'navigation.alerts', icon: <Warning />, path: '/alertas', permission: 'alerts:read', badge: 3 },
    { labelKey: 'navigation.reports', icon: <Assessment />, path: '/reportes', permission: 'reports:read' },
    { labelKey: 'navigation.settings', icon: <Settings />, path: '/configuracion', permission: 'settings:read' },
    { labelKey: 'navigation.users', icon: <AdminPanelSettings />, path: '/usuarios', permission: 'users:read' },
];

const MainLayout: React.FC<MainLayoutProps> = ({ toggleTheme, isDarkMode }) => {
    const { t } = useTranslation();
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down('md'));
    const navigate = useNavigate();
    const location = useLocation();
    const { user, logout, hasRole, hasPermission, getRoleInfo } = useAuth();

    const roleInfo = getRoleInfo();

    const [mobileOpen, setMobileOpen] = useState(false);
    const [collapsed, setCollapsed] = useState(false);
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

    const handleDrawerToggle = () => {
        if (isMobile) {
            setMobileOpen(!mobileOpen);
        } else {
            setCollapsed(!collapsed);
        }
    };

    const handleLogout = () => {
        setAnchorEl(null);
        logout();
        navigate('/login');
    };

    const drawerContent = (
        <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
            {/* Header */}
            <Box
                sx={{
                    p: 2,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: collapsed && !isMobile ? 'center' : 'space-between',
                }}
            >
                {(!collapsed || isMobile) && (
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                        <Box
                            sx={{
                                width: 40,
                                height: 40,
                                borderRadius: 2,
                                background: 'linear-gradient(135deg, #00d4aa 0%, #00b894 100%)',
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                            }}
                        >
                            <LocalShipping sx={{ color: 'white', fontSize: 24 }} />
                        </Box>
                        <Typography variant="h6" fontWeight={700} color="primary">
                            TrackWay
                        </Typography>
                    </Box>
                )}
                {!isMobile && (
                    <IconButton onClick={() => setCollapsed(!collapsed)} size="small">
                        <ChevronLeft sx={{ transform: collapsed ? 'rotate(180deg)' : 'none', transition: '0.3s' }} />
                    </IconButton>
                )}
            </Box>

            <Divider />

            {/* Navigation */}
            <List sx={{ flex: 1, px: 1, py: 2 }}>
                {navItems
                    .filter((item) => {
                        // Verificar por permiso si existe
                        if (item.permission) {
                            return hasPermission(item.permission);
                        }
                        // Verificar por roles si existe
                        if (item.roles && item.roles.length > 0) {
                            return hasRole(item.roles);
                        }
                        return true;
                    })
                    .map((item) => (
                        <Tooltip key={item.path} title={collapsed && !isMobile ? t(item.labelKey) : ''} placement="right">
                            <ListItem disablePadding sx={{ mb: 0.5 }}>
                                <ListItemButton
                                    selected={location.pathname === item.path}
                                    onClick={() => {
                                        navigate(item.path);
                                        if (isMobile) setMobileOpen(false);
                                    }}
                                    sx={{
                                        borderRadius: 2,
                                        minHeight: 48,
                                        justifyContent: collapsed && !isMobile ? 'center' : 'flex-start',
                                        '&.Mui-selected': {
                                            background: 'linear-gradient(135deg, rgba(0,212,170,0.15) 0%, rgba(0,184,148,0.15) 100%)',
                                            '& .MuiListItemIcon-root': { color: 'primary.main' },
                                            '& .MuiListItemText-primary': { color: 'primary.main', fontWeight: 600 },
                                        },
                                    }}
                                >
                                    <ListItemIcon
                                        sx={{
                                            minWidth: collapsed && !isMobile ? 0 : 40,
                                            justifyContent: 'center',
                                        }}
                                    >
                                        {item.badge ? (
                                            <Badge badgeContent={item.badge} color="error">
                                                {item.icon}
                                            </Badge>
                                        ) : (
                                            item.icon
                                        )}
                                    </ListItemIcon>
                                    {(!collapsed || isMobile) && <ListItemText primary={t(item.labelKey)} />}
                                </ListItemButton>
                            </ListItem>
                        </Tooltip>
                    ))}
            </List>

            <Divider />

            {/* User section */}
            <Box sx={{ p: 2 }}>
                <Box
                    sx={{
                        display: 'flex',
                        alignItems: 'center',
                        gap: 1.5,
                        justifyContent: collapsed && !isMobile ? 'center' : 'flex-start',
                    }}
                >
                    <Avatar
                        sx={{
                            width: 36,
                            height: 36,
                            bgcolor: roleInfo?.color || 'primary.main',
                            fontSize: 14,
                        }}
                    >
                        {user?.nombre?.[0]}{user?.apellidos?.[0]}
                    </Avatar>
                    {(!collapsed || isMobile) && (
                        <Box sx={{ flex: 1, minWidth: 0 }}>
                            <Typography variant="body2" fontWeight={600} noWrap>
                                {user?.nombre} {user?.apellidos}
                            </Typography>
                            {user?.rol && <RoleBadge role={user.rol} size="small" />}
                        </Box>
                    )}
                </Box>
            </Box>
        </Box>
    );

    const currentWidth = isMobile ? 0 : collapsed ? collapsedWidth : drawerWidth;

    return (
        <Box sx={{ display: 'flex', minHeight: '100vh' }}>
            {/* AppBar */}
            <AppBar
                position="fixed"
                sx={{
                    width: { xs: '100%', md: `calc(100% - ${currentWidth}px)` },
                    ml: { xs: 0, md: `${currentWidth}px` },
                    bgcolor: 'background.paper',
                    color: 'text.primary',
                    boxShadow: 'none',
                    borderBottom: 1,
                    borderColor: 'divider',
                    transition: theme.transitions.create(['width', 'margin'], {
                        easing: theme.transitions.easing.sharp,
                        duration: theme.transitions.duration.enteringScreen,
                    }),
                    zIndex: theme.zIndex.drawer + 1,
                }}
            >
                <Toolbar>
                    <IconButton
                        color="inherit"
                        edge="start"
                        onClick={handleDrawerToggle}
                        sx={{ mr: 2, display: { md: 'none' } }}
                    >
                        <MenuIcon />
                    </IconButton>

                    <Box sx={{ flex: 1 }} />

                    {/* Language switcher */}
                    <LanguageSwitcher />

                    {/* Theme toggle */}
                    <Tooltip title={isDarkMode ? t('common.lightMode') : t('common.darkMode')}>
                        <IconButton onClick={toggleTheme} color="inherit" sx={{ ml: 1 }}>
                            {isDarkMode ? <LightMode /> : <DarkMode />}
                        </IconButton>
                    </Tooltip>

                    {/* User menu */}
                    <IconButton
                        onClick={(e) => setAnchorEl(e.currentTarget)}
                        sx={{ ml: 1 }}
                    >
                        <Avatar sx={{ width: 32, height: 32, bgcolor: 'primary.main', fontSize: 12 }}>
                            {user?.nombre?.[0]}{user?.apellidos?.[0]}
                        </Avatar>
                    </IconButton>
                    <Menu
                        anchorEl={anchorEl}
                        open={Boolean(anchorEl)}
                        onClose={() => setAnchorEl(null)}
                    >
                        <MenuItem onClick={handleLogout}>
                            <Logout sx={{ mr: 1, fontSize: 20 }} /> {t('common.logout')}
                        </MenuItem>
                    </Menu>
                </Toolbar>
            </AppBar>

            {/* Drawer - Mobile */}
            <Drawer
                variant="temporary"
                open={mobileOpen}
                onClose={handleDrawerToggle}
                ModalProps={{ keepMounted: true }}
                sx={{
                    display: { xs: 'block', md: 'none' },
                    '& .MuiDrawer-paper': { width: drawerWidth, boxSizing: 'border-box' },
                }}
            >
                {drawerContent}
            </Drawer>

            {/* Drawer - Desktop */}
            <Drawer
                variant="permanent"
                sx={{
                    display: { xs: 'none', md: 'block' },
                    '& .MuiDrawer-paper': {
                        width: currentWidth,
                        boxSizing: 'border-box',
                        transition: theme.transitions.create('width', {
                            easing: theme.transitions.easing.sharp,
                            duration: theme.transitions.duration.enteringScreen,
                        }),
                        overflowX: 'hidden',
                        borderRight: `1px solid ${theme.palette.divider}`,
                        position: 'fixed',
                        height: '100%',
                    },
                }}
            >
                {drawerContent}
            </Drawer>

            {/* Main content */}
            <Box
                component="main"
                sx={{
                    flexGrow: 1,
                    p: { xs: 2, md: 3 },
                    pt: { xs: 9, md: 11 },
                    ml: { xs: 0, md: `${currentWidth}px` },
                    width: { xs: '100%', md: `calc(100% - ${currentWidth}px)` },
                    minHeight: '100vh',
                    bgcolor: 'background.default',
                    transition: theme.transitions.create(['width', 'margin'], {
                        easing: theme.transitions.easing.sharp,
                        duration: theme.transitions.duration.enteringScreen,
                    }),
                    overflow: 'auto',
                }}
            >
                <Outlet />
            </Box>
        </Box>
    );
};

export default MainLayout;
