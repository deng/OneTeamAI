# 运维与部署说明

## 1. 环境变量

后端至少需要以下环境变量：

```bash
Chatbot__ApiKey=your_api_key
Chatbot__Model=gpt-4.1-mini
Chatbot__Endpoint=https://api.openai.com/v1
Chatbot__SessionLifetimeDays=30
ConnectionStrings__DefaultConnection=Data Source=chatbot.db
ASPNETCORE_URLS=http://0.0.0.0:5078
```

前端建议配置：

```bash
VITE_CHATBOT_API_BASE_URL=http://127.0.0.1:5078
```

说明：

- 开发环境不要把 `Chatbot__ApiKey` 写回 `appsettings.Development.json`
- 建议通过部署平台密钥管理或系统环境变量注入
- SQLite 适合单机试用和早期内测，正式多实例部署应切换到更稳定的数据库

## 2. 本地或单机部署流程

### 后端

```bash
export Chatbot__ApiKey=your_api_key
export ASPNETCORE_URLS=http://0.0.0.0:5078
export ConnectionStrings__DefaultConnection="Data Source=chatbot.db"

dotnet build AI-Chatbot.sln -m:1
dotnet run --project Chatbot.Api/Chatbot.Api.csproj
```

### 前端

```bash
cd Chatbot.Web
npm install
VITE_CHATBOT_API_BASE_URL=http://127.0.0.1:5078 npm run build
```

## 2.1 Docker 单机部署

仓库已提供：

- [docker-compose.yml](/Users/dengzhizhong/data/repos/deng/AI-Chatbot/docker-compose.yml)
- [Chatbot.Api/Dockerfile](/Users/dengzhizhong/data/repos/deng/AI-Chatbot/Chatbot.Api/Dockerfile)
- [Chatbot.Web/Dockerfile](/Users/dengzhizhong/data/repos/deng/AI-Chatbot/Chatbot.Web/Dockerfile)

启动方式：

```bash
cp .env.example .env
# 编辑 .env，填入 CHATBOT_API_KEY
docker compose up --build
```

默认暴露：

- Web: `http://localhost:8080`
- API: `http://localhost:5078`

说明：

- `api` 服务会把 SQLite 数据库存到 Docker volume `chatbot-data`
- `web` 服务会在构建阶段注入 `VITE_CHATBOT_API_BASE_URL`
- 前端通过 Nginx 以 SPA 方式托管，已包含路由回退配置

## 3. 数据库迁移

新增迁移：

```bash
dotnet ef migrations add MigrationName --project Chatbot.Api/Chatbot.Api.csproj
```

应用迁移：

```bash
dotnet ef database update --project Chatbot.Api/Chatbot.Api.csproj
```

说明：

- 当前后端启动时会自动执行 `MigrateAsync()`
- 正式环境建议在部署流程里显式执行迁移，并保留回滚预案

## 4. SQLite 备份与恢复

### 备份

```bash
bash scripts/backup-sqlite.sh
```

指定数据库路径和备份目录：

```bash
bash scripts/backup-sqlite.sh ./Chatbot.Api/chatbot.dev.db ./backups
```

### 恢复

```bash
bash scripts/restore-sqlite.sh ./backups/chatbot.dev-20260331-120000.tar.gz
```

说明：

- 备份脚本会同时打包 `.db`、`.db-shm`、`.db-wal`
- 恢复前建议先停止后端服务，避免 SQLite 文件被占用

## 5. 健康检查与审计

健康检查：

```text
GET /health
```

返回内容已包含：

- 数据库可达性
- Chatbot 配置状态
- 活跃会话数
- 过期会话数
- 团队数
- 待处理邀请数
- 已过期邀请数
- 审计日志数
- 检查时间

运行时维护：

- 读取健康检查时会顺带清理过期登录会话
- 读取当前用户邀请时会顺带刷新过期邀请状态
- 读取当前登录会话时会更新 `LastSeenAt`

审计日志接口：

```text
GET /api/audit-logs/me
GET /api/teams/{teamId}/audit-logs
GET /api/auth/sessions
POST /api/auth/logout-all
POST /api/auth/sessions/{sessionId}/revoke
```

用途：

- 回看当前用户最近操作
- 回看当前用户当前和历史登录会话
- 回看团队关键写操作
- 支撑后续排障和安全审计

## 6. 当前部署建议

当前阶段建议的部署方式：

1. 单机部署后端
2. 单独托管前端静态资源
3. 使用环境变量注入密钥
4. 每次部署前先备份 SQLite
5. 用 `/health` 做最小监控探针

不建议当前阶段直接做：

- 多实例共享 SQLite
- 在仓库中保留任何真实密钥
- 没有备份策略就直接对外开放试用
