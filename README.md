# AI Chatbot

基于 Microsoft Agent Framework 和 ASP.NET Core 的 Chatbot 后端模块，并提供独立的纯前端聊天界面。

## 后端架构

- 智能体层: 使用 Microsoft Agent Framework 封装 LLM 能力。
- 接口层: 提供标准 HTTP 接口和 SSE 流式接口。
- 托管层: 暴露 AG-UI 标准端点，方便前端或其他 Agent Client 直接接入。
- 会话层: 使用内存会话存储保存多轮对话上下文。

## 目录结构

```text
Chatbot.Api/
Chatbot.Web/
```

## 配置

在 `Chatbot.Api/appsettings.json` 或环境变量中配置:

```bash
Chatbot__ApiKey=your_openai_api_key
Chatbot__Model=gpt-4.1-mini
Chatbot__Endpoint=https://api.openai.com/v1
Chatbot__SessionLifetimeDays=30
```

说明：

- `Chatbot.Api/appsettings.Development.json` 不再保存明文 `ApiKey`
- 本地开发请优先通过环境变量注入 `Chatbot__ApiKey`
- `Chatbot__SessionLifetimeDays` 可控制登录会话有效期，当前允许范围是 `1-90` 天

## 启动后端

```bash
cd Chatbot.Api
dotnet run
```

启动后可访问 Swagger:

```text
http://127.0.0.1:5078/swagger
```

OpenAPI JSON:

```text
http://127.0.0.1:5078/swagger/v1/swagger.json
```

## 启动前端

```bash
cd Chatbot.Web
npm install
npm run dev
```

## 一键联调

```bash
chmod +x start-dev.sh
./start-dev.sh
```

脚本会自动:

- 启动 `Chatbot.Api`
- 检查 `/health`
- 启动 `Chatbot.Web`

## 停止联调

```bash
chmod +x stop-dev.sh
./stop-dev.sh
```

## 接口

### 1. 非流式

`POST /api/chat`

```json
{
  "message": "介绍一下你自己",
  "sessionId": "optional-session-id"
}
```

### 2. SSE 流式

`POST /api/chat/stream`

返回 `text/event-stream`，事件包括:

- `session`
- `delta`
- `completed`
- `error`

### 3. AI SDK 文本流接口

`POST /api/chat/text-stream`

用于 Vercel AI SDK 的 `TextStreamChatTransport`。

### 4. AG-UI 标准接口

`POST /agui`

## 前端调用建议

前端如果要调用后端业务接口，优先建议基于 OpenAPI 规范生成客户端，而不是手写 fetch。

示例：

```bash
npm --prefix Chatbot.Web run generate:api
```

默认会抓取 `http://127.0.0.1:5078/swagger/v1/swagger.json`，并生成到 `Chatbot.Web/src/generated/api`。

也支持自定义来源：

```bash
npm --prefix Chatbot.Web run generate:api -- http://127.0.0.1:5078/swagger/v1/swagger.json
npm --prefix Chatbot.Web run generate:api -- /absolute/path/to/swagger.json
```

这样前端可以直接复用后端接口定义、请求模型和响应类型。

## 前端配置

在 `Chatbot.Web/.env.example` 基础上配置:

```bash
VITE_CHATBOT_API_BASE_URL=http://127.0.0.1:5078
```

## 运维与部署

已经补充运维说明文档：

- [docs/operations-guide.md](/Users/dengzhizhong/data/repos/deng/AI-Chatbot/docs/operations-guide.md)

包含：

- 环境变量建议
- 单机部署流程
- EF Migration 使用方式
- SQLite 备份与恢复脚本
- `/health` 与审计日志接口说明

SQLite 备份示例：

```bash
bash scripts/backup-sqlite.sh
```

恢复示例：

```bash
bash scripts/restore-sqlite.sh ./backups/chatbot.dev-20260331-120000.tar.gz
```

## Docker 部署

仓库已经补了容器化配置：

- [docker-compose.yml](/Users/dengzhizhong/data/repos/deng/AI-Chatbot/docker-compose.yml)
- [Chatbot.Api/Dockerfile](/Users/dengzhizhong/data/repos/deng/AI-Chatbot/Chatbot.Api/Dockerfile)
- [Chatbot.Web/Dockerfile](/Users/dengzhizhong/data/repos/deng/AI-Chatbot/Chatbot.Web/Dockerfile)

使用方式：

```bash
cp .env.example .env
# 填入 CHATBOT_API_KEY 等变量
docker compose up --build
```

默认端口：

- 前端：`http://localhost:8080`
- 后端：`http://localhost:5078`
