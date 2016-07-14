# Simple Injector

[![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/simpleinjector/SimpleInjector?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) [![Build status](https://ci.appveyor.com/api/projects/status/2k9ududhkqqufk76?svg=true)](https://ci.appveyor.com/project/simpleinjector/simpleinjector) [![Coverage Status](https://coveralls.io/repos/simpleinjector/SimpleInjector/badge.svg)](https://coveralls.io/github/simpleinjector/SimpleInjector) [![NuGet](https://img.shields.io/nuget/v/SimpleInjector.svg)](https://www.nuget.org/packages/simpleinjector)

_**To get a high level overview of Simple Injector, please [visit our website](https://simpleinjector.org/)**. And did you know there's a [Simple Injector blog](https://simpleinjector.org/blog)?_

The goal of **Simple Injector** is to provide .NET application developers with an easy, flexible, and fast **Dependency Injection library** that promotes best practice to steer developers towards the pit of success.

Many of the existing DI libraries have a big complicated legacy API or are new, immature, and lack features often required by large scale development projects. Simple Injector fills this gap by supplying a simple implementation with a carefully selected and complete set of features. File and attribute based configuration methods have been abandoned (they invariably result in brittle and maintenance heavy applications), favoring simple code based configuration instead. This is enough for most applications, requiring only that the configuration be performed at the start of the program. The core library contains many features for all your [advanced](https://simpleinjector.readthedocs.org/en/latest/advanced.html) needs.

The following platforms are supported:

* *.NET 4.0* and up.
* *Silverlight 4* and up.
* *Windows Phone 8*.
* *Windows Store Apps*.
* *Mono*.
* *.NET Core*.

> Simple Injector is carefully designed to run in **partial / medium trust**, and it is fast; blazingly fast.

Getting started
===============

The easiest way to get started is by installing [the available NuGet packages](https://www.nuget.org/packages?q=Author%3ASimpleInjector-Contributors&sortOrder=package-download-count) and if you're not a NuGet fan then follow these steps:

* Download the latest **runtime library** from: https://simpleinjector.org/download;
* Unpack the downloaded `.zip` file;
* Add the **SimpleInjector.dll** to your start-up project by right-clicking on a project in the Visual Studio solution explorer and selecting 'Add Reference...'.
* Add the **using SimpleInjector;** directive on the top of the code file where you wish to configure the application.
* Look at the [Using](https://simpleinjector.readthedocs.org/en/latest/using.html) section in the documentation for how to configure and use Simple Injector.
* Look at the [More Information](https://simpleinjector.readthedocs.org/en/latest/quickstart.html#quickstart-more-information) section to learn more or if you have any questions.

A Quick Example
===============

Dependency Injection
--------------------

The general idea behind Simple Injector (or any DI library for that matter) is that you design your application around loosely coupled components using the [dependency injection pattern](https://en.wikipedia.org/wiki/Dependency_injection) while adhering to the [Dependency Inversion Principle](https://en.wikipedia.org/wiki/Dependency_inversion_principle). Take for instance the following `UserController` class in the context of an ASP.NET MVC application:

> **Note:** Simple Injector works for many different technologies and not just MVC. Please see the [integration](https://simpleinjector.readthedocs.org/en/latest/integration.html) for help using Simple Injector with your technology of choice.

``` c#

public class UserController : Controller {
    private readonly IUserRepository repository;
    private readonly ILogger logger;

    // Use constructor injection for the dependencies
    public UserController(IUserRepository repository, ILogger logger) {
        this.repository = repository;
        this.logger = logger;
    }

    // implement UserController methods here:
    public ActionResult Index() {
        this.logger.Log("Index called");
        return View(this.repository.GetAll());
    }
}
    
public class SqlUserRepository : IUserRepository {
    private readonly ILogger logger;

    // Use constructor injection for the dependencies
    public SqlUserRepository(ILogger logger) {
        this.logger = logger;
    }
    
    public User GetById(Guid id) {
        this.logger.Log("Getting User " + id);
        // retrieve from db.
    }
}
```

The `UserController` class depends on the `IUserRepository` and `ILogger` interfaces. By not depending on concrete implementations, we can test `UserController` in isolation. But ease of testing is only one of a number of things that Dependency Injection gives us. It also enables us, for example, to design highly flexible systems that can be completely composed in one specific location (often the startup path) of the application.

Introducing Simple Injector
---------------------------

Using Simple Injector, the configuration of the application using the `UserController` and `SqlUserRepository` classes shown above, might look something like this:

``` c#
protected void Application_Start(object sender, EventArgs e) {
    // 1. Create a new Simple Injector container
    var container = new Container();

    // 2. Configure the container (register)
    container.Register<IUserRepository, SqlUserRepository>(Lifestyle.Transient);

    container.Register<ILogger, MailLogger>(Lifestyle.Singleton);

    // 3. Optionally verify the container's configuration.
    container.Verify();

    // 4. Register the container as MVC3 IDependencyResolver.
    DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
}
```

> **Tip**: If you start with a MVC application, use the [NuGet Simple Injector MVC Integration Quick Start package](https://nuget.org/packages/SimpleInjector.MVC3).

The given configuration registers implementations for the `IUserRepository` and `ILogger` interfaces. The code snippet shows a few interesting things. First of all, you can map concrete instances (such as `SqlUserRepository`) to an interface or base type. In the given example, every time you ask the container for an `IUserRepository`, it will always create a new `SqlUserRepository` on your behalf (in DI terminology: an object with a **Transient** lifestyle).

The seconds registration maps the `ILogger` interface to a `MailLogger` implementation. This `MailLogger` is registered with the **Singleton** lifestyle; only one instance of `MailLogger` will ever be created by the `Container`.

> **Note**: We did not register the `UserController`, because the `UserController` is a concrete type, Simple Injector can implicitly create it (as long as its dependencies can be resolved).
    
Using this configuration, when a `UserController` is requested, the following object graph is constructed:

``` c#
new UserController(
    new SqlUserRepository(
        logger),
    logger);
```

Note that object graphs can become very deep. What you can see is that not only `UserController` contains dependencies, so does `SqlUserRepository`. In this case `SqlUserRepository` itself contains an `ILogger` dependency itself. Simple Injector will not only resolve the dependencies of `UserController` but will instead build a whole tree structure of any level deep for you. 

And this is all it takes to start using Simple Injector. Design your classes around the SOLID principles and the dependency injection pattern (which is actually the hard part) and configure them during application initialization. Some frameworks (such as ASP.NET MVC) will do the rest for you, other frameworks (like ASP.NET Web Forms) will need a little bit more work. See the [integration guide](https://simpleinjector.readthedocs.org/en/latest/integration.html) for examples of many common frameworks.

> Please go to the [using](https://simpleinjector.readthedocs.org/en/latest/using.html) section in the [documentation](https://simpleinjector.readthedocs.org/) to see more examples.

More information
================

For more information about Simple Injector please visit the following links: 

* [using](https://simpleinjector.readthedocs.org/en/latest/using.html) will guide you through the Simple Injector basics.
* The [lifetimes](https://simpleinjector.readthedocs.org/en/latest/lifetimes.html) page explains how to configure lifestyles such as *transient*, *singleton*, and many others.
* See the [Reference library](https://simpleinjector.org/ReferenceLibrary/) for the complete API documentation of the latest version.
* See the [integration guide](https://simpleinjector.readthedocs.org/en/latest/integration.html) for more information about how to integrate Simple Injector into your specific application framework.
* For more information about dependency injection in general, please visit [this page on Stackoverflow](https://stackoverflow.com/tags/dependency-injection/info).
* If you have any questions about how to use Simple Injector or about dependency injection in general, the experts at [Stackoverflow.com](https://stackoverflow.com/questions/ask?tags=simple-injector%20ioc-container%20dependency-injection%20.net%20c%23) are waiting for you.
* For all other Simple Injector related question and discussions, such as bug reports and feature requests, the [Simple Injector discussion forum](https://simpleinjector.org/forum) will be the place to start.

**Happy injecting!**
