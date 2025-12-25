import axios from 'axios';

const API_URL = 'http://localhost:5112';

const api = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
    },
    withCredentials: true,
});

// Request interceptor - agregar token
api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('trackway_token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response interceptor - manejar errores y refresh token
api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config;

        // Si es 401 y no es retry, intentar refresh token
        if (error.response?.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;

            const refreshToken = localStorage.getItem('trackway_refresh_token');
            const accessToken = localStorage.getItem('trackway_token');
            if (refreshToken && accessToken) {
                try {
                    const response = await axios.post(`${API_URL}/api/auth/refresh`, {
                        accessToken,
                        refreshToken,
                    });

                    // Backend devuelve directamente: { success, accessToken, refreshToken, ... }
                    const { accessToken: newAccessToken, refreshToken: newRefreshToken } = response.data;

                    localStorage.setItem('trackway_token', newAccessToken);
                    localStorage.setItem('trackway_refresh_token', newRefreshToken);

                    api.defaults.headers.common['Authorization'] = `Bearer ${newAccessToken}`;
                    originalRequest.headers['Authorization'] = `Bearer ${newAccessToken}`;

                    return api(originalRequest);
                } catch {
                    // Refresh falló - logout
                    localStorage.removeItem('trackway_token');
                    localStorage.removeItem('trackway_refresh_token');
                    localStorage.removeItem('trackway_user');
                    window.location.href = '/login';
                }
            }
        }

        return Promise.reject(error);
    }
);

// ===================== API Types =====================
export interface ApiResponse<T> {
    success: boolean;
    data?: T;
    message?: string;
    errors?: string[];
}

export interface PagedResult<T> {
    items: T[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

// ===================== Vehicle Types =====================
export interface Vehicle {
    id: number;
    placa: string;
    vin: string;
    marca: string;
    modelo: string;
    año: number;
    tipoCombustible: string;
    kmActual: number;
    estado: string;
    alertaKm: boolean;
    alertaDocumentos: boolean;
    conductorActualId?: number;
}

export interface VehicleAlert {
    vehicleId: number;
    placa: string;
    alertas: string[];
    diasParaVencimiento?: number;
}

// ===================== Driver Types =====================
export interface Driver {
    id: number;
    nombres: string;
    apellidos: string;
    documento: string;
    licencia: string;
    categoriaLicencia: string;
    fechaVencimientoLicencia: string;
    scoring: number;
    telefono?: string;
    email?: string;
    activo: boolean;
}

export interface DriverAlert {
    driverId: number;
    nombreCompleto: string;
    diasParaVencimiento: number;
    categoria: string;
}

// ===================== Maintenance Types =====================
export interface MaintenanceOrder {
    id: number;
    vehiculoPlaca: string;
    tipo: string;
    estado: string;
    descripcion: string;
    fechaProgramada: string;
    costoEstimado: number;
    costoReal?: number;
}

// ===================== Dashboard Types =====================
export interface DashboardKPIs {
    totalVehiculos: number;
    vehiculosOperativos: number;
    vehiculosEnMantenimiento: number;
    alertasCriticas: number;
    kmPromedioDiario: number;
    galonesUltimos30Dias: number;
    costoMantenimientoMes: number;
}

// ===================== API Functions =====================

// Vehicles
export const getVehicles = (page = 1, pageSize = 10) =>
    api.get<ApiResponse<PagedResult<Vehicle>>>(`/api/vehicles?pageNumber=${page}&pageSize=${pageSize}`);

export const getVehicleById = (id: number) =>
    api.get<ApiResponse<Vehicle>>(`/api/vehicles/${id}`);

export const getVehicleAlerts = (diasAlerta = 30) =>
    api.get<ApiResponse<VehicleAlert[]>>(`/api/vehicles/alertas?diasAlerta=${diasAlerta}`);

// Mapeo de tipos de combustible a valores de enum del backend
const fuelTypeToEnum: Record<string, number> = {
    'Gasolina': 1,
    'Diesel': 2,
    'GLP': 3,
    'GNV': 4,
    'Eléctrico': 5,
    'Electrico': 5,
    'Híbrido': 6,
    'Hibrido': 6,
};

export const createVehicle = (data: Partial<Vehicle>) => {
    // Mapear campos del frontend a los campos esperados por el backend
    const backendData = {
        placa: data.placa,
        vin: data.vin,
        marca: data.marca,
        modelo: data.modelo,
        anoFabricacion: data.año,
        combustible: fuelTypeToEnum[data.tipoCombustible || 'Diesel'] || 2,
        kmInicial: data.kmActual || 0,
    };
    return api.post<ApiResponse<{ id: number }>>('/api/vehicles', backendData);
};

export const updateVehicleKm = (id: number, km: number) =>
    api.patch<ApiResponse<boolean>>(`/api/vehicles/${id}/kilometraje`, km);

export const deleteVehicle = (id: number) =>
    api.delete<ApiResponse<boolean>>(`/api/vehicles/${id}`);

// Drivers
export const getDrivers = (page = 1, pageSize = 10) =>
    api.get<ApiResponse<PagedResult<Driver>>>(`/api/drivers?pageNumber=${page}&pageSize=${pageSize}`);

export const getDriverById = (id: number) =>
    api.get<ApiResponse<Driver>>(`/api/drivers/${id}`);

export const getDriverAlerts = (diasAlerta = 30) =>
    api.get<ApiResponse<DriverAlert[]>>(`/api/drivers/alertas/licencia?diasAlerta=${diasAlerta}`);

// Mapeo de categorías de licencia a valores de enum del backend
const licenseCategoryToEnum: Record<string, number> = {
    'AI': 1, 'AIIa': 2, 'AIIb': 3, 'AIIIa': 4,
    'AIIIb': 5, 'AIIIc': 6, 'AIVA': 7, 'BIVB': 8,
    // Alias adicionales del frontend
    'BI': 1, 'BIIa': 2, 'BIIb': 3, 'BIIc': 6
};

export const createDriver = (data: Partial<Driver> & {
    nombres?: string;
    fechaVencimientoLicencia?: string;
    licencia?: string;
}) => {
    // Mapear campos del frontend a los campos esperados por el backend
    const backendData = {
        nombre: data.nombres,
        apellidos: data.apellidos,
        documento: data.documento,
        licenciaNum: data.licencia,
        categoriaLicencia: licenseCategoryToEnum[data.categoriaLicencia || 'AIIa'] || 2,
        licenciaVencimiento: data.fechaVencimientoLicencia,
        email: data.email || '',
        telefono: data.telefono || '',
    };
    return api.post<ApiResponse<{ id: number }>>('/api/drivers', backendData);
};

// Maintenance
export const getMaintenanceOrders = (page = 1, pageSize = 10) =>
    api.get<ApiResponse<PagedResult<MaintenanceOrder>>>(`/api/maintenance?pageNumber=${page}&pageSize=${pageSize}`);

export const getMaintenanceAlerts = () =>
    api.get<ApiResponse<any[]>>('/api/maintenance/alertas');

export const createMaintenanceOrder = (data: {
    vehicleId: number;
    tipo: number;
    descripcion: string;
    fechaProgramada: string;
    costoEstimado: number;
}) => {
    // Backend enum: Preventivo=1, Correctivo=2, Predictivo=3, Emergencia=4
    // Frontend sends: 0=Preventivo, 1=Correctivo
    const backendData = {
        vehicleId: data.vehicleId,
        tipo: data.tipo + 1, // Map 0→1, 1→2
        descripcion: data.descripcion,
        fechaProgramada: data.fechaProgramada,
        costoEstimado: Math.max(0, data.costoEstimado), // Ensure non-negative
    };
    return api.post<ApiResponse<number>>('/api/maintenance', backendData);
};

// Dashboard
export const getDashboardKPIs = () =>
    api.get<ApiResponse<DashboardKPIs>>('/api/fleet/dashboard');

export default api;
