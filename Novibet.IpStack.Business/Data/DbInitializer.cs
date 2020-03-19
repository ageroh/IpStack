using System.Linq;

namespace Novibet.IpStack.Business.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IpStackContext context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.IpAddressess.Any())
            {
                return;
            }

            context.SaveChanges();
        }
    }
}
