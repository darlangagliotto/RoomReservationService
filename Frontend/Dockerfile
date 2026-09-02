FROM node:lts-alpine AS build
WORKDIR /app

# Camada de dependencias separada: so refaz o install quando o lock muda.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:alpine AS runtime

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY proxy-app.conf /etc/nginx/proxy-app.conf

EXPOSE 5000

# nginx:alpine nao traz curl; o healthcheck do compose usa o wget do busybox.
