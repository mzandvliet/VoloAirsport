using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Unity.Utility
{
    public static class FunctionComposition
    {
        public static Action Comp(params Action[] actions)
        {
            return () =>
            {
                for (int i = actions.Length - 1; i >= 0; i--)
                {
                    actions[i]();
                }
            };
        }
    }

    public static class PartialFunctionApplication
    {
        // Action

        // Arity: 1

        public static Action Partial<T1>(this Action<T1> subject, T1 var1)
        {
            return () => subject(var1);
        }

        // Arity: 2

        public static Action<T2> Partial<T1, T2>(this Action<T1, T2> subject, T1 var1)
        {
            return var2 => subject(var1, var2);
        }

        public static Action Partial<T1, T2>(this Action<T1, T2> subject, T1 var1, T2 var2)
        {
            return () => subject(var1, var2);
        }

        // Arity: 3

        public static Action<T2, T3> Partial<T1, T2, T3>(this Action<T1, T2, T3> subject, T1 var1)
        {
            return (var2, var3) => subject(var1, var2, var3);
        }

        public static Action<T3> Partial<T1, T2, T3>(this Action<T1, T2, T3> subject, T1 var1, T2 var2)
        {
            return var3 => subject(var1, var2, var3);
        }
        public static Action Partial<T1, T2, T3>(this Action<T1, T2, T3> subject, T1 var1, T2 var2, T3 var3)
        {
            return () => subject(var1, var2, var3);
        }

        // Arity: 4

        public static Action<T2, T3, T4> Partial<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> subject, T1 var1)
        {
            return (var2, var3, var4) => subject(var1, var2, var3, var4);
        }

        public static Action<T3, T4> Partial<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> subject, T1 var1, T2 var2)
        {
            return (var3, var4) => subject(var1, var2, var3, var4);
        }
        public static Action<T4> Partial<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> subject, T1 var1, T2 var2, T3 var3)
        {
            return var4 => subject(var1, var2, var3, var4);
        }
        public static Action Partial<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> subject, T1 var1, T2 var2, T3 var3, T4 var4)
        {
            return () => subject(var1, var2, var3, var4);
        }

        
        // Func

        // Arity: 1

        public static Func<TResult> Partial<T1, TResult>(this Func<T1, TResult> subject, T1 var1)
        {
            return () => subject(var1);
        }


        // Arity: 2

        public static Func<T2, TResult> Partial<T1, T2, TResult>(this Func<T1, T2, TResult> subject, T1 var1)
        {
            return var2 => subject(var1, var2);
        }

        public static Func<TResult> Partial<T1, T2, TResult>(this Func<T1, T2, TResult> subject, T1 var1, T2 var2)
        {
            return () => subject(var1, var2);
        }


        // Arity: 3

        public static Func<T2, T3, TResult> Partial<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> subject, T1 var1)
        {
            return (var2, var3) => subject(var1, var2, var3);
        }

        public static Func<T3, TResult> Partial<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> subject, T1 var1, T2 var2)
        {
            return var3 => subject(var1, var2, var3);
        }

        public static Func<TResult> Partial<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> subject, T1 var1, T2 var2, T3 var3)
        {
            return () => subject(var1, var2, var3);
        }


        // Arity: 4

        public static Func<T2, T3, T4, TResult> Partial<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> subject, T1 var1)
        {
            return (var2, var3, var4) => subject(var1, var2, var3, var4);
        }

        public static Func<T3, T4, TResult> Partial<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> subject, T1 var1, T2 var2)
        {
            return (var3, var4) => subject(var1, var2, var3, var4);
        }

        public static Func<T4, TResult> Partial<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> subject, T1 var1, T2 var2, T3 var3)
        {
            return var4 => subject(var1, var2, var3, var4);
        }

        public static Func<TResult> Partial<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> subject, T1 var1, T2 var2, T3 var3, T4 var4)
        {
            return () => subject(var1, var2, var3, var4);
        }
    }
}
