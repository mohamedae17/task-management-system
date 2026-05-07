# Multi-stage build for the Angular client.
# Build context: repository root (so we can also copy docker/nginx.conf).

# ---- build ----
FROM node:22-alpine AS build
WORKDIR /src

COPY client/package.json client/package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY client/ ./
RUN npm run build -- --configuration production

# ---- runtime: nginx serves the SPA and proxies /api + /hubs to the backend ----
FROM nginx:1.27-alpine AS runtime
COPY --from=build /src/dist/client/browser /usr/share/nginx/html
COPY docker/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
