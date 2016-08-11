# CQRS.Light 1.1.0.1
===
A simple CQRS and Event Sourcing Framework

Forked from the excellent aksharp/DDD.Light

![Version](https://img.shields.io/badge/version-v1.1.0.1-green.svg)

This is still very much work in progress.  If you do encounter any bugs please create an [issue](https://github.com/wallaceiam/DDD.Light/issues "Issue").  Thanks

* [CQRS.Light.Contracts](https://www.nuget.org/packages/CQRS.Light.Contracts/)
* [CQRS.Light.Core](https://www.nuget.org/packages/CQRS.Light.Core/)
* [CQRS.Light.Repository.InMemory](https://www.nuget.org/packages/CQRS.Light.Repository.InMemory/)
* [CQRS.Light.Repository.MongoDB](https://www.nuget.org/packages/CQRS.Light.Repository.MongoDB/)

## Quick Start

Install the nuget packages using the Nuget Package Manager or from the command line

```dos
PM> Install-Package CQRS.Light.Contracts
PM> Install-Package CQRS.Light.Core

PM> Install-Package CQRS.Light.Repository.InMemory
OR
PM> Install-Package CQRS.Light.Repository.MongoDB
```


### Commands

#### Command Handler

```csharp
public class MyFirstCommandHandler : CommandHandler<MyFirstCommand>
{

}
```

#### Sending a Command

```csharp
public class MyFirstCommand
{

}
```

### Events

#### Event Handler

```csharp
```

#### Raising an Event

```csharp
```
