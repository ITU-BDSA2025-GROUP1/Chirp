---
title: _Chirp!_ Project Report
subtitle: ITU BDSA 2025 Group 1
author:
- "Noah Heining <nohe@itu.dk>"
- "Balder Hegermann <bamo@itu.dk>"
- "Lukas Valdemar <luvp@itu.dk>"
- "Osvald Gotthardt <osmo@itu.dk>"
- "Nicole Haut <nhau@itu.dk>"
- "Andreas Dunk <andu@itu.dk>"

numbersections: true
---

# Design and Architecture of _Chirp!_

## Domain model

_Chirp_ is built on two main entities, which are `Author` and `Cheep`.
-`Author` is representing one of our users on _Chirp_. It's an extension of `ASP.NET Core Identity`, and it enables users to send Cheeps, follow other users and like other cheeps. 
-`Cheep` is representing all of the Cheeps being sent. This contains the specific attributes that make each `Cheep` unique, such as timestamps, text and `Author` of the `Cheep`.

Below is a domain model, documenting the essenential concepts of _Chirp_, to create a conceptual framework that accurately describes the structure and dynamic of the program. 


![Illustration of the _Chirp!_ data model as UML class diagram.](docs/images/domain_model.png)

## Architecture — In the small

## Architecture of deployed application

![Illustration of the _Chirp!_ Onion Architecture as UML class diagram.](docs/images/Onion_Architecture.png)

## User activities

## Sequence of functionality/calls trough _Chirp!_

# Process

## Build, test, release, and deployment

## Team work

## How to make _Chirp!_ work locally

## How to run test suite locally

# Ethics

## License

## LLMs, ChatGPT, CoPilot, and others
