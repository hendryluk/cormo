# Cormo
Cormo is a .NET application development framework that brings both [Spring](http://spring.io) and the CDI (Contexts and Dependency Injection) spec from Java EE (specifically its implementation by [JBoss Weld] (http://weld.cdi-spec.org)) into .NET development.

Two main objectives thatCormo is trying to achieve:
* A simpler way for application components to interact. Traditional layered architecture is often rigidly bloated with vertical stacks of indirections that become heavy and unmanageable as your codebase grows. JavaEE 6 introduced CDI as the standard glue for independent parts of your application, which uses contextual awareness to provide a loosely-coupled way to reference functionalities from different parts of your application, eliminating several layers of indirections and abstractions, resulting in a more organic and pragmatic layering. It makes many common .NET patterns and "best-practices" superfluous (interfaces, repository, read/write services, commands, handlers, event-bus). Instead the magic words are YAGNI and KISS. ([Read#1](http://www.oracle.com/au/products/database/o11java-195110.html), [Read#2](http://antoniogoncalves.org/2013/10/29/several-architectural-styles-with-java-ee-7/))
* Remove ceremonies out of writing code. Cormo aims to provide an environment to develop and host modules that you can pick and plug onto your application to speed up development and take care of plumbing works so you don't have to. Writing code shouldn't be so hard: configuring IoC, ORM, auditing, security, messaging, events, logging, transactions, scheduling, health-monitoring, etc, I want to only need to say so and they just happen. [Spring-Boot](http://projects.spring.io/spring-boot/) is a great framework that currently offers such capability, and Cormo tries to bring it to .NET environment.

# Getting Started
To start a new web project, start an *empty* ASP.NET project (do not pick any template e.g. MVC or WebAPI).

Then NuGet:
```
Install-Package Cormo.Web
```
This does not add/modify any file in your project. At runtime Cormo.Web will wire up ASP.NET plumbing for you with sensible defaults (e.g. OWIN, WebAPI, Dependency Injections) so no configuration or setup code needed.

Now with the still empty project, go ahead and write your first controller:
```csharp
[RestController]
public class MyController
{
  [Route("/hello")]
  public string Hello() { return "Hello world"; }  
}
```
That was the only file you need to have in the project. Now build and run it on the browser.

Note that extending ApiController or IHttpController is optional. At runtime Cormo.Web will inject that for you (see: Mixins). This is to promote lightweight POCOs and dependency injection principles, keeping testability and sanity of your code. ApiController class carries heavy infrastructural baggage that goes agains the spirit of CDI, so we encourage to keep that away from your POCO and let Cormo.Web take care of wiring that up from behind the stage.

[A single-file sample app] (https://github.com/hendryluk/cormo/blob/master/src/SampleWebApp/MyController.cs)

# Dependency Injection
Dependency Injection is fully configured for you. No additional setup needed.
Example:
```csharp
[RestController]
public class MyController
{
  [Inject] Greeter _greeter;   // <- Here
  
  [Route("/hello")]
  public string Hello() { return _greeter.Greet("world"); }  
}

public class Greeter
{
  public string Greet(string name) { return "Hello " + name; }
}
```
Of course you could also use interfaces but it's not required. You could also put [Inject] on constructors, fields, methods, and properties. Open generic types and constraints are also supported (TODO: example).

TODO: explain [Inject], [Produce], [Qualifier], [PostConstruct], [PreDeploy]

# Tweaking Your Plumbing
Cormo modules (e.g. Cormo.Web in this case) configure your environment with sensible defaults. If you do however want to deviate from the provided default, you can always override it by declaring your own components. For instance, to override WebApi's HttpConfiguration settings:
```csharp
public class WebApiConfiguration
{
  [Produces] HttpConfiguration GetConfiguration()
  {
    return new HttpConfiguration( /* ... */ );
  }
}
```
Here you redefine your own complete HttpConfiguration as you wish. However, in many cases you're probably happy with the default values except a few things you want to tweak. In this case, rather than redefining the component you can simply modify the existing default.
```csharp
[Configuration]
public class WebApiConfiguration
{
  [Inject] void InitConfiguration(HttpConfiguration config)
  {
     // Do something with config
  }
}
```
See "Dependency Injection" for more.

Note: Marking your class with [Configuration] ensures your class gets executed before the application starts.

# Entity Framework
[Read more here](http://github.com/hendryluk/cormo/wiki/Cormo.Data.EntityFramework)

# Further Reading
Cormo combines ideas from these following frameworks. Each comes with great documentation far beyond what Cormo has at the moment (or will ever have). Many of what you'll get from those documentations will be applicable to Cormo (or what Cormo will come to be), so check them out. Just replace the word "Bean" with "Component".
* [JBoss Weld](http://weld.cdi-spec.org)  [ [Doc](https://docs.jboss.org/weld/reference/latest/en-US/html/) ]
* [JBoss Seam 3](http://seamframework.org/Seam3/Home.html)  [ [Doc](https://docs.jboss.org/seam/latest/reference/html/) ]
* [Spring Framework](http://projects.spring.io/spring-framework/)  [ [Doc](http://docs.spring.io/spring/docs/4.2.0.BUILD-SNAPSHOT/spring-framework-reference/html/) ]
* [Spring Boot](http://projects.spring.io/spring-boot/)  [ [Doc](http://docs.spring.io/spring-boot/docs/1.2.2.BUILD-SNAPSHOT/reference/html/) ]
* JAX-RS  [ [Doc](http://docs.jboss.org/resteasy/docs/3.0.6.Final/userguide/html/) ]

# NEXT

TODO: Scopes (Dependent, Singleton, RequestScoped, ResponseScoped, custom scopes)

TODO: Decorator and Interceptor

TODO: Mixins

TODO: Creating modules

TODO: Event<T> and [Observes]

TODO: Scheduled tasks

TODO: Cormo.Web.Security

TODO: Messaging

TODO: Unit-testing
