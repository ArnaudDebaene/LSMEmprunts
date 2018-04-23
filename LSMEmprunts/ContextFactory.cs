using LSMEmprunts.Data;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

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
