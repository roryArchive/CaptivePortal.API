﻿using CaptivePortal.API.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CaptivePortal.API.Context
{
    public class CPDBContext:DbContext
    {
        public CPDBContext() : base("name=CPDBContext")
        {
             
        }
        public DbSet<Users> Users { get; set; }
        public DbSet<UsersAddress> UsersAddress { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<Nas> Nas { get; set; }
        public DbSet<Organisation> Organisation { get; set; }
        public DbSet<Site> Site { get; set; }
        public DbSet<RadGroupCheck> RadGroupCheck { get; set; }
        public DbSet<Radacct> Radacct { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<Role> Role { get; set; } 

    }
}