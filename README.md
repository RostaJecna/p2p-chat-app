# Peer-To-Peer Chat Application

This is a school project focusing on peer-to-peer communication, creating a network for chatting.

## Table Of Contents

- [Overview](#overview)
    - [Projects](#projects)
        - [Peer2P](#peer2p)
        - [WebApp](#webapp)
- [Configuration](#configuration)
- [Running and Building](#running-and-building)
- [License](#license)

## Overview

The Peer-To-Peer Chat Application is a networking application designed to facilitate communication between peers in a decentralized manner. This document provides an overview of the solution's structure, projects, configuration, and how to run the application.

### Projects

The solution consists of two projects:

#### [Peer2P](./Peer2P/README.md)

This project implements a decentralized peer-to-peer chat system enabling autonomous discovery of other peers, exchange of chat message history, and real-time communication. It ensures distributed chat history across the network, minimizing message loss.

#### [WebApp](./WebApp/README.md)

This project provides a user-friendly interface for a complete chat system utilizing the Peer2P library. Users can engage in peer-to-peer chat sessions seamlessly.

## Configuration

The application can be configured by modifying the `appsettings.json` file in the `WebApp` project. The file contains configuration settings for the web server, Peer2P library, and logging.

```json
{
  "Urls": "http://0.0.0.0:8000/",
  "AllowedHosts": "*",
  "Peer2P": {
    "Global": {
      "AppPeerId": null,
      "NetworkInterfaceId": 1
    },
    "Communication": {
      "BroadcastPort": 9876,
      "MessagesBufferSize": 20480,
      "MaxMessages" : 100,
      "Commands": {
        "OnRequest": "hello",
        "OnNewMessage": "new_message"
      },
      "Status": {
        "OnResponse": "ok"
      }
    },
    "Timing": {
      "UdpDiscoveryInterval": 10000,
      "ClientTimeoutDelay": 18000
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

Settings Explanation:

- `Urls`: Specifies the URL and port on which the web server will listen for incoming requests.
- `AllowedHosts`: Determines the allowed hosts for the web server.
- `Peer2P`: Configuration settings for the Peer2P library.
    - `Global`: Global settings for the Peer2P library.
        - `AppPeerId`: Defines the unique identifier for the application peer. If set to `null`, the machine/computer name will be used.
        - `NetworkInterfaceId`: Specifies the network interface identifier to use for communication. _(Must be greater than `0`)_
            - To determine your interface ID:
                - **Windows**: Run `ipconfig` in your terminal and locate the desired interface.
                - **Linux**: Run `ip a` in your terminal and locate the desired interface.
            - **Warnings**:
                1. If multiple interfaces exist, ensure the correct one is selected.
                2. The interface must have a set IPv4 address and a valid mask for broadcast calculation.
                    - Otherwise, an error will occur.
    - `Communication`: Communication settings for the Peer2P library.
        - `BroadcastPort`: Specifies the port used for broadcasting messages to discover other peers. _(Range: `1-65535`)_
        - `MessagesBufferSize`: Specifies the size of the buffer in bytes for storing chat messages. _(Must be at least `4096` bytes)_
            - **Note**: The buffer size should be large enough to store the maximum number of messages.
            - **Warning**: Insufficient buffer size may result in lost or undelivered messages.
        - `MaxMessages`: Specifies the maximum number of messages to store in the buffer. _(Must be at least `10`)_
        - `Commands`: Defines commands used for communication between peers.
            - `OnRequest`: The command used to request peer information.
            - `OnNewMessage`: The command used to send a new chat message via TCP.
        - `Status`: Defines status messages used for communication between peers.
            - `OnResponse`: The status message used to acknowledge successful communication.
    - `Timing`: Timing settings for the Peer2P library.
        - `UdpDiscoveryInterval`: Specifies the interval for UDP discovery of other peers.
        - `ClientTimeoutDelay`: Specifies the delay for client timeout when no response is received or TCP client doesn't send new message from a peer.
    - `Logging`: Logging settings for the application.
        - `LogLevel`: Specifies the log level for the Microsoft.AspNetCore.
            - **Note**: Used for development, program debugging, or application running status.
            - For more information on this configuration section, refer to [here](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-8.0).


> This configuration allows for flexible project customization.

## Running and Building

### Prerequisites

Before running or building the project, ensure you have the following prerequisites installed on your system.

#### Windows

- [C# 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
    - You can download and install the latest version of .NET Core, which includes C#.

#### Linux

- [C# 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- GLIBC 2.38
    - Ensure your Linux distribution has GLIBC version 2.38 or later.
- GLIBCXX_3.4.32
    - Ensure your Linux distribution has GLIBCXX version 3.4.32 or later.

### Running on both systems

- Navigate to the `WebApp` project directory.
- Open the terminal.
- Type `dotnet run`.
    - It may take a while; the solution will be built as a whole application and run automatically.


### Building

You can also build the project and run it manually. For example, to run the app as a task or via `systemctl` in Linux systems, follow these steps:

1. Navigate to the [WebApp](./WebApp) project folder in your terminal.
2. Run the following command:


```sh
dotnet publish --configuration Release
```

#### Running after build

After the project is built, navigate to `./WebApp/bin/Release/net7.0/publish`.

Depending on your system, execute:

##### Windows3

- Run `WebApp.exe`

##### Linux

- Run `WebApp`

#### Additional Notes

- Ensure that you have the necessary permissions to execute the project.

## License

This project is licensed under the MIT License.