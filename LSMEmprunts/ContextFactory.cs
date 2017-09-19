using LSMEmprunts.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSMEmprunts
{
    static class ContextFactory
    {
        public static Context OpenContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<Context>();
            optionsBuilder.UseSqlite(ConfigurationManager.ConnectionStrings["LSMEmprunts"].ConnectionString);
            var retval = new Context(optionsBuilder.Options);
            retval.Database.Migrate();
            return retval;
        }
    }
}
