# PythonAgent/src/handlers/__init__.py
from handlers.base import BaseHandler
from handlers.hello_handler import HelloHandler
from handlers.chat_handler import ChatHandler
from handlers.ping_handler import PingHandler

# 自动注册所有处理器
HANDLERS = [
    HelloHandler(),
    ChatHandler(),
    PingHandler(),
]