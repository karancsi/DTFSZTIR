using System;
using System.Collections.Generic;
using System.Text;

namespace DTFSZTIR_bead
{    public class PalmerJob
    {
        public int Id { get; set; }
        public int p { get; set; }

        public PalmerJob(int id, int priority)
        {
            Id = id;
            p = priority;
        }
    }

}
