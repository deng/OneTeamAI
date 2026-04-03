# 实体模型草案

本文档对应 [product-requirements.md](/Users/dengzhizhong/data/repos/deng/AI-Chatbot/docs/product-requirements.md) 和 [integration-architecture.md](/Users/dengzhizhong/data/repos/deng/AI-Chatbot/docs/integration-architecture.md)，用于指导后续使用 EF Code First 建模。

## 1. 建模原则

1. 先定义领域实体，再补 `DbContext`、Fluent API 和 Migration。
2. 核心组织实体由本系统持有主数据。
3. 项目、客户、工单等业务实体保留本地字段，同时支持外部系统映射。
4. 不把 `Nextcloud` / `ERPNext` 写死在核心实体字段名里，而是统一使用外部映射字段。

## 2. 当前实体

当前已建立以下实体骨架：

1. `User`
2. `Team`
3. `Member`
4. `AIMemberProfile`
5. `Project`
6. `ConciergeApp`
7. `Customer`
8. `Conversation`
9. `ConversationMessage`
10. `Ticket`
11. `IntegrationConnection`

## 3. 当前枚举

当前已建立以下枚举：

1. `ExternalSystemType`
2. `RecordSourceType`
3. `MemberType`
4. `MemberRole`
5. `MemberStatus`
6. `ProjectStatus`
7. `ConciergeAppStatus`
8. `CustomerStatus`
9. `ConversationStatus`
10. `ConversationParticipantType`
11. `TicketStatus`
12. `TicketPriority`

## 4. 外部系统映射策略

以下实体继承 `ExternalMappedEntityBase`：

1. `Project`
2. `Customer`
3. `Ticket`

统一字段包括：

1. `SourceType`
2. `ExternalSystemType`
3. `ExternalId`
4. `ExternalRef`

这样后续既可以接 `ERPNext`，也可以扩展到同类系统，而不用重命名核心实体字段。

## 5. 当前持久化结构

当前已补充：

1. `AppDbContext`
2. 每个核心实体对应的 Fluent API 配置类
3. 基础索引和删除行为约束
4. 设计时 `AppDbContextFactory`
5. SQLite 作为当前 MVP 默认 provider

当前策略：

1. 一对一关系：
   `Member` -> `AIMemberProfile`
2. 一对多关系：
   `Team` -> `Members / Projects / ConciergeApps / IntegrationConnections`
3. 业务归属关系：
   `Project / Customer / Conversation / Ticket` 均保留团队或项目级归属
4. 外部映射索引：
   `Project / Customer / Ticket` 对 `ExternalSystemType + ExternalId` 建索引

## 6. 当前 provider 策略

当前默认使用 SQLite，原因如下：

1. MVP 本地开发最轻量
2. 适合快速生成首个 migration
3. 不影响后续切换到 PostgreSQL 等更正式数据库

当前连接串来源：

1. `appsettings.json` -> `chatbot.db`
2. `appsettings.Development.json` -> `chatbot.dev.db`
3. 设计时工厂 -> `chatbot.design.db`

## 7. 下一步建议

下一步建议按以下顺序继续：

1. 生成首个 Migration
2. 视需要增加种子数据或初始化流程
3. 在应用层增加仓储 / 查询服务
4. 再开始把业务接口逐步迁移到实体层
