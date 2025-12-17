import React, { ReactNode } from 'react';
import { Box, Typography, Paper, Alert, Button } from '@mui/material';
import { Lock as LockIcon, Warning as WarningIcon } from '@mui/icons-material';
import { useAuth, Permission, UserRole, ROLE_INFO } from '../contexts/AuthContext';
import { useTranslation } from 'react-i18next';

// Props para el componente PermissionGate
interface PermissionGateProps {
    children: ReactNode;
    permission?: Permission;
    permissions?: Permission[];
    requireAll?: boolean;
    roles?: UserRole[];
    fallback?: ReactNode;
    showFallback?: boolean;
}

/**
 * Componente que controla el acceso basado en permisos o roles
 * Si el usuario no tiene los permisos requeridos, muestra el fallback o nada
 */
export const PermissionGate: React.FC<PermissionGateProps> = ({
    children,
    permission,
    permissions,
    requireAll = false,
    roles,
    fallback = null,
    showFallback = false,
}) => {
    const { hasPermission, hasAnyPermission, hasAllPermissions, hasRole } = useAuth();

    let hasAccess = true;

    // Verificar por roles si se especificaron
    if (roles && roles.length > 0) {
        hasAccess = hasRole(roles);
    }

    // Verificar por permiso individual
    if (hasAccess && permission) {
        hasAccess = hasPermission(permission);
    }

    // Verificar por múltiples permisos
    if (hasAccess && permissions && permissions.length > 0) {
        hasAccess = requireAll 
            ? hasAllPermissions(permissions) 
            : hasAnyPermission(permissions);
    }

    if (hasAccess) {
        return <>{children}</>;
    }

    return showFallback ? <>{fallback}</> : null;
};

// Props para página sin acceso
interface AccessDeniedPageProps {
    title?: string;
    message?: string;
    showBackButton?: boolean;
    onBack?: () => void;
}

/**
 * Página completa que se muestra cuando el usuario no tiene acceso
 */
export const AccessDeniedPage: React.FC<AccessDeniedPageProps> = ({
    title,
    message,
    showBackButton = true,
    onBack,
}) => {
    const { t } = useTranslation();
    const { user, getRoleInfo } = useAuth();
    const roleInfo = getRoleInfo();

    const handleBack = () => {
        if (onBack) {
            onBack();
        } else {
            window.history.back();
        }
    };

    return (
        <Box
            sx={{
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                minHeight: '60vh',
                p: 3,
            }}
        >
            <Paper
                elevation={0}
                sx={{
                    p: 4,
                    textAlign: 'center',
                    maxWidth: 500,
                    borderRadius: 3,
                    background: (theme) => 
                        theme.palette.mode === 'dark'
                            ? 'linear-gradient(145deg, #1e1e1e 0%, #2d2d2d 100%)'
                            : 'linear-gradient(145deg, #ffffff 0%, #f5f5f5 100%)',
                    border: '1px solid',
                    borderColor: 'divider',
                }}
            >
                <Box
                    sx={{
                        width: 80,
                        height: 80,
                        borderRadius: '50%',
                        background: 'linear-gradient(135deg, #ff6b6b 0%, #ff8e53 100%)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        mx: 'auto',
                        mb: 3,
                    }}
                >
                    <LockIcon sx={{ fontSize: 40, color: 'white' }} />
                </Box>

                <Typography variant="h5" fontWeight={600} gutterBottom>
                    {title || t('access.denied', 'Acceso Denegado')}
                </Typography>

                <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
                    {message || t('access.noPermission', 'No tienes permiso para acceder a esta sección.')}
                </Typography>

                {user && roleInfo && (
                    <Alert 
                        severity="info" 
                        sx={{ mb: 3, textAlign: 'left' }}
                        icon={<WarningIcon />}
                    >
                        <Typography variant="body2">
                            {t('access.currentRole', 'Tu rol actual')}: <strong style={{ color: roleInfo.color }}>{roleInfo.label}</strong>
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                            {roleInfo.description}
                        </Typography>
                    </Alert>
                )}

                {showBackButton && (
                    <Button
                        variant="contained"
                        onClick={handleBack}
                        sx={{
                            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                            '&:hover': {
                                background: 'linear-gradient(135deg, #5a6fd6 0%, #6a4190 100%)',
                            },
                        }}
                    >
                        {t('common.goBack', 'Volver')}
                    </Button>
                )}
            </Paper>
        </Box>
    );
};

// Props para botón con control de acceso
interface AccessControlledButtonProps {
    permission?: Permission;
    permissions?: Permission[];
    roles?: UserRole[];
    children: ReactNode;
    disabledTooltip?: string;
    hideWhenDisabled?: boolean;
    [key: string]: any;
}

/**
 * Wrapper para botones que controla visibilidad/habilitación basado en permisos
 */
export const AccessControlledButton: React.FC<AccessControlledButtonProps> = ({
    permission,
    permissions,
    roles,
    children,
    hideWhenDisabled = false,
    ...props
}) => {
    const { hasPermission, hasAnyPermission, hasRole } = useAuth();

    let hasAccess = true;

    if (roles && roles.length > 0) {
        hasAccess = hasRole(roles);
    }

    if (hasAccess && permission) {
        hasAccess = hasPermission(permission);
    }

    if (hasAccess && permissions && permissions.length > 0) {
        hasAccess = hasAnyPermission(permissions);
    }

    if (!hasAccess && hideWhenDisabled) {
        return null;
    }

    // Clonar el children con disabled si no tiene acceso
    if (!hasAccess) {
        return React.cloneElement(children as React.ReactElement, {
            ...props,
            disabled: true,
        });
    }

    return <>{children}</>;
};

// Hook para verificar acceso a módulos
export const useModuleAccess = (module: string) => {
    const { canRead, canWrite, canDelete } = useAuth();

    return {
        canView: canRead(module),
        canCreate: canWrite(module),
        canEdit: canWrite(module),
        canRemove: canDelete(module),
    };
};

// Componente de rol badge
interface RoleBadgeProps {
    role: UserRole;
    size?: 'small' | 'medium';
}

export const RoleBadge: React.FC<RoleBadgeProps> = ({ role, size = 'medium' }) => {
    const info = ROLE_INFO[role];

    return (
        <Box
            sx={{
                display: 'inline-flex',
                alignItems: 'center',
                px: size === 'small' ? 1 : 1.5,
                py: size === 'small' ? 0.25 : 0.5,
                borderRadius: 1,
                backgroundColor: `${info.color}20`,
                border: `1px solid ${info.color}40`,
            }}
        >
            <Typography
                variant={size === 'small' ? 'caption' : 'body2'}
                sx={{ color: info.color, fontWeight: 600 }}
            >
                {info.label}
            </Typography>
        </Box>
    );
};

export default PermissionGate;
