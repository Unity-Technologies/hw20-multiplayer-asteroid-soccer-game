local nk = require("nakama")

local function makematch(context, matched_users)
  -- print matched users
  for _, user in ipairs(matched_users) do
    local presence = user.presence
    nk.logger_info(("Matched user '%s' named '%s'"):format(presence.user_id, presence.username))
    for k, v in pairs(user.properties) do
      nk.logger_info(("Matched on '%s' value '%s'"):format(k, v))
    end
  end

  local modulename = "hello_world"
  local setupstate = { invited = matched_users }
  local matchid = nk.match_create(modulename, setupstate)
  return matchid
end

nk.register_matchmaker_matched(makematch)