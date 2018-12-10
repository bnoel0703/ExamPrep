using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ExamPrepWebApp.Models
{
    public class ExamPrepWebAppContext : DbContext
    {
        public ExamPrepWebAppContext (DbContextOptions<ExamPrepWebAppContext> options)
            : base(options)
        {
        }

        public DbSet<ExamPrepWebApp.Models.MusicTrack> MusicTrack { get; set; }
    }
}
