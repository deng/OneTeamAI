FROM node:22-alpine AS build
WORKDIR /app

ARG VITE_CHATBOT_API_BASE_URL=http://localhost:5078
ENV VITE_CHATBOT_API_BASE_URL=${VITE_CHATBOT_API_BASE_URL}

COPY Chatbot.Web/package*.json ./
RUN npm ci

COPY Chatbot.Web/. ./
RUN npm run build

FROM nginx:1.27-alpine AS runtime
WORKDIR /usr/share/nginx/html

COPY Chatbot.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist ./

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
