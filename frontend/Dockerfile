FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .

# Vite bakes VITE_-prefixed variables into the JS bundle at build time, so the
# API's address has to be supplied as a build arg rather than a runtime env var.
# Defaults to how the API is reachable when both run via docker compose.
ARG VITE_API_BASE_URL=http://localhost:8080/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

RUN npm run build

FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
