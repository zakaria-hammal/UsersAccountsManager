using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CLIENT;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5250") });

builder.Services.AddBlazoredLocalStorageAsSingleton();

builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();

builder.Services.AddAuthorizationCore(config =>
    {
        config.AddPolicy("Can_Add_Role", policy => policy.RequireClaim("Can Add Role", "Can Add Role"));
        config.AddPolicy("Can_Add_Admin", policy => policy.RequireClaim("Can Add Admin", "Can Add Admin"));
        config.AddPolicy("Can_Add_Claim", policy => policy.RequireClaim("Can Add Claim", "Can Add Claim"));
        config.AddPolicy("Can_Add_User", policy => policy.RequireClaim("Can Add User", "Can Add User"));
        config.AddPolicy("Can_Add_Product", policy => policy.RequireClaim("Can Add Product", "Can Add Product"));
        config.AddPolicy("Can_View_Role", policy => policy.RequireClaim("Can View Role", "Can View Role"));
        config.AddPolicy("Can_View_Admin", policy => policy.RequireClaim("Can View Admin", "Can View Admin"));
        config.AddPolicy("Can_View_Claim", policy => policy.RequireClaim("Can View Claim", "Can View Claim"));
        config.AddPolicy("Can_View_User", policy => policy.RequireClaim("Can View User", "Can View User"));
        config.AddPolicy("Can_View_Product", policy => policy.RequireClaim("Can View Product", "Can View Product"));
        config.AddPolicy("Can_DeleteOrLock_User", policy => policy.RequireClaim("Can DeleteOrLock User", "Can DeleteOrLock User"));
        config.AddPolicy("Can_Delete_Admin", policy => policy.RequireClaim("Can Delete Admin", "Can Delete Admin"));
    }
);

await builder.Build().RunAsync();
