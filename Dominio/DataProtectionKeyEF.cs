using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("DataProtectionKeys")]
    public class DataProtectionKeyEF
    {
        [Key]
        public int Id { get; set; }
        public string FriendlyName { get; set; }
        public string Xml { get; set; }
    }
}