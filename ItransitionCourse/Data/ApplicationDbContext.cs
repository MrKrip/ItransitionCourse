using ItransitionCourse.Models.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ItransitionCourse.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<TaskEntity> Tasks { get; set; }
        public DbSet<TaskImageEntity> Images { get; set; }
        public DbSet<UserAnswerEntity> Answers { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
