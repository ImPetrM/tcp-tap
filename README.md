# tcp-tap

A lightweight TCP proxy for inspecting, logging and manipulating TCP traffic between a client and a server.

## About

`tcp-tap` is a lightweight TCP proxy for development, testing and debugging. It is intended for local development and test environments, not as a production proxy.

It was originally built to help analyze and debug communication between systems using a proprietary TCP protocol. By placing tcp-tap between the client and the server, it becomes possible to inspect raw traffic, introduce artificial latency and log communication.

## How it works

`tcp-tap` acts as a transparent TCP proxy between a client and a server.

Instead of connecting directly to the destination server, the client connects to tcp-tap. The tool then establishes a second TCP connection to the real server and forwards data in both directions.

While forwarding traffic, `tcp-tap` can:
* display transferred data in hexadecimal and text form
* log traffic to a file
* introduce a fixed network delay
* introduce random latency (jitter)
* 
```
+--------+        TCP         +---------+        TCP         +--------+
| Client | <----------------> | tcp-tap | <----------------> | Server |
+--------+                    +---------+                    +--------+
                                   |
                                   +--> inspect traffic
                                   +--> log traffic
                                   +--> add latency
                                   +--> add jitter
```

To use tcp-tap, configure your client to connect to the listening port instead of the real server.

Example:

Server: `192.168.1.10:502`
`tcp-tap`: listening on `localhost:5000`
Client: connects to `localhost:5000`

tcp-tap then forwards all traffic to `192.168.1.10:502`.

## Example

Forward traffic from local port `5000` to `192.168.1.10:502`:

```bash
tcp-tap \
    --listen-port 5000 \
    --forward-host 192.168.1.10 \
    --forward-port 502
```

Add a fixed 200 ms delay:

```bash
tcp-tap \
    --listen-port 5000 \
    --forward-host 192.168.1.10 \
    --forward-port 502 \
    --delay-ms 200
```

## Usage

```text
tcp-tap [options]
```

## Options

```text
--listen-port <port>         Local port to listen on (required)
--forward-host <host>        Destination hostname or IP address (required)
--forward-port <port>        Destination port (required)

--delay-ms <ms>              Apply a fixed delay to forwarded traffic
--delay-min-ms <ms>          Minimum random delay
--delay-max-ms <ms>          Maximum random delay

--log-to-file <file>         Save captured traffic to a file

-h, --help                   Show help information
--version                    Show version information
```

## Examples

Forward traffic:

```bash
tcp-tap \
    --listen-port 5000 \
    --forward-host localhost \
    --forward-port 6000
```

Simulate network latency:

```bash
tcp-tap \
    --listen-port 5000 \
    --forward-host localhost \
    --forward-port 6000 \
    --delay-ms 150
```

Simulate network jitter:

```bash
tcp-tap \
    --listen-port 5000 \
    --forward-host localhost \
    --forward-port 6000 \
    --delay-min-ms 50 \
    --delay-max-ms 300
```

Capture traffic to a file:

```bash
tcp-tap \
    --listen-port 5000 \
    --forward-host localhost \
    --forward-port 6000 \
    --log-to-file capture.log
```

Combine latency simulation and logging:

```bash
tcp-tap \
    --listen-port 5000 \
    --forward-host 192.168.0.15 \
    --forward-port 502 \
    --delay-min-ms 20 \
    --delay-max-ms 100 \
    --log-to-file traffic.log
```

## Features

* TCP proxy for transparent forwarding
* View transferred data in hexadecimal and text format
* Fixed latency simulation
* Random latency (jitter) simulation
* Traffic logging
* Lightweight and dependency-free

## Build

```bash
dotnet build
```

## Run

```bash
dotnet run -- \
    --listen-port 5000 \
    --forward-host localhost \
    --forward-port 6000
```
