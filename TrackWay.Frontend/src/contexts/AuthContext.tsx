import React, { createContext, useContext, useState, useEffect, ReactNode, useCallback } from 'react';
import api from '../services/api';

// Tipos de roles disponibles
export type UserRole = 'Admin' | 'Gerente' | 'Asistente' | 'Chofer';

// Permisos del sistema
export type Permission = 
    | 'dashboard:read'
    | 'vehicles:read' | 'vehicles:write' | 'vehicles:delete'
    | 'drivers:read' | 'drivers:write' | 'drivers:delete'
    | 'maintenance:read' | 'maintenance:write' | 'maintenance:delete'
    | 'alerts:read' | 'alerts:write'
    | 'reports:read' | 'reports:export'
    | 'users:read' | 'users:write' | 'users:delete'
    | 'settings:read' | 'settings:write';

// Mapeo de permisos por rol
export const ROLE_PERMISSIONS: Record<UserRole, Permission[]> = {
    Admin: [
        'dashboard:read',
        'vehicles:read', 'vehicles:write', 'vehicles:delete',
        'drivers:read', 'drivers:write', 'drivers:delete',
        'maintenance:read', 'maintenance:write', 'maintenance:delete',
        'alerts:read', 'alerts:write',
        'reports:read', 'reports:export',
        'users:read', 'users:write', 'users:delete',
        'settings:read', 'settings:write',
    ],
    Gerente: [
        'dashboard:read',
        'vehicles:read', 'vehicles:write',
        'drivers:read', 'drivers:write',
        'maintenance:read', 'maintenance:write',
        'alerts:read', 'alerts:write',
        'reports:read', 'reports:export',
        'users:read',
    ],
    Asistente: [
        'dashboard:read',
        'vehicles:read',
        'drivers:read',
        'maintenance:read',
        'alerts:read',
        'reports:read',
    ],
    Chofer: [
        'dashboard:read',
        'vehicles:read',
        'maintenance:read',
        'alerts:read',
    ],
};

// Descripciones de roles para UI
export const ROLE_INFO: Record<UserRole, { label: string; color: string; description: string }> = {
    Admin: { label: 'Administrador', color: '#f44336', description: 'Acceso total al sistema' },
    Gerente: { label: 'Gerente', color: '#2196f3', description: 'Gestión de flota y reportes' },
    Asistente: { label: 'Asistente', color: '#4caf50', description: 'Consulta de información' },
    Chofer: { label: 'Chofer', color: '#ff9800', description: 'Acceso a vehículo asignado' },
};

// Types
export interface User {
    id: number;
    email: string;
    nombre: string;
    apellidos: string;
    rol: UserRole;
}

interface AuthContextType {
    user: User | null;
    token: string | null;
    isAuthenticated: boolean;
    isLoading: boolean;
    login: (email: string, password: string) => Promise<void>;
    logout: () => void;
    hasRole: (roles: UserRole[]) => boolean;
    isAdmin: () => boolean;
    isGerente: () => boolean;
    isAsistente: () => boolean;
    isChofer: () => boolean;
    hasPermission: (permission: Permission) => boolean;
    hasAnyPermission: (permissions: Permission[]) => boolean;
    hasAllPermissions: (permissions: Permission[]) => boolean;
    canRead: (module: string) => boolean;
    canWrite: (module: string) => boolean;
    canDelete: (module: string) => boolean;
    getRoleInfo: () => { label: string; color: string; description: string } | null;
}

interface AuthResponse {
    success: boolean;
    accessToken: string;
    refreshToken: string;
    expiresAt: string;
    user: {
        id: number;
        email: string;
        nombreCompleto: string;
        role: string;
    };
    error?: string;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Storage keys
const TOKEN_KEY = 'trackway_token';
const REFRESH_TOKEN_KEY = 'trackway_refresh_token';
const USER_KEY = 'trackway_user';

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [user, setUser] = useState<User | null>(() => {
        const stored = localStorage.getItem(USER_KEY);
        return stored ? JSON.parse(stored) : null;
    });
    const [token, setToken] = useState<string | null>(() => localStorage.getItem(TOKEN_KEY));
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        // Verificar token al cargar
        const validateToken = async () => {
            if (token) {
                try {
                    api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
                    // Validar token obteniendo usuario actual
                    const response = await api.get('/api/auth/me');
                    // Backend devuelve directamente: { id, email, nombreCompleto, role }
                    const userData = response.data;
                    if (userData && userData.email) {
                        const mappedUser: User = {
                            id: parseInt(userData.id) || userData.Id,
                            email: userData.email || userData.Email,
                            nombre: (userData.nombreCompleto || userData.NombreCompleto || '').split(' ')[0],
                            apellidos: (userData.nombreCompleto || userData.NombreCompleto || '').split(' ').slice(1).join(' '),
                            rol: (userData.role || userData.Role) as UserRole,
                        };
                        setUser(mappedUser);
                        localStorage.setItem(USER_KEY, JSON.stringify(mappedUser));
                    } else {
                        logout();
                    }
                } catch {
                    logout();
                }
            }
            setIsLoading(false);
        };
        validateToken();
    }, []);

    const login = async (email: string, password: string): Promise<void> => {
        setIsLoading(true);
        try {
            const response = await api.post<AuthResponse>('/api/auth/login', { email, password });

            if (response.data.success) {
                const { accessToken, refreshToken, user: userData } = response.data;

                // Mapear usuario al formato esperado
                const mappedUser: User = {
                    id: userData.id,
                    email: userData.email,
                    nombre: userData.nombreCompleto.split(' ')[0],
                    apellidos: userData.nombreCompleto.split(' ').slice(1).join(' '),
                    rol: userData.role as UserRole,
                };

                // Guardar en localStorage
                localStorage.setItem(TOKEN_KEY, accessToken);
                localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
                localStorage.setItem(USER_KEY, JSON.stringify(mappedUser));

                // Configurar axios
                api.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;

                setToken(accessToken);
                setUser(mappedUser);
            } else {
                throw new Error(response.data.error || 'Credenciales inválidas');
            }
        } catch (error: any) {
            throw new Error(error.response?.data?.error || error.message || 'Error al iniciar sesión');
        } finally {
            setIsLoading(false);
        }
    };

    const logout = () => {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(REFRESH_TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
        delete api.defaults.headers.common['Authorization'];
        setToken(null);
        setUser(null);
    };

    // Verificación de roles
    const hasRole = useCallback((roles: UserRole[]): boolean => {
        return user ? roles.includes(user.rol) : false;
    }, [user]);

    const isAdmin = useCallback((): boolean => user?.rol === 'Admin', [user]);
    const isGerente = useCallback((): boolean => user?.rol === 'Gerente', [user]);
    const isAsistente = useCallback((): boolean => user?.rol === 'Asistente', [user]);
    const isChofer = useCallback((): boolean => user?.rol === 'Chofer', [user]);

    // Verificación de permisos
    const hasPermission = useCallback((permission: Permission): boolean => {
        if (!user) return false;
        const permissions = ROLE_PERMISSIONS[user.rol];
        return permissions?.includes(permission) ?? false;
    }, [user]);

    const hasAnyPermission = useCallback((permissions: Permission[]): boolean => {
        return permissions.some(p => hasPermission(p));
    }, [hasPermission]);

    const hasAllPermissions = useCallback((permissions: Permission[]): boolean => {
        return permissions.every(p => hasPermission(p));
    }, [hasPermission]);

    // Helpers para operaciones CRUD por módulo
    const canRead = useCallback((module: string): boolean => {
        return hasPermission(`${module}:read` as Permission);
    }, [hasPermission]);

    const canWrite = useCallback((module: string): boolean => {
        return hasPermission(`${module}:write` as Permission);
    }, [hasPermission]);

    const canDelete = useCallback((module: string): boolean => {
        return hasPermission(`${module}:delete` as Permission);
    }, [hasPermission]);

    // Obtener info del rol actual
    const getRoleInfo = useCallback(() => {
        if (!user) return null;
        return ROLE_INFO[user.rol];
    }, [user]);

    return (
        <AuthContext.Provider
            value={{
                user,
                token,
                isAuthenticated: !!token && !!user,
                isLoading,
                login,
                logout,
                hasRole,
                isAdmin,
                isGerente,
                isAsistente,
                isChofer,
                hasPermission,
                hasAnyPermission,
                hasAllPermissions,
                canRead,
                canWrite,
                canDelete,
                getRoleInfo,
            }}
        >
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = (): AuthContextType => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth debe usarse dentro de AuthProvider');
    }
    return context;
};

export default AuthContext;
