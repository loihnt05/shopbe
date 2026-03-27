using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shopbe.Infrastructure.Persistence.Configurations.ShipConf;

public class ShipFeeConfiguration : IEntityTypeConfiguration<ShipFeeConfiguration>
{
    public void Configure(EntityTypeBuilder<ShipFeeConfiguration> builder)
    {
        
    }
}