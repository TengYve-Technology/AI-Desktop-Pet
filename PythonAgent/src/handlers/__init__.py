"""
handlers 包 —— WebSocket 消息处理器注册表

所有处理器继承 BaseHandler，实现 can_handle() 和 handle() 方法。
HANDLERS 列表在 server.py 中被遍历，用于将消息路由到匹配的处理器。

当前已注册处理器：
  - HelloHandler: 处理连接握手 (type="hello")
  - ChatHandler:  处理聊天消息 (type="chat")，注意：实际被 server.py 中的 PetAgent 拦截
  - PingHandler:  处理心跳检测 (type="ping")
"""
from handlers.base import BaseHandler
from handlers.hello_handler import HelloHandler
from handlers.chat_handler import ChatHandler
from handlers.ping_handler import PingHandler

# 自动注册所有处理器（顺序决定优先级）
HANDLERS = [
    HelloHandler(),
    ChatHandler(),
    PingHandler(),
]