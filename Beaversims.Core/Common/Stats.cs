using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Beaversims.Core
{
    public enum HasteScalerType { Cast, Auto, Tick }
    public enum StatName { Intellect, Stamina, Vers, Crit, Mastery, Haste, Leech, Avoidance }
    public enum StatAmountType { Rating, Base, Multi, CritIncHeal, CritIncDmg}
    internal abstract class Stat
    {
        public double Rating { get; set; } = 0.0;
        public double Base { get; set; } = 0.0;
        public double Multi { get; set; } = 1.0;
        protected double Eff { get; set; } = 0.0;
        public double SimExtraEff { get; set; } = 0.0;
        public double TempExtraEff { get; set; } = 0.0;

        public abstract StatName Name { get; }
        public abstract int Level { get; }
        public abstract double GetEffDiff(double addRating, bool removal);
        public abstract void SetEff();
        public abstract void FullUpdate();
        public double SimTempEff()
        {
            return Eff + TempExtraEff;
        }
        public double TrueEff()
        {
            return Eff + SimExtraEff;
        }
        public double ApplyMult(double amount)
        {
            return amount * Multi;
        }
        public double RemoveMult(double amount)
        {
            return amount / Multi;
        }

        public virtual void ChangeRating(double amount, bool removal)
        {
            if (removal)
            {
                amount *= -1.0;
            }
            Rating = ((Rating / Multi) + amount) * Multi;
            FullUpdate();
        }

        public void ChangeMulti(double amount, bool removal)
        {
            amount += 1;
            var nonMultiRating = Rating / Multi;
            if (removal) {Multi /= amount;}
            else {Multi *= amount;}
            Rating = nonMultiRating * Multi;
            FullUpdate();
        }

        public void ChangeBase(double amount, bool removal)
        {
            if (removal) {amount *= -1.0;}
            Base += amount;
            SetEff();
        }


        public virtual void ChangeAmount(double amount, StatAmountType amountType, bool removal)
        {
            // Override in Crit
            if (amountType == StatAmountType.Rating)
            {
                ChangeRating(amount, removal);
            }
            else if (amountType == StatAmountType.Multi)
            {
                ChangeMulti(amount, removal);
            }
            else if (amountType == StatAmountType.Base)
            {
                ChangeBase(amount, removal);
            }
        }


        public abstract Stat Clone();
        protected void CopyTo(Stat other)
        {
            other.Rating = Rating;
            other.Base = Base;
            other.Multi = Multi;
            other.Eff = Eff;
        }
    }

    internal abstract class PrimaryStat : Stat
    {
        public override double GetEffDiff(double addRating, bool removal)
        {
            if (removal)
            {
                addRating *= -1.0;
            }
            var newRating = ((Rating / Multi) + addRating) * Multi;
            var newEff = newRating + Base;
            return newEff - Eff;
        }
        public override void SetEff()
        {
            Eff = Rating + Base;
        }

        public override void FullUpdate()
        {
            SetEff();
        }

    }

    internal abstract class NonPrimaryStat : Stat
        //todo need separate tertiary/secondary. DR is different.
    {
        public double PostDr { get; set; } = 0.0;
        public int Bracket { get; set; } = 0;
        public abstract double PercentRate { get; set; }
        public abstract double DrRate { get; }

        public double ApplyDryMult(double amount)
        {
            return amount * (1 - (0.1 * Bracket)) * Multi;
        }
        public double RemoveDryMult(double amount)
        {
            return amount / (1 - (0.1 * Bracket)) / Multi;
        }

        public override void ChangeRating(double amount, bool removal)
        {
            if (removal)
            {
                amount *= -1.0;
            }
            Rating = ((Rating / Multi) + amount) * Multi;
            FullUpdate();
        }
        public override double GetEffDiff(double addRating, bool removal)
        {
            if (removal)
            {
                addRating *= -1.0;
            }

            var newRating = ((Rating / Multi) + addRating) * Multi;


            var newBracket = Calc.GetBracket(newRating, DrRate);
            var newPostDr = Calc.CalcPostDr(newBracket, newRating, DrRate);

            var newEff = newPostDr + Base;
            return newEff - Eff;
        }

        public override void SetEff()
        {
            Eff = PostDr + Base;
        }

        //public double GetPostDr(double rating, double drRate)
        //{
        //    var bracket = Calc.GetBracket(rating, drRate);
        //    var postDr = Calc.CalcPostDr(bracket, rating, drRate);
        //    return postDr;
        //}

        public override void FullUpdate()
        {
            Bracket = Calc.GetBracket(Rating, DrRate);
            PostDr = Calc.CalcPostDr(Bracket, Rating, DrRate);
            SetEff();
        }
      

        protected void CopyTo(NonPrimaryStat other)
        {
            base.CopyTo(other);
            other.PostDr = PostDr;
            other.Bracket = Bracket;
            other.PercentRate = PercentRate;
        }
    }

    internal abstract class SecondaryStat : NonPrimaryStat
    {


      

    }
    internal abstract class TertiaryStat : NonPrimaryStat
    {

    }

    internal class Intellect : PrimaryStat
    {
        public override StatName Name { get; }
        public override int Level { get; }

        public Intellect()
        {
            Name = StatName.Intellect;
            Level = 1;
        }

        public override Stat Clone()
        {
            var clone = new Intellect();
            CopyTo(clone);
            return clone;
        }
    }

    internal class Stamina : PrimaryStat
    {
        public override StatName Name { get; }
        public override int Level { get; }

        public Stamina()
        {
            Name = StatName.Stamina;
            Level = 1;
        }

        public override Stat Clone()
        {
            var clone = new Stamina();
            CopyTo(clone);
            return clone;
        }
    }

    internal class Crit : SecondaryStat
    {
        public override StatName Name { get; }
        public override int Level { get; }
        public const double percentRate = 700;
        public override double PercentRate { get; set; }
        public override double DrRate { get; }
        public double IncHeal { get; set; } = 2;
        public double IncDmg { get; set; } = 2;

        public override void ChangeAmount(double amount, StatAmountType amountType, bool removal)
        {
            if (amountType == StatAmountType.Rating)
            {
                ChangeRating(amount, removal);
            }
            else if (amountType == StatAmountType.Multi)
            {
                ChangeMulti(amount, removal);
            }
            else if (amountType == StatAmountType.Base)
            {
                ChangeBase(amount, removal);
            }
            else if (amountType == StatAmountType.CritIncHeal)
            {
                if (removal) 
                {
                    amount *= -1;
                }
                IncHeal += amount;
            }
            else if (amountType == StatAmountType.CritIncDmg)
            {
                if (removal)
                {
                    amount *= -1;
                }
                IncDmg += amount;
            }
        }

        public Crit()
        {
            Name = StatName.Crit;
            Level = 2;
            PercentRate = percentRate;
            DrRate = PercentRate;
        }

        public override Stat Clone()
        {
            var clone = new Crit();
            CopyTo(clone);
            return clone;
        }

        protected void CopyTo(Crit other)
        {
            base.CopyTo(other);
            other.IncDmg = IncDmg;
            other.IncHeal = IncHeal;
        }

    }

    internal class Haste : SecondaryStat
    {
        public override StatName Name { get; }
        public override int Level { get; }
        public const double percentRate = 660;

        public override double PercentRate { get; set; }
        public override double DrRate { get; }

        public override double GetEffDiff(double addRating, bool removal)
        {
            if (removal)
            {
                addRating *= -1.0;
            }

            var newRating = ((Rating / Multi) + addRating) * Multi;


            var newBracket = Calc.GetBracket(newRating, DrRate);
            var newPostDr = Calc.CalcPostDr(newBracket, newRating, DrRate);
            var trueBase = Base * (1 + (newPostDr / percentRate / 100));
            var newEff = newPostDr + trueBase;
            return newEff - Eff;
        }

        public override void SetEff()
        {
            //Retarded haste scaling
            var trueBase = Base * (1 + (PostDr / percentRate / 100));
            Eff = trueBase + PostDr;
        }

        public Haste()
        {
            Name = StatName.Haste;
            Level = 2;
            PercentRate = percentRate;
            DrRate = PercentRate;
        }

        public override Stat Clone()
        {
            var clone = new Haste();
            CopyTo(clone);
            return clone;
        }
    }

    internal class Mastery : SecondaryStat
    {
        public override StatName Name { get; }
        public override int Level { get; }

        public override double PercentRate { get; set; }
        public override double DrRate { get; }
        public const double tooltipPercentRate = 700;
        public double TooltipPercentRate { get; }

        public Mastery(double percentRate)
        {
            Name = StatName.Mastery;
            Level = 2;
            PercentRate = percentRate;
            TooltipPercentRate = tooltipPercentRate;
            DrRate = TooltipPercentRate;
        }

        public override Stat Clone()
        {
            // PercentRate should never be null for Mastery; fall back to 0 if it somehow is.
            var clone = new Mastery(PercentRate);
            CopyTo(clone);
            return clone;
        }
    }

    internal class Vers : SecondaryStat
    {
        public override StatName Name { get; }
        public override int Level { get; }
        public const double percentRate = 780;
        public override double PercentRate { get; set; }
        public override double DrRate { get; }
        public double DefPercentRate { get; }

        public Vers()
        {
            Name = StatName.Vers;
            Level = 2;
            PercentRate = percentRate;
            DrRate = PercentRate;
            DefPercentRate = PercentRate * 2;
        }

        public override Stat Clone()
        {
            var clone = new Vers();
            CopyTo(clone);
            return clone;
        }
    }

    internal class Leech : TertiaryStat
    {
        public override StatName Name { get; }
        public override int Level { get; }
        public const double percentRate = 1019.995125;

        public override double PercentRate { get; set; }
        public override double DrRate { get; }

        public Leech()
        {
            Name = StatName.Leech;
            Level = 3;
            PercentRate = percentRate;
            DrRate = PercentRate;
        }

        public override Stat Clone()
        {
            var clone = new Leech();
            CopyTo(clone);
            return clone;
        }
    }

    internal class Avoidance : TertiaryStat
    {
        public override StatName Name { get; }
        public override int Level { get; }
        public const double percentRate = 543.9974;

        public override double PercentRate { get; set; }
        public override double DrRate { get; }

        public Avoidance()
        {
            Name = StatName.Avoidance;
            Level = 3;
            PercentRate = percentRate;
            DrRate = PercentRate;
        }

        public override Stat Clone()
        {
            var clone = new Avoidance();
            CopyTo(clone);
            return clone;
        }
    }
}
