using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApiCore.Persistance
{
   public class DatabaseOptions
    {
        [Required(ErrorMessage = "Database connection string is required.")]
        public string MyLocalDataBase { get; set; } = string.Empty;

    }
}
