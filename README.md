# TAKE ME TO POCHINKI BOT #
Discord bot of WoW guild "Take me to Pochinki"

# Invite url
https://discordapp.com/api/oauth2/authorize?client_id=428198104559255564&permissions=470150208&scope=bot

# Docker Images
https://cloud.docker.com/repository/docker/ipilot93/pochinki-bot

# Requirements
## Configuration file example
```json
{
  "Token": "",
  "Redis": {
    "Host": "0.0.0.0:6379",
    "Timeout": 10000,
    "Database": 2,
    "JobStorageDatabase": 3
  }
}
```
## Redis 
Storage for bot data and hangfire jobs
