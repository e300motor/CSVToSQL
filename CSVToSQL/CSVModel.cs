namespace CSVToSQL
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class CSVModel : DbContext
    {
        public CSVModel()
            : base("name=CSVModel")
        {
        }

        public virtual DbSet<LiftChairs> LiftChairs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
