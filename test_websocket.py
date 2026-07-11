import asyncio
import websockets

async def test():
    async with websockets.connect('ws://127.0.0.1:8766') as ws:
        await ws.send('{"type":"hello","id":"test123"}')
        response = await ws.recv()
        print('Response:', response)
        
        await ws.send('{"type":"ping","id":"ping123"}')
        response = await ws.recv()
        print('Ping Response:', response)
        
        await ws.send('{"type":"chat","id":"chat123","data":{"text":"Hello from test!"}}')
        response = await ws.recv()
        print('Chat Response:', response)

asyncio.run(test())