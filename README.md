# Alpaca
Alpaca is a .NET application development framework that brings both [Spring](http://spring.io) and the CDI (Contexts and Dependency Injection) spec from Java EE (specifically its implementation by [JBoss Weld] (http://weld.cdi-spec.org)) into .NET development.

Two main objectives thatAlpaca is trying to achieve:
* Remove ceremonies out of writing code. Alpaca aims to provide an ecosystem of modules that you can pick and plug onto your application to take care of various dirty plumbing works so you don't have to. This is 21st century. With all conveniences in life, writing code shouldn't be so hard. You shouldn't need to do anything to have fully configured IoC, ORM, auditing, security, messaging, events, logging, transactions, scheduling, health-monitoring, etc. I want to only need to say so and they just happen. [Spring-Boot](http://projects.spring.io/spring-boot/) is a great framework that currently offers such capability, and Alpaca tries to bring it to .NET environment.
* CDI -> TODO

# Getting Started
To start a new web project, start a blank ASP.NET project (do not pick any template e.g. MVC or WebAPI).

Then NuGet: (TODO: not currently published yet)
```
Install-Package Alpaca.Web
```
This does not add/modify any file in your project. At runtime Alpaca.Web will wire up ASP.NET plumbing for you with sensible defaults (e.g. OWIN, WebAPI, Dependency Injections) so no configuration or setup code needed.

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

Note that extending ApiController or IHttpController is optional. At runtime Alpaca.Web will inject that for you (see: Mixins). This is to promote lightweight POCOs and dependency injection principles, keeping testability and sanity of your code. ApiController class carries heavy infrastructural baggage that goes agains the spirit of CDI, so we encourage to keep that away from your POCO and let Alpaca.Web take care of wiring that up from behind the stage.

[A single-file sample app] (https://github.com/hendryluk/alpaca/blob/master/src/SampleWebApp/MyController.cs)

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
Alpaca modules (e.g. Alpaca.Web in this case) configure your environment with sensible defaults. If you do however want to deviate from the provided default, you can always override it by declaring your own components. For instance, to override WebApi's HttpConfiguration settings:
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

# NEXT

TODO: Scopes (Dependent, Singleton, RequestScoped, ResponseScoped, custom scopes)

TODO: Decorator and Interceptor

TODO: Mixins

TODO: Creating modules

TODO: Event<T> and [Observes]

TODO: Scheduled tasks

TODO: Alpaca.Web.Security

TODO: Messaging

TODO: Unit-testing
