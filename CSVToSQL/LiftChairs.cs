namespace CSVToSQL
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class LiftChairs
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public int Section { get; set; }

        public int RPM { get; set; }

        public double Current { get; set; }

        public double Temp { get; set; }
    }
}
