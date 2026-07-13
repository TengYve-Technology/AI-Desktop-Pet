/**
 * 帮助命令模块
 *
 * 显示所有可用命令和使用说明
 */

function showHelp() {
    console.log(`
╔══════════════════════════════════════════════════════════════╗
║           AI Desktop Pet - 命令行工具 v1.0.0                ║
╚══════════════════════════════════════════════════════════════╝

用法:
  dtp <命令选项> <具体命令> [参数...]

命令选项:
  config      配置管理
  memory      记忆管理
  status      查看当前状态
  help, -h    显示帮助信息
  version, -v 显示版本信息

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

【config - 配置管理】

  dtp config set <配置项> <值>
    设置应用配置（保存到环境变量）

    支持的配置项:
      model      AI 模型类型
                 可选值: openai, chatglm, ollama, deepseek, qwen, siliconflow
                 示例: dtp config set model openai

      api-url    API 地址
                 示例: dtp config set api-url https://api.openai.com/v1

      api-secret API 密钥
                 会自动检测密钥格式是否符合对应模型要求
                 示例: dtp config set api-secret sk-xxxxx

      path       项目路径
                 设置 Desktop Pet 项目根目录
                 示例: dtp config set path /home/user/AI-Desktop-Pet

  dtp config show
    显示所有当前配置

  dtp config clean
    清除所有配置（会先询问确认）

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

【memory - 记忆管理】

  dtp memory clean
    清空所有记忆文件（path 配置下的 memory 文件夹）

  dtp memory zip
    压缩所有记忆文件（会调用 Python 接口）

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

【status - 状态查看】

  dtp status
    显示当前配置、服务状态等信息

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

示例:
  dtp config set model openai
  dtp config set api-secret sk-xxxxxxxxxxxxxxxx
  dtp config show
  dtp memory clean

更多信息请查看项目文档: https://github.com/your-repo/AI-Desktop-Pet
`);
}

module.exports = { showHelp };