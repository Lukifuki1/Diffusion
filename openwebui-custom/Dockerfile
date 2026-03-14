# AuraFlow Studio Custom OpenWebUI Build

FROM ghcr.io/open-webui/open-webui:main as base

# Copy AuraFlow branding files
COPY ./branding/custom.css /app/backend/static/
COPY ./assets/logo-auraflow.svg /app/frontend/src/
COPY ./assets/auraflow-theme.js /app/frontend/src/

# Copy AuraFlow plugin
COPY ./openwebui-plugin /app/backend/bots/studio/auraflow-plugin
RUN cd /app/backend/bots/studio/auraflow-plugin && npm install

# Build with custom configuration
RUN npm run build --prefix /app/frontend

# Environment variables for AuraFlow Studio
ENV WEBUI_NAME="AuraFlow Studio"
ENV OLLAMA_BASE_URL=http://auraflow-api:5000
ENV ENABLE_API_KEY=TRUE
ENV API_KEY_NUM=4

EXPOSE 8080

CMD ["python", "-m", "uvicorn", "--host", "0.0.0.0", "--port", "8080", "main:app"]
