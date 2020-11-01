# Subspace

Subspace is a Web RTC API written in pure C# intended for use in .NET Core applications. 
Given there are no unmanaged dependencies, it should run on any host environment that supports the .NET Core runtime.

The main goal of this project is to provide a fully compatible WebRTC runtime without depending on any native code.

It was originally written with the intent of being able to stream IP cameras in real time on 
modern browsers without the use of any plugins. While, there are WebRTC APIs written for other languages, 
I could not find any written natively in C#, so I decided to create this project from the ground up after many hours of research into the underlying protocols.

Keep in mind that the project is currently in an **alpha** state and thus will most likely have many breaking changes
until it reaches a stable state.

## Building

1. Install [.NET Core 3.1](https://dotnet.microsoft.com/download) SDK
2. Build using `dotnet build` or load the [solution](Subspace.sln) in [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/).

## Features

### Web RTC Server

Core features implemented

1. SRTP Encryption
2. DTLS-SRTP (using [Bouncy-Castle](http://www.bouncycastle.org/csharp/) library)
3. RTP/RTCP Client
4. Stun Server
5. SDP Builder/Parser

### RTSP Client

This project includes an RTSP Client, which can be useful for proxing RTSP Streams over WebRTC.

## Roadmap
1. SRTP Decryption
2. Data Channel Support
3. Revise Web RTC library to more closely follow [WebRTC](https://www.w3.org/TR/webrtc/) spec

## Examples

See example applications under the [examples](examples) directory.

## Contributing

Feel free to submit a pull request for bug fixes. For major changes or feature requests, please open an issue first to discuss what you would like to change.

## License

[MIT](LICENSE)

## References

* https://webrtchacks.com/
* https://webrtcforthecurious.com/
* https://webrtc.org/