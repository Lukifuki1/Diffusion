# AuraFlow Studio Custom OpenWebUI Build

FROM ghcr.io/open-webui/open-webui:main as base

# Copy AuraFlow branding files
COPY ./branding/custom.css /app/backend/static/
COPY ./assets/logo-auraflow.svg /app/frontend/assets/
COPY ./assets/auraflow-theme.js /app/frontend/src/

# Build with custom configuration
RUN npm run build --prefix /app/frontend

EXPOSE 8080

CMD ["python", "-m", "uvicorn", "--host", "0.0.0.0", "--port", "8080", "main:app"]
