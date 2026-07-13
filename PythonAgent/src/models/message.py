"""
message.py —— WebSocket 消息模型

定义统一的消息数据结构，用于 Python 服务端内部的 JSON 解析和响应构造。

消息格式约定：
  请求: {id, type, protocol, timestamp, data}
  响应: {type:"response", protocol, in_reply_to, timestamp, data}
  错误: {type:"error", protocol, in_reply_to, timestamp, error}

与计划书中的 JSON-RPC 2.0 不同，当前使用自定义的轻量格式。
"""
from dataclasses import dataclass
from typing import Optional, Dict, Any
from datetime import datetime
import uuid

@dataclass
class Message:
    """
    WebSocket 消息数据类

    Attributes:
        id:        消息唯一标识，用于请求-响应匹配
        type:      消息类型（hello / ping / chat / response / error / notification）
        protocol:  协议版本号，当前为 "v1"
        timestamp: 消息时间戳（ISO 8601 格式）
        data:      消息携带的业务数据字典
    """
    id: str
    type: str
    protocol: str = "v1"
    timestamp: str = None
    data: Optional[Dict[str, Any]] = None

    def __post_init__(self):
        """自动生成 id 和 timestamp（若未提供）"""
        if not self.id:
            self.id = str(uuid.uuid4())
        if not self.timestamp:
            self.timestamp = datetime.now().isoformat()

    @classmethod
    def from_json(cls, raw: dict) -> "Message":
        """
        从 JSON 字典构造 Message 对象

        Args:
            raw: 从 WebSocket 接收到的原始 JSON 解析后的字典

        Returns:
            构造好的 Message 实例
        """
        return cls(
            id=raw.get("id", str(uuid.uuid4())),
            type=raw.get("type", "unknown"),
            protocol=raw.get("protocol", "v1"),
            timestamp=raw.get("timestamp"),
            data=raw.get("data")
        )

    def to_response(self, data: Any) -> dict:
        """
        构造成功响应字典

        Args:
            data: 响应携带的业务数据

        Returns:
            符合协议规范的响应字典
        """
        return {
            "type": "response",
            "protocol": self.protocol,
            "in_reply_to": self.id,
            "timestamp": datetime.now().isoformat(),
            "data": data
        }

    def to_error(self, error: str) -> dict:
        """
        构造错误响应字典

        Args:
            error: 错误描述信息

        Returns:
            符合协议规范的错误响应字典
        """
        return {
            "type": "error",
            "protocol": self.protocol,
            "in_reply_to": self.id,
            "timestamp": datetime.now().isoformat(),
            "error": error
        }