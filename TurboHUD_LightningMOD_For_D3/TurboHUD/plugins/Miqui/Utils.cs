using Turbo.Plugins.Default;

namespace Turbo.Plugins.Miqui
{

    public static class Utils
    {
        public const int TICKS_PER_SECOND = 60;

        public const double PAIN_ENHANCER_RANGE = 20.0; // Yards
        public const double PAIN_ENHANCER_ATTACK_SPEED_PER_TARGET = 3.0; // Aps

        // Attack speed buffs
        public const int LAWS_OF_VALOR_PASSIVE_BUFF = 8;
        public const int LAWS_OF_VALOR_ACTIVE_BUFF = 7;
        public const int BIG_BAD_VOODOO_BUFF = 15;
        public const int PARAGON_ATTACK_SPEED_BUFF = 10;
        public const int SPEED_PYLON_ATTACK_SPEED_BUFF = 30;
        public const int SEIZE_THE_INITIATIVE_ATTACK_SPEED_BUFF = 30;
        public const int FERVOR_ATTACK_SPEED_BUFF = 15;
    }

    public static class EquipExtension
    {
        public static bool IsEquipped(this ItemLocation location)
        {
            return location >= ItemLocation.Head && location <= ItemLocation.Neck;
        }
    }
}