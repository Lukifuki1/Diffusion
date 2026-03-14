// AuraFlow Studio Theme Configuration
// This file configures the OpenWebUI theme to use AuraFlow branding colors

export const auraflowTheme = {
  name: 'AuraFlow Studio',
  colors: {
    primary: '#1E40AF',      // Deep Blue
    secondary: '#7C3AED',    // Purple  
    accent: '#F59E0B',       // Amber
    background: '#0F172A',   // Dark BG
    textLight: '#F8FAFC',    // Light Text
  },
  branding: {
    appName: 'AuraFlow Studio',
    logoPath: '/assets/logo-auraflow.svg',
    faviconPath: '/assets/favicon-auraflow.ico'
  },
  features: {
    enableImageGeneration: true,
    enableVideoGeneration: true,
    maxConcurrentGenerations: 3,
    defaultModel: 'flux-dev',
    timeoutSeconds: 120
  }
};

// Apply theme to OpenWebUI
export function applyAuraflowTheme() {
  // Set CSS variables for theme colors
  const root = document.documentElement;
  Object.entries(auraflowTheme.colors).forEach(([key, value]) => {
    root.style.setProperty(`--${key}`, value);
  });

  // Update app title
  document.title = `${auraflowTheme.branding.appName} - AI Image & Video Generation`;

  // Inject custom CSS if not already present
  if (!document.getElementById('auraflow-theme-css')) {
    const style = document.createElement('style');
    style.id = 'auraflow-theme-css';
    style.textContent = `
      .app-header h1 {
        background: linear-gradient(135deg, ${auraflowTheme.colors.primary}, ${auraflowTheme.colors.secondary});
        -webkit-background-clip: text;
        -webkit-text-fill-color: transparent;
      }
    `;
    document.head.appendChild(style);
  }
}

// Initialize theme on load
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', applyAuraflowTheme);
} else {
  applyAuraflowTheme();
}
