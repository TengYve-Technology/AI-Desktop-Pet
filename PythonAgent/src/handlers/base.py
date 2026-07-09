# PythonAgent/src/handlers/base.py
from abc import ABC, abstractmethod
from models.message import Message

class BaseHandler(ABC):
    @abstractmethod
    def can_handle(self, msg: Message) -> bool:
        pass

    @abstractmethod
    def handle(self, msg: Message) -> dict:
        pass