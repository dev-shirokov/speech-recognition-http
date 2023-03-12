# Speech recognition HTTP-server on NET7 using vosk/kaldi

## Presets
1. docker -> settings -> resources -> Advanced
	* change RAM to 8Gb+
2. recreation SSL-cert for Visual Studio ([link](https://learn.microsoft.com/en-us/aspnet/core/security/docker-compose-https?view=aspnetcore-7.0))
3. docker -> settings -> resources -> file sharing
	* added C:/Users/ximlr/.aspnet/https

## Vosk/kaldi server
WebSocket server `docker run -d -p 2700:2700 --name kaldi_ru alphacep/kaldi-ru:latest`
- https://alphacephei.com/vosk/install
- https://github.com/alphacep/vosk-server
- https://github.com/alphacep/vosk-api

**IMPORTANT**: the server only processes audio in the format _WAV_, codec _PCM_, channels _MONO 16bit_, frequency _8khz_

## Helpers
https://convertio.co/

## How it works