Aşağıda yeni bir proje nasıl oluşturulur.
Gerekli nugetler nasıl eklenir ve basic bir TODO app nasıl yapılır adım adım anlatımı mevcuttur.



//--------------


Create the project
First create a folder in a spot on your machine where you want your project to live. I’m going to put mine on ~/Documents/Workspace folder.
From Terminal, run the following commands:
mkdir TodoApi
dotnet new webapi -o TodoApi
code TodoApi
The TodoApi folder opens in Visual Studio Code (VS Code).


￼
TodoApi workspace in VS Code
Select Yes to the Warn message “Required assets to build and debug are missing from ‘TodoApi’. Add them?”


￼
Warning pop-up to add required assets to build and debug
Press Debug (F5) to build and run the program. If 500x port is already bound by other process, server will crash with exception having message “Failed to bind to address”


￼
Failed to bind address by server
To resolve it,
* Open launch.json in your workspace
* Go to NET Core Launch (web) config
* Specify the unused port in “args” array (command-line args). I use port 5002.


￼
Unused port in args
Error has gone 😀. WebAPI is launched in your default browser with URL
http://localhost:5002
4. Append endpoint api/values in browser as mentioned in launchSettings.json’s project profile’s launchUrl key


￼
launchUrl key of profiles
5. The following output is displayed:
["value1","value2"]


￼
output of WebAPI
Adding NuGet Packages
* Open your integrated terminal


￼
Integrated terminal of VS Code
2. Execute the following commands
.NET Core 2.1 installs Entity Framework Core 2.1 by default. However, if you install PostgreSQL package through NuGet package manager, it’ll install PostgreSQL 2.1.1. It results in version conflict between PostgreSQL 2.1.1 & Entity Framework Core 2.1.0.


￼
.NET Core 2.1
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 2.1.0
dotnet add package NpgSql.EntityFrameworkCore.PostgreSQL.Design
Create Your Database
Open pgAdmin 4.x


￼
Add New Server Dialog of pgAdmin
In the Add New Server Dialog, enter a server name in the General tab, I chose “TodoApi”. In the Connection tab you can enter localhost for the hostname and type a password of your choice. For this example my password will be password. You can leave the other fields as their default values.


￼
database server config
Add a model class
In VS Code create a Models Folder in the root of your project.
A model is an object representing the data in your app. In this case, the only model is a to-do item. Inside of the Models folder create a TodoItem.cs class with the following code:
namespace TodoApi.Models {     
public class TodoItem   
{        
 
public long Id { get; set; }        
public string Name { get; set; }         
public bool IsComplete { get; set; }     
  }
}


￼
TodoItem.cs
Create the database context
The database context is the main class that coordinates Entity Framework functionality for a given data model. You create this class by deriving from the Microsoft.EntityFrameworkCore.DbContext class.
Add a TodoContext.cs class in the Models folder:
using Microsoft.EntityFrameworkCore;  
namespace TodoApi.Models {     
public class TodoContext : DbContext     
  {         
    public TodoContext(DbContextOptions<TodoContext> options)                            : base(options)         
{         
}       
    public DbSet<TodoItem> TodoItems { get; set; }     
  
   } 
}
Register the database context
In this step, the database context is registered with the dependency injectioncontainer. Services (such as the DB context) that are registered with the dependency injection (DI) container are available to the controllers.
Register the DB context with the service container using the built-in support for dependency injection. Replace the contents of the Startup.cs file with the following code:
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
namespace TodoApi
{
public class Startup
{
public Startup(IConfiguration configuration)
{
Configuration = configuration;
}
public IConfiguration Configuration { get; }
// This method gets called by the runtime. Use this method to add services to the container.
public void ConfigureServices(IServiceCollection services)
{
// Add framework services.
services.AddDbContext<TodoContext>(options =>
options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
}
// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
if (env.IsDevelopment())
{
app.UseDeveloperExceptionPage();
}
else
{
app.UseHsts();
}
app.UseHttpsRedirection();
app.UseMvc();
}
}
}
Change Connection String
Next open appsettings.json file, in this file you will add your PostgreSQL connection string.
"ConnectionStrings": {
"DefaultConnection":
"Host=localhost;Port=5432;Username=postgres;Password=password;Database=TodoList;"
}
Run database Migration
It’s finally time to add your migration and update your database. We are going to call our migration “initial” but you can call it whatever you like.
In the VS Code integrated terminal execute the command: dotnet ef migrations add initial


￼
output of dotnet ef migrations add initial
This should have generated some files under a new folder in your project called Migrations


￼
Migrations folder
Lastly to update the database execute the command: dotnet ef database update


￼
output of database update in terminal
Your database should now be updated in PgAdmin. You may have to refresh the database to see the changes.


￼
Database & Tables in PostgreSQL
Add a controller
In the Controllers folder, create a class named TodoController. Copy the following code:
using Microsoft.AspNetCore.Mvc; 
using System.Collections.Generic; 
using System.Linq; 
using TodoApi.Models;  
namespace TodoApi.Controllers {     
[Route("api/[controller]")]     
[ApiController]     
public class TodoController : ControllerBase     
{        
private readonly TodoContext _context;          
public TodoController(TodoContext context)         
{             _context = context;              
if (_context.TodoItems.Count() == 0)             
{                 
_context.TodoItems.Add(new TodoItem { Name = "Item1" });                 _context.SaveChanges();             
}         
}            
} 
}
The preceding code defines an API controller class without methods. In the next sections, methods are added to implement the API. The class is annotated with an [ApiController] attribute to enable some convenient features.
The controller’s constructor uses Dependency Injection to inject the database context (TodoContext) into the controller. The database context is used in each of the CRUD methods in the controller. The constructor adds an item to the Postgres database if one doesn't exist.
Get to-do items
To get to-do items, add the following methods to the TodoController class:
[HttpGet] 
public ActionResult<List<TodoItem>> GetAll() 
{     
return _context.TodoItems.ToList(); 
} 
 
[HttpGet("{id}", Name = "GetTodo")] 
public ActionResult<TodoItem> GetById(long id) 
{    
var item = _context.TodoItems.Find(id);     
if (item == null)    
{         
return NotFound();     
}     
return item; 
}
These methods implement the two GET methods:
* 		GET /api/todo
* 		GET /api/todo/{id}
Routing and URL paths
The [HttpGet] attribute denotes a method that responds to an HTTP GET request. The URL path for each method is constructed as follows:
* 		Take the template string in the controller’s Route attribute:
namespace TodoApi.Controllers 
{     
[Route("api/[controller]")]     
[ApiController]     
public class TodoController : ControllerBase     
{         
private readonly TodoContext _context;
* 		[controller] is a variable which takes the controller class name minus the "Controller" suffix. For this sample, the controller class name is TodoController and the root name is "todo". ASP.NET Core routing is case insensitive.


￼
TodoController
* 		If the [HttpGet] attribute has a route template (such as [HttpGet("/products")], append that to the path. This sample doesn't use a template.
In the following GetById method, "{id}" is a placeholder variable for the unique identifier of the to-do item. When GetById is invoked, it assigns the value of "{id}" in the URL to the method's idparameter.
[HttpGet(“{id}”, Name = “GetTodo”)] 
public ActionResult<TodoItem> GetById(long id) 
{ 
var item = _context.TodoItems.Find(id); 
if (item == null) 
{ 
return NotFound(); 
} 
return item; 
}
Name = "GetTodo" creates a named route. Named routes:
* 		Enable the app to create an HTTP link using the route name.
Return values
The GetAll method returns a collection of TodoItem objects. MVC automatically serializes the object to JSON and writes the JSON into the body of the response message. The response code for this method is 200, assuming there are no unhandled exceptions. Unhandled exceptions are translated into 5xx errors.
In contrast, the GetById method returns the ActionResult<T> type, which represents a wide range of return types. GetById has two different return types:
* 		If no item matches the requested ID, the method returns a 404 error. Returning NotFound returns an HTTP 404 response.
* 		Otherwise, the method returns 200 with a JSON response body. Returning item results in an HTTP 200 response.
Launch the app
In VS Code, press F5 to launch the app. Navigate to http://localhost:5002/api/todo (the Todocontroller we created).
Here’s a sample HTTP response for the GetAll method:
[   
{     "id": 1,     
"name": "Item1",     
"isComplete": false   
} 
]

￼
Response of http://localhost:5002/api/todo in browser
Let’s match it with data in TodoItems table. Do the following steps,
* Open pgAdmin 4
* Select TodoApi server >> TodoList databse >> Schemas >> Tables >> TodoItems
* Right click on TodoItems table and select View/Edit Data


￼
View Data of TodoItems table
Response of http://localhost:5002/api/todo is matched with data in TodoItems table. 👏
If you want to navigate to http://localhost:5002/api/todo/2,
Here’s a sample HTTP response for the GetById method:
[   
{     "id": 2,     
"name": "Item1",     
"isComplete": false   
} 
]
Where to Go From Here?
You can download the completed project using the link at the bottom of this tutorial.
Download Materials
As always, any feedback is appreciated, so feel free to comment down here or reach out on twitter — and, as always,



Source : https://medium.com/@agavatar/webapi-with-net-core-and-postgres-in-visual-studio-code-8b3587d12823


//------------------

API VERSIONING

EFCORE POSTGRESQL USING
dotnet new mvc -o mvcTest

dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 2.1.0

dotnet add package NpgSql.EntityFrameworkCore.PostgreSQL.Design

create model

create dataContex

--startup.cs ->
services.AddDbContext<testContext>(options =>
                            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

--appsettings.json
  "ConnectionStrings": {
    "DefaultConnection":
    "Host=localhost;Port=5432;Username=postgres;Password=password;Database=testMvc;"
    }


dotnet ef migrations add initial

dotnet ef database update



------

Api Versioning

API Versioning in .NET Core
.NET Core is a great framework for building out front- and backend applications. Read on to learn how to version the APIs your app's call using this framework.

Jumpstart your Angular applications with Indigo.Design, a unified platform for visual design, UX prototyping, code generation, and app development.
In this post, we will see how to use different options for versioning in .NET Core API projects. Versioning APIs is very important and it should be implemented in any API project. Let's see how to achieve this in .NET Core.
Prerequisites:
* Visual Studio 2017 community edition, download here.
* .NET Core 2.0 SDK from here (I have written a post to install SDK here).
Create the API App Using a .NET Core 2.0 Template in VS 2017
Once you have all these installed, open your Visual Studio 2017 -> Create New Project -> Select Core Web application:
￼
Click on Ok and in the next window, select API as shown below:
￼
Visual Studio will create a well-structured application for you.
Install the NuGet Package for API Versioning
The first step is to install the NuGet package for API Versioning.
Search with "Microsoft.AspNetCore.Mvc.Versioning" in the NuGet Package Manager and click on Install:
￼
This NuGet package is a service API versioning library for Microsoft ASP.NET Core.
Changes in Startup Class
Once the NuGet package is installed, the next step is to add the API Versioning service in the ConfigureService method as shown below:

services.AddApiVersioning(o => {

o.ReportApiVersions = true;

o.AssumeDefaultVersionWhenUnspecified = true;

o.DefaultApiVersion = new ApiVersion(1, 0);

});

Some points here:
* The ReportApiVersions flag is used to add the API versions in the response header as shown below:
￼
* The AssumeDefaultVersionWhenUnspecified flag is used to set the default version when the client has not specified any versions. Without this flag, the UnsupportedApiVersion exception will occur when the version is not specified by the client.
* The DefaultApiVersion flag is used to set the default version count.
Create Multiple Versions of the Sample API
Once the API versioning service is added, the next step is to create multiple versions of our Values API.
For now, just keep the GET method and remove the rest of the methods and create version 2 of the same API, as shown below:

using System;

using System.Collections.Generic;

using System.Linq;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;



namespace ApiVersioningSampleApp.Controllers {

 [ApiVersion("1.0")]

 [Route("api/Values")]

 public class ValuesV1Controller: Controller {

  // GET api/values

  [HttpGet]

  public IEnumerable < string > Get() {

   return new string[] {

    "Value1 from Version 1",

    "value2 from Version 1"

   };

  }

 }



 [ApiVersion("2.0")]

 [Route("api/Values")]

 public class ValuesV2Controller: Controller {

  // GET api/values

  [HttpGet]

  public IEnumerable < string > Get() {

   return new string[] {

    "value1 from Version 2",

    "value2 from Version 2"

   };

  }

 }

}

In the above code:
* We have applied the attribute [ApiVersion("1.0")] for Version 1.
* We have applied the attribute [ApiVersion("2.0")] for Version 2.
* Also changed the GET value to understand which version is getting called.
Just run your application and you will see the Version 1 API is getting called because we did not specify any specific version; thus the default version (1.0 in our case) will be called:
￼
There are some ways by which you can specify the version of the API, we'll discuss below.
Query String-Based Versioning
In this, you can specify the version of the API in the query string. For example, to call version 2 of the Values API, the below call should work:
/api/values?api-version=2.0 
￼
URL-Based Versioning
There are many people who do not like query based patterns, in which case we can implement URL-based versioning by changing the route as shown below:
[Route("api/{v:apiVersion}/Values")] 
In such a case, the below call will return the version 2 of the API:
/api/2.0/values 
￼
This approach is more readable.
HTTP Header-Based Versioning
If you do not wish to change the URL of the API then you can send the version of API in the HTTP header.
To enable this, the version reader needs to be added to the ConfigureService method as shown below:

o.ApiVersionReader = new HeaderApiVersionReader("x-api-version");

Once you enable this, the query string approach will not work. If you wish to enable both of them then just use the below code:

o.ApiVersionReader = new QueryStringOrHeaderApiVersionReader("x-api-version");

Once the API Version reader is enabled, you can specify the API version while calling this particular API. For example, I have given Version 1 while calling the API from Postman:
￼
Some Useful Features
Deprecating the Versions
Sometimes we need to deprecate some of the versions of the API, but we do not wish to completely remove that particular version of the API.
In such cases, we can set the Deprecated flag to true for that API, as shown below:

[ApiVersion("1.0", Deprecated = true)]

[Route("api/Values")]

public class ValuesV1Controller: Controller {



 //// Code



}

It will not remove this version of the API, but it will return the list of deprecated versions in the header, api-deprecated-versions, as shown below:
￼
Assign the Versions Using Conventions
If you have lots of versions of the API, instead of putting the ApiVersion attribute on all the controllers, we can assign the versions using a conventions property.
In our case, we can add the convention for both the versions as shown below:

o.Conventions.Controller<ValuesV1Controller>().HasApiVersion(new ApiVersion(1, 0)); 

o.Conventions.Controller<ValuesV2Controller>().HasApiVersion(new ApiVersion(2, 0));

Which means that we are not required to put [ApiVersion] attributes above the controller
API Version Neutral
There might be a case when we need to opt out the version for some specific APIs.
For example, Health checking APIs which are used for pinging. In such cases, we can opt out of this API by adding the attribute [ApiVersionNeutral] as shown below:

[ApiVersionNeutral] 

[RoutePrefix( "api/[controller]" )] 

public class HealthCheckController : Controller {

////Code

}

Other Features to Consider:
* Add MapToApiVersion in the attribute if you wish to apply versioning only for specific action methods instead of the whole controller. For example:

[HttpGet, MapToApiVersion("2.0")]

public IEnumerable<string> Get() {

return new string[] { "value1 from Version 2", "value2 from Version 2" };

}

* We can get the version information from the method HttpContext.GetRequestedApiVersion();, this is useful to check which version has been requested by the client.
* Version Advertisement can be used by each service to advertise the supported and deprecated API versions it knows about. This is generally used when the service API versions are split across hosted applications.
* We can allow clients to request a specific API version by media type. This option can be enabled by adding the below line in the API versioning options in the ConfigureService method:

options => options.ApiVersionReader = new MediaTypeApiVersionReader();

Hope this helps.
You can find my all .NET core posts here.