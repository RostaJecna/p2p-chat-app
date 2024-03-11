# Peer-To-Peer Chat Library

This project implements a decentralized peer-to-peer chat system where each peer autonomously discovers others on the network, exchanges chat message history, and facilitates real-time communication. This solution ensures that the chat history is distributed across the network, minimizing the risk of message loss.

## Table Of Contents

- [Overview](#peer-to-peer-chat-library)
    - [Features](#features)
    - [Architecture Overview](#architecture-overview)
        - [SettingsLoader](#1-settingsloader)
        - [Peer2PSettings](#2-peer2psettings)
        - [Peer2PManager](#3-peer2pmanager)
        - [TcpHandler](#4-tcphandler)
        - [TcpConnections](#5-tcpconnections)
        - [UdpHandler](#6-udphandler)
        - [UdpDiscovery](#7-udpdiscovery)
        - [NetworkData](#8-networkdata)
        - [Logger](#9-logger)
    - [Strengths](#strengths)
    - [Weaknesses](#weaknesses)

## Features

- **Configuration Settings**: The library is configurable, allowing fine-tuning of parameters such as network interface, communication ports, buffer size, maximum message history, command names, and timing intervals for UDP discovery and client timeouts.
- **Peer Discovery**: Peers dynamically discover others on the network using UDP-based dynamic discovery.
- **Trusted Peers Management**: The library includes a mechanism for managing trusted peers, enabling secure and controlled communication between identified and verified peers.
- **Chat History Integration**: Each peer retrieves chat message history from discovered peers, merges it with its own history, and saves incoming messages locally.
- **TCP Communication**: The system utilizes both TCP and UDP for communication. TCP handles reliable data exchange between peers.
- **Asynchronous Operations**: All communication and handling of connected clients are performed asynchronously, ensuring responsiveness and efficient resource utilization.
- **Cross-Platform**: The chat system is designed to work on multiple platforms, including Windows and Linux.

## Architecture Overview

### 1. SettingsLoader

- **Responsibility:** Loads configuration settings for the library.
- **Usage:** Initializes the configuration builder, loads settings from the specified section, and sets up network interfaces.

### 2. Peer2PSettings

- **Responsibility:** Singleton class, that holds global settings for the library.
- **Usage:** Provides a singleton instance with settings for global, communication, timing, and network configurations.

### 3. Peer2PManager

- **Responsibility:** Organizes the initialization of the Peer2P library.
- **Usage:** Loads settings, sets up network interfaces, and starts services such as TCP listener, client check, and UDP discovery.

### 4. TcpHandler

- **Responsibility:** Manages TCP communication, including listening for incoming connections and handling accepted clients.
- **Usage:** Initiates TCP listener and handles incoming TCP connections asynchronously.

### 5. TcpConnections

- **Responsibility:** Manages connected clients, their status, and performs periodic checks.
- **Usage:** Stores connected clients, checks their status, and removes inactive clients.

### 6. UdpHandler

- **Responsibility:** Handles UDP-based dynamic peer discovery and periodic checks on trusted peers.
- **Usage:** Sends and listens for UDP discovery messages, handles periodic trusted peer checks.

### 7. UdpDiscovery

- **Responsibility:** Implements UDP-based peer discovery functionality.
- **Usage:** Sends periodic discovery messages, listens for incoming discovery messages, and updates the list of trusted peers.

### 8. NetworkData

- **Responsibility:** Manages chat message data, including serialization and deserialization.
- **Usage:** Handles the serialization of chat messages and merging message histories.

### 9. Logger

- **Responsibility:** Provides logging functionality for the library.
- **Usage:** Logs messages with specified types (Error, Warning, Expecting) to the console.

## Strengths

1. **Decentralization:** The library follows a decentralized architecture, reducing reliance on a centralized server and providing robustness against failures.

2. **Asynchronous Operations:** Utilizes asynchronous programming to ensure responsiveness and efficient resource utilization, especially in handling concurrent tasks.

3. **Configuration Flexibility:** The library offers flexible configuration through the use of settings, enabling easy customization to fit diverse network environments.

4. **Dynamic Peer Discovery:** Implements dynamic peer discovery using UDP, allowing peers to autonomously discover and connect to each other.

## Weaknesses

1. **Limited Security Measures:** The library focuses on simplicity and does not prioritize advanced security measures, making it susceptible to certain security threats.

2. **Scalability Concerns:** In large networks, the decentralized approach may introduce challenges in terms of scalability. As the network grows, managing connections and chat histories may become more complex.

3. **Dependency on External Libraries:** Relies on external libraries for certain functionalities, potentially leading to versioning issues or compatibility concerns.

4. **Single Network Interface:** The library assumes a single network interface, which may not be suitable for systems with multiple network interfaces.
