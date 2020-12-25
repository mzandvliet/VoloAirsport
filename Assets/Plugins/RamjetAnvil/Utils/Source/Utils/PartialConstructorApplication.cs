using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RamjetAnvil.Unity.Utility
{
    public class PartialConstructor<T>
    {
        private readonly ConstructorInfo _ctor;
        private readonly IDictionary<string, object> _parameters;
        private readonly IList<string> _requiredParams;

        public PartialConstructor(ConstructorInfo ctor)
        {
            _ctor = ctor;
            _parameters = new Dictionary<string, object>();
            var ctorParams = ctor.GetParameters();
            _requiredParams = ctorParams.Select(p => p.Name).ToListOptimized();
        }

        private PartialConstructor(ConstructorInfo ctor, IList<string> requiredParams, IDictionary<string, object> parameters)
        {
            _ctor = ctor;
            _requiredParams = requiredParams;
            _parameters = parameters;
        }

        public PartialConstructor<T> Apply(string name, object value)
        {
            Preconditions.CheckArgument(_requiredParams.Contains(name), "Parameter with name " + name + 
                                                                        " does not exist in constructor for type " + typeof(T));

            var parameters = new Dictionary<string, object>(_parameters);
            parameters[name] = value;
            return new PartialConstructor<T>(_ctor, _requiredParams, parameters);
        }

        public T Construct()
        {
            Preconditions.CheckArgument(_requiredParams.All(p => _parameters.ContainsKey(p)), 
                "Not all params of this constructor were applied, cannot construct type " + typeof(T));

            var filledParams = _requiredParams.Select(p => _parameters[p]).ToArray();
            return (T)_ctor.Invoke(filledParams);
        }
    }

    public static class PartialConstructor
    {
        public static PartialConstructor<T> GetSingleConstructor<T>()
        {
            var subject = typeof(T);
            var ctors = subject.GetConstructors().Where(c => c.IsPublic);
            if (ctors.Count() > 1)
            {
                throw new Exception("More than one public constructor found, cannot determine which one to use.");
            }
            else if (!ctors.Any())
            {
                throw new Exception("No public constructor found.");
            }
            return new PartialConstructor<T>(ctors.First());
        }
    }
}