# PythonAgent/src/server.py
import asyncio
import json
import websockets
from handlers import HANDLERS
from models.message import Message


async def handle_client(websocket):
    async for raw_message in websocket:
        try:
            data = json.loads(raw_message)
            msg = Message.from_json(data)

            # 查找能处理该消息的处理器
            response = None
            for handler in HANDLERS:
                if handler.can_handle(msg):
                    response = handler.handle(msg)
                    break

            if response is None:
                response = msg.to_error("No handler for this message type")

            await websocket.send(json.dumps(response))

        except json.JSONDecodeError:
            error = {"type": "error", "error": "Invalid JSON"}
            await websocket.send(json.dumps(error))


async def main():
    host = "127.0.0.1"
    port = 8765
    print(f"Server running on ws://{host}:{port}")
    async with websockets.serve(handle_client, host, port):
        await asyncio.Future()


if __name__ == "__main__":
    asyncio.run(main())