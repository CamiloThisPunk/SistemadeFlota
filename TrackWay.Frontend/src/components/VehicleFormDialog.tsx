import React, { useEffect } from 'react';
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    Grid2 as Grid,
    TextField,
    MenuItem,
    Alert,
    CircularProgress,
} from '@mui/material';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useTranslation } from 'react-i18next';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createVehicle, Vehicle } from '../services/api';
import api from '../services/api';

// Validation schema
const vehicleSchema = z.object({
    placa: z.string()
        .min(6, 'La placa debe tener al menos 6 caracteres')
        .max(10, 'La placa no puede tener más de 10 caracteres')
        .regex(/^[A-Z0-9-]+$/i, 'Formato de placa inválido'),
    vin: z.string()
        .length(17, 'El VIN debe tener exactamente 17 caracteres')
        .regex(/^[A-HJ-NPR-Z0-9]+$/i, 'Formato de VIN inválido'),
    marca: z.string().min(2, 'La marca es requerida'),
    modelo: z.string().min(1, 'El modelo es requerido'),
    año: z.number()
        .min(1990, 'El año debe ser mayor a 1990')
        .max(new Date().getFullYear() + 1, 'El año no puede ser futuro'),
    tipoCombustible: z.string().min(1, 'El tipo de combustible es requerido'),
    kmActual: z.number().min(0, 'El kilometraje no puede ser negativo'),
});

type VehicleFormData = z.infer<typeof vehicleSchema>;

interface VehicleFormDialogProps {
    open: boolean;
    onClose: () => void;
    vehicle?: Vehicle | null; // null para crear, objeto para editar
}

const fuelTypes = ['Diesel', 'Gasolina', 'GLP', 'GNV', 'Híbrido', 'Eléctrico'];

const VehicleFormDialog: React.FC<VehicleFormDialogProps> = ({ open, onClose, vehicle }) => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const isEditing = !!vehicle;

    const {
        control,
        handleSubmit,
        reset,
        formState: { errors, isSubmitting },
    } = useForm<VehicleFormData>({
        resolver: zodResolver(vehicleSchema),
        defaultValues: {
            placa: '',
            vin: '',
            marca: '',
            modelo: '',
            año: new Date().getFullYear(),
            tipoCombustible: 'Diesel',
            kmActual: 0,
        },
    });

    // Reset form when vehicle changes
    useEffect(() => {
        if (vehicle) {
            reset({
                placa: vehicle.placa,
                vin: vehicle.vin,
                marca: vehicle.marca,
                modelo: vehicle.modelo,
                año: vehicle.año,
                tipoCombustible: vehicle.tipoCombustible,
                kmActual: vehicle.kmActual,
            });
        } else {
            reset({
                placa: '',
                vin: '',
                marca: '',
                modelo: '',
                año: new Date().getFullYear(),
                tipoCombustible: 'Diesel',
                kmActual: 0,
            });
        }
    }, [vehicle, reset]);

    // Create mutation
    const createMutation = useMutation({
        mutationFn: createVehicle,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['vehicles'] });
            handleClose();
        },
    });

    // Update mutation
    const updateMutation = useMutation({
        mutationFn: (data: VehicleFormData) => 
            api.put(`/api/vehicles/${vehicle?.id}`, data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['vehicles'] });
            handleClose();
        },
    });

    const handleClose = () => {
        reset();
        onClose();
    };

    const onSubmit = (data: VehicleFormData) => {
        if (isEditing) {
            updateMutation.mutate(data);
        } else {
            createMutation.mutate(data);
        }
    };

    const isLoading = createMutation.isPending || updateMutation.isPending;
    const error = createMutation.error || updateMutation.error;

    return (
        <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
            <DialogTitle>
                {isEditing ? t('vehicles.editVehicle') : t('vehicles.addNew')}
            </DialogTitle>
            <form onSubmit={handleSubmit(onSubmit)}>
                <DialogContent>
                    {error && (
                        <Alert severity="error" sx={{ mb: 2 }}>
                            {t('messages.error')}
                        </Alert>
                    )}

                    <Grid container spacing={2}>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <Controller
                                name="placa"
                                control={control}
                                render={({ field }) => (
                                    <TextField
                                        {...field}
                                        fullWidth
                                        label={t('vehicles.plate')}
                                        error={!!errors.placa}
                                        helperText={errors.placa?.message}
                                        disabled={isEditing} // No editar placa
                                        inputProps={{ style: { textTransform: 'uppercase' } }}
                                    />
                                )}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <Controller
                                name="vin"
                                control={control}
                                render={({ field }) => (
                                    <TextField
                                        {...field}
                                        fullWidth
                                        label={t('vehicles.vin')}
                                        error={!!errors.vin}
                                        helperText={errors.vin?.message}
                                        inputProps={{ style: { textTransform: 'uppercase' } }}
                                    />
                                )}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <Controller
                                name="marca"
                                control={control}
                                render={({ field }) => (
                                    <TextField
                                        {...field}
                                        fullWidth
                                        label={t('vehicles.brand')}
                                        error={!!errors.marca}
                                        helperText={errors.marca?.message}
                                    />
                                )}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <Controller
                                name="modelo"
                                control={control}
                                render={({ field }) => (
                                    <TextField
                                        {...field}
                                        fullWidth
                                        label={t('vehicles.model')}
                                        error={!!errors.modelo}
                                        helperText={errors.modelo?.message}
                                    />
                                )}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 4 }}>
                            <Controller
                                name="año"
                                control={control}
                                render={({ field: { onChange, value, ...field } }) => (
                                    <TextField
                                        {...field}
                                        value={value}
                                        onChange={(e) => onChange(Number(e.target.value))}
                                        fullWidth
                                        type="number"
                                        label={t('vehicles.year')}
                                        error={!!errors.año}
                                        helperText={errors.año?.message}
                                    />
                                )}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 4 }}>
                            <Controller
                                name="tipoCombustible"
                                control={control}
                                render={({ field }) => (
                                    <TextField
                                        {...field}
                                        select
                                        fullWidth
                                        label={t('vehicles.fuelType')}
                                        error={!!errors.tipoCombustible}
                                        helperText={errors.tipoCombustible?.message}
                                    >
                                        {fuelTypes.map((type) => (
                                            <MenuItem key={type} value={type}>
                                                {type}
                                            </MenuItem>
                                        ))}
                                    </TextField>
                                )}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 4 }}>
                            <Controller
                                name="kmActual"
                                control={control}
                                render={({ field: { onChange, value, ...field } }) => (
                                    <TextField
                                        {...field}
                                        value={value}
                                        onChange={(e) => onChange(Number(e.target.value))}
                                        fullWidth
                                        type="number"
                                        label={t('vehicles.currentKm')}
                                        error={!!errors.kmActual}
                                        helperText={errors.kmActual?.message}
                                    />
                                )}
                            />
                        </Grid>
                    </Grid>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleClose} disabled={isLoading}>
                        {t('common.cancel')}
                    </Button>
                    <Button
                        type="submit"
                        variant="contained"
                        disabled={isLoading || isSubmitting}
                        startIcon={isLoading ? <CircularProgress size={20} /> : null}
                    >
                        {t('common.save')}
                    </Button>
                </DialogActions>
            </form>
        </Dialog>
    );
};

export default VehicleFormDialog;
