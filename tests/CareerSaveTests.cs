using Striker.Core.Career;
using Xunit;

namespace Striker.Tests
{
    /// <summary>
    /// CareerSave'ın seri hale getirme/geri okuma mantığını doğrular (engine-bağımsız).
    /// Faz 7 (Save/Load): kariyer kimliği diske güvenle yazılıp okunur.
    /// </summary>
    public class CareerSaveTests
    {
        [Fact]
        public void Kimlik_YazilipOkunur_AdayVeRolKorunur()
        {
            CareerSave original = new CareerSave(new PlayerIdentity("Efe Korkmaz", PlayerRole.Striker), signed: false);

            string data = original.ToSaveString();
            CareerSave? restored = CareerSave.FromSaveString(data);

            Assert.NotNull(restored);
            Assert.Equal("Efe Korkmaz", restored!.Identity.Name);
            Assert.Equal(PlayerRole.Striker, restored.Identity.Role);
            Assert.False(restored.Signed);
        }

        [Fact]
        public void ImzaliKariyer_SignedBaytKorunur()
        {
            CareerSave original = new CareerSave(new PlayerIdentity("Deniz Arslan", PlayerRole.Winger), signed: true);

            CareerSave? restored = CareerSave.FromSaveString(original.ToSaveString());

            Assert.NotNull(restored);
            Assert.True(restored!.Signed);
            Assert.Equal(PlayerRole.Winger, restored.Identity.Role);
        }

        [Theory]
        [InlineData(PlayerRole.Striker)]
        [InlineData(PlayerRole.Playmaker)]
        [InlineData(PlayerRole.Winger)]
        [InlineData(PlayerRole.Defender)]
        public void TumRoller_RoundTripIleKorunur(PlayerRole role)
        {
            CareerSave original = new CareerSave(new PlayerIdentity("Test", role), signed: false);

            CareerSave? restored = CareerSave.FromSaveString(original.ToSaveString());

            Assert.NotNull(restored);
            Assert.Equal(role, restored!.Identity.Role);
        }

        [Fact]
        public void BozukVeri_NullDoner_OyunCokmezmez()
        {
            Assert.Null(CareerSave.FromSaveString(string.Empty));
            Assert.Null(CareerSave.FromSaveString("garbage"));
            Assert.Null(CareerSave.FromSaveString("striker.save.v2\nname=X"));
            Assert.Null(CareerSave.FromSaveString(CareerSave.FormatVersion + "\nrole=Striker")); // name eksik
        }

        [Fact]
        public void CiftYenidenDize_GecersizRolleriYokSayar()
        {
            string data = CareerSave.FormatVersion + "\nname=Efe\nrole=Bilinmeyen\nsigned=0\n";

            CareerSave? restored = CareerSave.FromSaveString(data);

            Assert.Null(restored); // role parse edilemedi → kimlik eksik → null
        }
    }
}
