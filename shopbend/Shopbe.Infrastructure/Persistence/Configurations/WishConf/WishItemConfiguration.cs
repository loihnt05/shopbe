using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shopbe.Infrastructure.Persistence.Configurations.WishConf;

public class WishItemConfiguration : IEntityTypeConfiguration<WishItemConfiguration>
{
    public void Configure(EntityTypeBuilder<WishItemConfiguration> builder)
    {
        
    }
}