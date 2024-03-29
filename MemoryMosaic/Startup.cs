using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Toolbelt.Blazor.Extensions.DependencyInjection; 

namespace MemoryMosaic;

public sealed class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddRazorPages();
        services.AddServerSideBlazor().AddHubOptions(hub => hub.MaximumReceiveMessageSize = 100 * 1024 * 1024); //https://stackoverflow.com/a/61459904
        services.AddHotKeys();

    #if DEBUG
        services.AddSassCompiler();
    #endif
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        //https://stackoverflow.com/questions/38406571/static-files-outside-the-wwwroot-for-netcore-app
        //Allows MM to display photos and videos outside wwwroot.
        app.UseStaticFiles();
        app.UseStaticFiles(new StaticFileOptions
        {
            ServeUnknownFileTypes = true,
            FileProvider = new PhysicalFileProvider(S.LibFolderPath),
            RequestPath = new PathString("/mm_library")
        });
        app.UseStaticFiles(new StaticFileOptions
        {
            ServeUnknownFileTypes = true,
            FileProvider = new PhysicalFileProvider(S.ImportFolderPath),
            RequestPath = new PathString("/mm_import")
        });
        app.UseStaticFiles(new StaticFileOptions
        {
            ServeUnknownFileTypes = true,
            FileProvider = new PhysicalFileProvider(S.TmpFolderPath),
            RequestPath = new PathString("/mm_tmp")
        });

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}