import mss
import mss.tools
import os
from PIL import Image
from datetime import datetime
import numpy as np

class ScreenCapture:
    def __init__(self, output_dir: str = "data/screenshots"):
        self.output_dir = output_dir
        os.makedirs(output_dir, exist_ok=True)

    def capture_screen(self, monitor: int = -1) -> Image.Image:
        with mss.mss() as sct:
            if monitor == -1:
                monitor = sct.monitors[0]
            else:
                monitor = sct.monitors[monitor]

            sct_img = sct.grab(monitor)
            img = Image.frombytes("RGB", sct_img.size, sct_img.bgra, "raw", "BGRX")
            return img

    def capture_region(self, x: int, y: int, width: int, height: int) -> Image.Image:
        with mss.mss() as sct:
            monitor = {
                "top": y,
                "left": x,
                "width": width,
                "height": height
            }
            sct_img = sct.grab(monitor)
            img = Image.frombytes("RGB", sct_img.size, sct_img.bgra, "raw", "BGRX")
            return img

    def save_screenshot(self, filename: str = None, monitor: int = -1) -> str:
        img = self.capture_screen(monitor)
        
        if filename is None:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            filename = f"screenshot_{timestamp}.png"
        
        filepath = os.path.join(self.output_dir, filename)
        img.save(filepath)
        return filepath

    def get_screen_size(self) -> tuple:
        with mss.mss() as sct:
            primary = sct.monitors[1]
            return (primary["width"], primary["height"])

    def get_all_monitors(self) -> list:
        with mss.mss() as sct:
            monitors = []
            for i, monitor in enumerate(sct.monitors):
                monitors.append({
                    "index": i,
                    "width": monitor["width"],
                    "height": monitor["height"],
                    "left": monitor.get("left", 0),
                    "top": monitor.get("top", 0),
                    "is_primary": i == 1
                })
            return monitors

    def capture_and_resize(self, max_width: int = 640, max_height: int = 480) -> Image.Image:
        img = self.capture_screen()
        img.thumbnail((max_width, max_height))
        return img

    def get_image_bytes(self, format: str = "PNG") -> bytes:
        img = self.capture_screen()
        import io
        buffer = io.BytesIO()
        img.save(buffer, format=format)
        return buffer.getvalue()

    @staticmethod
    def image_to_base64(img: Image.Image, format: str = "PNG") -> str:
        import base64
        import io
        buffer = io.BytesIO()
        img.save(buffer, format=format)
        return base64.b64encode(buffer.getvalue()).decode("utf-8")

    @staticmethod
    def base64_to_image(base64_str: str) -> Image.Image:
        import base64
        import io
        img_bytes = base64.b64decode(base64_str)
        return Image.open(io.BytesIO(img_bytes))