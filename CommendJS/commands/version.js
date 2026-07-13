/**
 * 版本命令模块
 *
 * 显示命令行工具版本信息
 */

const packageJson = require('../package.json');

function showVersion() {
    console.log(`
AI Desktop Pet CLI v${packageJson.version}

模型支持:
  - OpenAI (GPT-4, GPT-3.5)
  - ChatGLM (智谱 AI)
  - Ollama (本地模型)
  - DeepSeek
  - Qwen (通义千问)
  - SiliconFlow (硅基流动)

项目地址: https://github.com/your-repo/AI-Desktop-Pet
`);
}

module.exports = { showVersion };