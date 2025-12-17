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
import { createDriver, Driver } from '../services/api';
import api from '../services/api';

// Validation schema
const driverSchema = z.object({
    nombres: z.string()
        .min(2, 'El nombre debe tener al menos 2 caracteres')
        .max(100, 'El nombre no puede tener más de 100 caracteres'),
    apellidos: z.string()
        .min(2, 'Los apellidos deben tener al menos 2 caracteres')
        .max(100, 'Los apellidos no pueden tener más de 100 caracteres'),
    documento: z.string()
        .min(8, 'El documento debe tener al menos 8 caracteres')
        .max(15, 'El documento no puede tener más de 15 caracteres'),
    licencia: z.string()
        .min(6, 'La licencia debe tener al menos 6 caracteres')
        .max(20, 'La licencia no puede tener más de 20 caracteres'),
    categoriaLicencia: z.string().min(1, 'La categoría es requerida'),
    fechaVencimientoLicencia: z.string().min(1, 'La fecha de vencimiento es requerida'),
    telefono: z.string()
        .regex(/^[0-9]{9}$/, 'El teléfono debe tener 9 dígitos')
        .optional()
        .or(z.literal('')),
    email: z.string()
        .email('Email inválido')
        .optional()
        .or(z.literal('')),
});

type DriverFormData = z.infer<typeof driverSchema>;

interface DriverFormDialogProps {
    open: boolean;
    onClose: () => void;
    driver?: Driver | null;
}

const licenseCategories = ['AI', 'AIIa', 'AIIb', 'AIIIa', 'AIIIb', 'AIIIc', 'BI', 'BIIa', 'BIIb', 'BIIc'];

const DriverFormDialog: React.FC<DriverFormDialogProps> = ({ open, onClose, driver }) => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const isEditing = !!driver;

    const {
        control,
        handleSubmit,
        reset,
        formState: { errors, isSubmitting },
    } = useForm<DriverFormData>({
        resolver: zodResolver(driverSchema),
        defaultValues: {
            nombres: '',
            apellidos: '',
            documento: '',
            licencia: '',
            categoriaLicencia: 'AIIa',
            fechaVencimientoLicencia: '',
            telefono: '',
            email: '',
        },
    });

    // Reset form when driver changes
    useEffect(() => {
        if (driver) {
            reset({
                nombres: driver.nombres,
                apellidos: driver.apellidos,
                documento: driver.documento,
                licencia: driver.licencia,
                categoriaLicencia: driver.categoriaLicencia,
                fechaVencimientoLicencia: driver.fechaVencimientoLicencia?.split('T')[0] || '',
                telefono: driver.telefono || '',
                email: driver.email || '',
            });
        } else {
            reset({
                nombres: '',
                apellidos: '',
                documento: '',
                licencia: '',
                categoriaLicencia: 'AIIa',
                fechaVencimientoLicencia: '',
                telefono: '',
                email: '',
            });
        }
    }, [driver, reset]);

    // Create mutation
    const createMutation = useMutation({
        mutationFn: createDriver,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['drivers'] });
            handleClose();
        },
    });

    // Update mutation
    const updateMutation = useMutation({
        mutationFn: (data: DriverFormData) => 
            api.put(`/api/drivers/${driver?.id}`, data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['drivers'] });
            handleClose();
        },
    });

    const handleClose = () => {
        reset();
        onClose();
    };

    const onSubmit = (data: DriverFormData) => {
        const submitData = {
            ...data,
            telefono: data.telefono || undefined,
            email: data.email || undefined,
        };

        if (isEditing) {
            updateMutation.mutate(submitData);
        } else {
            createMutation.mutate(submitData);
        }
    };

    const isLoading = createMutation.isPending || updateMutation.isPending;
    const error = createMutation.error || updateMutation.error;

    return (
        <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
            <DialogTitle>
                {isEditing ? t('drivers.editDriver') : t('drivers.addNew')}
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
                                name="nombres"
                                control={control}
                                render={({ field }) => (
                                    <TextField
                                        {...field}
                                        fullWidth
                                        label={t('drivers.firstNames')}
                                        error={!!errors.nombres}
                                        helperText={errors.nombres?.message}
                                    />
                                )}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <Controller
                                name="apellidos"
                                control={control}
                                render={({ field }) => (
                                    <TextField
                                        {...field}
                                        fullWidth
                                        label={t('drivers.lastNames')}
                                        error={!!errors.apellidos}
                                        helperText={errors.apellidos?.message}
                                    />
                                )}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <Controller
                                name="documento"
                                control={control}
                                render={({ field }) => (
                                    <TextField
                                        {...field}
                                        fullWidth
                                        label={t('drivers.document')}
                                        error={!!errors.documento}
                                        helperText={errors.documento?.message}
                                        disabled={isEditing}
                                    />
                                )}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <Controller
                                name="licencia"
                                control={control}
                                render={({ field }) => (
                                    <TextField
                                        {...field}
                                        fullWidth
                                        label={t('drivers.license')}
                                        error={!!errors.licencia}
                                        helperText={errors.licencia?.message}
                                    />
                                )}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <Controller
                                name="categoriaLicencia"
                                control={control}
                                render={({ field }) => (
                                    <TextField
                                        {...field}
                                        select
                                        fullWidth
                                        label={t('drivers.licenseCategory')}
                                        error={!!errors.categoriaLicencia}
                                        helperText={errors.categoriaLicencia?.message}
                                    >
                                        {licenseCategories.map((cat) => (
                                            <MenuItem key={cat} value={cat}>
                                                {cat}
                                            </MenuItem>
                                        ))}
                                    </TextField>
                                )}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <Controller
                                name="fechaVencimientoLicencia"
                                control={control}
                                render={({ field }) => (
                                    <TextField
                                        {...field}
                                        fullWidth
                                        type="date"
                                        label={t('drivers.licenseExpiry')}
                                        error={!!errors.fechaVencimientoLicencia}
                                        helperText={errors.fechaVencimientoLicencia?.message}
                                        InputLabelProps={{ shrink: true }}
                                    />
                                )}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <Controller
                                name="telefono"
                                control={control}
                                render={({ field }) => (
                                    <TextField
                                        {...field}
                                        fullWidth
                                        label={t('drivers.phone')}
                                        error={!!errors.telefono}
                                        helperText={errors.telefono?.message}
                                        placeholder="987654321"
                                    />
                                )}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 6 }}>
                            <Controller
                                name="email"
                                control={control}
                                render={({ field }) => (
                                    <TextField
                                        {...field}
                                        fullWidth
                                        type="email"
                                        label={t('drivers.email')}
                                        error={!!errors.email}
                                        helperText={errors.email?.message}
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

export default DriverFormDialog;
