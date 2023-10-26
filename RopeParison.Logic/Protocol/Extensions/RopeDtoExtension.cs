using RopeParison.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeParison.Logic.Protocol.Extensions
{
    public static class RopeDtoExtension
    {
        public static bool Has_ImpactForce55kgOneStrand(this RopeDto rope)
        {
            if (rope == null || rope.Category == null)
                return false;

            var test = new[] { 
                Category.HalfTwin,
                Category.Half,
                Category.Triple
                }.Any(x => x == rope.Category.Category);
            return test;
        }

        public static bool Has_ImpactForce80kgOneStrand(this RopeDto rope)
        {
            if (rope == null || rope.Category == null)
                return false;

            var test = new[] { 
                Category.Single,
                Category.Triple
                }.Any(x => x == rope.Category.Category);
            return test;
        }

        public static bool Has_ImpactForce80kgTwoStrand(this RopeDto rope)
        {
            if (rope == null || rope.Category == null)
                return false;

            var test = new[] {
                Category.HalfTwin,
                Category.Twin,
                Category.Triple 
                }.Any(x => x == rope.Category.Category);
            return test;
        }

        public static bool Has_StaticElongation80kgOneStrand(this RopeDto rope)
        {
            if (rope == null || rope.Category == null)
                return false;

            var test = new[] {
                Category.Single,
                Category.HalfTwin,
                Category.Half,
                Category.Triple
                }.Any(x => x == rope.Category.Category);
            return test;
        }

        public static bool Has_StaticElongation80kgTwoStrand(this RopeDto rope)
        {
            if (rope == null || rope.Category == null)
                return false;

            var test = new[] {
                Category.HalfTwin,
                Category.Twin,
                Category.Triple
                }.Any(x => x == rope.Category.Category);
            return test;
        }

        public static bool Has_DynamicElongation55kgOneStrand(this RopeDto rope)
        {
            if (rope == null || rope.Category == null)
                return false;

            var test = new[] {
                Category.HalfTwin,
                Category.Half,
                Category.Triple
                }.Any(x => x == rope.Category.Category);
            return test;
        }

        public static bool Has_DynamicElongation80kgOneStrand(this RopeDto rope)
        {
            if (rope == null || rope.Category == null)
                return false;

            var test = new[] {
                Category.Single,
                Category.Triple
                }.Any(x => x == rope.Category.Category);
            return test;
        }


        public static bool Has_DynamicElongation80kgTwoStrand(this RopeDto rope)
        {
            if (rope == null || rope.Category == null)
                return false;

            var test = new[] {
                Category.HalfTwin,
                Category.Twin,
                Category.Triple
                }.Any(x => x == rope.Category.Category);
            return test;
        }
    }
}
