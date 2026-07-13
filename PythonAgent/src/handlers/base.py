"""
base.py —— 消息处理器基类

定义处理器的统一接口：
  - can_handle(msg): 判断是否能处理该消息
  - handle(msg):     执行处理逻辑，返回响应字典

新增处理器时，只需继承 BaseHandler 并实现这两个方法，
然后在 handlers/__init__.py 的 HANDLERS 列表中注册即可。
"""
from abc import ABC, abstractmethod
from models.message import Message

class BaseHandler(ABC):
    """消息处理器抽象基类，所有处理器必须实现 can_handle 和 handle"""

    @abstractmethod
    def can_handle(self, msg: Message) -> bool:
        """
        判断当前处理器是否能处理该消息

        Args:
            msg: 接收到的 Message 对象

        Returns:
            True 表示可以处理，False 表示不能
        """
        pass

    @abstractmethod
    def handle(self, msg: Message) -> dict:
        """
        处理消息并返回响应

        Args:
            msg: 需要处理的 Message 对象

        Returns:
            响应字典，将通过 WebSocket 发回客户端
        """
        pass