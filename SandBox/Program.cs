using System;
using SandBox.DbContextSource;

namespace SandBox
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new ArtsdatabankenSIContext())
            {
                ShowAllIds(db);
            }
        }

        private static void ShowAllIds(ArtsdatabankenSIContext db)
        {
            foreach (var item in db.RfReference)
            {
                Console.Write(item.PkReferenceId + ",");
            }
        }
    }
}
