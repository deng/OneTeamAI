#!/usr/bin/env bash
#
# seed-demo.sh — 为 AI-Chatbot 创建一套完整的演示数据
#
# 用法:
#   bash scripts/seed-demo.sh [API_BASE_URL]
#
# 默认 API_BASE_URL=http://127.0.0.1:5078
#
# 依赖: curl, jq

set -euo pipefail

API_BASE="${1:-http://127.0.0.1:5078}"
TOKEN=""
TEAM_ID=""
FRONT_DESK_ID=""
TICKET_AI_ID=""
PROJECT_AI_ID=""
PROJECT_ID=""
CONCIERGE_ID=""
CUSTOMER_A=""
CUSTOMER_B=""
CONVERSATION_ID=""
CONVERSATION_B_ID=""

log()  { printf "\033[36m[seed]\033[0m %s\n" "$*" >&2; }
ok()   { printf "\033[32m[seed]\033[0m ✓ %s\n" "$*" >&2; }
warn() { printf "\033[33m[seed]\033[0m ⚠ %s\n" "$*" >&2; }
fail() { printf "\033[31m[seed]\033[0m ✗ %s\n" "$*" >&2; exit 1; }

# --- 前置检查 ---
check_deps() {
  command -v curl >/dev/null 2>&1 || fail "需要 curl，请先安装"
  command -v jq   >/dev/null 2>&1 || fail "需要 jq，请先安装（brew install jq / apt install jq）"
}

wait_for_api() {
  local max=30
  log "等待后端就绪 ..."
  for _ in $(seq 1 "$max"); do
    if curl -fsS "${API_BASE}/health" >/dev/null 2>&1; then
      ok "后端已就绪"
      return 0
    fi
    sleep 1
  done
  fail "后端 $API_BASE 在 ${max}s 内未就绪，请先启动 Chatbot.Api"
}

# --- API 辅助函数 ---
curl_auth()    { curl -sfS -H "Authorization: Bearer $TOKEN" "$@" 2>/dev/null; }
curl_post()    { curl_auth -H "Content-Type: application/json" -X POST -d "$2" "${API_BASE}$1"; }
curl_patch()   { curl_auth -H "Content-Type: application/json" -X PATCH -d "$2" "${API_BASE}$1"; }
curl_post_na() { curl -sfS -H "Content-Type: application/json" -X POST -d "$2" "${API_BASE}$1" 2>/dev/null; }
curl_json_get(){ curl_auth -sS "${API_BASE}$1" 2>/dev/null; }

# 从 JSON 中提取 id，非空判断后打印 ok 或 fail
extract_id_or_fail() {
  local label="$1" json="$2"
  local id; id=$(echo "$json" | jq -r '.id // empty')
  if [[ -n "$id" ]] && [[ "$id" != "null" ]]; then
    echo "$id"
  else
    local err; err=$(echo "$json" | jq -r '.error // empty' 2>/dev/null)
    fail "创建${label}失败: ${err:-$json}"
  fi
}

# --- 1. 注册演示用户 ---
create_user() {
  log "注册演示用户 ..."
  local email="demo@example.com" pass="Demo@123456"

  local body
  body=$(curl_post_na "/api/auth/register" \
    '{"email":"demo@example.com","password":"Demo@123456","displayName":"演示用户","companyName":"演示科技有限公司"}') || true

  if echo "$body" | jq -e '.accessToken' >/dev/null 2>&1; then
    TOKEN=$(echo "$body" | jq -r '.accessToken')
    ok "演示用户已创建（demo@example.com / Demo@123456）"
  elif echo "$body" | jq -e '.error == "email_exists"' >/dev/null 2>&1; then
    warn "演示用户已存在，执行登录"

    body=$(curl_post_na "/api/auth/login" \
      '{"email":"demo@example.com","password":"Demo@123456"}') || fail "登录失败"
    TOKEN=$(echo "$body" | jq -r '.accessToken')
    ok "演示用户已登录"
  else
    fail "注册失败: $(echo "$body" | jq -r '.error // empty' 2>/dev/null || echo "$body")"
  fi
}

# --- 2. 获取或创建演示团队 ---
create_team() {
  log "查找或创建演示团队 ..."

  TEAM_ID=$(curl_json_get "/api/teams/me" | jq -r '.id // empty')

  if [[ -n "$TEAM_ID" ]]; then
    ok "使用已有团队"
  else
    local body
    body=$(curl_post "/api/teams" \
      '{"name":"演示团队","description":"用于产品演示的默认团队","brandName":"Demo Ltd."}')
    TEAM_ID=$(extract_id_or_fail "团队" "$body")
    ok "团队已创建"
  fi
}

# --- 3. 创建 AI 成员 ---
create_ai_members() {
  log "创建 AI 成员 ..."

  local existing
  existing=$(curl_json_get "/api/teams/${TEAM_ID}/members")
  local ai_count
  ai_count=$(echo "$existing" | jq '[.[] | select(.memberType == "Ai")] | length')

  if [[ "$ai_count" -ge 1 ]]; then
    warn "AI 成员已存在，跳过创建"
    FRONT_DESK_ID=$(echo "$existing" | jq -r '[.[] | select(.memberType == "Ai")][0].id // empty')
    TICKET_AI_ID=$(echo "$existing" | jq -r '[.[] | select(.memberType == "Ai")][1].id // empty')
    PROJECT_AI_ID=$(echo "$existing" | jq -r '[.[] | select(.memberType == "Ai")][2].id // empty')
    return
  fi

  local body

  body=$(curl_post "/api/teams/${TEAM_ID}/members/ai" \
    '{"displayName":"小迎","jobTitle":"客户接待专员","responsibilitySummary":"负责首次接待客户、确认来意、收集联系方式，并把明确需求整理成后续动作。","templateKey":"front-desk","isAutonomous":false}')
  FRONT_DESK_ID=$(extract_id_or_fail "前台接待 AI" "$body")
  ok "前台接待 AI「小迎」已创建"

  body=$(curl_post "/api/teams/${TEAM_ID}/members/ai" \
    '{"displayName":"小协","jobTitle":"工单协调专员","responsibilitySummary":"负责判断优先级、推荐负责人、推动工单进入正确状态。","templateKey":"ticket-coordinator","isAutonomous":true}')
  TICKET_AI_ID=$(extract_id_or_fail "工单协调 AI" "$body")
  ok "工单协调 AI「小协」已创建"

  body=$(curl_post "/api/teams/${TEAM_ID}/members/ai" \
    '{"displayName":"小助","jobTitle":"项目助理","responsibilitySummary":"负责整理项目上下文、汇总会话和工单，帮助快速了解项目状态。","templateKey":"project-assistant","isAutonomous":false}')
  PROJECT_AI_ID=$(extract_id_or_fail "项目助理 AI" "$body")
  ok "项目助理 AI「小助」已创建"
}

# --- 4. 创建项目 ---
create_project() {
  log "创建演示项目 ..."

  PROJECT_ID=$(curl_json_get "/api/teams/${TEAM_ID}/projects" | jq -r '.[0].id // empty')

  if [[ -n "$PROJECT_ID" ]]; then
    warn "项目已存在，跳过创建"
    return
  fi

  local body
  body=$(curl_post "/api/teams/${TEAM_ID}/projects" \
    '{"name":"官网改版项目","description":"公司官方网站全面升级，包括 UI 重新设计、内容迁移和 SEO 优化。","stageLabel":"需求收集","summary":"正在进行用户调研和竞品分析，收集各部门需求。","riskSummary":"UI 设计人力不足，可能需要外包。","nextSteps":"① 完成调研报告  ② 确认技术方案  ③ 启动设计稿"}')
  PROJECT_ID=$(extract_id_or_fail "项目" "$body")
  ok "项目「官网改版」已创建"
}

# --- 5. 创建坐台程序 ---
create_concierge_app() {
  log "创建演示坐台程序 ..."

  CONCIERGE_ID=$(curl_json_get "/api/teams/${TEAM_ID}/concierge-apps" | jq -r '.[0].id // empty')

  if [[ -n "$CONCIERGE_ID" ]]; then
    warn "坐台程序已存在，跳过创建"
    return
  fi

  local body
  body=$(curl_post "/api/teams/${TEAM_ID}/concierge-apps" \
    '{"projectId":"'"${PROJECT_ID}"'","name":"官网在线客服","description":"官网右下角在线客服入口，用于接待网站访客。","serviceScope":"解答产品咨询、接收用户反馈、引导提交工单","welcomeMessage":"您好！欢迎访问演示官网，有什么可以帮您的？","faqScope":"产品功能、价格方案、技术支持渠道","businessHours":"工作日 09:00-18:00","channelLabel":"官网右下角","intakeGuidance":"请简要描述您的问题或需求","suggestedPrompts":"产品功能介绍\n如何收费？\n我想提交一个反馈\n联系技术支持","requireEmail":true,"requirePhoneNumber":false,"primaryAiMemberId":"'"${FRONT_DESK_ID}"'","ticketCreationPolicy":"auto_for_complex","humanHandoffPolicy":"on_request"}')
  CONCIERGE_ID=$(extract_id_or_fail "坐台程序" "$body")
  ok "坐台程序「官网在线客服」已创建"
}

# --- 6. 创建客户 ---
create_customers() {
  log "创建演示客户 ..."

  local existing
  existing=$(curl_json_get "/api/teams/${TEAM_ID}/customers")
  local count
  count=$(echo "$existing" | jq length 2>/dev/null || echo "0")

  if [[ "$count" -ge 2 ]]; then
    warn "客户已存在，跳过创建"
    return
  fi

  local body

  body=$(curl_post "/api/teams/${TEAM_ID}/customers" \
    '{"displayName":"张三","email":"zhangshan@example.com","phoneNumber":"13800138001","companyName":"创新科技有限公司","sourceLabel":"官网","tags":"VIP,技术决策者","followUpStatus":1,"notes":"对产品演示很感兴趣，希望安排一次深度演示。"}')
  CUSTOMER_A=$(extract_id_or_fail "客户张三" "$body")
  ok "客户「张三」已创建"

  body=$(curl_post "/api/teams/${TEAM_ID}/customers" \
    '{"displayName":"李四","email":"lisi@example.com","phoneNumber":"13900139002","companyName":"云端信息有限公司","sourceLabel":"转介绍","tags":"潜在客户,关注售后","followUpStatus":2,"notes":"朋友推荐了解产品，主要关注售后响应速度。"}')
  CUSTOMER_B=$(extract_id_or_fail "客户李四" "$body")
  ok "客户「李四」已创建"
}

# --- 7. 通过坐台程序创建会话（自动生成工单） ---
create_conversations() {
  log "创建演示会话 ..."

  local existing
  existing=$(curl_json_get "/api/concierge-apps/${CONCIERGE_ID}/conversations")
  local count
  count=$(echo "$existing" | jq length 2>/dev/null || echo "0")

  if [[ "$count" -ge 1 ]]; then
    warn "会话已存在，跳过创建"
    return
  fi

  local body

  body=$(curl_post "/api/concierge-apps/${CONCIERGE_ID}/conversations" \
    '{"customerId":"'"${CUSTOMER_A}"'","customerDisplayName":"张三","customerEmail":"zhangshan@example.com","initialMessage":"你好，我想了解一下你们的产品功能，特别是能不能对接我们现有的 CRM 系统。","autoCreateTicket":true}')
  CONVERSATION_ID=$(extract_id_or_fail "会话" "$body")
  ok "会话「张三 → 产品咨询」已创建（自动生成工单）"

  body=$(curl_post "/api/concierge-apps/${CONCIERGE_ID}/conversations" \
    '{"customerId":"'"${CUSTOMER_B}"'","customerDisplayName":"李四","customerEmail":"lisi@example.com","initialMessage":"我们最近系统升级后遇到了一些问题，售后工单提交后一直没有响应，能帮忙跟进一下吗？","autoCreateTicket":true}')
  CONVERSATION_B_ID=$(extract_id_or_fail "会话李四" "$body")
  ok "会话「李四 → 售后跟进」已创建（自动生成工单）"
}

# --- 8. 输出摘要 ---
print_summary() {
  log ""
  log "═══════════════════════════════════════════"
  log "  演示数据创建完成！"
  log "═══════════════════════════════════════════"
  log ""
  log "  后端地址:  $API_BASE"
  log "  登录邮箱:  demo@example.com"
  log "  登录密码:  Demo@123456"
  log ""
  log "  创建的数据："
  log "  · 1 个演示团队"
  log "  · 3 个 AI 成员（前台接待 / 工单协调 / 项目助理）"
  log "  · 1 个项目（官网改版）"
  log "  · 1 个坐台程序（官网在线客服）"
  log "  · 2 个客户（张三 / 李四）"
  log "  · 2 个会话（各自动创建了工单）"
  log ""
  log "  请打开前端页面，使用以上账号登录体验。"
  log ""
}

# --- main ---
check_deps
wait_for_api
create_user
create_team
create_ai_members
create_project
create_concierge_app
create_customers
create_conversations
print_summary
