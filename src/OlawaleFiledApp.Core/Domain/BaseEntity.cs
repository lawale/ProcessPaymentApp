using System;
using System.ComponentModel.DataAnnotations;

namespace OlawaleFiledApp.Core.Domain
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        //configuration for soft deletes
        public DateTime? DeletedAt { get; set; }
        
        public bool IsDeleted { get; set; }

        public BaseEntity()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}