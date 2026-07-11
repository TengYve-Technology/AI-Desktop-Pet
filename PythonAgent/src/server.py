import asyncio
import json
import websockets
import yaml
import os
from dotenv import load_dotenv
from agent.core import PetAgent
from handlers import HANDLERS
from models.message import Message

load_dotenv()

def load_config() -> dict:
    config_path = os.path.join(os.path.dirname(__file__), "..", "config.yaml")
    if os.path.exists(config_path):
        with open(config_path, "r", encoding="utf-8") as f:
            return yaml.safe_load(f)
    return {}

config = load_config()
agent = PetAgent(config)

async def handle_client(websocket):
    print(f"[Server] Client connected")
    
    async for raw_message in websocket:
        try:
            data = json.loads(raw_message)
            msg = Message.from_json(data)
            
            response = None
            
            if msg.type == "chat":
                chat_data = msg.data or {}
                result = agent.process_message("chat", {"text": chat_data.get("text", "")})
                response = msg.to_response(result)
            else:
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
        except Exception as e:
            error = {"type": "error", "error": str(e)}
            await websocket.send(json.dumps(error))
    
    print(f"[Server] Client disconnected")

async def main():
    host = config.get("server", {}).get("host", "127.0.0.1")
    port = config.get("server", {}).get("port", 8766)
    print(f"[Server] Starting WebSocket server on ws://{host}:{port}")
    print(f"[Server] LLM initialized: {agent.get_status()}")
    
    async with websockets.serve(handle_client, host, port):
        await asyncio.Future()

if __name__ == "__main__":
    asyncio.run(main())