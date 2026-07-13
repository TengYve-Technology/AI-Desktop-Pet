"""
screen_capture.py —— 屏幕截图工具

基于 mss 库实现屏幕截图，支持：
  - 全屏截图 / 指定区域截图
  - 截图保存为文件 / 转为 PIL Image 对象
  - 缩略图生成（用于 VLM 输入预处理）
  - Base64 编码/解码（用于网络传输）
  - 多显示器支持
"""
import mss
import mss.tools
import os
from PIL import Image
from datetime import datetime
import numpy as np

class ScreenCapture:
    """
    屏幕截图工具

    封装 mss 库，提供截图、保存、缩放、编码等能力，
    供 PetAgent 的 screenshot 和 capture_screen 工具调用。

    Attributes:
        output_dir: 截图文件的默认保存目录
    """

    def __init__(self, output_dir: str = "data/screenshots"):
        """
        初始化截图工具

        Args:
            output_dir: 截图保存目录，不存在会自动创建
        """
        self.output_dir = output_dir
        os.makedirs(output_dir, exist_ok=True)

    def capture_screen(self, monitor: int = -1) -> Image.Image:
        """
        截取整个屏幕

        Args:
            monitor: 显示器索引，-1 表示所有显示器拼接，1 表示主显示器

        Returns:
            PIL Image 对象（RGB 格式）
        """
        with mss.mss() as sct:
            if monitor == -1:
                monitor = sct.monitors[0]
            else:
                monitor = sct.monitors[monitor]

            sct_img = sct.grab(monitor)
            img = Image.frombytes("RGB", sct_img.size, sct_img.bgra, "raw", "BGRX")
            return img

    def capture_region(self, x: int, y: int, width: int, height: int) -> Image.Image:
        """
        截取屏幕指定区域

        Args:
            x:      区域左上角 X 坐标
            y:      区域左上角 Y 坐标
            width:  区域宽度
            height: 区域高度

        Returns:
            PIL Image 对象（RGB 格式）
        """
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
        """
        截屏并保存为文件

        Args:
            filename: 保存文件名，默认按时间戳自动命名（screenshot_YYYYMMDD_HHMMSS.png）
            monitor:  显示器索引

        Returns:
            保存的文件绝对路径
        """
        img = self.capture_screen(monitor)
        
        if filename is None:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            filename = f"screenshot_{timestamp}.png"
        
        filepath = os.path.join(self.output_dir, filename)
        img.save(filepath)
        return filepath

    def get_screen_size(self) -> tuple:
        """获取主显示器的分辨率 (width, height)"""
        with mss.mss() as sct:
            primary = sct.monitors[1]
            return (primary["width"], primary["height"])

    def get_all_monitors(self) -> list:
        """获取所有显示器的信息列表（索引、分辨率、位置、是否主显示器）"""
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
        """
        截屏并缩放到指定最大尺寸（保持宽高比）

        用于 VLM 输入预处理，减小图像尺寸以降低 API 调用成本。

        Args:
            max_width:  最大宽度
            max_height: 最大高度

        Returns:
            缩放后的 PIL Image 对象
        """
        img = self.capture_screen()
        img.thumbnail((max_width, max_height))
        return img

    def get_image_bytes(self, format: str = "PNG") -> bytes:
        """
        截屏并转为字节数据

        Args:
            format: 图像格式（PNG / JPEG 等）

        Returns:
            图像的原始字节数据
        """
        img = self.capture_screen()
        import io
        buffer = io.BytesIO()
        img.save(buffer, format=format)
        return buffer.getvalue()

    @staticmethod
    def image_to_base64(img: Image.Image, format: str = "PNG") -> str:
        """
        将 PIL Image 编码为 Base64 字符串

        用于将截图通过 JSON/WebSocket 传输。

        Args:
            img:    PIL Image 对象
            format: 编码格式（PNG / JPEG 等）

        Returns:
            Base64 编码字符串
        """
        import base64
        import io
        buffer = io.BytesIO()
        img.save(buffer, format=format)
        return base64.b64encode(buffer.getvalue()).decode("utf-8")

    @staticmethod
    def base64_to_image(base64_str: str) -> Image.Image:
        """
        将 Base64 字符串解码为 PIL Image

        Args:
            base64_str: Base64 编码的图像字符串

        Returns:
            PIL Image 对象
        """
        import base64
        import io
        img_bytes = base64.b64decode(base64_str)
        return Image.open(io.BytesIO(img_bytes))