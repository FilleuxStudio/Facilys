﻿using System.ComponentModel.DataAnnotations;

namespace Facilys.Components.Models
{
    public class ReferencesIgnored
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string Reference { get; set; } = string.Empty;
    }

    public class InterestingReferences
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string Reference { get; set; } = string.Empty;
        [Required]
        public float Price { get; set; } = 0.0f;
    }
}
