# Copilot Instructions

## Project Guidelines
- All existing and future methods should have start/end informational logs using ILogger. For methods with return values, add the end log immediately before the function returns. Do not log full sensitive values; prefer summaries (presence, length).
- Constructors do not need start/end ILogger logs; exclude constructors from the logging rule.