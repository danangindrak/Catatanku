//using AspNet.Security.OAuth.Keycloak;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;

namespace WebAuthNet5
{
    public class Startup
    {
        private string _realm;
        private string _client_id;
        private string _client_secret;
        private string _issuer;
        private string _authorization_endpoint;
        private string _token_endpoint;
        private string _redirect_uri;
        private string _logout_uri;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            _realm = Configuration.GetSection("keycloak").GetSection("realm").Value;
            _client_id = Configuration.GetSection("keycloak").GetSection("client_id").Value;
            _client_secret = Configuration.GetSection("keycloak").GetSection("client_secret").Value;
            _issuer = Configuration.GetSection("keycloak").GetSection("issuer").Value;
            _redirect_uri = Configuration.GetSection("keycloak").GetSection("redirect_uri").Value;
            _logout_uri = Configuration.GetSection("keycloak").GetSection("logout_uri").Value;
            _authorization_endpoint = Configuration.GetSection("keycloak").GetSection("end_point").GetSection("authorization").Value;
            _token_endpoint = Configuration.GetSection("keycloak").GetSection("end_point").GetSection("token").Value;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        /// <summary>
        /// reference: https://github.com/tuxiem/AspNetCore-keycloak
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddAuthentication(options =>
            {
                //Sets cookie authentication scheme
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })

            .AddCookie()
            .AddOpenIdConnect(options =>
            {
                /*
                 * ASP.NET core uses the http://*:5000 and https://*:5001 ports for default communication with the OIDC middleware
                 * The app requires load balancing services to work with :80 or :443
                 * These needs to be added to the keycloak client, in order for the redirect to work.
                 * If you however intend to use the app by itself then,
                 * Change the ports in launchsettings.json, but beware to also change the options.CallbackPath and options.SignedOutCallbackPath!
                 * Use LB services whenever possible, to reduce the config hazzle :)
                */

                //Use default signin scheme
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //Keycloak server
                options.Authority = _authorization_endpoint;
                //Keycloak client ID
                options.ClientId = _client_id;
                //Keycloak client secret
                options.ClientSecret = _client_secret;
                //Keycloak .wellknown config origin to fetch config
                options.MetadataAddress = $"{_issuer}/.well-known/openid-configuration";
                //Require keycloak to use SSL
                options.RequireHttpsMetadata = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = true;
                //Token response type, will sometimes need to be changed to IdToken, depending on config.
                options.ResponseType = OpenIdConnectResponseType.Code;
                //SameSite is needed for Chrome/Firefox, as they will give http error 500 back, if not set to unspecified.
                options.NonceCookie.SameSite = SameSiteMode.Unspecified;
                options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
                options.RequireHttpsMetadata = false;
                options.CallbackPath = _redirect_uri; // "/signin-keycloak";

                options.ClaimsIssuer = _issuer;

                options.SignedOutCallbackPath = _logout_uri;

            });

            services.AddControllersWithViews();
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
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            //app.UseCookiePolicy();


            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
