"""
server.py —— WebSocket 服务端入口

启动一个基于 websockets 库的 WebSocket 服务器，监听 Unity 前端的连接。
职责：
  1. 接收客户端消息，按 type 字段路由到对应处理器
  2. 对 chat 类型消息，直接交给 PetAgent 处理（调用 LLM 生成回复）
  3. 对其它类型消息，遍历 HANDLERS 注册表寻找匹配处理器
  4. 处理异常（JSON 解析失败、无匹配处理器等），返回统一错误格式

注意：当前使用 websockets 库，仅支持 WebSocket 协议，不支持浏览器 HTTP 访问。
"""

import asyncio
import json
import websockets
import yaml
import os
from dotenv import load_dotenv
from agent.core import PetAgent
from handlers import HANDLERS
from models.message import Message

# 加载 .env 环境变量（API Key 等敏感配置）
load_dotenv()

def load_config() -> dict:
    """从 config.yaml 加载项目配置，若文件不存在则返回空字典"""
    config_path = os.path.join(os.path.dirname(__file__), "..", "config.yaml")
    if os.path.exists(config_path):
        with open(config_path, "r", encoding="utf-8") as f:
            return yaml.safe_load(f)
    return {}

config = load_config()
# 初始化宠物 Agent（包含 LLM、记忆、感知、工具等子系统）
agent = PetAgent(config)

async def handle_client(websocket):
    """
    处理单个客户端连接的生命周期

    流程：持续接收消息 → 解析为 Message 对象 → 路由到处理器 → 返回响应
    chat 类型直接走 PetAgent，其它类型走 HANDLERS 注册表。
    """
    print(f"[Server] Client connected")

    async for raw_message in websocket:
        try:
            data = json.loads(raw_message)
            msg = Message.from_json(data)

            response = None

            # chat 类型直接交给 PetAgent 处理（调用 LLM 生成回复）
            if msg.type == "chat":
                chat_data = msg.data or {}
                result = agent.process_message("chat", {"text": chat_data.get("text", "")})
                response = msg.to_response(result)
            else:
                # 其它类型遍历 HANDLERS 注册表，寻找 can_handle 返回 True 的处理器
                for handler in HANDLERS:
                    if handler.can_handle(msg):
                        response = handler.handle(msg)
                        break

            # 没有匹配的处理器，返回错误
            if response is None:
                response = msg.to_error("No handler for this message type")

            await websocket.send(json.dumps(response))

        except json.JSONDecodeError:
            # JSON 解析失败
            error = {"type": "error", "error": "Invalid JSON"}
            await websocket.send(json.dumps(error))
        except Exception as e:
            # 其它未预期异常
            error = {"type": "error", "error": str(e)}
            await websocket.send(json.dumps(error))

    print(f"[Server] Client disconnected")

async def main():
    """启动 WebSocket 服务器，阻塞运行"""
    host = config.get("server", {}).get("host", "127.0.0.1")
    port = config.get("server", {}).get("port", 8766)
    print(f"[Server] Starting WebSocket server on ws://{host}:{port}")
    print(f"[Server] LLM initialized: {agent.get_status()}")

    async with websockets.serve(handle_client, host, port):
        await asyncio.Future()  # 永久阻塞，保持服务运行

if __name__ == "__main__":
    asyncio.run(main())