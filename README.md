# sample-masstransit-kubernetes

This is a fork of [sample-masstransit](https://github.com/hgmauri/sample-masstransit]). sample-masstransit is a simple C# project with a Web API client and 3 workers. This project is a good playground to try-out and learn masstransit.

## Why this fork?

Changes in this “fork” could contradict the original idea of a minimalist example for mass transit.

Changes:

- Updated Readme
- `appsetting.json` for different configurations. This contradicts the original idea.



## Run in Visual Studio

RabbitMQ runs in docker. A local installation is not required.

Requires:

- Docker Desktop
- Visual Studio
- Windows PowerShell

Installation:

1. `docker pull rabbitmq:3.13.7-management`
2. `docker run --name rabbitmq -d -p 15672:15672 -p 5672:5672 rabbitmq:3.13.7-management`
3. Open http://localhost:15672. login is guest/guest

Run in Visual Studio

1. Open sample-masstransit.sln in Visual Studio

2. Set Sample.Masstransit.Worker as startup project

3. Start Sample.Masstransit.Worker with debugging

   Log line should be: "Bus started: rabbitmq://localhost/"

   RabbitMQ should have 1 connection

4. Open sample-masstransit.sln in a second Visual Studio

5. Set Sample.Masstransit.WebApi as startup project

6. Start Sample.Masstransit.WebApi with debugging

   Log line should be: "Bus started: rabbitmq://localhost/"

   RabbitMQ should have 2 connections

   SwaggerUI should appear in the Web browser

Debug in Visual Studio

1. Add breakpoint in Sample.Masstransit.WebApi/ClientController/Post

2. Sample.Masstransit.Worker/QueueSendEmailConsumer/Consume

3. In the Swagger UI, execute a "/client post"

   Breakpoints should be hit



## Run in docker

Requires

- Docker Desktop
- Windows PowerShell

Build

1. `cd <project-root-folder>`

2. `docker build -f .\src\Sample.Masstransit.Worker\Dockerfile -t masstransit-worker .`

    image masstransit-worker should be visible in docker

3. `docker build -f .\src\Sample.Masstransit.WebApi\Dockerfile -t masstransit-webapi .`

    image masstransit-webapi should be visible in docker

  Run

1. `docker run --name rabbitmq -d -p 15672:15672 -p 5672:5672 rabbitmq:3.13.7-management`

    Note: Not required if already running

2. `docker run --name masstransit-worker -d -e DOTNET_ENVIRONMENT=docker masstransit-worker`

   container masstransit-worker should run in docker

   Log line should be "Bus started: rabbitmq://host.docker.internal/"

3. `docker run --name masstransit-webapi -d -e ASPNETCORE_ENVIRONMENT=docker -p 8080:8080 masstransit-webapi`

    container masstransit-webapi should run in docker
    Log line should be "Bus started: rabbitmq://host.docker.internal/"

Test

1. Open http://localhost:8080/swagger/index.html

   Note: I have enabled swagger for all configurations, not just for development

2. In the Swagger UI, execute a "/client post"

   Log line in masstransit-webapi should be "Evento enviado: ClientInsertedEvent - string - string"

   Log line in masstransit-worker should be "Email enviado com sucesso: string"

## Run in MiniKube

Required

- minikube
- docker containers rabbitmq, masstransit-webapi, masstransit-worker stopped

Install

1. `minikube start`

2. `start minikube dashboard`

   Dashboard should open in Web browser

3. `minikube addons enable ingress`

   Note: not sure if that is required for this example

4. `kubectl create namespace messaging`

5. `kubectl create namespace sample`

4. `kubectl create deployment rabbitmq --image=rabbitmq:3.13.7`

   rabbitmq should be seen in Workloads/Deployments

4. `kubectl create -f .\rabbitmq-service.yaml`

5. `kubectl create -f .\src\Sample.Masstransit.WebApi\deploy.yaml`

   masstransit-webapi should be seen in Workloads\Deployments

6. `kubectl create -f .\src\Sample.Masstransit.WebApi\service.yaml`

   webapi should be seen in Service\Services

7. `kubectl create -f .\src\Sample.Masstransit.Worker\deploy.yaml`

    masstransit-worker should be seen in Workloads\Deployments
    
8. `start minikube tunnel`

    webapi should become status green in Service\Services

9. Open http://localhost:8080/swagger/

10. In the Swagger UI, execute a "/client post"

    Log line in Workloads\Pods\masstransit-webapi-<id> should be "Evento enviado: ClientInsertedEvent - string - string"

    Log line in Workloads\Pods\masstransit-worker-<id> should be "Email enviado com sucesso: string"



## Licence and Copyright

There is no copyright notice in the original source code. All my changes and additions are public domain. I assume that the source code is public, but I have not clarified this with the author.

