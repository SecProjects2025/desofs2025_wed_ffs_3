using AMAP.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AMAP.DataAccess.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets
        public DbSet<AppUser> AppUser { get; set; }

        public DbSet<Composite> Composites { get; set; }

        public DbSet<DeliveryDate> DeliveryDates { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<ProductOffer> ProductOffers { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<Delivery> Deliveries { get; set; }

        public DbSet<SubscriptionPeriod> SubscriptionPeriods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da relação entre Product e Producer
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Producer)
                .WithMany()
                .HasForeignKey(p => p.ProducerId)
                .OnDelete(DeleteBehavior.Restrict); // Previne o cascade delete direto

            // Configuração da flag IsDeleted com valor padrão
            modelBuilder.Entity<Product>()
                .Property(p => p.IsDeleted)
                .HasDefaultValue(false);

            // Filtro global para ignorar produtos deletados
            modelBuilder.Entity<Product>()
                .HasQueryFilter(p => !p.IsDeleted);

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany()
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade); // Exclui imagens ao excluir um produto

            modelBuilder.Entity<ProductImage>()
                .HasQueryFilter(pi => !pi.Product.IsDeleted);

            modelBuilder.Entity<Composite>()
                .HasKey(c => new { c.ProductId, c.SubProductId });

            // Relacionamento entre Composite e Product (ProductId)
            modelBuilder.Entity<Composite>()
                .HasOne(c => c.Product)
                .WithMany(p => p.Composites) // Relacionamento inverso, Composites é uma coleção em Product
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata

            // Relacionamento entre Composite e Product (SubProductId)
            modelBuilder.Entity<Composite>()
                .HasOne(c => c.SubProduct)
                .WithMany() // SubProduct não tem navegação inversa, então é apenas WithMany()
                .HasForeignKey(c => c.SubProductId)
                .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata

            // Filtro para ignorar produtos deletados
            modelBuilder.Entity<Composite>()
                .HasQueryFilter(c => !c.Product.IsDeleted && !c.SubProduct.IsDeleted);

            modelBuilder.Entity<DeliveryDate>()
                .HasOne(d => d.SubscriptionPeriod) // Uma DeliveryDate pertence a um SubscriptionPeriod
                .WithMany(s => s.DeliveryDates)    // Um SubscriptionPeriod pode ter várias DeliveryDates
                .HasForeignKey(d => d.SubscriptionPeriodId)
                .OnDelete(DeleteBehavior.Cascade); // Cascata para eliminar DeliveryDates ao apagar um SubscriptionPeriod

            // Configuração da relação entre ProductOffer e SubscriptionPeriod
            modelBuilder.Entity<ProductOffer>()
                .HasOne(po => po.SubscriptionPeriod)
                .WithMany()
                .HasForeignKey(po => po.SubscriptionPeriodId)
                .OnDelete(DeleteBehavior.Restrict); // Evitar exclusão em cascata (se não deseja excluir ProductOffers automaticamente)

            // Configuração da relação entre ProductOffer e Product
            modelBuilder.Entity<ProductOffer>()
                .HasOne(po => po.Product)
                .WithMany()
                .HasForeignKey(po => po.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Evitar exclusão em cascata (se não deseja excluir ProductOffers automaticamente)

            modelBuilder.Entity<ProductOffer>()
                .HasQueryFilter(po => !po.Product.IsDeleted);

            // Configuração da relação entre Subscription e ProductOffer
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.ProductOffer)
                .WithMany()
                .HasForeignKey(s => s.ProductOfferId)
                .OnDelete(DeleteBehavior.Restrict); // Evitar exclusão em cascata

            // Filtro global para a entidade Subscription, garantindo que a Subscription tenha um ProductOffer válido
            modelBuilder.Entity<Subscription>()
                .HasQueryFilter(s => s.ProductOffer == null || !s.ProductOffer.Product.IsDeleted);  // Filtra subscrições com ofertas de produtos não excluídos

            // Configuração da relação entre Subscription e Producer (AppUser)
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.CoProducer)
                .WithMany()
                .HasForeignKey(s => s.CoProducerId)
                .OnDelete(DeleteBehavior.Restrict); // Evitar exclusão em cascata

            modelBuilder.Entity<Delivery>()
                .HasOne(s => s.Subscription)
                .WithMany()
                .HasForeignKey(s => s.SubscriptionId)
                .OnDelete(DeleteBehavior.Restrict); // Evitar exclusão em cascata

            modelBuilder.Entity<Delivery>()
                .HasQueryFilter(d => d.Subscription == null ||
                    (d.Subscription.ProductOffer == null ||
                    !d.Subscription.ProductOffer.Product.IsDeleted));

            // Configurar a relação entre Payment e AppUser (Producer)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Subscription)
                .WithMany()
                .HasForeignKey(p => p.SubscriptionId)
                .OnDelete(DeleteBehavior.Restrict); // Evita exclusão em cascata

            modelBuilder.Entity<Payment>()
                .HasQueryFilter(p => p.Subscription == null || !p.Subscription.ProductOffer.Product.IsDeleted);
        }
    }
}
