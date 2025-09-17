

namespace Beaversims.Core.Parser
{
    public enum Role { Tank, Healer, Dps }

    internal class Unit
    {
        public string Name { get; }
        public UnitId Id { get; }

        public Unit(string name, UnitId id)
        {
            Name = name;
            Id = id;
        }

        public bool IsUnit(Unit otherUnit) =>
            Id == otherUnit.Id;
    }

    internal class Player : Unit
    {
        public Role Role { get; init; }
        public long DamageDone { get; set; }
        public long HealingDone { get; set; }

        public Player(string name, UnitId id, Role role)
            : base(name, id)
        {
            Role = role;
        }
    }
    internal class User : Player
    {
        public Spec Spec { get; set; }
        public AbilityRepo Abilities { get; } = new AbilityRepo();
        public HashSet<int> SummonIds { get; set; } = new();//Only collecting Type Id, not Instance Id


        public User(string name, UnitId id, Role role)
            : base(name, id, role)
        {
        }
    }
}