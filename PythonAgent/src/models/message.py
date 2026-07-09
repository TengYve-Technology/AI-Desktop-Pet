# PythonAgent/src/models/message.py
from dataclasses import dataclass
from typing import Optional, Dict, Any
from datetime import datetime
import uuid

@dataclass
class Message:
    id: str
    type: str
    protocol: str = "v1"
    timestamp: str = None
    data: Optional[Dict[str, Any]] = None

    def __post_init__(self):
        if not self.id:
            self.id = str(uuid.uuid4())
        if not self.timestamp:
            self.timestamp = datetime.now().isoformat()

    @classmethod
    def from_json(cls, raw: dict) -> "Message":
        return cls(
            id=raw.get("id", str(uuid.uuid4())),
            type=raw.get("type", "unknown"),
            protocol=raw.get("protocol", "v1"),
            timestamp=raw.get("timestamp"),
            data=raw.get("data")
        )

    def to_response(self, data: Any) -> dict:
        return {
            "type": "response",
            "protocol": self.protocol,
            "in_reply_to": self.id,
            "timestamp": datetime.now().isoformat(),
            "data": data
        }

    def to_error(self, error: str) -> dict:
        return {
            "type": "error",
            "protocol": self.protocol,
            "in_reply_to": self.id,
            "timestamp": datetime.now().isoformat(),
            "error": error
        }