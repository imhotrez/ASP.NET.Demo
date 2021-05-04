using System;
using System.ComponentModel.DataAnnotations.Schema;
using Demo.Models.Domain.Auth;
using Demo.Models.Interfaces;

namespace Demo.Models.Domain.Image {
    [Table(name: nameof(Metadata), Schema = "image")]
    public class Metadata : INamedEntity {
        public long Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public long UserId { get; set; }

        public AppUser User { get; set; }
    }
}
