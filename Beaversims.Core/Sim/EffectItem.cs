using Beaversims.Core.Common;
using Beaversims.Core.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Beaversims.Core
{

    internal class ScalingData
    {
        public double Coef { get; set; }
        public int Class { get; set; }
        public ScalingData(int _class, double coef)
        {
            Class = _class;
            Coef = coef;
        }
    }



    internal abstract class SpecialEffect
    {
        public string Name { get; set; }
        public double Rppm { get; set; }
        public bool HasteScaling { get; set; }
        public double blp = Proc.onPullBlp;
        public double lastAttempt = Proc.initLastAttempt;
        public Dictionary<int, int> Ilvls { get; } = []; //<GearSetId, ilvl>
        public Dictionary<int, ItemSlot> ItemSlots { get; } = []; //<GearSetId, ItemSlot>
        public Dictionary<int, Dictionary<StatName, double>> RatingInc { get; } = [];

        //public bool DoNotRun { get; set; } = false;
        public virtual void Reset(User user, Fight fight)
        {
            blp = Proc.onPullBlp;
            lastAttempt = Proc.initLastAttempt;
        }
        public abstract void Init(List<Event> events, User user, Fight fight);
        public abstract void Call(Event evt, User user);

    }

    internal static class SpecialEffectFactory
    {
        private static readonly List<Type> _effectTypes;

        static SpecialEffectFactory()
        {
            _effectTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(SpecialEffect)) && !t.IsAbstract)
                .ToList();
        }

        public static SpecialEffect? CreateFromName(string gearName)
        {
            foreach (var type in _effectTypes)
            {
                var sourcesField = type.GetField("Sources", BindingFlags.Public | BindingFlags.Static);
                if (sourcesField?.GetValue(null) is HashSet<string> sources && sources.Contains(gearName))
                {

                    var instance = (SpecialEffect?)Activator.CreateInstance(type);
                    return instance;
                }
            }
            return null;
        }
    }
}
