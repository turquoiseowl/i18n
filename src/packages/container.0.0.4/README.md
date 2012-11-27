# Container
## A simple interface for function-descriptor style IoC containers

```
PM> Install-Package container
```

### Introduction
IoC is an overwrought, stinky little skeleton in the static language closet. There, I said it.
But it _is_ useful for distributing libraries when you want to allow consumers to swap the
internal componentry. Since I have to maintain a dozen or so of these, this is the small
abstraction I use to enable embedded dependency resolution. 

### Features

* Uses the function-descriptor style of registration, which, come on, is the nicest way
* Low to no ceremony
* Ships with Munq already embedded in it for a good default choice

### Usage

#### Registering and resolving

```csharp
var container = new Container();
container.Register<IFoo>(r => new Foo());
container.Register<IBar>(r => new Bar(r.Resolve<IFoo>()));

var bar = container.Resolve<IBar>();
```

#### Swapping the container

```csharp
Container.DefaultContainer = ()=> new MyAwesomeContainer();
```

### Why did you embed Munq?
		
I like Munq. It's fast enough and it's simple. But it has a few problems, 
mainly summed up [here](http://munq.codeplex.com/workitem/7131). It doesn't sound
like a big deal, but it boils down to having to wrap resolutions in a try/catch,
which is terrible for high-performing applications. The Munq that ships as the 
default container will not throw exceptions, making it more suitable for rapid
resolution and cheap tests using `CanResolve`. If the author ever patches it, I'll be 
happy to take on a dependency.