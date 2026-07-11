import asyncio
import websockets

async def test_chat():
    async with websockets.connect('ws://127.0.0.1:8766') as ws:
        print("Connected to server")
        
        await ws.send('{"type":"hello","id":"test123"}')
        response = await ws.recv()
        print('Hello Response:', response)
        
        await ws.send('{"type":"ping","id":"ping123"}')
        response = await ws.recv()
        print('Ping Response:', response)
        
        await ws.send('{"type":"chat","id":"chat123","data":{"text":"Hello, how are you?"}}')
        response = await ws.recv()
        print('Chat Response:', response)

asyncio.run(test_chat())