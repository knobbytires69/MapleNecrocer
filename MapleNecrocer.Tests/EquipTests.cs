using MapleNecrocer;
using Xunit;

namespace MapleNecrocer.Tests
{
    public class EquipTests
    {
        [Theory]
        [InlineData("1040000", "Coat/")]
        [InlineData("1000001", "Cap/")]
        [InlineData("1302000", "Weapon/")]
        [InlineData("1700000", "Weapon/")]
        [InlineData("1900000", "TamingMob/")]
        [InlineData("1980000", "TamingMob/")]
        [InlineData("20000", "Face/")]
        [InlineData("30000", "Hair/")]
        [InlineData("1010000", "Accessory/")]
        [InlineData("1100000", "Cape/")]
        [InlineData("0", "")]
        public void GetDir_MapsIdToCharacterDirectory(string id, string expected)
        {
            Assert.Equal(expected, Equip.GetDir(id));
        }

        [Theory]
        [InlineData("1040000", PartName.Coat)]
        [InlineData("1000001", PartName.Cap)]
        [InlineData("1302000", PartName.Weapon)]
        [InlineData("1700000", PartName.CashWeapon)]
        [InlineData("1900000", PartName.WalkTamingMob)]
        [InlineData("1980000", PartName.SitTamingMob)]
        [InlineData("20000", PartName.Face)]
        [InlineData("30000", PartName.Hair)]
        [InlineData("1010000", PartName.FaceAcc)]
        [InlineData("1100000", PartName.Cape)]
        [InlineData("0", PartName.Body)]
        public void GetPart_MapsIdToPart(string id, PartName expected)
        {
            Assert.Equal(expected, Equip.GetPart(id));
        }

        [Theory]
        // reqJob bitmask: bit0=Warrior, bit1=Mage, bit2=Archer, bit3=Thief, bit4=Pirate
        // selectedClass: 1=Warrior, 2=Mage, 3=Archer, 4=Thief, 5=Pirate
        [InlineData(0, 1, false, true)]    // any-class item matches any class when not exclusive
        [InlineData(0, 2, false, true)]
        [InlineData(0, 1, true, false)]    // any-class item is excluded in exclusive mode
        [InlineData(-1, 1, false, false)]  // none-class item never matches
        [InlineData(-1, 1, true, false)]
        [InlineData(1, 1, false, true)]    // warrior -> warrior
        [InlineData(1, 1, true, true)]     // pure warrior matches exclusive
        [InlineData(1, 2, false, false)]   // warrior -> mage
        [InlineData(2, 2, false, true)]    // mage -> mage
        [InlineData(2, 2, true, true)]
        [InlineData(6, 2, false, true)]    // mage+archer -> mage ok (non-exclusive)
        [InlineData(6, 2, true, true)]     // mage+archer -> mage ok (exclusive still shows class-restricted)
        [InlineData(3, 1, false, true)]    // warrior+mage -> warrior ok
        [InlineData(3, 1, true, true)]     // warrior+mage -> warrior ok (exclusive still shows class-restricted)
        [InlineData(16, 5, false, true)]   // pirate -> pirate
        [InlineData(5, 3, false, true)]    // warrior+archer -> archer ok
        [InlineData(5, 3, true, true)]     // warrior+archer -> archer ok (exclusive still shows class-restricted)
        [InlineData(1, 6, false, false)]   // invalid selectedClass
        [InlineData(1, 0, false, true)]    // All class shows a class-restricted item
        [InlineData(0, 0, false, true)]    // All class shows a universal item
        [InlineData(0, 0, true, true)]     // All class ignores exclusive
        public void MatchesClass_FiltersByJobBitmask(int reqJob, int selectedClass, bool exclusive, bool expected)
        {
            Assert.Equal(expected, Equip.MatchesClass(reqJob, selectedClass, exclusive));
        }
    }
}
