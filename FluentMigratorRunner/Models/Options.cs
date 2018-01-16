namespace FluentMigratorRunner.Models
{
    public class Options
    {
        public Options()
        {
            Connection = string.Empty;
        }

        public string Connection { get; set; }
        public DbEnum DbType { get; set; }
    }
}
