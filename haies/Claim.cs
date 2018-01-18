using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace haies
{
   public class Claim
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Customer_type Type { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string Email { get; set; }
        public object PersonId { get; set; }
    }
}
