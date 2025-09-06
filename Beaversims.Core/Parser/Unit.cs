namespace Beaversims.Core.Parser
{
    public enum Role
    {
        Tank,
        Healer,
        Dps,
    }

    internal class Unit
    {
        public string Name;
        public int Id;
        public int InstanceId;
        public Role? Role;

        public Unit(string name, int id, int instanceId)
        {
            Name = name;
            Id = id;
            InstanceId = instanceId;
        }
        public bool IsUnit(Unit otherUnit)
        {
            return Id == otherUnit.Id && InstanceId == otherUnit.InstanceId;
        }
    }
}


