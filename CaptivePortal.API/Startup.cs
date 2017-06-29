﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using System.Web.Http;
using CaptivePortal.API.Context;
using System.Net.Http.Formatting;
using Newtonsoft.Json.Serialization;
using CaptivePortal.API.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

[assembly: OwinStartup(typeof(CaptivePortal.API.Startup))]

namespace CaptivePortal.API
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            createRolesandUsers();
        }


        // In this method we will create default User roles and Admin user for login   
        private void createRolesandUsers()
        {
            DbContext context = new DbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<Users>(new UserStore<Users>(context));


            // In Startup iam creating first Admin Role and creating a default Admin User    
            if (!roleManager.RoleExists("GlobalAdmin"))
            {

               // first we create Admin rool
               var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "GlobalAdmin";
                roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website                  

                var user = new Users();
                user.UserName = "admin@airloc8.com";
                user.Email = "admin@airloc8.com";
                user.CreationDate = DateTime.Now;
                user.UpdateDate = DateTime.Now;
                user.EmailConfirmed = true;
                user.AccessFailedCount = 0;
                user.LockoutEnabled = false;
                user.TwoFactorEnabled = false;
                user.PhoneNumberConfirmed = true;

                string userPWD = "Tes@123";

                var chkUser = UserManager.Create(user, userPWD);

                //Add default User to Role Admin   
                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "GlobalAdmin");

                }
            }

            // creating Creating LocalAdmin role    
            if (!roleManager.RoleExists("CompanyAdmin"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "CompanyAdmin";
                roleManager.Create(role);

            }

            // creating Creating BusinessUser role    
            if (!roleManager.RoleExists("BusinessUser"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "BusinessUser";
                roleManager.Create(role);

            }
            // creating Creating User role    
            if (!roleManager.RoleExists("WifiUser"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "WifiUser";
                roleManager.Create(role);

            }
        }
    }
}
