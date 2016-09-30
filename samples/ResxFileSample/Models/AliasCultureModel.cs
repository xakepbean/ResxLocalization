using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ResxFileSample.Models
{
    public class AliasCultureModel
    {
        private CultureInfo _Culture = null;

        public CultureInfo Culture { get { return _Culture; } }
        private string _Name = string.Empty;

        [Required(ErrorMessage = "The Name field is required.")]
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                _Culture = new CultureInfo(_Name);
            }
        }

        [Required(ErrorMessage = "The Alia field is required.")]
        public string Alia { get; set; }
        public bool Enabled { get; set; }
    }
}
