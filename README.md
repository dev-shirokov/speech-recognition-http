# Speech recognition HTTP-server on NET7 using vosk/kaldi

## Presets
Docker Desktop -> settings -> resources -> Advanced

    change RAM to 8Gb+

## Vosk/kaldi server
Use WebSocket server `docker run -d -p 2700:2700 alphacep/kaldi-ru:latest`. In project this server run from `docker-compose`
- https://alphacephei.com/vosk/install
- https://alphacephei.com/vosk/models
- https://github.com/alphacep/vosk-server
- https://github.com/alphacep/vosk-api

**IMPORTANT**: the server only processes audio in the format **WAV**, codec **PCM**, channels **MONO 16bit**, frequency **8khz**

## How it works
Run from directory of solution `docker-compose up -d`

Attach audio file and execute request: 
```
curl -X 'POST' \
	'https://localhost/api/Speech' \
	-H 'accept: */*' \
	-H 'Content-Type: multipart/form-data' \
	-F 'file=@voxworker-voice-file_8khz.wav;type=audio/wav'
```
Response
```
{
  "items": [
    {
      "conf": 1,
      "end": 0.57,
      "start": 0,
      "word": "создать"
    },
    {
      "conf": 0.427799,
      "end": 1.062693,
      "start": 0.57,
      "word": "задачу"
    },
    {
      "conf": 1,
      "end": 1.47,
      "start": 1.08,
      "word": "купить"
    },
    {
      "conf": 1,
      "end": 1.89,
      "start": 1.47,
      "word": "творог"
    }
  ],
  "text": "создать задачу купить творог",
  "partial": null
}
```

![](res/speech-recogn.svg)

## Helpers
https://convertio.co/ - help convert files to the desired format