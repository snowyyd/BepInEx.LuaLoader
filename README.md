# BepInEx's Lua Loader

A universal BepInEx plugin that gives Unity games the ability to load and execute scripts written in the Lua language (powered by NLua).

> [!WARNING]
> **Work In Progress (WIP):** This project is currently under active development. Features are being ported, and stability fixes for Lua 5.4 compatibility are ongoing. Expect bugs and potential crashes.

*This project is a port and continuation based on the [original MelonLoader mod](https://github.com/Fukashiro-Yukari/LuaLoader) by Fukashiro-Yukari.*

---

## Licensing & Credits

> [!IMPORTANT]
> This project is open-source and licensed under the **GNU GPL v3** license.

This project contains code, implementations, and core logic from the following repositories:

- **[LuaLoader](https://github.com/Fukashiro-Yukari/LuaLoader)** (GPL-3.0) - The baseline implementation originally designed for MelonLoader by Fukashiro-Yukari.
- **[UnityExplorer](https://github.com/sinai-dev/UnityExplorer)** (GPL-3.0) - Developed by Sinai. We utilize core implementations from their Input Manager, Force Unlock Cursor logic, and Config framework.
- **[NLua](https://github.com/nlua/NLua)** (MIT) - The bridging framework between the .NET CLR and the Lua Runtime.
- **[Lua](https://www.lua.org/license.html)** (MIT) - The underlying scripting language environment.
