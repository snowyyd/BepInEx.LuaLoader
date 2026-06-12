-- Load basic utility files
include('import.lua')
include('util.lua')

-- Initialize modules
json = require('JSON')
command = require('command')
hook = require('hook')
timer = require('timer')

-- Standard library extensions
include('extensions/math.lua')
include('extensions/string.lua')
include('extensions/table.lua')
include('extensions/util.lua')

-- Secure function to compile and execute live code (Lua 5.4 compliant)
local function loadlua(code)
    -- The third argument 't' specifies text mode.
    -- The fourth argument _G passes the global environment context.
    local chunk, err = load(code, "=(console)", "t", _G)
    if not chunk then
        error("Syntax Error: " .. tostring(err))
    end
    return chunk()
end

local function clua(cmd, args, argstr)
    local success, err = pcall(loadlua, argstr)

    if not success then
        if LuaLoaderLog then LuaLoaderLog.Error(err) end

        error(err)
    end
end

-- Console command registration
command.Add('lua', clua)
command.Add('lua_run', clua)
command.Add('lua_reload', function() Loader.ReloadLua() end)
