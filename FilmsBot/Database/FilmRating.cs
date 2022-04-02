using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FilmsBot.Database
{
    [Table("RATINGS")]
    public class FilmRating 
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Column("PARTICIPANT_ID")]
        public ulong ParticipantId { get; set; }

        [Column("RATING")]
        public double Rating { get; set; }

        [Column("DATE")]
        public DateTime Date { get; set; }

        #region Relations

        public virtual Participant? Participant { get; set; }

        #endregion

        #region Configuration

        public class Configuration : IEntityTypeConfiguration<FilmRating>
        {
            public void Configure(EntityTypeBuilder<FilmRating> builder)
            {
                builder
                    .HasOne(r => r.Participant)
                    .WithMany(p => p.Ratings)
                    .HasForeignKey(r => r.ParticipantId)
                    .HasPrincipalKey(p => p.Id)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_FILMS_RATINGS_PARTICIPANT_ID");
            }
        }

        #endregion
    }
}