using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using RamjetAnvil.Unity.Utility;
using Guid = System.Guid;

namespace RamjetAnvil.Volo.CourseEditing
{
    public struct CourseData {
        public string Id { get; set; }
        public int FormatVersion { get; set; }
        public string Name { get; set; }
        public IList<Prop> Props { get; set; }

        public static CourseData CreateNew() {
            return new CourseData { Id = Guid.NewGuid().ToString(), Name = "", Props = ImmutableList<Prop>.Empty };
        }

        public override string ToString() {
            return string.Format("CourseId: {0}, CourseName: {1}, Props: {2}", Id, Name, Props);
        }
    }

    public struct Prop {
        public PropType PropType { get; set; }
        public ImmutableTransform Transform { get; set; }
    }
}
