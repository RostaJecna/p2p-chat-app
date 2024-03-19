# Web Chat Application

This application serves as a platform for running a complete chat system with the initialization of the Peer2P library.

## Table Of Contents

- [Overview](#overview)
    - [Architecture Overview](#architecture-overview)
    
## Overview

The Web Chat Application integrates a web server with both frontend and backend components powered by the Peer2P library. It provides a user-friendly interface for users to engage in peer-to-peer chat sessions.    

## Architecture Overview


The architecture consists of the following key components:

- **Web Server**: The application utilizes a web server to serve both frontend and backend functionalities. This server hosts the chat interface and handles incoming requests.

- **Frontend**: The frontend of the application is built using HTML, CSS, and JavaScript. It includes the chat interface where users can view and send messages.

- **Backend (Peer2P Library)**: The backend functionality is provided by the Peer2P library, which handles the peer-to-peer communication aspect of the chat system. It manages connection, peer discovery, and message history synchronization among peers.

- **Controllers (REST API)**: The application implements RESTful API controllers to handle requests for getting peer messages and sending new messages to connected peers.

- **External Libraries and Frameworks**:
    - **DOMPurify**: Used for sanitizing and ensuring the security of user-generated HTML content.
    - **Bootstrap**: Provides pre-styled UI components and layout utilities for creating a responsive design.
    - **jQuery**: Simplifies DOM manipulation and event handling in JavaScript.

The chat application is structured with a single-page layout containing the chat interface, while the REST API controller manage the communication between the frontend and the Peer2P backend.

> Note: API handling logic can be found in the `wwwroot/js/site.js` file, while the `Pages` folder contains the main page `Index.cshtml` with the chat application and associated layouts and views. The `Controllers` folder houses the `MessagesController.cs` file, which implements the REST API for managing messages.