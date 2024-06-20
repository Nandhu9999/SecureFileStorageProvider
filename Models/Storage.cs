using System;
using System.Collections.Generic;

namespace SecureFileStorageProvider.Models
{
    public partial class Storage
    {
        public string StorageId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int UserId { get; set; }
        public int Size { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
