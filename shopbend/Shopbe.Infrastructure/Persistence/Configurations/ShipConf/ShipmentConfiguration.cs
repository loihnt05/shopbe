using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shopbe.Infrastructure.Persistence.Configurations.ShipConf;

public class ShipmentConfiguration : IEntityTypeConfiguration<ShipmentConfiguration>
{
    public void Configure(EntityTypeBuilder<ShipmentConfiguration> builder)
    {
        
    }
}