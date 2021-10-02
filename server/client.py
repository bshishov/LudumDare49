import websockets
import logging

from server import messages as msg

logging.basicConfig(level=logging.INFO)


async def main(uri):
    try:
        await asyncio.sleep(1)
        logging.info(f'Bot is connecting to {uri}')
        async with websockets.connect(uri) as websocket:
            async def send(m: msg.PlayerMessage):
                await websocket.send(msg.player_message_to_str(m))

            logging.info(f'Bot connected')

            await send(msg.PlayerHello(
                username="123",
                token="123"
            ))

            while True:
                response = await websocket.recv()
                if not response:
                    continue

                server_message = msg.str_to_server_message(response)
                logging.info(f"Received: {server_message}")
                await asyncio.sleep(0.5)
    except Exception as err:
        logging.exception(err, exc_info=True)


if __name__ == '__main__':
    import asyncio
    asyncio.run(main("ws://localhost:6789/"))
