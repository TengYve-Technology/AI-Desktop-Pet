# PythonAgent/src/handlers/hello_handler.py
from datetime import datetime
from handlers.base import BaseHandler
from models.message import Message

class HelloHandler(BaseHandler):
    def can_handle(self, msg: Message) -> bool:
        return msg.type == "hello"

    def handle(self, msg: Message) -> dict:
        return msg.to_response({
            "greeting": "Hello Back!",
            "server_time": datetime.now().isoformat()
        })