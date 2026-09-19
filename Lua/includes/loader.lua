import('LuaLoader', 'LuaLoader.LuaClass');
import('LuaLoader', 'LuaLoader.Helpers');

local Msg = Loader.LogMessage
local table = table

function print(...)
    local r = {}

    for i = 1, select('#', ...) do
        table.insert(r, tostring(select(i, ...)))
    end

    if #r == 0 then
        table.insert(r, 'nil')
    end

    Msg(table.concat(r, '  '))
end
