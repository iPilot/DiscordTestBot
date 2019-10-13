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
    "Database": 1,
    "JobStorageDatabase": 0
  },
  "DailyPidor": {
    "PidorLengthSeconds": 86400
  },
  "RussianRoulette": {
    "RouletteCooldownSeconds": 43200,
    "RouletteWinnerRestorationSeconds": 28800
  }
}
```
## Redis 
Storage for bot data and hangfire jobs

## Deploy script
```sh
docker stop pochinki-bot
docker rm pochinki-bot
docker rmi ipilot93/pochinki-bot:latest
docker pull ipilot93/pochinki-bot:latest
docker run -d -v /home/$user_name$/bot_confguration:/app/cfg --network bridge --name pochinki-bot ipilot93/pochinki-bot:latest --c cfg/config.json
```
