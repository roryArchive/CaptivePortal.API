﻿using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using CaptivePortal.API.Models;
using CaptivePortal.API.Context;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Net.Mail;

namespace CaptivePortal.API
{



    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            //string senderID = "dotnet.sahookashi@gmail.com";// use sender’s email id here..
            //const string senderPassword = "chand121"; // sender password here…
            //message.Destination = "praveshtiwariknp@gmail.com";
            string senderID = "tls@tes.media";
            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.avecsys.net", // smtp server address here…
                Port = 25,
                EnableSsl = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new System.Net.NetworkCredential("user@smtp.avecsys.net", "ema1ls3rv3r"),
                Timeout = 30000,
            };
            MailMessage mailMessage = new MailMessage(senderID, message.Destination, message.Subject, message.Body);
            mailMessage.IsBodyHtml = true;
            smtp.Send(mailMessage);
            return Task.FromResult(0);

        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

    public class ApplicationUserManager : UserManager<Users>
    {
        public ApplicationUserManager(IUserStore<Users> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<Users>(context.Get<DbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<Users>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<Users>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }


    

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<Users, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        //public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        //{
        //    return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        //}

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

    // Configure the RoleManager used in the application. RoleManager is defined in the ASP.NET Identity core assembly
    public class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        public ApplicationRoleManager(IRoleStore<IdentityRole, string> roleStore)
            : base(roleStore)
        {
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            return new ApplicationRoleManager(new RoleStore<IdentityRole>(context.Get<DbContext>()));
        }


    }
}
