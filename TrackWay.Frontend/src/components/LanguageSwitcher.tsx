import React from 'react';
import { useTranslation } from 'react-i18next';
import { Box, ToggleButton, ToggleButtonGroup } from '@mui/material';

const LanguageSwitcher: React.FC = () => {
    const { i18n } = useTranslation();
    const [language, setLanguage] = React.useState(i18n.language.split('-')[0]);

    const handleLanguageChange = (_event: React.MouseEvent<HTMLElement>, newLanguage: string) => {
        if (newLanguage !== null) {
            setLanguage(newLanguage);
            i18n.changeLanguage(newLanguage);
        }
    };

    return (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <ToggleButtonGroup
                value={language}
                exclusive
                onChange={handleLanguageChange}
                size="small"
                sx={{
                    '& .MuiToggleButton-root': {
                        px: 1.5,
                        py: 0.5,
                        fontSize: '0.75rem',
                        textTransform: 'uppercase',
                    },
                }}
            >
                <ToggleButton value="es">ES</ToggleButton>
                <ToggleButton value="en">EN</ToggleButton>
            </ToggleButtonGroup>
        </Box>
    );
};

export default LanguageSwitcher;
