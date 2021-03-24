using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Demo.Models.Interfaces;

namespace Demo.Models.Domain.Auth
{
    [Table(name: nameof(AppRole), Schema = "auth")]
    public class RefreshSession : IIdEntity, IDatedEntity
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public Guid RefreshToken { get; set; }

        [NotNull]
        [MaxLength(200)]
        public string UserAgent { get; set; }

        [NotNull]
        [MaxLength(200)]
        public string Fingerprint { get; set; }

        [NotNull]
        [MaxLength(15)]
        public string IPAddress { get; set; }

        public DateTime ExpiresIn { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public AppUser User { get; set; }
    }
}
